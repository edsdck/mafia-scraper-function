using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace MafiaScraper.Jpegmafia
{
    public class JpegmafiaFunction
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public JpegmafiaFunction(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [FunctionName("JpegmafiaFunction")]
        public async Task Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, ILogger log)
        {
            var client = _httpClientFactory.CreateClient();
            var req = await client.GetAsync("");
        }
    }
}
