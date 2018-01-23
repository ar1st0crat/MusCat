using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using MusCat.Core.Entities;
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

        public async Task<string> LoadBioAsync(Performer performer)
        {
            var url = string.Format(@"https://www.last.fm/music/{0}/+wiki", performer.Name);

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
    }
}
