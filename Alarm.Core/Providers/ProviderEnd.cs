﻿using Alarm.Core;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Alarm.Providers;
#pragma warning restore IDE0130 // 命名空间与文件夹结构不匹配

public class ProviderEnd : ProviderBase
{
    private ProviderEnd()
    {
        Upstream = this;
    }

    public override string Next()
    {
        return "";
    }

    public override void OnPlaybackFinished(string file, TimeSpan length)
    {
        return;
    }

    public static readonly ProviderEnd Instance = new();
}
