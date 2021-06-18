using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ExcelDataReader;
using Vescon.So.WebApi.Client;
using Vescon.So.WebApi.Client.Dtos;

// ReSharper disable StringLiteralTypo

namespace Sample
{
    /// <summary>
    /// Imports placements from Excel file exported via element list.
    /// Export with Settings->Expert mode so column header won't have customized column headers
    /// Required columns: PlacementGuid,Placement type,Full identifying value,X,Y,Z,Rotation Z,Symbol path,Macro reference guid,Macro path,IsSubSymbol,IsConnectionSymbol, "all attribute value columns", "all attribute value columns Is overwritten", "all attribute value columns Description"
    /// InsertionPoints for Macros should be adapted manually in excel file and be the same for all macro placements
    /// SubSymbols and ConnectionSymbols won't be imported
    /// </summary>
    internal class ExcelImporter
    {
        // e.g.
        // [=] (P) FG-Counter Is overwritten
        private static readonly Regex KeyColumnRegex = new(
            @"(?<fullname>\[(?<prefix>.+)\] (?<isKeyPart>\(P\) )?(?<keyName>.+))",
                RegexOptions.Singleline | RegexOptions.Compiled);

        private static readonly Regex KeyColumnRegexIsOverwritten = new(
            @"(?<fullname>\[(?<prefix>.+)\] (?<isKeyPart>\(P\) )?(?<keyName>.+)) Is overwritten",
                RegexOptions.Singleline | RegexOptions.Compiled);

        private static readonly Regex KeyColumnRegexDescription = new(
            @"(?<fullname>\[(?<prefix>.+)\] (?<isKeyPart>\(P\) )?(?<keyName>.+)) Description",
            RegexOptions.Singleline | RegexOptions.Compiled);

        private readonly So3ApiConnector _connector;
        
        public ExcelImporter(So3ApiConnector connector)
        {
            _connector = connector;
        }

        public async Task ImportFromFile(string filepath, Guid layoutPageGuid)
        {
            var placements = await LoadPlacementsFromExcel(filepath);

            var groupedPlacements = placements.ToLookup(x => x.MacroReferenceGuid != null);
            var macroReferences = groupedPlacements[true].ToLookup(x => x.MacroReferenceGuid.Value);
            foreach (var macroReference in macroReferences)
            {
                var identifier = macroReference.First(x => !string.IsNullOrEmpty(x.FullIdentifyingValue)).FullIdentifyingValue ?? string.Empty;

                var firstElement = macroReference.First();
                var top = macroReference
                    .Select(x => x.Y)
                    .Max();
                var left = macroReference
                    .Select(x => x.X)
                    .Min();
                var macroPath = firstElement.MacroPath;

                Console.WriteLine($"Creating macro: {macroPath} ...");
                var createdPlacements = await _connector.CreatePlacement(
                    layoutPageGuid,
                    macroPath,
                    (int) left,
                    (int) top,
                    identification: identifier);
                
                Console.WriteLine("Result:");
                DumpPlacements(createdPlacements);
            }

            // import to layout page
            var otherPlacements = groupedPlacements[false].ToList();
            var importablePlacements = otherPlacements
                .Where(x => x.PlacementType == "SymbolReference")
                .Where(x => !x.IsSubSymbol)
                .Where(x => !x.IsConnectionSymbol)
                .ToList();
            foreach (var placement in importablePlacements)
            {
                var rotationRad = placement.RotationZ / 360 * 2 * Math.PI;

                var attributeImport = placement.AttributeImport;
                var fixedAttributeValueParts = attributeImport.ValueParts
                    .SelectMany(x =>
                    {
                        if (string.IsNullOrEmpty(x.Value) || string.IsNullOrEmpty(x.Description))
                            return new List<AttributeValuePart> {x};

                        // update value and description has to be 2 commands
                        var updateDescription = new AttributeValuePart(x.Name, x.Language!, x.Description, true);
                        x.Language = null;
                        x.Description = null;
                        return new List<AttributeValuePart> {x, updateDescription};

                    })
                    .ToList();

                var attributeUpdates = new AttributeUpdates(
                    new PlacementsSelector(placement.PlacementGuid),
                    fixedAttributeValueParts,
                    attributeImport.OverwrittenValues);

                Console.WriteLine($"Creating symbol: {placement} ...");
                var createdPlacements = await _connector.CreatePlacement(
                    layoutPageGuid,
                    placement.SymbolPath,
                    (int) placement.X,
                    (int) placement.Y,
                    (float) rotationRad,
                    attributeUpdates: new List<AttributeUpdates> {attributeUpdates});

                Console.WriteLine("Result:");
                DumpPlacements(createdPlacements);
            }
        }

