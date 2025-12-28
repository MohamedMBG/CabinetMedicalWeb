using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CabinetMedicalWeb.Services;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace CabinetMedicalWeb.Tests
{
    public class AzureTranslatorServiceTests
    {
        [Fact]
        public async Task TranslateBatchAsync_WithRegion_AddsHeaderAndReturnsTranslations()
        {
            var handler = new StubHandler(request =>
            {
                Assert.True(request.Headers.Contains("Ocp-Apim-Subscription-Region"));
                return CreateSuccessResponse();
            });

            var service = BuildService(handler,
                endpoint: "https://api.cognitive.microsofttranslator.com",
                region: "westeurope");

            var translations = await service.TranslateBatchAsync(new[] { "Hello" }, "fr");

            Assert.Equal("Bonjour", Assert.Single(translations));
        }

        [Fact]
        public async Task TranslateBatchAsync_WithoutRegion_ForGlobalEndpoint_Succeeds()
        {
            var handler = new StubHandler(request =>
            {
                Assert.False(request.Headers.Contains("Ocp-Apim-Subscription-Region"));
                return CreateSuccessResponse();
            });

            var service = BuildService(handler,
                endpoint: "https://api.translator.azure.com",
                region: null);

            var translations = await service.TranslateBatchAsync(new[] { "Hello" }, "fr");

            Assert.Equal("Bonjour", Assert.Single(translations));
        }

        [Fact]
        public async Task TranslateBatchAsync_MissingRegion_ForCognitiveServicesEndpoint_Throws()
        {
            var service = BuildService(new StubHandler(_ => CreateSuccessResponse()),
                endpoint: "https://api.cognitive.microsofttranslator.com",
                region: null);

            var exception = await Assert.ThrowsAsync<Exception>(() => service.TranslateBatchAsync(new[] { "Hello" }, "fr"));
            Assert.Contains("Region is missing", exception.Message);
        }

        private static AzureTranslatorService BuildService(HttpMessageHandler handler, string endpoint, string? region)
        {
            var httpClient = new HttpClient(handler);
            var configValues = new Dictionary<string, string?>
            {
                ["AzureTranslator:SubscriptionKey"] = "test-key",
                ["AzureTranslator:Endpoint"] = endpoint,
                ["AzureTranslator:Region"] = region
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configValues!)
                .Build();

            return new AzureTranslatorService(httpClient, configuration);
        }

        private static HttpResponseMessage CreateSuccessResponse()
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("[{\"translations\":[{\"text\":\"Bonjour\",\"to\":\"fr\"}]}]")
            };
        }

        private class StubHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

            public StubHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
            {
                _handler = handler;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(_handler(request));
            }
        }
    }
}
