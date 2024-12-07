using Common.Data.ChatInfo;

namespace DtekSheduleSendTg.Data.ChatInfo
{
    public class ChatInfoRepositoryApp (string regionDir)
        : ChatInfoRepository (Path.Combine(Environment.CurrentDirectory, "WorkDir", regionDir, "Chats", "ChatInfo.json"))
    {
    }
}
