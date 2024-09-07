namespace DtekSheduleSendTg.Data.ChatInfo
{
    public record ChatInfo
    {
        public long Id { get; set; }
        public string Caption { get; set; }
        public int Group { get; set; }
    }
}
