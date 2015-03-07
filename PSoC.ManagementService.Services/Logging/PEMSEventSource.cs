using System;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using NLog.Config;

using PSoC.ManagementService.Core;
using PSoC.ManagementService.Services.Interfaces;
using PSoC.ManagementService.Services.Models;

namespace PSoC.ManagementService.Services.Logging
{
    /// <summary>
    /// A singleton object for logging events in PEMS
    /// </summary>
    [EventSource(Name = "PEMS")]
    public class PEMSEventSource : EventSource, IPEMSEventSource
    {
        public const String LogTargetName = "database";
        public const LogLevel MinLevelToLogPayload = LogLevel.Info;
        public const String MaskedValue = "---";
        public static readonly String[] FilteredFields = { "userType", "UserType", "username", "userName", "Username", "UserName", "deviceName", "DeviceName" };
        public static readonly String RegexFilterPattern = @"((?<=""usertype""[ :]+)|(?<=""userName""[ :]+)|(?<=""deviceName""[ :]+))(\""[^""]*\"")";
        private static readonly Lazy<IPEMSEventSource> Instance = new Lazy<IPEMSEventSource>(() => new PEMSEventSource());

        public ILogger Logger { get; set; }

        private PEMSEventSource() { }

        public static IPEMSEventSource Log { get { return Instance.Value; } }

        /// <summary>
        /// Add custom keywords by incrementing the enum by 2
        /// </summary>
        [Flags]
        public enum Keywords
        {
            License = 1,
            WebAPI = 2,
            Login = 4,
            Schoolnet = 8,
            Exception = 16,
            DeviceService = 32,
            LicenseTimerService = 64,
            LicenseService = 128,
            WhiteListService = 256,
            DistrictService = 512,
            AccessPointService = 1024,
            AdminService = 2048,
            UserService = 4096,
            SchoolService = 8192,
            ConfigCodeService = 16384
        }

        /// <summary>
        /// Add custom tasks
        /// </summary>
        public enum Tasks
        {
            WebAPIGet = 1,
            WebAPIPost = 2,
            WebAPIPut = 3,
            WebAPIDelete = 4
        }

        /// <summary>
        /// JSON filter method
        /// </summary>
        public enum JsonFilterMethod
        {
            Newtonsoft,
            Regex
        }

        /// <summary>
        /// Write a log
        /// </summary>
        /// <param name="request"></param>
        public void WriteLog(LogRequest request)
        {
            var logger = Logger ?? NLogger.GetLogger(request.Logger);
            if (logger.IsEnabled(request.Level))
                logger.Log(Filter(request, JsonFilterMethod.Newtonsoft));
        }

        /// <summary>
        /// Ping log by creating sample logs for all levels
        /// </summary>
        public void PingLog()
        {
            foreach (var vp in NLogger.MapLogLevels)
            {
                if (vp.Key != LogLevel.Off)
                {
                    WriteLog(new LogRequest
                    {
                        Logger = GlobalAppSettings.LogAll,
                        Level = vp.Key,
                        Message = vp.Key.ToString()
                    });
                }
            }
        }

        /// <summary>
        /// Configure log all for debugging
        /// </summary>
        /// <returns></returns>
        public Boolean ConfigureLogAll()
        {
            // Add/remove LogAll rule
            Boolean enabled = false;
            bool changedValue = GlobalAppSettings.GetBool(GlobalAppSettings.LogAll);
            var config = LogManager.Configuration;
            Func<LoggingRule, Boolean> logAllRule = r => (r.LoggerNamePattern == "*") && (r.Final);

            if (changedValue)
            {
                // "true": Add catch-all trace rule if not exist
                var isFound = config.LoggingRules.Any(logAllRule);
                if (!isFound)
                {
                    var target = config.FindTargetByName(LogTargetName);
                    config.LoggingRules.Insert(0, new LoggingRule("*", NLog.LogLevel.Trace, target) { Final = true });
                    config.Reload();
                    enabled = true;
                }
            }
            else
            {
                // "false": Remove catch-all trace rule if exist
                var loggingRule = config.LoggingRules.FirstOrDefault(logAllRule);
                if (loggingRule != null)
                {
                    config.LoggingRules.Remove(loggingRule);
                    config.Reload();
                }
            }

            return enabled;
        }

