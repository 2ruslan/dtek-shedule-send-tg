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

            if (DateTime.Now.DayOfWeek == DayOfWeek.Monday 
                && scheduleWeek.Schedules
                                .Where(x => x.DayOfWeek > 1)
                                .Select(x => x.DayOfWeek)
                                .Any())
                scheduleWeek.Schedules.Clear();

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

                        await SendToSvitlobot(ci.SvitlobotKey, sb.ToString(), ci.GroupNum);

                        monitoring.Counter(MonitoringName);
                    }
                }
            }

            loger.LogInformation("SendToSvitlobot Send end");
        }

        private async Task SendToSvitlobot(string key, string schedules, string group)
        {
            loger.LogInformation("SendToSvitlobot {0} {1} {2}", group, key, schedules);

            var client = new HttpClient();

            try
            {
                if (GroupHelper.SvitlobotGroupMaps.ContainsKey(group))
                {
                    Thread.Sleep(50);

                    var grp = GroupHelper.SvitlobotGroupMaps[group];
                    string urlGroup = $"https://api.svitlobot.in.ua/website/setChannelTimetable?channel_key={key}&timetable_id={grp}";
                    var resultGrp = await client.GetAsync(urlGroup);
                    loger.LogInformation("SendToSvitlobot set group result code ={0}", resultGrp.StatusCode);
                }
            }
            catch (Exception e)
            {
                loger.LogError(e, "send group to svitlobot");
            }

            try
            {
                Thread.Sleep(50);

                string urlSchedules = $"https://api.svitlobot.in.ua/website/timetableEditEvent?&channel_key={key}&timetableData={schedules}";
                var resultSch = await client.GetAsync(urlSchedules);
                loger.LogInformation("SendToSvitlobot set schedule result code ={0}", resultSch.StatusCode);
            }
            catch (Exception e)
            {
                loger.LogError(e, "send schedule to svitlobot");
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
