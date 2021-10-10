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

        private CurrentPlayingAutoGetter AutoGetter;

        private long FirstLagTime = -1000;
        private long TimeOffset = 0;


        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (!Messenger.Initialize())
            {
                Close();
                return;
            }

            Spotify = Spotify.Create(false, 60000);
            if (Spotify == null)
            {
                this.Close();
                return;
            }
            AutoGetter = new(Spotify.SpotifyClient, GetCallback);
            timer1.Start();
        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Messenger.Terminalize();
        }




        private void GetCurrentPlaying_Click(object sender, EventArgs e)
        {
            if (AutoGetter.IsLooping)
            {
                AutoGetter.Instantly();
            }
            else
            {
                AutoGetter.Start(0);
            }
        }




        static byte[] no_playing = null;
        static byte[] ad = null;

        private FullTrack LastTrack = null;
        private long TrackStartTimeStamp = 0;

        private void GetCallback(CurrentlyPlaying playing)
        {
            if (!Messenger.IsValid())
                return;
            if (playing == null)
            {
                LastTrack = null;
                if (no_playing == null)
                {
                    MetaData data = new("no playing", new string[] { "Spotify" }, "",0);
                    no_playing = JsonSerializer.SerializeToUtf8Bytes<MetaData>(data);
                }
                Messenger.Update(EnumPlaybackEvent.Stop, 0, no_playing);
                Invoke((MethodInvoker)(() =>
                {
                    textBox1.Text = "No playing";
                }));
                return;
            }

            if (playing.CurrentlyPlayingType == "ad")
            {
                LastTrack = null;
                if (ad == null)
                {
                    MetaData data = new("ad", new string[] { "Spotify" }, "Spotify",0);
                    ad = JsonSerializer.SerializeToUtf8Bytes<MetaData>(data);
                }
                Messenger.Update(EnumPlaybackEvent.SeekStop, 0, ad);
                Invoke((MethodInvoker)(() =>
                {
                    textBox1.Text = "広告再生中 これの再生時間を取る方法なんかないのか？";
                }));
                return;
            }

            if (playing.Item is FullTrack track)
            {
                EnumPlaybackEvent playbackEvent = playing.IsPlaying ? EnumPlaybackEvent.SeekPlay : EnumPlaybackEvent.SeekStop;

                if (LastTrack == null || track.Id != LastTrack.Id)
                {
                    TrackStartTimeStamp = playing.Timestamp;//Unix TimeStamp(ms)

                    long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    long progress = playing.ProgressMs.Value;

                    long offset = (TrackStartTimeStamp + progress - now);
                    TimeOffset = FirstLagTime;

                    MetaData data = new(track);
                    byte[] json = JsonSerializer.SerializeToUtf8Bytes<MetaData>(data);
                    Messenger.Update(playbackEvent, (progress + TimeOffset) / 1000.0, json);
                    LastTrack = track;
                    Invoke((MethodInvoker)(() =>
                    {
                        JsonSerializerOptions options = new()
                        {
                            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                            WriteIndented = true
                        };
                        textBox1.Text = JsonSerializer.Serialize<MetaData>(data, options) + $"\nLag:{offset}ms";
                    }));
                }
                else
                {
                    if (TrackStartTimeStamp != playing.Timestamp)
                    {
                        TimeOffset = 0;
                    }
                    Messenger.Update(playbackEvent, (playing.ProgressMs.Value + TimeOffset) / 1000.0);
                }
                return;
            }
            return;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            label_Countdown.Text = $"Next:{AutoGetter.RemainMs / 1000.0}";
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            long num = (long)(numericUpDown1.Value * 1000);
            if (TimeOffset == FirstLagTime)
            {
                TimeOffset = num;
            }
            FirstLagTime = num;
        }
    }
}
