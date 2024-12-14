using DtekSheduleSendTg.Data.Shedule;
using Telegram.Bot.Types;

namespace DtekSheduleSendTg.Abstraction
{
    public interface IDtekShedule
    {
        public bool AnalyzeFile(string file);
        
        public string GetFullPictureDescription(string group, string firsttLine, string linePatern, string leadingSymbol);
        
        public string GetSchedule(string group);
    }
}
