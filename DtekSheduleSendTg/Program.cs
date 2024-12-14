﻿using DtekSheduleSendTg.Data.ChatInfo;
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