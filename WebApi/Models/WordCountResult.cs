namespace WebApi.Models;

public class WordCountResult
{
    public WordCountResult(
        string fileName,
        Dictionary<string, int> counts = null)
    {
        this.FileName = fileName;
        this.Counts = counts == null
            ? new Dictionary<string, int>()
            : new Dictionary<string, int>(counts);
    }

    public string FileName { get; }

    public Dictionary<string, int> Counts { get; }
}