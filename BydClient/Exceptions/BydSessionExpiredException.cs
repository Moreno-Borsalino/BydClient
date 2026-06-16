using System;
using System.Collections.Generic;
using System.Text;

namespace BydClient.Exceptions;

public class BydSessionExpiredException : BydApiException
{
    public BydSessionExpiredException(string message, int code = 0, string endpoint = "", Exception? previous = null)
        : base(message, code, endpoint, previous)
    {
    }
}
