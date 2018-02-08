using System.IO;
using System.Net;
using System.Net.Http;
using System.Web;

namespace MusCat.WebApi.Controllers
{
    public class AudioStream
    {
        // This will be used in copying input stream to output stream.
        public const int ReadStreamBufferSize = 1024 * 1024;

        private readonly string _path;

        public AudioStream(string path)
        {
            _path = path;
        }

        public async void WriteToStream(Stream outputStream, HttpContent content, TransportContext context)
        {
            try
            {
                var buffer = new byte[ReadStreamBufferSize];

                using (Stream stream = new FileStream(_path, FileMode.Open, FileAccess.Read))
                {
                    int count = 0;
                    do
                    {
                        count = stream.Read(buffer, 0, ReadStreamBufferSize);
                        await outputStream.WriteAsync(buffer, 0, count);
                    }
                    while (stream.CanRead && count > 0);
                }
            }
            catch (HttpException)
            {
                // do smth.
                return;
            }
            finally
            {
                outputStream.Close();
            }
        }
    }
}