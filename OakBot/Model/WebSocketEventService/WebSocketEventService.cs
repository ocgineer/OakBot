using System;
using System.Linq;

using GalaSoft.MvvmLight.Messaging;
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
        /// Does not set the main service <see cref="ChangeApiToken(string)"/> does this.
        /// </summary>
        public WebSocketEventService()
        {
            // Initialize
            _register = new WebSocketEventClientRegister();
        }

        #endregion

        #region Interface Methods

        /// <summary>
        /// Start the websocket server with given api token for the main service.
        /// </summary>
        /// <param name="port"></param>
        /// <param name="token"></param>
        public bool StartService(int port, string token)
        {
            if (_server == null)
            {
                _token = token;
                _server = new WebSocketServer(System.Net.IPAddress.Any, port);
                _server.AddWebSocketService("/", () => new WebSocketMainService(token, _register));

                // Try to start WebSocket server without any service attached
                try
                {
                    // Start WebSocket server
                    _server.Start();

                    // Broadcast System Message on success
                    Messenger.Default.Send<bool>(true, "WebSocketEventServiceStatusChanged");

                    // Return successful
                    return true;
                }
                catch
                {
                    // Dispose
                    _server = null;
                    
                    // Broadcast System Message on failure
                    Messenger.Default.Send<bool>(false, "WebSocketEventServiceStatusChanged");

                    // Return failure
                    return false;
                }
            }
            else
            {
                // Server already running, return successful
                return true;
            } 
        }
        
        /// <summary>
        /// Changes the API key required to connect to the websocket main service.
        /// If the main service is already running, will stop and disconnect all clients first.
        /// </summary>
        /// <param name="token">Token to be used to authenticate as client.</param>
        public void ChangeApiToken(string token)
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
            var clients = _register.GetClientsSubscirbedForEvent(wsevent);

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
