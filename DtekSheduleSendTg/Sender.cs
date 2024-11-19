using DtekSheduleSendTg.Abstraction;
using DtekSheduleSendTg.DTEK;
using Microsoft.Extensions.Logging;


namespace DtekSheduleSendTg
{
    public class Sender(
        ILogger logger, 
        ISiteAnalyzer siteAnalyzer, 
        ITelegramBot bot, 
        IDtekShedule dtekShedule, 
        IChatInfoRepository chatInfoRepository
       )
    {
        public void CheckAndSend()
        {
            logger.LogInformation("Start CheckAndSend");

            var siteInfo = siteAnalyzer.Analyze();

            if (!string.IsNullOrEmpty(siteInfo.Text))
                SendText(siteInfo.Text);

            if (!string.IsNullOrEmpty(siteInfo.PIctureFile))
                SendPicture(siteInfo.PIctureFile);
            
            logger.LogInformation("End CheckAndSend");
        }

        private void SendText(string message)
        {
            logger.LogInformation("Start SendText");

            var chats = chatInfoRepository.GetChatInfo();
            logger.LogInformation("Chat Info count = {0}", chats.Count());

            foreach (var chat in chats)
                bot.SendText(chat.Id, message);

            logger.LogInformation("End SendText");
        }

        private void SendPicture(string fileName)
        {
            logger.LogInformation("Start SendPicture");

            if (string.IsNullOrEmpty(fileName))
                return;

            dtekShedule.AnalyzeFile(fileName);

            var chats = chatInfoRepository.GetChatInfo();
            logger.LogInformation("Chat Info count = {0}", chats.Count());

            foreach (var chat in chats)
            {
                try
                {
                    if (dtekShedule.IsNoSendPicture2Group(chat.Group))
                    {
                        logger.LogInformation("IsNoSendPicture2Group {0}", chat.Group);
                        continue;
                    }

                    var description = dtekShedule.GetFullPictureDescription(chat.Group, chat.Caption);

                    bot.SendPicture(chat.Id, fileName, description);

                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error SendPicture to {0}", chat.Id);
                }

                logger.LogInformation("End SendPicture");
            }
        }
    }
}
