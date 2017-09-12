using System;
using System.Timers;

using Newtonsoft.Json;
using WebSocketSharp;

namespace OakBot.Model
{
    public class TwitchPubSubService : ITwitchPubSubService
    {
        #region Fields

        private WebSocket _wsclient;
        private Timer _pinger;
        private Timer _pongtimer;

        private TwitchCredentials _credentials;
        private string _channelID;

        #endregion

        #region Constructors

        public TwitchPubSubService()
        {
            // Register to the shutdown notification to properlly close connection
            GalaSoft.MvvmLight.Messaging.Messenger.Default.Register<GalaSoft.MvvmLight.Messaging.NotificationMessage>
                (this, "shutdown", (msg) => { Close(); });

            // Initialize WebSocket client connection
            _wsclient = new WebSocket("wss://pubsub-edge.twitch.tv");
            _wsclient.OnOpen += _wsclient_OnOpen;
            _wsclient.OnClose += _wsclient_OnClose;
            _wsclient.OnError += _wsclient_OnError;
            _wsclient.OnMessage += _wsclient_OnMessage;

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
        /// <param name="caster">Caster credentials, to requests private PubSub events.</param>
        public void Connect(TwitchCredentials caster)
        {
            if (!caster.IsCaster)
                throw new ArgumentException("Given credentials are not from the caster account.");

            _credentials = caster;

            // TODO: Get channel ID from given caster ID
            _channelID = "";

            // Connect
            this.Connect();
        }

        /// <summary>
        /// Closes the WebSocket client connected to Twitch PubSub.
        /// </summary>
        public void Close()
        {
            _pinger.Stop();
            _pongtimer.Stop();
            _wsclient.Close(CloseStatusCode.Away);
        }

        /// <summary>
        /// 
        /// </summary>
        private void Connect()
        {
            _wsclient.ConnectAsync();
        }

        private void Reconnect()
        {
            this.Close();
            this.Connect();
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

                // TODO: remove console debugging output
                Console.WriteLine("PUBSUB PING TRANSMITTED");
            }
        }

        /// <summary>
        /// Pong timer. If no PONG received after sending PING, silently reconnect.
        /// </summary>
        private void _pongtimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // Reconnect silently
            this.Close();
            this.Connect();
        }

        private void _wsclient_OnOpen(object sender, EventArgs e)
        {
            // Start pinger
            _pinger.Start();
            
            // Create PubSub subscription payload
            var payload = new TwitchPubSubPayload
            {
                Type = "LISTEN",
                Nonce = DateTime.UtcNow.Ticks.ToString(),
                Data = new TwitchPubSubPayloadData
                {
                    Topics = new string[] {
                        $"channel-subscribe-events-v1.{_channelID}",
                        $"channel-bits-events-v1.{_channelID}",
                        $"channel-commerce-events-v1.{_channelID}",
                        $"whispers.{_channelID}"
                    },
                    AuthToken = _credentials.OAuth
                }
            };

            // Async send json converted payload to server
            _wsclient.SendAsync(JsonConvert.SerializeObject(payload), null);

            // TODO: remove console debugging output
            Console.WriteLine("PUBSUB OPEN");
        }

        private void _wsclient_OnClose(object sender, CloseEventArgs e)
        {
            // Stop Pinger and PONG timer
            _pinger.Stop();
            _pongtimer.Stop();

            // TODO: remove console debugging output
            Console.WriteLine("PUBSUB CLOSED");
        }

        private void _wsclient_OnError(object sender, ErrorEventArgs e)
        {
            // TODO: remove console debugging output
            Console.WriteLine("PUBSUB ERROR");
        }

        private void _wsclient_OnMessage(object sender, MessageEventArgs e)
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

            // PING from server
            if (received.Type == "PING")
            {
                var payload = received;
                payload.Type = "PONG";
                _wsclient.SendAsync(JsonConvert.SerializeObject(payload), null);

                // TODO: remove console debugging output
                Console.WriteLine("PUBSUB PING RECEIVED");
            }

            else if (received.Type == "PONG")
            {
                // Stop PONG timer
                _pongtimer.Stop();
                
                // TODO: remove console debugging output
                Console.WriteLine("PUBSUB PONG RECEIVED");
            }
            
            // PubSub listen response
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
                        // TODO: remove console debugging output
                        Console.WriteLine("PUBSUB SUBSCRIPTION OK!");
                        break;
                }
            }

            // PubSub message on event registered for
            else if (received.Type == "MESSAGE")
            {
                Console.WriteLine(received.Data.Message);
            }   
        }

        #endregion
    }
}
