using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using Camera_DECT.Models;

namespace DECT_Web.Services;

public class MongoDbServices(HttpClient client)
{
    public async Task<ImageDto> GetLatestImageData()
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
}