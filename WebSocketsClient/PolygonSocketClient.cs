﻿using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PolygonApiClient.WebSocketsClient
{
    public class PolygonSocketClient
    {
        #region Client Events

        public event EventHandler Opened;
        private void OnOpened()
        {
            Opened?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler Closed;
        private void OnClosed()
        {
            Closed?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler<ErrorReceivedEventArgs> ErrorReceived;
        private void OnError(string message)
        {
            ErrorReceived?.Invoke(this, new ErrorReceivedEventArgs(message));
        }

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;
        private void OnMessageReceived(string message)
        {
            MessageReceived?.Invoke(this, new MessageReceivedEventArgs(message));
        }

        #endregion

        #region Socket Frame Settings

        public double FrameSizeMbMax { get; } = 10;
        public double FrameSizeMbMin { get; } = .01;

        private const int BytesInMb = 1048576;

        private double _FrameSizeMb { get; set; } = .1;
        public double FrameSizeMb
        {
            get => _FrameSizeMb;
            set
            {
                _FrameSizeMb = Math.Min(FrameSizeMbMax, value);
                _FrameSizeMb = Math.Max(FrameSizeMbMin, value);
            }
        }

        private int _BufferFrameSize
        {
            get => (int)(FrameSizeMb * BytesInMb);
        }

        #endregion

        public string Name { get; private set; }

        public PolygonConnectionEndpoint Endpoint { get; }

        public bool Connected { get; private set; } = false;

        public const string BaseUri = "wss://socket.polygon.io/";

        private ClientWebSocket client { get; } = new ClientWebSocket();

        private Uri connectionPath { get; }

        private string APIKey { get; }

        public PolygonSocketClient(string apiKey, PolygonConnectionEndpoint endpoint)
        {
            Endpoint = endpoint;
            Name = endpoint.ToString();
            connectionPath = new Uri($"{BaseUri}{endpoint.ToString()}");
            APIKey = apiKey;
        }

        #region Connection Management

        /// <summary>
        /// Opens the WebSockets connection
        /// </summary>
        /// <returns></returns>
        public async Task OpenAsync()
        {
            try
            {
                await client.ConnectAsync(connectionPath, CancellationToken.None);
                listenForMessages();
            }
            catch (Exception ex)
            {
                OnError(ex.Message);
            }
        }

        /// <summary>
        /// Closes the WebSockets connection
        /// </summary>
        /// <returns></returns>
        public async Task CloseAsync()
        {
            try
            {
                await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "closed", CancellationToken.None);
                Connected = false;
                OnClosed();
            }
            catch (Exception ex)
            {
                OnError(ex.Message);
            }
        }

        #endregion

        #region Socket Message Processing

        /// <summary>
        /// Send a property formatted Polygon message string
        /// </summary>
        /// <param name="message"></param>
        private async Task sendAsync(string message)
        {
            var buffer = getSendBuffer(message);
            await client.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        /// <summary>
        /// Send the API key to Polygon to authenticate this session
        /// </summary>
        private async Task sendAuthorizationMessage()
        {
            await sendAsync($"{{\"action\":\"auth\",\"params\":\"{APIKey}\"}}");
        }

        /// <summary>
        /// Starts a background thread to listen for incoming socket messages
        /// </summary>
        private void listenForMessages()
        {
            try
            {
                new Thread(async () =>
                {
                    // Create a new buffer to receive messages
                    var buffer = getReceiveBuffer();

                    // Continue listening while connected
                    while (client.State == System.Net.WebSockets.WebSocketState.Open)
                    {
                        // Clear the contents of the buffer
                        clearReceiveBuffer(buffer);

                        // Wait for the remote host to send a message
                        var result = await client.ReceiveAsync(buffer, CancellationToken.None);

                        // Read the buffer to a string
                        string msg = System.Text.Encoding.Default.GetString(buffer.TakeWhile(x => x != '\0').ToArray());

                        // If the message is fragmented, continue reading
                        while (!result.EndOfMessage)
                        {
                            // Continue to receive chunked message
                            clearReceiveBuffer(buffer);
                            result = await client.ReceiveAsync(buffer, CancellationToken.None);

                            // Concatenate to message received so far
                            msg += System.Text.Encoding.Default.GetString(buffer.TakeWhile(x => x != '\0').ToArray());
                        }

                        // Handle initialization messages
                        if (msg.Contains("Connected Successfully"))
                        {
                            // Send the API key
                            await sendAuthorizationMessage();
                        }
                        else if (msg.Contains("authenticated"))
                        {
                            // Signal a completed initialization
                            Connected = true;

                            OnOpened();
                        }
                        else
                        {
                            // Send all normally formatted messages to be processed
                            processMessage(msg);
                        }
                    };
                })
                { IsBackground = true }.Start();
            }
            catch (Exception ex)
            {
                OnError(ex.Message);
                CloseAsync().Wait();
            }
        }

        /// <summary>
        /// Converts a string message to JSON objects and sends to appropriate handlers
        /// </summary>
        /// <param name="message"></param>
        /// <exception cref="Exception"></exception>
        private void processMessage(string message)
        {
            var objs = JsonConvert.DeserializeObject<JArray>(message);

            foreach (var obj in objs)
            {
                switch (obj.Value<string>("ev"))
                {
                    case "status":
                        statusMessageHandler(obj.ToObject<Socket_Message>());
                        break;
                    case "AM":
                        aggregateMinuteMessageHandler(obj.ToObject<Socket_Aggregate>());
                        break;
                    case "A":
                        aggregateSecondMessageHandler(obj.ToObject<Socket_Aggregate>());
                        break;
                    case "T":
                        tradeMessageHandler(obj.ToObject<Socket_Trade>());
                        break;
                    case "Q":
                        quoteMessageHandler(obj.ToObject<Socket_Quote>());
                        break;
                    default:
                        throw new Exception("Unknown Socket event type");
                }
            }
        }

        /// <summary>
        /// Creates a new receive buffer based on the set frame size
        /// </summary>
        /// <returns></returns>
        private ArraySegment<byte> getReceiveBuffer()
        {
            return new ArraySegment<byte>(new byte[_BufferFrameSize]);
        }

        /// <summary>
        /// Clears a receive buffer
        /// </summary>
        /// <param name="buffer"></param>
        private void clearReceiveBuffer(ArraySegment<byte> buffer)
        {
            Array.Clear(buffer.Array, 0, buffer.Count);
        }

        /// <summary>
        /// Creates a buffer sized for the provided message
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        private ArraySegment<byte> getSendBuffer(string msg)
        {
            return new ArraySegment<byte>(Encoding.Default.GetBytes(msg));
        }

        #endregion

        #region Socket Handlers

        private Dictionary<string, SocketHandler> socketHandlers = new Dictionary<string, SocketHandler>();

        public SocketHandler GetSocketHandler(string symbol)
        {
            if (socketHandlers.TryGetValue(symbol, out var ret))
                return ret;
            else
            {
                socketHandlers.Add(symbol, new SocketHandler(symbol));
                return GetSocketHandler(symbol);
            }
        }

        #endregion

        #region Data Request Methods

        public SocketHandler Aggregate_Second_Bars_Streaming(string symbol, bool subscribe = true)
        {
            // A.
            if (subscribe)
                return subscribeStreaming(symbol, "A.");
            else
            {
                unsubscribeStreaming(symbol, "A.");
                return null;
            }
        }

        public SocketHandler Aggregate_Minute_Bars_Streaming(string symbol, bool subscribe = true)
        {
            // AM.
            if (subscribe)
                return subscribeStreaming(symbol, "AM.");
            else
            {
                unsubscribeStreaming(symbol, "AM.");
                return null;
            }
        }

        public SocketHandler Trades_Streaming(string symbol, bool subscribe = true)
        {
            // T.
            if (subscribe)
                return subscribeStreaming(symbol, "T.");
            else
            {
                unsubscribeStreaming(symbol, "T.");
                return null;
            }
        }

        public SocketHandler Quotes_Streaming(string symbol, bool subscribe = true)
        {
            // Q.
            if (subscribe)
                return subscribeStreaming(symbol, "Q.");
            else
            {
                unsubscribeStreaming(symbol, "Q.");
                return null;
            }
        }

        private SocketHandler subscribeStreaming(string symbol, string prefix)
        {
            string reqStr = @"{""action"":""subscribe"", ""params"":""" + prefix + symbol.ToUpper() + @"""}";

            var ret = GetSocketHandler(symbol);

            sendAsync(reqStr).Wait();

            return ret;
        }

        private void unsubscribeStreaming(string symbol, string prefix)
        {
            string reqStr = @"{""action"":""unsubscribe"", ""params"":""" + prefix + symbol.ToUpper() + @"""}";

            sendAsync(reqStr).Wait();
        }

        #endregion

        #region Data Response Handlers

        private void statusMessageHandler(Socket_Message obj)
        {
            OnMessageReceived(obj.Message);
        }

        private void tradeMessageHandler(Socket_Trade obj)
        {
            GetSocketHandler(obj.Symbol).T(obj);
        }

        private void quoteMessageHandler(Socket_Quote obj)
        {

            GetSocketHandler(obj.Symbol).Q(obj);
        }

        private void aggregateSecondMessageHandler(Socket_Aggregate obj)
        {
            GetSocketHandler(obj.Symbol).A(obj);
        }

        private void aggregateMinuteMessageHandler(Socket_Aggregate obj)
        {
            GetSocketHandler(obj.Symbol).AM(obj);
        }

        #endregion  
    }
}