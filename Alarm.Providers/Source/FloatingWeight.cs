using Alarm.Core;
using System.Text.Json;

namespace Alarm.Providers.Source
{
    public class FloatingWeight : SourceBase
    {
        public struct Options()
        {
            public int defaultWeight = 300;
            public double unitTime = 1;
            public int unitWeight = 1;
        }

        private readonly string[] list;
        private readonly Dictionary<string, int> index = [];
        private readonly int[] weight;
        private long sum;
        private readonly Options options;
        private readonly Dictionary<string, int> raw;

        public FloatingWeight(IEnumerable<string> src, IDictionary<string, int> weight, Options options)
        {
            list = new HashSet<string>(src).ToArray();
            Random.Shared.Shuffle(list);
            this.weight = new int[list.Length];
            this.options = options;
            raw = new Dictionary<string, int>(weight);
            sum = 0;
            for (int i = 0; i < list.Length; i++)
            {
                index.Add(list[i], i);
                if (!weight.TryGetValue(list[i], out this.weight[i]))
                {
                    this.weight[i] = options.defaultWeight;
                }
                if (this.weight[i] > 0)
                    sum += this.weight[i];
            }
        }

        public override string Next()
        {
            long n = Random.Shared.NextInt64(sum);
            for (int i = 0; i < list.Length; i++)
            {
                if (weight[i] > 0)
                {
                    n -= weight[i];
                    if (n < 0)
                    {
                        return list[i];
                    }
                }
            }
            return list[^1];
        }

        public override void OnPlaybackFinished(string file, TimeSpan length)
        {
            TimeSpan dt = TimeSpan.FromSeconds(options.unitTime);
            TimeSpan rt = length;
            bool requireSum = false;
            if (!index.TryGetValue(file, out int i0))
            {
                return;
            }
            while (rt > TimeSpan.Zero)
            {
                int ir = Random.Shared.Next(weight.Length);
                weight[i0] -= options.unitWeight;
                if (weight[i0] < 0 || weight[ir] < 0) requireSum = true;
                weight[ir] += options.unitWeight;
                rt -= dt;
            }
            if (requireSum)
            {
                sum = 0;
                foreach (int w in weight)
                {
                    if (w > 0)
                        sum += w;
                }
            }
            return;
        }

        public Dictionary<string, int> WeightData()
        {
            Dictionary<string, int> outWeight = [];
            for (int i = 0; i < list.Length; i++)
            {
                outWeight.Add(list[i], weight[i]);
            }
            foreach (var (k, v) in raw)
            {
                outWeight.TryAdd(k, v);
            }
            return outWeight;
        }

        public class Managed : FloatingWeight
        {
            internal struct Config()
            {
                public required string data;
                public Options opts = new();
            }

            private readonly string data;

            private Managed(string dataFile, IEnumerable<string> src, IDictionary<string, int> weight, Options options)
                : base(src, weight, options)
            {
                data = dataFile;
            }

            public static Managed FromJson(JsonElement json, IProvider _, IEnumerable<string> playlist)
            {
                Config config = json.Deserialize<Config>(Configuration.JsonSerializerOptions);
                Dictionary<string, int> weightData;
                try
                {
                    using var datafile = File.OpenRead(config.data);
                    weightData = JsonSerializer.Deserialize<Dictionary<string, int>>(datafile, Configuration.JsonSerializerOptions) ?? [];
                }
                catch (FileNotFoundException)
                {
                    weightData = [];
                }
                return new Managed(config.data, playlist, weightData, config.opts);
            }

            public override void OnPlaybackFinished(string file, TimeSpan length)
            {
                base.OnPlaybackFinished(file, length);
                using var datafile = File.Open(data, FileMode.Create);
                JsonSerializer.Serialize(datafile, WeightData(), Configuration.JsonSerializerOptions);
                datafile.Flush();
                datafile.Close();
            }
        }
    }
}
