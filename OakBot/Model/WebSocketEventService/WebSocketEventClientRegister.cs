using System;
using System.Collections.Generic;
using System.Linq;

namespace OakBot.Model
{
    internal class WebSocketEventClientRegister
    {
        #region Fields

        private List<Tuple<string, string>> _register;
        private object _lock;

        #endregion

        #region Constructors

        public WebSocketEventClientRegister()
        {
            _register = new List<Tuple<string, string>>();
            _lock = new object();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Add client with a given event to the registery.
        /// </summary>
        /// <param name="clientId">Client session ID.</param>
        /// <param name="wsevent">Event name.</param>
        public void ClientSubscribeEvent(string clientId, string wsevent)
        {
            lock (_lock)
            {
                var tuple = Tuple.Create(wsevent, clientId);
                if (!_register.Contains(tuple))
                {
                    _register.Add(tuple);
                }
            }
        }

        /// <summary>
        /// Remove client with a given event from the registery.
        /// </summary>
        /// <param name="clientId">Client session ID.</param>
        /// <param name="wsevent">Event name.</param>
        public void ClientUnsubscribeEvent(string clientId, string wsevent)
        {
            lock (_lock)
            {
                _register.RemoveAll(x => x.Item1 == wsevent && x.Item2 == clientId);
            }
        }

        /// <summary>
        /// Remove specific client from the registery, unsubscribing to all events.
        /// </summary>
        /// <param name="clientId">Client session ID.</param>
        public void RemoveClient(string clientId)
        {
            lock (_lock)
            {
                _register.RemoveAll(tup => tup.Item2 == clientId);
            }
        }

        /// <summary>
        /// Get a IEnumerable containing each client that registered for given event.
        /// </summary>
        /// <param name="wsevent">Event name.</param>
        /// <returns>IEnumerable of client IDs registerd for given event.</returns>
        public IEnumerable<string> GetClientsSubscirbedForEvent(string wsevent)
        {
            lock (_lock)
            {
                return _register.Where(x => x.Item1 == wsevent).Select(y => y.Item2);
            }
        }

        /// <summary>
        /// Get a IEnumerable containing each event the given client registered for.
        /// </summary>
        /// <param name="clientId">Client Id.</param>
        /// <returns>IEnumerable of events the client Id is registered for.</returns>
        public IEnumerable<string> GetEventsSubscribedByClient(string clientId)
        {
            lock (_lock)
            {
                return _register.Where(x => x.Item2 == clientId).Select(y => y.Item1);
            }
        }

        /// <summary>
        /// Clears the event client registery.
        /// </summary>
        public void ClearRegister()
        {
            lock (_lock)
            {
                _register.Clear();
            }
        }

        #endregion
    }

}
