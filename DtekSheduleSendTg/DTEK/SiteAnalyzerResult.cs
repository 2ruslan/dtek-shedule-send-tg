using DtekSheduleSendTg.Abstraction;

namespace DtekSheduleSendTg.DTEK
{
    public record SiteAnalyzerResult //: ISiteAnalyzerResult 
    { 
        public string PIctureFile { get; set; }
        public string Text { get; set; }
    }
}
