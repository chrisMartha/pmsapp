using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using PSoC.ManagementService.Services.Logging;

namespace PSoC.ManagementService.Controllers
{
    public abstract class ControllerBase : ApiController
    {
        protected LogUser CurrentUser { get; set; }
        protected LogRequest LogRequest { get; set; }
        private readonly DateTime _startTime = DateTime.UtcNow;

        /// <summary>
        /// Execute a single HTTP operation
        /// </summary>
        /// <param name="controllerContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task<HttpResponseMessage> ExecuteAsync(HttpControllerContext controllerContext,
            CancellationToken cancellationToken)
        {
            var req = controllerContext.Request;

            // Execute request
            var pageStartTime = DateTime.UtcNow;

            // Get client info
            String _ipAddress = GetIpAddressFromRequest(req);
            String _userAgent = GetUserAgent(req);
            CurrentUser = new LogUser
            {
                IpAddress = _ipAddress,
                UserAgent = _userAgent,
                StartTime = pageStartTime
            };

            // Create the log request
            LogRequest = new LogRequest
            {
                Logger = GetType().FullName,
                Timestamp = DateTime.UtcNow,
                Level = LogLevel.Trace,
                IpAddress = _ipAddress,
                UserAgent = _userAgent,
                HttpMethod = req.Method,
                Url = req.RequestUri.ToString()
            };

            try
            {
                // Log payload setting
                Boolean isPayloadLogEnabled = PEMSEventSource.Log.IsPayloadLogEnabled(LogRequest);

                // Request
                var reqContent = req.Content;
                if (reqContent != null)
                {
                    // Get request length
                    Int64 reqlength = 0;
                    var reqContentHeaders = reqContent.Headers;
                    var reqContentType = String.Empty;
                    if (reqContentHeaders != null)
                    {
                        reqlength = reqContentHeaders.ContentLength.GetValueOrDefault();
                        reqContentType = reqContentHeaders.ContentType.ToString();
                    }

                    // Get request payload
                    if ((isPayloadLogEnabled) && (IsJsonContent(reqContentType)))
                    {
                        String requestPayload = await reqContent.ReadAsStringAsync().ConfigureAwait(false);
                        LogRequest.JsonRequest = requestPayload;

                        if (reqlength == 0)
                            reqlength = requestPayload.Length;
                    }
                    else if (reqlength == 0)
                        reqlength = (await reqContent.ReadAsStringAsync().ConfigureAwait(false)).Length;

                    LogRequest.RequestLength = reqlength;
                }

                // Execute and return a response
                var resp = await base.ExecuteAsync(controllerContext, cancellationToken).ConfigureAwait(false);

                // Response
                var respContent = resp.Content;
                if (respContent != null)
                {
                    // Get response length
                    Int64 respLength = 0;
                    var respContentHeaders = respContent.Headers;
                    var respContentType = String.Empty;
                    if (respContentHeaders != null)
                    {
                        respLength = respContentHeaders.ContentLength.GetValueOrDefault();
                        respContentType = respContentHeaders.ContentType.ToString();
                    }

                    // Get response payload
                    if ((isPayloadLogEnabled) && (IsJsonContent(respContentType)))
                    {
                        String responsePayload = await respContent.ReadAsStringAsync().ConfigureAwait(false);
                        LogRequest.JsonResponse = responsePayload;

                        if (respLength == 0)
                            respLength = responsePayload.Length;
                    }
                    else if (respLength == 0)
                        respLength = (await respContent.ReadAsStringAsync().ConfigureAwait(false)).Length;

                    LogRequest.ResponseLength = respLength;
                }

                // Get time
                var totalDuration = new TimeSpan(DateTime.UtcNow.Ticks - _startTime.Ticks).TotalMilliseconds;

                // Add response info to log request
                LogRequest.Duration = (long)totalDuration;
                LogRequest.HttpStatusCode = resp.StatusCode;

                return resp;
            }
            finally
            {
                // Write the log request
                PEMSEventSource.Log.WriteLog(LogRequest);
            }
        }

        #region Client Info
        /// <summary>
        /// Get client's user agent
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected static String GetUserAgent(HttpRequestMessage request)
        {
            if ((request == null) || (request.Headers == null))
            {
                return null;
            }

            var userAgent = request.Headers.UserAgent;
            if (userAgent != null)
            {
                var userAgentString = userAgent.ToString();
                if (!String.IsNullOrWhiteSpace(userAgentString))
                {
                    // Limit length for security purpose
                    return (userAgentString.Length > 4000)
                        ? String.Format("{0}...", userAgentString.Substring(0, 3995))
                        : userAgentString;
                }
            }

            return null;
        }

        /// <summary>
        /// Get client's IP Address from request message
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected static String GetIpAddressFromRequest(HttpRequestMessage request)
        {
            if (request == null)
            {
                return null;
            }

            Object value;

            // Web-hosting
            const String WebHostingContext = "MS_HttpContext";
            if (request.Properties.TryGetValue(WebHostingContext, out value))
            {
                if (value != null)
                {
                    var ctx = (HttpContextBase)value;
                    return ctx.Request.UserHostAddress;
                }
            }

            return null;
        }
        #endregion

        /// <summary>
        /// Determines whether the content type is JSON
        /// </summary>
        private Boolean IsJsonContent(String contentType)
        {
            if (String.IsNullOrEmpty(contentType)) return false;

            const String jsonMime = "application/json";
            return contentType.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Any(t => t.Equals(jsonMime, StringComparison.OrdinalIgnoreCase));
        }
    }
}