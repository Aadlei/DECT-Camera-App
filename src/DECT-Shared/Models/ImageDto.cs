namespace DECT_Shared.Models;

public class ImageDto
{
    public string TransmitterId { get; set; } = "";

    public int HopCount { get; set; }

    public DateTime DeviceTimestamp { get; set; }
    public DateTime ReceivedAt      { get; set; }

    public byte[] Image { get; set; } = Array.Empty<byte>();
    public string? ImageDataUrl =>
        Image.Length > 0 ? $"data:image/jpeg;base64,{Convert.ToBase64String(Image)}" : null;

    public int SizeBytes { get; set; }

    /// <summary>Firmware wire-format seq_num (from edge PT). Used for packet-loss tracking.</summary>
    public int FirmwareSeqNum { get; set; }

    /// <summary>Dashboard arrival order (per-component counter). Used as the X axis on charts.</summary>
    public int SequenceNumber { get; set; }

    public List<string> DevicesVisited  { get; set; } = new();
    public List<long>   PerHopLatencyMs { get; set; } = new();
    public List<int>    PerLinkRssi     { get; set; } = new();

    /// <summary>Sink-side end-to-end delay (last cumulative hop). Index 0 of PerHopLatencyMs is always 0.</summary>
    public long EndToEndDelayMs =>
        PerHopLatencyMs is { Count: > 0 } ? PerHopLatencyMs[^1] : 0;
}
