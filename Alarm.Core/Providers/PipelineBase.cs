using Alarm.Core;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Alarm.Providers;
#pragma warning restore IDE0130 // 命名空间与文件夹结构不匹配

public abstract class PipelineBase : ProviderBase
{
    protected PipelineBase(IProvider upstream)
    {
        Upstream = upstream;
    }
}

