namespace DECT_Shared.Models;

/// <summary>
/// DTO used for a session of experiment.
/// </summary>
public class ExperimentDto
{
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }

    public List<ImageDto> Entries { get; } = new();
    public TimeSpan ElapsedTime => (EndTime ?? DateTime.UtcNow) - StartTime;
    public bool IsRunning => EndTime is null;

    public int TotalImages => Entries.Count;
    public long TotalBytes => Entries.Sum(e => e.SizeBytes);

    /// <summary>
    /// Goodput: successfully decoded JPEG image bytes delivered to the sink per second,
    /// excluding DECT NR+ MAC headers, the wire-format envelope, SPI framing, and any
    /// incomplete images discarded by the gap-detection logic.
    /// </summary>
    public double GoodputBytesPerSecond =>
        ElapsedTime.TotalSeconds > 0 ? TotalBytes / ElapsedTime.TotalSeconds : 0;

    /// <summary>
    /// Average end-to-end latency across all received images, measured from the edge PT
    /// capturing the image to the sink delivering it over UART.
    /// </summary>
    public double AvgEndToEndLatencyMs =>
        Entries.Count > 0 ? Entries.Average(e => e.EndToEndDelayMs) : 0;

    public int DroppedImages { get; set; }
    public int TotalExpected { get; set; }

    /// <summary>
    /// Image loss rate: fraction of images expected (per the firmware sequence counter)
    /// that did not arrive at the sink.
    /// </summary>
    public double ImageLossPercent =>
        TotalExpected > 0 ? DroppedImages * 100.0 / TotalExpected : 0;

    /// <summary>
    /// Average RSSI across all links and all images, excluding zero (unset) values.
    /// </summary>
    public double AvgRssiDbm
    {
        get
        {
            var allRssi = Entries
                .SelectMany(e => e.PerLinkRssi)
                .Where(r => r != 0)
                .ToList();
            return allRssi.Count > 0 ? allRssi.Average() : 0;
        }
    }

    public double AvgHopCount =>
        Entries.Count > 0 ? Entries.Average(e => e.HopCount) : 0;

    public Dictionary<string, TransmitterStatsDto> PerTransmitter =>
        Entries
            .GroupBy(e => e.TransmitterId)
            .ToDictionary(g => g.Key, g => new TransmitterStatsDto
            {
                ImageCount = g.Count(),
                TotalBytes = g.Sum(e => e.SizeBytes),
                AvgHopCount = (long)g.Average(e => e.HopCount),
            });
}