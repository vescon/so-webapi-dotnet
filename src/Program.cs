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

        public static async Task Main()
        {
            var url = EnvironmentInfo.ApiUrl;
            var connection = new So3ApiConnector(url);

            await Login(connection, url, EnvironmentInfo);

            var layoutPage = await SetupLayoutPage(connection, EnvironmentInfo);
            var layoutPageGuid = layoutPage.LayoutGuid;

            Console.WriteLine("Create anonymous placement:");
            var placement1 = await CreateAnonymousPlacement(
                connection,
                layoutPageGuid,
                EnvironmentInfo.SymbolPath,
                0);
            DumpPlacements(placement1);

            Console.WriteLine("Create placement with identification:");
            var placement2 = await CreatePlacementWithIdentification(
                connection, layoutPageGuid,
                EnvironmentInfo.SymbolPath,
                100,
                EnvironmentInfo.Identification);
            DumpPlacements(placement2);

            Console.WriteLine("Create placement with attribute updates (identifying):");
            var placement3 =
                await CreatePlacementWithAttributeUpdates(
                    connection,
                    layoutPageGuid,
                    EnvironmentInfo.SymbolPath,
                    200,
                    EnvironmentInfo.AttributeValuePartsIdentifying);
            DumpPlacements(placement3);

            Console.WriteLine("Create placement with attribute updates (descriptive multilanguage):");
            var placement4 = await CreatePlacementWithAttributeUpdates(
                connection,
                layoutPageGuid,
                EnvironmentInfo.SymbolPath,
                300,
                EnvironmentInfo.AttributeValuePartsDescriptiveMultilanguage);
            DumpPlacements(placement4);

            Console.WriteLine("Create placement with attribute updates (property indexed):");
            var placement5 = await CreatePlacementWithAttributeUpdates(
                connection,
                layoutPageGuid,
                EnvironmentInfo.SymbolPath,
                400,
                EnvironmentInfo.AttributeValuePartsPropertyIndexed);
            DumpPlacements(placement5);
        }

        private static async Task Login(So3ApiConnector connection, string url, EnvironmentInfoBase environmentInfo)
        {
            Console.WriteLine($"Login into '{url}'");
            await connection.Login(environmentInfo.Username, environmentInfo.Password);
            Console.WriteLine("Successfully Logged in");
        }

        private static async Task<LayoutPageResponse> SetupLayoutPage(
            So3ApiConnector connection,
            EnvironmentInfoBase environmentInfo)
        {
            var path = environmentInfo.LayoutFacilityPath;
            var name = environmentInfo.LayoutPageName;
            var fullPath = $"{path}/{name}";

            // Setup layout page
            Console.WriteLine($"Checking if Layout page with the path '<{fullPath}>' exists...");
            var layoutPage = await connection.GetLayoutPage(fullPath);
            if (layoutPage == null)
            {
                Console.WriteLine("Layout page doesn't exist. Creating ...");
                layoutPage = await connection.CreateLayoutPage(path, name);
                Console.WriteLine($"Layout page {layoutPage.Name} created with guid {layoutPage.LayoutGuid}");
            }
            else
                Console.WriteLine($"Layout page {layoutPage.Name} exists with guid {layoutPage.LayoutGuid}");

            return layoutPage;
        }

        private static async Task<PlacementsHeader> CreateAnonymousPlacement(
            So3ApiConnector connection,
            Guid layoutPageGuid,
            string symbolPath,
            int locationX)
        {
            var placements = await connection.CreatePlacement(
                layoutPageGuid,
                symbolPath,
                locationX,
                0);
            return placements.Single();
        }

        private static async Task<PlacementsHeader> CreatePlacementWithIdentification(
            So3ApiConnector connection,
            Guid layoutPageGuid,
            string symbolPath,
            int locationX,
            string identification)
        {
            var placements = await connection.CreatePlacement(
                layoutPageGuid,
                symbolPath,
                locationX,
                0,
                identification: identification);
            return placements.Single();
        }

        private static async Task<PlacementsHeader> CreatePlacementWithAttributeUpdates(
            So3ApiConnector connection,
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
                var placements = await connection.CreatePlacement(
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

        private static void DumpPlacements(PlacementsHeader placement)
        {
            if (placement == null)
                return;

            Console.WriteLine($"Created: Guid: {placement.Guid} - Identification: {placement.Identification}");
        }
    }
}