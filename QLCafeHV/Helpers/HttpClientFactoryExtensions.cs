using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace QLCafeHV.Helpers
{
    public static class HttpClientFactoryExtensions
    {
        // Extension method cho IHttpClientFactory
        public static HttpClient PortalAPIs(this IHttpClientFactory factory)
        {           
            return factory.CreateClient();
        }
        public static async Task<T> PostAsync<T>(this HttpClient client, object body, string url)
        {
            var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, content);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(result);
        }
    }
}
