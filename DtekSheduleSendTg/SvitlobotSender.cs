using Common.Abstraction;
using DtekSheduleSendTg.Abstraction;
using DtekSheduleSendTg.Data.PIctureFileInfo;
using DtekSheduleSendTg.Data.Shedule;
using Microsoft.Extensions.Logging;
using System.Text;

namespace DtekSheduleSendTg
{
    public class SvitlobotSender(ILogger loger
                                , IChatInfoRepository chatInfoRepository
                                , IScheduleWeekRepository scheduleWeekRepository
                                , IEnumerable<PIctureFileInfo> pIctureFiles
                                , IDtekShedule dtekShedule
        
        )
    {
        private const string emptyDaySchedule = "000000000000000000000000";
        private ScheduleWeek scheduleWeek;

        private static bool IsMondayNoUpdateTime
                        => (DateTime.Now.DayOfWeek == DayOfWeek.Monday && DateTime.Now.Hour < 12);
                

        public void Prepare()
        {
            loger.LogInformation("SendToSvitlobot Prepare start");

            if (IsMondayNoUpdateTime)
                return;

            scheduleWeek = scheduleWeekRepository.GetSchedules();

            foreach(var itm in  scheduleWeek.Schedules)
                itm.IsChaged = false;

            if (DateTime.Now.DayOfWeek == DayOfWeek.Monday && scheduleWeek.Schedules.Count > 0)
                scheduleWeek.Schedules.Clear();

            foreach (var fi in pIctureFiles)
            {
                dtekShedule.AnalyzeFile(fi.FileName);
                for (int i = 1; i < 7; i++)
                    SetShedule(i, fi.OnDate, dtekShedule.GetSchedule(i));
            }

            scheduleWeekRepository.StoreSchedules(scheduleWeek);

            loger.LogInformation("SendToSvitlobot Prepare end");
        }

        public async Task Send()
        {
            loger.LogInformation("SendToSvitlobot Send start");

            if (IsMondayNoUpdateTime)
                return;

            foreach (var ci in chatInfoRepository.GetChatInfo())
            {
                if (!string.IsNullOrEmpty(ci.SvitlobotKey))
                {
                    var grpSchedules = scheduleWeek.Schedules.Where(x => x.Group == ci.Group).OrderBy(x => x.DayOfWeek);

                    if (grpSchedules.Count() == 0 || grpSchedules.Any(x => x.IsChaged))
                    {
                        loger.LogInformation("SendToSvitlobot Send to {0} {1}", ci.Id, ci.SvitlobotKey);

                        var sb = new StringBuilder("");
                        for (int i = 1; i < 8; i++)
                            sb.AppendFormat("{0}%3B", grpSchedules.FirstOrDefault(x => x.DayOfWeek == i)?.SheduleString ?? emptyDaySchedule);

                        await SendToSvitlobot(ci.SvitlobotKey, sb.ToString());
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
                Thread.Sleep(1000);
            }
            catch (Exception e)
            {
                loger.LogError(e, "send to svitlobot");
            }
        }

        private void SetShedule(int group, DateOnly date, string schedule)
        {
            var normalDayOfWeek = date.DayOfWeek == DayOfWeek.Sunday ? 7 : ((int)date.DayOfWeek);

            var daySchedule = scheduleWeek.Schedules.FirstOrDefault(x => x.Group == group && x.DayOfWeek == normalDayOfWeek);
            if (daySchedule is null)
                scheduleWeek.Schedules.Add(daySchedule = new ScheduleWeekDay() { Group = group, DayOfWeek = normalDayOfWeek });

            var newSchedule = schedule
                                  .Replace('0', 'x')
                                  .Replace('1', '0')
                                  .Replace('x', '1');

            daySchedule.IsChaged = !string.Equals(daySchedule.SheduleString, newSchedule);
            daySchedule.SheduleString = newSchedule;
        }
    }
}
