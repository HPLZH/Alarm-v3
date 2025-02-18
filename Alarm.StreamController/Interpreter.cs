using Alarm.Core;
using System.CommandLine;

namespace Alarm.StreamController
{
    public class Interpreter(TextReader reader, Application env)
    {
        readonly Application env = env;
        readonly TextReader reader = reader;

        public IEnumerable<string> ReadToken()
        {
            List<char> seq = [];
            char q = '\0';
            char c;
            int ci;
            while ((ci = reader.Read()) != -1)
            {
                c = (char)ci;
                if (q == '\0')
                {
                    if (char.IsWhiteSpace(c))
                    {
                        if (seq.Count > 0)
                        {
                            yield return new string([.. seq]);
                            seq.Clear();
                        }
                    }
                    else if (c == '\'' || c == '"')
                    {
                        q = c;
                    }
                    else if (c == ';')
                    {
                        if (seq.Count > 0)
                            yield return new string([.. seq]);
                        seq.Clear();
                        yield return ";";
                    }
                    else if (c == '#')
                    {
                        reader.ReadLine();
                    }
                    else
                    {
                        seq.Add(c);
                    }
                }
                else
                {
                    if (c == q)
                    {
                        int i = reader.Peek();
                        if (i != -1 && (char)i == q)
                        {
                            reader.Read();
                            seq.Add(c);
                        }
                        else
                        {
                            yield return new string([.. seq]);
                            seq.Clear();
                        }
                    }
                    else
                    {
                        seq.Add(c);
                    }
                }
            }
            if (seq.Count > 0)
                yield return "#" + new string([.. seq]);
        }

        public IEnumerable<string[]> ReadCommand()
        {
            List<string> cur = [];
            foreach (var token in ReadToken())
            {
                if (token.StartsWith('#'))
                {
                    cur.Add(token[1..]);
                }
                else if (token == ";")
                {
                    if (cur.Count > 0)
                        yield return cur.ToArray();
                    cur.Clear();
                }
                else
                {
                    cur.Add(token);
                }
            }
            if (cur.Count > 0)
                yield return cur.ToArray();
        }

        public void Execute()
        {
            foreach (var args in ReadCommand())
            {
                CommandLine.Execute(args, env);
            }
        }

        static class CommandLine
        {
            static readonly RootCommand root = [];

            static readonly Command cmdExit = new("exit");
            static readonly Command cmdNext = new("next");

            static readonly Command cmdExitLater = new("later");

            static CommandLine()
            {
                root.Add(cmdExit);
                root.Add(cmdNext);

                cmdExit.Add(cmdExitLater);

            }

            public static void Execute(string[] args, Application env)
            {
                Controller controller = (Controller)env[Application.NAME_CONTROLLER];

                cmdExit.SetHandler(() =>
                {
                    Application.Exit(env, 0);
                });

                cmdExitLater.SetHandler(() =>
                {
                    controller.PlaybackFinished += (_, _) =>
                    {
                        Application.Exit(env, 0);
                    };
                });

                cmdNext.SetHandler(controller.Skip);

                root.Invoke(args, new System.CommandLine.IO.TestConsole());
            }
        }
    }
}
