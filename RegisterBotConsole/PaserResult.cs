namespace RegisterBotConsole
{
    internal record PaserResult
    {
        public bool HasError { get; set; }
        public string Error { get; set; }

        public string Region { get; set; }

        public bool IsDeleteChatCommand { get; set; }


        public long? Id { get; set; }
        public bool IsThisChatBotId { get; set; }


        public int? Group { get; set; }

        public bool HasGroupNum { get; set; }
        public string GroupNum { get; set; }

        public bool? IsDeletePrevMessage { get; set; }
        public bool? IsSendTextMessage { get; set; }
        public bool? IsSendTextMessageWhenNoPict { get; set; }
        
        public bool? IsNoSendPictureDescription { get; set; }

        public bool? IsSendWhenChanged { get; set; }

        
        public bool IsGetInfo { get; set; }

        public bool HasCaption { get; set; }
        public string Caption { get; set; }

        public bool HasPowerOffLinePattern { get; set; }
        public string PowerOffLinePattern { get; set; }

        public bool HasPowerOffLeadingSymbol { get; set; }
        public string PowerOffLeadingSymbol { get; set; }

        public bool HasKey { get; set; }
        public string Key { get; set; }
    }
}
