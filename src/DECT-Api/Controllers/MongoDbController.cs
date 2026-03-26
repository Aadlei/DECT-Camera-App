using System.Diagnostics;
using DECT_Api.Services;
using DECT_Shared.Models;
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
    public async Task<ActionResult<List<ImageDto>>> GetLatestImageDataByIdAsync(int txId) => 
        Ok(await mongo.GetLatestImageDataByIdAsync(txId));

    [HttpGet("latest/{txId}/{amount}")]
    public async Task<ActionResult<List<ImageDto>>> GetLatestImageDataByAmountAsync(int amount, int txId) =>
        Ok(await mongo.GetLatestImagesByIdAsync(txId, amount));
    
    [HttpGet("all-tx-ids")] 
    public async Task<ActionResult<List<int>>> GetAllTxIds() => Ok(await mongo.GetAllUniqueTransmittersAsync());
}