using DtekSheduleSendTg.Abstraction;
using System.Text.Json;

namespace DtekSheduleSendTg.Data.TextInfo
{
    public class TextInfoRepository(string regionDir) : ITextInfoRepository
    {
        private readonly string InfoFile = Path.Combine(Environment.CurrentDirectory, "WorkDir", regionDir, "TextInfo", "Messages.json");
        private readonly string LastInfoFile = Path.Combine(Environment.CurrentDirectory, "WorkDir", regionDir, "TextInfo", "LastInfoMessage.txt");

        public IEnumerable<TextInfo> GetTextInfo()
            => File.Exists(InfoFile) ? JsonSerializer.Deserialize<List<TextInfo>>(File.ReadAllText(InfoFile)) : new List<TextInfo>();

        public string GetLastInfoMessage()
            => File.Exists(LastInfoFile) ? File.ReadAllText(LastInfoFile) : string.Empty;

        public void StoreLastInfoMessage(string message)
            => File.WriteAllText(LastInfoFile, message);

    }
}
