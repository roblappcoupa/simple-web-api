namespace WebApi.Models;

public class WordCountResult
{
    public WordCountResult(
        string fileName,
        List<WordCount> counts = null)
    {
        this.FileName = fileName;
        this.Counts = counts == null
            ? []
            : [..counts];
    }

    public string FileName { get; }

    public List<WordCount> Counts { get; }
}

public class WordCount
{
    public WordCount(
        string word,
        long frequency)
    {
        this.Word = word;
        this.Frequency = frequency;
    }

    public string Word { get; }
    
    public long Frequency { get; }
}