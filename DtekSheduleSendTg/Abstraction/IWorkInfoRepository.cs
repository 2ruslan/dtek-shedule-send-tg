using DtekSheduleSendTg.Data.WotkInfo;

namespace DtekSheduleSendTg.Abstraction
{
    public interface IWorkInfoRepository
    {
        WorkInfo GetWorkInfo();

        void StoreWorkInfo(WorkInfo workInfo);
    }
}
