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
    /// <summary>
    /// Imports placements from Excel file exported via element list.
    /// SymbolPfad: SO3/Master Data/Symbol Libraries/Symbole/Alfing/Api01
    /// LayoutPfad: SO3/Projects/Alfing/Alfing/F1/Import
    /// Positionsnummer -> Funktionsgruppe(=)
    /// BMK -> BMK1(-)
    /// Funktionstext 1 -> SubBMK description in Konnektor 1 (:1)
    /// Funktionstext 2 -> SubBMK description in Konnektor 2 (:2)
    /// Artikelnummer -> ArtNr von MainSymbol(-X)
    /// Hersteller -> Hersteller von MainSymbol(-X)
    /// Alfing Nummer -> Alfing Nummer von MainSymbol(-X)
    /// </summary>
    internal class SimpleExcelImporter
    {
        private readonly Dictionary<string, string> _keyMapping =
            new()
            {
                { nameof(ImportPlacement.Positionsnummer), "Station" }, // Funktionsgruppe - Part1
                { nameof(ImportPlacement.BMK), "BMK1" },
                { nameof(ImportPlacement.Funktionstext1), "SubBMK" },
                { nameof(ImportPlacement.Funktionstext2), "SubBMK" },
                { nameof(ImportPlacement.Artikelnummer), "ArtNr" },
                { nameof(ImportPlacement.Hersteller), "Hersteller" },
                { nameof(ImportPlacement.AlfingNummer), "Alfing Nummer" }
            };

        private const string SymbolPath = "SO3/Master Data/Symbol Libraries/Symbole/Alfing/Api01";
        private const string Language = "de-DE";

        private const int yOffset = -12;
        private const int xOffset = 20;

        private readonly So3ApiConnector _connector;

        public SimpleExcelImporter(So3ApiConnector connector)
        {
            _connector = connector;
        }
        
        public async Task ImportFromFile(string filepath, Guid layoutPageGuid)
        {
            var placements = await LoadPlacementsFromExcel(filepath);

            var grouped = placements
                .GroupBy(x => x.Positionsnummer)
                .ToDictionary(x => x.Key, x => x.OrderBy(p => p.BMK).ToList());

            var x = 0;
            var y = 0;
            foreach (var g in grouped)
            {
                Console.WriteLine($"Creating symbols for Positionsnummer: '{g.Key}' with following symbols: {g.Value.Select(x => x.BMK).Concatenate(", ")}");

                foreach (var placement in g.Value)
                {
                    var mainValueParts = new List<AttributeValuePart>
                    {
                        new(_keyMapping[nameof(ImportPlacement.Positionsnummer)], placement.Positionsnummer),
                        new(_keyMapping[nameof(ImportPlacement.BMK)], placement.BMK),
                        new(_keyMapping[nameof(ImportPlacement.Artikelnummer)], 0, placement.Artikelnummer),
                        new(_keyMapping[nameof(ImportPlacement.Hersteller)], placement.Hersteller),
                        new(_keyMapping[nameof(ImportPlacement.AlfingNummer)], placement.AlfingNummer)
                    };

                    var attributeUpdatesMainSymbol = new AttributeUpdates(
                        new PlacementsSelector(string.Empty) { TypePath = SymbolPath },
                        mainValueParts);

                    var bmkPrefix = $"{placement.Positionsnummer}-{placement.BMK}";
                    var attributeUpdatesConnector1 = new AttributeUpdates(
                        new PlacementsSelector($"{bmkPrefix}:1"),
                        new List<AttributeValuePart>
                        {
                            new(_keyMapping[nameof(ImportPlacement.Funktionstext1)], Language, placement.Funktionstext1, true)
                        });
                    var attributeUpdatesConnector2 = new AttributeUpdates(
                        new PlacementsSelector($"{bmkPrefix}:2"),
                        new List<AttributeValuePart>
                        {
                            new(_keyMapping[nameof(ImportPlacement.Funktionstext2)], Language, placement.Funktionstext2, true)
                        });

                    Console.WriteLine($"Creating symbol: {placement} ...");
                    var createdPlacements = await _connector.CreatePlacement(
                        layoutPageGuid,
                        SymbolPath,
                        x,
                        y,
                        0f,
                        attributeUpdates: new List<AttributeUpdates>
                        {
                            attributeUpdatesMainSymbol,
                            attributeUpdatesConnector1,
                            attributeUpdatesConnector2
                        });

                    Console.WriteLine("Result:");
                    DumpPlacements(createdPlacements);

                    y += yOffset;
                }

                x += xOffset;
                y = 0;
            }
        }

        private static void DumpPlacements(IEnumerable<PlacementHeader> createdPlacements)
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
                .Cast<DataColumn>()
                .ToDictionary(x => x.ColumnName, x => x.Ordinal);
            
            return exportSheet.Rows
                .Cast<DataRow>()
                .Select(row => new ImportPlacement
                {
                    Positionsnummer = Map(row, "Positionsnummer", columnMapping, Convert.ToString),
                    BMK = Map(row, "BMK", columnMapping, Convert.ToString),
                    Funktionstext1 = Map(row, "Funktionstext 1", columnMapping, Convert.ToString),
                    Funktionstext2 = Map(row, "Funktionstext 2", columnMapping, Convert.ToString),
                    Artikelnummer = Map(row, "Artikelnummer", columnMapping, Convert.ToString),
                    Hersteller = Map(row, "Hersteller", columnMapping, Convert.ToString),
                    AlfingNummer = Map(row, "Alfing Nummer", columnMapping, Convert.ToString)
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

        private class ImportPlacement
        {
            public string Positionsnummer { get; set; }
            public string BMK { get; set; }
            public string Funktionstext1 { get; set; }
            public string Funktionstext2 { get; set; }
            public string Artikelnummer { get; set; }
            public string Hersteller { get; set; }
            public string AlfingNummer { get; set; }

            public override string ToString()
            {
                return $"{Positionsnummer} - {BMK} - {Funktionstext1} - {Funktionstext2}";
            }
        }
    }
}