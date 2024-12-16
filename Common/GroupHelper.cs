namespace Common
{
    public static class GroupHelper
    {
        public const string AllGroups = "-1";

        public readonly static string[] Groups = 
        {
            "1.1",
            "1.2",
            "2.1",
            "2.2",
            "3.1",
            "3.2",
            "4.1",
            "4.2",
            "5.1",
            "5.2",
            "6.1",
            "6.2",
        };

        public static Dictionary<int, string> DtekPositions { get; } = new()
        {
            [1]  = "1.1",
            [2]  = "1.2",
            [3]  = "2.1",
            [4]  = "2.2",
            [5]  = "3.1",
            [6]  = "3.2",
            [7]  = "4.1",
            [8]  = "4.2",
            [9]  = "5.1",
            [10] = "5.2",
            [11] = "6.1",
            [12] = "6.2",
        };

        public static Dictionary<string, int> SvitlobotPositions { get; } = new()
        {
            ["1.1"] =  1,
            ["1.2"] =  2,
            ["2.1"] =  3,
            ["2.2"] =  4,
            ["3.1"] =  5,
            ["3.2"] =  6,
            ["4.1"] =  7,
            ["4.2"] =  8,
            ["5.1"] =  9,
            ["5.2"] = 10,
            ["6.1"] = 11,
            ["6.2"] = 12,
        };
    }
}
