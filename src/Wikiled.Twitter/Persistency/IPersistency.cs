using System.Threading.Tasks;

namespace Wikiled.Twitter.Persistency
{
    public interface IPersistency
    {
        void Save(string json);
    }
}