using RegisterBotConsole.Data.ChatInfo;

namespace RegisterBotConsole
{
    internal class ChatSettings
    {
        readonly ChatInfoRepository repo;
        readonly IList<ChatInfo> chats;
        readonly ChatInfo chat;

        public ChatSettings(string file, long chatId) 
        {
            repo = new ChatInfoRepository(file);
            chats = repo.GetChatInfo();
            chat = chats.FirstOrDefault(x => x.Id == chatId);
            if (chat is null)
            {
                chat = new ChatInfo()
                {
                    Id = chatId
                };
                chats.Add(chat);
            }
        }

        public void ApllyChanges()
            => repo.StoreChatInfo(chats);

        public void RemoveChat()
        { 
            chats.Remove(chat);
            ApllyChanges();
        }

        public void SetGroup(int group)
        {
            if (chat.Group != group)
            { 
                chat.Group = group;
                chat.Caption = $"🗓️Графік відключень, {group} група";
            }
        }

        public bool IsDeletePrevMessage
        {
            get { return chat.IsDeletePrevMessage; }
            set { chat.IsDeletePrevMessage = value; }
        }

        public bool IsSendTextMessage
        {
            get { return chat.IsSendTextMessage; }
            set { chat.IsSendTextMessage = value; }
        }

        public bool IsNoSendPictureDescription
        {
            get { return chat.IsNoSendPictureDescription; }
            set { chat.IsNoSendPictureDescription = value; }
        }

        public string Caption
        {
            get { return chat.Caption; }
            set { chat.Caption = value; }
        }

        public string PowerOffLinePattern
        {
            get { return chat.PowerOffLinePattern; }
            set { chat.PowerOffLinePattern = value; }
        }

        public string PowerOffLeadingSymbol
        {
            get { return chat.PowerOffLeadingSymbol; }
            set { chat.PowerOffLeadingSymbol = value; }
        }
    }
}
