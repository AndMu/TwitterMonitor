using ProtoBuf;

namespace Wikiled.Twitter.Persistency.Data
{
    [ProtoContract]
    public class RawTweetData
    {
        [ProtoMember(1)]
        public byte[] Data { get; set; }
    }
}
