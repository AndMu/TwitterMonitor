using ProtoBuf;

namespace Wikiled.Twitter.Persistency.Data
{
    [ProtoContract]
    public class TweetData
    {
        [ProtoMember(1)]
        public double Latitude { get; set; }

        [ProtoMember(2)]
        public double Longitude { get; set; }

        [ProtoMember(3)]
        public string Country { get; set; }

        [ProtoMember(4)]
        public string CountryCode { get; set; }

        [ProtoMember(5)]
        public string Created { get; set; }

        [ProtoMember(6)]
        public string Text { get; set; }

        [ProtoMember(7)]
        public string UserLocation { get; set; }

        [ProtoMember(8)]
        public bool IsRetweet { get; set; }

        [ProtoMember(9)]
        public int RetweetCount { get; set; }

        [ProtoMember(10)]
        public string PlaceType { get; set; }

        [ProtoMember(11)]
        public string Place { get; set; }

        [ProtoMember(12)]
        public string Creator { get; set; }

        [ProtoMember(13)]
        public long CreatorId { get; set; }

        [ProtoMember(14)]
        public long Tick { get; set; }

        [ProtoMember(15)]
        public long RetweetedId { get; set; }

        [ProtoMember(16)]
        public long Id { get; set; }
    }
}
