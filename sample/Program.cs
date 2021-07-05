using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Sample.EnvironmentInfos;
using Vescon.So.WebApi.Client;
using Vescon.So.WebApi.Client.Dtos;
using Vescon.So.WebApi.Client.Responses;

namespace Sample
{
    public static class Program
    {
        private static readonly EnvironmentInfoBase EnvironmentInfo = new So3LocalWebApiSource();

        private static PlacementHeader _symbolReference1;
        private static PlacementHeader _symbolReference2;
        private static PlacementHeader _symbolReference3;
        private static PlacementHeader _symbolReference4;
        private static PlacementHeader _symbolReference5;

        private static List<PlacementHeader> _symbolReferenceWithConnectors;

        private static string _facilityPath;
        private static string _pageName;

        public static async Task Main()
        {
            ReadExcelImportPath();

            var url = EnvironmentInfo.ApiUrl;
            var connector = new So3ApiConnector(url);

            await Login(connector, url, EnvironmentInfo);

            ////await RunSimpleImport(connector);
            await RunExcelImport(connector); // requires matching symbol/macro paths
        }

        private static void ReadExcelImportPath()
        {
            var defaultPath = $"{EnvironmentInfo.ExcelImportFacilityPath}/{EnvironmentInfo.ExcelImportPageName}";
            var effectivePath = defaultPath;
#if !DEBUG
            Console.Write($"Please enter layout import path: [{defaultPath}] ");
            var input = Console.ReadLine();
            effectivePath = string.IsNullOrEmpty(input) ? defaultPath : input;
#endif
            var path = effectivePath.Split('/');
            _facilityPath = path.Take(path.Length - 1).Concatenate("/");
            _pageName = path.Last();
        }

        private static async Task RunSimpleImport(So3ApiConnector connector)
        {
            var layoutPage = await SetupLayoutPage(
                connector,
                EnvironmentInfo.LayoutFacilityPath,
                EnvironmentInfo.LayoutPageName);
            var layoutPageGuid = layoutPage.LayoutGuid;

            await CreateSymbolReferences(connector, layoutPageGuid);
            await LoadPlacements(connector, layoutPageGuid);
            await UpdatePlacements(connector, layoutPageGuid);

            await CreateSymbolReferenceWithConnectors(connector, layoutPageGuid);
            await UpdateConnector(connector, layoutPageGuid);

            await UpdateMarkedForDeletion(connector, layoutPageGuid);
            await CreateMacroReference(connector, layoutPageGuid);
        }

        private static async Task RunExcelImport(So3ApiConnector connector)
        {
            var file = EnvironmentInfo.ExcelFile;
            if (string.IsNullOrEmpty(file))
                return;
            var path = Path.Combine(Environment.CurrentDirectory, file);
            
            Console.WriteLine("Importing excel file: " + path);
            var importFacilityPath = _facilityPath ?? EnvironmentInfo.ExcelImportFacilityPath;
            var importPageName = _pageName ?? EnvironmentInfo.ExcelImportPageName;
            var layoutPage = await SetupLayoutPage(
                connector,
                importFacilityPath,
                importPageName);
            var layoutPageGuid = layoutPage.LayoutGuid;

            var importer = new ExcelImporter(connector);
            await importer.ImportFromFile(path, layoutPageGuid);
        }

        private static async Task Login(So3ApiConnector connector, string url, EnvironmentInfoBase environmentInfo)
        {
            Console.WriteLine($"Login into '{url}'...");
            await connector.Login(environmentInfo.Username, environmentInfo.Password);
            Console.WriteLine("Successfully Logged in");
        }

        private static async Task CreateSymbolReferences(So3ApiConnector connector, Guid layoutPageGuid)
        {
            Console.WriteLine();
            Console.WriteLine("Create anonymous symbol reference 1:");
            _symbolReference1 = await CreateAnonymousPlacement(
                connector,
                layoutPageGuid,
                EnvironmentInfo.SymbolPath,
                100,
                (float) Math.PI / 2);
            DumpPlacement(_symbolReference1);

            Console.WriteLine();
            Console.WriteLine("Create symbol reference 2 via identification:");
            _symbolReference2 = (await CreatePlacementWithIdentification(
                connector, layoutPageGuid,
                EnvironmentInfo.SymbolPath,
                200,
                EnvironmentInfo.Identification)).Single();
            DumpPlacement(_symbolReference2);

            Console.WriteLine();
            Console.WriteLine("Create symbol reference 3 via attribute updates (identifying):");
            _symbolReference3 = await CreatePlacementWithAttributeUpdates(
                connector,
                layoutPageGuid,
                EnvironmentInfo.SymbolPath,
                300,
                EnvironmentInfo.AttributeValuePartsIdentifying);
            DumpPlacement(_symbolReference3);

            if (EnvironmentInfo.AttributeValuePartsDescriptiveMultilanguage != null)
            {
                Console.WriteLine();
                Console.WriteLine("Create symbol reference 4 via attribute updates (descriptive multi-language):");
                _symbolReference4 = await CreatePlacementWithAttributeUpdates(
                    connector,
                    layoutPageGuid,
                    EnvironmentInfo.SymbolPath,
                    400,
                    EnvironmentInfo.AttributeValuePartsDescriptiveMultilanguage);
                DumpPlacement(_symbolReference4);
            }

            if (EnvironmentInfo.AttributeValuePartsPropertyIndexed != null)
            {
                Console.WriteLine();
                Console.WriteLine("Create symbol reference 5 via attribute updates (property indexed):");
                _symbolReference5 = await CreatePlacementWithAttributeUpdates(
                    connector,
                    layoutPageGuid,
                    EnvironmentInfo.SymbolPath,
                    500,
                    EnvironmentInfo.AttributeValuePartsPropertyIndexed);
                DumpPlacement(_symbolReference5);
            }
        }

