using System;
using System.IO;

namespace Wikiled.Twitter.Persistency
{
    public interface IStreamSource : IDisposable
    {
        Stream GetStream();
    }
}