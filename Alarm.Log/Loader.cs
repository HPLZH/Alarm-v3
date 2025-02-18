using Alarm.Core;
using Alarm.Loader;
using System.Text.Json;

namespace Alarm.Log
{
    public class Loader : ModLoader
    {
        protected override void Load()
        {
            base.Load();
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            RegisterPreHandler("loggers", Logger.FromJson);
            RegisterPreHandler("logger.trace", TraceLogger.FromJson);
            RegisterPreHandler("logger.out.stdout", (json, env) => Logger.LinkStream(json.Deserialize<Logger.LinkConfig>(Configuration.JsonSerializerOptions).upstream, env, Console.OpenStandardOutput()));
            RegisterPreHandler("logger.out.stderr", (json, env) => Logger.LinkStream(json.Deserialize<Logger.LinkConfig>(Configuration.JsonSerializerOptions).upstream, env, Console.OpenStandardError()));
            RegisterPreHandler("logger.out.file", Logger.LinkFileStream);
            RegisterPostHandler("logger.in.playback", PlaybackEventLogger.FromJson);
        }
    }
}
