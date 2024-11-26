using DtekSheduleSendTg.Abstraction;
using System.Text.Json;

namespace DtekSheduleSendTg.Data.ChatInfo
{
    public class ChatInfoRepository (string regionDir): IChatInfoRepository 
    {
        private readonly string chatInfoFile = Path.Combine(Environment.CurrentDirectory, "WorkDir", regionDir, "Chats", "ChatInfo.json");

        public IEnumerable<ChatInfo> GetChatInfo()
            => JsonSerializer.Deserialize<List<ChatInfo>>(File.ReadAllText(chatInfoFile));
    }
}
