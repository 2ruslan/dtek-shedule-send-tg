using DtekSheduleSendTg.Data.ChatInfo;
using DtekSheduleSendTg.Data.TextInfo;
using DtekSheduleSendTg.Data.WotkInfo;
using DtekSheduleSendTg.DTEK;
using Microsoft.Extensions.Logging;
using NReco.Logging.File;
using System.Configuration;

namespace DtekSheduleSendTg
{
    public static class Program
    {
        private static ILogger logger = LoggerFactory
                                            .Create(builder => builder
                                                            .AddConsole()
                                                            .AddFile("app.log"
                                                            , fileLoggerOpts =>
                                                            {
                                                                fileLoggerOpts.Append = true;
                                                                fileLoggerOpts.MaxRollingFiles = 10;
                                                                fileLoggerOpts.FileSizeLimitBytes = 500 * 1024;
                                                            }
                                                )
                                            ).CreateLogger("DST");
        static Program()
            => AppDomain.CurrentDomain.UnhandledException += UnhandledException;

        private static async Task Main(string[] args)
        {
            logger.LogInformation("-------------------{0}--------------------", DateTime.Now.ToString());

            if (args.Length == 0)
            {
                logger.LogInformation("Start without region. Exit.");
                return;
            }
            
            var region = args[0];

            logger.LogInformation("-------------------{0}--------------------", region);

            // global config value
            var botToken = ConfigurationManager.AppSettings["BotToken"];

            var statisticsChanel = long.Parse(ConfigurationManager.AppSettings["StatisticsGroup"]);

            // region config value
            var site = GetRegionConfigValue("Site", region);
            var shedilePicRegex = GetRegionConfigValue("SchedulePicRegex", region); 

            var chatInfoRepository = new ChatInfoRepositoryApp(region);
            var textInfoRepository = new TextInfoRepository(region);
            var workInfoRepository = new WorkInfoRepository(region);
            var scheduleWeekRepository = new ScheduleWeekRepository(region);

            var siteSource = new SiteSource(logger, site, region);

            var siteAnalyzer = new SiteAnalyzer(logger, textInfoRepository, siteSource, shedilePicRegex);
            

            var bot = new TelegramBot(logger, botToken);
            var dtekShedule = new DtekShedule(logger);

            /*
            dtekShedule.AnalyzeFile(@"c:\temp\page-chart-9107-1050.png");
            Console.WriteLine(dtekShedule.GetSchedule("1.1"));
            Console.WriteLine(dtekShedule.GetFullPictureDescription("1.1", "uhf {d}", string.Empty, " "));
            Console.WriteLine(dtekShedule.GetSchedule("1.2"));
            Console.WriteLine(dtekShedule.GetFullPictureDescription("1.2", "uhf {d}", string.Empty, " "));
            Console.WriteLine(dtekShedule.GetSchedule("2.1"));
            Console.WriteLine(dtekShedule.GetFullPictureDescription("2.1", "uhf {d}", string.Empty, " "));
            Console.WriteLine(dtekShedule.GetSchedule("2.2"));
            Console.WriteLine(dtekShedule.GetFullPictureDescription("2.2", "uhf {d}", string.Empty, " "));
            Console.WriteLine(dtekShedule.GetSchedule("3.1"));
            Console.WriteLine(dtekShedule.GetFullPictureDescription("3.1", "uhf {d}", string.Empty, " "));
            Console.WriteLine(dtekShedule.GetSchedule("3.2"));
            Console.WriteLine(dtekShedule.GetFullPictureDescription("3.2", "uhf {d}", string.Empty, " "));
            Console.WriteLine(dtekShedule.GetSchedule("5.1"));
            Console.WriteLine(dtekShedule.GetFullPictureDescription("5.1", "uhf {d}", string.Empty, " "));
            return;
            */
            var monitoring = new Monitoring2Txt($"  --=  {region}  =--  ");

            var sender = new Sender(logger, 
                                    siteAnalyzer, 
                                    bot, 
                                    dtekShedule, 
                                    chatInfoRepository, 
                                    workInfoRepository, 
                                    scheduleWeekRepository,
                                    monitoring
                                    );

            await sender.CheckAndSend();

            await bot.SendText(statisticsChanel, monitoring.GetInfo());

            logger.LogInformation("  ");
        }

        private static string GetRegionConfigValue(string key, string region)
            => ConfigurationManager.AppSettings[$"{region}.{key}"] ?? ConfigurationManager.AppSettings[key];
        
        private static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
             => logger.LogError("UnhandledException : {0}", e);
    }
}