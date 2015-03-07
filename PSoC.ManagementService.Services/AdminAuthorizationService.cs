using System;
using PSoC.ManagementService.Core;
using PSoC.ManagementService.Data.Models;
using PSoC.ManagementService.Services.Interfaces;
using PSoC.ManagementService.Services.Models;

namespace PSoC.ManagementService.Services
{
    public class AdminAuthorizationService : IAdminAuthorizationService
    {
        /// <summary>
        /// Determine if user is authorized at given access level or higher
        /// </summary>
        /// <param name="admin">Admin DTO</param>
        /// <param name="adminType">Minimum admin access level</param>
        /// <returns>True if user is authorized at given access level or higher, false otherwise.</returns>
        public bool IsAuthorized(AdminDto admin, AdminType adminType)
        {
            if (admin == null)
                return false;

            switch (adminType)
            {
                case AdminType.GlobalAdmin:
                    return (admin.AdminType == AdminType.GlobalAdmin);

                case AdminType.DistrictAdmin:
                    return (admin.AdminType == AdminType.GlobalAdmin
                        || admin.AdminType == AdminType.DistrictAdmin);

                case AdminType.SchoolAdmin:
                    return (admin.AdminType == AdminType.GlobalAdmin
                        || admin.AdminType == AdminType.DistrictAdmin
                        || admin.AdminType == AdminType.SchoolAdmin);

                default:
                    return false;
            }
        }

        /// <summary>
        /// Determine if user is authorized for the specified district
        /// </summary>
        /// <param name="admin">Admin DTO</param>
        /// <param name="districtId">District Id</param>
        /// <returns>True if user is authorized for the given district, false otherwise.</returns>
        public bool IsAuthorized(AdminDto admin, Guid districtId)
        {
            if ((admin == null) || (districtId == default(Guid)))
                return false;

            switch (admin.AdminType)
            {
                // Global admin: all districts
                case AdminType.GlobalAdmin:
                    return true;

                // District admin: own district only
                case AdminType.DistrictAdmin:
                    return (admin.District.DistrictId == districtId);

                default:
                    return false;
            }
        }

        /// <summary>
        /// Determine if user is authorized for the specified school
        /// </summary>
        /// <param name="admin">Admin</param>
        /// <param name="school">School</param>
        /// <returns>True if user is authorized for the given school, false otherwise.</returns>
        public bool IsAuthorized(AdminDto admin, School school)
        {
            if ((admin == null) || (school == null))
                return false;

            switch (admin.AdminType)
            {
                // Global admin: all schools
                case AdminType.GlobalAdmin:
                    return true;

                // District admin: school is in own district only
                case AdminType.DistrictAdmin:
                    return (admin.District.DistrictId == school.District.DistrictId);

                // School admin: own school only
                case AdminType.SchoolAdmin:
                    return (admin.School.SchoolID == school.SchoolId);

                default:
                    return false;
            }
        }
    }
}
