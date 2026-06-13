using System;
using System.Collections.Generic;
using System.Text;

namespace BydClient.Exceptions;

/// <summary>
/// Exception related to authentication.
/// </summary>
public class BydAuthenticationException : BydException
{
    //private readonly int code;

    /// <summary>
    /// Gets the API endpoint that was called when the exception occurred.
    /// </summary>
    public string Endpoint { get; }

    public BydAuthenticationException(string message, int code = 0, string endpoint = "")
        : base(message)
    {
        this.HResult = code;
        Endpoint = endpoint;
    }
}

