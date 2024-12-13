using DtekSheduleSendTg.Abstraction;
using DtekSheduleSendTg.Data.PIctureFileInfo;

namespace DtekSheduleSendTg.DTEK
{
    public record SiteAnalyzerResult 
    { 
        public IEnumerable<PIctureFileInfo> PIctureFiles { get; set; }
        public string Text { get; set; }
    }
}
