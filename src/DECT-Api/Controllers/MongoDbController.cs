using System.Diagnostics;
using Camera_DECT.Models;
using DECT_Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace DECT_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MongoDbController(MongoDbService mongo) : ControllerBase
{
    [HttpGet("latest")]
    public async Task<ActionResult<ImageDto>> GetLatest()
    {
        try
        {
            var img = await mongo.GetLatestImageAsync();
            Trace.WriteLine($"Result: {img?.TransmitterId}");
            return img is null ? NotFound() : Ok(img);
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Error: {ex.Message}");
            return StatusCode(500, ex.Message);
        }
    }
    
    [HttpGet("latest/{txId}")]
    public async Task<ActionResult<List<ImageDto>>> GetLatestImageDataByIdAsync(int txId) => Ok(await mongo.GetLatestImageDataByIdAsync(txId));
    
    [HttpGet("all-tx-ids")] 
    public async Task<ActionResult<List<int>>> GetAllTxIds() => Ok(await mongo.GetAllUniqueTransmittersAsync());
}