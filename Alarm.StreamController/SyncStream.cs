using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alarm.StreamController;

public class SyncStream(Stream stream) : Stream
{
    private Lock rLock = new(), wLock = new();

    public override bool CanRead => stream.CanRead;

    public override bool CanSeek => stream.CanSeek;

    public override bool CanWrite => stream.CanWrite;

    public override long Length => stream.Length;

    public override long Position { get => stream.Position; set => stream.Position = value; }

    public override void Flush()
    {
        lock (wLock)
        {
            stream.Flush();
        }
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        lock (rLock)
        {
            return stream.Read(buffer, offset, count);
        }
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        lock (rLock)
        {
            lock (wLock)
            {
                return stream.Seek(offset, origin);
            }
        }
    }

    public override void SetLength(long value)
    {
        lock (rLock)
        {
            lock (wLock)
            {
                stream.SetLength(value);
            }
        }
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        lock (wLock)
        {
            stream.Write(buffer, offset, count);
        }
    }
}
