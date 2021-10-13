using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp.Html.Parser;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
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
        public async Task Run([TimerTrigger("* * * * *")]TimerInfo myTimer,
        [CosmosDB(
                databaseName: "mafia-scraper-db",
                collectionName: "jpegmafiaProducts",
                ConnectionStringSetting = "CosmosDbConnectionString")] DocumentClient documentClient,
        ILogger log)
        {
            var client = _httpClientFactory.CreateClient();
            var request = await client.GetAsync("https://shop.jpegmafia.net");
            var result = await request.Content.ReadAsStringAsync();

            var parser = new HtmlParser();
            var document = await parser.ParseDocumentAsync(result);

            var productTitles = document.All
                .Where(d => d.ClassName == "thumb-title-wrap")
                .Select(d => d.TextContent.ToLowerInvariant().Trim());

            var collectionUri = UriFactory.CreateDocumentCollectionUri("mafia-scraper-db", "jpegmafiaProducts");

            var query = await documentClient.ReadDocumentFeedAsync(collectionUri);

            var productTitlesFromDb = query.Select(doc => (JpegmafiaProduct)(dynamic)doc).Select(product => product.Name).ToList();

            var titleIntersection = productTitles.Intersect(productTitlesFromDb).ToList();

            if (titleIntersection.Count != productTitlesFromDb.Count)
            {
                Console.WriteLine("new objects");
            }
/*             foreach (var title in productTitles)
            {
                await documentClient.CreateDocumentAsync(collectionUri, new
                {
                    id = Guid.NewGuid().ToString(),
                    name = title
                });
            } */

            /* log.LogInformation(string.Join('\n', productTitles)); */
        }
    }
}
