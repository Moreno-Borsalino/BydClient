using System;
using System.Collections.Generic;
using System.Text;

namespace BydClient.Exceptions;

/// <summary>
/// Base exception for BYD API client.
/// </summary>
public class BydException : Exception
{
    public BydException() : base() { }
    public BydException(string message) : base(message) { }
    public BydException(string message, Exception? innerException) : base(message, innerException) { }
}
