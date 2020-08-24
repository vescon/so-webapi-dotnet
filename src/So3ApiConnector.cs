using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Sample.Dtos;
using Sample.Responses;
using WebApi.Api.V1.Layouts;

namespace Sample
{
    public class So3ApiConnector
    {
        private const string ApiPrefix = "/api/v1";

        private readonly HttpClient _client;

        public So3ApiConnector(string baseUrl)
        {
            _client = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(1),
                BaseAddress = new Uri(baseUrl)
            };
        }

        public async Task Login(string username, string password)
        {
            const string url = ApiPrefix + "/users/login";
            var request = new
            {
                Username = username,
                Password = password
            };
            var content = CreateJsonContent(request);
            var response = await _client.PostAsync(url, content);

            var responseData = await GetFromJsonContent<LoginUserResponse>(response.Content);
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {responseData.Token}");
        }

        public async Task<LayoutPageResponse?> GetLayoutPage(string path)
        {
            const string url = ApiPrefix + "/layouts";
            var parameters = new Dictionary<string, string> { { "Path", path } };
            var urlWithParameters = QueryHelpers.AddQueryString(url, parameters);
            var response = await _client.GetAsync(urlWithParameters);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            return await GetFromJsonContent<LayoutPageResponse>(response.Content);
        }
        
        public async Task<List<PlacementsHeader>> CreatePlacement(
            Guid layoutGuid,
            string placementTypePath,
            int x,
            int y,
            float rotationZ = 0,
            string? identification = null,
            List<AttributeUpdates>? attributeUpdates = null)
        {
            var url = $"{ApiPrefix}/layout/{layoutGuid}/Placements";
            var request = new
            {
                Type = new { Path = placementTypePath },
                Location = new { X = x, Y = y },
                RotationZ = rotationZ,
                Identification = identification,
                AttributeUpdates = attributeUpdates ?? new List<AttributeUpdates>()
            };
            var content = CreateJsonContent(request);
            var response = await _client.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
                throw new Exception("creating placement was not successful");

            var responseData = await GetFromJsonContent<CreatePlacementResponse>(response.Content);
            return responseData.Placements ?? new List<PlacementsHeader>();
        }

        public async Task<LayoutPageResponse> CreateLayoutPage(string path, string name)
        {
            const string url = ApiPrefix + "/layouts";
            var request = new 
            {
                Path = path,
                Name = name
            };
            var content = CreateJsonContent(request);
            var response = await _client.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
                throw new Exception("creating layout page was not successful");

            return await GetFromJsonContent<LayoutPageResponse>(response.Content);
        }

        public async Task UpdateAttributes(
            Guid layoutGuid,
            PlacementsSelector selector,
            string dataLanguage, 
            string identification = null,
            List<AttributeValuePart> valueParts = null)
        {
            var url = ApiPrefix + $"/layouts/{layoutGuid}/Placements/Attributes";
            var request = new 
            {
                Selector = selector,
                DataLanguage = dataLanguage,
                Identification = identification,
                ValueParts = valueParts
            };
            var content = CreateJsonContent(request);
            var response = await _client.PutAsync(url, content);

            if (!response.IsSuccessStatusCode)
                throw new Exception("updating attributes was not successful");
        }
        
        public async Task<GetPlacementsResponse> GetPlacements(
            Guid layoutGuid,
            string dataLanguage, 
            int? pageIndex = null,
            Guid selector_placementGuid = default,
            string selector_identification = null)
        {
            var url = ApiPrefix + $"/layouts/{layoutGuid}/Placements";
            
            var parameters = new Dictionary<string, string>
            {
                { "DataLanguage", dataLanguage }
            };
            
            if (pageIndex != null) parameters.Add("PageIndex", pageIndex.Value.ToString());
            if (selector_placementGuid != Guid.Empty) parameters.Add("PlacementGuid", selector_placementGuid.ToString());
            if (selector_identification != null) parameters.Add("IdentificationPrefix", selector_identification);

            var urlWithParameters = QueryHelpers.AddQueryString(url, parameters);
            var response = await _client.GetAsync(urlWithParameters);

            if (!response.IsSuccessStatusCode)
                throw new Exception("loading placements was not successful");
            
            return await GetFromJsonContent<GetPlacementsResponse>(response.Content);
        }
        
        private static StringContent CreateJsonContent(object request)
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json");
            return content;
        }

        private static async Task<T> GetFromJsonContent<T>(HttpContent content)
        {
            var json = await content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<T>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
    }
}
