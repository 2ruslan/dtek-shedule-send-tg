
using Common;
using Microsoft.Extensions.Logging;
using NReco.Logging.File;
using RegisterBotConsole;
using RegisterBotConsole.Data.ChatInfo;
using System.Security.Cryptography.Xml;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

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
        "Також є можливість використовувати додаткові параметри :" + "\r\n" +
        "  +delold - для видалення попереднього повідомлення з розкладом" + "\r\n" +
        "  +addtext - для відправки текстових інформаційних повідомлень від НЕК Укренерго" + "\r\n" +
        "  +piconly - для відправки графіка без тексту" + "\r\n" +
        "Приклад повідомлення для отримання графіків відключень в київській області по 3 групі: -123456789 r 3" + "\r\n" +
        "Приклад повідомлення для видалення в київській області : -123456789 r d" + "\r\n" +
        "Приклад повідомлення для з видалення попереднього повідомлення з розкладом для 2 групи: -123456789 r 2 +delold" + "\r\n" +
        "Приклад повідомлення для з видалення попереднього повідомлення з розкладом для 2 групи і відправкою інформаційних повідомлень від НЕК Укренерг: -123456789 r 2 +delold+addtext"
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

            await bot.SendMessage(message.Chat.Id, outMessage, parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
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
    var paser = new Paser();
    var paresrResult = await paser.Parse(message);

    if (paresrResult.HasError)
        return paresrResult.Error;

    var rightsError = await CheckRights(paresrResult.Id.Value, userid);
    if (!string.IsNullOrEmpty(rightsError))
        return rightsError;

    var file = ResolveFileName(paresrResult.Region);
    if (string.IsNullOrEmpty(file))
        return "Невідомий регіон";

    var chatSettings = new ChatSettings(file, paresrResult.Id.Value);

    if (paresrResult.IsDeleteChatCommand)
    {
        chatSettings.RemoveChat();
    }
    else
    {
        if (paresrResult.Group.HasValue)
            chatSettings.SetGroup(paresrResult.Group.Value);

        if (paresrResult.IsDeletePrevMessage.HasValue)
            chatSettings.IsDeletePrevMessage = paresrResult.IsDeletePrevMessage.Value;

        if (paresrResult.IsSendTextMessage.HasValue)
            chatSettings.IsSendTextMessage = paresrResult.IsSendTextMessage.Value;

        if (paresrResult.IsNoSendPictureDescription.HasValue)
            chatSettings.IsNoSendPictureDescription = paresrResult.IsNoSendPictureDescription.Value;

        if (paresrResult.HasCaption)
            chatSettings.Caption = paresrResult.Caption;

        if (paresrResult.HasPowerOffLeadingSymbol)
            chatSettings.PowerOffLeadingSymbol = paresrResult.PowerOffLeadingSymbol;

        if (paresrResult.HasPowerOffLinePattern)
            chatSettings.PowerOffLinePattern = paresrResult.PowerOffLinePattern;
    }

    chatSettings.ApllyChanges();

    var sb = new StringBuilder("Успішно виконано.");

    if (paresrResult.HasCaption || paresrResult.HasPowerOffLeadingSymbol || paresrResult.HasPowerOffLinePattern)
    {
        sb.AppendLine("Приклад підпису:");
        sb.AppendLine(chatSettings.Caption);
        sb.AppendLine(TextHelper.GetFomatedLine(chatSettings.PowerOffLinePattern, chatSettings.PowerOffLeadingSymbol, 3, 9));
        sb.AppendLine(TextHelper.GetFomatedLine(chatSettings.PowerOffLinePattern, chatSettings.PowerOffLeadingSymbol, 11, 15));
    }
        
    return sb.ToString();
}

async Task<string> CheckRights(long chatId, long userid)
{
    var chkChat = await bot.GetChat(chatId);

    var admins = await bot.GetChatAdministrators(chkChat.Id);

    if (Array.Find(admins, x => x.User.Id == userid) == null)
        return "У Вас відсутні права адміністратора на вказаний чат/групу.";

    var botAdmin = Array.Find(admins, x => x.User.Id == me.Id);
    if (botAdmin == null)
        return "У бота відсутні права адміністратора на вказаний чат/групу.";

    return string.Empty;
}

string ResolveFileName(string region)
{
    if (region == "k")
        return kyivChatsFilePath;
    else if (region == "r")
        return kyivRegionChatsFilePath;
    else if (region == "d")
        return dniproRegionChatsFilePath;
    else if (region == "o")
        return odesaChatsFilePath;
    else
        return string.Empty;
}