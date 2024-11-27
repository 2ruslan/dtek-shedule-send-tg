namespace RegisterBotConsole.Data.ChatInfo
{
    public record ChatInfo
    {
        public long Id { get; set; }
        public string Caption { get; set; }
        public int Group { get; set; }


        public bool IsDeletePrevMessage { get; set; }
        public bool IsSendTextMessage { get; set; }
    }
}
