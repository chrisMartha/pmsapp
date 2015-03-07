using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Microsoft.WindowsAzure.ServiceRuntime;
using PSoC.ManagementService.Core;
using PSoC.ManagementService.Data.Interfaces;
using PSoC.ManagementService.Data.Repositories;
using PSoC.ManagementService.Services;
using PSoC.ManagementService.Services.Interfaces;
using PSoC.ManagementService.Services.Logging;

namespace PSoC.ManagementService.LicenseTimer
{
    public class WorkerRole : RoleEntryPoint
    {
        private IUnityContainer _container;
        private readonly CancellationTokenSource _cancelSource = new CancellationTokenSource();
        private ILicenseService _licenseService;

        public override bool OnStart()
        {
            PEMSEventSource.Log.LicenseTimerServiceStarted("Worker role starting...");
            _container = new UnityContainer();
            RegisterTypes();

            ServicePointManager.DefaultConnectionLimit = 12;

            RoleEnvironment.Changing += RoleEnvironmentChanging; 

            var enabled = PEMSEventSource.Log.ConfigureLogAll();
            PEMSEventSource.Log.LicenseTimerServiceConfigureLog("Worker role enabling Log All (" + enabled + ")...");
            PEMSEventSource.Log.PingLog();

            return base.OnStart();
        }

        private void RegisterTypes()
        {
            _container.RegisterInstance(_container);

            _container.RegisterType<IUnitOfWork, UnitOfWork>();
            _container.RegisterType<ILicenseService, LicenseService>();
            _container.RegisterType<ILicenseRepository, LicenseRepository>();
            _container.RegisterType<ILicenseRequestRepository, LicenseRequestRepository>();
            _container.RegisterType<IDeviceService, DeviceService>();
            _container.RegisterType<IDeviceInstalledCourseRepository, DeviceInstalledCourseRepository>();
            _container.RegisterType<IAccessPointDeviceStatusRepository, AccessPointDeviceStatusRepository>();

            _licenseService = _container.Resolve<LicenseService>();
        }

        public override void OnStop()
        {
            PEMSEventSource.Log.LicenseTimerServiceShuttingDown("Worker role shutting down...");
            _cancelSource.Cancel();

            base.OnStop();
        }

        /// <summary>
        /// Event triggered when the role environment is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RoleEnvironmentChanging(object sender, RoleEnvironmentChangingEventArgs e)
        {
            // Force the role to go offline and restart for log setting change
            var change = e.Changes.OfType<RoleEnvironmentConfigurationSettingChange>().FirstOrDefault(c => c.ConfigurationSettingName == GlobalAppSettings.LogAll);
            if (change != null)
            {
                e.Cancel = true;
            }
        }

        public override void Run()
        {
            RoleEnvironment.Changed += RoleEnvironment_Changed;

            // This must never exit or the worker role will die and restart
            RunAsync().Wait();
        }

        private async Task RunAsync()
        {
            int expireInterval;
            int archiveInterval;

            try
            {
                expireInterval = GlobalAppSettings.GetInt(GlobalAppSettings.LicenseCleanupInterval);
            }
            catch (Exception e)
            {
                PEMSEventSource.Log.LicenseTimerServiceException(string.Format("Encounted exception {0} while trying to retrieve configuration setting LicenseCleanupInterval. Using default of 300 seconds (every 5 minutes)", e));
                expireInterval = 300; // Every five minutes by default		
            }

            try
            {
                archiveInterval = GlobalAppSettings.GetInt(GlobalAppSettings.LicenseArchiveCleanupInterval);
            }
            catch (Exception e)
            {
                PEMSEventSource.Log.LicenseTimerServiceException(string.Format("Encounted exception {0} while trying to retrieve configuration setting LicenseArchiveCleanupInterval. Using default of 660 seconds (every 11 minutes)", e));
                archiveInterval = 660; // Every five minutes by default		
            }
            

            var licenseEvaluator = new LicenseEvaluator(_licenseService, expireInterval);
            PEMSEventSource.Log.LicenseTimerServiceStartingEvaluator("Starting license evaluator...");
            licenseEvaluator.Start();

            var archiveEvaluator = new ArchiveEvaluator(_licenseService, archiveInterval);
            PEMSEventSource.Log.LicenseTimerServiceStartingEvaluator("Starting archive evaluator...");
            archiveEvaluator.Start();

            _cancelSource.Token.WaitHandle.WaitOne();
            PEMSEventSource.Log.LicenseTimerServiceStoppingEvaluator("Stopping license evaluator...");
            licenseEvaluator.Stop();

            PEMSEventSource.Log.LicenseTimerServiceStoppingEvaluator("Stopping archive evaluator...");
            archiveEvaluator.Stop();
        }

        void RoleEnvironment_Changed(object sender, RoleEnvironmentChangedEventArgs e)
        {
            RoleEnvironmentConfigurationSettingChange change = e.Changes.OfType<RoleEnvironmentConfigurationSettingChange>().FirstOrDefault();
            if (change != null)
            {
                // Perform an action, for example, you can initialize a client, 
                // or you can recycle the role

                if (change.ConfigurationSettingName == GlobalAppSettings.LicenseCleanupInterval)
                {
                    PEMSEventSource.Log.LicenseTimerServiceConfigChange("{0} setting has changed. Recycling the worker role...", GlobalAppSettings.LicenseCleanupInterval);
                    _cancelSource.Cancel();
                }
                else if (change.ConfigurationSettingName == GlobalAppSettings.LicenseArchiveCleanupInterval)
                {
                    PEMSEventSource.Log.LicenseTimerServiceConfigChange("{0} setting has changed. Recycling the worker role...", GlobalAppSettings.LicenseArchiveCleanupInterval);
                    _cancelSource.Cancel();
                }
            }
        }
    }
}