        /// <summary>
        /// Determine if payload log is enabled
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Boolean IsPayloadLogEnabled(LogRequest request)
        {
            var logger = Logger ?? NLogger.GetLogger(request.Logger);
            return logger.IsEnabled(MinLevelToLogPayload);
        }

        #region Application
        public void ApplicationStart(string message)
        {
            var req = new LogRequest
            {
                Logger = Keywords.WebAPI.ToString(),
                Level = LogLevel.Warn,
                Message = message,
                EventId = 103,
                Keywords = Keywords.WebAPI.ToString()
            };
            WriteLog(req);
        }

        public void ApplicationConfigureLog(string message)
        {
            var req = new LogRequest
            {
                Logger = Keywords.WebAPI.ToString(),
                Level = LogLevel.Warn,
                Message = message,
                EventId = 104,
                Keywords = Keywords.WebAPI.ToString()
            };
            WriteLog(req);
        }

        public void ApplicationFailure(string message, LogRequest logRequest = null)
        {
            var req = logRequest ?? new LogRequest();
            req.Logger = Keywords.Exception.ToString();
            req.Level = LogLevel.Fatal;
            req.Message = string.Format("Application Failure: {0}", message);
            req.EventId = 100;
            req.Keywords = Keywords.Exception.ToString();
            WriteLog(req);
        }

        public void ApplicationException(string message, LogRequest logRequest = null)
        {
            var req = logRequest ?? new LogRequest();
            req.Logger = Keywords.Exception.ToString();
            req.Level = LogLevel.Error;
            req.Message = string.Format("Exception Occured: {0}", message);
            req.EventId = 101;
            req.Keywords = Keywords.Exception.ToString();
            WriteLog(req);
        }

        public void AccountLoginFailed(string message, LogRequest logRequest = null)
        {
            var req = logRequest ?? new LogRequest();
            req.Logger = Keywords.Login.ToString();
            req.Level = LogLevel.Info;
            req.Message = string.Format("Exception Occured: {0}", message);
            req.EventId = 102;
            req.Keywords = Keywords.Login.ToString();
            WriteLog(req);
        }

        #endregion

        #region Schoolnet
        public void SchoolnetAuthorizationRequested(string restUrl, LogRequest logRequest = null)
        {
            var req = logRequest ?? new LogRequest();
            req.Logger = Keywords.Schoolnet.ToString();
            req.Level = LogLevel.Info;
            req.EventId = 200;
            req.Keywords = Keywords.Schoolnet.ToString();
            req.Url = restUrl;
            WriteLog(req);
        }

        public void SchoolnetAuthorizationSucceeded(LogRequest logRequest = null)
        {
            var req = logRequest ?? new LogRequest();
            req.Logger = Keywords.Schoolnet.ToString();
            req.Level = LogLevel.Info;
            req.EventId = 201;
            req.Keywords = Keywords.Schoolnet.ToString();
            WriteLog(req);
        }

        public void SchoolnetAuthorizationFailure(string message, string data, LogRequest logRequest = null)
        {
            var req = logRequest ?? new LogRequest();
            req.Logger = Keywords.Schoolnet.ToString();
            req.Level = LogLevel.Error;
            req.Message = string.Format("{0}\n[Data] {1}", message, data);
            req.EventId = 202;
            req.Keywords = (Keywords.Schoolnet | Keywords.Exception).ToString();
            WriteLog(req);
        }
        #endregion

        #region AzureTableService
        public void AzureTableServiceInvalidTable(string tableEndpoint, string tableName)
        {
            throw new NotImplementedException("Azure table is no longer supported.");
        }

        public void AzureTableServiceException(string message)
        {
            throw new NotImplementedException("Azure table is no longer supported.");
        }

        public void AzureTableServiceInsertException(string message)
        {
            throw new NotImplementedException("Azure table is no longer supported.");
        }

        public void AzureTableServiceDeleteException(string message)
        {
            throw new NotImplementedException("Azure table is no longer supported.");
        }

        public void AzureTableServiceGetException(string message)
        {
            throw new NotImplementedException("Azure table is no longer supported.");
        }
        #endregion

        #region DeviceApi

        public void DeviceApiGetFailure(string message, LogRequest logRequest = null)
        {
            DeviceApiGetFailure(message, null, logRequest);
        }

