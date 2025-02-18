using Alarm.Core;
using System.Diagnostics;
using System.Text.Json;

namespace Alarm.Log
{
    public class TraceLogger
    {
        public static readonly Logger Logger = new();
        public static readonly TextWriterTraceListener TraceListener = new(Logger);

        public required string id;

        static TraceLogger()
        {
            Logger.AutoFlush = true;
        }

        public static void FromJson(JsonElement json, IDictionary<string, object> env)
        {
            var trlogger = json.Deserialize<TraceLogger>(Configuration.JsonSerializerOptions) ?? throw new Exception();
            if (env.ContainsKey(trlogger.id))
            {
                throw new Exception("标识符已存在");
            }
            env[trlogger.id] = Logger;
        }
    }
}
