namespace DtekSheduleSendTg.Data.WotkInfo
{
    public record class WorkInfo
    {
        public Dictionary<long, List<SentPictInfo>> SendedPictInfo { get; set; } = new();

        public Dictionary<long, List<SentTxtInfo>> SendedTxtInfo { get; set; } = new();
        
    }
}
