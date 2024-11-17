namespace FileGenerator;

public class ApplicationConfiguration
{
    public long TargetSizeInMegaBytes { get; set; }
    
    public int ChunkSizeInBytes { get; set; }
    
    public string FilePath { get; set; }
    
}