using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sample.Dtos;
using Sample.EnvironmentInfos;
using Sample.Responses;

namespace Sample
{
    public static class Program
    {
        private static readonly EnvironmentInfoBase EnvironmentInfo = new So3LocalWebApiSource();
        private static PlacementHeader _placement1;
        private static PlacementHeader _placement2;
        private static PlacementHeader _placement3;
        private static PlacementHeader _placement4;
        private static PlacementHeader _placement5;

        public static async Task Main()
        {
            var url = EnvironmentInfo.ApiUrl;
            var connector = new So3ApiConnector(url);

            await Login(connector, url, EnvironmentInfo);

            var layoutPage = await SetupLayoutPage(connector, EnvironmentInfo);
            var layoutPageGuid = layoutPage.LayoutGuid;

            await CreatePlacements(connector, layoutPageGuid);
            await LoadPlacements(connector, layoutPageGuid);
            await UpdatePlacements(connector, layoutPageGuid);
        }

        private static async Task UpdatePlacements(So3ApiConnector connector, Guid layoutPageGuid)
        {
            Console.WriteLine();
            Console.WriteLine("Update Placement 2 attribute value parts");
            await connector.UpdateAttributes(
                layoutPageGuid,
                new PlacementsSelector(_placement2.Guid),
                "en-US",
                valueParts: EnvironmentInfo.AttributeValuePartsDescriptiveMultilanguage
            );

            Console.WriteLine();
            Console.WriteLine("Update Placement 2 attributes via identification");
            await connector.UpdateAttributes(
                layoutPageGuid,
                new PlacementsSelector(EnvironmentInfo.Identification),
                "en-US",
                identification: "==123=XXX+457"
            );
        }

        private static async Task LoadPlacements(So3ApiConnector connector, Guid layoutPageGuid)
        {
            Console.WriteLine();
            Console.WriteLine("Load placement 4 infos via placement guid (en-US)");
            var loadedPlacement4 = connector.GetPlacementsAsync(
                layoutPageGuid,
                "en-US",
                selectorPlacementGuid: _placement4.Guid
            );
            await foreach (var placement in loadedPlacement4)
                DumpPlacement(placement);

            Console.WriteLine();
            Console.WriteLine("Load placement 4 infos via placement guid (de-DE)");
            loadedPlacement4 = connector.GetPlacementsAsync(
                layoutPageGuid,
                "de-DE",
                selectorPlacementGuid: _placement4.Guid
            );
            await foreach (var placement in loadedPlacement4)
                DumpPlacement(placement);

            Console.WriteLine();
            Console.WriteLine("Load placement 5 infos via placement guid");
            var loadedPlacement5 = connector.GetPlacementsAsync(
                layoutPageGuid,
                "en-US",
                selectorPlacementGuid: _placement5.Guid
            );
            await foreach (var placement in loadedPlacement5)
                DumpPlacement(placement);

            Console.WriteLine();
            Console.WriteLine("Load placement 2 infos via identification");
            var loadedPlacement2 = connector.GetPlacementsAsync(
                layoutPageGuid,
                "en-US",
                selectorIdentification: EnvironmentInfo.Identification
            );
            await foreach (var placement in loadedPlacement2)
                DumpPlacement(placement);
        }

        private static async Task CreatePlacements(So3ApiConnector connector, Guid layoutPageGuid)
        {
            Console.WriteLine();
            Console.WriteLine("Create anonymous placement 1:");
            _placement1 = await CreateAnonymousPlacement(
                connector,
                layoutPageGuid,
                EnvironmentInfo.SymbolPath,
                100,
                (float) Math.PI / 2);
            DumpPlacement(_placement1);

            Console.WriteLine();
            Console.WriteLine("Create placement 2 with identification:");
            _placement2 = await CreatePlacementWithIdentification(
                connector, layoutPageGuid,
                EnvironmentInfo.SymbolPath,
                200,
                EnvironmentInfo.Identification);
            DumpPlacement(_placement2);

            Console.WriteLine();
            Console.WriteLine("Create placement 3 with attribute updates (identifying):");
            _placement3 = await CreatePlacementWithAttributeUpdates(
                connector,
                layoutPageGuid,
                EnvironmentInfo.SymbolPath,
                300,
                EnvironmentInfo.AttributeValuePartsIdentifying);
            DumpPlacement(_placement3);

            Console.WriteLine();
            Console.WriteLine("Create placement 4 with attribute updates (descriptive multilanguage):");
            _placement4 = await CreatePlacementWithAttributeUpdates(
                connector,
                layoutPageGuid,
                EnvironmentInfo.SymbolPath,
                400,
                EnvironmentInfo.AttributeValuePartsDescriptiveMultilanguage);
            DumpPlacement(_placement4);

            Console.WriteLine();
            Console.WriteLine("Create placement 5 with attribute updates (property indexed):");
            _placement5 = await CreatePlacementWithAttributeUpdates(
                connector,
                layoutPageGuid,
                EnvironmentInfo.SymbolPath,
                500,
                EnvironmentInfo.AttributeValuePartsPropertyIndexed);
            DumpPlacement(_placement5);
        }

        private static async Task Login(So3ApiConnector connector, string url, EnvironmentInfoBase environmentInfo)
        {
            Console.WriteLine($"Login into '{url}'...");
            await connector.Login(environmentInfo.Username, environmentInfo.Password);
            Console.WriteLine("Successfully Logged in");
        }

        private static async Task<LayoutPageResponse> SetupLayoutPage(
            So3ApiConnector connector,
            EnvironmentInfoBase environmentInfo)
        {
            var path = environmentInfo.LayoutFacilityPath;
            var name = environmentInfo.LayoutPageName;
            var fullPath = $"{path}/{name}";

            // Setup layout page
            Console.WriteLine($"Checking if Layout page with the path '<{fullPath}>' exists...");
            var layoutPage = await connector.GetLayoutPage(fullPath);
            if (layoutPage == null)
            {
                Console.WriteLine("Layout page doesn't exist. Creating ...");
                layoutPage = await connector.CreateLayoutPage(path, name);
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

        private static async Task<PlacementHeader> CreatePlacementWithIdentification(
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
            return placements.Single();
        }

        private static async Task<PlacementHeader> CreatePlacementWithAttributeUpdates(
            So3ApiConnector connector,
            Guid layoutPageGuid,
            string symbolPath,
            int locationX,
            List<AttributeValuePart> attributeValueParts)
        {

            var attributeUpdates = new List<AttributeUpdates>
            {
                new AttributeUpdates(
                    new PlacementsSelector(true),
                    attributeValueParts)
            };

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
                return null;
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
                $"Guid: {placement.Guid} - Identification: {placement.Identification} - TypePath: '{placement.TypePath}' - Location: {placement.Location} - RotationZ: {placement.RotationZ}");
            Dump(placement.AttributeValueParts);
            Console.WriteLine("-----------------------------------");
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