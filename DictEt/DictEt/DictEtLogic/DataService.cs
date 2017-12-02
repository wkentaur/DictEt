using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;

namespace DictEtLogic
{
    public static class DataService
    {
        private static readonly HttpClient myHttpClient;

        static DataService()
        {
            // HttpClient is intended to be instantiated once and re-used throughout the life of an application. 
            // Instantiating an HttpClient class for every request will exhaust the number of sockets available under heavy loads. 
            // This will result in SocketException errors.
            // https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient?view=netframework-4.7.1
            myHttpClient = new HttpClient();

            myHttpClient.DefaultRequestHeaders.Accept.Clear();
            myHttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            // tools.wmflabs.org wants the User-Agent
            myHttpClient.DefaultRequestHeaders.Add("User-Agent", "WordBrowser");
        }

        public static async Task<dynamic> GetDataFromService(string queryString)
        {
            HttpResponseMessage response = await myHttpClient.GetAsync(queryString).ConfigureAwait(false);
            dynamic data = null;

            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                data = JsonConvert.DeserializeObject(json);
            }

            return data;
        }

        public static async Task<dynamic> GetDataFromService(string queryString, CancellationToken ct)
        {
            HttpResponseMessage response = await myHttpClient.GetAsync(queryString, ct).ConfigureAwait(false);
            dynamic data = null;

            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                data = JsonConvert.DeserializeObject(json);
            }

            return data;
        }

    }
}
