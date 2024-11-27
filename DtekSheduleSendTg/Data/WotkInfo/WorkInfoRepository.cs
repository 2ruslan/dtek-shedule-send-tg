using DtekSheduleSendTg.Abstraction;
using System.Text.Json;

namespace DtekSheduleSendTg.Data.WotkInfo
{
    internal record WorkInfoRepository(string regionDir) : IWorkInfoRepository
    {
        private readonly string repoFile = Path.Combine(Environment.CurrentDirectory, "WorkDir", regionDir, "WorkInfo.json");

        public WorkInfo GetWorkInfo()
            => File.Exists(repoFile) ? JsonSerializer.Deserialize<WorkInfo>(File.ReadAllText(repoFile)) : new WorkInfo();

        public void StoreWorkInfo(WorkInfo workInfo)
            => File.WriteAllText(repoFile, JsonSerializer.Serialize(workInfo, new JsonSerializerOptions { WriteIndented = true }));
    }
}
