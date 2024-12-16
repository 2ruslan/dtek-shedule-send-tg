namespace DtekSheduleSendTg.Data.Shedule
{
    public record SheduleData
    {
        public const char _ON = '0';
        public const char _OFF = '1';
        public const char _ON_05_START = '+';
        public const char _ON_05_END = '*';

        public string GroupNum { get; set; }
      
        public string SheduleString { get; set; }
    }
}
