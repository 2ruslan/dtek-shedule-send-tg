namespace Common.Data.ChatInfo
{
    public record ChatInfo
    {
        public long Id { get; set; }
        public string Caption { get; set; }

        public string GroupNum { get; set; }
       
        public bool IsDeletePrevMessage { get; set; }
        public bool IsSendTextMessage { get; set; }

        public bool IsNoSendPictureDescription { get; set; }

        public bool IsSendWhenPictChanged { get; set; }

        public string PowerOffLinePattern { get; set; }
        public string PowerOffLeadingSymbol { get; set; }

        public string SvitlobotKey { get; set; }
    }
}
