namespace Demo.MessageQueue.Response;

public class AuthenResponse
{
    public string? RequestId { get; set; }
    public string? AuthType { get; set; } = "OAuth2Authentication";
    public bool IsAuthenticated { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerId { get; set; }

    public string? FailureInfo { get; set; }
    public string? ServicePack { get; set; }
}