namespace Camera_DECT.Models;

public class ImageDto
{
    public int TransmitterId { get; set; }
    public int HopCount { get; set; }
    public DateTime Timestamp { get; set; }
    public byte[] Image { get; set; }
    public string? ImageDataUrl => $"data:image/jpeg;base64,{Convert.ToBase64String(Image)}";
    public int ImageCount { get; set; }
    public int SizeBytes { get; set; }
}