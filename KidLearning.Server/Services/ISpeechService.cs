namespace KidsLearning.Server.Services;

public interface ISpeechService
{
    Task<byte[]> GetPhonemeAsync(
        string phoneme,
        CancellationToken cancellationToken = default);

    Task<byte[]> GetSyllableAsync(
        string syllable,
        CancellationToken cancellationToken = default);
}