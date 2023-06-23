using UserService.Helper.Job;
using UserService.Helper.Telegram;

namespace UserService.Helper.Job
{
    public interface IWeekReportJob
    {
        Task Excute();
    }

    public class WeekReportJob : IWeekReportJob
    {
        private readonly ILogger<WeekReportJob> _logger;
        private readonly ITelegramBotService _teleBot;

        public WeekReportJob(
            ILogger<WeekReportJob> logger,
            ITelegramBotService teleBot
            )
        {
            _logger = logger;
            _teleBot = teleBot;
        }
        
        public Task Excute()
        {
            try
            {
                _teleBot.SendReport("Report.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Week report bad with exception {ExMesg}", ex.Message);
                _teleBot.SendMessage($"Hsm session state exception {ex.Message}");
                return Task.CompletedTask;
            }
            return Task.CompletedTask;
        }
    }
}
