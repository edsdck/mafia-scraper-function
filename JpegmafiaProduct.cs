using Newtonsoft.Json;

namespace MafiaScraper.Jpegmafia
{
    public class JpegmafiaProduct
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}