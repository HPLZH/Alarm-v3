using Alarm.Core;
using System.Diagnostics;
using System.Text.Json;

namespace Alarm.Log
{
    public class Logger : StreamWriter
    {
        class Output
        {
            internal long pos;
            internal Stream stream;

            internal Output(Stream stream)
            {
                this.stream = stream;
                pos = 0;
            }
        }

        readonly Lock @lock = new();
        readonly MemoryStream stream;
        readonly List<Output> outputs = [];

        public Logger() : base(new MemoryStream())
        {
            Debug.Assert(BaseStream is MemoryStream);
            stream = (MemoryStream)BaseStream;
        }

        public override void Flush()
        {
            lock (@lock)
            {
                base.Flush();
                byte[] buf = stream.ToArray();
                long spos = stream.Position;
                foreach (var output in outputs)
                {
                    if (output.pos == -1)
                    {
                        continue;
                    }
                    try
                    {
                        if (output.pos < spos)
                        {
                            output.stream.Write(buf, (int)output.pos, (int)(spos - output.pos));
                            output.stream.Flush();
                            output.pos = spos;
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError(ex.ToString());
                    }
                }
            }

        }

        public long RemoveOutput(Stream outStream)
        {
            lock (@lock)
            {
                int r = -1;
                for (int i = 0; i < outputs.Count; i++)
                {
                    if (outputs[i].stream == outStream)
                    {
                        r = i;
                        break;
                    }
                }
                if (r >= 0)
                {
                    var o = outputs[r];
                    outputs.RemoveAt(r);
                    return o.pos;
                }
                else
                {
                    return -1;
                }
            }

        }

        public void AddOutput(Stream outStream, long offset = 0)
        {
            lock (@lock)
            {
                RemoveOutput(outStream);
                var o = new Output(outStream) { pos = offset };
                if (offset < 0)
                {
                    o.pos = stream.Position + offset + 1;
                }
                outputs.Add(o);
                Flush();
            }
        }

        public static void FromJson(JsonElement json, IDictionary<string, object> env)
        {
            Config[] confs = json.Deserialize<Config[]>(Configuration.JsonSerializerOptions) ?? [];
            foreach (Config conf in confs)
            {
                if (env.ContainsKey(conf.id))
                {
                    throw new Exception("标识符已存在");
                }
                else
                {
                    Logger logger = new();
                    env[conf.id] = logger;
                }
            }
        }

        public static void LinkStream(string upstream, IDictionary<string, object> env, Stream stream)
        {
            if (env.TryGetValue(upstream, out object? o))
            {
                Debug.Assert(o is Logger);
                ((Logger)o).AddOutput(stream);
            }
            else
            {
                throw new KeyNotFoundException();
            }
        }

        public static void LinkFileStream(JsonElement json, IDictionary<string, object> env)
        {
            LinkFileConfig[] confs = json.Deserialize<LinkFileConfig[]>(Configuration.JsonSerializerOptions) ?? [];
            foreach (var config in confs)
            {
                if (env.ContainsKey(config.id))
                {
                    throw new Exception("标识符已存在");
                }
                var f = File.Open(config.file, config.append ? FileMode.Append : FileMode.Create);
                LinkStream(config.upstream, env, f);
                env[config.id] = f;
            }
        }

        private struct Config
        {
            public required string id;
        }

        internal struct LinkConfig
        {
            public required string upstream;
        }

        private struct LinkFileConfig
        {
            public required string id;
            public required string upstream;
            public required string file;
            public bool append = true;

            public LinkFileConfig() { }
        }
    }
}
