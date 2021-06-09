using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExcelDataReader;
using Vescon.So.WebApi.Client;
using Vescon.So.WebApi.Client.Dtos;

// ReSharper disable StringLiteralTypo

namespace Sample
{
    internal class ExcelImporter
    {
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
                var identifier = macroReference
                    .Where(x => !string.IsNullOrEmpty(x.FullIdentifyingValue))
                    .OrderBy(x => x.FullIdentifyingValue)
                    .FirstOrDefault()?.FullIdentifyingValue ?? string.Empty;

                var top = macroReference
                    .Select(x => x.Y)
                    .Max();
                var left = macroReference
                    .Select(x => x.X)
                    .Min();

                var macroPath = macroReference.First().MacroPath;

                Console.WriteLine($"Creating macro: {macroPath} ...");
                var createdPlacements = await _connector.CreatePlacement(
                    layoutPageGuid,
                    macroPath,
                    (int) top,
                    (int) left,
                    identification: identifier);
                
                Console.WriteLine("Result:");
                DumpPlacements(createdPlacements);
            }

            // import to layout page
            var otherPlacements = groupedPlacements[false].ToList();
            var importablePlacements = otherPlacements
                .Where(x => x.PlacementType == "SymbolReference")
                .Where(x => !x.IsSubsymbol)
                .Where(x => !x.IsConnectionSymbol)
                .ToList();
            foreach (var placement in importablePlacements)
            {
                var rotationRad = placement.RotationZ / 360 * 2 * Math.PI;

                Console.WriteLine($"Creating symbol: {placement} ...");
                var createdPlacements = await _connector.CreatePlacement(
                    layoutPageGuid,
                    placement.SymbolPath,
                    (int)placement.X,
                    (int)placement.Y,
                    (float)rotationRad,
                    placement.FullIdentifyingValue);

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

            return exportSheet.Rows
                .Cast<DataRow>()
                .Select(row => new ImportPlacement
                {
                    PlacementGuid = Map(row, "Placement guid", columnMapping, x => Guid.Parse(Convert.ToString(x)!)),
                    PlacementType = Map(row, "Placement type", columnMapping, Convert.ToString),
                    FullIdentifyingValue = Map(row, "Full identifying value", columnMapping, Convert.ToString),
                    IsSubsymbol = Map(row, "Is Subsymbol", columnMapping, Convert.ToBoolean),
                    IsConnectionSymbol = Map(row, "Is connection symbol", columnMapping, Convert.ToBoolean),
                    X = Map(row, "X", columnMapping, Convert.ToSingle),
                    Y = Map(row, "Y", columnMapping, Convert.ToSingle),
                    Z = Map(row, "Z", columnMapping, Convert.ToSingle),
                    RotationZ = Map(row, "Rotation Z", columnMapping, Convert.ToSingle),
                    RegionName = Map(row, "Region name", columnMapping, Convert.ToString),
                    SymbolPath = Map(row, "Symbol path", columnMapping, Convert.ToString),
                    MacroPath = Map(row, "Macro path", columnMapping, Convert.ToString),
                    MacroReferenceGuid = Map(row, "Macro reference guid", columnMapping, x =>
                    {
                        var value = Convert.ToString(x);
                        return string.IsNullOrEmpty(value)
                            ? (Guid?) null
                            : Guid.Parse(value);
                    })
                })
                .ToList();
        }

        private static T Map<T>(
            DataRow row,
            string column,
            IReadOnlyDictionary<string, int> columnMapping,
            Func<object, T> converter)
        {
            var index = columnMapping[column];
            var value = row.ItemArray[index];
            var converted = converter(value);
            return converted;
        }
    }

    internal class ImportPlacement
    {
        public Guid PlacementGuid { get; init; }
        public string PlacementType { get; init; }
        public string FullIdentifyingValue { get; init; }
        public float X { get; init; }
        public float Y { get; init; }
        public float Z { get; init; }
        public float RotationZ { get; init; }
        public string SymbolPath { get; init; }
        public string RegionName { get; init; }
        public string MacroPath { get; init; }
        public Guid? MacroReferenceGuid { get; init; }
        public bool IsSubsymbol { get; init; }
        public bool IsConnectionSymbol { get; init; }

        public override string ToString()
        {
            return $"{PlacementGuid} - {FullIdentifyingValue} - {PlacementType} - {MacroPath} - {SymbolPath}";
        }
    }
}