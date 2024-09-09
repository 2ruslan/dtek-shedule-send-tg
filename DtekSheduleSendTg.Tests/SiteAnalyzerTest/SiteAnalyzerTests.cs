using DtekSheduleSendTg.Abstraction;
using DtekSheduleSendTg.Data.TextInfo;
using DtekSheduleSendTg.DTEK;
using Microsoft.Extensions.Logging;

namespace DtekSheduleSendTg.Tests.SiteAnalyzerTest
{
    public class SiteAnalyzerTests
    {
        [Fact]
        public void AnalyzeTestTextNoPowerOff()
        {
            // Arrange
            var fileSource = Path.Combine(Environment.CurrentDirectory, "SiteAnalyzerTest", "SiteSource", "SourceNoPowerOff.txt");

            var textInfo = new List<TextInfo>()
            {
                new TextInfo ()
                {
                    Regex =  @"(За наказом НЕК Укренерго стабілізаційні відключення).+?(не заплановані).+?(\.)",
                    Message = "*"
                }
            };
            
            var logger = new Mock<ILogger>();
            
            var repo = new Mock<ITextInfoRepository>();
            repo.Setup(x => x.GetTextInfo()).Returns(textInfo);
            repo.Setup(x => x.GetLastInfoMessage()).Returns("");

            var siteSource = new Mock<ISiteSource>();
            siteSource.Setup(x => x.GetSource()).Returns(File.ReadAllText(fileSource));

            var siteAnalyzer = new SiteAnalyzer(logger.Object, repo.Object, siteSource.Object);
            
            // Act
            var analyzeResult = siteAnalyzer.Analyze();

            // Assert
            Assert.IsType<SiteAnalyzerTextResult>(analyzeResult);

            var textResult = analyzeResult as SiteAnalyzerTextResult;

            Assert.Contains("За наказом НЕК Укренерго стабілізаційні відключення на 8 вересня не заплановані.", textResult?.Text);
        }
    }
}
