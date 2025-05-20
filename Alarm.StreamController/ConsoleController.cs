using Alarm.Core;
using System.Text.Json;

namespace Alarm.StreamController
{
    public static class ConsoleController
    {
        public static Interpreter? Interpreter { get; set; }

        public static void FromJson(JsonElement json, Application env)
        {
            if (json.Deserialize<bool>(Configuration.JsonSerializerOptions))
            {
                Interpreter = new(Console.In, env, Console.Out);
                env.AddMainTask(new Task(Interpreter.Execute));
            }
        }
    }
}
