using KidsLearning.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace KidsLearning.Api.Controllers;


[ApiController]
[Route("api/speech")]
public sealed class SpeechController : ControllerBase
{
    private readonly ISpeechService _speechService;

    public SpeechController(ISpeechService speechService)
    {
        _speechService = speechService;
    }

    [HttpGet("phoneme/{phoneme}")]
    public async Task<IActionResult> GetPhoneme(
        string phoneme,
        CancellationToken cancellationToken)
    {
        var audio = await _speechService.GetPhonemeAsync(
            phoneme,
            cancellationToken);

        return File(audio, "audio/wav");
    }

    [HttpGet("syllable/{syllable}")]
    public async Task<IActionResult> GetSyllable(
        string syllable,
        CancellationToken cancellationToken)
    {
        var audio = await _speechService.GetSyllableAsync(
            syllable,
            cancellationToken);

        return File(audio, "audio/wav");
    }
}