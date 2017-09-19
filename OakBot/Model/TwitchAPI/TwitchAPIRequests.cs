using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OakBot.Model
{
    internal class TwitchAPIRequests
    {
        // Twitch API Private Client ID
        private static readonly string _clientId = "gtpc5vtk1r4u8fm9l45f9kg1fzezrv8";

        /// <summary>
        /// Make a unauthenticated Twitch APIv5 GET request to the specified endpoint.
        /// </summary>
        /// <param name="endpoint">The URL of the GET request, without Client-ID or Authorization.</param>
        /// <returns>The received unparsed response string.</returns>
        /// <exception cref="WebException"/>
        internal static async Task<string> GetRequest(string endpoint)
        {
            // Create HttpWebRequest then set the method and required Twitch API headers
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(endpoint));
            request.Method = "GET";
            request.Headers.Add("Client-ID", _clientId);

            // Get the request response, handle WebExceptions in the endpoint methods
            using (WebResponse response = await request.GetResponseAsync())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    return await reader.ReadToEndAsync();
                }
            }
        }

        /// <summary>
        /// Make a authenticated Twitch APIv5 GET request to the specified endpoint.
        /// </summary>
        /// <param name="endpoint">The URL of the GET request, without Client-ID or Authorization.</param>
        /// <param name="oauth">Authenticated Oauth to make an authenticated call.</param>
        /// <returns>The received unparsed response string.</returns>
        /// <exception cref="WebException"/>
        internal static async Task<string> GetRequest(string endpoint, string oauth)
        {
            // Create HttpWebRequest then set the method and required Twitch API headers
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(endpoint));
            request.Method = "GET";
            request.Headers.Add("Client-ID", _clientId);
            request.Headers.Add("Authorization", $"OAuth {oauth}");

            // Get the request response, handle WebExceptions in the endpoint methods
            using (WebResponse response = await request.GetResponseAsync())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    return await reader.ReadToEndAsync();
                }
            }
        }
    }
}
