using Alarm.Core;
using Alarm.Loader;
using Alarm.Log;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.Text.Json;

Application env;
Controller controller;
MainConfig mainConfig;

AppDomain.CurrentDomain.UnhandledException += (object sender, UnhandledExceptionEventArgs e) =>
{
    Trace.TraceError(e.ExceptionObject.ToString());
};

var LoadModules = () =>
{
    new Alarm.Providers.Loader().LoadMod();
    new Alarm.Log.Loader().LoadMod();
    new Alarm.StreamController.Loader().LoadMod();
    new Alarm.Net.Loader().LoadMod();
};

var LoadExternalMods = (Stream f) =>
{
    var modconf = JsonSerializer.Deserialize<ModLoader.Config>(f, Configuration.JsonSerializerOptions);
    if (modconf.mods != null)
    {
        foreach (var modfile in modconf.mods)
        {
            var modinfo = new FileInfo(modfile);
            ModLoader.LoadFile(modinfo);
        }
    }
};

var root = new RootCommand();

var start = new Command("start", "启动闹钟");
start.AddAlias("x");
root.Add(start);

var test = new Command("test", "测试配置文件");
test.AddAlias("t");
root.Add(test);

var schema = new Command("schema", "生成配置文件的 Json Schema");
root.Add(schema);

var configFile = new Argument<FileInfo>("config.json", "配置文件路径");
start.Add(configFile);
test.Add(configFile);

var conf = new Option<FileInfo?>("--config", "配置文件路径");
conf.AddAlias("-C");
schema.Add(conf);

var outfile = new Option<FileInfo?>("--output", "导出文件路径");
schema.Add(outfile);

var mod = new Option<bool>("--mod", "加载外部 Mod");
mod.AddAlias("-M");
root.AddGlobalOption(mod);

var clean = new Option<bool>("--clean", "禁用内置 Mod");
clean.AddAlias("--cl");
root.AddGlobalOption(clean);

var device = new Command("device", "关于音频输出设备");
device.AddAlias("d");
root.Add(device);

var deviceList = new Command("list", "列出所有设备");
deviceList.AddAlias("ls");
device.Add(deviceList);

start.SetHandler((config, moden, cl) =>
{
    env = [];
    env.AddExitAction(Environment.Exit);
    if (!cl)
    {
        TraceLogger.TraceListener.TraceOutputOptions = TraceOptions.DateTime;
        Trace.AutoFlush = true;
        Trace.Listeners.Add(TraceLogger.TraceListener);
        LoadModules();
    }
    if (moden)
    {
        using var f0 = config.OpenRead();
        LoadExternalMods(f0);
    }
    using (var f = config.OpenRead())
    {
        env = Application.AppInit(f, env);
    }
    controller = (Controller)env[Application.NAME_CONTROLLER];
    mainConfig = (MainConfig)env[Application.NAME_MAINCONFIG];
    VolumeManager volumeManager = Application.ExecuteVolumeConfig(mainConfig.GetVolumeInfo());
    env.AddExitAction(_ => volumeManager.Foreach(volumeManager.Restore));
    controller.Play();
    env.StartMainTasks();
    while (true)
    {
        Thread.Sleep(10000);
    }
}, configFile, mod, clean);

test.SetHandler((config, moden, cl) =>
{
    Environment.SetEnvironmentVariable("ALARM_TEST", "TEST");
    try
    {
        env = [];
        env.AddExitAction(Environment.Exit);
        env.AddExitAction(c => Console.WriteLine($"Exit Code: {c} (0x{c:x8})"));
        env.AddExitAction(c => { if (c == 0) Console.WriteLine("测试通过"); else Console.WriteLine("测试不通过"); });
        if (!cl)
        {
            LoadModules();
        }
        if (moden)
        {
            using var f0 = config.OpenRead();
            LoadExternalMods(f0);
        }
        Console.WriteLine("ModLoader(s):");
        foreach (var m in ModLoader.ModLoaders)
        {
            Console.WriteLine($"  {m.AssemblyQualifiedName}");
            Console.WriteLine($"    from {m.Assembly.Location}");
        }
        Console.WriteLine();
        using (var f = config.OpenRead())
        {
            env = Application.AppInit(f, env);
        }
        controller = (Controller)env[Application.NAME_CONTROLLER];
        mainConfig = (MainConfig)env[Application.NAME_MAINCONFIG];
        VolumeManager volumeManager = Application.ExecuteVolumeConfig(mainConfig.GetVolumeInfo());
        env.AddExitAction(_ => volumeManager.Foreach(volumeManager.Restore));
        Application.Exit(env);
    }
    catch (Exception ex)
    {
        Trace.TraceError(ex.ToString());
        Console.WriteLine("测试不通过");
        Environment.Exit(1);
    }
}, configFile, mod, clean);

schema.SetHandler((config, moden, cl, output) =>
{
    using TextWriter outw = output == null ? Console.Out : new StreamWriter(output.OpenWrite());
    if (!cl)
    {
        LoadModules();
    }
    if (moden && config != null)
    {
        using var f0 = config.OpenRead();
        LoadExternalMods(f0);
    }
    outw.WriteLine(moden ? ModLoader.GetConfigSchema() : Application.GetConfigJsonSchema());
    if (output != null)
    {
        Console.WriteLine($"已写入文件: {output.FullName}");
    }
    outw.Close();
}, conf, mod, clean, outfile);

deviceList.SetHandler(() =>
{
    VolumeManager.ListDevice(Console.WriteLine);
});

var cmdBuilder = new CommandLineBuilder(root);
cmdBuilder.UseVersionOption("--version", "-v", "-V").UseDefaults();
cmdBuilder.Build().Invoke(args);