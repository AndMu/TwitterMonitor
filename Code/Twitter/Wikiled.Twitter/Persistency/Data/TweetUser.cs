using System;
using ProtoBuf;

namespace Wikiled.Twitter.Persistency.Data
{
    [ProtoContract]
    public class TweetUser
    {
        [ProtoMember(1)]
        public string Name { get; set; }

        [ProtoMember(2)]
        public string Description { get; set; }

        [ProtoMember(3)]
        public DateTime CreatedAt { get; set; }

        [ProtoMember(4)]
        public string Location { get; set; }

        [ProtoMember(5)]
        public bool GeoEnabled { get; set; }

        [ProtoMember(6)]
        public string Url { get; set; }

        [ProtoMember(7)]
        public string Language { get; set; }

        [ProtoMember(8)]
        public string Email { get; set; }

        [ProtoMember(9)]
        public int StatusesCount { get; set; }

        [ProtoMember(10)]
        public int FollowersCount { get; set; }

        [ProtoMember(11)]
        public int FriendsCount { get; set; }

        [ProtoMember(12)]
        public bool Following { get; set; }

        [ProtoMember(13)]
        public bool Protected { get; set; }

        [ProtoMember(14)]
        public bool Verified { get; set; }

        [ProtoMember(15)]
        public bool Notifications { get; set; }

        [ProtoMember(16)]
        public string ProfileImageUrl { get; set; }

        [ProtoMember(17)]
        public bool FollowRequestSent { get; set; }

        [ProtoMember(18)]
        public bool DefaultProfile { get; set; }

        [ProtoMember(19)]
        public int? ListedCount { get; set; }

        [ProtoMember(20)]
        public int? UtcOffset { get; set; }

        [ProtoMember(21)]
        public string TimeZone { get; set; }

        [ProtoMember(22)]
        public long Id { get; set; }
    }
}