using System;
using System.Linq;

using Newtonsoft.Json;
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
            // Client wants to subscribe to event(s)
            if (e.Data.StartsWith("subscribe", StringComparison.CurrentCultureIgnoreCase))
            {
                // Split remove empty, Get Distinct values, trim value and skip first element
                var wsevents = e.Data.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries)
                    .Distinct(StringComparer.CurrentCultureIgnoreCase).Select(x => x.Trim()).Skip(1);

                // Continue if events are given
                if (wsevents.Count() > 0)
                {
                    // register each event in the registery
                    foreach (string wsevent in wsevents)
                    {
                        _register.ClientSubscribeEvent(this.ID, wsevent);
                    }

                    // send a SUBSCRIBED event back with subbed events
                    this.SendAsync(JsonConvert.SerializeObject(
                        new WebSocketEventPayload("SUBSCRIBED", new { subscribed_events = wsevents })), null);
                }
            }

            // Client wants to unsubscrive to event(s)
            else if (e.Data.StartsWith("unsubscribe", StringComparison.CurrentCultureIgnoreCase))
            {
                // Split remove empty, Get Distinct values, trim value and skip first element
                var wsevents = e.Data.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries)
                    .Distinct(StringComparer.CurrentCultureIgnoreCase).Select(x => x.Trim()).Skip(1);

                // Continue if events are given
                if (wsevents.Count() > 0)
                {
                    // register each event in the registery
                    foreach (string wsevent in wsevents)
                    {
                        _register.ClientUnsubscribeEvent(this.ID, wsevent);
                    }

                    // send a UNSUBSCRIBED event back with unsubbed events
                    this.SendAsync(JsonConvert.SerializeObject(
                        new WebSocketEventPayload("UNSUBSCRIBED", new { unsubscribed_events = wsevents })), null);
                }
            }
        }

        #endregion
    }
}
