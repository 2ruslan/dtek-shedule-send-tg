namespace DtekSheduleSendTg.Abstraction
{
    public interface ITelegramBot
    {
        void SendPicture(long chatId, string fileName, string description);
        void SendText(long chatId, string message);
    }
}
