using System.Text;
using System.Text.Json;
using Bogus;
using FileGenerator;
using Microsoft.Extensions.Configuration;

IConfiguration configurationRoot = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true)
    .AddCommandLine(args)
    .Build();

ApplicationConfiguration config = new();
configurationRoot.Bind(config);

var targetSizeInBytes = config.TargetSizeInMegaBytes * 1024 * 1024;

Console.WriteLine(
    "Creating a file with random text with the following settings:\n{0}",
    JsonSerializer.Serialize(config));

if (config.ChunkSizeInBytes > targetSizeInBytes)
{
    throw new Exception(
        $"Invalid configuration. {nameof(config.ChunkSizeInBytes)} > {nameof(config.TargetSizeInMegaBytes)}");
}

Faker faker = new();

long currentSize = 0;
var totalChunks = 0;

using (var fs = new FileStream(config.FilePath, FileMode.Create, FileAccess.Write))
{
    while (currentSize < targetSizeInBytes)
    {
        var randomText = GenerateRandomText(faker, config.ChunkSizeInBytes);

        var data = Encoding.UTF8.GetBytes(randomText);

        fs.Write(data, 0, data.Length);
        currentSize += data.Length;

        totalChunks++;
        
        Console.WriteLine("Completed chunk {0}. Total size so far: {1} bytes", totalChunks, currentSize);
    }
}

Console.WriteLine(
    "File created. Path: {0}, Final size: {1} bytes",
    config.FilePath,
    currentSize);

return;

static string GenerateRandomText(Faker faker, int chunkSizeInBytes)
{
    var randomText = new StringBuilder();

    while (randomText.Length < chunkSizeInBytes)
    {
        randomText.AppendLine(faker.Lorem.Sentence());
    }

    randomText.Length = chunkSizeInBytes;

    return randomText.ToString();
}