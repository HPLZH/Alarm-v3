using Alarm.Providers;
using System.Text.Json;

namespace Alarm.Core
{
    public partial class Application
    {
        public static Player BuildPlayer(PlayerInfo info)
        {
            return info.type switch
            {
                "direct" => Player.BuildFromDevice(info.guid ?? Guid.Empty),
                _ => Player.Build(),
            };
        }

        public static VolumeManager ExecuteVolumeConfig(IDictionary<string, int> volume, VolumeManager? volumeManager = null)
        {
            VolumeManager manager = volumeManager ?? VolumeManager.Shared;
            foreach (var (d, v) in volume)
            {
                if (v >= 0)
                    manager.Foreach(manager.SaveAndSet(v / 100f, false), d);
                else if (v == -2)
                    manager.Foreach(manager.SaveAndSetMute(false), d);
            }
            return manager;
        }

        public static Controller BuildController(MainConfig config)
        {
            Player player = BuildPlayer(config.player);
            IEnumerable<string> playlist = config.playlist.GetList();
            IProvider provider = ProviderBuilder.Build(config.provider, ProviderEnd.Instance, playlist);
            Controller controller = new(player, provider);
            return controller;
        }

        public static Application AppInit(Stream utf8stream, Application? initApp = null)
        {
            Application app = initApp ?? [];
            MainConfig mainConfig = JsonSerializer.Deserialize<MainConfig>(utf8stream, Configuration.JsonSerializerOptions);
            app[NAME_MAINCONFIG] = mainConfig;
            foreach (var (name, json) in mainConfig.ext)
            {
                if (configHandlers.TryGetValue(name, out var handler) && handler.isPreHandler)
                {
                    handler.handler.Invoke(json, app);
                }
            }
            app[NAME_CONTROLLER] = BuildController(mainConfig);
            foreach (var (name, json) in mainConfig.ext)
            {
                if (configHandlers.TryGetValue(name, out var handler) && !handler.isPreHandler)
                {
                    handler.handler.Invoke(json, app);
                }
            }
            return app;
        }

        public const string NAME_CONTROLLER = "alarm.core:controller";
        public const string NAME_MAINCONFIG = "alarm.core:config";

        public static readonly Dictionary<string, ConfigHandlerInfo> configHandlers = [];
        public static readonly Dictionary<Type, Action<string, object>> exitHandlers = [];

        public static void Exit(Application app, int code = 0)
        {
            (app[NAME_CONTROLLER] as Controller)?.Stop();
            foreach (var (k, v) in app)
            {
                ApplyExitHandler(v.GetType(), k, v);
            }
            while (app.onExit.TryPop(out var action))
            {
                action(code);
            }
        }

        private static void ApplyExitHandler(Type type, string key, object value)
        {
            if (exitHandlers.TryGetValue(type, out var handler))
            {
                handler(key, value);
            }
            else if (type.BaseType != null)
            {
                ApplyExitHandler(type.BaseType, key, value);
            }
        }
    }

}
