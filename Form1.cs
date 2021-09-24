using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Diagnostics;

using System.Text.Json;

using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using static SpotifyAPI.Web.Scopes;


using Titalyver2;

namespace TitalyverMessengerForSpotify
{
    public partial class Form1 : Form
    {
        private Spotify Spotify = null;
        private MMFMessenger Messenger = new();

        private PlayerCurrentlyPlayingRequest Request = new() { Market = "from_token" };

    public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (!Messenger.Initialize())
            {
                Messenger = null;
                Close();
                return;
            }

            Spotify = Spotify.Create(false, 60000).Result;
            if (Spotify == null)
            {
                this.Close();
                return;
            }
        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Messenger.Terminalize();
        }




        private void GetCurrentPlaying_Click(object sender, EventArgs e)
        {
            AutoGetAndUpdate();
        }


        Task AutoUpdate = null;
        CancellationTokenSource Cancellation = null;
        private void AutoGetAndUpdate()
        {
            Debug.WriteLine("AutoGetAndUpdate:Start");
            if (AutoUpdate != null && AutoUpdate.IsCompleted == false)
            {
                Cancellation.Cancel();
            }
            CurrentlyPlayingState state = GetAndUpdate();

            int wait_ms = 0;
            switch(state)
            {
                case CurrentlyPlayingState.Uninitialized:
                case CurrentlyPlayingState.NoPlaying:
                case CurrentlyPlayingState.NotTrack:
                case CurrentlyPlayingState.Stop:
                    return;

                case CurrentlyPlayingState.Advertisement:
                    wait_ms = 15000 - LastPlaying.ProgressMs.GetValueOrDefault(0) + 1000;
                    break;
                case CurrentlyPlayingState.Playing:
                    wait_ms = LastTrack.DurationMs - LastPlaying.ProgressMs.GetValueOrDefault(0) + 1000;
                    break;
            }
            {
                Cancellation?.Dispose();
                Cancellation = new();
                AutoUpdate = Task.Run(async () =>
                {
                    try
                    {
                        Debug.WriteLine("AutoGetAndUpdate:Delay Start");
                        wait_ms = Math.Min(wait_ms, 30 * 1000);
//                        wait_ms = Math.Man(wait_ms, 1000);
                        await Task.Delay(wait_ms , Cancellation.Token);
                        Debug.WriteLine("AutoGetAndUpdate:Delay End");
                    }
                    catch (System.Threading.Tasks.TaskCanceledException cancel)
                    {
                        Debug.WriteLine("AutoGetAndUpdate:Delay Cancel");
                        return;
                    }
                    AutoGetAndUpdate();
                });
            }
            Debug.WriteLine("AutoGetAndUpdate:End");
        }

        public enum CurrentlyPlayingState { Uninitialized, NoPlaying, Advertisement, NotTrack, Stop, Playing }

        static byte[] no_playing = null;
        static byte[] ad = null;

        private CurrentlyPlaying LastPlaying = null;
        private FullTrack LastTrack = null;

        private CurrentlyPlayingState GetAndUpdate()
        {
            if (Spotify == null || !Messenger.IsValid())
                return CurrentlyPlayingState.Uninitialized;
            CurrentlyPlaying playing = Spotify.SpotifyClient.Player.GetCurrentlyPlaying(Request).Result;
            LastPlaying = playing;
            if (playing == null)
            {
                if (no_playing == null)
                {
                    MetaData data = new("no playing", new string[] { "Spotify" }, "");
                    no_playing = JsonSerializer.SerializeToUtf8Bytes<MetaData>(data);
                }
                Messenger.Update(ITitalyverReceiver.EnumPlaybackEvent.Stop, 0, no_playing);
                Invoke((MethodInvoker)(() =>
                {
                    JsonSerializerOptions options = new()
                    {
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                        WriteIndented = true
                    };
                    textBox1.Text = "Not playing";
                }));

                return CurrentlyPlayingState.NoPlaying;
            }

            if (playing.CurrentlyPlayingType == "ad")
            {
                if (ad == null)
                {
                    MetaData data = new("ad", new string[] { "Spotify" }, "Spotify");
                    ad = JsonSerializer.SerializeToUtf8Bytes<MetaData>(data);
                }
                Messenger.Update(ITitalyverReceiver.EnumPlaybackEvent.SeekStop, 0, ad);
                Invoke((MethodInvoker)(() =>
                {
                    JsonSerializerOptions options = new()
                    {
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                        WriteIndented = true
                    };
                    textBox1.Text = "広告再生中 これの再生時間を取る方法なんかないのか？";
                }));
                return CurrentlyPlayingState.Advertisement;
            }

            if (playing.Item is FullTrack track)
            {
                ITitalyverReceiver.EnumPlaybackEvent playbackEvent = playing.IsPlaying ? ITitalyverReceiver.EnumPlaybackEvent.SeekPlay : ITitalyverReceiver.EnumPlaybackEvent.SeekStop;

                if (LastTrack == null || track.Id != LastTrack.Id)
                {
                    MetaData data = new(track);
                    byte[] json = JsonSerializer.SerializeToUtf8Bytes<MetaData>(data);
                    Messenger.Update(playbackEvent, playing.ProgressMs.Value / 1000.0, json);
                    LastTrack = track;
                    Invoke((MethodInvoker)(() =>
                    {
                        JsonSerializerOptions options = new()
                        {
                            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                            WriteIndented = true
                        };
                        textBox1.Text = JsonSerializer.Serialize<MetaData>(data, options);
                    }));
                }
                else
                {
                    Messenger.Update(playbackEvent, playing.ProgressMs.Value / 1000.0);
                }
                return playing.IsPlaying ? CurrentlyPlayingState.Playing : CurrentlyPlayingState.Stop;
            }
            return CurrentlyPlayingState.NotTrack;

        }

    }
}
