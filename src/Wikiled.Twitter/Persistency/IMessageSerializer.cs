namespace Wikiled.Twitter.Persistency
{
    public interface IMessageSerializer
    {
        void Save(string json);
    }
}