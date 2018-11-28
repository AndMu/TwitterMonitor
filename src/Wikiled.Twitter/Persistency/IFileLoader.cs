namespace Wikiled.Twitter.Persistency
{
    public interface IFileLoader
    {
        string[] Load(string fileName);
    }
}