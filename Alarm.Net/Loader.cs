using Alarm.Loader;

namespace Alarm.Net
{
    public class Loader : ModLoader
    {
        protected override void Load()
        {
            base.Load();
            RegisterPostHandler("server.http", new(HttpServer.FromJson, typeof(HttpServer.Config[])));
            RegisterPostHandler("server.tcp", new(TcpServer.FromJson, typeof(TcpServer.Config[])));
            RegisterPostHandler("server.websocket", new(WebSocketServer.FromJson, typeof(WebSocketServer.Config[])));
        }
    }
}
