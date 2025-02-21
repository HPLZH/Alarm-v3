using Alarm.Core;
using Alarm.Log;
using Alarm.StreamController;
using System.Diagnostics;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alarm.Net
{
    public class WebSocketServer(Application app)
    {
        readonly Application app = app;
        readonly HttpListener listener = new()
        {
            IgnoreWriteExceptions = true
        };
        bool get, post = false;
        Logger? logger;

        public void RunServer()
        {
            listener.Start();
            while (listener.IsListening)
            {
                try
                {
                    ContextHandler(listener.GetContext());
                }
                catch (Exception ex)
                {
                    Trace.TraceError(ex.ToString());
                }
            }
        }

        public async void ContextHandler(HttpListenerContext context)
        {
            await Task.Run(async () =>
            {
                if (!context.Request.IsWebSocketRequest)
                {
                    context.Response.StatusCode = 426;
                    context.Response.AddHeader("Upgrade", "websocket");
                    context.Response.OutputStream.Write(Encoding.UTF8.GetBytes("This is a websocket server."));
                    context.Response.Close();
                    return;
                }
                WebSocketContext wsContext;
                try
                {
                    wsContext = await context.AcceptWebSocketAsync(null);
                }
                catch (Exception ex)
                {
                    Trace.TraceError(ex.ToString());
                    return;
                }
                WebSocket webSocket = wsContext.WebSocket;
                WebSocketStream stream = new(webSocket)
                {
                    ReadAsap = true
                };
                if (get)
                {
                    logger?.AddOutput(stream);
                }
                try
                {
                    if (post)
                    {
                        StreamReader reader = new(stream);
                        Interpreter interpreter = new(reader, app);
                        interpreter.Execute();
                    }
                    else
                    {
                        while (stream.CanRead)
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
            var confs = json.Deserialize<Config[]>(Configuration.JsonSerializerOptions) ?? [];
            foreach (var conf in confs)
            {
                WebSocketServer server = new(app);
                foreach (var prefix in conf.prefixes)
                {
                    server.listener.Prefixes.Add(prefix);
                }
                server.post = conf.asCtrl;
                if (conf.linkTo is not null)
                {
                    server.get = true;
                    if (app.TryGetValue(conf.linkTo.Value.upstream, out var o))
                    {
                        server.logger = (Logger)o;
                    }
                    else
                    {
                        throw new KeyNotFoundException();
                    }
                }
                app.AddMainTask(new Task(server.RunServer));
                app.AddExitAction(_ => server.listener.Stop());
            }
        }

        internal struct Config()
        {
            public string[] prefixes = [];

            [JsonPropertyName("controller.websocket")]
            public bool asCtrl = false;

            [JsonPropertyName("logger.out.websocket")]
            public HttpServer.LinkConfig? linkTo;
        }
    }
}
