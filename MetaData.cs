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
            meta["title"] = track.Name;
            meta["artist"] = track.Artists.Select(a => a.Name).ToArray();
            meta["album"] = track.Album.Name;
        }
        public MetaData(string title,string[] artists,string album)
        {
            meta = new() { { "title", title }, { "artist", artists }, { "album", album } };
        }

        public string path { get; set; } = "";
        public Dictionary<string, object> meta { get; set; }
    }
}