        public void DeviceApiGetFailure(string message, string deviceId, LogRequest logRequest = null, bool writeLog = false)
        {
            var req = logRequest ?? new LogRequest();
            req.Logger = Keywords.WebAPI.ToString();
            req.Level = LogLevel.Error;
            req.Message = message;
            req.EventId = 400;
            req.Task = Tasks.WebAPIGet.ToString();
            req.Keywords = (Keywords.WebAPI | Keywords.Exception).ToString();
            req.DeviceId = deviceId;
            if (writeLog)
                WriteLog(req);
        }

        public void DeviceApiPutFailure(string message, string deviceId, LogRequest logRequest = null, bool writeLog = false)
        {
            var req = logRequest ?? new LogRequest();
            req.Logger = Keywords.WebAPI.ToString();
            req.Level = LogLevel.Error;
            req.Message = message;
            req.EventId = 401;
            req.Task = Tasks.WebAPIPut.ToString();
            req.Keywords = (Keywords.WebAPI | Keywords.Exception).ToString();
            req.DeviceId = deviceId;
            if (writeLog)
                WriteLog(req);
        }

        public void DeviceApiDeleteBadRequest(string message, string deviceId, LogRequest logRequest = null, bool writeLog = false)
        {
            var req = logRequest ?? new LogRequest();
            req.Logger = Keywords.WebAPI.ToString();
            req.Level = LogLevel.Error;
            req.Message = message;
            req.EventId = 402;
            req.Task = Tasks.WebAPIDelete.ToString();
            req.Keywords = (Keywords.WebAPI | Keywords.Exception).ToString();
            req.DeviceId = deviceId;
            if (writeLog)
                WriteLog(req);
        }

        public void DeviceApiDeleteItemNotFound(string message, string deviceId, LogRequest logRequest = null, bool writeLog = false)
        {
            var req = logRequest ?? new LogRequest();
            req.Logger = Keywords.WebAPI.ToString();
            req.Level = LogLevel.Error;
            req.Message = message;
            req.EventId = 403;
            req.Task = Tasks.WebAPIDelete.ToString();
            req.Keywords = (Keywords.WebAPI | Keywords.Exception).ToString();
            req.DeviceId = deviceId;
            if (writeLog)
                WriteLog(req);
        }

        public void DeviceApiLicenseRequested(string message, string deviceId, LogRequest logRequest = null, bool writeLog = false)
        {
            var req = logRequest ?? new LogRequest();
            req.Logger = Keywords.WebAPI.ToString();
            req.Level = LogLevel.Info;
            req.Message = message;
            req.EventId = 404;
            req.Task = Tasks.WebAPIPut.ToString();
            req.Keywords = (Keywords.WebAPI | Keywords.License).ToString();
            req.DeviceId = deviceId;
            if (writeLog)
                WriteLog(req);
        }

        public void DeviceApiLicenseReturned(string message, string deviceId, LogRequest logRequest = null, bool writeLog = false)
        {
            var req = logRequest ?? new LogRequest();
            req.Logger = Keywords.WebAPI.ToString();
            req.Level = LogLevel.Info;
            req.Message = message;
            req.EventId = 408;
            req.Task = Tasks.WebAPIPut.ToString();
            req.Keywords = (Keywords.WebAPI | Keywords.License).ToString();
            req.DeviceId = deviceId;
            if (writeLog)
                WriteLog(req);
        }

        public void DeviceApiLicenseRevoked(string message, string deviceId, LogRequest logRequest = null, bool writeLog = false)
        {
            var req = logRequest ?? new LogRequest();
            req.Logger = Keywords.WebAPI.ToString();
            req.Level = LogLevel.Info;
            req.Message = message;
            req.EventId = 409;
            req.Task = Tasks.WebAPIPut.ToString();
            req.Keywords = (Keywords.WebAPI | Keywords.License).ToString();
            req.DeviceId = deviceId;
            if (writeLog)
                WriteLog(req);
        }

        public void DeviceApiLicenseFailed(string message, string deviceId, LogRequest logRequest = null, bool writeLog = false)
        {
            var req = logRequest ?? new LogRequest();
            req.Logger = Keywords.WebAPI.ToString();
            req.Level = LogLevel.Error;
            req.Message = message;
            req.EventId = 405;
            req.Task = Tasks.WebAPIPut.ToString();
            req.Keywords = (Keywords.WebAPI | Keywords.License).ToString();
            req.DeviceId = deviceId;
            if (writeLog)
                WriteLog(req);
        }

