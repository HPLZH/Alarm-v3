using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alarm.Core
{
    public static class Configuration
    {
        public readonly static JsonSerializerOptions JsonSerializerOptions = new()
        {
            WriteIndented = true,
            AllowTrailingCommas = true,
            IncludeFields = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            ReadCommentHandling = JsonCommentHandling.Skip,
        };
    }
    public delegate void ConfigHandler(JsonElement json, Application app);
}
