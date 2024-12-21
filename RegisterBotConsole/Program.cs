
using Common;
using Microsoft.Extensions.Logging;
using NReco.Logging.File;
using RegisterBotConsole;
using System.Text;
using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Polling;

string workDir = Path.Combine(Environment.CurrentDirectory, "WorkDir");

ILogger logger = LoggerFactory
                                            .Create(builder => builder
                                                            .AddConsole()
                                                            .AddFile("app.log"
                                                            , fileLoggerOpts =>
                                                            {
                                                                fileLoggerOpts.Append = true;
                                                                fileLoggerOpts.MaxRollingFiles = 10;
                                                                fileLoggerOpts.FileSizeLimitBytes = 500 * 1024;
                                                            }
                                                )
                                            ).CreateLogger("DSB");


var botToken = System.Configuration.ConfigurationManager.AppSettings["BotToken"];


var adminUserId = int.Parse(System.Configuration.ConfigurationManager.AppSettings["AdminUserId"]);


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
            if (message.From.Id == adminUserId && message.ForwardOrigin != null)
            {
                var startMessage = new StartMessage(workDir);
                await startMessage.Set(message);

                return;
            }
            else if (message.ForwardFromChat?.Id is not null)
            {
                await bot.SendMessage(message.Chat.Id, $"id: {message.ForwardFromChat.Id} \r\nname: {message.ForwardFromChat.Username}\r\ntitle: {message.ForwardFromChat.Title}");
                return;
            }
            else if (string.IsNullOrEmpty(message?.Text))
            {
                await bot.SendMessage(message.Chat.Id, "Нічого ніпонятно, но дуже інтересно. Для коректної роботи рекомендую почитати інструкцію, яку можна отримати написавши help цьому ботові. Повідомлення з командами ,бажано, не копіювати а писати рукаами. Якщо і копіювати то з блокноту, з телеграму може непрацювати.");
                return;
            }
            else if (message.Text.ToLower().EndsWith("start") || message.Text.ToLower().EndsWith("help") || message.Text.ToLower().Equals("?"))
            {
                var startMessage = new StartMessage(workDir);
                var msg = await startMessage.Get();
                await bot.SendMessage(message.Chat.Id, msg.Text, parseMode: Telegram.Bot.Types.Enums.ParseMode.None, entities: msg.Entities);

                return;
            }
            else
            {
                try
                {
                    outMessage = await HandleMessage(message);
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

async Task<string> HandleMessage(Telegram.Bot.Types.Message msg)
{
    var message = msg.Text;
    var userid = msg.From.Id;

    var paser = new Paser();
    var paresrResult = paser.Parse(message);

    if (paresrResult.HasError)
        return paresrResult.Error;

    long GroupId;
    if (paresrResult.IsThisChatBotId)
        GroupId = msg.Chat.Id;
    else
    {
        GroupId = paresrResult.Id.Value;

        var rightsError = await TelegramHelper.CheckRights(bot, GroupId, userid);
        if (!string.IsNullOrEmpty(rightsError))
            return rightsError;
    }

    var file = ChatFiles.ResolveFileName(paresrResult.Region);

    var chatSettings = new ChatSettings(file, GroupId);
    var sb = new StringBuilder();


    if (paresrResult.IsGetInfo)
    {
        return JsonSerializer.Serialize(chatSettings, new JsonSerializerOptions { WriteIndented = true });
    }
    else if (paresrResult.IsDeleteChatCommand)
    {
        chatSettings.RemoveChat();
        return "Успішно видалено.";
    }
    else
    {
        if (paresrResult.HasGroupNum)
            chatSettings.GroupNum = paresrResult.GroupNum;

        if (paresrResult.IsDeletePrevMessage.HasValue)
            chatSettings.IsDeletePrevMessage = paresrResult.IsDeletePrevMessage.Value;

        if (paresrResult.IsSendTextMessage.HasValue)
            chatSettings.IsSendTextMessage = paresrResult.IsSendTextMessage.Value;

        if (paresrResult.IsSendTextMessageWhenNoPict.HasValue)
            chatSettings.IsSendTextMessageWhenNoPict = paresrResult.IsSendTextMessageWhenNoPict.Value;
        
        if (paresrResult.IsNoSendPictureDescription.HasValue)
            chatSettings.IsNoSendPictureDescription = paresrResult.IsNoSendPictureDescription.Value;

        if (paresrResult.IsSendWhenChanged.HasValue)
            chatSettings.IsSendWhenPictChanged = paresrResult.IsSendWhenChanged.Value;
        
        if (paresrResult.HasCaption)
            chatSettings.Caption = paresrResult.Caption;

        if (paresrResult.HasPowerOffLeadingSymbol)
            chatSettings.PowerOffLeadingSymbol = paresrResult.PowerOffLeadingSymbol;

        if (paresrResult.HasPowerOffLinePattern)
            chatSettings.PowerOffLinePattern = paresrResult.PowerOffLinePattern;

        if (paresrResult.HasKey)
            chatSettings.Key = paresrResult.Key;

        if (string.IsNullOrEmpty(chatSettings.GroupNum))
            sb.Append("Схоже невірно вказаний другий параметр, перевірте літеру що вказує на регіон.");
        else
        {
            chatSettings.ApllyChanges();

            sb.Append("Успішно виконано.");

            if (paresrResult.HasCaption || paresrResult.HasPowerOffLeadingSymbol || paresrResult.HasPowerOffLinePattern)
            {
                sb.AppendLine("Приклад підпису:");
                sb.AppendLine(TextHelper.GetFomatedFirstLine(chatSettings.Caption, DateOnly.FromDateTime(DateTime.Now)));
                sb.AppendLine(TextHelper.GetFomatedLine(chatSettings.PowerOffLinePattern, chatSettings.PowerOffLeadingSymbol, 3, 0, 9, 30));
                sb.AppendLine(TextHelper.GetFomatedLine(chatSettings.PowerOffLinePattern, chatSettings.PowerOffLeadingSymbol, 11, 0, 15, 0));
            }
        }
    }

    return sb.ToString();
}


