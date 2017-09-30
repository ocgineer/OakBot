using System;
using System.Timers;
using System.Threading.Tasks;

using GalaSoft.MvvmLight.Messaging;
using Newtonsoft.Json;
using WebSocketSharp;

namespace OakBot.Model
{
    /*
        PUBSUB MESSAGE: {"type":"whisper_received","data":"{\"message_id\":\"b565ed03-ac08-4b00-8d65-12bce898ae4a\",\"id\":740,\"thread_id\":\"50553721_86308687\",\"body\":\"test\",\"sent_ts\":1505530256,\"from_id\":86308687,\"tags\":{\"login\":\"oakminati\",\"display_name\":\"Oakminati\",\"color\":\"#FF0000\",\"user_type\":\"\",\"turbo\":false,\"emotes\":[],\"badges\":[]},\"recipient\":{\"id\":50553721,\"username\":\"ocgineer\",\"display_name\":\"Ocgineer\",\"color\":\"#A54467\",\"user_type\":\"\",\"turbo\":true,\"badges\":[{\"id\":\"partner\",\"version\":\"1\"}],\"profile_image\":null}}","data_object":{"message_id":"b565ed03-ac08-4b00-8d65-12bce898ae4a","id":740,"thread_id":"50553721_86308687","body":"test","sent_ts":1505530256,"from_id":86308687,"tags":{"login":"oakminati","display_name":"Oakminati","color":"#FF0000","user_type":"","turbo":false,"emotes":[],"badges":[]},"recipient":{"id":50553721,"username":"ocgineer","display_name":"Ocgineer","color":"#A54467","user_type":"","turbo":true,"badges":[{"id":"partner","version":"1"}],"profile_image":null}}}
        PUBSUB MESSAGE: {"type":"whisper_received","data":"{\"message_id\":\"ec81a48a-179e-4267-bc4f-096fb0275942\",\"id\":741,\"thread_id\":\"50553721_86308687\",\"body\":\"this seem to work PogChamp\",\"sent_ts\":1505530339,\"from_id\":86308687,\"tags\":{\"login\":\"oakminati\",\"display_name\":\"Oakminati\",\"color\":\"#FF0000\",\"user_type\":\"\",\"turbo\":false,\"emotes\":[{\"id\":88,\"start\":18,\"end\":25}],\"badges\":[]},\"recipient\":{\"id\":50553721,\"username\":\"ocgineer\",\"display_name\":\"Ocgineer\",\"color\":\"#A54467\",\"user_type\":\"\",\"turbo\":true,\"badges\":[{\"id\":\"partner\",\"version\":\"1\"}],\"profile_image\":null}}","data_object":{"message_id":"ec81a48a-179e-4267-bc4f-096fb0275942","id":741,"thread_id":"50553721_86308687","body":"this seem to work PogChamp","sent_ts":1505530339,"from_id":86308687,"tags":{"login":"oakminati","display_name":"Oakminati","color":"#FF0000","user_type":"","turbo":false,"emotes":[{"id":88,"start":18,"end":25}],"badges":[]},"recipient":{"id":50553721,"username":"ocgineer","display_name":"Ocgineer","color":"#A54467","user_type":"","turbo":true,"badges":[{"id":"partner","version":"1"}],"profile_image":null}}}
        */

    public class TwitchPubSubService : ITwitchPubSubService
    {
        #region Fields

        private WebSocket _wsclient;
        private Timer _pinger;
        private Timer _pongtimer;
        private int _reconnectionAttempt;

        private TwitchCredentials _casterCredentials;
        private string _userId;

        #endregion

        #region Events

        public event EventHandler Connected;
        public event EventHandler Disconnected;
        public event EventHandler Subscribed;
        public event EventHandler<EventArgs> Message;

        #endregion

        #region Constructors

