namespace Wikiled.Twitter.Persistency
{
    public interface IPersistencyFactory
    {
        IPersistency Create(bool compressed);
    }
}