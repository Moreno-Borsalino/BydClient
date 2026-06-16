using System;
using System.Collections.Generic;
using System.Text;

namespace BydClient.Exceptions;

/// <summary>
/// Exception related to HTTP transport.
/// </summary>
public class BydTransportException : Exception
{
    public int StatusCode { get; }
    public string Endpoint { get; }

    public BydTransportException(string message, int statusCode = 0, string endpoint = "", Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        Endpoint = endpoint;
    }
}

