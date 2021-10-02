using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TitalyverMessengerForSpotify
{
    public class MetaData
    {
        public MetaData(SpotifyAPI.Web.FullTrack track)
        {
            meta = new();
            meta["name"] = title = track.Name;
            meta["artist"] = artists = track.Artists.Select(a => a.Name).ToArray();
            meta["album"] = album = track.Album.Name;
            duration = track.DurationMs / 1000.0;
            meta["id"] = track.Id;
        }
        public MetaData(string title,string[] artists,string album,double duration)
        {
            meta = new() { { "name", title }, { "artist", artists }, { "album", album } };
            this.title = title;
            this.artists = artists;
            this.album = album;
            this.duration = duration;
        }

        public string path { get; set; } = "";
        public string title { get; set; } = "";
        public string[] artists { get; set; } = { "" };
        public string album { get; set; } = "";
        public double duration { get; set; } = 0;
        public Dictionary<string, object> meta { get; set; }
    }
}
