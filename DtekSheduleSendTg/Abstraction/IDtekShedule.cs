namespace DtekSheduleSendTg.Abstraction
{
    public interface IDtekShedule
    {
        public bool AnalyzeFile(string file);
        public bool IsNoSendPicture2Group(long group);
        public string GetFullPictureDescription(long group, string firsttLine);
    }
}
