using KidsLearning.Server.Services;
using System.Collections.Concurrent;

namespace KidsLearning.Api.Services;

public sealed class PiperSpeechService : ISpeechService
{
    private readonly WyomingClient _wyoming;
    private readonly string _audioPath;

    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    public PiperSpeechService(
        WyomingClient wyoming,
        IConfiguration configuration)
    {
        _wyoming = wyoming;

        _audioPath = configuration["Speech:AudioPath"]
            ?? throw new InvalidOperationException(
                "Speech:AudioPath is not configured.");

        Directory.CreateDirectory(
            Path.Combine(_audioPath, "phonemes"));

        Directory.CreateDirectory(
            Path.Combine(_audioPath, "syllables"));
    }

    public Task<byte[]> GetPhonemeAsync(
        string phoneme,
        CancellationToken cancellationToken = default)
    {
        return GetAudioAsync(
            "phonemes",
            phoneme,
            cancellationToken);
    }

    public Task<byte[]> GetSyllableAsync(
        string syllable,
        CancellationToken cancellationToken = default)
    {
        return GetAudioAsync(
            "syllables",
            syllable,
            cancellationToken);
    }

    private async Task<byte[]> GetAudioAsync(
        string category,
        string text,
        CancellationToken cancellationToken)
    {
        text = text.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException(
                "Text cannot be empty.",
                nameof(text));
        }

        var fileName = SanitizeFileName(text);

        var directory = Path.Combine(
            _audioPath,
            category);

        var outputPath = Path.Combine(
            directory,
            $"{fileName}.wav");

        // Already cached.
        if (File.Exists(outputPath))
        {
            return await File.ReadAllBytesAsync(
                outputPath,
                cancellationToken);
        }

        var cacheKey = $"{category}:{fileName}";

        var semaphore = _locks.GetOrAdd(
            cacheKey,
            _ => new SemaphoreSlim(1, 1));

        await semaphore.WaitAsync(
            cancellationToken);

        try
        {
            // Another request generated it while we waited.
            if (File.Exists(outputPath))
            {
                return await File.ReadAllBytesAsync(
                    outputPath,
                    cancellationToken);
            }

            var audio = await _wyoming.SynthesizeAsync(
                text,
                cancellationToken);

            var temporaryPath =
                $"{outputPath}.{Guid.NewGuid():N}.tmp";

            try
            {
                await File.WriteAllBytesAsync(
                    temporaryPath,
                    audio,
                    cancellationToken);

                File.Move(
                    temporaryPath,
                    outputPath,
                    overwrite: true);
            }
            finally
            {
                if (File.Exists(temporaryPath))
                {
                    File.Delete(temporaryPath);
                }
            }

            return audio;
        }
        finally
        {
            semaphore.Release();
        }
    }

    private static string SanitizeFileName(
        string value)
    {
        var invalidChars =
            Path.GetInvalidFileNameChars();

        return new string(
                value
                    .Where(c => !invalidChars.Contains(c))
                    .ToArray())
            .Replace(" ", "_");
    }
}