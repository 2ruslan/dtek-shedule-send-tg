
using System.Text.Json;

namespace DtekSheduleSendTg.Data.PIctureFileInfo
{
    internal static class PIctureFileInfoRepository
    {
        public static PIctureFileInfo GetPIctureFileInfo(string fileName)
            => File.Exists(fileName) ? JsonSerializer.Deserialize<PIctureFileInfo>(File.ReadAllText(fileName)) : null;

        public static void StorePIctureFileInfo(string fileName, PIctureFileInfo info)
            => File.WriteAllText(fileName, JsonSerializer.Serialize(info, new JsonSerializerOptions { WriteIndented = true }));
    }
}
