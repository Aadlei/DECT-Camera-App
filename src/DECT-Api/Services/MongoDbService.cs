using DECT_Shared.Models;
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
        
        _images.Indexes.CreateOne(new CreateIndexModel<BsonDocument>(
            Builders<BsonDocument>.IndexKeys
                .Ascending("transmitter_id")
                .Descending("timestamp")));
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

    public async Task<ImageDto?> GetLatestImageDataByIdAsync(string txId)
    {
        var filter = Builders<BsonDocument>.Filter.Eq("transmitter_id", txId);
        var doc = await _images
            .Find(filter)
            .SortByDescending(d => d["timestamp"])
            .FirstOrDefaultAsync();
        return doc is null ? null : MapToDto(doc);
    }

    public async Task<List<ImageDto>> GetLatestImagesByIdAsync(string txId, int amount)
    {
        var filter = Builders<BsonDocument>.Filter.Eq("transmitter_id", txId);
        var doc = await _images
            .Find(filter)
            .SortByDescending(d => d["timestamp"])
            .Limit(amount)
            .ToListAsync();
        return doc.Select(MapToDto).ToList();
    }

    // Get all unique transmitters, for example 1337, 4567..
    public async Task<List<string>> GetAllUniqueTransmittersAsync()
    {
        var filter = Builders<BsonDocument>.Filter.And(
            Builders<BsonDocument>.Filter.Exists("transmitter_id"),
            Builders<BsonDocument>.Filter.Ne("transmitter_id", BsonNull.Value)
        );

        var result = await _images.DistinctAsync<string>("transmitter_id", filter);
        return await result.ToListAsync();
    }

    // Get all images for a specific transmitter
    public async Task<List<ImageDto>> GetByTransmitterAsync(string txId)
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
        TransmitterId  = doc.GetValue("transmitter_id", "unknown").AsString,
        Image          = doc.GetValue("image_data").AsByteArray,
        HopCount       = doc.GetValue("num_links", 0).ToInt32(),
        DeviceTimestamp = doc["timestamp"].ToUniversalTime(),
        SizeBytes      = doc["size_bytes"].ToInt32(),
        SequenceNumber = 0,
        FirmwareSeqNum = doc["seq_num"].ToInt32(),

        DevicesVisited = doc.Contains("devices_visited")
            ? doc["devices_visited"].AsBsonArray
                .Select(v => $"0x{v.ToInt64():x8}")
                .ToList()
            : new List<string>(),

        PerHopLatencyMs = doc.Contains("per_link_delay")
            ? doc["per_link_delay"].AsBsonArray
                .Select(v => v.ToInt64())
                .ToList()
            : new List<long>(),
        
        PerLinkRssi = doc.Contains("per_link_rssi")
            ? doc["per_link_rssi"].AsBsonArray
                .Select(v => v.ToInt32())
                .ToList()
            : new List<int>(),

    };
}