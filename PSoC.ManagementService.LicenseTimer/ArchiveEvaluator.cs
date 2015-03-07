using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using PSoC.ManagementService.Services.Interfaces;
using PSoC.ManagementService.Services.Logging;

namespace PSoC.ManagementService.LicenseTimer
{
    public class ArchiveEvaluator
    {
         private readonly ILicenseService _licenseService;

        private IDisposable _subscription;
        private readonly double _intervalValue; //seconds

        public ArchiveEvaluator(ILicenseService licenseService, int intervalValue)
        {
            _licenseService = licenseService;
            _intervalValue = intervalValue;
        }

        public void Start()
        {
            _subscription = Observable.Generate(
                0,
                p => true,
                p => p,
                p => p,
                p => TimeSpan.FromSeconds(_intervalValue)
            )
                //Single stream, pulish only when we have subscribers
            .Publish().RefCount().Subscribe(async x => await ArchiveOldLicenseRequests().ConfigureAwait(false));
        }

        public void Stop()
        {
            //Publisher stops once all subscriptions end. In this case we only have one.
            _subscription.Dispose();
        }

        private async Task ArchiveOldLicenseRequests()
        {
            bool success = await _licenseService.ArchiveLicenseRequestsAsync().ConfigureAwait(false);
            PEMSEventSource.Log.LicenseTimerServiceLicensesArchive("Archive licenses request success: {0}", success);
        }
    }
}
