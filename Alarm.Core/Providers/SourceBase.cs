using Alarm.Core;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Alarm.Providers;
#pragma warning restore IDE0130 // 命名空间与文件夹结构不匹配

public abstract class SourceBase : ProviderBase
{
    protected SourceBase()
    {
        Upstream = ProviderEnd.Instance;
    }
}
