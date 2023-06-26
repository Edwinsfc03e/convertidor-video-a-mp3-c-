
namespace WebApplication2.Controllers
{
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using YoutubeExplode;
    using YoutubeExplode.Videos.Streams;

    namespace WebApplication2.Controllers
    {
        [Route("api/[controller]")]
        [ApiController]
        public class YoutubeToMp3Controller : ControllerBase
        {
            private readonly YoutubeClient _youtubeClient;

            public YoutubeToMp3Controller()
            {
                _youtubeClient = new YoutubeClient();
            }

            [HttpPost("convert")]
            public async Task<IActionResult> ConvertToMp3([FromBody] string youtubeUrl)
            {
                var video = await _youtubeClient.Videos.GetAsync(youtubeUrl);
                var streamManifest = await _youtubeClient.Videos.Streams.GetManifestAsync(video.Id);
                var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();

                if (streamInfo != null)
                {
                    var stream = await _youtubeClient.Videos.Streams.GetAsync(streamInfo);
                    var outputStream = new MemoryStream();
                    await stream.CopyToAsync(outputStream);
                    outputStream.Seek(0, SeekOrigin.Begin);

                    var contentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
                    {
                        FileName = $"{video.Title}.mp3"
                    };
                    Response.Headers.Add("Content-Disposition", contentDisposition.ToString());

                    return File(outputStream, "audio/mpeg");
                }
                else
                {
                    return BadRequest("No se pudo obtener el stream de audio.");
                }
            }
        }
    }

}