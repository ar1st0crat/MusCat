using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using MusCat.Core.Interfaces.Networking;
using MusCat.Core.Interfaces.Tracklist;

namespace MusCat.Infrastructure.Services.Networking
{
    public class TracklistWebLoader : ITracklistWebLoader
    {
        public async Task<Track[]> LoadTracksAsync(string performer, string album)
        {
            var url = $@"https://www.google.com/search?q={performer}+{album}+discogs";

            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(url).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Bio could not be loaded! Error " + response.StatusCode);
                }

                var html = await response.Content.ReadAsStringAsync().ConfigureAwait(false);


                // find the first link to discogs.com and navigate there

                // for albums with the same name as artist's name (like "Beatles - Beatles")
                // the first link found will most probably navigate to the artist page (so skip it)

                var linkPos = 0;
                var link = "";

                while (!link.Contains("release"))
                {
                    linkPos = html.IndexOf("https://www.discogs.com", linkPos + 1);

                    if (linkPos < 0)
                    {
                        throw new Exception("Could not found album info!");
                    }

                    link = html.Substring(linkPos, html.IndexOf("&", linkPos + 1) - linkPos);
                }

                response = await client.GetAsync(link).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Bio could not be loaded! Error " + response.StatusCode);
                }

                html = await response.Content.ReadAsStringAsync().ConfigureAwait(false);


                var tracks = new List<Track>();

                html = html.Replace("itemprop=\"name\"", "");       // remove this occasional substring


                const string trackClass = "<span class=\"trackTitle";
                var offset = trackClass.Length + 8;


                var startPos = html.IndexOf(trackClass);

                byte trackNo = 1;
                while (startPos > -1)
                {
                    var track = html.Substring(startPos + offset, html.IndexOf("<", startPos + 1) - startPos - offset);

                    tracks.Add(new Track
                    {
                        No = trackNo++,
                        Title = HttpUtility.HtmlDecode(track)
                    });

                    startPos = html.IndexOf(trackClass, startPos + 1);
                }


                const string durationClass = "class=\"duration";
                const string spanTag = "<span>";
                offset = spanTag.Length;

                startPos = 0;

                for (var i = 0; i < tracks.Count; i++)
                {
                    startPos = html.IndexOf(durationClass, startPos);
                    startPos = html.IndexOf(spanTag, startPos);

                    tracks[i].Duration = html.Substring(startPos + offset, html.IndexOf("<", startPos + 1) - startPos - offset);
                }

                return tracks.ToArray();
            }
        }
    }
}
