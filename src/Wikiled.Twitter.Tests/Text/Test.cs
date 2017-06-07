using System.Collections.Generic;

namespace Wikiled.Twitter.Tests.Text
{
    internal class Test<TExpected>
    {
        public string Description { get; set; }

        public string Text { get; set; }

        public string Json { get; set; }

        public TExpected Expected { get; set; }

        public List<List<int>> Hits { get; set; }
    }
}