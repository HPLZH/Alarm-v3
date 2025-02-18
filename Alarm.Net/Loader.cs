using Alarm.Loader;

namespace Alarm.Net
{
    public class Loader : ModLoader
    {
        protected override void Load()
        {
            base.Load();
            RegisterPostHandler("server.http", HttpServer.FromJson);
            RegisterPostHandler("server.tcp", TcpServer.FromJson);
            RegisterPostHandler("server.websocket", WebSocketServer.FromJson);
        }
    }
}
