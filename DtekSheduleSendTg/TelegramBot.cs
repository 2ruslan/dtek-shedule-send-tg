using DtekSheduleSendTg.Abstraction;
using DtekSheduleSendTg.Common;
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

        public async Task<int> SendText(long chatId, string message)
        {
            logger.LogInformation("TelegramBot start SendText");
            
            Message result = null;

            try
            {
                logger.LogInformation("Try Send {0} to {1}", message, chatId);

                Thread.Sleep(WAIT_BEFORE_SEND_TEXT * 1000);
                
                try
                {
                    // try send as html
                    logger.LogInformation("Try Send as html [{0}]", message);

                    result = await bot.SendMessage(chatId,
                                                       message.FixHtml2Telegram(),
                                                       disableNotification: true,
                                                       parseMode: Telegram.Bot.Types.Enums.ParseMode.Html
                                );
                }
                catch (Exception ex)
                {
                    // try send as text
                    logger.LogError(ex, "Try Send as text [{0}]", message);

                    result = bot.SendMessage(chatId,
                                                         message.DeleteAllTags(),
                                                         disableNotification:true
                                  ).Result;
                }


                logger.LogInformation("Sended to {0} {1}", chatId, result.Date);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error send to {0}", chatId);
            }
            
            logger.LogInformation("TelegramBot end SendText");

            return result == null? -1 : result.Id;
        }

        public async Task<int> SendPicture(long chatId, string fileName, string description)
        {
            logger.LogInformation("TelegramBot start SendPicture");

            logger.LogInformation("Try send {0} to {1}", fileName, chatId);

            if (string.IsNullOrEmpty(fileName))
                return -1;

            Message result = null;

            try
            {
                var fileId = fileInfo.ContainsKey(fileName) ? fileInfo[fileName] : string.Empty;
                logger.LogInformation("fileId = {0}", fileId);

                InputFile inputFile = string.IsNullOrEmpty(fileId)
                    ? InputFile.FromStream(System.IO.File.OpenRead(fileName))
                    : InputFile.FromFileId(fileId);

                Thread.Sleep(WAIT_BEFORE_SEND_PICTURE * 1000);
                
                try
                {
                    // try send caption as html
                    logger.LogInformation("Try Send as html [{0}]", description);

                    result = await bot.SendPhoto(chatId,
                                                    inputFile,
                                                    caption: description.FixHtml2Telegram(),
                                                    disableNotification: true,
                                                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Html
                                                    );
                }
                catch(Exception ex)
                {
                    // try send caption as text
                    logger.LogError(ex, "Try Send as text [{0}]", description);

                    result = await bot.SendPhoto(chatId,
                                                    inputFile,
                                                    caption: description.DeleteAllTags(),
                                                    disableNotification: true
                                                    );
                }

                fileInfo[fileName] = result.Photo.FirstOrDefault()?.FileId;

                logger.LogInformation("Sended to {0} {1}", chatId, result.Date);

                Thread.Sleep(3 * 1000);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error send to {0}", chatId);
            }

            logger.LogInformation("TelegramBot end SendPicture");

            return result == null ? -1 : result.Id;
        }

        public async Task DeleteMessage(long chatId, int msgId)
        {
            logger.LogInformation("TelegramBot start DeleteMessage {0} from {0}", msgId, chatId);

            try
            {
                await bot.DeleteMessage(chatId, msgId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error delete from {0} message {1}", chatId, msgId);
            }
            logger.LogInformation("TelegramBot start DeleteMessage");
        }
    }
}
