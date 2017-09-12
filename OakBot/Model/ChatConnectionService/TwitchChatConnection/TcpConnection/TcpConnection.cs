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
    /// TcpConnection only handles the TCP connection for sending and receiving raw IRC messages.
    /// <see cref="Connected"/> is fired on connection,
    /// <see cref="Disconnected"/> is fired on disconnect,
    /// <see cref="MessageReceived"/> is fired on IRC message received,
    /// <see cref="MessageTransmitted"/> is fired on transmission of an enqued message.
    /// </summary>
    internal class TcpConnection
    {
        #region Fields

        private static readonly string[] _messageSeparators = new string[] { "\r\n" };

        private TcpClient _client;
        private NetworkStream _networkStream;
        private SslStream _sslStream;
        
        private bool _secure;
        private int _rateLimit;

        private ConcurrentQueue<string> _sendQ;
        private ConcurrentQueue<string> _prioritySendQ;
        
        private System.Timers.Timer _pinger;

        #endregion

        #region Events

        internal event EventHandler<TcpConnectedEventArgs> Connected;
        internal event EventHandler<TcpDisconnectedEventArgs> Disconnected;
        internal event EventHandler<TcpMessageReceivedEventArgs> MessageReceived;
        internal event EventHandler<TcpMessageTransmittedEventArgs> MessageTransmitted;

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize TcpConnection, unconnected.
        /// </summary>
        /// <param name="rateLimit">Optional ratelimit of the transmit of normal enqueued message in milliseconds.</param>
        internal TcpConnection()
        {
            // Initialize
            _sendQ = new ConcurrentQueue<string>();
            _prioritySendQ = new ConcurrentQueue<string>();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Async connect and set the created network stream(s).
        /// </summary>
        /// <param name="address">The address of the server to connect to as string.</param>
        /// <param name="port">The port to use to connect to the server as int.</param>
        /// <param name="secure">Set true to use a secure (SSL) connection.</param>
        /// <param name="ratelimit">Ratelimit to delay to transmit normal priority messages.</param>
        /// <returns></returns>
        internal async Task<bool> ConnectAsync(string address, int port, bool secure, int ratelimit)
        {
            // Create a new TcpClient and set flags
            _client = new TcpClient();
            _secure = secure;
            _rateLimit = ratelimit;
            
            try
            {
                // Async connect to given address and port
                await _client.ConnectAsync(address, port);
            }
            catch (SocketException)
            {
                // On failure Disconnect to clear resources and return
                Disconnect(TcpDisconnectReason.ConnectionFailure);
                return false;
            }
            catch
            {
                Disconnect(TcpDisconnectReason.Unknown);
                return false;
            }

            // Verify if we are really connected
            if (!_client.Connected)
            {
                Disconnect(TcpDisconnectReason.ConnectionFailure);
                return false;
            }
            
            // Connection established, set endpoint and get the network stream(s)
            _networkStream = _client.GetStream();
            if (_secure)
            {
                _sslStream = new SslStream(_client.GetStream());
                _sslStream.AuthenticateAsClient(address);
            }
            
            // Initialize and start the Listener and Sender threads
            Thread tlisten = new Thread(() => Listener());
            Thread tsend = new Thread(() => Sender());
            tlisten.Start();
            tsend.Start();

            // Start the PING timer on a 60 sec interval
            _pinger = new System.Timers.Timer(60000);
            _pinger.Elapsed += _pinger_Elapsed;
            _pinger.Start();

            // Fire the Connected event
            OnConnection();

            return true;
        }

        /// <summary>
        /// Disconnects and disposes of the internal TcpClient and Pinger.
        /// </summary>
        /// <param name="reason">The reason for the disconnection.</param>
        internal void Disconnect(TcpDisconnectReason reason)
        {
            // Dispose resources
            _pinger.Stop();
            _pinger.Dispose();
            _client.Close();

            // Fire disconnection event
            OnDisconnection(reason);
        }

        /// <summary>
        /// Enqueue a message in either the priority-, or normal-queue to be transmitted.
        /// </summary>
        /// <param name="message">Raw message to be transmitted.</param>
        /// <param name="hasPriority">True if priority message, else normal priority.</param>
        internal void EnqueueMessage(string message, bool hasPriority = false)
        {
            (hasPriority ? _prioritySendQ : _sendQ).Enqueue(message);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Send PING to connected server every timer elapsed period.
        /// </summary>
        private void _pinger_Elapsed(object sender, ElapsedEventArgs e)
        {
            EnqueueMessage("PING :" + DateTime.Now.Ticks, true);
        }

        /// <summary>
        /// Async listener to await data from the TCP network stream.
        /// </summary>
        private async void Listener()
        {
            byte[] buffer = new byte[1024];
            StringBuilder msgBuilder = new StringBuilder();

            int badMessagesReceived = 0;

            Stream stream;
            if (_secure)
            {
                stream = _sslStream;
            }
            else
            {
                stream = _networkStream;
            }

            while (_client.Connected)
            {
                // Continue if there is data available
                if (_networkStream.DataAvailable)
                {
                    // Clear buffer and read new data from network stream
                    Array.Clear(buffer, 0, buffer.Length);
                    await stream.ReadAsync(buffer, 0, buffer.Length);

                    // Encode received data as UTF8 into the string builder
                    msgBuilder.Append(Encoding.UTF8.GetString(buffer).TrimEnd('\0'));
                    string msgString = msgBuilder.ToString();

                    // Partial (last) message in the builder, parse out any complete messages
                    if (!msgString.EndsWith("\r\n"))
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
                                Disconnect(TcpDisconnectReason.ErrorReceiveData);
                                break;
                            }
                        }

                        // Buffer contains actual data
                        else
                        {
                            // Check if buffer contains full chat messages
                            var idx = msgString.LastIndexOf("\r\n");
                            if (idx != -1)
                            {
                                idx += 2;
                                msgBuilder.Remove(0, idx);
                                msgString = msgString.Substring(0, idx);
                            }
                        }
                    }
                    // Complete message(s) in the builder
                    else
                    {
                        msgBuilder.Clear();
                    }

                    // Split msg string if multiple messages are read from buffer
                    string[] messages = msgString.Split(_messageSeparators, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string message in messages)
                    {
                        OnMessageReceived(message);
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
        /// Sender that sends enqued IRC messages to the TCP network stream.
        /// </summary>
        private void Sender()
        {
            while (_client.Connected)
            {
                // Transmit a priority message before any other normal messages in queue
                if (!_prioritySendQ.IsEmpty && _prioritySendQ.TryDequeue(out string primessage))
                {
                    TransmitMessage(primessage);
                    OnMessageTransmitted(primessage);
                    Thread.Sleep(1);
                }
                // If there are no priority message in queue send out any normal messages
                else if (!_sendQ.IsEmpty && _sendQ.TryDequeue(out string message))
                {
                    TransmitMessage(message);
                    OnMessageTransmitted(message);

                    // Message Throttle, ratelimit
                    Thread.Sleep(_rateLimit);
                }
                else
                {
                    Thread.Sleep(5);
                }  
            }

            // Clear queue buffers
            _prioritySendQ = new ConcurrentQueue<string>();
            _sendQ = new ConcurrentQueue<string>();
        }

        /// <summary>
        /// Transmit a message to a network stream
        /// </summary>
        /// <param name="message">Raw IRC message to be send, this already adds \r\n as message termination.</param>
        private void TransmitMessage(string message)
        {
            // Fill in the buffer with the message plus add <cr><nl> for proper IRC message
            byte[] buffer = Encoding.UTF8.GetBytes(message + "\r\n");

            try
            {
                if (_secure)
                {
                    _sslStream.Write(buffer, 0, buffer.Length);
                }
                else
                {
                    _networkStream.Write(buffer, 0, buffer.Length);
                }

            }
            catch (Exception ex) when (ex is IOException || ex is ObjectDisposedException)
            {
                Disconnect(TcpDisconnectReason.ErrorTransmitData);
            }
        }

        #endregion

        #region Protected Virtual

        protected virtual void OnConnection()
        {
            Connected?.Invoke(this, new TcpConnectedEventArgs());
        }

        protected virtual void OnDisconnection(TcpDisconnectReason reason)
        {
            Disconnected?.Invoke(this, new TcpDisconnectedEventArgs(reason));
        }

        protected virtual void OnMessageReceived(string message)
        {
            MessageReceived?.Invoke(this, new TcpMessageReceivedEventArgs(message));
        }

        protected virtual void OnMessageTransmitted(string message)
        {
            MessageTransmitted?.Invoke(this, new TcpMessageTransmittedEventArgs(message));
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
                if (_client != null)
                {
                    return _client.Connected;
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
