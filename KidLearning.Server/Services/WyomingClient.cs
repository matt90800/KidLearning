using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace KidLearning.Server.Services;

public sealed class WyomingClient
{
    private readonly string _host;
    private readonly int _port;

    public WyomingClient(IConfiguration configuration)
    {
        _host = configuration["Speech:PiperHost"] ?? "kidslearning-piper";
        _port = configuration.GetValue("Speech:PiperPort", 10200);
    }

    public async Task<byte[]> SynthesizeAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        using var client = new TcpClient();

        await client.ConnectAsync(
            _host,
            _port,
            cancellationToken);

        await using var stream = client.GetStream();

        var audioRate = 22050;
        var audioWidth = 2;
        var audioChannels = 1;

        // Wyoming synthesize event
        var synthesize = new
        {
            type = "synthesize",
            data = new
            {
                text
            }
        };

        var json = JsonSerializer.Serialize(synthesize);

        Console.WriteLine($"WYOMING SEND: {json}");

        var message = Encoding.UTF8.GetBytes(json + "\n");

        await stream.WriteAsync(
            message,
            cancellationToken);

        await stream.FlushAsync(cancellationToken);

        using var audio = new MemoryStream();

        while (true)
        {
            var headerLine = await ReadLineAsync(
                stream,
                cancellationToken);

            if (headerLine is null)
                break;

            Console.WriteLine($"WYOMING RECV: {headerLine}");

            using var document = JsonDocument.Parse(headerLine);
            var root = document.RootElement;

            var type = root.GetProperty("type").GetString();

            var payloadLength =
                root.TryGetProperty(
                    "payload_length",
                    out var payloadProperty)
                    ? payloadProperty.GetInt32()
                    : 0;

            var dataLength =
                root.TryGetProperty(
                    "data_length",
                    out var dataProperty)
                    ? dataProperty.GetInt32()
                    : 0;

            string? data = null;

            // Read JSON data following the header
            if (dataLength > 0)
            {
                var dataBytes = await ReadExactlyAsync(
                    stream,
                    dataLength,
                    cancellationToken);

                data = Encoding.UTF8.GetString(dataBytes);

                Console.WriteLine($"WYOMING DATA: {data}");
            }

            // Read raw payload
            if (payloadLength > 0)
            {
                var payload = await ReadExactlyAsync(
                    stream,
                    payloadLength,
                    cancellationToken);

                if (type == "audio-chunk")
                {
                    // Piper sends audio parameters with every audio chunk
                    if (data is not null)
                    {
                        using var chunkDocument =
                            JsonDocument.Parse(data);

                        var chunk =
                            chunkDocument.RootElement;

                        audioRate =
                            chunk.GetProperty("rate").GetInt32();

                        audioWidth =
                            chunk.GetProperty("width").GetInt32();

                        audioChannels =
                            chunk.GetProperty("channels").GetInt32();
                    }

                    await audio.WriteAsync(
                        payload,
                        cancellationToken);
                }
            }

            if (type == "audio-stop")
            {
                Console.WriteLine(
                    $"Generated WAV: {audio.Length} bytes, " +
                    $"{audioRate} Hz, " +
                    $"{audioWidth} bytes/sample, " +
                    $"{audioChannels} channel(s)");

                return CreateWav(
                    audio.ToArray(),
                    audioRate,
                    audioWidth,
                    audioChannels);
            }

            if (type == "error")
            {
                throw new InvalidOperationException(
                    $"Piper returned an error: {headerLine}");
            }
        }

        throw new InvalidOperationException(
            "Piper closed the connection before returning audio.");
    }

    private static async Task<string?> ReadLineAsync(
        NetworkStream stream,
        CancellationToken cancellationToken)
    {
        using var buffer = new MemoryStream();

        var oneByte = new byte[1];

        while (true)
        {
            var read = await stream.ReadAsync(
                oneByte,
                cancellationToken);

            if (read == 0)
            {
                if (buffer.Length == 0)
                    return null;

                break;
            }

            if (oneByte[0] == '\n')
                break;

            buffer.WriteByte(oneByte[0]);
        }

        return Encoding.UTF8.GetString(
            buffer.ToArray());
    }

    private static async Task<byte[]> ReadExactlyAsync(
        NetworkStream stream,
        int length,
        CancellationToken cancellationToken)
    {
        var buffer = new byte[length];
        var offset = 0;

        while (offset < length)
        {
            var read = await stream.ReadAsync(
                buffer.AsMemory(
                    offset,
                    length - offset),
                cancellationToken);

            if (read == 0)
            {
                throw new IOException(
                    "Piper closed the connection while sending data.");
            }

            offset += read;
        }

        return buffer;
    }

    private static byte[] CreateWav(
        byte[] pcm,
        int sampleRate,
        int bytesPerSample,
        int channels)
    {
        using var output = new MemoryStream();
        using var writer = new BinaryWriter(output);

        var blockAlign =
            channels * bytesPerSample;

        var byteRate =
            sampleRate * blockAlign;

        // RIFF header
        writer.Write(
            Encoding.ASCII.GetBytes("RIFF"));

        writer.Write(
            36 + pcm.Length);

        writer.Write(
            Encoding.ASCII.GetBytes("WAVE"));

        // fmt chunk
        writer.Write(
            Encoding.ASCII.GetBytes("fmt "));

        writer.Write(16); // PCM chunk size

        writer.Write((short)1); // PCM format

        writer.Write(
            (short)channels);

        writer.Write(
            sampleRate);

        writer.Write(
            byteRate);

        writer.Write(
            (short)blockAlign);

        writer.Write(
            (short)(bytesPerSample * 8));

        // data chunk
        writer.Write(
            Encoding.ASCII.GetBytes("data"));

        writer.Write(
            pcm.Length);

        writer.Write(pcm);

        writer.Flush();

        return output.ToArray();
    }
}