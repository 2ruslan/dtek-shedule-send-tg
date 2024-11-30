namespace DtekSheduleSendTg.Abstraction
{
    public interface IDtekShedule
    {
        public bool AnalyzeFile(string file);
        
        public string GetFullPictureDescription(long group, string firsttLine, string linePatern, string leadingSymbol);
    }
}
