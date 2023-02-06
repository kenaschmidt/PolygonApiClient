using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolygonApiClient.WebSocketsClient
{

    public delegate void MessageReceivedEventHandler(object sender, MessageReceivedEventArgs e);

    public delegate void ErrorReceivedEventHandler(object sender, ErrorReceivedEventArgs e);

    public class MessageReceivedEventArgs : EventArgs
    {
        public string Message { get; }

        public MessageReceivedEventArgs(string message)
        {
            Message = message;
        }
    }

    public class ErrorReceivedEventArgs : EventArgs
    {
        public string ErrorMessage { get; }

        public ErrorReceivedEventArgs(string message)
        {
            ErrorMessage = message;
        }
    }
}
