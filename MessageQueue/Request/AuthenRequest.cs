namespace Demo.Workflow.MessageQueue.Request;

public class AuthenRequest
{
    public string? RequestId { get; set; }
    public string? ClientId { get; set; }
    public ClientIP? ClientIP { get; set; }

    public UserPass? UserPass { get; set; }
}

public class UserPass
{
    public string? Username { get; set; }
    public string? Password { get; set; }
}

public class ClientIP
{
    public string? IP { get; set; }
}