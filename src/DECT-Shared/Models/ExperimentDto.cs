namespace DECT_Shared.Models;

/// <summary>
/// Session-level aggregate metrics for one experiment run.
/// Per-image data lives in <see cref="Entries"/>; the properties on this
/// type are derived rollups intended for the dashboard metric tiles.
/// </summary>
public class ExperimentDto
{
    // ── Overhead model (used by Throughput) ───────────────
    //
    // Throughput = goodput + per-image overhead. We count two overhead
    // sources:
    //
    //   1. The 97-byte wire-format trailer prepended/appended by the edge
    //      PT before transmission (magic + total_length + metadata + CRC,
    //      as defined in uart.h).
    //
    //   2. The MAC/PHY framing tax per chunk on the air. JPEGs are split
    //      into chunks of <see cref="ChunkSizeBytes"/>; each chunk carries
    //      a fixed DECT NR+ MAC header. The number here is a placeholder
    //      until validated against ETSI TS 103 636-4 — see the thesis
    //      Implementation chapter for the exact derivation.
    //
    // These constants should match the firmware. Update both places
    // together when the wire format or chunking strategy changes.
    public const int WireTrailerBytes         = 97;
    public const int ChunkSizeBytes           = 1024;
    public const int MacOverheadPerChunkBytes = 16; // TODO: verify against ETSI TS 103 636-4

    public DateTime  StartTime { get; set; }
    public DateTime? EndTime   { get; set; }

    public List<ImageDto> Entries     { get; } = new();
    public TimeSpan       ElapsedTime => (EndTime ?? DateTime.UtcNow) - StartTime;
    public bool           IsRunning   => EndTime is null;

    public int  TotalImages => Entries.Count;
    public long TotalBytes  => Entries.Sum(e => e.SizeBytes);

    /// <summary>
    /// Total on-air overhead bytes across all received images: the wire-format
    /// trailer plus the per-chunk MAC framing tax. Does not include retransmissions
    /// or other PHY-level costs we don't currently measure.
    /// </summary>
    public long TotalOverheadBytes =>
        Entries.Sum(e => OverheadBytesFor(e.SizeBytes));

    /// <summary>
    /// Goodput: payload bytes successfully decoded at the sink per second.
    /// Excludes wire-format overhead, MAC/PHY headers, and any incomplete
    /// images discarded by the gap-detection logic.
    /// </summary>
    public double GoodputBytesPerSecond =>
        ElapsedTime.TotalSeconds > 0 ? TotalBytes / ElapsedTime.TotalSeconds : 0;

    /// <summary>
    /// Throughput: all bytes pushed onto the radio per second, including
    /// the wire-format trailer and per-chunk MAC framing. Always ≥ goodput.
    /// </summary>
    public double ThroughputBytesPerSecond =>
        ElapsedTime.TotalSeconds > 0 ? (TotalBytes + TotalOverheadBytes) / ElapsedTime.TotalSeconds : 0;

    /// <summary>
    /// Per-image overhead in bytes. Exposed as a static helper so the dashboard's
    /// sliding-window throughput chart can compute it consistently with the
    /// session-aggregate properties above.
    /// </summary>
    public static long OverheadBytesFor(int imageSizeBytes)
    {
        if (imageSizeBytes <= 0) return 0;
        int chunks = (imageSizeBytes + ChunkSizeBytes - 1) / ChunkSizeBytes;
        return WireTrailerBytes + (long)chunks * MacOverheadPerChunkBytes;
    }

    /// <summary>
    /// Average end-to-end latency across all received images, measured from the
    /// edge PT capturing the image to the sink delivering it over UART.
    /// </summary>
    public double AvgEndToEndLatencyMs =>
        Entries.Count > 0 ? Entries.Average(e => e.EndToEndDelayMs) : 0;

    public int DroppedImages { get; set; }
    public int TotalExpected { get; set; }

    /// <summary>
    /// Image loss rate: fraction of images expected (per the firmware sequence
    /// counter) that did not arrive at the sink.
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
                ImageCount  = g.Count(),
                TotalBytes  = g.Sum(e => e.SizeBytes),
                AvgHopCount = (long)g.Average(e => e.HopCount),
            });
}
