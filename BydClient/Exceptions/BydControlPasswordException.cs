using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace BydClient.Exceptions;

public class BydControlPasswordException : BydException
{
    /// <summary>
    /// Gets the API endpoint that was called when the exception occurred.
    /// </summary>
    public string Endpoint { get; }

    public BydControlPasswordException(string message, int code = 0, string endpoint = "", Exception? previous = null)
        : base(message, previous)
    {
        Endpoint = endpoint;
        // Nota: La proprietà HResult di System.Exception può essere utilizzata per memorizzare un codice di errore specifico.
        // Tuttavia, è importante notare che HResult è un campo protetto e non dovrebbe essere modificato direttamente.
        // Invece, è consigliabile utilizzare la proprietà HResult solo per leggere il codice di errore associato all'eccezione.
        this.HResult = code;
    }
}
