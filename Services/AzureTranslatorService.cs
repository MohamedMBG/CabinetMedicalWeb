using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace CabinetMedicalWeb.Services
{
    public class AzureTranslatorService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public AzureTranslatorService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<string> TranslateAsync(string text, string targetLang)
        {
            var translations = await TranslateBatchAsync(new List<string> { text }, targetLang);
            return translations.FirstOrDefault() ?? text;
        }

        public async Task<List<string>> TranslateBatchAsync(IEnumerable<string> texts, string targetLang)
        {
            var key = _configuration["AzureTranslator:Key"];
            var endpoint = _configuration["AzureTranslator:Endpoint"];
            var region = _configuration["AzureTranslator:Region"];

            var route = $"/translate?api-version=3.0&to={targetLang}";

            var body = texts.Select(text => new { Text = text }).ToArray();
            var requestBody = JsonSerializer.Serialize(body);

            using var request = new HttpRequestMessage(HttpMethod.Post, endpoint + route);
            request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");

            request.Headers.Add("Ocp-Apim-Subscription-Key", key);
            request.Headers.Add("Ocp-Apim-Subscription-Region", region);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(result);
            var translations = new List<string>();

            foreach (var item in doc.RootElement.EnumerateArray())
            {
                var translatedText = item
                    .GetProperty("translations")[0]
                    .GetProperty("text")
                    .GetString() ?? string.Empty;

                translations.Add(translatedText);
            }

            return translations;
        }
    }
}
