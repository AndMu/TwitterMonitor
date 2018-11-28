using System;
using System.Threading.Tasks;
using Tweetinvi.Models;
using Wikiled.Redis.Persistency;
using Wikiled.Twitter.Persistency.Data;

namespace Wikiled.Twitter.Persistency
{
    public interface IRedisPersistency : IRepository
    {
        bool ResolveRetweets { get; set; }

        Task<long> Count(string keyName);

        Task<long> CountUser();

        IObservable<MessageItem> LoadAll(string index = "All", long begin = 0, long end = -1);

        IObservable<UserItem> LoadAllUsers(int begin = 0, int end = -1);

        Task<TweetData> LoadMessage(long id);

        Task<UserItem> LoadUser(long id);

        Task Save(ITweet tweet);


        Task SaveUser(IUser user);

        Task SaveUser(TweetUser user);
    }
}