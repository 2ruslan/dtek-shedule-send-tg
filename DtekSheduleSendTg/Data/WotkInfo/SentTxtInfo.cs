namespace DtekSheduleSendTg.Data.WotkInfo
{
    public record SentTxtInfo
    {
        public int MsgId { get; set; }
        public DateOnly SendDt { get; set; }
        public string Text { get; set; }
    }
}