        public void DeviceApiLicenseSucceeded(string message, string deviceId, LogRequest logRequest = null, bool writeLog = false)
        {
            var req = logRequest ?? new LogRequest();
            req.Logger = Keywords.WebAPI.ToString();
            req.Level = LogLevel.Info;
            req.Message = message;
            req.EventId = 406;
            req.Task = Tasks.WebAPIPut.ToString();
            req.Keywords = (Keywords.WebAPI | Keywords.License).ToString();
            req.DeviceId = deviceId;
            if (writeLog)
                WriteLog(req);
        }

        public void DeviceApiReportDownloadStatusFailure(string message, string deviceId, LogRequest logRequest = null, bool writeLog = false)
        {
            var req = logRequest ?? new LogRequest();
            req.Logger = Keywords.WebAPI.ToString();
            req.Level = LogLevel.Error;
            req.Message = message;
            req.EventId = 407;
            req.Task = Tasks.WebAPIPost.ToString();
            req.Keywords = (Keywords.WebAPI | Keywords.Exception).ToString();
            req.DeviceId = deviceId;
            if (writeLog)
                WriteLog(req);
        }

        public void DeviceApiLicenseNotFound(string message, string deviceId, LogRequest logRequest = null, bool writeLog = false)
        {
            var req = logRequest ?? new LogRequest();
            req.Logger = Keywords.WebAPI.ToString();
            req.Level = LogLevel.Warn;
            req.EventId = 410;
            req.Task = Tasks.WebAPIPut.ToString();
            req.Keywords = (Keywords.WebAPI | Keywords.License).ToString();
            req.DeviceId = deviceId;
            if (writeLog)
                WriteLog(req);
        }

        #endregion

        #region DeviceService

        public void DeviceServiceException(string message, LogRequest logRequest = null)
        {
            var req = logRequest ?? new LogRequest();
            req.Logger = Keywords.DeviceService.ToString();
            req.Level = LogLevel.Error;
            req.Message = message;
            req.EventId = 500;
            req.Keywords = (Keywords.DeviceService | Keywords.Exception).ToString();
            WriteLog(req);
        }

        public void DeviceServiceGetDevicesRequested(string message, LogRequest logRequest = null)
        {
            var req = logRequest ?? new LogRequest();
            req.Logger = Keywords.DeviceService.ToString();
            req.Level = LogLevel.Info;
            req.Message = message;
            req.EventId = 501;
            req.Keywords = Keywords.DeviceService.ToString();
            WriteLog(req);
        }

        public void DeviceServiceInsertUpdate(string message, string deviceId, LogRequest logRequest = null)
        {
            var req = logRequest ?? new LogRequest();
            req.Logger = Keywords.DeviceService.ToString();
            req.Level = LogLevel.Info;
            req.Message = message;
            req.EventId = 502;
            req.Keywords = Keywords.DeviceService.ToString();
            WriteLog(req);
        }

        public void DeviceServiceDelete(string message, string deviceId, LogRequest logRequest = null)
        {
            var req = logRequest ?? new LogRequest();
            req.Logger = Keywords.DeviceService.ToString();
            req.Level = LogLevel.Info;
            req.Message = message;
            req.EventId = 503;
            req.Keywords = Keywords.DeviceService.ToString();
            WriteLog(req);
        }

        public void DeviceServiceSaveDownloadStatusException(string message, string deviceId, LogRequest logRequest = null)
        {
            var req = logRequest ?? new LogRequest();
            req.Logger = Keywords.DeviceService.ToString();
            req.DeviceId = deviceId;
            req.Level = LogLevel.Error;
            req.Message = message;
            req.EventId = 504;
            req.Keywords = Keywords.DeviceService.ToString();
            WriteLog(req);
        }

        public void DeviceServiceGetAccessPointDeviceStatusException(string message, LogRequest logRequest = null)
        {
            var req = logRequest ?? new LogRequest();
            req.Logger = Keywords.DeviceService.ToString();
            req.Level = LogLevel.Error;
            req.Message = message;
            req.EventId = 505;
            req.Keywords = Keywords.DeviceService.ToString();
            WriteLog(req);
        }

        #endregion

        #region LicenseTimerService

