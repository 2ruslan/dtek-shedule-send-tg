using DtekSheduleSendTg.Abstraction;
using DtekSheduleSendTg.Data.Shedule;
using System.Text.Json;

namespace DtekSheduleSendTg.Data.WotkInfo
{
    internal class ScheduleWeekRepository(string regionDir) : IScheduleWeekRepository
    {
        private readonly string repoFile = Path.Combine(Environment.CurrentDirectory, "WorkDir", regionDir, "Schedules.json");

        public ScheduleWeek GetSchedules()
            => File.Exists(repoFile) ? JsonSerializer.Deserialize<ScheduleWeek>(File.ReadAllText(repoFile)) : new ScheduleWeek();

        public void StoreSchedules(ScheduleWeek schedules)
            => File.WriteAllText(repoFile, JsonSerializer.Serialize(schedules, new JsonSerializerOptions { WriteIndented = true }));
    }
}
