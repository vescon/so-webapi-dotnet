using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sample.Dtos;
using Sample.Responses;
using Sample.SampleInfos;

namespace Sample
{
    public static class Program
    {
        private static readonly SampleInfo SampleInfo = new So3LocalWebApiSource();

        public static async Task Main()
        {
            var url = SampleInfo.ApiUrl;
            var connection = new So3ApiConnector(url);

            await Login(connection, url, SampleInfo);

            var layoutPage = await SetupLayoutPage(connection, SampleInfo);
            var layoutPageGuid = layoutPage.LayoutGuid;

            var placement1 = await CreateAnonymousPlacement(connection, layoutPageGuid, SampleInfo);
            DumpPlacements(placement1);

            var placement2 = await CreatePlacementWithIdentification(connection, layoutPageGuid, SampleInfo);
            DumpPlacements(placement2);

            var placement3 = await CreatePlacementWithAttributeUpdatesIdentifying(connection, layoutPageGuid, SampleInfo);
            DumpPlacements(placement3);
            
            var placement4 = await CreatePlacementWithAttributeUpdatesDescriptiveMultilanguage(connection, layoutPageGuid, SampleInfo);
            DumpPlacements(placement3);
        }

        private static async Task Login(So3ApiConnector connection, string url, SampleInfo sampleInfo)
        {
            Console.WriteLine($"Login into '{url}'");
            await connection.Login(sampleInfo.Username, sampleInfo.Password);
            Console.WriteLine("Successfully Logged in");
        }

        private static async Task<LayoutPageResponse> SetupLayoutPage(
            So3ApiConnector connection,
            SampleInfo sampleInfo)
        {
            var path = sampleInfo.LayoutFacilityPath;
            var name = sampleInfo.LayoutPageName;
            var fullPath = $"{path}/{name}";

            // Setup layout page
            Console.WriteLine($"Check if Layout page with the path (<{fullPath}>)exists");
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
            SampleInfo sampleInfo)
        {
            Console.WriteLine("Create anonymous placement:");
            var placements = await connection.CreatePlacement(
                layoutPageGuid,
                sampleInfo.SymbolPath,
                0,
                0);
            return placements.Single();
        }

        private static async Task<PlacementsHeader> CreatePlacementWithIdentification(
            So3ApiConnector connection,
            Guid layoutPageGuid,
            SampleInfo sampleInfo)
        {
            Console.WriteLine("Create placement with identification:");
            var placements = await connection.CreatePlacement(
                layoutPageGuid,
                sampleInfo.SymbolPath,
                100,
                100,
                identification: sampleInfo.Identification);
            return placements.Single();
        }

        private static async Task<PlacementsHeader> CreatePlacementWithAttributeUpdatesIdentifying(
            So3ApiConnector connection,
            Guid layoutPageGuid,
            SampleInfo sampleInfo)
        {
            Console.WriteLine("Create placement with attribute updates (identifying):");
            var attributeUpdates = new List<AttributeUpdates>
            {
                new AttributeUpdates(
                    new PlacementsSelector(true),
                    sampleInfo.AttributeValuePartsIdentifying)
            };

            var placements = await connection.CreatePlacement(
                layoutPageGuid,
                sampleInfo.SymbolPath,
                200,
                200,
                attributeUpdates: attributeUpdates);
            return placements.Single();
        }
        
        private static async Task<PlacementsHeader> CreatePlacementWithAttributeUpdatesDescriptiveMultilanguage(
            So3ApiConnector connection,
            Guid layoutPageGuid,
            SampleInfo sampleInfo)
        {
            Console.WriteLine("Create placement with attribute updates (descriptive multilanguage):");
            var attributeUpdates = new List<AttributeUpdates>
            {
                new AttributeUpdates(
                    new PlacementsSelector(true),
                    sampleInfo.AttributeValuePartsDescriptiveMultilanguage)
            };

            var placements = await connection.CreatePlacement(
                layoutPageGuid,
                sampleInfo.SymbolPath,
                200,
                200,
                attributeUpdates: attributeUpdates);
            return placements.Single();
        }

        private static void DumpPlacements(PlacementsHeader placement)
        {
            Console.WriteLine("Created placement:");
            Console.WriteLine($"Guid: {placement.Guid} Identification: {placement.Identification}");
        }
    }
}