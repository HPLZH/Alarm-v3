using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alarm.Core;

public struct PlaylistInfo
{
    public string type;
    public string? src;
    public string? raw;
    public string[]? data;

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

public struct JsonReference<T>
{
    public string? src;
    public string? raw;
    public T? data;

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

    public int volume = -1;
    public bool unmute = true;

    public PlayerInfo() { }
}

public struct ProviderBuildInfo
{
    public string type;
    public JsonElement config;
}

public struct MainConfig
{
    public PlayerInfo player;
    public ProviderBuildInfo[] provider;
    public PlaylistInfo playlist;

    [JsonExtensionData]
    public Dictionary<string, JsonElement> ext;
}