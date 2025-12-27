using System.Collections.Generic;
using CabinetMedicalWeb.Services;
using Microsoft.AspNetCore.Mvc;

namespace CabinetMedicalWeb.Controllers
{
    [ApiController]
    [Route("api/translate")]
    public class TranslationController : ControllerBase
    {
        private readonly AzureTranslatorService _translator;

        public TranslationController(AzureTranslatorService translator)
        {
            _translator = translator;
        }

        [HttpPost]
        public async Task<IActionResult> Translate([FromBody] TranslateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Text) || string.IsNullOrWhiteSpace(request.Language))
            {
                return BadRequest("Text and language are required.");
            }

            var translated = await _translator.TranslateAsync(request.Text, request.Language);

            return Ok(new { translated });
        }

        [HttpPost("batch")]
        public async Task<IActionResult> TranslateBatch([FromBody] TranslateBatchRequest request)
        {
            if (request.Texts is null || request.Texts.Count == 0 || string.IsNullOrWhiteSpace(request.Language))
            {
                return BadRequest("Texts and language are required.");
            }

            var translated = await _translator.TranslateBatchAsync(request.Texts, request.Language);
            return Ok(new { translations = translated });
        }
    }

    public class TranslateRequest
    {
        public string Text { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
    }

    public class TranslateBatchRequest
    {
        public List<string> Texts { get; set; } = new();
        public string Language { get; set; } = string.Empty;
    }
}
