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
            RegisterPreHandler("loggers", new(Logger.FromJson, typeof(Logger.Config)));
            RegisterPreHandler("logger.trace", new(TraceLogger.FromJson, typeof(TraceLogger)));
            RegisterPreHandler("logger.out.stdout", new((json, env) => Logger.LinkStream(json.Deserialize<Logger.LinkConfig>(Configuration.JsonSerializerOptions).upstream, env, Console.OpenStandardOutput()), typeof(Logger.LinkConfig)));
            RegisterPreHandler("logger.out.stderr", new((json, env) => Logger.LinkStream(json.Deserialize<Logger.LinkConfig>(Configuration.JsonSerializerOptions).upstream, env, Console.OpenStandardError()), typeof(Logger.LinkConfig)));
            RegisterPreHandler("logger.out.file", new(Logger.LinkFileStream, typeof(Logger.LinkFileConfig)));
            RegisterPostHandler("logger.in.playback", new(PlaybackEventLogger.FromJson, typeof(PlaybackEventLogger)));
        }
    }
}
