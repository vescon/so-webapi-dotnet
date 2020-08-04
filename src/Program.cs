using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sample.Dtos;
using Sample.Responses;

namespace Sample
{
    public static class Program
    {
        private const string SymbolPath = "SO3/Master Data/Symbol Libraries/Symbols - Symbole/17 Robots - Roboter/ROB01";

        public static async Task Main()
        {
            var url = "http://localhost:5000";
            var connection = new So3ApiConnector(url);

            Console.WriteLine("Login ...");
            await connection.Login("Username", "Password");
            Console.WriteLine("Successfully Logged in");

            var path = "SO3/Projects/Sample/Sample/Sample/Sample Facility";
            var name = "page01";
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

            // Create layout placements
            Console.WriteLine("Create anonymous placement:");
            var placements = await connection.CreatePlacement(
                layoutPage.LayoutGuid,
                SymbolPath,
                0,
                0);
            Console.WriteLine("Created placement:");
            DumpPlacements(placements);

            Console.WriteLine("Create placement via identification:");
            placements = await connection.CreatePlacement(
                layoutPage.LayoutGuid,
                SymbolPath,
                100,
                100,
                identification: "==123=ABC+456");
            Console.WriteLine("Created placement:");
            DumpPlacements(placements);

            Console.WriteLine("Create placement via attribute updates:");
            var attributeUpdates = new List<AttributeUpdates>
            {
                new AttributeUpdates(
                    new PlacementsSelector(true),
                    new List<AttributeValuePart>
                    {
                        new AttributeValuePart("Plant", "XXX")
                    })
            };

            placements = await connection.CreatePlacement(
                layoutPage.LayoutGuid,
                SymbolPath,
                200,
                200,
                attributeUpdates: attributeUpdates);
            Console.WriteLine("Created placement:");
            DumpPlacements(placements);
        }

        private static void DumpPlacements(List<PlacementsHeader> placements)
        {
            placements?.ForEach(x => Console.WriteLine($"Guid: {x.Guid} Identification: {x.Identification}"));
        }
    }
}