using Common.Data.ChatInfo;

namespace Common.Abstraction
{
    public interface IChatInfoRepository
    {
        IList<ChatInfo> GetChatInfo();
        void StoreChatInfo(IList<ChatInfo> chatInfo);
    }
}
