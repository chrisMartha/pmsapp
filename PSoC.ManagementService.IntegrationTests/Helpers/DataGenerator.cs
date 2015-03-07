using System;
using System.Linq;

namespace PSoC.ManagementService.IntegrationTests.Helpers
{
    // TODO: Consider consolidating this class with same-named class in PSoC.ManagementService.Data.Integration test project to avoid duplicate code
    internal static class DataGenerator
    {
        internal static string GetRandomMacAddress()
        {
            var random = new Random();
            var buffer = new byte[6];
            random.NextBytes(buffer);
            var result = String.Concat(buffer.Select(x => string.Format("{0}:", x.ToString("X2"))).ToArray());
            return result.TrimEnd(':');
        }
    }
}