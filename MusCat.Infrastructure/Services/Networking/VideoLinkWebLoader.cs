using MusCat.Core.Interfaces.Networking;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace MusCat.Infrastructure.Services.Networking
{
    public class VideoLinkWebLoader : IVideoLinkWebLoader
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        public async Task<string[]> LoadVideoLinksAsync(string performer, string song, int linkCount = 5)
        {
            var query = HttpUtility.UrlEncode($"{performer} {song}");
            var url = $"https://www.youtube.com/results?search_query={query}";

            var response = await _httpClient.GetAsync(url).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var html = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var links = new string[linkCount];

            int pos = 0;
            for (int i = 0; i < links.Length; i++)
            {
                pos = html.IndexOf("\"url\":\"/watch", pos + 1);

                if (pos < 0) break;

                pos += 7;
                var startPos = pos;
                pos = html.IndexOf("\"", pos + 1);
                var addr = html.Substring(startPos + 9, pos - startPos - 9);

                links[i] = $"https://www.youtube.com/embed/{addr}";

                //var link = $"https://www.youtube.com/embed/{addr}";
                //links[i] = $"<html><body><iframe src=\"{link}\" width=\"100%\" height=\"100%\" frameborder=\"0\" allowfullscreen></iframe></body></html>";
            }

            return links;
        }
    }
}
