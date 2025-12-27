using Microsoft.AspNetCore.Mvc;

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
        var translated = await _translator.TranslateAsync(
            request.Text,
            request.Language
        );

        return Ok(new { translated });
    }
}

public class TranslateRequest
{
    public string Text { get; set; }
    public string Language { get; set; }
}
