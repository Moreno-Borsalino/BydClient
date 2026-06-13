using System;
using System.Collections.Generic;
using System.Text;

namespace BydClient.Exceptions;

/// <summary>
/// Exception related to Bangcle encryption/decryption.
/// </summary>
internal class BangcleException : BydException
{
    public BangcleException(string message) : base(message) { }

    public BangcleException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
