using DtekSheduleSendTg.Abstraction;
using System.Text.Json;

namespace DtekSheduleSendTg.Data.Shedule
{
    public class SheduleRepository (string regionDir) : ISheduleRepository
    {
        private readonly string SheduleFile = Path.Combine(Environment.CurrentDirectory, "WorkDir", regionDir, "Shedule", "SheduleLast.json");

        public IEnumerable<SheduleData> GetShedule()
            => File.Exists(SheduleFile) 
                ? JsonSerializer.Deserialize<List<SheduleData>>(File.ReadAllText(SheduleFile))
                : new List<SheduleData>();

        public void StoreShedule(IEnumerable<SheduleData> shedules)
            => File.WriteAllText(SheduleFile, JsonSerializer.Serialize(shedules, new JsonSerializerOptions { WriteIndented = true }));
    }
}
