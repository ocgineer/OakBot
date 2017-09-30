using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OakBot.Model
{
    internal class TwitchAPI
    {
        #region Fields

        // General JsonSerialized Settings
        private static readonly JsonSerializerSettings jSettings = new JsonSerializerSettings
        {
            // Ignore null value convert errors in the json blob
            NullValueHandling = NullValueHandling.Ignore
        };

        #endregion

        #region Get Users Endpoint
        /* https://dev.twitch.tv/docs/api/reference#get-users */

        /// <summary>
        /// Gets information about the user associated with the supplied oauth.
        /// Returns <see cref="User"/> model of the user associated with supplied oauth.
        /// </summary>
        /// <param name="caster">Caster <see cref="TwitchCredentials"/> containing a set oauth.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="TwitchAPIException"/>
        internal static async Task<User> GetUser(TwitchCredentials caster)
        {
            if (caster == null)
            {
                throw new ArgumentNullException("caster", "Argument cannot be null.");
            }

            try
            {
                var blob = await TwitchAPIRequests.GetRequest($"https://api.twitch.tv/helix/users", caster.OAuth);
                return (JsonConvert.DeserializeObject<TwitchApiResponse<User>>(blob, jSettings)).Data[0];
            }
            catch (WebException ex)
            {
                throw new TwitchAPIException(ex.Message, ((HttpWebResponse)ex.Response).StatusCode);
            }
        }

        /// <summary>
        /// Gets information about one specified Twitch user by login.
        /// Returns <see cref="User"/> model if user exists, <see cref="null"/> if not.
        /// </summary>
        /// <param name="login">Login of the user to get information about.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="TwitchAPIException"/>
        internal static async Task<User> GetUserByLogin(string login)
        {
            // If id is not given, throw exception
            if (string.IsNullOrWhiteSpace(login))
            {
                throw new ArgumentNullException("login", "Given login cannot be empty, whitespace, or null.");
            }

            try
            {
                var blob = await TwitchAPIRequests.GetRequest($"https://api.twitch.tv/helix/users?login={login}");
                return (JsonConvert.DeserializeObject<TwitchApiResponse<User>>(blob, jSettings)).Data[0];
            }
            catch (ArgumentOutOfRangeException)
            {
                // User does not exists
                return null;
            }
            catch (WebException ex)
            {
                throw new TwitchAPIException(ex.Message, ((HttpWebResponse)ex.Response).StatusCode);
            }
        }

        /// <summary>
        /// Gets information about one specified Twitch user by id.
        /// Returns <see cref="User"/> model if user exists, <see cref="null"/> if not.
        /// </summary>
        /// <param name="login">Id of the user to get information about.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="TwitchAPIException"/>
        internal static async Task<User> GetUserById(string id)
        {
            // If id is not given, throw exception
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException("id", "Given id cannot be empty, whitespace, or null.");
            }

            try
            {
                var blob = await TwitchAPIRequests.GetRequest($"https://api.twitch.tv/helix/users?id={id}");
                return (JsonConvert.DeserializeObject<TwitchApiResponse<User>>(blob, jSettings)).Data[0];
            }
            catch (ArgumentOutOfRangeException)
            {
                // User does not exists
                return null;
            }
            catch (WebException ex)
            {
                throw new TwitchAPIException(ex.Message, ((HttpWebResponse)ex.Response).StatusCode);
            }
        }

        /// <summary>
        /// Gets information about one or more specified Twitch users by login, up to 100 at a time.
        /// Returns an enumerable of <see cref="User"/> models containing existing users.
        /// </summary>
        /// <param name="logins"><see cref="IEnumerable{T}"/> of type <see cref="string"/> containing logins of users to get information about, maximum 100 logins.</param>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="TwitchAPIException"/>
        internal static async Task<IEnumerable<User>> GetUsersByLogin(IEnumerable<string> logins)
        {
            // More than 100 elements in the IEnumerable
            if (logins.Count() > 100)
            {
                throw new ArgumentOutOfRangeException("logins", "IEnumerable of logins can only contain up to 100 elements.");
            }

            // Return empty enumerable if there are 0 elements in the IEnumerable
            if (logins.Count() == 0)
            {
                return Enumerable.Empty<User>();
            }

            // Join strings in the list with query as seperator
            string concat = string.Join("&login=", logins);

            try
            {
                var blob = await TwitchAPIRequests.GetRequest($"https://api.twitch.tv/helix/users?login={concat}");
                return (JsonConvert.DeserializeObject<TwitchApiResponse<User>>(blob, jSettings)).Data;
            }
            catch (WebException ex)
            {
                throw new TwitchAPIException(ex.Message, ((HttpWebResponse)ex.Response).StatusCode);
            }
        }

        /// <summary>
        /// Gets information about one or more specified Twitch users by id up to 100 at a time.
        /// Returns an enumerable of <see cref="User"/> models containing existing users.
        /// </summary>
        /// <param name="ids"><see cref="IEnumerable{T}"/> of type <see cref="string"/> containing ids of users to get information about, maximum 100 logins.</param>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="TwitchAPIException"/>
        internal static async Task<IEnumerable<User>> GetUsersById(IEnumerable<string> ids)
        {
            // More than 100 elements in the IEnumerable
            if (ids.Count() > 100)
            {
                throw new ArgumentOutOfRangeException("ids", "IEnumerable of ids can only contain up to 100 elements.");
            }

            // Return empty enumerable if there are 0 elements in the IEnumerable
            if (ids.Count() == 0)
            {
                return Enumerable.Empty<User>();
            }

            // Join strings in the list with a comma as seperator
            string concat = string.Join("&id=", ids);

            try
            {
                var blob = await TwitchAPIRequests.GetRequest($"https://api.twitch.tv/helix/users?id={concat}");
                return (JsonConvert.DeserializeObject<TwitchApiResponse<User>>(blob, jSettings)).Data;

            }
            catch (WebException ex)
            {
                throw new TwitchAPIException(ex.Message, ((HttpWebResponse)ex.Response).StatusCode);
            }
        }

        #endregion

        #region Get Users Follows Endpoint
        /* https://dev.twitch.tv/docs/api/reference#get-users-follows */

        /// <summary>
        /// Gets information on follow relationship between two Twitch ids.
        /// Returns <see cref="Follows"/> model if user is following the target, <see cref="null"/> otherwise.
        /// </summary>
        /// <param name="userId">Id of the user to check the relationship towards targetId.</param>
        /// <param name="targetId">Id of the user as target to check relationship from userId.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="TwitchAPIException"/>
        internal static async Task<Follows> GetUserIsFollowingTarget(string userId, string targetId)
        {
            // If user id is not given, throw exception
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentNullException("userId", "Given id of the user cannot be empty, whitespace, or null");
            }

            // If target id is not given, throw exception
            if (string.IsNullOrWhiteSpace(targetId))
            {
                throw new ArgumentNullException("targetId", "Given id of the target cannot be empty, whitespace, or null");
            }

            try
            {
                var blob = await TwitchAPIRequests.GetRequest($"https://api.twitch.tv/helix/users/follows?from_id={userId}&to_id={targetId}");
                return (JsonConvert.DeserializeObject<TwitchApiResponse<Follows>>(blob, jSettings)).Data[0];
            }
            catch (ArgumentOutOfRangeException)
            {
                // User is not following
                return null;
            }
            catch (WebException ex)
            {
                throw new TwitchAPIException(ex.Message, ((HttpWebResponse)ex.Response).StatusCode);
            }
        }

        /// <summary>
        /// Gets information on follow relationship between two Twitch ids.
        /// Returns type <see cref="bool"/> true if user is following the target, false otherwise.
        /// </summary>
        /// <param name="userId">Id of the user to check the relationship towards targetId.</param>
        /// <param name="targetId">Id of the user as target to check relationship from userId.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="TwitchAPIException"/>
        internal static async Task<bool> GetUserIsFollowingTargetBool(string userId, string targetId)
        {
            // If user id is not given, throw exception
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentNullException("userId", "Given id of the user cannot be empty, whitespace, or null");
            }

            // If target id is not given, throw exception
            if (string.IsNullOrWhiteSpace(targetId))
            {
                throw new ArgumentNullException("targetId", "Given id of the target cannot be empty, whitespace, or null");
            }

            try
            {
                var blob = await TwitchAPIRequests.GetRequest($"https://api.twitch.tv/helix/users/follows?from_id={userId}&to_id={targetId}");
                return (JsonConvert.DeserializeObject<TwitchApiResponse<Follows>>(blob, jSettings)).Data.Count > 0 ? true : false;
            }
            catch (WebException ex)
            {
                throw new TwitchAPIException(ex.Message, ((HttpWebResponse)ex.Response).StatusCode);
            }
        }

        /// <summary>
        /// Gets followers from given user id.
        /// Returns raw <see cref="TwitchApiResponse{T}"/> of type <see cref="Follows"/> models containing followers and pagination info.
        /// </summary>
        /// <param name="userId">Id of the user to fetch followers from.</param>
        /// <param name="limit">Amount of records to fetch per API call.</param>
        /// <param name="cursor">Cursor to continue fetching records to continue fetching more followers.</param>
        internal static async Task<TwitchApiResponse<Follows>> GetUserFollowers(string userId, int limit = 20, string cursor = null)
        {
            var blob = await TwitchAPIRequests.GetRequest($"https://api.twitch.tv/helix/users/follows?to_id={userId}&first={limit}&after={cursor}");
            return JsonConvert.DeserializeObject<TwitchApiResponse<Follows>>(blob, jSettings);
        }

        /// <summary>
        /// Gets latest followers from given user id.
        /// Returns an enumerable of type <see cref="Follows"/> models of followers.
        /// </summary>
        /// <param name="userId">Id of the user to get the latest followers from.</param>
        /// <param name="limit">Amount of latest followers to fetch. Minimum 1, maximum 100.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="TwitchAPIException"/>
        internal static async Task<IEnumerable<Follows>> GetUserLatestFollowers(string userId, int limit = 20)
        {
            // If target id is not given, throw exception
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentNullException("targetId", "Given id of the target cannot be empty, whitespace, or null");
            }

            if (limit < 1 || limit > 100)
            {
                throw new ArgumentOutOfRangeException("limit", "Limit cannot be less than 1 or exceeding 100.");
            }

            try
            {
                return (await GetUserFollowers(userId, limit, null)).Data;
            }
            catch (WebException ex)
            {
                throw new TwitchAPIException(ex.Message, ((HttpWebResponse)ex.Response).StatusCode);
            }
        }

        /// <summary>
        /// Gets followings (user is following) from given user id. Unchecked arguments and any exception are passed onwards.
        /// Returns raw <see cref="TwitchApiResponse{T}"/> of type <see cref="Follows"/> models containing followings and pagination info.
        /// </summary>
        /// <param name="userId">Id of the user to fetch followings from.</param>
        /// <param name="limit">Amount of records to fetch per API call.</param>
        /// <param name="cursor">Cursor to continue fetching records to continue fetching more followings.</param>
        internal static async Task<TwitchApiResponse<Follows>> GetUserFollowings(string userId, int limit = 20, string cursor = null)
        {
            var blob = await TwitchAPIRequests.GetRequest($"https://api.twitch.tv/helix/users/follows?from_id={userId}&first={limit}&after={cursor}");
            return JsonConvert.DeserializeObject<TwitchApiResponse<Follows>>(blob, jSettings);
        }

        /// <summary>
        /// Gets latests followings (user is following) from given user id.
        /// Returns an enumerable of type <see cref="Follows"/> models of followings.
        /// </summary>
        /// <param name="userId">Id of the user to get the latest followings from.</param>
        /// <param name="limit">Amount of latest following to fetch. Minimum 1, maximum 100.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="TwitchAPIException"/>
        internal static async Task<IEnumerable<Follows>> GetUserLatestFollowings(string userId, int limit = 20)
        {
            // If target id is not given, throw exception
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentNullException("userId", "Given id of the user cannot be empty, whitespace, or null");
            }

            if (limit < 1 || limit > 100)
            {
                throw new ArgumentOutOfRangeException("limit", "Limit cannot be less than 1 or exceeding 100.");
            }

            try
            {
                return (await GetUserFollowings(userId, limit, null)).Data;
            }
            catch (WebException ex)
            {
                throw new TwitchAPIException(ex.Message, ((HttpWebResponse)ex.Response).StatusCode);
            }
        }

        #endregion

        /// <summary>
        /// [unsupported endpoint] Gets all current chatters connected to the specified Twitch IRC channel. 
        /// </summary>
        /// <param name="channelName">The name of the channel to fetch the chatters from.</param>
        /// <returns>List of Tuples containing chatter username and chatter type.</returns>
        public static async Task<List<ChatterAPI>> GetChannelChatters(string channelName)
        {
            // Initialize collection
            List<ChatterAPI> chatters = new List<ChatterAPI>();

            // Request
            try
            {
                // Create the request
                var blob = await TwitchAPIRequests.GetRequest($"http://tmi.twitch.tv/group/user/{channelName}/chatters");

                // Parse response to json object
                JObject parsed = JObject.Parse(blob);

                // Iterate over chatter types (casted as JObject for KeyValuePair)
                foreach (var chatterType in (JObject)parsed["chatters"])
                {
                    // Set user type (TwitchChatMessageEnums) from key name
                    UserType type;
                    switch (chatterType.Key)
                    {
                        case "moderators":
                            type = UserType.Moderator;
                            break;
                        case "staff":
                            type = UserType.Staff;
                            break;
                        case "admins":
                            type = UserType.Admin;
                            break;
                        case "global_mods":
                            type = UserType.GlobalMod;
                            break;
                        default:
                            type = UserType.Normal;
                            break;
                    }

                    // Iterate over chatters of said type (casted to JArray)
                    foreach (string chatter in (JArray)chatterType.Value)
                    {
                        // Add the chatter struct to the collection
                        chatters.Add(new ChatterAPI(chatter, type));
                    }
                }
            }
            catch (WebException ex)
            {
                throw new TwitchAPIException(ex.Message, ((HttpWebResponse)ex.Response).StatusCode);
            }

            // Return the collection
            return chatters;
        }
    }
}
