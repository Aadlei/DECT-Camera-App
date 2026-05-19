namespace DECT_Shared.Models;

public class ImageDto
{
    public string TransmitterId { get; set; }
    public int HopCount { get; set; }
    public DateTime DeviceTimestamp { get; set; }
    public DateTime ReceivedAt { get; set; }
    public byte[] Image { get; set; }
    public string? ImageDataUrl => $"data:image/jpeg;base64,{Convert.ToBase64String(Image)}";
    public int ImageCount { get; set; }
    /// <summary>Firmware wire-format seq_num (from edge PT). Used for packet loss.</summary>
    public int FirmwareSeqNum { get; set; }
    
    /// <summary>Dashboard arrival order (per-component counter). Used for chart X axes.</summary>
    public int SequenceNumber { get; set; }
    public int SizeBytes { get; set; }
    public List<string> DevicesVisited { get; set; } = new();
    public List<long>   PerHopLatencyMs { get; set; } = new();
    public long EndToEndDelayMs => 
        PerHopLatencyMs is { Count: > 0 } ? PerHopLatencyMs[^1] : 0; // index 0 is always 0
    public List<int> PerLinkRssi { get; set; } = new();
}