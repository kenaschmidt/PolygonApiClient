using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PolygonApiClient.WebSocketsClient
{
    public class PolygonClient
    {
        public double FrameSizeMbMax { get; } = 2;
        public double FrameSizeMbMin { get; } = .01;

        private const int BytesInMb = 1024 * 1024;

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

        public event EventHandler Error;
        private void OnError()
        {
            Error?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;
        private void OnMessageReceived(string message)
        {
            MessageReceived?.Invoke(this, new MessageReceivedEventArgs(message));
        }

        private System.Net.WebSockets.ClientWebSocket client { get; } = new ClientWebSocket();
        private Uri connectionPath { get; set; }

        public PolygonSocketClient(string connectionPathString)
        {
            connectionPath = new Uri(connectionPathString);
        }

        public async Task OpenAsync()
        {
            try
            {
                await client.ConnectAsync(connectionPath, CancellationToken.None);
                listenForMessages();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public void Send(string message)
        {
            var buffer = getSendBuffer(message);
            client.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None).Wait();
        }

        private void listenForMessages()
        {
            new Thread(() =>
            {
                var buffer = getReceiveBuffer();

                while (client.State == System.Net.WebSockets.WebSocketState.Open)
                {
                    clearReceiveBuffer(buffer);
                    var result = client.ReceiveAsync(buffer, CancellationToken.None).Result;

                    // Get returned message
                    string msg = System.Text.Encoding.Default.GetString(buffer.TakeWhile(x => x != '\0').ToArray());

                    while (!result.EndOfMessage)
                    {
                        // Continue to receive chunked message
                        clearReceiveBuffer(buffer);
                        result = client.ReceiveAsync(buffer, CancellationToken.None).Result;

                        // Concatenate to message received so far
                        msg += System.Text.Encoding.Default.GetString(buffer.TakeWhile(x => x != '\0').ToArray());
                    }

                    if (msg.Contains("Connected Successfully"))
                    {
                        OnOpened();
                    }
                    else
                    {
                        OnMessageReceived(msg);
                    }
                };
            })
            { IsBackground = true }.Start();
        }

        private ArraySegment<byte> getReceiveBuffer()
        {
            return new ArraySegment<byte>(new byte[_BufferFrameSize]);
        }
        private void clearReceiveBuffer(ArraySegment<byte> buffer)
        {
            Array.Clear(buffer.Array, 0, buffer.Count);
        }
        private ArraySegment<byte> getSendBuffer(string msg)
        {
            return new ArraySegment<byte>(Encoding.Default.GetBytes(msg));
        }

    }
}
