using System.ComponentModel;

namespace Demo.Workflow.MessageQueue;

public class RabbitMQSetting
{
    public string? Host { get; set; }
    public ushort Port { get; set; } = 5672;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? ProductName { get; set; }
    public string? Uri { get; set; }

    [Description("RabbitMQ request timeout in seconds")]
    public int TimeoutSeconds { get; set; } = 10;

    public string ConnectionString =>
        string.Format("host={0};persistentMessages=false;" +
                      "virtualHost=/;username={1};password={2};timeout=60;product={3}",
            Uri, Username, Password, ProductName);
}