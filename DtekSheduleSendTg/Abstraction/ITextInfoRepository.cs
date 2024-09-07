using DtekSheduleSendTg.Data.TextInfo;

namespace DtekSheduleSendTg.Abstraction
{
    public interface ITextInfoRepository
    {
        public IEnumerable<TextInfo> GetTextInfo();

        public string GetLastInfoMessage();

        public void StoreLastInfoMessage(string message);
    }
}
