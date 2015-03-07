using System;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.WindowsAzure;

namespace PSoC.ManagementService.Core
{
    public static class GlobalAppSettings
    {
        // Common settings names (used by both web role and worker role)
        public const string LogAll = "LogAll";

        // Web role setting names (used only by web role)
        public const string OAuthApplicationId = "OAuthApplicationId";
        public const string OAuthClientId = "OAuthClientId";
        public const string OAuthUrl = "OAuthUrl";
        public const string EncryptionCertificateThumbprint = "EncryptionCertificateThumbprint";
        public const string AuthenticationBypassEnabled = "AuthenticationBypassEnabled";
        public const string LicenseRequestRetentionInDays = "LicenseRequestRetentionInDays";
        public const string LicenseRequestArchiveBatchSize = "LicenseRequestArchiveBatchSize";
        public const string LicenseArchiveCleanupInterval = "LicenseArchiveCleanupInterval";
        public const string BulkCopyTimeoutInSecounds = "BulkCopyTimeoutInSecounds";
        public const string BulkCopyBatchSize = "BulkCopyBatchSize";
        public const string SqlCommandTimeout = "SqlCommandTimeout";    


        // Worker role setting names (used only by worker role)
        public const string LicenseCleanupInterval = "LicenseCleanupInterval";
        public const string TimeoutToDeleteExpiredLicenses = "TimeoutToDeleteExpiredLicenses";

        private static readonly HashSet<string> CloudSettingNames = new HashSet<string>
        {
            OAuthApplicationId,
            OAuthClientId,
            OAuthUrl,
            LogAll,
            EncryptionCertificateThumbprint,
            AuthenticationBypassEnabled,
            LicenseCleanupInterval,
            
            LicenseRequestRetentionInDays,
            LicenseRequestArchiveBatchSize,
            LicenseArchiveCleanupInterval,
            BulkCopyTimeoutInSecounds,
            BulkCopyBatchSize,
            TimeoutToDeleteExpiredLicenses,
            SqlCommandTimeout
        };

        /// <summary>
        /// Gets the configuration value for the specified key as string by default. An exception is thrown if missing/empty.
        /// </summary>
        /// <param name="name">Name of the setting in cloud/app/web config file</param>
        /// <exception cref="ConfigurationErrorsException">The value is not specified, or is blank.</exception>
        /// <returns>Returns value for the key</returns>
        public static string GetString(string name)
        {
            string value;
            try
            {
                value = CloudSettingNames.Contains(name) ? CloudConfigurationManager.GetSetting(name) : ConfigurationManager.AppSettings[name];
            }
            catch (Exception e)
            {
                throw new ConfigurationErrorsException("No application setting available for key: " + name, e);
            }
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ConfigurationErrorsException("No application setting available for key: " + name);
            }
            return value;
        }

        /// <summary>
        /// Gets the configuration value for the specified key, parsed as a Boolean. An exception is thrown if missing/empty.
        /// </summary>
        /// <param name="name">Name of the setting in cloud/app/web config file</param>
        /// <exception cref="ConfigurationErrorsException">The value is not specified or cannot be parsed as a Boolean.</exception>
        /// <returns>Returns value for the key</returns>
        public static bool GetBool(string name)
        {
            string str = GetString(name);
            bool value;
            if (!bool.TryParse(str, out value))
            {
                string message = string.Format("Unable to parse app setting value for {0} as a Boolean: {1}", name, str);
                throw new ConfigurationErrorsException(message);
            }
            return value;
        }

        /// <summary>
        /// Gets the configuration value for the specified key, parsed as an integer. An exception is thrown if missing/empty.
        /// </summary>
        /// <param name="name">Name of the setting in cloud/app/web config file</param>
        /// <exception cref="ConfigurationErrorsException">The value is not specified or cannot be parsed as an integer.</exception>
        /// <returns>Returns value for the key</returns>
        public static int GetInt(string name)
        {
            string str = GetString(name);
            int value;
            if (!int.TryParse(str, out value))
            {
                string message = string.Format("Unable to parse app setting value for {0} as an integer: {1}", name, str);
                throw new ConfigurationErrorsException(message);
            }
            return value;
        }
    }
}
