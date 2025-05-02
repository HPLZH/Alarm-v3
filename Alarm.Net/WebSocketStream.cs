using System.IO.Pipelines;
using System.Net.WebSockets;

namespace Alarm.Net
{
    public class WebSocketStream : Stream
    {
        readonly WebSocket webSocket;
        public WebSocketMessageType MessageType { get; set; } = WebSocketMessageType.Text;

        readonly Pipe pipe = new();
        readonly Stream reader;
        readonly Stream writer;

        [Obsolete("Read 方法已改用 PipeReader 实现")]
        public bool ReadAsap { get; set; } = false;

        readonly Lock sendLock = new();
        readonly byte[] buffer = new byte[1024];

        public WebSocketStream(WebSocket webSocket)
        {
            this.webSocket = webSocket;
            reader = pipe.Reader.AsStream();
            writer = pipe.Writer.AsStream();
            Task.Run(() =>
            {
                while (webSocket.State == WebSocketState.Connecting)
                {
                    Task.Delay(100).Wait();
                }
                while (webSocket.State == WebSocketState.Open)
                {
                    var r = webSocket.ReceiveAsync(buffer, CancellationToken.None).Result;
                    if (r.Count == 0 && r.EndOfMessage)
                    {
                        Flush();
                    }
                    else
                    {
                        writer.Write(buffer, 0, r.Count);
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

        public override int Read(byte[] buffer, int offset, int count) => reader.Read(buffer, offset, count);

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
