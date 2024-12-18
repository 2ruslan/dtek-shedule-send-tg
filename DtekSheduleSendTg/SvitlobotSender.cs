using Common;
using Common.Abstraction;
using Common.Data.ChatInfo;
using DtekSheduleSendTg.Abstraction;
using DtekSheduleSendTg.Data.PIctureFileInfo;
using DtekSheduleSendTg.Data.Shedule;
using DtekSheduleSendTg.DTEK;
using Microsoft.Extensions.Logging;
using System.Text;

namespace DtekSheduleSendTg
{
    public class SvitlobotSender(ILogger loger
                                , IList<ChatInfo> chatInfos
                                , IScheduleWeekRepository scheduleWeekRepository
                                , IEnumerable<PIctureFileInfo> pIctureFiles
                                , IDtekShedule dtekShedule
                                , IMonitoring monitoring
        )
    {
        private const string emptyDaySchedule = "000000000000000000000000";
        private ScheduleWeek scheduleWeek;

        private static bool IsMondayNoUpdateTime
                        => (DateTime.Now.DayOfWeek == DayOfWeek.Monday && DateTime.Now.Hour < 12)
            ;
                

        public void Prepare()
        {
            loger.LogInformation("SendToSvitlobot Prepare start");

            if (IsMondayNoUpdateTime)
                return;

            scheduleWeek = scheduleWeekRepository.GetSchedules();

            foreach(var itm in  scheduleWeek.Schedules)
                itm.IsChaged = false;

           // if (DateTime.Now.DayOfWeek == DayOfWeek.Monday && scheduleWeek.Schedules.Count > 0)
           //     scheduleWeek.Schedules.Clear();

            foreach (var fi in pIctureFiles)
            {
                dtekShedule.AnalyzeFile(fi.FileName);
                foreach(var g in GroupHelper.Groups)
                    SetShedule(g, fi.OnDate, dtekShedule.GetSchedule(g));
            }

            scheduleWeekRepository.StoreSchedules(scheduleWeek);

            loger.LogInformation("SendToSvitlobot Prepare end");
        }

        public async Task Send()
        {
            const string MonitoringName = "sent sbt";
            monitoring.CounterRgister(MonitoringName);

            loger.LogInformation("SendToSvitlobot Send start");

            if (IsMondayNoUpdateTime)
                return;

            foreach (var ci in chatInfos)
            {
                if (!string.IsNullOrEmpty(ci.SvitlobotKey) && GroupHelper.Groups.Any( x => x == ci.GroupNum))
                {
                    var grpSchedules = scheduleWeek.Schedules
                                                    .Where(x => x.GroupNum == ci.GroupNum)
                                                    .OrderBy(x => x.DayOfWeek);

                    if (grpSchedules.Count() == 0 || grpSchedules.Any(x => x.IsChaged))
                    {
                        loger.LogInformation("SendToSvitlobot Send to {0} {1}", ci.Id, ci.SvitlobotKey);

                        var sb = new StringBuilder("");
                        for (int i = 1; i < 8; i++)
                            sb.AppendFormat("{0}%3B", grpSchedules.FirstOrDefault(x => x.DayOfWeek == i)?.SheduleString ?? emptyDaySchedule);

                        await SendToSvitlobot(ci.SvitlobotKey, sb.ToString());

                        monitoring.Counter(MonitoringName);
                    }
                }
            }

            loger.LogInformation("SendToSvitlobot Send end");
        }

        private async Task SendToSvitlobot(string key, string schedules)
        {
            try
            {
                string url = $"https://api.svitlobot.in.ua/website/timetableEditEvent?&channel_key={key}&timetableData={schedules}";
                
                loger.LogInformation("SendToSvitlobot {0} {1}", key, schedules);

                var client = new HttpClient();
                await client.GetAsync(url);
                Thread.Sleep(300);
            }
            catch (Exception e)
            {
                loger.LogError(e, "send to svitlobot");
            }
        }

        private void SetShedule(string group, DateOnly date, string schedule)
        {
            var normalDayOfWeek = date.DayOfWeek == DayOfWeek.Sunday ? 7 : ((int)date.DayOfWeek);

            var daySchedule = scheduleWeek.Schedules
                                .FirstOrDefault(x => x.GroupNum == group && x.DayOfWeek == normalDayOfWeek);


            if (daySchedule is null)
                scheduleWeek.Schedules.Add(daySchedule = new ScheduleWeekDay() { GroupNum = group, DayOfWeek = normalDayOfWeek });

            var newSchedule = schedule
                                  .Replace(SheduleData._ON_05_END, '2')
                                  .Replace(SheduleData._ON_05_START, '3')
                                  ;

            daySchedule.IsChaged = !string.Equals(daySchedule.SheduleString, newSchedule);
            daySchedule.SheduleString = newSchedule;
        }
    }
}
