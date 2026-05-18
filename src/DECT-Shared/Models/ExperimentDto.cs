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
    
    public double ThroughputBytesPerSecond => ElapsedTime.TotalSeconds > 0 ? TotalBytes / ElapsedTime.TotalSeconds : 0;
    public double AvgEndToEndDelayMs  => Entries.Count > 0 ? Entries.Average(e => e.EndToEndDelayMs) : 0;
    public int DroppedPackets { get; set; }
    public int TotalExpected  { get; set; }
    public double PacketLossPercent => TotalExpected > 0 ? DroppedPackets * 100.0 / TotalExpected : 0;
    /// <summary>
    /// Average inter-arrival across all hops 
    /// </summary>
    public double AvgInterArrivalMs
    {
        get
        {
            if (Entries.Count < 2) return 0;
            var ordered = Entries.OrderBy(e => e.DeviceTimestamp).ToList();
            var deltas = ordered
                .Zip(ordered.Skip(1), (a, b) => (b.DeviceTimestamp - a.DeviceTimestamp).TotalMilliseconds)
                .ToList();
            return deltas.Count > 0 ? deltas.Average() : 0;
        }
    }
    
    
    /// <summary>
    /// Average RSSI across all hops and all images, excluding zero (unset) values.
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