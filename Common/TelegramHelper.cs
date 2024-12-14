using Telegram.Bot;

namespace Common
{
    public static class TelegramHelper
    {
        public static  async Task<string> CheckRights(TelegramBotClient bot, long chatId, long userid)
        {
            var cancellationToken = new CancellationTokenSource().Token;
            
            var me = await bot.GetMe(cancellationToken);

            var chkChat = await bot.GetChat(chatId);

            var admins = await bot.GetChatAdministrators(chkChat.Id);

            if (Array.Find(admins, x => x.User.Id == userid) == null)
                return "У Вас відсутні права адміністратора на вказаний чат/групу.";

            var botAdmin = Array.Find(admins, x => x.User.Id == me.Id);
            if (botAdmin == null)
                return "У бота відсутні права адміністратора на вказаний чат/групу.";

            return string.Empty;
        }
    }
}
