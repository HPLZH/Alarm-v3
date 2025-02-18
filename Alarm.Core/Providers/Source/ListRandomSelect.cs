using Alarm.Core;
using System.Text.Json;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Alarm.Providers.Source;
#pragma warning restore IDE0130 // 命名空间与文件夹结构不匹配

public class ListRandomSelect : SourceBase
{
    private readonly string[] list;

    public ListRandomSelect(IEnumerable<string> src)
    {
        list = src.ToArray();
        Random.Shared.Shuffle(list);
    }

    public override string Next()
    {
        int i = Random.Shared.Next(list.Length);
        return list[i];
    }

    public static ListRandomSelect FromJson(JsonElement _1, IProvider _2, IEnumerable<string> playlist)
    {
        return new ListRandomSelect(playlist);
    }
}
