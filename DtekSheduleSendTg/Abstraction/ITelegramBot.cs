namespace DtekSheduleSendTg.Abstraction
{
    public interface ITelegramBot
    {
        Task<int> SendPicture(long chatId, string fileName, string description);
        Task<int> SendText(long chatId, string message);
        Task DeleteMessage(long chatId, int msgId);
    }
}
