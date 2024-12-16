using Common;
using Common.Abstraction;
using Common.Data.ChatInfo;
using DtekSheduleSendTg.Abstraction;
using DtekSheduleSendTg.Data.PIctureFileInfo;
using DtekSheduleSendTg.Data.WotkInfo;
using Microsoft.Extensions.Logging;
using System.Data;

namespace DtekSheduleSendTg
{
    public class Sender(
        ILogger logger, 
        ISiteAnalyzer siteAnalyzer, 
        ITelegramBot bot, 
        IDtekShedule dtekShedule, 
        IChatInfoRepository chatInfoRepository,
        IWorkInfoRepository workInfoRepository,
        IScheduleWeekRepository scheduleWeekRepository,
        IMonitoring monitoring
       )
    {
        public async Task CheckAndSend()
        {
            logger.LogInformation("Start CheckAndSend");

            monitoring.Start();

            var siteInfo = siteAnalyzer.Analyze();

            monitoring.AddCheckpoint("src&analyze");

            var chats = chatInfoRepository.GetChatInfo();
            logger.LogInformation("Chat Info count = {0}", chats.Count());
            monitoring.Append("Chats count", chats.Count);


            if (!string.IsNullOrEmpty(siteInfo.Text))
                await SendText(chats, siteInfo.Text);

            monitoring.AddCheckpoint("txt sent");

            foreach (var file in siteInfo.PIctureFiles)
                if (!string.IsNullOrEmpty(file.FileName))
                    await SendPicture(chats, file);

            monitoring.AddCheckpoint("pic sent");
            
            //await Send2Svitlobot(siteInfo.PIctureFiles);

            //monitoring.AddCheckpoint("sbt sent");
            
            monitoring.Finish();

            logger.LogInformation("End CheckAndSend");
        }

        private async Task Send2Svitlobot(IEnumerable<PIctureFileInfo> pIctureFileInfos)
        {
            var svitlobotSender = new SvitlobotSender(logger,
                                                            chatInfoRepository,
                                                            scheduleWeekRepository,
                                                            pIctureFileInfos,
                                                            dtekShedule,
                                                            monitoring
                                                            );
            try
            {
                svitlobotSender.Prepare();
                await svitlobotSender.Send();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error Send2Svitlobot to");
            }
        }

        private async Task SendText(IList<ChatInfo> chats, string message)
        {
            const string MonitoringName = "Send txt";
            monitoring.CounterRgister(MonitoringName);

            logger.LogInformation("Start SendText");

            foreach (var chat in chats)
                if (chat.IsSendTextMessage)
                {
                    await bot.SendText(chat.Id, message);
                    monitoring.Counter(MonitoringName);
                }

            logger.LogInformation("End SendText");
        }

        private async Task SendPicture(IList<ChatInfo> chats, PIctureFileInfo fileInfo)
        {
            const string MonitoringName = "Send pic";
            monitoring.CounterRgister(MonitoringName);

            logger.LogInformation("Start SendPicture");

            dtekShedule.AnalyzeFile(fileInfo.FileName);
          
            var workInfo = workInfoRepository.GetWorkInfo();

            foreach (var chat in chats)
            {
                var currentSh = dtekShedule.GetSchedule(chat.GroupNum);

                var sendedPictInfo = workInfo.SendedPictInfo.ContainsKey(chat.Id)
                            ? workInfo.SendedPictInfo[chat.Id]
                            : (workInfo.SendedPictInfo[chat.Id] = new());

                bool IsSendPict = !sendedPictInfo.Exists(x => string.Equals(x.FileName, fileInfo.FileName));

                if (IsSendPict && chat.IsSendWhenPictChanged)
                {
                    var prevInfo = GetInfoOnDate(sendedPictInfo, fileInfo.OnDate);
                    IsSendPict = prevInfo is null || !string.Equals(prevInfo.ScheduleString, currentSh);
                }

                if (IsSendPict)
                {
                    try
                    {
                        var description = chat.IsNoSendPictureDescription
                            ? string.Empty
                            : dtekShedule.GetFullPictureDescription(chat.GroupNum,
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
                                    FileName = fileInfo.FileName,
                                    OnDate = fileInfo.OnDate,
                                    ScheduleString = currentSh
                                });

                        monitoring.Counter(MonitoringName);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error SendPicture to {0}", chat.Id);
                    }
                }

                try
                {
                    logger.LogInformation("Delete Old msgs chatid = {0}", chat.Id);

                    foreach (var msgId in GetMsgIdForDelete(sendedPictInfo))
                    {
                        if (chat.IsDeletePrevMessage)
                            await bot.DeleteMessage(chat.Id, msgId);
                        sendedPictInfo.Remove(sendedPictInfo.Find(x => x.MsgId == msgId));
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error 2 SendPicture to {0}", chat.Id);
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
                                .Select(x => x.MsgId) 
                                .DefaultIfEmpty()
                                .Max()
                                ;

            return sendedPictInfos.FirstOrDefault(x => x.MsgId == lastMsgId);
        }
    }
}
