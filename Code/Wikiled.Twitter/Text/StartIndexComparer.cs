using System.Collections.Generic;

namespace Wikiled.Twitter.Text
{
    /// <summary>
    ///     Compares entities bases on start index.
    /// </summary>
    internal class StartIndexComparer : Comparer<TweetEntity>
    {
        public override int Compare(TweetEntity a, TweetEntity b)
        {
            if(a.Start > b.Start)
            {
                return 1;
            }

            if(a.Start < b.Start)
            {
                return -1;
            }

            return 0;
        }
    }
}