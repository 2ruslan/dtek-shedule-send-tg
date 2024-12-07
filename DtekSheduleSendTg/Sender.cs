using Common;
using Common.Abstraction;
using DtekSheduleSendTg.Abstraction;
using DtekSheduleSendTg.Data.WotkInfo;
using DtekSheduleSendTg.DTEK;
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
            
            foreach(var file in siteInfo.PIctureFiles)
                await SendPicture(file);
            
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

        private async Task SendPicture(PIctureFileInfo fileInfo)
        {
            logger.LogInformation("Start SendPicture");


            dtekShedule.AnalyzeFile(fileInfo.FileName);
            

            var chats = chatInfoRepository.GetChatInfo();
            logger.LogInformation("Chat Info count = {0}", chats.Count());

            var workInfo = workInfoRepository.GetWorkInfo();

            foreach (var chat in chats)
            {
                var currentSh = dtekShedule.GetSchedule(chat.Group);

                var sendedPictInfo = workInfo.SendedPictInfo.ContainsKey(chat.Id)
                            ? workInfo.SendedPictInfo[chat.Id]
                            : (workInfo.SendedPictInfo[chat.Id] = new());

                bool IsSendPict = true;

                if (chat.IsSendWhenPictChanged)
                {
                    var prevInfo = GetInfoOnDate(sendedPictInfo, fileInfo.OnDate);
                    IsSendPict = prevInfo is null ||
                                 !string.Equals(prevInfo.ScheduleString, currentSh) ||
                                 prevInfo.OnDate < DateOnly.FromDateTime(DateTime.Now);
                }

                if (IsSendPict)
                {
                    try
                    {
                        var description = chat.IsNoSendPictureDescription
                            ? string.Empty
                            : dtekShedule.GetFullPictureDescription(chat.Group,
                                                                    TextHelper.GetFomatedFirstLine(chat.Caption, fileInfo.OnDate), 
                                                                    chat.PowerOffLinePattern, 
                                                                    chat.PowerOffLeadingSymbol);

                        var id = await bot.SendPicture(chat.Id, fileInfo.FileName, description);

                        sendedPictInfo.Add(
                                new SendedPictInfo()
                                {
                                    MsgId = id,
                                    SendDt = DateTime.Now,
                                    Url = fileInfo.Url,
                                    OnDate = fileInfo.OnDate,
                                    ScheduleString = currentSh
                                });

                        foreach (var msgId in GetMsgIdForDelete(sendedPictInfo))
                        {
                            if (chat.IsDeletePrevMessage)
                                await bot.DeleteMessage(chat.Id, msgId);
                            sendedPictInfo.Remove(sendedPictInfo.Find(x => x.MsgId == msgId));
                        }

                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error SendPicture to {0}", chat.Id);
                    }
                }
            }

            workInfoRepository.StoreWorkInfo(workInfo);

            logger.LogInformation("End SendPicture");
        }

        private IEnumerable<int> GetMsgIdForDelete(List<SendedPictInfo> sendedPictInfos)
        {
            var activeMsgs = sendedPictInfos
                            .Where(x => x.OnDate >= DateOnly.FromDateTime(DateTime.Now))
                            .GroupBy(x => x.OnDate)
                            .Select(g => new
                            {
                                OnDate = g.Key,
                                MsgId = g.Max(x => x.MsgId)
                            });

            var delMsgs = from spi in sendedPictInfos
                            join am in activeMsgs
                            on new { spi.OnDate, spi.MsgId } equals new { am.OnDate, am.MsgId } into joinedData
                            from am in joinedData.DefaultIfEmpty()
                            where am == null
                            select spi.MsgId;

            return delMsgs.ToArray();
        }

        private static SendedPictInfo GetInfoOnDate(List<SendedPictInfo> sendedPictInfos, DateOnly onDt)
        {
            var lastMsgId = sendedPictInfos
                                .Where(x => x.OnDate == onDt)
                                .Max(x => x.MsgId);

            return sendedPictInfos.FirstOrDefault(x => x.MsgId == lastMsgId);
        }
    }
}