        public void LicenseTimerServiceStarted(string message)
        {
            var req = new LogRequest
            {
                Logger = Keywords.LicenseTimerService.ToString(),
                Level = LogLevel.Warn,
                Message = message,
                EventId = 600,
                Keywords = Keywords.LicenseTimerService.ToString()
            };
            WriteLog(req);
        }

        public void LicenseTimerServiceShuttingDown(string message)
        {
            var req = new LogRequest
            {
                Logger = Keywords.LicenseTimerService.ToString(),
                Level = LogLevel.Warn,
                Message = message,
                EventId = 601,
                Keywords = Keywords.LicenseTimerService.ToString()
            };
            WriteLog(req);
        }

        public void LicenseTimerServiceException(string message)
        {
            var req = new LogRequest
            {
                Logger = Keywords.LicenseTimerService.ToString(),
                Level = LogLevel.Error,
                Message = message,
                EventId = 602,
                Keywords = (Keywords.LicenseTimerService | Keywords.Exception).ToString()
            };
            WriteLog(req);
        }

        public void LicenseTimerServiceStartingEvaluator(string message)
        {
            var req = new LogRequest
            {
                Logger = Keywords.LicenseTimerService.ToString(),
                Level = LogLevel.Warn,
                Message = message,
                EventId = 603,
                Keywords = Keywords.LicenseTimerService.ToString()
            };
            WriteLog(req);
        }

        public void LicenseTimerServiceStoppingEvaluator(string message)
        {
            var req = new LogRequest
            {
                Logger = Keywords.LicenseTimerService.ToString(),
                Level = LogLevel.Warn,
                Message = message,
                EventId = 604,
                Keywords = Keywords.LicenseTimerService.ToString()
            };
            WriteLog(req);
        }

        public void LicenseTimerServiceConfigChange(string message, string configurationSettingName)
        {
            var req = new LogRequest
            {
                Logger = Keywords.LicenseTimerService.ToString(),
                Level = LogLevel.Warn,
                Message = string.Format(message, configurationSettingName),
                EventId = 605,
                Keywords = Keywords.LicenseTimerService.ToString()
            };
            WriteLog(req);
        }

        public void LicenseTimerServiceLicenses(string message, int numberOfLicenses)
        {
            var req = new LogRequest
            {
                Logger = Keywords.LicenseTimerService.ToString(),
                Level = LogLevel.Info,
                Message = string.Format(message, numberOfLicenses),
                EventId = 606,
                Keywords = Keywords.LicenseTimerService.ToString(),
                DownloadRequested = numberOfLicenses
            };
            WriteLog(req);
        }

        public void LicenseTimerServiceConfigureLog(string message)
        {
            var req = new LogRequest
            {
                Logger = Keywords.LicenseTimerService.ToString(),
                Level = LogLevel.Warn,
                Message = string.Format(message),
                EventId = 607,
                Keywords = Keywords.LicenseTimerService.ToString()
            };
            WriteLog(req);
        }

        public void LicenseTimerServiceLicensesArchive(string message, bool success)
        {
            var req = new LogRequest
            {
                Logger = Keywords.LicenseTimerService.ToString(),
                Level = LogLevel.Info,
                Message = string.Format(message, success),
                EventId = 608,
                Keywords = Keywords.LicenseTimerService.ToString()
            };
            WriteLog(req);
        }
        #endregion
		
		#region LicenseService
        public void LicenseServiceException(string message, LogRequest logRequest = null)
        {
            var req = logRequest ?? new LogRequest();
            req.Logger = Keywords.LicenseService.ToString();
            req.Level = LogLevel.Error;
            req.Message = message;
            req.EventId = 700;
            req.Keywords = (Keywords.LicenseService | Keywords.Exception).ToString();
            WriteLog(req);
        }

        public void LicenseServiceMaxExceeded(string message, int customerMax, int current, LogRequest logRequest = null)
        {
            var req = logRequest ?? new LogRequest();
            req.Logger = Keywords.LicenseService.ToString();
            req.Level = LogLevel.Info;
            req.Message = message;
            req.EventId = 701;
            req.Keywords = Keywords.LicenseService.ToString();
            WriteLog(req);
        }

        public void LicenseServiceDeleteExpiredLicenses(string message, LogRequest logRequest = null)
        {
            var req = logRequest ?? new LogRequest();
            req.Logger = Keywords.LicenseService.ToString();
            req.Level = LogLevel.Info;
            req.Message = message;
            req.EventId = 702;
            req.Keywords = Keywords.LicenseService.ToString();
            WriteLog(req);
        }
        #endregion

