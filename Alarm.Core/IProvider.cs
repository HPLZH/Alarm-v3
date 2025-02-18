namespace Alarm.Core
{
    public interface IProvider
    {
        string Next();
        void OnPlaybackFinished(string file, TimeSpan length);
    }
}