        public TwitchPubSubService()
        {
            // Initialize WebSocket client connection
            _wsclient = new WebSocket("wss://pubsub-edge.twitch.tv");
            _wsclient.OnOpen += _wsclient_OnOpen;
            _wsclient.OnClose += _wsclient_OnClose;
            _wsclient.OnError += _wsclient_OnError;
            _wsclient.OnMessage += _wsclient_OnMessage;

            // Set reconnection attempts to zero
            _reconnectionAttempt = 0;

            // Initialize Pinger and pong timer
            _pinger = new Timer(120000);
            _pinger.Elapsed += _pinger_Elapsed;
            _pongtimer = new Timer(10000)
            {
                AutoReset = false
            };
            _pongtimer.Elapsed += _pongtimer_Elapsed;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Attemt connection to PubSub server, after fetching channelIDs from Twitch API.
        /// </summary>
        /// <param name="credentials">Caster credentials, to requests private PubSub events.</param>
        public async Task<bool> Connect(TwitchCredentials credentials)
        {
            if (credentials.IsCaster)
            {
                // Set caster credentials
                _casterCredentials = credentials;

                try
                {
                    v5User authUser = await TwitchAPIv5.GetUser(credentials.OAuth);
                    if (authUser != null)
                    {
                        _userId = authUser.Id;
                        _wsclient.ConnectAsync();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Closes the WebSocket client connected to Twitch PubSub.
        /// </summary>
        public void Close()
        {
            _pinger.Stop();
            _pongtimer.Stop();
            _wsclient.CloseAsync(CloseStatusCode.Away);
        }

        /// <summary>
        /// Reconnects to PubSub with prior given credentials.
        /// Closes PubSub connection first if still active.
        /// </summary>
        public async Task Reconnect()
        {
            // Close websocket
            Close();

            // Exponential reconnection backoff delay
            if (_reconnectionAttempt++ > 10)
            {
                await Task.Delay(TimeSpan.FromSeconds(120));
            }
            else
            {
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(_reconnectionAttempt, 2)));
            }

            // Attempt new websocket connection
            _wsclient.ConnectAsync();
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Pinger. Sends a PING type message to PubSub on every elapsed event.
        /// </summary>
        private void _pinger_Elapsed(object sender, ElapsedEventArgs e)
        {
            // Send PING when connection is alive
            if (_wsclient.IsAlive)
            {
                // Create PING payload
                var payload = new TwitchPubSubPayload
                {
                    Type = "PING"
                };

                // Async send json converted payload to server
                _wsclient.Send(JsonConvert.SerializeObject(payload));

                // Start pong timer
                _pongtimer.Start();
            }
        }

        /// <summary>
        /// Pong timer. If no PONG received after sending PING, silently reconnect.
        /// </summary>
        private async void _pongtimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            await Reconnect();
        }

        /// <summary>
        /// WebSocket OnOpen event handler; Send listen topics and fire Connected event.
        /// </summary>
        private void _wsclient_OnOpen(object sender, EventArgs e)
        {
            // Start pinger
            _pinger.Start();

            // Reset reconnect attempts
            _reconnectionAttempt = 0;

            // Fire connected event and broadcast message
            Connected?.Invoke(this, null);
            
            // Create PubSub subscription payload
            var payload = new TwitchPubSubPayload
            {
                Type = "LISTEN",
                Nonce = DateTime.UtcNow.Ticks.ToString(),
                Data = new TwitchPubSubPayloadData
                {
                    Topics = new string[] {
                        $"channel-subscribe-events-v1.{_userId}",
                        $"channel-bits-events-v1.{_userId}",
                        $"channel-commerce-events-v1.{_userId}",
                        $"whispers.{_userId}"
                    },
                    AuthToken = _casterCredentials.OAuth
                }
            };

            // Async send json converted payload to server
            _wsclient.SendAsync(JsonConvert.SerializeObject(payload), null);
        }

        /// <summary>
        /// WebSocket OnClose event handler; Stop timers and fire Disconnected event.
        /// </summary>
        private void _wsclient_OnClose(object sender, CloseEventArgs e)
        {
            // Stop Pinger and PONG timer
            _pinger.Stop();
            _pongtimer.Stop();

            // Fire Disconnected event and broadcast message
            Disconnected?.Invoke(this, null);
            Messenger.Default.Send<bool>(false, "PubSubConnectionChanged");
        }

        /// <summary>
        /// WebSocket OnError event handler; ... TODO ...
        /// </summary>
        private void _wsclient_OnError(object sender, ErrorEventArgs e)
        {
            #if DEBUG
            Console.WriteLine("PUBSUB ERROR: " + e.Message);
            #endif
        }

        private async void _wsclient_OnMessage(object sender, MessageEventArgs e)
        {
            // Ignore if its not text
            if (!e.IsText)
                return;

            // Try to parse else ignore message
            TwitchPubSubPayload received;
            try
            {
                received = JsonConvert.DeserializeObject<TwitchPubSubPayload>(e.Data);
            }
            catch
            {
                // TODO: remove console debugging output
                Console.WriteLine("PUBSUB Deserialize Failure...");
                return;
            }

            // PONG received
            if (received.Type == "PONG")
            {
                // Stop PONG timer
                _pongtimer.Stop();

                // TODO: remove console debugging output
                #if DEBUG
                Console.WriteLine("PUBSUB PONG RECEIVED");
                #endif
            }
            
            // RECONNECT received
            else if (received.Type == "RECONNECT")
            {
                // Silently reconnect
                await Reconnect();
            }

            // RESONSE received
            else if (received.Type == "RESPONSE")
            {
                switch (received.Type)
                {
                    // Server error
                    case "ERR_SERVER":

                        break;

                    // Bad message error
                    case "ERR_BADMESSAGE":

                        break;

                    // Bad oauth error
                    case "ERR_BADAUTH":

                        break;

                    // Bad topic error
                    case "ERR_BADTOPIC":

                        break;

                    // All OK
                    default:
                        Messenger.Default.Send<bool>(true, "PubSubConnectionChanged");
                        Subscribed?.Invoke(this, null);
                        break;
                }
            }

            // PubSub message on event registered for
            else if (received.Type == "MESSAGE")
            {
                Message?.Invoke(this, new EventArgs());
                Console.WriteLine("PUBSUB MESSAGE: " + received.Data.Message);
            }   
        }

        #endregion
    }
}
