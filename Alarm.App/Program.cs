using Alarm.Core;
using Alarm.Loader;
using Alarm.Log;
using System.CommandLine;
using System.Diagnostics;
using System.Text.Json;

Application env;
Controller controller;
MainConfig mainConfig;

var root = new RootCommand();

var start = new Command("start");
root.Add(start);

var configFile = new Argument<FileInfo>("config-json");
start.Add(configFile);

var mod = new Option<bool>("--mod");
start.Add(mod);

start.SetHandler((config, moden) =>
{
    env = [];
    env.AddExitAction(Environment.Exit);
    TraceLogger.TraceListener.TraceOutputOptions = TraceOptions.DateTime | TraceOptions.ThreadId;
    Trace.AutoFlush = true;
    Trace.Listeners.Add(TraceLogger.TraceListener);
    new Alarm.Providers.Loader().LoadMod();
    new Alarm.Log.Loader().LoadMod();
    new Alarm.StreamController.Loader().LoadMod();
    new Alarm.Net.Loader().LoadMod();
    if (moden)
    {
        using var f0 = config.OpenRead();
        var modconf = JsonSerializer.Deserialize<ModLoader.Config>(f0, Configuration.JsonSerializerOptions);
        if (modconf.mods != null)
        {
            foreach (var modfile in modconf.mods)
            {
                var modinfo = new FileInfo(modfile);
                ModLoader.LoadFile(modinfo);
            }
        }
    }
    using (var f = config.OpenRead())
    {
        env = Application.AppInit(f, env);
    }
    controller = (Controller)env[Application.NAME_CONTROLLER];
    mainConfig = (MainConfig)env[Application.NAME_MAINCONFIG];
    Application.ExecuteVolumeConfig(mainConfig.player);
    controller.Play();
    env.StartMainTasks();
    while (true)
    {
        Thread.Sleep(10000);
    }
}, configFile, mod);

root.Invoke(args);