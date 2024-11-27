namespace DtekSheduleSendTg.Data.WotkInfo
{
    public record class WorkInfo
    {
        public Dictionary<long, int> LastPictureMessagesId { get; set; } = new();
    }
}
