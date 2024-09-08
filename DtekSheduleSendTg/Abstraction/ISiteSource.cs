namespace DtekSheduleSendTg.Abstraction
{
    public interface ISiteSource
    {
        string GetSource();
        string StorePicFromUrl(string url);
    }
}
