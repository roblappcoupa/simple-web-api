namespace WebApi;

public class PublishMessageRequest
{
    public string Exchange { get; set; }
    
    public object Message { get; set; }
}

public class MessageWrapper<TBody>
    where TBody : class
{
    public MessageWrapper(TBody body)
    {
        this.Body = body;
    }

    public TBody Body { get; }

    public string CorrelationId { get; set; }

    public string Culture { get; set; } = "en-US";

    public IDictionary<string, string> Headers { get; } = new Dictionary<string, string>();

    public int MaxRetryCount { get; set; } = 0;

    public int RetryCount { get; set; } = 0;

    public string UiCulture { get; set; } = "en-US";

    public string UserClaims { get; set; }
}