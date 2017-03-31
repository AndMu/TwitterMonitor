using System.Text.RegularExpressions;

namespace Wikiled.Twitter.Text
{
    /// <summary>
    /// </summary>
    public class TweetEntity
    {
        /// <summary>
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="value"></param>
        /// <param name="listSlug"></param>
        /// <param name="type"></param>
        public TweetEntity(int start, int end, string value, string listSlug, TweetEntityType type)
        {
            Start = start;
            End = end;
            Value = value;
            ListSlug = listSlug;
            Type = type;
        }

        /// <summary>
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="value"></param>
        /// <param name="type"></param>
        public TweetEntity(int start, int end, string value, TweetEntityType type)
            : this(start, end, value, null, type)
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="matcher"></param>
        /// <param name="type"></param>
        /// <param name="groupNumber"></param>
        public TweetEntity(Match matcher, TweetEntityType type, int groupNumber)
            : this(matcher, type, groupNumber, -1) // Offset -1 on start index to include @, # symbols for mentions and hashtags
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="matcher"></param>
        /// <param name="type"></param>
        /// <param name="groupNumber"></param>
        /// <param name="startOffset"></param>
        public TweetEntity(Match matcher, TweetEntityType type, int groupNumber, int startOffset)
            : this(matcher.Groups[groupNumber].Index + startOffset, matcher.Groups[groupNumber].Index + matcher.Groups[groupNumber].Length, matcher.Groups[groupNumber].Value, type)
        {
        }

        /// <summary>
        /// </summary>
        public string DisplayUrl { get; set; }

        /// <summary>
        /// </summary>
        public int End { get; }

        /// <summary>
        /// </summary>
        public string ExpandedUrl { get; set; }

        /// <summary>
        ///     listSlug is used to store the list portion of @mention/list.
        /// </summary>
        public string ListSlug { get; }

        /// <summary>
        /// </summary>
        public int Start { get; }

        /// <summary>
        /// </summary>
        public TweetEntityType Type { get; }

        /// <summary>
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if(this == obj)
            {
                return true;
            }

            if(!(obj is TweetEntity))
            {
                return false;
            }

            TweetEntity other = (TweetEntity)obj;

            if(Type.Equals(other.Type) &&
               Start == other.Start &&
               End == other.End &&
               Value.Equals(other.Value))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Type.GetHashCode() + Value.GetHashCode() + Start + End;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{Value}({Type})[{Start},{End}]";
        }
    }
}
