using System.Text.RegularExpressions;

namespace DtekSheduleSendTg.Common
{
    public static class TextHelper
    {
        
        public static string DeleteHtmlTags(this string str)
        {
            string[] telegramHtmlTags =  { "b", "strong", "i", "em", "code", "s", "strike", "del", "u" };
            
            foreach(Match match in Regex.Matches(str, "<.*?>"))
            { 
                if (!telegramHtmlTags.Any(x => 
                                        string.Equals($"<{x}>", match.Value) 
                                        || string.Equals($"</{x}>", match.Value)))
                 str = str.Replace(match.Value, string.Empty);
            }

            return str;
        }

        public static string MarkBold(this string str)
           => $"<b>{str}</b>";

    }
}
