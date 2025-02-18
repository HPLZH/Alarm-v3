using Alarm.Providers.Source;
using System.Text.Json;

namespace Alarm.Core
{
    public static class ProviderBuilder
    {
        public static readonly Dictionary<string, ProviderFromJson> builders = [];

        static ProviderBuilder()
        {
            builders.Add("random", ListRandomSelect.FromJson);
        }

        public static IProvider Build(ProviderBuildInfo buildInfo, IProvider upstream, IEnumerable<string> playlist)
        {
            if (builders.TryGetValue(buildInfo.type, out var builder))
            {
                return builder.Invoke(buildInfo.config, upstream, playlist);
            }
            else
            {
                throw new KeyNotFoundException();
            }
        }

        public static IProvider Build(IEnumerable<ProviderBuildInfo> buildInfos, IProvider upstream, IEnumerable<string> playlist)
        {
            IProvider cur = upstream;
            foreach (var buildInfo in buildInfos)
            {
                cur = Build(buildInfo, cur, playlist);
            }
            return cur;
        }
    }

    public delegate IProvider ProviderFromJson(JsonElement json, IProvider upstream, IEnumerable<string> playlist);
}
