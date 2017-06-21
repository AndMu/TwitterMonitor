using System.Threading.Tasks;
using Tweetinvi.Models.DTO;

namespace Wikiled.Twitter.Persistency
{
    public interface IPersistency
    {
        void Save(ITweetDTO tweet);
    }
}