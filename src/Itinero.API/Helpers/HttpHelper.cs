using System.IO;
using System.Net.Http;

namespace Itinero.API.Helpers
{
    public class HttpHelper
    {
        public static Stream GetRoutingFileFromUrl(string routingFilePath)
        {
            using (var client = new HttpClient())
            {
                // New code:
                client.DefaultRequestHeaders.Accept.Clear();
                var stream = client.GetStreamAsync(routingFilePath).Result;
                return ZipHelper.Unzip(stream).Result;
            }
        }
    }
}
