using DtekSheduleSendTg.Abstraction;

namespace DtekSheduleSendTg.DTEK
{
    public record SiteAnalyzerPictureResult : ISiteAnalyzerResult 
    { 
        public string PIctureFile { get; set; } 
    }
}
