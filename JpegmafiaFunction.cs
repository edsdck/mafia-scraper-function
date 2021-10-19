using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp.Html.Parser;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace MafiaScraper.Jpegmafia
{
    public class JpegmafiaFunction
    {
        private const string _databaseName = "mafia-scraper-db";
        private const string _collectionName = "jpegmafiaProducts";
        private static Uri _collectionUri = UriFactory.CreateDocumentCollectionUri(_databaseName, _collectionName);

        private readonly IHttpClientFactory _httpClientFactory;

        public JpegmafiaFunction(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [FunctionName("JpegmafiaFunction")]
        public async Task Run([TimerTrigger("* * * * *")]TimerInfo myTimer,
        [CosmosDB(
                databaseName: _databaseName,
                collectionName: _collectionName,
                ConnectionStringSetting = "CosmosDbConnectionString")] DocumentClient documentClient,
        ILogger log)
        {
            var products = await ScrapeProducts();
            var dbProducts = await QueryProducts(documentClient);

            var titleIntersection = products.Intersect(dbProducts).ToList();

            log.LogInformation("TASK COMPLETED!");

            if (titleIntersection.Count == dbProducts.Count &&
                titleIntersection.Count == products.Count)
            {
                return;
            }
            /*             foreach (var title in productTitles)
                        {
                            await documentClient.CreateDocumentAsync(_collectionUri, new
                            {
                                id = Guid.NewGuid().ToString(),
                                name = title
                            });
                        } */

            /* log.LogInformation(string.Join('\n', productTitles)); */
        }

        private async Task<IList<string>> ScrapeProducts()
        {
            var client = _httpClientFactory.CreateClient();
            var request = await client.GetAsync("https://shop.jpegmafia.net");
            var result = await request.Content.ReadAsStringAsync();

            var parser = new HtmlParser();
            var document = await parser.ParseDocumentAsync(result);

            var productTitles = document.All
                .Where(d => d.ClassName == "thumb-title-wrap")
                .Select(d => d.TextContent.ToLowerInvariant().Trim());

            return productTitles.ToList();
        }

        private async Task<IList<string>> QueryProducts(DocumentClient documentClient)
        {
            var query = await documentClient.ReadDocumentFeedAsync(_collectionUri);

            return query.Select(doc => (JpegmafiaProduct)(dynamic)doc).Select(product => product.Name).ToList();
        }
    }
}
