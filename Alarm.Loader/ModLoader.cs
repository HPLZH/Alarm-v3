using Alarm.Core;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json.Nodes;

namespace Alarm.Loader
{
    public class ModLoader
    {
        protected virtual void Load() { }

        public static ImmutableArray<Type> ModLoaders => [.. loaders];
        private static readonly List<Type> loaders = [];

        public void LoadMod()
        {
            Load();
            var type = GetType();
            var assemblyName = type.Assembly.GetName();
            Trace.WriteLine($"ModLoader: Loaded {type.FullName} from {assemblyName.Name} [{assemblyName.Version}].");
            loaders.Add(type);
        }

        public static void LoadFile(FileInfo file)
        {
            Trace.WriteLine($"ModLoader: Loading assembly file {file.FullName}.");
            var assembly = Assembly.LoadFile(file.FullName);
            var assemblyName = assembly.GetName();
            Trace.WriteLine($"ModLoader: Loading assembly {assembly.FullName}.");
            var types = assembly.GetTypes();
            int count = 0;
            foreach (var type in types)
            {
                if (type.BaseType == typeof(ModLoader))
                {
                    var o = type.InvokeMember(string.Empty, BindingFlags.CreateInstance, null, null, null);
                    if (o is ModLoader loader)
                    {
                        loader.LoadMod();
                        count++;
                    }
                }
            }
            Trace.WriteLine($"ModLoader: Loaded {count} ModLoader(s) from {assemblyName.Name}.");
        }

        public struct Config()
        {
            public string[] mods = [];
        }

        public static JsonObject GetConfigSchema()
        {
            JsonObject schema = Application.GetConfigJsonSchema();
            JsonObject props = schema["properties"]?.AsObject() ?? throw new NullReferenceException();
            props.Insert(0, "mods", Schema.GetSchema(typeof(string[])));
            return schema;
        }

        public static void RegisterPreHandler(string id, ConfigHandlerInfo handler)
        {
            ConfigHandlerInfo info = handler;
            info.isPreHandler = true;
            Application.configHandlers.Add(id, info);
        }

        public static void RegisterPostHandler(string id, ConfigHandlerInfo handler)
        {
            ConfigHandlerInfo info = handler;
            info.isPreHandler = false;
            Application.configHandlers.Add(id, info);
        }

        public static void RegisterConfigHandler(string id, ConfigHandlerInfo handler)
        {
            Application.configHandlers.Add(id, handler);
        }


        public static void RegisterProviderBuilder(string id, ProviderTypeInfo builder)
        {
            ProviderBuilder.builders.Add(id, builder);
        }

        public static void RegisterExitHandler(Type type, Action<string, object> handler)
        {
            Application.exitHandlers.Add(type, handler);
        }
    }
}