        private static async Task CreateSymbolReferenceWithConnectors(So3ApiConnector connector, Guid layoutPageGuid)
        {
            Console.WriteLine();
            Console.WriteLine("Create symbol reference with connectors via identification:");
            _symbolReferenceWithConnectors = await CreatePlacementWithIdentification(
                connector, layoutPageGuid,
                EnvironmentInfo.SymbolPathWithConnectors,
                -100,
                EnvironmentInfo.IdentificationMainSymbolReference);
            _symbolReferenceWithConnectors.ForEach(DumpPlacement);
        }

        private static async Task UpdateConnector(So3ApiConnector connector, Guid layoutPageGuid)
        {
            var connectorReferenceGuid = _symbolReferenceWithConnectors
                .Single(x => x.Identification == EnvironmentInfo.IdentificationConnector).Guid;

            Console.WriteLine();
            Console.WriteLine("Update connector");
            await connector.UpdateAttributes(
                layoutPageGuid,
                new PlacementsSelector(connectorReferenceGuid),
                EnvironmentInfo.IdentificationConnectorNew
            );
        }

        private static async Task LoadPlacements(So3ApiConnector connector, Guid layoutPageGuid)
        {
            if (_symbolReference4 != null)
            {
                Console.WriteLine();
                Console.WriteLine("Load symbol reference 4 infos via placement guid (en-US)");
                var placements = await connector.GetPlacements(
                    layoutPageGuid,
                    "en-US",
                    selectorPlacementGuid: _symbolReference4.Guid
                );
                DumpPlacements(placements);

                Console.WriteLine();
                Console.WriteLine("Load symbol reference 4 infos via placement guid (de-DE)");
                placements = await connector.GetPlacements(
                    layoutPageGuid,
                    "de-DE",
                    selectorPlacementGuid: _symbolReference4.Guid
                );
                DumpPlacements(placements);
            }

            if (_symbolReference5 != null)
            {
                Console.WriteLine();
                Console.WriteLine("Load symbol reference 5 infos via placement guid");
                var placements = await connector.GetPlacements(
                    layoutPageGuid,
                    "en-US",
                    selectorPlacementGuid: _symbolReference5.Guid
                );
                DumpPlacements(placements);
            }

            Console.WriteLine();
            Console.WriteLine("Load symbol reference 2 infos via identification");
            var placements2 = await connector.GetPlacements(
                layoutPageGuid,
                "en-US",
                selectorIdentificationPrefix: EnvironmentInfo.Identification
            );
            DumpPlacements(placements2);

            Console.WriteLine();
            Console.WriteLine("Load symbol reference via type path");
            var placements3 = await connector.GetPlacements(
                layoutPageGuid,
                "en-US",
                selectorTypePath: EnvironmentInfo.SymbolPath
            );
            DumpPlacements(placements3);
        }

        private static async Task UpdatePlacements(So3ApiConnector connector, Guid layoutPageGuid)
        {
            Console.WriteLine();
            Console.WriteLine("Update symbol reference 2 attribute value parts");
            await connector.UpdateAttributes(
                layoutPageGuid,
                new PlacementsSelector(_symbolReference2.Guid),
                "en-US",
                valueParts: EnvironmentInfo.AttributeValuePartsDescriptiveMultilanguage
            );

            Console.WriteLine();
            Console.WriteLine("Update symbol reference 2 attributes via identification");
            await connector.UpdateAttributes(
                layoutPageGuid,
                new PlacementsSelector(EnvironmentInfo.Identification),
                identification: "==123=XXX+457"
            );
        }

        private static async Task UpdateMarkedForDeletion(So3ApiConnector connector, Guid layoutPageGuid)
        {
            Console.WriteLine();
            Console.WriteLine("Update symbol reference 2 MarkedForDeletion to true");
            await connector.UpdateMarkedForDeletion(
                layoutPageGuid,
                new PlacementsSelector(_symbolReference2.Guid),
                true
            );

            Console.WriteLine();
            Console.WriteLine("Load symbol reference 2 infos via identification");
            var loadedPlacement2 = await connector.GetPlacements(
                layoutPageGuid,
                "en-US",
                _symbolReference2.Guid
            );
            foreach (var placement in loadedPlacement2)
                DumpPlacement(placement);
        }

