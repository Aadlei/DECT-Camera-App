using Camera_DECT.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DECT_Api.Services;

public class MongoDbService
{
    private readonly IMongoCollection<BsonDocument> _images;

    public MongoDbService()
    {
        var client = new MongoClient("mongodb://10.225.150.248:27017");
        var database = client.GetDatabase("mesh_network");
        _images = database.GetCollection<BsonDocument>("images");
    }

    // Get the latest image
    public async Task<ImageDto?> GetLatestImageAsync()
    {
        var doc = await _images
            .Find(FilterDefinition<BsonDocument>.Empty)
            .SortByDescending(d => d["timestamp"])
            .FirstOrDefaultAsync();

        if (doc == null) return null;

        return MapToDto(doc);
    }

    // Get all images for a specific transmitter
    public async Task<List<ImageDto>> GetByTransmitterAsync(int txId)
    {
        var filter = Builders<BsonDocument>.Filter.Eq("transmitter_id", txId);
        var docs = await _images
            .Find(filter)
            .SortByDescending(d => d["timestamp"])
            .ToListAsync();

        return docs.Select(MapToDto).ToList();
    }

    // Get the actual image bytes
    public async Task<byte[]?> GetImageDataAsync(string id)
    {
        var filter = Builders<BsonDocument>.Filter.Eq("_id", new ObjectId(id));
        var doc = await _images.Find(filter).FirstOrDefaultAsync();
        return doc?["image_data"].AsByteArray;
    }

    private static ImageDto MapToDto(BsonDocument doc) => new()
    {
        TransmitterId = doc.GetValue("transmitter_id", 0).ToInt32(),
        Image = doc.GetValue("image_data").AsByteArray,
        HopCount = doc.GetValue("hop_count", 0).ToInt32(),
        Timestamp = doc["timestamp"].ToUniversalTime(),
        SizeBytes = doc["size_bytes"].ToInt32(),
    };
}