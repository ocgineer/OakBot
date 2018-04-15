using System;
using System.IO;
using System.Text;
using System.Timers;
using System.Net.Sockets;
using System.Net.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace OakBot.Model
{
    /// <summary>
    /// IrcTcpConnection only handles the TCP connection for sending and receiving raw IRC messages.
    /// <see cref="Connected"/> is fired on connection,
    /// <see cref="Disconnected"/> is fired on disconnect,
    /// <see cref="MessageReceived"/> is fired on each IRC message received,
    /// <see cref="MessageTransmitted"/> is fired on transmission of an enqued message.
    /// </summary>
    internal class IrcTcpConnection
    {
        #region Fields

        private static readonly string[] _messageSeparators = new string[] { "\r\n" };

        private TcpClient _tcpClient;
        private NetworkStream _networkStream;
        private SslStream _sslStream;
        
        private bool _secure;
        private int _rateLimit;

        private ConcurrentQueue<string> _sendQ;
        private ConcurrentQueue<string> _prioritySendQ;
        
        private System.Timers.Timer _pinger;
        private System.Timers.Timer _pongTimer;

        #endregion

        #region Events

        internal event EventHandler<IrcTcpConnectedEventArgs> Connected;
        internal event EventHandler<IrcTcpDisconnectedEventArgs> Disconnected;
        internal event EventHandler<IrcTcpMessageEventArgs> MessageReceived;
        internal event EventHandler<IrcTcpMessageEventArgs> MessageTransmitted;

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize TcpConnection, unconnected.
        /// </summary>
        /// <param name="rateLimit">Optional ratelimit of the transmit of normal enqueued message in milliseconds.</param>
        internal IrcTcpConnection()
        {
            // Initialize Message queues
            _sendQ = new ConcurrentQueue<string>();
            _prioritySendQ = new ConcurrentQueue<string>();

            // Initialize Pinger with 60s interval
            _pinger = new System.Timers.Timer(60000);
            _pinger.Elapsed += _pinger_Elapsed;

            // Initialize Pong timer with 10s interval
            _pongTimer = new System.Timers.Timer(10000)
            {
                AutoReset = false
            };
            _pongTimer.Elapsed += _pongTimer_Elapsed;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Connect to address with TCP as an asynchronous operation.
        /// </summary>
        /// <param name="address">The address of the server to connect to as string.</param>
        /// <param name="port">The port to use to connect to the server as int.</param>
        /// <param name="secure">Set true to use a secure (SSL) connection.</param>
        /// <param name="ratelimit">Ratelimit to delay to transmit normal priority messages.</param>
        /// <returns></returns>
        internal async Task ConnectAsync(string address, int port, bool secure, int ratelimit)
        {
            // Create a new TcpClient and set flags
            _tcpClient = new TcpClient();
            _secure = secure;
            _rateLimit = ratelimit;
            
            try
            {
                // Async connect to given address and port
                await _tcpClient.ConnectAsync(address, port);
            }
            catch (SocketException)
            {
                // Disconnect and return unsuccessful
                Disconnect(DisconnectReason.ConnectionFailure);
                return;
            }
            catch
            {
                // Disconnect and return unsuccessful
                Disconnect(DisconnectReason.Unknown);
                return;
            }

            // Verify if we are really connected
            if (!_tcpClient.Connected)
            {
                // Disconnect and return unsuccessful
                Disconnect(DisconnectReason.ConnectionFailure);
                return;
            }

            // Connection established, set endpoint and get the network stream(s)
            _networkStream = _tcpClient.GetStream();
            if (_secure)
            {
                _sslStream = new SslStream(_networkStream);
                _sslStream.AuthenticateAsClient(address);
            }
            
            // Initialize and start the Listener and Sender threads
            Thread tlisten = new Thread(() => ListenerAsync());
            Thread tsend = new Thread(() => SenderAsync());
            tlisten.Start();
            tsend.Start();

            // Start pinger
            _pinger.Start();

            // Fire the Connected event
            Connected?.Invoke(this, new IrcTcpConnectedEventArgs());
        }

        /// <summary>
        /// Disconnects TcpClient and stops the Pinger.
        /// </summary>
        /// <param name="reason">The reason for the disconnection.</param>
        internal void Disconnect(DisconnectReason reason)
        {
            // Stop pinger
            _pinger.Stop();

            // Dispose resources
            _tcpClient?.Dispose();
            _networkStream?.Dispose();
            _sslStream?.Dispose();

            // Clear transmit queues
            _prioritySendQ = new ConcurrentQueue<string>();
            _sendQ = new ConcurrentQueue<string>();

            // Fire disconnected event
            Disconnected?.Invoke(this, new IrcTcpDisconnectedEventArgs(reason));
        }

        /// <summary>
        /// Enqueue a message in either the priority-, or normal-queue to be transmitted.
        /// </summary>
        /// <param name="message">IRC message to be transmitted.</param>
        /// <param name="hasPriority">True if priority message, else normal (default) priority.</param>
        internal void EnqueueMessage(string message, bool hasPriority = false)
        {
            (hasPriority ? _prioritySendQ : _sendQ).Enqueue(message);
        }
        
        /// <summary>
        /// Notifies <see cref="IrcTcpConnection"/> that a PONG has been received to stop the
        /// internal pong timer so it won't disconnection on no PONG received.
        /// </summary>
        internal void PongReceived()
        {
            _pongTimer.Stop();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Send PING to connected server every timer elapsed period.
        /// </summary>
        private void _pinger_Elapsed(object sender, ElapsedEventArgs e)
        {
            EnqueueMessage("PING :" + DateTime.Now.Ticks, true);
            _pongTimer.Start();
        }

        /// <summary>
        /// If pong timer is not stopped, no pong has been received.
        /// </summary>
        private void _pongTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Disconnect(DisconnectReason.NoPongReceived);
        }

        /// <summary>
        /// Listener to read and extract IRC messages from network stream as an asynchronous operation.
        /// </summary>
        private async void ListenerAsync()
        {
            byte[] buffer = new byte[1024];
            StringBuilder builder = new StringBuilder();
            int badMessagesReceived = 0;

            // Set stream to read from determined by _secure
            Stream stream = _secure ? (Stream)_sslStream : _networkStream;

            while (_tcpClient.Connected)
            {
                // Continue if there is data available
                if (_networkStream.DataAvailable)
                {
                    try
                    {
                        // Clear buffer and read new data from network stream
                        Array.Clear(buffer, 0, buffer.Length);
                        await stream.ReadAsync(buffer, 0, buffer.Length);
                    }
                    catch
                    {
                        // Exception occurred while reading stream
                        Disconnect(DisconnectReason.ReceiveFailure);
                        break;
                    }

                    // Append newly received data as UTF8 to the message builder
                    builder.Append(Encoding.UTF8.GetString(buffer).TrimEnd('\0'));

                    // Get the whole string within the builder
                    string rawMessages = builder.ToString();

                    // Builder contained only full IRC message(s), clear builder
                    if (rawMessages.EndsWith("\r\n"))
                    {
                        builder.Clear();
                    }

                    // Builder can contain a partial message(s), check and parse out any complete messages
                    else
                    {
                        // Check if buffer contains only zeroes
                        bool allZeroes = true;
                        foreach (var b in buffer)
                        {
                            if (b != 0)
                            {
                                allZeroes = false;
                                break;
                            }
                        }

                        // Buffer only contains zeroes
                        if (allZeroes)
                        {
                            if (++badMessagesReceived > 2)
                            {
                                Disconnect(DisconnectReason.ReceiveFailure);
                                break;
                            }
                        }

                        // Buffer still contains actual data
                        else
                        {
                            // Check if buffer contains full chat messages
                            var idx = rawMessages.LastIndexOf("\r\n");
                            if (idx != -1)
                            {
                                idx += 2;
                                builder.Remove(0, idx);
                                rawMessages = rawMessages.Substring(0, idx);
                            }
                        }
                    }

                    // Split rawMessages, chances are there are multiple messages read from one read operation
                    foreach (string message in rawMessages.Split(_messageSeparators, StringSplitOptions.RemoveEmptyEntries))
                    {
                        // Fire message received event containing the raw singular IRC message
                        MessageReceived?.Invoke(this, new IrcTcpMessageEventArgs(message));
                    }
                }
                // No data available yet
                else
                {
                    Thread.Sleep(1);
                }
            }
        }

        /// <summary>
        /// Sender to send enqued IRC messages to network stream as an asynchronous operation.
        /// </summary>
        private async void SenderAsync()
        {
            while (_tcpClient.Connected)
            {
                // Transmit a priority message before any other normal messages in queue
                if (!_prioritySendQ.IsEmpty && _prioritySendQ.TryDequeue(out string primessage))
                {
                    // transmit message and send 
                    if (await TransmitMessageAsync(primessage))
                    {
                        // Fire transmitted event and apply a small sleep
                        MessageTransmitted?.Invoke(this, new IrcTcpMessageEventArgs(primessage));
                        Thread.Sleep(1);
                    }
                    else
                    {
                        // Disconnect on transmit failure
                        Disconnect(DisconnectReason.TransmitFailure);
                        break;
                    } 
                }
                // If there are no priority message in queue send out any normal messages
                else if (!_sendQ.IsEmpty && _sendQ.TryDequeue(out string message))
                {
                    if (await TransmitMessageAsync(message))
                    {
                        // Fire transmitted event and apply ratelimit
                        MessageTransmitted?.Invoke(this, new IrcTcpMessageEventArgs(message));
                        Thread.Sleep(_rateLimit);
                    }
                    else
                    {
                        // Disconnect on transmit failure
                        Disconnect(DisconnectReason.TransmitFailure);
                        break;
                    }
                }
                // Nothing to transmit
                else
                {
                    // Apply a small sleep

                    Thread.Sleep(1);
                }  
            }
        }

        /// <summary>
        /// Encodes and transmit an IRC message to network stream as an asynchronous operation.
        /// </summary>
        /// <param name="message">Message to be send. Internally adds \r\n as message termination.</param>
        /// <returns>True on success, false otherwise.</returns>
        private async Task<bool> TransmitMessageAsync(string message)
        {
            // Create byte buffer from the message with added CR&NL as per IRC protocol
            byte[] buffer = Encoding.UTF8.GetBytes(message + "\r\n");

            // Try to transmit the buffer, return true on success false on error
            try
            {
                if (_secure)
                {
                    await _sslStream.WriteAsync(buffer, 0, buffer.Length);
                    return true;
                }
                else
                {
                    await _networkStream.WriteAsync(buffer, 0, buffer.Length);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Get or Set the rate limit to send out normal enqueued IRC messages.
        /// </summary>
        internal int RateLimit
        {
            get
            {
                return _rateLimit;
            }
            set
            {
                if (value != _rateLimit)
                {
                    _rateLimit = value;
                }    
            }
        }

        /// <summary>
        /// Get if the internal IRC TcpClient is connected.
        /// </summary>
        internal bool IsConnected
        {
            get
            {
                if (_tcpClient != null)
                {
                    return _tcpClient.Connected;
                }
                else
                {
                    return false;
                }
            }
        }

        #endregion
    }
}
