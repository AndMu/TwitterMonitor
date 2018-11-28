using System;

namespace Wikiled.Twitter.Persistency
{
    public class PersistencyFactory : IPersistencyFactory
    {
        private Func<FilePersistency> filePersistency;

        private Func<FlatFileSerializer> compressedPersistency;

        public PersistencyFactory(Func<FlatFileSerializer> compressedPersistency, Func<FilePersistency> filePersistency)
        {
            this.compressedPersistency = compressedPersistency;
            this.filePersistency = filePersistency;
        }

        public IPersistency Create(bool compressed)
        {
            return compressed ? compressedPersistency() : (IPersistency)filePersistency();
        }
    }
}
