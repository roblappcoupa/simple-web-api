namespace WebApi.Controllers;

using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WebApi.Configuration;
using WebApi.Logging;
using WebApi.Models;
using WebApi.Utils;

[ApiController]
[Route("api/v1/[controller]")]
public class UploadController : ControllerBase
{
    private readonly IOptionsMonitor<ApplicationConfiguration> options;
    private readonly ILogger<UploadController> logger;
    
    public UploadController(
        IOptionsMonitor<ApplicationConfiguration> options,
        ILogger<UploadController> logger)
    {
        this.options = options;
        this.logger = logger;
    }
    
    // [HttpPost("memory-stream")]
    // [DisableRequestSizeLimit]
    // [RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue)]
    // public async Task<IActionResult> UploadUsingMemoryStream(
    //     IFormFile file,
    //     [FromQuery] int? bufferSize,
    //     [FromQuery] int n = 10)
    // {
    //     if (file == null || file.Length == 0)
    //     {
    //         return this.BadRequest("File is empty or not provided.");
    //     }
    //
    //     this.logger.LogMemoryUsage($"Starting {nameof(this.UploadUsingFormFile)}");
    //     
    //     MemoryStream ms = new();
    //     await using var formFileStream = file.OpenReadStream();
    //     this.logger.LogInformation("FormFileStream Length: {Length}", formFileStream.Length);
    //     
    //     if (bufferSize.HasValue)
    //     {
    //         await formFileStream.CopyToAsync(ms, bufferSize: bufferSize.Value);
    //     }
    //     else
    //     {
    //         await formFileStream.CopyToAsync(ms);
    //     }
    //     
    //     ms.Seek(0, SeekOrigin.Begin);
    //
    //     this.logger.LogInformation("MemoryStream Length: {Length}, CurrentPosition: {Position}", ms.Length, ms.Position);
    //     ms.Seek(0, SeekOrigin.Begin);
    //
    //     var result = await ProcessUpload(ms);
    //     
    //     this.logger.LogMemoryUsage($"Completed {nameof(this.UploadUsingFormFile)}");
    //
    //     return this.Ok(new WordCountResult(file.FileName, result.TopN(n)));
    // }

    [HttpPost("form-file")]
    [DisableRequestSizeLimit]
    [RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue)]
    public async Task<IActionResult> UploadUsingFormFile(
        IFormFile file,
        [FromQuery] int? bufferSize,
        [FromQuery] int n = 10)
    {
        if (file == null || file.Length == 0)
        {
            return this.BadRequest("File is empty or not provided.");
        }

        this.logger.LogMemoryUsage($"Starting {nameof(this.UploadUsingFormFile)}. FileSize: {file.Length}");

        await using var stream = file.OpenReadStream();

        var result = await ProcessUpload(stream, bufferSize);
        
        this.logger.LogMemoryUsage($"Completed {nameof(this.UploadUsingFormFile)}");

        return this.Ok(new WordCountResult(file.FileName, result.TopN(n)));
    }
    
    // [HttpPost("buffered-stream")]
    // [DisableRequestSizeLimit]
    // [RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue)]
    // public async Task<IActionResult> UploadUsingStream([FromQuery] int n = 10)
    // {
    //     if (this.Request.ContentLength is null or 0)
    //     {
    //         return this.BadRequest("File is empty or not provided.");
    //     }
    //
    //     this.logger.LogMemoryUsage($"Starting {nameof(this.UploadUsingStream)}");
    //
    //     var fileName = GetFileNameFromContentDisposition(this.Request.Headers.ContentDisposition);
    //
    //     await using var bufferedStream = new BufferedStream(
    //         this.Request.Body,
    //         bufferSize: this.options.CurrentValue.Upload.BufferSize);
    //
    //     var result = await ProcessUpload(bufferedStream);
    //     
    //     this.logger.LogMemoryUsage($"Completed {nameof(this.UploadUsingStream)}");
    //
    //     return this.Ok(new WordCountResult(fileName, result.TopN(n)));
    // }

    private static async Task<Dictionary<string, long>> ProcessUpload(
        Stream stream,
        int? bufferSize)
    {
        Dictionary<string, long> wordCounts = new();

        using var reader = new StreamReader(stream, bufferSize: bufferSize ?? -1);

        while (await reader.ReadLineAsync() is { } line) // Read asynchronously
        {
            var words = ExtractWords(line);

            foreach (var word in words)
            {
                if (!wordCounts.TryAdd(word, 1))
                {
                    wordCounts[word]++;
                }
            }
        }

        return wordCounts;
    }

    private static IEnumerable<string> ExtractWords(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return [];
        }

        return Regex.Split(input.ToLower(), @"\W+")
            .Where(word => !string.IsNullOrWhiteSpace(word))
            .Select(x => x.Trim());
    }
    
    private static string GetFileNameFromContentDisposition(string contentDisposition)
    {
        if (string.IsNullOrWhiteSpace(contentDisposition))
        {
            return null;
        }

        // Parse the Content-Disposition header to find "filename="
        var fileNamePart = contentDisposition.Split(';')
            .FirstOrDefault(part => part.Trim().StartsWith("filename=", StringComparison.OrdinalIgnoreCase));

        // Extract the file name and remove surrounding quotes if present
        var fileName = fileNamePart?.Split('=').Last().Trim().Trim('"');
        return fileName;
    }
}