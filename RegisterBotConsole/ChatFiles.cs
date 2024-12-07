namespace RegisterBotConsole
{
    internal static class ChatFiles
    {
        private static readonly string kyivChatsFilePath = System.Configuration.ConfigurationManager.AppSettings["KyivChatsFilePath"];
        private static readonly string kyivRegionChatsFilePath = System.Configuration.ConfigurationManager.AppSettings["KyivRegionChatsFilePath"];
        private static readonly string dniproRegionChatsFilePath = System.Configuration.ConfigurationManager.AppSettings["DnyproChatsFilePath"];
        private static readonly string odesaChatsFilePath = System.Configuration.ConfigurationManager.AppSettings["OdesaChatsFilePath"];

        public static string ResolveFileName(string region)
        {
            if (region == "k")
                return kyivChatsFilePath;
            else if (region == "r")
                return kyivRegionChatsFilePath;
            else if (region == "d")
                return dniproRegionChatsFilePath;
            else if (region == "o")
                return odesaChatsFilePath;
            else
                throw new Exception ("Невідомий регіон.");
        }
    }
}
