using System.Text.RegularExpressions;

namespace DtekSheduleSendTg.Common
{
    public static class TextHelper
    {
        public static string Html2Markdown(this string str)
        {
            return str?
                .Replace("<strong>", "*")
                .Replace("</strong>", "*")
                ;
        }

        public static string DeleteHtmlTags(this string str)
            => Regex.Replace(str, "<.*?>", String.Empty);

        public static string MarkMarkdownBold(this string str)
           => $"*{str}*";

        public static string PrepateAsMarkdown(this string str)
            => str?
                .Replace(".", @"\.")
                .Replace("_", @"\_")
           //     .Replace("*", @"\*")
                .Replace("[", @"\[")
                .Replace("[", @"\[")
                .Replace("(", @"\(")
                .Replace(")", @"\)")
                .Replace("~", @"\~")
                .Replace("`", @"\`")
                .Replace(">", @"\>")
                .Replace("#", @"\#")
                .Replace("+", @"\+")
                .Replace("-", @"\-")
                .Replace("=", @"\=")
                .Replace("|", @"\|")
                .Replace("{", @"\{")
                .Replace("}", @"\}")
                .Replace("!", @"\!");
    }
}