        private static void DumpPlacements(List<PlacementHeader> createdPlacements)
        {
            foreach (var createdPlacement in createdPlacements.OrderBy(x => x.Identification ?? string.Empty))
                Console.WriteLine($@"{createdPlacement.Guid} - {createdPlacement.Identification}");
        }

        private static async Task<List<ImportPlacement>> LoadPlacementsFromExcel(string filepath)
        {
            if (!File.Exists(filepath))
                throw new FileNotFoundException("Excel import file not found", filepath);

            // workaround for .NET Core 3.1
            // https://stackoverflow.com/questions/50858209/system-notsupportedexception-no-data-is-available-for-encoding-1252
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            await using var stream = File.Open(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite); // use FileShare.ReadWrite for open excel files
            using var reader = ExcelReaderFactory.CreateReader(stream, new ExcelReaderConfiguration()
            {
                FallbackEncoding = Encoding.UTF8
            });

            // Use the AsDataSet extension method
            var configuration = new ExcelDataSetConfiguration
                {ConfigureDataTable = _ => new ExcelDataTableConfiguration {UseHeaderRow = true}};
            var result = reader.AsDataSet(configuration);

            var exportSheet = result.Tables[0];
            var columnMapping = exportSheet.Columns
                .Cast<DataColumn>().ToDictionary(x => x.ColumnName, x => x.Ordinal);

            var allKeys = ExtractAttributeKeys(columnMapping);

            return exportSheet.Rows
                .Cast<DataRow>()
                .Select(row => new ImportPlacement
                {
                    PlacementGuid = Map(row, "PlacementGuid", columnMapping, x => Guid.Parse(Convert.ToString(x)!)),
                    PlacementType = Map(row, "Placement type", columnMapping, Convert.ToString),
                    FullIdentifyingValue = Map(row, "Full identifying value", columnMapping, Convert.ToString),
                    IsSubSymbol = Map(row, "IsSubSymbol", columnMapping, Convert.ToBoolean),
                    IsConnectionSymbol = Map(row, "IsConnectionSymbol", columnMapping, Convert.ToBoolean),
                    X = Map(row, "X", columnMapping, Convert.ToSingle),
                    Y = Map(row, "Y", columnMapping, Convert.ToSingle),
                    Z = Map(row, "Z", columnMapping, Convert.ToSingle),
                    RotationZ = Map(row, "Rotation Z", columnMapping, Convert.ToSingle),
                    SymbolPath = Map(row, "Symbol path", columnMapping, Convert.ToString),
                    MacroPath = Map(row, "Macro path", columnMapping, Convert.ToString),
                    MacroReferenceGuid = Map(row, "Macro reference guid", columnMapping, x =>
                    {
                        var value = Convert.ToString(x);
                        return string.IsNullOrEmpty(value)
                            ? (Guid?) null
                            : Guid.Parse(value);
                    }),

                    AttributeImport = ParseValueColumns(row, allKeys)
                })
                .ToList();
        }

        private static List<AttributeKey> ExtractAttributeKeys(Dictionary<string, int> columnMapping)
        {
            var regexes = new List<Tuple<Regex, Action<AttributeKey>>>
            {
                Tuple.Create<Regex, Action<AttributeKey>>(KeyColumnRegexDescription, x => x.IsDescription = true),
                Tuple.Create<Regex, Action<AttributeKey>>(KeyColumnRegexIsOverwritten, x => x.IsOverwritten = true),
                Tuple.Create<Regex, Action<AttributeKey>>(KeyColumnRegex, _ => { })
            };

            var allKeys = columnMapping
                .Select(x => regexes
                    .Select(regex => new
                    {
                        Entry = x,
                        Regex = regex,
                        Match = regex.Item1.Match(x.Key)
                    })
                    .FirstOrDefault(y => y.Match.Success))
                .Where(x => x != null)
                .Select(x =>
                {
                    var key = new AttributeKey
                    {
                        ColumnName = x.Entry.Key,
                        Prefix = x.Match.Groups["prefix"].Value,
                        Identifier = x.Match.Groups["identifier"].Value,
                        KeyName = x.Match.Groups["keyName"].Value,
                        Index = x.Entry.Value,
                        IsKeyPart = !string.IsNullOrEmpty(x.Match.Groups["isKeyPart"].Value)
                    };
                    x.Regex.Item2(key); // update key for used regex -> IsOverwritten / IsDescription
                    return key;
                })
                .ToList();

            var fullKeys = allKeys
                .Where(x => !x.IsKeyPart)
                .GroupBy(x => x.Prefix)
                .Select(x => x.OrderBy(c => c.IsKeyPart).ThenBy(c => c.IsOverwritten).First())
                .ToDictionary(x => x.Prefix, x => x);
            foreach (var key in allKeys.Where(x => x.IsKeyPart && !x.IsOverwritten))
            {
                if (fullKeys.TryGetValue(key.Prefix, out var fullKey))
                {
                    if (fullKey == key)
                        continue;

                    fullKey.AddChild(key);
                }
            }

            return allKeys;
        }

