using System;
using System.Collections.Generic;
using System.Text;

namespace BydClient.Models.Control;

public interface IControlParams
{
    /// <summary>
    /// Convert to control params map for API requests.
    /// </summary>
    /// <returns>Dictionary with string keys and string values</returns>
    IDictionary<string, string?> ToControlParamsMap();
}



