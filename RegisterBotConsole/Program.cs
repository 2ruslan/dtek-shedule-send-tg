
using Microsoft.Extensions.Logging;
using NReco.Logging.File;
using RegisterBotConsole.Data.ChatInfo;
using Telegram.Bot;
using Telegram.Bot.Polling;

const string StartMessage = 
"Для регістрації відправки розкладу відключень відправте повідомлення з" + "\r\n" +
        "  ід групи (можна скористатися ботом @userinfobot)" + "\r\n" +
        "  латинську літеру:" + "\r\n" +
        "    k - для Києва, " + "\r\n" +
        "    r - для Київської області " + "\r\n" +
        "    d - для Дніпра " + "\r\n" +
        "    o - для Одеси " + "\r\n" +
        "  номером групи відключень (1-6) або латинську літеру d для видалення свого чату з відправки розкладу." + "\r\n" +
        "Ці дані повинні бути розділені пробілом." + "\r\n" +
        "Приклад повідомлення для отримання графіків відключень в київській області по 3 групі: -123456789 r 3" + "\r\n" +
        "Приклад повідомлення для видалення в київській області по 3 групі: -123456789 r d"
        ;

ILogger logger = LoggerFactory
                                            .Create(builder => builder
                                                            .AddConsole()
                                                            .AddFile("app.log", append: true)
                                            ).CreateLogger("DtekSheduleSendTg");

var botToken = System.Configuration.ConfigurationManager.AppSettings["BotToken"];
var kyivChatsFilePath = System.Configuration.ConfigurationManager.AppSettings["KyivChatsFilePath"];
var kyivRegionChatsFilePath = System.Configuration.ConfigurationManager.AppSettings["KyivRegionChatsFilePath"];
var dniproRegionChatsFilePath = System.Configuration.ConfigurationManager.AppSettings["DnyproChatsFilePath"];
var odesaChatsFilePath = System.Configuration.ConfigurationManager.AppSettings["OdesaChatsFilePath"];

var cancellationToken = new CancellationTokenSource().Token;

var bot = new TelegramBotClient(botToken);

var me = await bot.GetMe(cancellationToken);

Console.WriteLine(me.FirstName + " " + me.LastName);

try
{
    await bot.ReceiveAsync(
        HandleUpdateAsync,
        HandleErrorAsync,
        new ReceiverOptions { AllowedUpdates = { } },
        cancellationToken
    );
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}

async Task HandleUpdateAsync(ITelegramBotClient botClient, Telegram.Bot.Types.Update update, CancellationToken cancellationToken)
{
    try
    {
        if (update.Message is Telegram.Bot.Types.Message message)
        {
            logger.LogInformation("=====================================================");
            if (message.From?.Id is not null)
                logger.LogInformation("From.Id:{0}", message.From.Id);
            if (message.From?.Username is not null)
                logger.LogInformation("From.Username:{0}", message.From.Username);
            if (message.From?.FirstName is not null)
                logger.LogInformation("From.FirstName:{0}", message.From.FirstName);
            if (message.From?.LastName is not null)
                logger.LogInformation("From.LastName:{0}", message.From.LastName);
            if (message.ForwardFromMessageId is not null)
                logger.LogInformation("ForwardFromMessageId:{0}", message.ForwardFromMessageId);
            if (message.ForwardFrom?.Id is not null)
                logger.LogInformation("ForwardFrom:{0}", message.ForwardFrom.Id);
            if (message.ForwardFromChat?.Id is not null)
                logger.LogInformation("ForwardFromChat.id:{0}", message.ForwardFromChat.Id);
            if (message.ForwardFromChat?.Username is not null)
                logger.LogInformation("ForwardFromChat.Username:{0}", message.ForwardFromChat.Username);
            if (message.Text is not null)
                logger.LogInformation(message.Text);

            Console.WriteLine("=====================================================");

            string outMessage;

            if (message.Text.ToLower().EndsWith("start"))
                outMessage = StartMessage;
            else
            {
                try
                {
                    outMessage = await HandleMessage(message.Text, message.From.Id);
                    logger.LogInformation(outMessage);
                }
                catch (Exception ex)
                {
                    outMessage = ex.Message;
                    logger.LogError(ex, "HandleMessage");
                }
            }

            await bot.SendMessage(message.Chat.Id, outMessage);
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "HandleMessage");
    }
}



async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    logger.LogError(exception, "HandleErrorAsync");
}

async Task<string> HandleMessage(string message, long userid)
{
    var separators = new char[] { '\t', ' ', '\r', '\n', ',', ';' };
    var types = new char[] { 'k', 'r', 'd', 'o'};

    var parts = (message ?? string.Empty).Split(separators);

    if (parts.Length != 3)
        return "Невірний формат повідомлення (повинен скаладатися з трьох частин).";

    long chatId;
    if (!long.TryParse(parts[0].Trim(), out chatId) || chatId > 0)
        return "Перший параметр повинен бути ід групи (від`ємне цифрове значення)";

    var chkChat = await bot.GetChat(chatId);

    var admins = await bot.GetChatAdministrators(chkChat.Id);

    if (Array.Find(admins, x => x.User.Id == userid) == null)
        return "У Вас відсутні права адміністратора на вказаний чат/групу.";

    var botAdmin = Array.Find(admins, x => x.User.Id == me.Id);
    if (botAdmin == null)
        return "У бота відсутні права адміністратора на вказаний чат/групу.";

    var type = parts[1].Trim().ToLower().First();
    if (!types.Contains(type))
        return "Другий парметр повинен бути латинська літера що характеризує регіон (див.опис по start).";
    
    var part3 = parts[2].Trim().ToLower();

    int group;
    if (part3 == "d")
        group = -1;
    else if (!int.TryParse(parts[2].Trim(), out group) || group < 1 || group > 6)
        return "Третій параметр повинен бути номером групи відключення (1-6)";

    var file = string.Empty;
    if (type == 'k')
        file = kyivChatsFilePath;
    else if (type == 'r')
        file = kyivRegionChatsFilePath;
    else if (type == 'd')
        file = dniproRegionChatsFilePath;
    else if (type == 'o')
        file = odesaChatsFilePath;
    else
        return "Невідомий регіон";

    var repo = new ChatInfoRepository(file);

    var chats = repo.GetChatInfo();

    var chat = chats.FirstOrDefault(x => x.Id == chatId);

    if (group == -1)
    {
        chats.Remove(chat);
    }
    else if (chat != null)
    {
        chat.Group = group;
        chat.Caption = $"Графік відключень, {group} група";
    }
    else
        chats.Add(new ChatInfo()
        {
            Id = chatId,
            Caption = $"Графік відключень, {group} група",
            Group = group
        });

    repo.StoreChatInfo(chats);

    return "Успішно виконано.";
}
