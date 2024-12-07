using Common.Abstraction;
using System.Text.Json;

namespace Common.Data.ChatInfo
{
    public class ChatInfoRepository(string chatInfoFile) : IChatInfoRepository
    {
        public IList<ChatInfo> GetChatInfo()
            => JsonSerializer.Deserialize<List<ChatInfo>>(File.ReadAllText(chatInfoFile));

        public void StoreChatInfo(IList<ChatInfo> chatInfo)
           => File.WriteAllText(chatInfoFile, JsonSerializer.Serialize(chatInfo, new JsonSerializerOptions { WriteIndented = true }));
    }
}
