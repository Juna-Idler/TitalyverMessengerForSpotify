using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

using SpotifyAPI.Web;


namespace TitalyverMessengerForSpotify
{
    public class CurrentPlayingAutoGetter
    {
        private readonly SpotifyClient Spotify;
        public delegate void GetCallback(SpotifyAPI.Web.CurrentlyPlaying playing);
        private readonly GetCallback Callback;

        private Stopwatch Timer = new();
        public int Wait { get; private set; }

        public int RemainMs => IsLooping ? Wait - (int)Timer.ElapsedMilliseconds : -1;

        public CurrentPlayingAutoGetter(SpotifyClient spotify, GetCallback callback)
        {
            Spotify = spotify;
            Callback = callback;
        }
        ~CurrentPlayingAutoGetter()
        {
            Stop();
            Cancellation.Dispose();
        }

        private CancellationTokenSource Cancellation = new();
        private bool LoopEnd = true;

        public bool IsLooping => !LoopEnd;

        public void Stop()
        {
            LoopEnd = true;
            Cancellation.Cancel();
        }
        public void Instantly()
        {
            Cancellation.Cancel();
        }

        public void Start(int wait)
        {
            PlayerCurrentlyPlayingRequest request = new() { Market = "from_token" };
            LoopEnd = false;

            _ = Task.Run(async () =>
             {

                 while (true)
                 {
                     try
                     {
                         Wait = wait;
                         Timer.Restart();
                         await Task.Delay(wait, Cancellation.Token);
                     }
                     catch (TaskCanceledException)
                     {
                         Cancellation.Dispose();
                         Cancellation = new CancellationTokenSource();
                     }
                     if (LoopEnd)
                     {
                         break;
                     }
                     CurrentlyPlaying playing = Spotify.Player.GetCurrentlyPlaying(request).Result;
                     Callback(playing);

                     if (playing == null)
                     {
                         LoopEnd = true;
                         break;
                     }
                     if (playing.CurrentlyPlayingType == "ad")
                     {
                         wait = 31 * 1000 - playing.ProgressMs.GetValueOrDefault(0);
                         continue;
                     }
                     if (playing.Item is FullTrack track)
                     {
                         wait = track.DurationMs - playing.ProgressMs.GetValueOrDefault(0);
                     }
                     else
                     {
                         wait = 60 * 1000;
                     }
                     wait = Math.Min(wait, 60 * 1000);
                     wait = Math.Max(wait, 1000);
                 }
                 Timer.Stop();
             });
        }
    }
}
