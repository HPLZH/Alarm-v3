namespace Alarm.Core
{
    public class Controller
    {
        private readonly Player player;
        private readonly IProvider provider;

        public event EventHandler<Player.PlaybackEventArgs> PlaybackStarted = EventHelper.DoNothing;
        public event EventHandler<Player.PlaybackEventArgs> PlaybackFinished = EventHelper.DoNothing;

        public Controller(Player player, IProvider provider)
        {
            this.player = player;
            this.provider = provider;
            player.QueueCleared += OnQueueCleared;
            player.PlaybackFinished += OnPlaybackFinished;
            player.PlaybackStarted += OnPlaybackStarted;
        }

        private void OnPlaybackStarted(object? sender, Player.PlaybackEventArgs e)
        {
            PlaybackStarted.Invoke(this, e);
        }

        private void OnQueueCleared(object? sender, EventArgs e)
        {
            string next = provider.Next();
            player.Add(next);
            player.Play();
        }

        private void OnPlaybackFinished(object? sender, Player.PlaybackEventArgs e)
        {
            provider.OnPlaybackFinished(e.File, e.Length);
            PlaybackFinished.Invoke(this, e);
        }

        public void Play()
        {
            player.Play(true);
        }

        public void Pause()
        {
            player.Pause();
        }

        public void Stop()
        {
            player.AutoContinue = false;
            player.Stop();
        }

        public void Skip()
        {
            player.Stop();
        }

        public void Insert(string path)
        {
            player.Add(path);
        }
    }
}
