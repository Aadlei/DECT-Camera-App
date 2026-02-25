using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using Camera_DECT.Models;

namespace DECT_Web.Services;

public class MongoDbServices(HttpClient client)
{
    /// <summary>
    /// For testing purposes.
    /// </summary>
    /// <returns></returns>
    public async Task<ImageDto> GetLatestImageDataAsync()
    {
        try
        {
            return await client.GetFromJsonAsync<ImageDto>("api/mongodb/latest") ?? new();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        return new();
    }

    /// <summary>
    /// Get image data by id.
    /// </summary>
    /// <param name="txId"></param>
    /// <returns></returns>
    public async Task<ImageDto> GetLatestImageDataByIdAsync(int txId)
    {
        try
        {
            return await client.GetFromJsonAsync<ImageDto>($"api/mongodb/latest/{txId}") ?? new();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        return new();
    }

    /// <summary>
    /// Returns all transmitter ids that are unique.
    /// </summary>
    /// <returns></returns>
    public async Task<List<int>> GetAllTransmittersAsync()
    {
        try
        {
            return await client.GetFromJsonAsync<List<int>>("api/mongodb/all-tx-ids") ?? new();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        return new();
    }
}