        #region WhiteListService

        public void WhiteListServiceException(string message, LogRequest logRequest = null)
        {
            var req = logRequest ?? new LogRequest();
            req.Logger = Keywords.WhiteListService.ToString();
            req.Level = LogLevel.Error;
            req.Message = message;
            req.EventId = 800;
            req.Keywords = Keywords.WhiteListService.ToString();
            WriteLog(req);
        }

        public void WhiteListServiceRequested(string message, string environmentId, string userId, LogRequest logRequest = null)
        {
            var req = logRequest ?? new LogRequest();
            req.Logger = Keywords.WhiteListService.ToString();
            req.Level = LogLevel.Info;
            req.UserId = userId;
            req.Message = message;
            req.EventId = 801;
            req.Keywords = Keywords.WhiteListService.ToString();
            req.ConfigCode = environmentId;
            WriteLog(req);
        }

        #endregion

        #region DistrictService
        public void DistrictServiceException(string message, string districtId = null, string userId = null, LogRequest logRequest = null)
        {
            var req = logRequest ?? new LogRequest();
            req.Logger = Keywords.DistrictService.ToString();
            req.Level = LogLevel.Error;
            req.UserId = userId;
            req.Message = message;
            req.EventId = 900;
            req.Keywords = (Keywords.DistrictService | Keywords.Exception).ToString();
            req.DistrictId = districtId;
            WriteLog(req);
        }

        public void DistrictServiceException(string message, LogRequest logRequest = null)
        {
            var req = logRequest ?? new LogRequest();
            req.Logger = Keywords.DistrictService.ToString();
            req.Level = LogLevel.Error;
            req.Message = message;
            req.EventId = 901;
            req.Keywords = (Keywords.DistrictService | Keywords.Exception).ToString();
            WriteLog(req);
        }
        #endregion

        #region AccessPointService
        public void AccessPointServiceException(string message, string accessPointId = null, string userId = null, LogRequest logRequest = null)
        {
            var req = logRequest ?? new LogRequest();
            req.Logger = Keywords.AccessPointService.ToString();
            req.Level = LogLevel.Error;
            req.UserId = userId;
            req.Message = message;
            req.EventId = 1000;
            req.Keywords = (Keywords.AccessPointService | Keywords.Exception).ToString();
            req.AccessPointId = accessPointId;
            WriteLog(req);
        }
        #endregion

        #region AdminService
        public void AdminServiceException(string message, LogRequest logRequest = null)
        {
            var req = logRequest ?? new LogRequest();
            req.Logger = Keywords.AdminService.ToString();
            req.Level = LogLevel.Error;
            req.Message = message;
            req.EventId = 1100;
            req.Keywords = (Keywords.AdminService | Keywords.Exception).ToString();
            WriteLog(req);
        }
        #endregion

        #region UserService
        public void UserServiceException(string message, LogRequest logRequest = null)
        {
            var req = logRequest ?? new LogRequest();
            req.Logger = Keywords.UserService.ToString();
            req.Level = LogLevel.Error;
            req.Message = message;
            req.EventId = 1200;
            req.Keywords = (Keywords.UserService | Keywords.Exception).ToString();
            WriteLog(req);
        }
        #endregion

        #region SchoolService
        public void SchoolServiceException(string message, string districtId = null, string schoolId = null, string userId = null, LogRequest logRequest = null)
        {
            var req = logRequest ?? new LogRequest();
            req.Logger = Keywords.SchoolService.ToString();
            req.Level = LogLevel.Error;
            req.UserId = userId;
            req.Message = message;
            req.EventId = 1300;
            req.Keywords = (Keywords.SchoolService | Keywords.Exception).ToString();
            req.DistrictId = districtId;
            req.SchoolId = schoolId;
            WriteLog(req);
        }
        #endregion

        #region ConfigCodeService

        public void ConfigCodeServiceException(string message, LogRequest logRequest = null)
        {
            var req = logRequest ?? new LogRequest();
            req.Logger = Keywords.ConfigCodeService.ToString();
            req.Level = LogLevel.Error;
            req.Message = message;
            req.EventId = 1400;
            req.Keywords = (Keywords.ConfigCodeService | Keywords.Exception).ToString();
            WriteLog(req);
        }

