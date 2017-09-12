using Newtonsoft.Json;

namespace OakBot.Model
{
    public class TwitchPubSubPayload
    {
        /// <summary>
        /// Type of the payload. Valid values are: LISTEN, UNLISTEN.
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// (Optional) Random string to identify the response associated with this request.
        /// </summary>
        [JsonProperty("nonce")]
        public string Nonce { get; set; }

        /// <summary>
        /// Wraps the topics and auth_token fields.
        /// </summary>
        [JsonProperty("data")]
        public TwitchPubSubPayloadData Data { get; set; }

        /// <summary>
        /// The error message associated with the request, or an empty string if there is no error.
        /// For bits and whispers events requests, error responses can be: ERR_BADMESSAGE, ERR_BADAUTH, ERR_SERVER, ERR_BADTOPIC
        /// </summary>
        [JsonProperty("error")]
        public string Error { get; private set; }
    }

    public class TwitchPubSubPayloadData
    {
        #region Request Data

        /// <summary>
        /// List of topics to listen on.
        /// </summary>
        [JsonProperty("topics")]
        public string[] Topics { get; set; }

        /// <summary>
        /// OAuth token required to listen on some topics.
        /// </summary>
        [JsonProperty("auth_token")]
        public string AuthToken { get; set; }

        #endregion

        #region Received Message Data

        /// <summary>
        /// The topic that the message pertains to.
        /// </summary>
        [JsonProperty("topic")]
        public string Topic { get; private set; }

        /// <summary>
        /// The body of the received message.
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; private set; }

        #endregion
    }
}
