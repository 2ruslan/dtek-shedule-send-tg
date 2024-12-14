using System.Collections.Generic;

namespace Common
{
    public static class GroupHelper
    {
        public const string AllGroups = "-1";

        public readonly static string[] Groups = 
        {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
        };

        public static Dictionary<int, string> DtekPositions { get; } = new()
        {
            [1] = "1",
            [2] = "2",
            [3] = "3",
            [4] = "4",
            [5] = "5",
            [6] = "6",
        };

        public static Dictionary<string, int> SvitlobotPositions { get; } = new()
        {
            ["1"] = 1,
            ["2"] = 2,
            ["3"] = 3,
            ["4"] = 4,
            ["5"] = 5,
            ["6"] = 6,
        };
    }
}
