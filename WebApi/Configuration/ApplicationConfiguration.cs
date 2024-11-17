namespace WebApi.Configuration;

public class ApplicationConfiguration
{
    public ServerConfiguration Server { get; } = new();
    
    public UploadConfiguration Upload { get; } = new();
    
    public ApiConfiguration Api { get; } = new();
    
    public SqlServerConfiguration SqlServer { get; } = new();
}

public class ServerConfiguration
{
    public TimeSpan? ShutDownTime { get; set; }
}

public class UploadConfiguration
{
    public int BufferSize { get; set; } = 4 * 1024; // 1 KB
}

public class ApiConfiguration
{
    public string ApiBaseUrl { get; set; }
}

public class SqlServerConfiguration
{
    public string ConnectionString { get; set; }
}