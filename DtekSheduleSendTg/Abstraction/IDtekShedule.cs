using DtekSheduleSendTg.Data.Shedule;
using Telegram.Bot.Types;

namespace DtekSheduleSendTg.Abstraction
{
    public interface IDtekShedule
    {
        public bool AnalyzeFile(string file);
        
        public string GetFullPictureDescription(long group, string firsttLine, string linePatern, string leadingSymbol);
        
        public bool IsScheduleChanged(long group);
        public string GetSchedule(long group);
    }
}
