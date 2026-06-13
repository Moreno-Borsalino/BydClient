using System.Collections.Generic;
using System.Threading.Tasks;

namespace BydClient.Transport;

/// <summary>
/// Structural transport interface used by endpoint modules.
/// </summary>
public interface ITransport
{
    /// <summary>
    /// Sends a fully prepared secure request payload.
    /// </summary>
    /// <param name="endpoint">API endpoint path</param>
    /// <param name="outerPayload">Encrypted/signed outer payload</param>
    /// <returns>Decoded response payload</returns>
    /// <exception cref="BydTransportException"></exception>
    Task<Dictionary<string, object>> PostSecureAsync(
        string endpoint,
        Dictionary<string, object> outerPayload, CancellationToken cancellationToken = default);
}