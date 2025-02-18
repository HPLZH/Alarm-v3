using NAudio.Wave;
using System.Diagnostics;

namespace Alarm.Core
{
    public class Player
    {
        public const int LATENCY = 100;

        readonly IWavePlayer player;
        readonly Queue<string> queue = new();

        readonly Lock @lock = new();

        public event EventHandler QueueCleared = EventHelper.DoNothing;
        public event EventHandler<PlaybackEventArgs> PlaybackFinished = EventHelper.DoNothing;
        public event EventHandler<PlaybackEventArgs> PlaybackStarted = EventHelper.DoNothing;

        public bool AutoContinue { get; set; } = false;

        public enum PlaybackEventType
        {
            Start, Stop, Pause, Continue
        }

        public class PlaybackEventArgs(string file, TimeSpan length, PlaybackEventType type) : EventArgs
        {
            public string File { get; init; } = file;
            public TimeSpan Length { get; init; } = length;
            public PlaybackEventType Type { get; init; } = type;
        }

        public string Current { get; private set; } = "";

        private DateTime start = DateTime.UnixEpoch;
        private TimeSpan sum = TimeSpan.Zero;

        public Player(IWavePlayer player)
        {
            this.player = player;
            player.PlaybackStopped += OnPlaybackStopped;
        }

        public static Player Build()
        {
            WaveOutEvent waveOut = new();
            return new Player(waveOut);
        }

        public static Player BuildFromDevice(Guid device)
        {
            DirectSoundOut directSoundOut = new(device, LATENCY);
            return new Player(directSoundOut);
        }

        public void Play(bool? autoContinue = null)
        {
            lock (@lock)
            {
                AutoContinue = autoContinue ?? AutoContinue;
                switch (player.PlaybackState)
                {
                    case PlaybackState.Stopped:
                        if (queue.TryDequeue(out string? next))
                        {
                            Current = next;
                            AudioFileReader reader = new(next);
                            player.Init(reader);
                            player.Play();
                            start = DateTime.Now;
                            sum = TimeSpan.Zero;
                            PlaybackStarted.Invoke(this, new(Current, TimeSpan.Zero, PlaybackEventType.Start));
                        }
                        else
                        {
                            QueueCleared.Invoke(this, EventArgs.Empty);
                        }
                        break;
                    case PlaybackState.Paused:
                        player.Play();
                        start = DateTime.Now;
                        break;
                    case PlaybackState.Playing:
                        break;
                }
            }
        }

        public void Pause()
        {
            lock (@lock)
            {
                switch (player.PlaybackState)
                {
                    case PlaybackState.Stopped:
                    case PlaybackState.Paused:
                        break;
                    case PlaybackState.Playing:
                        player.Pause();
                        DateTime now = DateTime.Now;
                        Debug.Assert(start != DateTime.UnixEpoch);
                        sum += now - start;
                        start = DateTime.UnixEpoch;
                        break;
                }
            }
        }

        public void Stop()
        {
            lock (@lock)
            {
                switch (player.PlaybackState)
                {
                    case PlaybackState.Stopped:
                        break;
                    case PlaybackState.Paused:
                    case PlaybackState.Playing:
                        player.Stop();
                        break;
                }
            }
        }

        private void OnPlaybackStopped(object? sender, StoppedEventArgs e)
        {
            if (e.Exception != null)
            {
                throw e.Exception;
            }
            else
            {
                PlaybackEventArgs args;
                lock (@lock)
                {
                    DateTime now = DateTime.Now;
                    Debug.Assert(start != DateTime.UnixEpoch);
                    sum += now - start;
                    start = DateTime.UnixEpoch;
                    args = new(Current, sum, PlaybackEventType.Stop);
                    player.Dispose();
                    Current = "";
                }
                PlaybackFinished.Invoke(this, args);
                if (AutoContinue)
                    Play();
            }
        }

        public void Add(string path)
        {
            lock (@lock)
            {
                queue.Enqueue(path);
            }
        }

        public void Clear()
        {
            lock (@lock)
            {
                queue.Clear();
            }
        }
    }
}
