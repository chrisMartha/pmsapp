using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PSoC.ManagementService.Core;
using PSoC.ManagementService.Core.SearchFilter;
using PSoC.ManagementService.Services.Logging;
using PSoC.ManagementService.Services.Models;

namespace PSoC.ManagementService.Services.Interfaces
{
    public interface IConfigCodeService : IDataService<ConfigCode, Guid>
    {
        Task<IList<ConfigCode>> GetAsync(String username);
        Task<ConfigCode> GetByIdAsync(String username, Guid key);
        Task<IList<ConfigCode>> GetByDistrictIdAsync(Guid districtId);
        Task<ConfigCode> CreateAsync(String username, ConfigCode entity);
        Task<ConfigCode> UpdateAsync(String username, ConfigCode entity);
        Task<Boolean> DeleteAsync(String username, Guid key);
    }
}