        private static AttributeImport ParseValueColumns(DataRow row, List<AttributeKey> keys)
        {
            var import = new AttributeImport();

            var valueParts = new Dictionary<string, AttributeValuePart>();
            foreach (var key in keys)
            {
                var isOverwrittenColumn = key.IsOverwritten;

                var value = Map(row, key.Index, Convert.ToString) ?? string.Empty;
                bool isOverwritten;
                if (isOverwrittenColumn)
                {
                    isOverwritten = bool.Parse(value);
                    value = null;
                }
                else
                    isOverwritten = false;

                var keyName = key.KeyName;
                if (!valueParts.TryGetValue(keyName, out var valuePart))
                    valuePart = valueParts[keyName] = new AttributeValuePart(keyName, string.Empty);

                if (isOverwrittenColumn)
                {
                    if (isOverwritten)
                    {
                        if (key.IsKeyPart)
                            valuePart.IsOverwritten = true;
                        else
                            import.OverwrittenValues[keyName] = true;
                    }
                }
                else if (key.IsDescription)
                {
                    valuePart.Language = "en-US";
                    valuePart.Description = value;
                }
                else
                    valuePart.Value = value;
            }

            var keysWithChildren = keys
                .Where(x => x.Children.Any())
                .Select(x=>x.KeyName)
                .ToHashSet();

            var toImport = valueParts.Values
                .Where(x => !keysWithChildren.Contains(x.Name))
                .Where(x => x.IsOverwritten || !string.IsNullOrEmpty(x.Value) || !string.IsNullOrEmpty(x.Description))
                .ToList();

            import.ValueParts.AddRange(toImport);
            return import;
        }

        private static T Map<T>(
            DataRow row,
            string column,
            IReadOnlyDictionary<string, int> columnMapping,
            Func<object, T> converter)
        {
            var index = columnMapping[column];
            return Map(row, index, converter);
        }

        private static T Map<T>(
            DataRow row,
            int index,
            Func<object, T> converter)
        {
            var value = row.ItemArray[index];
            var converted = converter(value);
            return converted;
        }

        private class AttributeImport
        {
            public List<AttributeValuePart> ValueParts { get; } = new();
            public Dictionary<string, bool> OverwrittenValues { get; set; } = new();
        }

        private class ImportPlacement
        {
            public Guid PlacementGuid { get; init; }
            public string PlacementType { get; init; }
            public string FullIdentifyingValue { get; init; }
            public float X { get; init; }
            public float Y { get; init; }
            public float Z { get; init; }
            public float RotationZ { get; init; }
            public string SymbolPath { get; init; }
            public string MacroPath { get; init; }
            public Guid? MacroReferenceGuid { get; init; }
            public bool IsSubSymbol { get; init; }
            public bool IsConnectionSymbol { get; init; }
            public AttributeImport AttributeImport { get; init; }

            public override string ToString()
            {
                return $"{PlacementGuid} - {FullIdentifyingValue} - {PlacementType} - {MacroPath} - {SymbolPath}";
            }
        }
    }

    internal class AttributeKey
    {
        public AttributeKey Parent { get; set; }

        public string Prefix { get; init; }
        public string ColumnName { get; init; }
        public int Index { get; init; }
        public string Identifier { get; init; }
        public string KeyName { get; init; }
        public bool IsKeyPart { get; init; }
        public bool IsOverwritten { get; set; }
        public bool IsDescription { get; set; }
        public List<AttributeKey> Children { get; } = new();

        public override string ToString()
        {
            return
                $"{nameof(KeyName)}: {KeyName}, {nameof(IsKeyPart)}: {IsKeyPart}, {nameof(IsOverwritten)}: {IsOverwritten}, {nameof(IsDescription)}: {IsDescription}, {nameof(ColumnName)}: {ColumnName}, {nameof(Identifier)}: {Identifier}, {nameof(Index)}: {Index}, {nameof(Parent)}: {Parent?.KeyName}, {nameof(Children)}: {Children.Count}";
        }

        public void AddChild(AttributeKey key)
        {
            key.Parent = this;
            Children.Add(key);
        }
    }
}