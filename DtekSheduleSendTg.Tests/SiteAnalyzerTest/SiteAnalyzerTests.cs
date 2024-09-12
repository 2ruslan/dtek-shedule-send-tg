using DtekSheduleSendTg.Abstraction;
using DtekSheduleSendTg.Data.TextInfo;
using DtekSheduleSendTg.DTEK;
using Microsoft.Extensions.Logging;

namespace DtekSheduleSendTg.Tests.SiteAnalyzerTest
{
    public class SiteAnalyzerTests
    {
        class TestData : List<object[]>
        {
#pragma warning disable S1144 // Unused private types or members should be removed
            public TestData() {
                Add(new Object[] {
                    Path.Combine(Environment.CurrentDirectory, "SiteAnalyzerTest", "SiteSource", "SourceNoPowerOff.txt"),
                    "За наказом НЕК Укренерго стабілізаційні відключення на 8 вересня<strong> не заплановані</strong>." }
                    );
                Add(new Object[] {
                    Path.Combine(Environment.CurrentDirectory, "SiteAnalyzerTest", "SiteSource", "SourcePowerOffNoPict.txt"),
                    "За наказом НЕК Укренерго <strong>сьогодні з 13:00 до 23:00</strong> будуть діяти <strong>стабілізаційні відключення</strong> електроенергії." }
                    );
            }
#pragma warning restore S1144 // Unused private types or members should be removed
        }


        [Theory]
        [ClassData(typeof(TestData))]
        public void AnalyzeTestTextNoPowerOff(string fileSite, string expected)
        {
            // Arrange
            
            var textInfo = new List<TextInfo>()
            {
                new TextInfo ()
                {
                    Regex =  @"(За наказом НЕК Укренерго).+?(\.)",
                    Message = "*"
                }
            };

            var logger = new Mock<ILogger>();

            var repo = new Mock<ITextInfoRepository>();
            repo.Setup(x => x.GetTextInfo()).Returns(textInfo);
            repo.Setup(x => x.GetLastInfoMessage()).Returns("");

            var siteSource = new Mock<ISiteSource>();
            siteSource.Setup(x => x.GetSource()).Returns(File.ReadAllText(fileSite));

            var siteAnalyzer = new SiteAnalyzer(logger.Object, repo.Object, siteSource.Object);

            // Act
            var analyzeResult = siteAnalyzer.Analyze();

            // Assert
            Assert.IsType<SiteAnalyzerTextResult>(analyzeResult);

            var textResult = analyzeResult as SiteAnalyzerTextResult;

            Assert.Contains(expected, textResult?.Text);
        }

    }
}
