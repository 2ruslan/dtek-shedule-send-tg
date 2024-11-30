﻿namespace RegisterBotConsole
{
    internal class Paser
    {
        readonly char[] separators = { '\t', ' ', '\r', '\n', ',', ';' };
        readonly string[] types = { "k", "r", "d", "o" };
        const string DelOldCommand = "+delold";
        const string AddTextCommand = "+addtext";
        const string PictureOnlyCommand = "+piconly";

        public async Task<PaserResult> Parse(string message) 
        {
            var result = new PaserResult();

            var parts = (message ?? string.Empty).Split(separators, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 3)
                return GetError(result, "Невірний формат повідомлення (повинен скаладатися з трьох(або більше) частин).");

            #region chatId
            long chatId;
            if (!long.TryParse(parts[0].Trim(), out chatId) || chatId > 0)
                return GetError(result, "Перший параметр повинен бути ід групи (від`ємне цифрове значення)");

            result.Id = chatId;
            #endregion chatId

            #region region
            var type = parts[1].Trim().ToLower();
            if (!types.Contains(type))
                return GetError(result, "Другий парметр повинен бути латинська літера що характеризує регіон (див.опис по start).");

            result.Region = type;
            #endregion region

            #region group or delete command
            var part3 = parts[2].Trim().ToLower();

            int group;
            if (part3 == "d")
            {
                result.IsDeleteChatCommand = true;
                return result;
            }
            else if (part3 == "fl" || part3 == "pt" || part3 == "ls")
            {
                var str = message
                    .Replace(parts[0], string.Empty).TrimStart()
                    .Replace(parts[1], string.Empty).TrimStart()
                    .Replace(parts[2], string.Empty)
                    ;

                if (str.Length > 0)
                    str = str.Substring(1);

                if (part3 == "fl")
                {
                    result.HasCaption = true;
                    result.Caption = str == "del" ? string.Empty : str;
                }
                else if (part3 == "pt")
                {
                    result.HasPowerOffLinePattern = true;
                    result.PowerOffLinePattern = str == "del" ? string.Empty : str;
                }
                else if (part3 == "ls")
                {
                    result.HasPowerOffLeadingSymbol = true;
                    result.PowerOffLeadingSymbol = str == "del" ? string.Empty : str;
                }

                return result;
            }
            else if (!int.TryParse(parts[2].Trim(), out group) || group < 1 || group > 6)
                return GetError(result, "Третій параметр повинен бути номером групи відключення (1-6) або додаткова команда");
            else
                result.Group = group;
            #endregion group or delete command

            #region  addCommands
            result.IsDeletePrevMessage = (message ?? string.Empty).Contains(DelOldCommand);
            result.IsSendTextMessage = (message ?? string.Empty).Contains(AddTextCommand);
            result.IsNoSendPictureDescription = (message ?? string.Empty).Contains(PictureOnlyCommand);
            #endregion  addCommands
            
            return result;
        }

        private static PaserResult GetError(PaserResult paserResult, string error)
        { 
            paserResult.Error = error;
            paserResult.HasError = true;
            return paserResult;
        }
    }
}
