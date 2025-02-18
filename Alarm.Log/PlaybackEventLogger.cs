using Alarm.Core;
using System.Diagnostics;
using System.Text.Json;

namespace Alarm.Log
{
    public class PlaybackEventLogger
    {
        public string? start;
        public string? stop;

        public required string id;
        public string[] loggers = [];
        private readonly List<Logger> _loggers = [];

        public void OnStart(object? sender, Player.PlaybackEventArgs e)
        {
            if (start != null)
            {
                foreach (var _logger in _loggers)
                {
                    _logger?.WriteLine(string.Format(start, DateTime.Now, e.File, e.Length, Path.GetFileNameWithoutExtension(e.File)));
                    _logger?.Flush();
                }
            }
        }

        public void OnStop(object? sender, Player.PlaybackEventArgs e)
        {
            if (stop != null)
            {
                foreach (var _logger in _loggers)
                {
                    _logger?.WriteLine(string.Format(stop, DateTime.Now, e.File, e.Length));
                    _logger?.Flush();
                }
            }
        }

        public static void FromJson(JsonElement json, IDictionary<string, object> env)
        {
            var pbloggers = json.Deserialize<PlaybackEventLogger[]>(Configuration.JsonSerializerOptions) ?? [];
            var controller = (Controller)env[Application.NAME_CONTROLLER];

            foreach (var pblogger in pbloggers)
            {
                if (pblogger == null)
                {
                    throw new Exception();
                }

                if (env.ContainsKey(pblogger.id))
                {
                    throw new Exception("标识符已存在");
                }

                foreach (var loggerid in pblogger.loggers)
                {
                    if (env.TryGetValue(loggerid, out var logger))
                    {
                        Debug.Assert(logger is Logger);
                        pblogger._loggers.Add((Logger)logger);
                    }
                    else
                    {
                        throw new KeyNotFoundException();
                    }
                }
                env[pblogger.id] = pblogger;
                controller.PlaybackStarted += pblogger.OnStart;
                controller.PlaybackFinished += pblogger.OnStop;
            }
        }
    }
}
