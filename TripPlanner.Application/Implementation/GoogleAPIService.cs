using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Web;
using TripPlanner.Application.Abstraction;
using TripPlanner.Infrastructure.Database;
using static System.Net.WebRequestMethods;

namespace TripPlanner.Application.Implementation
{
    public class GoogleAPIService : IGoogleAPIService
    {
        private readonly HttpClient httpClient;
        private readonly string apiKey;

        public GoogleAPIService(HttpClient httpClient, IConfiguration configuration)
        {
            this.httpClient = httpClient;
            this.apiKey = configuration["GoogleApiKey"];
        }

        public async Task<string?> GetPhoto(string googlePlaceId, int maxWidth = 400)
        {
            var detailsUri = $"https://maps.googleapis.com/maps/api/place/details/json"
                           + $"?place_id={HttpUtility.UrlEncode(googlePlaceId)}"
                           + $"&fields=photos"
                           + $"&key={apiKey}";

            var response = await httpClient.GetAsync(detailsUri);
            if (!response.IsSuccessStatusCode)
                return null;

            using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
            var root = doc.RootElement;

            if (!root.TryGetProperty("status", out var statusElem) || statusElem.GetString() != "OK")
                return null;

            if (!root.TryGetProperty("result", out var resultElem) || resultElem.ValueKind != JsonValueKind.Object)
                return null;

            if (!resultElem.TryGetProperty("photos", out var photosElem) || photosElem.ValueKind != JsonValueKind.Array || photosElem.GetArrayLength() == 0)
                return null;

            var firstPhoto = photosElem[0];
            if (!firstPhoto.TryGetProperty("photo_reference", out var refElem))
                return null;

            var photoReference = refElem.GetString();
            if (string.IsNullOrEmpty(photoReference))
                return null;

            var photoUrl = $"https://maps.googleapis.com/maps/api/place/photo"
                         + $"?maxwidth={maxWidth}"
                         + $"&photoreference={HttpUtility.UrlEncode(photoReference)}"
                         + $"&key={apiKey}";

            return photoUrl;
        }
    }
}
