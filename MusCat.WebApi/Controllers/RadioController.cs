using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;
using System.Web.Http;
using MusCat.Infrastructure.Services;
using MusCat.Infrastructure.Services.Radio;

namespace MusCat.WebApi.Controllers
{
    /// <summary>
    /// TODO: call win service (self-hosted web api) here
    /// </summary>
    public class RadioController : ApiController
    {
        //[HttpGet]
        //public HttpResponseMessage Stream(HttpRequestMessage request)
        //{
        //    var response = Request.CreateResponse();

        //    response.Content = new PushStreamContent((a, b, c) =>
        //    {
        //        var path = FileLocator.FindSongPath(RadioService.CurrentSong);

        //        var audio = new HttpFileStream(path);

        //        audio.WriteToStream(a, b, c);
        //    }, 
        //    "audio/mpeg");

        //    return response;
        //}

        //[HttpGet]
        //[Route("api/radio/current")]
        //public HttpResponseMessage Current(HttpRequestMessage request)
        //{
        //}

        //[HttpGet]
        //[Route("api/radio/upcoming")]
        //public HttpResponseMessage Upcoming(HttpRequestMessage request)
        //{
        //}

        //[HttpGet]
        //[Route("api/radio/archive")]
        //public HttpResponseMessage Archive(HttpRequestMessage request)
        //{
        //}

        //[HttpGet]
        //[Route("api/radio/next")]
        //public async Task NextAsync()
        //{
        //}
    }
}
