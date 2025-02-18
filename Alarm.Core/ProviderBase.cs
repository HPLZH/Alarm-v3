namespace Alarm.Core
{
    public abstract class ProviderBase : IProvider
    {

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
        public IProvider Upstream { get; protected init; }
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。

        public virtual string Next()
        {
            return Upstream.Next();
        }

        public virtual void OnPlaybackFinished(string file, TimeSpan length)
        {
            Upstream.OnPlaybackFinished(file, length);
        }
    }
}
