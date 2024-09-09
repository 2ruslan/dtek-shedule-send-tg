using DtekSheduleSendTg.Abstraction;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DtekSheduleSendTg
{
    public class TelegramBot(ILogger logger, string botToken) : ITelegramBot
    {
        private const int WAIT_BEFORE_SEND_TEXT = 1;
        private const int WAIT_BEFORE_SEND_PICTURE = 3;

        private readonly TelegramBotClient bot = new TelegramBotClient(botToken);

        private readonly Dictionary<string, string> fileInfo= new();

        public void SendText(long chatId, string message)
        {
            logger.LogInformation("TelegramBot start SendText");

            try
            {
                logger.LogInformation("Try Send {0} to {1}", message, chatId);

                Thread.Sleep(WAIT_BEFORE_SEND_TEXT * 1000);

                var result = bot.SendTextMessageAsync(chatId,
                                                       message,
                                                       disableNotification: true//,
                                                //       parseMode: Telegram.Bot.Types.Enums.ParseMode.MarkdownV2
                ).Result;

                logger.LogInformation("Sended to {0} {1}", chatId, result.Date);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error send to {0}", chatId);
            }

            logger.LogInformation("TelegramBot end SendText");
        }

        public void SendPicture(long chatId, string fileName, string description)
        {
            logger.LogInformation("TelegramBot start SendPicture");

            logger.LogInformation("Try send {0} to {1}", fileName, chatId);

            if (string.IsNullOrEmpty(fileName))
                return;

            try
            {
                var fileId = fileInfo.ContainsKey(fileName) ? fileInfo[fileName] : string.Empty;
                logger.LogInformation("fileId = {0}", fileId);

                InputFile inputFile = string.IsNullOrEmpty(fileId)
                    ? InputFile.FromStream(System.IO.File.OpenRead(fileName))
                    : InputFile.FromFileId(fileId);

                Thread.Sleep(WAIT_BEFORE_SEND_PICTURE * 1000);

                var result = bot.SendPhotoAsync(chatId,
                                                inputFile,
                                                caption: description,
                                                disableNotification: true//,
                                                //parseMode: Telegram.Bot.Types.Enums.ParseMode.MarkdownV2
                                                ).Result;

                fileInfo[fileName] = result.Photo.FirstOrDefault()?.FileId;

                logger.LogInformation("Sended to {0} {1}", chatId, result.Date);

                Thread.Sleep(3 * 1000);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error send to {0}", chatId);
            }

            logger.LogInformation("TelegramBot end SendPicture");
        }
    }
}
