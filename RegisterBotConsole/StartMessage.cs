using System.Text.Json;
using Telegram.Bot.Types;

namespace RegisterBotConsole
{
    internal class StartMessage (string workDir)
    {
        readonly string messageFile = Path.Combine(workDir, "Help.json");

        public async Task Set(Message message)
        {
            
                Message storeMsg = new Message
                {
                    Text = message.Text,
                    Entities = message.Entities,
                    
                };

                await System.IO.File.WriteAllTextAsync(messageFile, JsonSerializer.Serialize(storeMsg, new JsonSerializerOptions { WriteIndented = true }));
        }

        public async Task<Message> Get()
        {
            var json = await System.IO.File.ReadAllTextAsync(messageFile);
            return JsonSerializer.Deserialize<Message>(json);
        }
    }
}
