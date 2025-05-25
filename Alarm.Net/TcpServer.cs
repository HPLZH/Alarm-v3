using Alarm.Core;
using Alarm.Log;
using Alarm.StreamController;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alarm.Net
{
    public class TcpServer(IPAddress addr, int port, Application app)
    {
        readonly TcpListener listener = new(addr, port);
        readonly Application app = app;
        Logger? logger;
        bool @in, @out = false;

        public void Start()
        {
            listener.Start();
            while (true)
            {
                try
                {
                    OnConnect(listener.AcceptTcpClient());
                }
                catch (Exception ex)
                {
                    Trace.TraceError(ex.ToString());
                }
            }
        }

        public void OnConnect(TcpClient client)
        {
            Stream stream = new SyncStream(client.GetStream());
            if (@out)
            {
                logger?.AddOutput(stream);
            }
            Task.Run(() =>
            {
                try
                {
                    if (@in)
                    {
                        using StreamReader reader = new(stream);
                        using StreamWriter writer = new(stream) { AutoFlush = true };
                        Interpreter interpreter = new(reader, app, writer);
                        interpreter.Execute();
                    }
                    else
                    {
                        while (client.Connected)
                        {
                            stream.ReadByte();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Trace.TraceError(ex.ToString());
                }
                finally
                {
                    logger?.RemoveOutput(stream);
                }
            });
        }

        public static void FromJson(JsonElement json, Application app)
        {
            Config[] confs = json.Deserialize<Config[]>(Configuration.JsonSerializerOptions) ?? [];
            foreach (Config conf in confs)
            {
                TcpServer server = new(IPAddress.Parse(conf.ip), conf.port, app);
                if (conf.linkTo != null)
                {
                    server.@out = true;
                    if (app.TryGetValue(conf.linkTo.Value.upstream, out var o))
                    {
                        server.logger = (Logger)o;
                    }
                    else
                    {
                        throw new KeyNotFoundException();
                    }
                }
                server.@in = conf.asCtrl;
                app.AddMainTask(new Task(server.Start));
                app.AddExitAction(_ => server.listener.Stop());
            }
        }

        internal struct Config()
        {
            public required string ip;
            public required int port;

            [JsonPropertyName("controller.tcp")]
            public bool asCtrl = false;

            [JsonPropertyName("logger.out.tcp")]
            public HttpServer.LinkConfig? linkTo;
        }
    }
}
