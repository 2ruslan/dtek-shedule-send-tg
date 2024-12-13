using DtekSheduleSendTg.Data.Shedule;

namespace DtekSheduleSendTg.Abstraction
{
    public interface IScheduleWeekRepository
    {
        public ScheduleWeek GetSchedules();

        public void StoreSchedules(ScheduleWeek schedules);
    }
}
