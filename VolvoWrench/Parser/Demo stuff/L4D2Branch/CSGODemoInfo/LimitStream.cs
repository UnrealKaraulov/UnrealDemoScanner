using System;
using System.IO;

namespace DemoScanner.DemoStuff.L4D2Branch.CSGODemoInfo
{
    public class LimitStream : Stream
    {
        private const int TrashSize = 4096;
        private static readonly byte[] Dignitrash = new byte[TrashSize];
        private readonly Stream Underlying;
        private long _Position;

        public LimitStream(Stream underlying, long length)
        {
            if (!underlying.CanRead) throw new NotImplementedException();

            if (length <= 0) throw new ArgumentException("length");

            Underlying = underlying;
            Length = length;
            _Position = 0;
        }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length { get; }

        public override long Position
        {
            get => _Position;
            set => throw new NotImplementedException();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                var remaining = Length - _Position;
                if (Underlying.CanSeek)
                    Underlying.Seek(remaining, SeekOrigin.Current);
                else
                    while (remaining > 0)
                    {
                        Underlying.Read(Dignitrash, 0, checked((int) Math.Min(TrashSize, remaining)));
                        remaining -= TrashSize; // could go beyond 0, but it's signed so who cares
                    }
            }
        }

        public override void Flush()
        {
            Underlying.Flush();
        }

        public byte[] ReadBytes(int count)
        {
            var data = new byte[count];
            var offset = 0;
            while (offset < count)
            {
                var thisTime = Read(data, offset, count - offset);
                if (thisTime == 0) throw new EndOfStreamException();

                offset += thisTime;
            }

            return data;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            count = checked((int) Math.Min(count, Length - _Position)); // should never throw (count <= int_max)
            var ret = Underlying.Read(buffer, offset, count);
            _Position += ret;
            return ret;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}