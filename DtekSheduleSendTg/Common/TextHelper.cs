using System.Text.RegularExpressions;

namespace DtekSheduleSendTg.Common
{
    public static class TextHelper
    {
        
        public static string FixHtml2Telegram(this string str)
        {
            string[] telegramHtmlTags = { "b", "strong", "i", "em", "code", "s", "strike", "del", "u" };
            
            // remove no telegram tags
            foreach(Match match in Regex.Matches(str, "<.*?>"))
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
        
        public static string MarkBold(this string str)
           => $"<b>{str}</b>";

    }
}
