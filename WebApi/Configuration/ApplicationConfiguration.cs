namespace WebApi.Configuration;

public class ApplicationConfiguration
{
    public ServerConfiguration Server { get; } = new();
    
    public ApiConfiguration Api { get; } = new();
    
    public SqlServerConfiguration SqlServer { get; } = new();
}

public class ServerConfiguration
{
    public TimeSpan? ShutDownTime { get; set; }
}

public class ApiConfiguration
{
    public string ApiBaseUrl { get; set; }
}

public class SqlServerConfiguration
{
    public string ConnectionString { get; set; }
}