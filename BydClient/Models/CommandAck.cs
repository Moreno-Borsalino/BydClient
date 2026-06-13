using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace BydClient.Models;


    /// <summary>
    /// Command acknowledgment.
    /// </summary>
    public class CommandAck : BaseModel
    {
        // Properties / getters
        public string ResultCode { get; set; } = string.Empty;

        public string ResultMsg { get; set; } = string.Empty;

        public string? SerialNumber { get; set; }

        public DateTimeOffset? Timestamp { get; set; }

    public CommandAck() { }

        public CommandAck(IDictionary<string, object?> data) : base(data)
        {
            if(data == null) throw new ArgumentNullException(nameof(data));
            Populate(data);
        }

        protected override void Populate(IDictionary<string, object?> data)
        {
            ResultCode = data["resultCode"]?.ToString() ?? string.Empty;
            ResultMsg = data["resultMsg"]?.ToString() ?? string.Empty;
            SerialNumber = data["serialNumber"]?.ToString();

            if(data.TryGetValue("timestamp", out var ts) && ts != null)
            {
                Timestamp = ParseTimestamp(ts);
            }
        }

        private static DateTimeOffset? ParseTimestamp(object timestamp)
        {
            if(timestamp == null) return null;
            if(!long.TryParse(timestamp.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var ts)) return null;

            // If value looks like milliseconds (>= 1_000_000_000_000), convert to seconds
            if(ts >= 1_000_000_000_000L) ts /= 1000L;

            try
            {
                return DateTimeOffset.FromUnixTimeSeconds(ts);
            }
            catch
            {
                return null;
            }
        }

        public bool IsSuccess()
        {
            return string.Equals(ResultCode, "success", StringComparison.OrdinalIgnoreCase)
                   || ResultCode == "0";
        }

        // Factory helper
        public static CommandAck FromDictionary(IDictionary<string, object?> data)
        {
            var inst = new CommandAck();
            inst.Populate(data);
            return inst;
        }
    }
