using DtekSheduleSendTg.Abstraction;
using DtekSheduleSendTg.Data.Shedule;
using Microsoft.Extensions.Logging;

namespace DtekSheduleSendTg.Tests.DtekSheduleTests
{
    public class DtekSheduleTests
    {
        [Fact]
        public void TestPaseFileA()
        {
            const string group1 = "Group 1";
            const string group2 = "Group 2";
            const string group3 = "Group 3";
            const string group4 = "Group 4";
            const string group5 = "Group 5";
            const string group6 = "Group 6";

            // Arrange
            var logger = new Mock<ILogger>();
            var repo = new Mock<ISheduleRepository>();
            var dtekShedule = new DtekShedule(logger.Object, repo.Object);

            // Act
            var file = Path.Combine(Environment.CurrentDirectory, "DtekSheduleTest", "TestData", "fileA.jpg");
            dtekShedule.AnalyzeFile(Path.Combine(Environment.CurrentDirectory, file));

            var description1 = dtekShedule.GetFullPictureDescription(1, group1);
            var description2 = dtekShedule.GetFullPictureDescription(2, group2);
            var description3 = dtekShedule.GetFullPictureDescription(3, group3);
            var description4 = dtekShedule.GetFullPictureDescription(4, group4);
            var description5 = dtekShedule.GetFullPictureDescription(5, group5);
            var description6 = dtekShedule.GetFullPictureDescription(6, group6);
            // Assert

            Assert.Contains(group1, description1);
            Assert.Contains("15 - 18", description1);

            Assert.Contains(group2, description2);
            Assert.Contains("6 - 9", description2);
            Assert.Contains("15 - 18", description2);

            Assert.Contains(group3, description3);
            Assert.Contains("0 - 3", description3);
            Assert.Contains("18 - 21", description3);

            Assert.Contains(group4, description4);
            Assert.Contains("0 - 1", description4);
            Assert.Contains("9 - 12", description4);
            Assert.Contains("18 - 21", description4);

            Assert.Contains(group5, description5);
            Assert.Contains("3 - 6", description5);
            Assert.Contains("12 - 15", description5);
            Assert.Contains("21 - 22", description5);

            Assert.Contains(group6, description6);
            Assert.Contains("3 - 4", description6);
            Assert.Contains("12 - 13", description6);
            Assert.Contains("21 - 24", description6);
        }

        [Fact]
        public void TestParceFileB()
        {
            const string group1 = "Group 1";
            const string group2 = "Group 2";
            const string group3 = "Group 3";
            const string group4 = "Group 4";
            const string group5 = "Group 5";
            const string group6 = "Group 6";

            var previosShedules = new List<SheduleData>()
                {
                    new SheduleData (){ Group = 1, SheduleString = "111111111111000000111000" },
                    new SheduleData (){ Group = 2, SheduleString = "111111111111111111111111" },
                };

            // Arrange
            var logger = new Mock<ILogger>();
            var repo = new Mock<ISheduleRepository>();
            repo.Setup(x => x.GetShedule()).Returns(previosShedules);

            var dtekShedule = new DtekShedule(logger.Object, repo.Object);

            // Act
            var file = Path.Combine(Environment.CurrentDirectory, "DtekSheduleTest", "TestData", "fileB.jpg");
            dtekShedule.AnalyzeFile(Path.Combine(Environment.CurrentDirectory, file));

            var description1 = dtekShedule.GetFullPictureDescription(1, group1);
            var description2 = dtekShedule.GetFullPictureDescription(2, group2);
            var description3 = dtekShedule.GetFullPictureDescription(3, group3);
            var description4 = dtekShedule.GetFullPictureDescription(4, group4);
            var description5 = dtekShedule.GetFullPictureDescription(5, group5);
            var description6 = dtekShedule.GetFullPictureDescription(6, group6);

            var noSendGroup1 = dtekShedule.IsNoSendPicture2Group(1);
            var noSendGroup2 = dtekShedule.IsNoSendPicture2Group(2);
            var noSendGroup3 = dtekShedule.IsNoSendPicture2Group(3);

            // Assert

            Assert.True(noSendGroup1);
            Assert.False(noSendGroup2);
            Assert.False(noSendGroup3);

            Assert.Contains(group1, description1);
            Assert.Contains("12 - 18", description1);
            Assert.Contains("21 - 24", description1);

            Assert.Contains(group2, description2);
            Assert.Contains("3 - 6", description2);
            Assert.Contains("12 - 15", description2);
            Assert.Contains("21 - 24", description2);

            Assert.Contains(group3, description3);
            Assert.Contains("6 - 9", description3);
            Assert.Contains("15 - 21", description3);

            Assert.Contains(group4, description4);
            Assert.Contains("6 - 9", description4);
            Assert.Contains("15 - 18", description4);
            
            Assert.Contains(group5, description5);
            Assert.Contains("0 - 3", description5);
            Assert.Contains("9 - 12", description5);
            Assert.Contains("18 - 21", description5);

            Assert.Contains(group6, description6);
            Assert.Contains("9 - 12", description6);
            Assert.Contains("18 - 22", description6);
        }
    }
}