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
            AutoGetter = new(Spotify, GetCallback);
            AutoGetter.Start(-1);
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

        private void GetCallback(CurrentlyPlaying playing)
        {
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
                    textBox1.Text = "No playing";
                }));
                return;
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
                    textBox1.Text = "広告再生中 これの再生時間を取る方法なんかないのか？";
                }));
                return;
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
                return;
            }
            return;
        }

    }
}
