using System;
using System.Linq;

using WebSocketSharp.Server;
using Newtonsoft.Json;

namespace OakBot.Model
{
    public class WebSocketEventService : IWebSocketEventService
    {
        #region Fields

        private WebSocketServer _server;
        private WebSocketEventClientRegister _register;
        private string _token;

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize and start the websocket server.
        /// Does not set the main service <see cref="SetApiToken(string)"/> does this.
        /// </summary>
        public WebSocketEventService()
        {
            // Initialize
            _register = new WebSocketEventClientRegister();
            _server = new WebSocketServer(System.Net.IPAddress.Any, 1337);

            // Start ws server without service the service
            try
            {
                _server.Start();
            }
            catch
            {
                // Another websocket is already using 1337
            }
            
        }

        #endregion

        #region Interface Methods

        /// <summary>
        /// Sets the API key and add the main service.
        /// If main service is already running it removes the service first.
        /// </summary>
        /// <param name="token">Token to be used to authenticate as client.</param>
        public void SetApiToken(string token)
        {
            // Remove main service if running
            if (_server.RemoveWebSocketService("/"))
            {
                // Clear client event register
                _register.ClearRegister();
            }
            
            // Set new key
            _token = token;

            // Add service with new API key
            _server.AddWebSocketService("/", () => new WebSocketMainService(_token, _register));
        }

        /// <summary>
        /// Broadcast an event with payload as json string to all connected clients on the main service.
        /// </summary>
        /// <param name="wsevent">Event name.</param>
        /// <param name="data">
        /// Event data, if string it will try to deserialize to object else raw string is send.
        /// Anything else than a string needs to be serilizable as json string else null is transmitted as data.
        /// </param>
        public void BroadcastEvent(string eventName, object data)
        {
            // Broadcast payload as json string
            _server.WebSocketServices["/"]?.Sessions.BroadcastAsync(
                JsonConvert.SerializeObject(new WebSocketEventPayload(eventName, data)), null);
        }

        /// <summary>
        /// Send an event with playload as json string to connected clients on the main service that registered for the event.
        /// </summary>
        /// <param name="wsevent">Event name.</param>
        /// <param name="data">
        /// Event data, if string it will try to deserialize to object else raw string is send.
        /// Anything else than a string needs to be serilizable as json string else null is transmitted as data.
        /// </param>
        public void SendRegisteredEvent(string wsevent, object data)
        {
            // Get clients registered for given event
            var clients = _register.GetClientsSubscirbedEvent(wsevent);

            // Continue if any registered clients
            if (clients.Count() > 0)
            {
                // Serilize event payload to json string
                string jsonstring = JsonConvert.SerializeObject(new WebSocketEventPayload(wsevent, data));

                // Send serilized payload to each registered client
                foreach (string client in clients)
                {
                    _server.WebSocketServices["/"]?.Sessions.SendToAsync(jsonstring, client, null);
                }
            }
        }

        #endregion
    }
}