        private static async Task CreateMacroReference(So3ApiConnector connector, Guid layoutPageGuid)
        {
            Console.WriteLine();
            Console.WriteLine("Create macro reference:");
            var placements = await connector.CreatePlacement(
                layoutPageGuid,
                EnvironmentInfo.MacroPath,
                100,
                300,
                (float) Math.PI / 2,
                "==XXX");
            placements.ForEach(DumpPlacement);

            Console.WriteLine();
            Console.WriteLine("Load macro placements via identification:");
            var loadedPlacement4 = await connector.GetPlacements(
                layoutPageGuid,
                "en-US",
                selectorIdentificationPrefix: "==XXX"
            );
            foreach (var placement in loadedPlacement4)
                DumpPlacement(placement);
        }

        private static async Task<LayoutPageResponse> SetupLayoutPage(
            So3ApiConnector connector,
            string pagePath,
            string pageName)
        {
            var fullPath = $"{pagePath}/{pageName}";

            // Setup layout page
            Console.WriteLine($"Checking if Layout page with the path '<{fullPath}>' exists...");
            var layoutPage = await connector.GetLayoutPage(fullPath);
            if (layoutPage == null)
            {
                Console.WriteLine("Layout page doesn't exist. Creating ...");
                layoutPage = await connector.CreateLayoutPage(pagePath, pageName);
                Console.WriteLine($"Layout page {layoutPage.Name} created with guid {layoutPage.LayoutGuid}");
            }
            else
                Console.WriteLine($"Layout page {layoutPage.Name} exists with guid {layoutPage.LayoutGuid}");

            return layoutPage;
        }

        private static async Task<PlacementHeader> CreateAnonymousPlacement(
            So3ApiConnector connector,
            Guid layoutPageGuid,
            string symbolPath,
            int locationX,
            float rotationZ)
        {
            var placements = await connector.CreatePlacement(
                layoutPageGuid,
                symbolPath,
                locationX,
                0,
                rotationZ);
            return placements.Single();
        }

        private static async Task<List<PlacementHeader>> CreatePlacementWithIdentification(
            So3ApiConnector connector,
            Guid layoutPageGuid,
            string symbolPath,
            int locationX,
            string identification)
        {
            var placements = await connector.CreatePlacement(
                layoutPageGuid,
                symbolPath,
                locationX,
                0,
                identification: identification);
            return placements;
        }

        private static async Task<PlacementHeader> CreatePlacementWithAttributeUpdates(
            So3ApiConnector connector,
            Guid layoutPageGuid,
            string symbolPath,
            int locationX,
            List<AttributeValuePart> attributeValueParts)
        {
            var attributeUpdates = new List<AttributeUpdates> {new AttributeUpdates(attributeValueParts)};

            try
            {
                var placements = await connector.CreatePlacement(
                    layoutPageGuid,
                    symbolPath,
                    locationX,
                    0,
                    attributeUpdates: attributeUpdates);
                return placements.Single();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error creating placement: " + ex.Message);
                var keyPartNames = attributeValueParts
                    .Select(x => x.Name)
                    .Distinct()
                    .OrderBy(x => x)
                    .Concatenate(", ");
                Console.WriteLine("Please verify that all key part names are correctly configured: " + keyPartNames);
                throw;
            }
        }

        private static void DumpPlacement(PlacementHeader placement)
        {
            if (placement == null)
                return;

            Console.WriteLine($"Created: Guid: {placement.Guid} - Identification: {placement.Identification}");
        }

        private static void DumpPlacement(Placement placement)
        {
            if (placement == null)
                return;

            Console.WriteLine("-----------------------------------");
            Console.WriteLine(
                $"Guid: {placement.Guid} - Identification: {placement.Identification} - Type: {placement.Type} - TypePath: '{placement.TypePath}' - Location: ({placement.Location.X}/{placement.Location.Y}) - RotationZ: {placement.RotationZ}");
            Dump(placement.AttributeValueParts);
            Console.WriteLine("-----------------------------------");
        }

        private static void DumpPlacements(IEnumerable<Placement> placements)
        {
            foreach (var placement in placements)
                DumpPlacement(placement);
        }

        private static void Dump(List<AttributeValuePart> attributeValueParts)
        {
            Console.WriteLine("Attribute value parts: ");

            attributeValueParts
                .ForEach(
                    x =>
                    {
                        var valueString = new List<string>
                            {
                                x.Name,
                                x.IsOverwritten.ToString(),
                                x.Index?.ToString(),
                                x.Language,
                                x.Value,
                                x.Description
                            }
                            .Where(v => v != null)
                            .Concatenate(" - ");
                        Console.WriteLine($"Name: {valueString}");
                    });
        }
    }
}