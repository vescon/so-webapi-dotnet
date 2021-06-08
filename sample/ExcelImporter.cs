using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExcelDataReader;
using Vescon.So.WebApi.Client;
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
            
            // import to layout page
            foreach (var placement in placements.Where(x=>x.PlacementType == "SymbolReference"))
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
                foreach (var createdPlacement in createdPlacements.OrderBy(x=>x.Identification ?? string.Empty))
                    Console.WriteLine($@"{createdPlacement.Guid} - {createdPlacement.Identification}");
            }
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
                    X = Map(row, "X", columnMapping, Convert.ToSingle),
                    Y = Map(row, "Y", columnMapping, Convert.ToSingle),
                    Z = Map(row, "Z", columnMapping, Convert.ToSingle),
                    RotationZ = Map(row, "Rotation Z", columnMapping, Convert.ToSingle),
                    SymbolPath = Map(row, "Symbol path", columnMapping, Convert.ToString),
                    RegionName = Map(row, "Region name", columnMapping, Convert.ToString)
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

        public override string ToString()
        {
            return $"{PlacementGuid} - {FullIdentifyingValue} - {PlacementType} - {SymbolPath}";
        }
    }
}