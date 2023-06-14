using Demo.Workflow.MessageQueue;
using Demo.Workflow.MessageQueue.Request;
using MassTransit;

namespace Demo.Services.AuthenService.Helper
{
    public static class MasstransitStartupHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        public static void AddMasstransitClient(this IServiceCollection services, RabbitMQSetting rabbit)
        {
            services.AddMassTransit(cfg =>
            {
                cfg.UsingRabbitMq((context, cfg1) =>
                {
                    cfg1.Host("" + rabbit.Host, rabbit.Port, "/", h =>
                    {
                        h.Username(rabbit.Username);
                        h.Password(rabbit.Password);
                    });
                    cfg1.ConfigureEndpoints(context);
                });
                cfg.AddRequestClient<AuthenRequest>(TimeSpan.FromSeconds(rabbit.TimeoutSeconds));
            }
            );
        }
    }
}
