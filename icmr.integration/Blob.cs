using System;
using System.IO;

namespace Icmr.Integration
{
    public class Blob : IDisposable
    {
        public readonly string ContentType;
        public readonly Stream Stream;

        public Blob(string contentType, Stream stream)
        {
            this.ContentType = contentType;
            this.Stream = stream;
        }

        public void Dispose()
        {
            this.Stream.Dispose();
        }
    }
}

