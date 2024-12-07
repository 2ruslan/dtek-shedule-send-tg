namespace DtekSheduleSendTg.DTEK
{
    public record PIctureFileInfo
    {
        public string Url { get; set; }
        public string FileName { get; set; }
        public DateOnly OnDate { get; set; }
    }
}
