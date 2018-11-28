using System;

namespace Wikiled.Twitter.Persistency
{
    public class TimingStreamConfig
    {
        public TimingStreamConfig(string path, TimeSpan fileCreation)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
            FileCreation = fileCreation;
        }

        public string Path { get; }

        public TimeSpan FileCreation { get; }
    }
}
