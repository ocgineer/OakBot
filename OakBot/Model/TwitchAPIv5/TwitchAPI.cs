using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OakBot.Model
{
    public static class TwitchAPI
    {
        #region Fields

        // General JsonSerialized Settings
        private static readonly JsonSerializerSettings jSettings = new JsonSerializerSettings
        {
            // Ignore null value convert errors in the json blob
            NullValueHandling = NullValueHandling.Ignore
        };

        // Authenticated Token
        private static string AccessToken = string.Empty;

        #endregion

        /// <summary>
        /// Sets the obtained authenticated access token to use the Twitch API
        /// </summary>
        /// <param name="accessToken"></param>
        public static void SetAccessToken(TwitchCredentials caster)
        {
            // set the access token, remove oauth: if present
            AccessToken = caster.OAuth;
        }

        #region Users [2/15 Endpoints]
        /* https://dev.twitch.tv/docs/v5/reference/users */

        /// <summary>
        /// Gets the user objects for the specified Twitch login names.
        /// https://dev.twitch.tv/docs/v5/reference/users#get-users
        /// </summary>
        /// <param name="usernames"><see cref="IEnumerable{string}"/> of Twitch usernames to get.</param>
        /// <returns><see cref="IEnumerable{User}"/> of <see cref="User"/> objects.</returns>
        public static async Task<IEnumerable<User>> GetUsers(IEnumerable<string> usernames)
        {
            // Create an empty list to fill with results
            List<User> TranslatedUsers = new List<User>();

            // If the IEnumerable has more than 100 names to lookup, split in batches
            if (usernames.Count() > 100)
            {
                // Using pure LINQ to group and create batches of 100 names
                foreach (var batch in usernames.Select((x, index) => new { x, index }).GroupBy(x => x.index / 100, y => y.x))
                {
                    // Translate this batch of max 100 selected names
                    TranslatedUsers.AddRange(await GetUsers(batch));

                    // Delay 500ms before requesting API endpoint again.
                    await Task.Delay(1000);
                }
            }

            // IEnumerable has 100 or less names to lookup
            else
            {
                // Join strings in the list with a comma as seperator
                string concatUsernames = string.Join(",", usernames);

                try
                {
                    var blob = TwitchApiRequests.GetRequest($"https://api.twitch.tv/kraken/users?login={concatUsernames}");
                    TranslatedUsers.AddRange(JsonConvert.DeserializeObject<GetUsers>(await blob, jSettings).Users);
                }
                catch
                {
                    // Ignore if API or Deserialization error
                    // will just return an empty enumerable.
                }
            }

            // Return list as enumerable fetched User objects
            return TranslatedUsers.AsEnumerable();
        }

        /// <summary>
        /// Gets a single user object for the specified Twitch login name.
        /// https://dev.twitch.tv/docs/v5/reference/users#get-users
        /// </summary>
        /// <param name="name">Twitch username to get user object for.</param>
        /// <returns><see cref="User"/> object on success, null otherwise.</returns>
        public static async Task<User> GetUsers(string username)
        {
            try
            {
                var blob = await TwitchApiRequests.GetRequest($"https://api.twitch.tv/kraken/users?login={username}");
                return JsonConvert.DeserializeObject<GetUsers>(blob, jSettings).Users[0];
            }
            catch (WebException)
            {
                return null;
            }
            catch (JsonSerializationException)
            {
                return null;
            }
        }

        /// <summary>
        /// Checks if a specified user follows a specified channel.
        /// https://dev.twitch.tv/docs/v5/reference/users#check-user-follows-by-channel
        /// </summary>
        /// <param name="userId">The id of the user to check.</param>
        /// <param name="channelId">The id of the channel to check.</param>
        /// <returns>If the user is following the channel, a <see cref="Follows"/> object is returned, else <see cref="null"/>.</returns>
        /// <exception cref="WebException">On anything other than a 404.</exception>
        public static async Task<Follows> CheckUserFollowsByChannel(string userId, string channelId)
        {
            try
            {
                // Make the request
                var blob = await TwitchApiRequests.GetRequest($"https://api.twitch.tv/kraken/users/{userId}/follows/channels/{channelId}");

                // If the response returned successful then the
                // specified user is following the specified channel
                return JsonConvert.DeserializeObject<Follows>(blob, jSettings);
            }
            catch (WebException ex)
            {
                // If the response returned a webexception with response 404 then the
                // specified user is not following the specified channel, return null
                if (((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.NotFound)
                    return null;

                // If response returned any other webexception, throw exception to handle
                throw new Exception($"{((HttpWebResponse)ex.Response).StatusCode}");
            }
        }

        #endregion

        #region Streams [1/5 Endpoints]
        /* https://dev.twitch.tv/docs/v5/reference/streams */

        /// <summary>
        /// WARNING: HACKY METHOD 1000ms between API calls, Future will only accept userids
        /// Gets stream information for a specified user.
        /// https://dev.twitch.tv/docs/v5/reference/streams#get-stream-by-user
        /// </summary>
        /// <param name="channelName">username of the user to fetch the stream from.</param>
        /// <returns>Returns <see cref="TwitchStream"/> when a stream is live, null if not or on error.</returns>
        public static async Task<TwitchStream> GetStreamByUser(string channelName)
        {
            try
            {
                // Make the request to get user id from username
                User user = await GetUsers(channelName);
                if (user == null)
                    return null;
                await Task.Delay(500);
                
                // Make the request
                var blob = await TwitchApiRequests.GetRequest($"https://api.twitch.tv/kraken/streams/{user.Id}");

                // Deserialize into a JObject, not using reflection here yet.
                JToken blobObj = JObject.Parse(blob);

                // Check if 'stream' value is given or not 
                if (blobObj["stream"] != null)
                {
                    // Stream is online, return TwitchStream object
                    return blobObj.SelectToken("stream").ToObject<TwitchStream>();
                }
                else
                {
                    // Stream is offline, return null
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }


        #endregion
    }
}
