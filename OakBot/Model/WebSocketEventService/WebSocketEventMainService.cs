using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace OakBot.Model
{
    internal class WebSocketMainService : WebSocketBehavior
    {
        #region Fields

        // Referenced from outside behavior
        private WebSocketEventClientRegister _register;
        private string _apikey;

        #endregion

        #region Constructor

        /// <summary>
        /// WebSocket Server main service.
        /// </summary>
        /// <param name="apikey">API key to use as authentication.</param>
        /// <param name="register">Reference to client event registery.</param>
        public WebSocketMainService(string apikey, WebSocketEventClientRegister register)
        {
            // Set reference from outside behavior
            _apikey = apikey;
            _register = register;
        }

        #endregion

        #region WebSocketBehavior Overrides

        protected override void OnOpen()
        {
            if (this.Context.QueryString["token"] == _apikey)
            {
                // Given token during handshake matches > CONNECTED event             
                this.SendAsync(JsonConvert.SerializeObject(
                    new WebSocketEventPayload("AUTHENTICATED", null)), null);
            }
            else
            {
                // Token does not match or not given, close session with unauth message
                this.Sessions.CloseSession(this.ID, CloseStatusCode.InvalidData,
                    "Unauthenticated; No or invalid API token given.");
            }
        }

        protected override void OnClose(CloseEventArgs e)
        {
            // On close remove the disconnected client from client event registery
            _register.RemoveClient(this.ID);
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            // return if data is not text
            if (!e.IsText)
            {
                return;
            }

            try
            {
                JToken blobObj = JObject.Parse(e.Data);

                // Client wants to subscribe to events
                if (blobObj["subscribe"] != null)
                {
                    // register each given event
                    foreach (string item in blobObj["subscribe"])
                    {
                        _register.ClientSubscribeEvent(this.ID, item);
                    }
                }

                // Client wants to unsubscribe from events
                if (blobObj["unsubscribe"] != null)
                {
                    // unregister each given event
                    foreach (string item in blobObj["unsubscribe"])
                    {
                        _register.ClientUnsubscribeEvent(this.ID, item);
                    }
                }

                // If subscribed or unsubscribed happend, transmit all current subscribed events
                if (blobObj["unsubscribe"] != null || blobObj["subscribe"] != null)
                {
                    // send list of registered events for by id
                    this.SendAsync(JsonConvert.SerializeObject(
                        new WebSocketEventPayload("SUBSCRIBED", _register.GetEventsSubscribedByClient(this.ID))), null);
                }
            }
            catch
            {
                // unable to parse received data
                return;
            }
        }

        #endregion
    }
}
