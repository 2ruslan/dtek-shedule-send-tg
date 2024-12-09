using System.Text.RegularExpressions;

namespace Common
{
    public static class TextHelper
    {
        public static string GetFomatedFirstLine(string patern, DateOnly dt)
            => string.Format(patern
                                .Replace("{d}",  "{0}")
                                .Replace("{dd}", "{1}")
                            , dt.ToString("dd.MM.yyyy")
                            , dt == DateOnly.FromDateTime(DateTime.Now) ? "сьогодні" : "завтра"
                );
        
        public static string GetFomatedLine(string patern, string leadingSymbol, int s, int f)
        {
            var h = f - s;

            if (string.IsNullOrEmpty(patern))
                patern = "<b>    {s} - {f}</b>";

            var realPatern = patern
                                .Replace("{s}", "{0}")
                                .Replace("{f}", "{1}")
                                .Replace("{h}", "{2}");

            if (string.IsNullOrEmpty(leadingSymbol))
                leadingSymbol = "  ";

            return string.Format(realPatern, GetFormatedH(s, leadingSymbol), GetFormatedH(f, leadingSymbol), h);
        }

        private static string GetFormatedH(int h, string leadingSymbol)
            => h < 10 ? $"{leadingSymbol}{h}:00" : $"{h}:00";


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
