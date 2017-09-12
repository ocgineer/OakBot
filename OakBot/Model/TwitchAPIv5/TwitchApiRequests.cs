using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace OakBot.Model
{
    internal static class TwitchApiRequests
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
            request.Accept = $"application/vnd.twitchtv.v5+json";
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
            request.Accept = $"application/vnd.twitchtv.v5+json";
            request.Method = "GET";
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

        /// <summary>
        /// Make a Twitch APIv5 authenticated REST request to the specified URL.
        /// </summary>
        /// <param name="endpoint">The URL of the REST request, without Client-ID or Authorization.</param>
        /// <param name="method">The method to use; PUT, POST, or DELETE</param>
        /// <param name="oauth">Authentication oauth to make a REST request.</param>
        /// <param name="payload">The data payload as object to send.</param>
        /// <returns>The received response string.</returns>
        /// <exception cref="WebException"/>
        internal static async Task<string> RestRequest(string endpoint, string method, string oauth, object payload)
        {
            // Create HttpWebRequest then set the method and required Twitch API headers
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(endpoint));
            request.Accept = $"application/vnd.twitchtv.v5+json";
            request.ContentType = "application/json";
            request.Method = method;
            request.Headers.Add("Authorization", $"OAuth {oauth}");

            // Transmit payload if available
            if (payload != null)
            {
                // Serialize the payload object to json string and convert it to bytes in a buffer.
                byte[] data = new UTF8Encoding().GetBytes(JsonConvert.SerializeObject(payload));

                using (Stream writer = await request.GetRequestStreamAsync())
                {
                    await writer.WriteAsync(data, 0, data.Length);
                }
            }

            // Get the request response, handle WebExceptions in the endpoint methods
            using (WebResponse response = await request.GetResponseAsync())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8, true))
                {
                    return await reader.ReadToEndAsync();
                }
            }
        }
    }
}
