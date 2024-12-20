﻿namespace DtekSheduleSendTg.Data.WotkInfo
{
    public record SentPictInfo
    {
        public int MsgId { get; set; }
        public DateTime SendDt { get; set; }
        public DateOnly OnDate { get; set; }
        public string Url { get; set; }
        public string FileName { get; set; }
        public string ScheduleString { get; set; }
    }
}
