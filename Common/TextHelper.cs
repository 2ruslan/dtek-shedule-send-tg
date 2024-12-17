using System.Text.RegularExpressions;

namespace Common
{
    public static class TextHelper
    {
        private static Dictionary<DayOfWeek, string> shortDayOfWeekNames = new Dictionary<DayOfWeek, string>
        {
            [DayOfWeek.Monday]      = "Пн",
            [DayOfWeek.Tuesday]     = "Вт",
            [DayOfWeek.Wednesday]   = "Ср",
            [DayOfWeek.Thursday]    = "Чт",
            [DayOfWeek.Friday]      = "Пт",
            [DayOfWeek.Saturday]    = "Сб",
            [DayOfWeek.Sunday]      = "Нд"
        };

        private static Dictionary<DayOfWeek, string> fullDayOfWeekNames = new Dictionary<DayOfWeek, string>
        {
            [DayOfWeek.Monday]      = "Понеділок",
            [DayOfWeek.Tuesday]     = "Вівторок",
            [DayOfWeek.Wednesday]   = "Середа",
            [DayOfWeek.Thursday]    = "Четвер",
            [DayOfWeek.Friday]      = "П'ятниця",
            [DayOfWeek.Saturday]    = "Субота",
            [DayOfWeek.Sunday]      = "Неділя"
        };

        public static string GetFomatedFirstLine(string patern, DateOnly dt)
            => string.Format(patern
                                .Replace("{d}",  "{0}")
                                .Replace("{dd}", "{1}")
                                .Replace("{sdw}","{2}")
                                .Replace("{fdw}","{3}")
                            , dt.ToString("dd.MM.yyyy")
                            , dt == DateOnly.FromDateTime(DateTime.Now) ? "сьогодні" : "завтра"
                            , shortDayOfWeekNames[dt.DayOfWeek]
                            , fullDayOfWeekNames[dt.DayOfWeek]
                );
        
        public static string GetFomatedLine(string patern, string leadingSymbol, int sh, int sm, int fh, int fm)
        {
            var h = fh - sh - (sm == 30 ? 0.5 : 0 ) + (fm == 30 ? 0.5 : 0);
            
            if (string.IsNullOrEmpty(patern))
                patern = "<b>    {s} - {f}</b>";

            var realPatern = patern
                                .Replace("{s}", "{0}")
                                .Replace("{f}", "{1}")
                                .Replace("{h}", "{2}");

            if (string.IsNullOrEmpty(leadingSymbol))
                leadingSymbol = "  ";

            return string.Format(realPatern, 
                    GetFormatedH(sh, sm, leadingSymbol), 
                    GetFormatedH(fh, fm, leadingSymbol),
                    string.Format("{0:#.#}",h));
        }

        private static string GetFormatedH(int h, int m, string leadingSymbol)
            => (h < 10 ? leadingSymbol : string.Empty) + string.Format("{0}:{1:00}",h, m);


        public static string FixHtml2Telegram(this string str)
        {
            string[] telegramHtmlTags = { "b", "strong", "i", "em", "code", "s", "strike", "del", "u" };

            // remove no telegram tags
            foreach (Match match in Regex.Matches(str, "<.*?>"))
            {
                if (!telegramHtmlTags.Any(x =>
                                        string.Equals($"<{x}>", match.Value)
                                        || string.Equals($"</{x}>", match.Value)))
                    str = str.Replace(match.Value, string.Empty);
            }

            // fix tags
            var openTags = Regex.Matches(str, @"<[^\/].*?>").ToList();
            openTags.Reverse();
            foreach (Match match in openTags)
            {
                var clocedTag = match.Value.Replace("<", "</");
                if (!str.Contains(clocedTag))
                    str += clocedTag;
            }

            return str
                    .Replace("&nbsp;", " ");
        }

        public static string DeleteAllTags(this string str)
            => Regex.Replace(str, "<.*?>", string.Empty);

        public static string DeleteWhiteSpace(this string str)
            => new string(str.ToCharArray().Where(c => !Char.IsWhiteSpace(c)).ToArray());
    }
}
