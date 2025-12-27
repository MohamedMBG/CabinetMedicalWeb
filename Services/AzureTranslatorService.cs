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
    var key = _configuration["AzureTranslator:SubscriptionKey"]; // ✅ fixed
    var endpoint = _configuration["AzureTranslator:Endpoint"]?.TrimEnd('/'); // ✅ fixed
    var region = _configuration["AzureTranslator:Region"];

    if (string.IsNullOrWhiteSpace(key))
        throw new Exception("AzureTranslator SubscriptionKey is missing. Check appsettings.json key name.");

    if (string.IsNullOrWhiteSpace(endpoint))
        throw new Exception("AzureTranslator Endpoint is missing.");

    var route = $"/translate?api-version=3.0&to={targetLang}";
    var body = texts.Select(t => new { Text = t }).ToArray();
    var requestBody = JsonSerializer.Serialize(body);

    using var request = new HttpRequestMessage(HttpMethod.Post, endpoint + route);
    request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");

    request.Headers.Add("Ocp-Apim-Subscription-Key", key);
    request.Headers.Add("Ocp-Apim-Subscription-Region", region);

    var response = await _httpClient.SendAsync(request);
    var result = await response.Content.ReadAsStringAsync();

    if (!response.IsSuccessStatusCode)
        throw new Exception($"Translator failed: {(int)response.StatusCode} {response.ReasonPhrase}\n{result}");

    var translationResponse = JsonSerializer.Deserialize<List<TranslationResponse>>(result) ?? new();

    return translationResponse
        .Select(item => item.Translations.FirstOrDefault()?.Text ?? string.Empty)
        .ToList();
}


    }

    public class TranslationResponse
    {
        public List<TranslationItem> Translations { get; set; } = new();
    }

    public class TranslationItem
    {
        public string Text { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
    }
}