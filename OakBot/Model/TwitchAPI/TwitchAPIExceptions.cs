using System;
using System.Net;

namespace OakBot.Model
{
    internal class TwitchAPIException : Exception
    {
        internal HttpStatusCode Status { get; private set; }

        internal TwitchAPIException(string message, HttpStatusCode status) : base(message)
        {
            Status = status;
        }
    }
}
