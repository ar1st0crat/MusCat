using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using MusCat.Core.Interfaces.Networking;

namespace MusCat.Infrastructure.Services.Networking
{
    public class LyricsWebLoader : ILyricsWebLoader
    {
        private readonly HttpClient _httpClient;

        public LyricsWebLoader(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Load lyrics from ChartLyrics API
        /// </summary>
        /// <param name="performer">Performer name</param>
        /// <param name="song">Song title</param>
        /// <returns>Song lyrics</returns>
        public async Task<string> LoadLyricsAsync(string performer, string song)
        {
            var performerUrl = HttpUtility.UrlEncode(performer);
            var songUrl = HttpUtility.UrlEncode(song);

            var url = $"http://api.chartlyrics.com/apiv1.asmx/SearchLyric?artist={performerUrl}&song={songUrl}";

            var response = await _httpClient.GetAsync(url).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                return $"Lyrics could not be loaded! Error {response.StatusCode}";
            }

            var html = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var xdoc = XDocument.Parse(html);
            var ns = xdoc.Root.Name.Namespace;

            var result = xdoc.Descendants(ns + "SearchLyricResult")
                             .Where(e => e.HasElements)
                             .FirstOrDefault(e => e.Element(ns + "Song").Value.ToLowerInvariant() == song.ToLowerInvariant());

            if (result == null)
            {
                return "No lyrics";
            }


            var id = result.Element(ns + "LyricId").Value;

            if (id == "0")
            {
                return "No lyrics";
            }

            var checksum = result.Element(ns + "LyricChecksum").Value;

            url = $"http://api.chartlyrics.com/apiv1.asmx/GetLyric?lyricId={id}&lyricCheckSum={checksum}";

            response = await _httpClient.GetAsync(url).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                return $"Lyrics could not be loaded! Error {response.StatusCode}";
            }

            html = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            xdoc = XDocument.Parse(html);
            ns = xdoc.Root.Name.Namespace;

            var lyrics = xdoc.Descendants(ns + "Lyric").First().Value;

            return lyrics;
        }
    }
}
