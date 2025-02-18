using System.Net.WebSockets;

namespace Alarm.Net
{
    public class WebSocketStream : Stream
    {
        readonly WebSocket webSocket;
        public WebSocketMessageType MessageType { get; set; } = WebSocketMessageType.Text;
        public bool ReadAsap { get; set; } = false;

        readonly Lock sendLock = new();
        readonly Lock recvLock = new();
        readonly byte[] buffer = new byte[1024];
        int offset = 0;
        int tail = 0;

        public WebSocketStream(WebSocket webSocket)
        {
            this.webSocket = webSocket;
            Task.Run(() =>
            {
                while (webSocket.State == WebSocketState.Connecting)
                {
                    Task.Delay(100).Wait();
                }
                while (CanRead)
                {
                    if (tail < buffer.Length)
                    {
                        lock (recvLock)
                        {
                            ArraySegment<byte> arr = new(buffer, tail, buffer.Length - tail);
                            var r = webSocket.ReceiveAsync(arr, CancellationToken.None).Result;
                            if (r.Count == 0)
                            {
                                Write([]);
                            }
                            else
                            {
                                tail += r.Count;
                            }
                        }
                    }
                    else
                    {
                        Task.Delay(80).Wait();
                    }
                }
            });
        }

        public override bool CanRead => webSocket.State == WebSocketState.Open;

        public override bool CanSeek => false;

        public override bool CanWrite => CanRead;

        public override long Length => throw new NotSupportedException();

        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        public override void Flush()
        {
            lock (sendLock)
            {
                ReadOnlyMemory<byte> memory = new([]);
                webSocket.SendAsync(memory, MessageType, true, CancellationToken.None).AsTask().Wait();
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int wcount = 0;
            while (CanRead && (ReadAsap ? wcount == 0 : wcount < count))
            {
                if (this.tail - this.offset > 0)
                {
                    lock (recvLock)
                    {
                        int cpn = Math.Min(this.tail - this.offset, count - wcount);
                        Array.Copy(this.buffer, offset, buffer, offset + wcount, cpn);
                        this.offset += cpn;
                        wcount += cpn;
                    }
                }
                else
                {
                    Task.Delay(60).Wait();
                }
            }
            return wcount;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            lock (sendLock)
            {
                ReadOnlyMemory<byte> memory = new(buffer, offset, count);
                webSocket.SendAsync(memory, MessageType, false, CancellationToken.None).AsTask().Wait();
            }
        }
    }
}
