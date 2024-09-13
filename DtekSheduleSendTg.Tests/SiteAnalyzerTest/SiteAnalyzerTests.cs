using DtekSheduleSendTg.Abstraction;
using DtekSheduleSendTg.Data.TextInfo;
using DtekSheduleSendTg.DTEK;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace DtekSheduleSendTg.Tests.SiteAnalyzerTest
{
    public class SiteAnalyzerTests
    {
        class AnalyzeTestRegexTestData : List<object[]>
        {
#pragma warning disable S1144 // Unused private types or members should be removed
            public AnalyzeTestRegexTestData() {
                Add(new Object[] {
                    Path.Combine(Environment.CurrentDirectory, "SiteAnalyzerTest", "SiteSource", "SourceNoPowerOff.txt"),
                    "За наказом НЕК Укренерго стабілізаційні відключення на 8 вересня<strong> не заплановані</strong>.",
                    @"(За наказом НЕК Укренерго).+?(\.)"
                    });
                Add(new Object[] {
                    Path.Combine(Environment.CurrentDirectory, "SiteAnalyzerTest", "SiteSource", "SourcePowerOffNoPict.txt"),
                    "За наказом НЕК Укренерго <strong>сьогодні з 13:00 до 23:00</strong> будуть діяти <strong>стабілізаційні відключення</strong> електроенергії." ,
                    @"(За наказом НЕК Укренерго).+?(\.)"
                    });
                Add(new Object[] {
                    Path.Combine(Environment.CurrentDirectory, "SiteAnalyzerTest", "SiteSource", "SourceNoPowerOff_v2.txt"),
                    "За наказом НЕК Укренерго стабілізаційні відключення на 13 вересня <strong>не заплановані." ,
                    @"(За наказом НЕК Укренерго).+?(\.)"
                    });
            }
#pragma warning restore S1144 // Unused private types or members should be removed
        }


        [Theory]
        [ClassData(typeof(AnalyzeTestRegexTestData))]
        public void AnalyzeGetTextRegex(string fileSite, string expected, string regex)
        {
            // Arrange
            
            var textInfo = new List<TextInfo>()
            {
                new TextInfo ()
                {
                    Regex =  regex,
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


        [Fact]
        public void AnalyzeGetPict()
        {
            var fileSite = Path.Combine(Environment.CurrentDirectory, "SiteAnalyzerTest", "SiteSource", "SourcePowerOffWithPict.txt");

            var logger = new Mock<ILogger>();

            var repo = new Mock<ITextInfoRepository>();

            var siteSource = new Mock<ISiteSource>();
            siteSource.Setup(x => x.GetSource()).Returns(File.ReadAllText(fileSite));
            siteSource.Setup(x => x.StorePicFromUrl(It.IsAny<string>())).Returns<string>(x=> x);

            var siteAnalyzer = new SiteAnalyzer(logger.Object, repo.Object, siteSource.Object);

            // Act
            var analyzeResult = siteAnalyzer.Analyze();

            // Assert
            Assert.IsType<SiteAnalyzerPictureResult>(analyzeResult);

            var pictResult = analyzeResult as SiteAnalyzerPictureResult;

            Assert.Contains(@"/media/page/page-chart-8670-1050.jpg", pictResult?.PIctureFile);
        }
    }
}
