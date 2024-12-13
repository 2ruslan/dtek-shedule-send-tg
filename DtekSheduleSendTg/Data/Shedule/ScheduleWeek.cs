namespace DtekSheduleSendTg.Data.Shedule
{
    public record ScheduleWeek
    { 
        public List<ScheduleWeekDay> Schedules { get; set; } = new List<ScheduleWeekDay>();
    }

    public record ScheduleWeekDay : SheduleData
    {
        public int DayOfWeek { get; set; }
        public bool IsChaged  { get; set; }
    }
}
