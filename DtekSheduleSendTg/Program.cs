﻿using DtekSheduleSendTg.Data.ChatInfo;
using DtekSheduleSendTg.Data.Shedule;
using DtekSheduleSendTg.Data.TextInfo;
using DtekSheduleSendTg.DTEK;
using Microsoft.Extensions.Logging;
using NReco.Logging.File;

namespace DtekSheduleSendTg
{
    public static class Program
    {
        private static ILogger logger = LoggerFactory
                                            .Create(builder => builder
                                                            .AddConsole()
                                                            .AddFile("app.log", append: true)
                                            ).CreateLogger("DtekSheduleSendTg");
        static Program()
            => AppDomain.CurrentDomain.UnhandledException += UnhandledException;

        private static void Main(string[] args)
        {
            logger.LogInformation("-------------------{0}--------------------", DateTime.Now.ToString());

            var botToken = System.Configuration.ConfigurationManager.AppSettings["BotToken"]; 
            var site = System.Configuration.ConfigurationManager.AppSettings["Site"];
            var shedilePicRegex = System.Configuration.ConfigurationManager.AppSettings["ShedilePicRegex"]; 

            var chatInfoRepository = new ChatInfoRepository();
            var sheduleRepository = new SheduleRepository();
            var textInfoRepository = new TextInfoRepository();

            var siteSource = new SiteSource(logger, site);

            var siteAnalyzer = new SiteAnalyzer(logger, textInfoRepository, siteSource, shedilePicRegex);
            var bot = new TelegramBot(logger, botToken);
            var dtekShedule = new DtekShedule(logger, sheduleRepository);

            var sender = new Sender(logger, siteAnalyzer, bot, dtekShedule, chatInfoRepository);

            sender.CheckAndSend();

            logger.LogInformation("  ");
        }

        private static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
             => logger.LogError("UnhandledException : {0}", e);
    }
}