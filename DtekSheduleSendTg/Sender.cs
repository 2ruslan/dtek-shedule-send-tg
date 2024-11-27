using DtekSheduleSendTg.Abstraction;
using Microsoft.Extensions.Logging;


namespace DtekSheduleSendTg
{
    public class Sender(
        ILogger logger, 
        ISiteAnalyzer siteAnalyzer, 
        ITelegramBot bot, 
        IDtekShedule dtekShedule, 
        IChatInfoRepository chatInfoRepository,
        IWorkInfoRepository workInfoRepository
       )
    {
        public async Task CheckAndSend()
        {
            logger.LogInformation("Start CheckAndSend");

            var siteInfo = siteAnalyzer.Analyze();

            if (!string.IsNullOrEmpty(siteInfo.Text))
                await SendText(siteInfo.Text);

            if (!string.IsNullOrEmpty(siteInfo.PIctureFile))
                await SendPicture(siteInfo.PIctureFile);
            
            logger.LogInformation("End CheckAndSend");
        }

        private async Task SendText(string message)
        {
            logger.LogInformation("Start SendText");
            
            var chats = chatInfoRepository.GetChatInfo();
            logger.LogInformation("Chat Info count = {0}", chats.Count());

            foreach (var chat in chats)
                if (chat.IsSendTextMessage)
                    await bot.SendText(chat.Id, message);
            
            logger.LogInformation("End SendText");
        }

        private async Task SendPicture(string fileName)
        {
            logger.LogInformation("Start SendPicture");

            if (string.IsNullOrEmpty(fileName))
                return;

            dtekShedule.AnalyzeFile(fileName);

            var chats = chatInfoRepository.GetChatInfo();
            logger.LogInformation("Chat Info count = {0}", chats.Count());

            var workInfo = workInfoRepository.GetWorkInfo();

            foreach (var chat in chats)
            {
                try
                {
                    /*
                    if (dtekShedule.IsNoSendPicture2Group(chat.Group))
                    {
                        logger.LogInformation("IsNoSendPicture2Group {0}", chat.Group);
                        continue;
                    }
                    */
                    var description = dtekShedule.GetFullPictureDescription(chat.Group, chat.Caption);

                    var id = await bot.SendPicture(chat.Id, fileName, description);

                    if (chat.IsDeletePrevMessage && workInfo.LastPictureMessagesId.ContainsKey(chat.Id))
                        await bot.DeleteMessage(chat.Id, workInfo.LastPictureMessagesId[chat.Id]);

                    workInfo.LastPictureMessagesId[chat.Id] = id;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error SendPicture to {0}", chat.Id);
                }

                logger.LogInformation("End SendPicture");
            }

            workInfoRepository.StoreWorkInfo(workInfo);
        }
    }
}
