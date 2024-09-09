using DtekSheduleSendTg.Data.ChatInfo;
using DtekSheduleSendTg.Data.Shedule;
using DtekSheduleSendTg.Data.TextInfo;
using DtekSheduleSendTg.DTEK;
using Microsoft.Extensions.Logging;
using NReco.Logging.File;

namespace DtekSheduleSendTg
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            using ILoggerFactory factory = LoggerFactory.Create(
                    builder => builder
                                    .AddConsole()
                                    .AddFile("app.log", append: true)
                    );
            var logger = factory.CreateLogger("DtekSheduleSendTg");

            logger.LogInformation("-------------------{0}--------------------", DateTime.Now.ToString());

            var botToken = System.Configuration.ConfigurationManager.AppSettings["BotToken"]; 

            var chatInfoRepository = new ChatInfoRepository();
            var sheduleRepository = new SheduleRepository();
            var textInfoRepository = new TextInfoRepository();

            var siteSource = new SiteSource(logger);

            var siteAnalyzer = new SiteAnalyzer(logger, textInfoRepository, siteSource);
            var bot = new TelegramBot(logger, botToken);
            var dtekShedule = new DtekShedule(logger, sheduleRepository);

            var sender = new Sender(logger, siteAnalyzer, bot, dtekShedule, chatInfoRepository);

            sender.CheckAndSend();

            logger.LogInformation("  ");
        }
    }
}