        public void ConfigCodeServiceException(string message, string configCodeId = null, string districtId = null, string userId = null, LogRequest logRequest = null)
        {
            var req = logRequest ?? new LogRequest();
            req.Logger = Keywords.ConfigCodeService.ToString();
            req.Level = LogLevel.Error;
            req.ConfigCode = configCodeId;
            req.UserId = userId;
            req.Message = message;
            req.EventId = 1401;
            req.Keywords = (Keywords.ConfigCodeService | Keywords.Exception).ToString();
            req.DistrictId = districtId;
            WriteLog(req);
        }

        #endregion ConfigCodeService

        #region Models
        /// <summary>
        /// Append device license request to a log request
        /// </summary>
        /// <param name="req"></param>
        /// <param name="logRequest"></param>
        /// <returns></returns>
        public LogRequest Append(DeviceLicenseRequest req, LogRequest logRequest)
        {
            if (req == null) return logRequest;
            logRequest = logRequest ?? new LogRequest();

            logRequest.UserId = req.UserId;
            logRequest.AccessPointId = req.WifiBSSID;
            logRequest.DeviceId = req.DeviceId;
            logRequest.ConfigCode = req.LastUsedConfigCode;
            logRequest.AppId = String.Format("{0}/{1}", req.PSoCAppId, req.PSoCAppVersion);
            logRequest.DownloadRequested = (req.DownloadLicenseRequested) ? 1 : (int?) null;
            logRequest.ItemsQueued = req.LearningContentQueued;

            return logRequest;
        }
        #endregion

        #region Helper methods
        /// <summary>
        /// Filter sensitive data from payload
        /// </summary>
        /// <param name="logRequest">Log request</param>
        /// <param name="method">JSON filter method</param>
        /// <returns>Filtered log request</returns>
        /// <remarks>Filter out values in JsonRequest and JsonResponse</remarks>
        public LogRequest Filter(LogRequest logRequest, JsonFilterMethod method)
        {
            if (logRequest == null) 
                return null;

            var jsonRequest = logRequest.JsonRequest;
            var jsonResponse = logRequest.JsonResponse;

            if (!String.IsNullOrWhiteSpace(jsonRequest))
                logRequest.JsonRequest = (method == JsonFilterMethod.Newtonsoft) ? FilterJsonByNewtonsoft(jsonRequest) : FilterJsonByRegex(jsonRequest);

            if (!String.IsNullOrWhiteSpace(jsonResponse))
                logRequest.JsonResponse = (method == JsonFilterMethod.Newtonsoft) ? FilterJsonByNewtonsoft(jsonResponse) : FilterJsonByRegex(jsonResponse);

            return logRequest;
        }

        /// <summary>
        /// Filter JSON by Newtonsoft
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        private String FilterJsonByNewtonsoft(String json)
        {
            // Check JSON type with the first non-whitespace
            var isArray = false;
            foreach (var c in json)
            {
                if (!Char.IsWhiteSpace(c))
                {
                    if (c == '[')
                        isArray = true;
                    break;
                }
            }

            // JSON array
            if (isArray)
            {
                var arr = JArray.Parse(json);
                foreach (JObject obj in arr)
                {
                    FilterJsonObject(obj);
                }
                return arr.ToString(Formatting.None);
            }
            // JSON object
            else
            {
                var obj = JObject.Parse(json);
                FilterJsonObject(obj);
                return obj.ToString(Formatting.None);
            }
        }

        /// <summary>
        /// Filter JSON by Regex
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        /// <remarks>Performance of this implementation is bad. Consider reusing compiled regex if needed.</remarks>
        private String FilterJsonByRegex(String json)
        {
            var regex = new Regex(RegexFilterPattern, RegexOptions.IgnoreCase);
            return regex.Replace(json, MaskedValue);
        }


        /// <summary>
        /// Filter a JSON object
        /// </summary>
        /// <param name="obj"></param>
        /// <remarks>When JSON includes filtered fields in deeper levels, this method will need to be modified and parse the entire object and examine nested objects/arrays.</remarks>
        private void FilterJsonObject(JObject obj)
        {
            foreach (var field in FilteredFields)
            {
                var filteredField = obj[field];
                if (filteredField != null)
                    obj[field] = MaskedValue;
            }
        }
        #endregion
        
    }
}
