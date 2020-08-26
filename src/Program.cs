using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sample.Dtos;
using Sample.EnvironmentInfos;
using Sample.Responses;
using WebApi.Api.V1.Layouts;

namespace Sample
{
    public static class Program
    {
        private static readonly EnvironmentInfoBase EnvironmentInfo = new So3LocalTestingExtern06();

        public static async Task Main()
        {
            var url = EnvironmentInfo.ApiUrl;
            var connector = new So3ApiConnector(url);

            await Login(connector, url, EnvironmentInfo);

            var layoutPage = await SetupLayoutPage(connector, EnvironmentInfo);
            var layoutPageGuid = layoutPage.LayoutGuid;

            Console.WriteLine("Create anonymous placement:");
            var placement1 = await CreateAnonymousPlacement(
                connector,
                layoutPageGuid,
                EnvironmentInfo.SymbolPath,
                0,
                (float) Math.PI / 2);
            DumpPlacement(placement1);

            Console.WriteLine("Create placement with identification:");
            var placement2 = await CreatePlacementWithIdentification(
                connector, layoutPageGuid,
                EnvironmentInfo.SymbolPath,
                100,
                EnvironmentInfo.Identification);
            DumpPlacement(placement2);

            Console.WriteLine("Create placement with attribute updates (identifying):");
            var placement3 =
                await CreatePlacementWithAttributeUpdates(
                    connector,
                    layoutPageGuid,
                    EnvironmentInfo.SymbolPath,
                    200,
                    EnvironmentInfo.AttributeValuePartsIdentifying);
            DumpPlacement(placement3);

            Console.WriteLine("Create placement with attribute updates (descriptive multilanguage):");
            var placement4 = await CreatePlacementWithAttributeUpdates(
                connector,
                layoutPageGuid,
                EnvironmentInfo.SymbolPath,
                300,
                EnvironmentInfo.AttributeValuePartsDescriptiveMultilanguage);
            DumpPlacement(placement4);

            Console.WriteLine("Create placement with attribute updates (property indexed):");
            var placement5 = await CreatePlacementWithAttributeUpdates(
                connector,
                layoutPageGuid,
                EnvironmentInfo.SymbolPath,
                400,
                EnvironmentInfo.AttributeValuePartsPropertyIndexed);
            DumpPlacement(placement5);
            
            Console.WriteLine("Load placement 5 infos via placement guid");
            var loadedPlacement5 = connector.GetPlacementsAsync(
                layoutPageGuid,
                "en-US",
                selectorPlacementGuid: placement5.Guid
            );
            await foreach (var placement in loadedPlacement5)
                DumpPlacement(placement);

            Console.WriteLine("Load placement 2 infos via identification");
            var loadedPlacement2 = connector.GetPlacementsAsync(
                layoutPageGuid,
                "en-US",
                selectorIdentification: EnvironmentInfo.Identification
            );
            await foreach (var placement in loadedPlacement2)
                DumpPlacement(placement);
            
            Console.WriteLine("Update Placement 2 attribute value parts");
            await connector.UpdateAttributes(
                layoutPageGuid,
                new PlacementsSelector(placement2.Guid),
                "en-US",
                valueParts: EnvironmentInfo.AttributeValuePartsDescriptiveMultilanguage
            );
            
            Console.WriteLine("Update Placement 2 attributes via identification");
            await connector.UpdateAttributes(
                layoutPageGuid,
                new PlacementsSelector(EnvironmentInfo.Identification),
                "en-US",
                identification: EnvironmentInfo.Identification
            );
        }

        private static async Task Login(So3ApiConnector connector, string url, EnvironmentInfoBase environmentInfo)
        {
            Console.WriteLine($"Login into '{url}'");
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
            
            Console.WriteLine($"Guid: {placement.Guid} Identification: {placement.Identification} Location: {placement.Location} - RotationZ: {placement.RotationZ}");
            Dump(placement.AttributeValueParts);
        }

        private static void Dump(List<AttributeValuePart> attributeValueParts)
        {
            Console.WriteLine("Attribute value parts: ");
            attributeValueParts
                .ForEach(
                    x =>
                        Console.WriteLine($"Name: {x.Name} - {x.Value}- {x.Language}- {x.Index}- {x.Description}"));
        }
    }
}