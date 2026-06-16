using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Linq;
using System.Threading.Tasks;
using BydClient.Config;
using BydClient.Exceptions;
using BydClient.Models;
using BydClient.Transport;

namespace BydClient.Api;

public static class ControlApi
    {
        private const string VerifyControlPasswordEndpoint = "/vehicle/vehicleswitch/verifyControlPassword";
        private const string RemoteControlEndpoint = "/control/remoteControl";
        private const string RemoteControlResultEndpoint = "/control/remoteControlResult";

        private static readonly int[] ControlPasswordErrorCodes = [5005, 5006];
        private static readonly int[] RemoteControlServiceErrorCodes = [1009];
        private static readonly int[] RemoteControlGenericErrorCodes = [1001];

        // --------------------------------------------------------
        // Build inner payload
        // --------------------------------------------------------

        private static Dictionary<string, string?> BuildControlInner(
            BydConfig config,
            string vin,
            string commandType,
            IDictionary<string, string?>? controlParams = null,
            string? commandPwd = null,
            string? requestSerial = null)
        {
            var inner = Common.BuildInnerBase(config, null, vin, requestSerial)
                .ToDictionary(kvp => kvp.Key, kvp => (string?)kvp.Value);

            inner["commandPwd"] = commandPwd ?? "";
            inner["commandType"] = commandType;

            if(controlParams != null)
            {
                inner["controlParamsMap"] = JsonSerializer.Serialize(controlParams);
            }

            return inner;
        }

        private static Dictionary<string, string?> BuildVerifyControlPasswordInner(
            BydConfig config,
            string vin,
            string commandPwd)
        {
            var inner = Common.BuildInnerBase(config, null, vin)
                .ToDictionary(kvp => kvp.Key, kvp => (string?)kvp.Value);

            inner["commandPwd"] = commandPwd;
            inner["functionType"] = "remoteControl";

            return inner;
        }

        // --------------------------------------------------------
        // Verify PIN
        // --------------------------------------------------------

        public static async Task<VerifyControlPasswordResponse> VerifyControlPasswordAsync(
            BydConfig config,
            Session session,
            ITransport transport,
            string vin,
            string commandPwd, CancellationToken cancellationToken = default)
        {
            var inner = BuildVerifyControlPasswordInner(config, vin, commandPwd);

            Dictionary<string, object?> raw;

            try
            {
                var result = await TokenJson.PostTokenJsonAsync(
                    VerifyControlPasswordEndpoint,
                    config,
                    session,
                    transport,
                    inner,
                    null,
                    vin,
                    null,
                    ControlPasswordErrorCodes);

                raw = result as Dictionary<string, object?> ?? new();
            }
            catch(BydApiException ex)
            {
                if(ControlPasswordErrorCodes.Contains(ex.HResult))
                {
                    throw new BydControlPasswordException(
                        ex.Message,
                        ex.HResult,
                        ex.Endpoint,
                        ex);
                }

                throw;
            }

            raw["vin"] = vin;
            raw["raw"] = raw;

            return new VerifyControlPasswordResponse(raw);
        }

        // --------------------------------------------------------
        // Helpers
        // --------------------------------------------------------

        private static bool IsRemoteControlReady(Dictionary<string, object?> data)
        {
            if(data.Count == 0)
                return false;

            if(data.TryGetValue("controlState", out var controlState))
            {
                if(Convert.ToInt32(controlState) != 0)
                    return true;
            }

            if(data.TryGetValue("res", out var res))
            {
                return Convert.ToInt32(res) >= 2;
            }

            return data.ContainsKey("result");
        }

        private static RemoteControlResult ParseRemoteControlResultData(
            Dictionary<string, object?> data)
        {
            return new RemoteControlResult(data);
        }

        // --------------------------------------------------------
        // Single endpoint fetch
        // --------------------------------------------------------

        private static async Task<(Dictionary<string, object?>, string?)> FetchControlEndpointAsync(
            string endpoint,
            BydConfig config,
            Session session,
            ITransport transport,
            string vin,
            string commandType,
            IDictionary<string, string?>? controlParams = null,
            string? commandPwd = null,
            string? requestSerial = null)
        {
            var inner = BuildControlInner(
                config,
                vin,
                commandType,
                controlParams,
                commandPwd,
                requestSerial);

            bool isRemote =
                endpoint == RemoteControlEndpoint ||
                endpoint == RemoteControlResultEndpoint;

            var errorCodes = isRemote
                ? ControlPasswordErrorCodes
                    .Concat(RemoteControlServiceErrorCodes)
                    .Concat(RemoteControlGenericErrorCodes)
                    .ToArray()
                : ControlPasswordErrorCodes;

            Dictionary<string, object?> result;

            try
            {
                var response = await TokenJson.PostTokenJsonAsync(
                    endpoint,
                    config,
                    session,
                    transport,
                    inner,
                    null,
                    vin,
                    null,
                    errorCodes);

                result = response as Dictionary<string, object?> ?? new Dictionary<string, object?>();
            }
            catch(BydApiException ex)
            {
                if(ControlPasswordErrorCodes.Contains(ex.HResult))
                    throw new BydControlPasswordException(ex.Message, ex.HResult, ex.Endpoint, ex);

                if(isRemote &&
                    (RemoteControlServiceErrorCodes.Contains(ex.HResult) ||
                     RemoteControlGenericErrorCodes.Contains(ex.HResult)))
                {
                    throw new BydRemoteControlException(ex.Message, ex.HResult, ex.Endpoint, ex);
                }

                throw;
            }

            string? nextSerial = null;

            if(result.TryGetValue("requestSerial", out var serialObj) &&
                serialObj is string serial)
            {
                nextSerial = serial;
            }
            else if(requestSerial != null)
            {
                nextSerial = requestSerial;
            }

            return (result, nextSerial);
        }

        // --------------------------------------------------------
        // Trigger + poll
        // --------------------------------------------------------

        public static async Task<RemoteControlResult> PollRemoteControlAsync(
            BydConfig config,
            Session session,
            ITransport transport,
            string vin,
            string commandType,
            IDictionary<string, string?>? controlParams = null,
            string? commandPwd = null,
            int pollAttempts = 10,
            double pollInterval = 1.5,
            CancellationToken cancellationToken = default)
        {
            // Phase 1: Trigger
            var (result, serial) = await FetchControlEndpointAsync(
                RemoteControlEndpoint,
                config,
                session,
                transport,
                vin,
                commandType,
                controlParams,
                commandPwd);

            if(IsRemoteControlReady(result))
            {
                var parsed = ParseRemoteControlResultData(result);

                if(parsed.ControlState == 2)
                {
                    var msg = result.ContainsKey("message")
                        ? result["message"]?.ToString()
                        : result.ContainsKey("msg")
                            ? result["msg"]?.ToString()
                            : "controlState=2";

                    throw new BydRemoteControlException(
                        $"Remote control {commandType} failed: {msg}",
                        2,
                        RemoteControlEndpoint);
                }

                return parsed;
            }

            if(serial == null)
            {
                return ParseRemoteControlResultData(result);
            }

            // Phase 2: Poll
            Dictionary<string, object?> latest = result;

            for(int attempt = 1; attempt <= pollAttempts; attempt++)
            {
                if(pollInterval > 0)
                    await Task.Delay(TimeSpan.FromSeconds(pollInterval));

                try
                {
                    var pollResult = await FetchControlEndpointAsync(
                        RemoteControlResultEndpoint,
                        config,
                        session,
                        transport,
                        vin,
                        commandType,
                        null,
                        null,
                        serial);

                    latest = pollResult.Item1;
                    serial = pollResult.Item2;

                    if(IsRemoteControlReady(latest))
                        break;
                }
                catch(BydApiException)
                {
                    // continue polling
                }
            }

            var finalParsed = ParseRemoteControlResultData(latest);

            if(finalParsed.ControlState == 2)
            {
                var msg = latest.ContainsKey("message")
                    ? latest["message"]?.ToString()
                    : latest.ContainsKey("msg")
                        ? latest["msg"]?.ToString()
                        : "controlState=2";

                throw new BydRemoteControlException(
                    $"Remote control {commandType} failed: {msg}",
                    2,
                    RemoteControlResultEndpoint);
            }

            return finalParsed;
        }
    }