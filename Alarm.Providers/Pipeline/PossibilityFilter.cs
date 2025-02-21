using Alarm.Core;
using System.Text.Json;

namespace Alarm.Providers.Pipeline
{
    public class PossibilityFilter(IProvider upstream, IDictionary<string, double> possibility, double basicPossibility = 1) : PipelineBase(upstream)
    {
        private readonly Dictionary<string, double> p = new(possibility);
        private readonly double basicP = basicPossibility;

        public override string Next()
        {
            string r;
            do
            {
                r = base.Next();
            } while (Random.Shared.NextDouble() < p.GetValueOrDefault(r, basicP));
            return r;
        }


        internal struct Config()
        {
            public double basicPossibility = 1;
            public required JsonReference<Dictionary<string, double>> possibility;

        }

        public static PossibilityFilter FromJson(JsonElement json, IProvider upstream, IEnumerable<string> _)
        {
            Config config = json.Deserialize<Config>(Configuration.JsonSerializerOptions);
            return new PossibilityFilter(upstream, config.possibility.GetValue() ?? [], config.basicPossibility);
        }
    }
}
