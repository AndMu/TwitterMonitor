namespace Wikiled.Twitter.Communication
{
    public interface IPublisher
    {
        void SendPrivate(long userId, string message);

        void PublishMessage(IFeedMessage publishMessage);
    }
}