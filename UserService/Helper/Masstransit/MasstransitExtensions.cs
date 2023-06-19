using Demo.Workflow.MessageQueue;
using MassTransit;

namespace Demo.Services.Helper.Masstransit;

/// <summary>
/// </summary>
public static class MasstransitExtensions
{
    /// <summary>
    /// </summary>
    /// <param name="services"></param>
    public static void AddMasstransitConsumer(this IServiceCollection services, RabbitMQSetting rabbit)
    {
        services.AddMassTransit(cfg =>
        {
            cfg.AddConsumer<AuthenticationConsumer>();

            cfg.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host("" + rabbit.Host, rabbit.Port, "/", h =>
                {
                    h.Username(rabbit.Username);
                    h.Password(rabbit.Password);
                });
                cfg.ConfigureEndpoints(context);
            });
        });
    }
}