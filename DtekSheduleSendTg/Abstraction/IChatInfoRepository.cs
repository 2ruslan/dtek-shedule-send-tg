using DtekSheduleSendTg.Data.ChatInfo;

namespace DtekSheduleSendTg.Abstraction
{
    public interface IChatInfoRepository
    {
        IEnumerable<ChatInfo> GetChatInfo();
    }
}
