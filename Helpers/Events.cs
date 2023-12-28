using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolygonApiClient
{
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
