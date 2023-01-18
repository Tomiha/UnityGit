using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using VenlySDK.Models;

namespace VenlySDK.Core
{
    public class VyException : Exception
    {
        public static implicit operator VyException(ArgumentException ex) => new(ex);

        public VyException(Exception ex) : base(ex.Message, ex)
        {
        }

        public VyException(string msg) : base(msg)
        {
        }

        public static void RaiseUnhandled(VyException unhandledException)
        {
#if ENABLE_VENLY_AZURE
            throw unhandledException;
#else
            UnityEngine.Debug.LogException(new Exception("[Venly Unhandled Exception]", unhandledException));
#endif
        }

        public static VyException Argument(string message, string paramName,
            [CallerMemberName] string callerName = "")
        {
            var exception =
                new VyException(
                    $"[Argument Exception] Parameter \'{paramName}\' (Method: {callerName}) is invalid!\nReason: {message}");
            //Todo set StackTrace
            return exception;
        }

        public static VyException ParsingError(string message)
        {
            return new VyException($"[Parsing Error] {message}");
        }

        public static VyException HttpResponseError(HttpResponseMessage msg, VyRequestData requestData = null)
        {
            if (requestData != null)
                return new VyException(
                    $"[HTTP Response] Code {msg.StatusCode} : {msg.ReasonPhrase}\nRequest Details: [{requestData.Method}] || {requestData.Uri}");

            return new VyException(
                $"[HTTP Response] Code {msg.StatusCode} : {msg.ReasonPhrase}\nNo Request Details available.");
        }

        public static VyException ApiResponseError(VyResponseDto apiResponse, HttpStatusCode statusCode,
            VyRequestData requestData)
        {
            var sb = new StringBuilder();

            if (apiResponse.Errors == null || !apiResponse.Errors.Any())
            {
                return new VyException($"[API Response] The reponse for {requestData.Uri} returned an error ({statusCode})");
            }

            sb.AppendLine(
                $"[API Response] The reponse for {requestData.Uri} returned {apiResponse.Errors.Length} error(s). ({statusCode})");

            int counter = 1;
            foreach (var err in apiResponse.Errors)
            {
                sb.AppendLine($"({counter}) [code={err.Code}, msg={err.Message}, traceCode={err.TraceCode}]");
                ++counter;
            }

            return new VyException(sb.ToString());
        }
    }
}