using DtekSheduleSendTg.Data.Shedule;

namespace DtekSheduleSendTg.Abstraction
{
    public interface ISheduleRepository
    {
        IEnumerable<SheduleData> GetShedule();

        void StoreShedule(IEnumerable<SheduleData> shedules);
    }
}
