using DtekSheduleSendTg.Abstraction;

namespace DtekSheduleSendTg.DTEK
{
    public record SiteAnalyzerResult 
    { 
        public IList<string> PIctureFiles { get; set; }
        public string Text { get; set; }
    }
}
