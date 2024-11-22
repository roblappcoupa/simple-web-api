namespace WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v1/[controller]")]
public class MemoryController : ControllerBase
{
    private static readonly List<byte[]> StoredBytes = [];
    
    [HttpPost("allocate/{n:long}")]
    public IActionResult Allocate([FromRoute]long n)
    {
        var bytes = new byte[n];
        Random.Shared.NextBytes(bytes);
        
        // Randomly touch some of the data to prevent any optimization
        // that prevents memory from being used
        bytes[Random.Shared.Next(0, bytes.Length)] = 10;

        return this.Ok($"Allocated {n} random bytes");
    }
    
    [HttpPost("leak/{n:long}")]
    public IActionResult Leak([FromRoute]long n)
    {
        var bytes = new byte[n];
        Random.Shared.NextBytes(bytes);
        
        // Randomly touch some of the data to prevent any optimization
        // that prevents memory from being used
        bytes[Random.Shared.Next(0, bytes.Length)] = 10;

        StoredBytes.Add(bytes);

        var total = StoredBytes.Aggregate(0L, (current, storedBytes) => current + storedBytes.Length);

        return this.Ok($"Allocated {n} random bytes. Total stored: {total}");
    }
    
    [HttpPost("free")]
    public IActionResult FreeMemory([FromQuery]bool gc)
    {
        var totalBefore = StoredBytes.Aggregate(0L, (current, storedBytes) => current + storedBytes.Length);

        StoredBytes.Clear();

        if (gc)
        {
            GC.Collect();
        }
        
        var totalAfter = StoredBytes.Aggregate(0L, (current, storedBytes) => current + storedBytes.Length);

        return this.Ok($"Cleared memory. Before: {totalBefore}, After: {totalAfter}");
    }
}