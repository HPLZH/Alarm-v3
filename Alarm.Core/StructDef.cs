using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Alarm.Core;

public struct PlaylistInfo : ISchemaDefined
{
    public string type;
    public string? src;
    public string? raw;
    public string[]? data;

    public readonly JsonNode GetSchema() => StaticSchema();

    public static JsonNode StaticSchema() => new JsonObject
    {
        ["type"] = "object",
        ["properties"] = new JsonObject
        {
            ["type"] = new JsonObject
            {
                ["type"] = "string",
                ["enum"] = new JsonArray("text", "m3u8", "json")
            },
            ["src"] = Schema.String(),
            ["raw"] = Schema.String(),
            ["data"] = Schema.String()
        },
        ["required"] = new JsonArray("type"),
        ["minProperties"] = 2,
        ["maxProperties"] = 2,
        ["additionalProperties"] = false
    };

    public readonly IEnumerable<string> GetList()
    {
        switch (type)
        {
            case "text":
            case "m3u8":
                if (src != null)
                {
                    return M3u8.ReadList(src);
                }
                else if (raw != null)
                {
                    return M3u8.GetListFromString(raw);
                }
                else
                {
                    return [];
                }
            case "json":
                if (data != null)
                {
                    return data;
                }
                else if (src != null)
                {
                    using var file = File.OpenRead(src);
                    return JsonSerializer.Deserialize<string[]>(file, Configuration.JsonSerializerOptions) ?? [];
                }
                else if (raw != null)
                {
                    return JsonSerializer.Deserialize<string[]>(raw, Configuration.JsonSerializerOptions) ?? [];
                }
                else
                {
                    return [];
                }
            default:
                throw new NotSupportedException();
        }
    }
}

public struct JsonReference<T> : ISchemaDefined
{
    public string? src;
    public string? raw;
    public T? data;

    public readonly JsonNode GetSchema() => StaticSchema();

    public static JsonNode StaticSchema()
    {
        var props = new JsonObject
        {
            ["src"] = Schema.String(),
            ["raw"] = Schema.String(),
            ["data"] = Schema.GetSchema(typeof(T)),
        };
        return new JsonObject()
        {
            ["type"] = "object",
            ["properites"] = props,
            ["minProperties"] = 1,
            ["maxProperties"] = 1,
            ["additionalProperties"] = false
        };
    }

    public readonly T? GetValue()
    {
        if (data != null)
        {
            return data;
        }
        else if (src != null)
        {
            using var f = File.OpenRead(src);
            return JsonSerializer.Deserialize<T>(f, Configuration.JsonSerializerOptions);
        }
        else if (raw != null)
        {
            return JsonSerializer.Deserialize<T>(raw, Configuration.JsonSerializerOptions);
        }
        else
        {
            return default;
        }
    }
}

public struct PlayerInfo
{
    public required string type;
    public Guid? guid;

    public PlayerInfo() { }
}

public struct ProviderBuildInfo
{
    public required string type;
    public JsonElement config;
}

public struct MainConfig()
{
    public required PlayerInfo player;
    public required ProviderBuildInfo[] provider;
    public required PlaylistInfo playlist;
    public Dictionary<string, JsonElement> volume = [];

    public readonly IDictionary<string, int> GetVolumeInfo()
    {
        return volume.Select(kvp => KeyValuePair.Create(kvp.Key, kvp.Value.ValueKind switch
        {
            JsonValueKind.Number => kvp.Value.GetInt32(),
            JsonValueKind.True => -2,
            _ => -1,
        })).ToDictionary();
    }

    [JsonExtensionData]
    public Dictionary<string, JsonElement> ext = [];
}

public struct ConfigHandlerInfo(ConfigHandler handler, JsonNode schema)
{
    public ConfigHandler handler = handler;
    public JsonNode schema = schema;
    public bool isPreHandler = false;

    public ConfigHandlerInfo(ConfigHandler handler, Type type)
        : this(handler, Schema.GetSchema(type)) { }
}

public struct ProviderTypeInfo(ProviderFromJson builder, JsonNode schema)
{
    public ProviderFromJson builder = builder;
    public JsonNode schema = schema;

    public ProviderTypeInfo(ProviderFromJson builder, Type type)
        : this(builder, Schema.GetSchema(type)) { }
}