using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using MusCat.Core.Interfaces.Networking;

namespace MusCat.Infrastructure.Services.Networking
{
    public class LastfmDataLoader : IWebDataLoader
    {
        private readonly string[] _newlineTags =
        {
            "<p>", "</p>", "<br/>", "<br />", "<br>"
        };

        private readonly string[] _ignoreTags =
        {
            "<em>", "</em>", "<strong>", "</strong>", "</a>"
        };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="performer"></param>
        /// <returns></returns>
        public async Task<string> LoadBioAsync(string performer)
        {
            var url = $@"https://www.last.fm/music/{performer}/+wiki";

            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(url).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Bio could not be loaded! Error " + response.StatusCode);
                }

                var html = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                const string pattern = "<div class=\"wiki-content\" itemprop=\"description\">";

                var startPos = html.IndexOf(pattern) + pattern.Length;
                var endPos = html.IndexOf("</div>", startPos);

                var bio = new StringBuilder(html.Substring(startPos, endPos - startPos).Trim());

                foreach (var tag in _newlineTags)
                {
                    bio.Replace(tag, "\n");
                }

                foreach (var tag in _ignoreTags)
                {
                    bio.Replace(tag, "");
                }

                var bioText = bio.ToString();
                var linkPos = bioText.IndexOf("<a href");

                while (linkPos >= 0)
                {
                    var linkEnd = bioText.IndexOf(">", linkPos + 1);
                    bio = bio.Remove(linkPos, linkEnd - linkPos + 1);
                    bioText = bio.ToString();
                    linkPos = bioText.IndexOf("<a href");
                }

                return HttpUtility.HtmlDecode(bioText);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="performer"></param>
        /// <param name="album"></param>
        /// <returns></returns>
        public async Task<Tuple<string[], string[]>> LoadTracksAsync(string performer, string album)
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

                var linkPos = html.IndexOf("https://www.discogs.com");

                var link = html.Substring(linkPos, html.IndexOf("&", linkPos + 1) - linkPos);

                response = await client.GetAsync(link).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Bio could not be loaded! Error " + response.StatusCode);
                }

                html = await response.Content.ReadAsStringAsync().ConfigureAwait(false);


                var tracks = new List<string>();

                html = html.Replace("itemprop=\"name\"", "");       // remove this occasional substring


                const string trackClass = "<span class=\"tracklist_track_title\">";
                var offset = trackClass.Length;

                var startPos = html.IndexOf(trackClass);
                
                while (startPos > -1)
                {
                    var track = html.Substring(startPos + offset, html.IndexOf("<", startPos + 1) - startPos - offset);

                    tracks.Add(HttpUtility.HtmlDecode(track));

                    startPos = html.IndexOf(trackClass, startPos + 1);
                }


                var durations = new string[tracks.Count];

                const string durationClass = "class=\"tracklist_track_duration\"";
                const string spanTag = "<span>";
                offset = spanTag.Length;

                startPos = 0;

                for (var i = 0; i < tracks.Count; i++)
                {
                    startPos = html.IndexOf(durationClass, startPos);
                    startPos = html.IndexOf(spanTag, startPos);

                    durations[i] = html.Substring(startPos + offset, html.IndexOf("<", startPos + 1) - startPos - offset);
                }

                return new Tuple<string[], string[]>(tracks.ToArray(), durations);
            }
        }
    }
}
