namespace Demo.Workflow.MessageQueue.Message;

public record UserAuthenticate
{
    public Guid MessageId { get; init; }
}