using Alarm.Core;
using Alarm.Log;
using Alarm.StreamController;
using System.Diagnostics;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alarm.Net
{
    public class HttpServer(Application app)
    {
        readonly Application app = app;
        readonly HttpListener listener = new()
        {
            IgnoreWriteExceptions = true
        };
        bool get, post = false;
        readonly MemoryStream stream = new();


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
            await Task.Run(() =>
            {
                Debug.WriteLine($"Http: {context.Request.HttpMethod} from {context.Request.LocalEndPoint.Address}");
                switch (context.Request.HttpMethod)
                {
                    case "GET":
                        if (get)
                        {
                            context.Response.StatusCode = 200;
                            context.Response.ContentType = "text/plain;charset=utf-8";
                            context.Response.OutputStream.Write(stream.ToArray());
                            context.Response.Close();
                            return;
                        }
                        break;
                    case "POST":
                        if (post)
                        {
                            using var instream = new MemoryStream();
                            context.Request.InputStream.CopyTo(instream);
                            instream.Seek(0, SeekOrigin.Begin);
                            using var reader = new StreamReader(instream, context.Request.ContentEncoding);
                            context.Response.StatusCode = 204;
                            context.Response.Close();
                            new Interpreter(reader, app).Execute();
                            return;
                        }
                        break;
                    default:
                        break;
                }
                context.Response.StatusCode = 405;
                context.Response.AddHeader("Allow", get ? post ? "GET, POST" : "GET" : post ? "POST" : "");
                context.Response.Close();
            });
        }

        public static void FromJson(JsonElement json, Application app)
        {
            var confs = json.Deserialize<Config[]>(Configuration.JsonSerializerOptions) ?? [];
            foreach (var conf in confs)
            {
                HttpServer server = new(app);
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
                        ((Logger)o).AddOutput(server.stream);
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

        private struct Config
        {
            public string[] prefixes;

            [JsonPropertyName("controller.http")]
            public bool asCtrl;

            [JsonPropertyName("logger.out.http")]
            public LinkConfig? linkTo;
        }

        internal struct LinkConfig
        {
            public required string upstream;
        }
    }
}
