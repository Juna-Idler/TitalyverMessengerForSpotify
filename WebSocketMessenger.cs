using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Titalyver2;

namespace TitalyverMessengerForSpotify
{
    class WebSocketMessenger
    {
        public ClientWebSocket WebSocket = null;

        Uri Uri = new("ws://127.0.0.1:14738");

        string Title;
        string[] Artists;
        string Album;
        double Duration;

        public WebSocketMessenger()
        {
        }
        ~WebSocketMessenger()
        {
            Terminalize();
        }

        public static int GetUtcTimeOfDay()
        {
            DateTime now = DateTime.UtcNow;
            return ((now.Hour * 60 + now.Minute) * 60 + now.Second) * 1000 + now.Millisecond;
        }


        public bool Initialize()
        {
            Terminalize();
            try
            {
                WebSocket = new ClientWebSocket();
            }
            catch (Exception e)
            {
                Terminalize();
                Debug.WriteLine(e.Message);
                return false;
            }
            return true;
        }

        public void Terminalize()
        {
            WebSocket?.Dispose();
            WebSocket = null;
        }

        public async Task<bool> UpdateAsync(EnumPlaybackEvent pbevent, double seektime,
                string title,string[] artists,string album,double duration)
        {
            Title = title;
            Artists = artists;
            Album = album;
            Duration = duration;

            switch (WebSocket.State)
            {
                case WebSocketState.Open:
                    break;
                case WebSocketState.Connecting:
                    break;
                default:
                    WebSocket = new ClientWebSocket();
                    await WebSocket.ConnectAsync(Uri, CancellationToken.None);
                    byte[] buffer = new byte[8];
                    _ = WebSocket.ReceiveAsync(buffer, CancellationToken.None);
                    break;
            }
            if (WebSocket.State != WebSocketState.Open)
                return false;

            string json = "{" +
                "\"event\":" + (int)pbevent + "," +
                "\"seek\":" + seektime + "," +
                "\"time\":" + GetUtcTimeOfDay() + "," +
                "\"path\":" + "\"\"" + "," +
                "\"title\":\"" + title + "\"," +
                "\"artists\":[\"" + string.Join("\",\"", artists) + "\"]," +
                "\"album\":\"" + album + "\"," +
                "\"duration\":" + duration + "," +
                "\"meta\":{}" +
                "}";

            byte[] utf8 = Encoding.UTF8.GetBytes(json);
            await WebSocket.SendAsync(utf8, WebSocketMessageType.Text, true, CancellationToken.None);

            return true;
        }

        public async Task<bool> UpdateAsync(EnumPlaybackEvent pbevent, double seektime)
        {
            switch (WebSocket.State)
            {
                case WebSocketState.Open:
                    {
                        string json = "{" +
                            "\"event\":" + (int)pbevent + "," +
                            "\"seek\":" + seektime + "," +
                            "\"time\":" + GetUtcTimeOfDay() + "," +
                            "}";

                        byte[] utf8 = Encoding.UTF8.GetBytes(json);
                        await WebSocket.SendAsync(utf8, WebSocketMessageType.Text, true, CancellationToken.None);
                        return true;
                    }
                case WebSocketState.Connecting:
                    break;
                default:
                    WebSocket = new ClientWebSocket();
                    await WebSocket.ConnectAsync(Uri, CancellationToken.None);
                    byte[] buffer = new byte[8];
                    _ = WebSocket.ReceiveAsync(buffer, CancellationToken.None);
                    break;
            }
            if (WebSocket.State != WebSocketState.Open)
                return false;
            {
                string json = "{" +
                    "\"event\":" + (int)pbevent + "," +
                    "\"seek\":" + seektime + "," +
                    "\"time\":" + GetUtcTimeOfDay() + "," +
                    "\"path\":" + "\"\"" + "," +
                    "\"title\":\"" + Title + "\"," +
                    "\"artists\":[\"" + string.Join("\",\"", Artists) + "\"]," +
                    "\"album\":\"" + Album + "\"," +
                    "\"duration\":" + Duration + "," +
                    "\"meta\":{}" +
                    "}";

                byte[] utf8 = Encoding.UTF8.GetBytes(json);
                await WebSocket.SendAsync(utf8, WebSocketMessageType.Text, true, CancellationToken.None);
                return true;
            }
        }


    }
}
