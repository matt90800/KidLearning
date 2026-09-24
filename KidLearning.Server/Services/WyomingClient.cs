using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace KidsLearning.Api.Services;

public sealed class WyomingClient
{
    private readonly string _host;
    private readonly int _port;

    public WyomingClient(
        IConfiguration configuration)
    {
        _host = configuration["Speech:PiperHost"] ?? "piper";

        _port = configuration.GetValue(
            "Speech:PiperPort",
            10200);
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

        // Tell Piper what client we are.
        await SendEventAsync(
            stream,
            "describe",
            new
            {
                text = "KidsLearning",
                version = "1.0"
            },
            cancellationToken);

        // Ask Piper to synthesize the text.
        await SendEventAsync(
            stream,
            "synthesize",
            new
            {
                text
            },
            cancellationToken);

        using var audio = new MemoryStream();

        while (true)
        {
            var @event = await ReadEventAsync(
                stream,
                cancellationToken);

            if (@event is null)
                break;

            switch (@event.Value.Type)
            {
                case "info":
                    // Piper can send information about itself.
                    break;

                case "audio-start":
                    break;

                case "audio-chunk":
                    if (@event.Value.Payload.Length > 0)
                    {
                        await audio.WriteAsync(
                            @event.Value.Payload,
                            cancellationToken);
                    }

                    break;

                case "audio-stop":
                    return audio.ToArray();

                case "error":
                    throw new InvalidOperationException(
                        $"Piper error: {@event.Value.Data}");

                default:
                    // Ignore events we don't need.
                    break;
            }
        }

        throw new InvalidOperationException(
            "Piper closed the connection before returning audio.");
    }

    private static async Task SendEventAsync(
        NetworkStream stream,
        string type,
        object data,
        CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(data);
        var jsonBytes = Encoding.UTF8.GetBytes(json);

        var header =
            $"{type}\n" +
            $"{jsonBytes.Length}\n" +
            "\n";

        var headerBytes = Encoding.UTF8.GetBytes(header);

        await stream.WriteAsync(
            headerBytes,
            cancellationToken);

        await stream.WriteAsync(
            jsonBytes,
            cancellationToken);

        await stream.FlushAsync(
            cancellationToken);
    }

    private static async Task<WyomingEvent?> ReadEventAsync(
        NetworkStream stream,
        CancellationToken cancellationToken)
    {
        var type = await ReadLineAsync(
            stream,
            cancellationToken);

        if (type is null)
            return null;

        var dataLengthLine = await ReadLineAsync(
            stream,
            cancellationToken);

        if (dataLengthLine is null)
            throw new InvalidOperationException(
                "Invalid Wyoming event: missing data length.");

        var payloadLengthLine = await ReadLineAsync(
            stream,
            cancellationToken);

        if (payloadLengthLine is null)
            throw new InvalidOperationException(
                "Invalid Wyoming event: missing payload length.");

        // Empty line separating headers from data.
        var separator = await ReadLineAsync(
            stream,
            cancellationToken);

        if (separator is null)
            throw new InvalidOperationException(
                "Invalid Wyoming event: missing header separator.");

        if (!int.TryParse(
                dataLengthLine,
                out var dataLength))
        {
            throw new InvalidOperationException(
                $"Invalid Wyoming data length: {dataLengthLine}");
        }

        if (!int.TryParse(
                payloadLengthLine,
                out var payloadLength))
        {
            throw new InvalidOperationException(
                $"Invalid Wyoming payload length: {payloadLengthLine}");
        }

        var dataBytes = await ReadExactlyAsync(
            stream,
            dataLength,
            cancellationToken);

        var payload = await ReadExactlyAsync(
            stream,
            payloadLength,
            cancellationToken);

        var data = Encoding.UTF8.GetString(
            dataBytes);

        return new WyomingEvent(
            type,
            data,
            payload);
    }

    private static async Task<string?> ReadLineAsync(
        NetworkStream stream,
        CancellationToken cancellationToken)
    {
        var buffer = new List<byte>();

        while (true)
        {
            var value = new byte[1];

            var read = await stream.ReadAsync(
                value,
                cancellationToken);

            if (read == 0)
            {
                if (buffer.Count == 0)
                    return null;

                throw new EndOfStreamException(
                    "Unexpected end of Wyoming stream.");
            }

            if (value[0] == '\n')
                return Encoding.UTF8.GetString(
                    buffer.ToArray());

            if (value[0] != '\r')
                buffer.Add(value[0]);
        }
    }

    private static async Task<byte[]> ReadExactlyAsync(
        NetworkStream stream,
        int length,
        CancellationToken cancellationToken)
    {
        if (length == 0)
            return [];

        var buffer = new byte[length];

        var offset = 0;

        while (offset < length)
        {
            var read = await stream.ReadAsync(
                buffer.AsMemory(offset, length - offset),
                cancellationToken);

            if (read == 0)
            {
                throw new EndOfStreamException(
                    "Unexpected end of Wyoming payload.");
            }

            offset += read;
        }

        return buffer;
    }

    private readonly record struct WyomingEvent(
        string Type,
        string Data,
        byte[] Payload);
}