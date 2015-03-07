using System;
using PSoC.ManagementService.Core;
using PSoC.ManagementService.Data.Models;
using PSoC.ManagementService.Services.Models;

namespace PSoC.ManagementService.Services.Interfaces
{
    public interface IAdminAuthorizationService
    {
        /// <summary>
        /// Determine if user is authorized at given access level or higher
        /// </summary>
        /// <param name="admin">Admin DTO</param>
        /// <param name="adminType">Minimum admin access level</param>
        /// <returns>True if user is authorized at given access level or higher, false otherwise.</returns>
        bool IsAuthorized(AdminDto admin, AdminType adminType);

        /// <summary>
        /// Determine if user is authorized for the specified district
        /// </summary>
        /// <param name="admin">Admin DTO</param>
        /// <param name="districtId">District Id</param>
        /// <returns>True if user is authorized for the given district, false otherwise.</returns>
        bool IsAuthorized(AdminDto admin, Guid districtId);
        
        /// <summary>
        /// Determine if user is authorized for the specified school
        /// </summary>
        /// <param name="admin">Admin</param>
        /// <param name="school">School</param>
        /// <returns>True if user is authorized for the given school, false otherwise.</returns>
        bool IsAuthorized(AdminDto admin, School school);
    }
}
