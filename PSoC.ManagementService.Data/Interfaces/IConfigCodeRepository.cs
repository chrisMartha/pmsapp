using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PSoC.ManagementService.Data.Models;

namespace PSoC.ManagementService.Data.Interfaces
{
    /// <summary>
    /// An interface for ConfigCode repository
    /// </summary>
    public interface IConfigCodeRepository : IDataRepository<ConfigCodeDto, Guid>
    {
        Task<IList<ConfigCodeDto>> GetByDistrictIdAsync(Guid districtId);
        
    }
}
