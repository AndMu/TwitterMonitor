using System.Threading.Tasks;

namespace Wikiled.Twitter.Persistency
{
    public interface IPersistency
    {
        Task Save(string json);
    }
}