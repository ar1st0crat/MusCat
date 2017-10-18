using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MusCat.Entities;

namespace MusCat.Services.Networking
{
    class LastfmDataLoader : IWebDataLoader
    {
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

                bio = bio.Replace("<p>", "\n")
                         .Replace("</p>", "\n")
                         .Replace("<br/>", "\n")
                         .Replace("<br />", "\n")
                         .Replace("<em>", "")
                         .Replace("</em>", "")
                         .Replace("<strong>", "")
                         .Replace("</strong>", "")
                         .Replace("&amp;", "&")
                         .Replace("&nbsp;", " ")
                         .Replace("&quot;", "\"")
                         .Replace("&#x27;", "'")
                         .Replace("</a>", "");

                var bioText = bio.ToString();
                var linkPos = bioText.IndexOf("<a href");

                while (linkPos >= 0)
                {
                    var linkEnd = bioText.IndexOf(">", linkPos + 1);
                    bio = bio.Remove(linkPos, linkEnd - linkPos + 1);
                    bioText = bio.ToString();
                    linkPos = bioText.IndexOf("<a href");
                }

                return bioText;
            }
        }
    }
}
