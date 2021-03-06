﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PSoC.ManagementService.Data.Models;

namespace PSoC.ManagementService.Data.Interfaces
{
    public interface ILicenseRequestRepository : IDataRepository<LicenseRequestDto, Guid>
    {
        Task<bool> ArchiveLicenseRequests(int days, int batchSize);
    }
}