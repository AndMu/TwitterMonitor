using System.Collections.Generic;

namespace Wikiled.Twitter.Helpers
{
    public static class StringHelper
    {
        public static IEnumerable<string> Chunks(this string str, int chunkSize)
        {
            if (str.Length <= chunkSize)
            {
                yield return str;
                yield break;
            }

            int pageMinSize = 7;
            var size = chunkSize - pageMinSize;
            var total = str.Length / size;
            if (total >= 10)
            {
                pageMinSize++;
            }

            size = chunkSize - pageMinSize;

            for (int i = 0; i < str.Length; i += chunkSize)
            {
                var footerText = $"\r\nPage {page}/{0}"
                yield return str.Substring(i, chunkSize);
            }
        }
    }
}
