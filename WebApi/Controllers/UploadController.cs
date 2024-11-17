namespace WebApi.Controllers;

using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WebApi.Configuration;
using WebApi.Models;

[ApiController]
[Route("api/v1/[controller]")]
public class UploadController : ControllerBase
{
    private readonly IOptionsMonitor<ApplicationConfiguration> options;
    private readonly ILogger<UploadController> logger;
    
    [HttpPost("in-memory")]
    public async Task<IActionResult> UploadUsingFormFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return this.BadRequest("File is empty or not provided.");
        }

        WordCountResult wordCounts = new(file.Name);

        using (var reader = new StreamReader(file.OpenReadStream()))
        {
            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                var words = ExtractWords(line);
                
                foreach (var word in words)
                {
                    if (!wordCounts.Counts.TryAdd(word, 1))
                    {
                        wordCounts.Counts[word]++;
                    }
                }
            }
        }

        return this.Ok(wordCounts);
    }
    
    [HttpPost("stream")]
    public async Task<IActionResult> UploadUsingStream()
    {
        if (this.Request.ContentLength == null || this.Request.ContentLength == 0)
        {
            return this.BadRequest("File is empty or not provided.");
        }

        var fileName = GetFileNameFromContentDisposition(this.Request.Headers.ContentDisposition);
        
        await using (var bufferedStream = new BufferedStream(
                         this.Request.Body,
                         bufferSize: this.options.CurrentValue.Upload.BufferSize))
        {
            var result = await ProcessUpload(fileName, bufferedStream);

            return this.Ok(result);
        }
    }

    private static async Task<WordCountResult> ProcessUpload(
        string fileName,
        Stream stream)
    {
        WordCountResult wordCounts = new(fileName);

        using var reader = new StreamReader(stream);
        
        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            var words = ExtractWords(line);
                
            foreach (var word in words)
            {
                if (!wordCounts.Counts.TryAdd(word, 1))
                {
                    wordCounts.Counts[word]++;
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
            .Where(word => !string.IsNullOrWhiteSpace(word));
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