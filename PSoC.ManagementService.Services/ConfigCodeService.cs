using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PSoC.ManagementService.Core;
using PSoC.ManagementService.Data.Interfaces;
using PSoC.ManagementService.Data.Models;
using PSoC.ManagementService.Services.Interfaces;
using PSoC.ManagementService.Services.Logging;
using PSoC.ManagementService.Services.Models;

namespace PSoC.ManagementService.Services
{
    /// <summary>
    /// Config Code service
    /// </summary>
    public class ConfigCodeService : IConfigCodeService
    {
        private IUnitOfWork _unitOfWork { get; set; }
        private readonly IConfigCodeRepository _configCodeRepository;
        private readonly IAdminAuthorizationService _adminAuthorizationService;

        public ConfigCodeService(IUnitOfWork unitOfWork, 
            IConfigCodeRepository configCodeRepository,
            IAdminAuthorizationService adminAuthorizationService)
        {
            _unitOfWork = unitOfWork;
            _configCodeRepository = configCodeRepository;
            _adminAuthorizationService = adminAuthorizationService;
        }

        /// <summary>
        /// Retrieve all items from the database
        /// </summary>
        /// <returns>List of database model objects</returns>
        public async Task<IList<ConfigCode>> GetAsync()
        {
            try
            {
                var resultList = new List<ConfigCode>();

                foreach (var item in await _configCodeRepository.GetAsync().ConfigureAwait(false))
                {
                    resultList.Add((ConfigCode)item);
                }

                return resultList;
            }
            catch (Exception ex)
            {
                PEMSEventSource.Log.ConfigCodeServiceException(ex.Message, logRequest: new LogRequest { Exception = ex });
                throw;
            }
        }

        /// <summary>
        /// Retrieve an item from the database
        /// </summary>
        /// <param name="key">Unique database item identifier, i.e. value of primary key</param>
        /// <returns>A database model object</returns>
        public async Task<ConfigCode> GetByIdAsync(Guid key)
        {
            try
            {
                if (key == Guid.Empty)
                {
                    throw new ArgumentException("Value for ConfigCodeId is invalid", "key");
                }

                var result = (ConfigCode)await _configCodeRepository.GetByIdAsync(key).ConfigureAwait(false);
                return result;
            }
            catch (Exception ex)
            {
                PEMSEventSource.Log.ConfigCodeServiceException(ex.Message, configCodeId: key.ToString(), 
                    logRequest: new LogRequest { Exception = ex });
                throw;
            }
        }

        /// <summary>
        /// Retrieve an item from the database
        /// </summary>
        /// <param name="key">Unique database item identifier for district, i.e. value of primary key</param>
        /// <returns>A database model object</returns>
        public async Task<IList<ConfigCode>> GetByDistrictIdAsync(Guid districtId)
        {
            try
            {
                if (districtId == Guid.Empty)
                {
                    throw new ArgumentException("Value for DistrictId is invalid", "districtId");
                }

                var resultList = new List<ConfigCode>();

                foreach (var item in await _configCodeRepository.GetByDistrictIdAsync(districtId).ConfigureAwait(false))
                {
                    resultList.Add((ConfigCode)item);
                }

                return resultList;
            }
            catch (Exception ex)
            {
                PEMSEventSource.Log.ConfigCodeServiceException(ex.Message, districtId: districtId.ToString(),
                    logRequest: new LogRequest { Exception = ex });
                throw;
            }
        }

        /// <summary>
        /// Add new item to the database
        /// </summary>
        /// <param name="entity">New database model object</param>
        /// <returns>Updated database model object, e.g. with identity primary key populated</returns>
        public async Task<ConfigCode> CreateAsync(ConfigCode entity)
        {
            try
            {
                if (entity == null || entity.ConfigCodeID == Guid.Empty)
                {
                    throw new ArgumentException("Value for entity or entity ConfigCodeId is invalid", "entity");
                }

                return (ConfigCode)await _configCodeRepository.InsertAsync((ConfigCodeDto)entity).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (entity != null)
                {
                    PEMSEventSource.Log.ConfigCodeServiceException(
                        ex.Message,
                        configCodeId: entity.ConfigCodeID.ToString(),
                        districtId: entity.DistrictId.ToString(),
                        userId: entity.CreatedByUserId.ToString(),
                        logRequest: new LogRequest { Exception = ex });
                }
                else
                {
                    PEMSEventSource.Log.ConfigCodeServiceException(
                        ex.Message,                       
                        logRequest: new LogRequest { Exception = ex });
                }
                throw;
            }
        }

        /// <summary>
        /// Update existing item in the database
        /// </summary>
        /// <param name="entity">Database model object</param>
        /// <returns>Database model object</returns>
        public async Task<ConfigCode> UpdateAsync(ConfigCode entity)
        {
            try
            {
                if (entity == null || entity.ConfigCodeID == Guid.Empty)
                {
                    throw new ArgumentException("Value for entity or entity ConfigCodeId is invalid", "entity");
                }

                return (ConfigCode)await _configCodeRepository.UpdateAsync((ConfigCodeDto)entity).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (entity != null)
                {
                    PEMSEventSource.Log.ConfigCodeServiceException(
                        ex.Message,
                        configCodeId: entity.ConfigCodeID.ToString(),
                        districtId: entity.DistrictId.ToString(),
                        userId: entity.CreatedByUserId.ToString(),
                        logRequest: new LogRequest { Exception = ex });
                }
                else
                {
                    PEMSEventSource.Log.ConfigCodeServiceException(
                        ex.Message,
                        logRequest: new LogRequest { Exception = ex });
                }
                throw;
            }
        }

        /// <summary>
        /// Delete existing item from the database
        /// </summary>
        /// <param name="key">Unique database item identifier, i.e. value of primary key</param>
        /// <returns>True if item was deleted successfully, false otherwise</returns>
        public async Task<Boolean> DeleteAsync(Guid key)
        {
            try
            {
                if (key == Guid.Empty)
                {
                    throw new ArgumentException("Value for ConfigCodeId is invalid", "key");
                }

                return await _configCodeRepository.DeleteAsync(key).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                PEMSEventSource.Log.ConfigCodeServiceException(ex.Message, configCodeId: key.ToString(), 
                    logRequest: new LogRequest { Exception = ex });
                throw;
            }
        }

        /// <summary>
        /// Delete existing item from the database with access check
        /// </summary>
        /// <param name="username">User name</param>
        /// <param name="key">Unique database item identifier, i.e. value of primary key</param>
        /// <returns>True if item was deleted successfully, false otherwise</returns>
        public async Task<bool> DeleteAsync(String username, Guid key)
        {
            AdminDto admin = null;
            try
            {
                if (key == Guid.Empty)
                {
                    throw new ArgumentException("Value for ConfigCodeId is invalid", "key");
                }

                if (string.IsNullOrWhiteSpace(username))
                {
                    throw new ArgumentException("Value for username is invalid", "username");
                }

                var configCode = await GetByIdAsync(key).ConfigureAwait(false);

                if (configCode != null)
                {
                    // Access check
                    admin = await _unitOfWork.AdminRepository.GetByUsernameAsync(username).ConfigureAwait(false);
                    // Non-global admin can only delete own district
                    if (!_adminAuthorizationService.IsAuthorized(admin, configCode.DistrictId))
                        throw new UnauthorizedAccessException("Unauthorized to delete the config code.");

                    return await DeleteAsync(key).ConfigureAwait(false);
                }

                return true;
            }
            catch (Exception ex)
            {
                var userId = (admin != null) ? admin.User.UserID.ToString() : null;
                PEMSEventSource.Log.ConfigCodeServiceException(ex.Message, configCodeId: key.ToString(), 
                    userId: userId, logRequest: new LogRequest { Exception = ex });
                throw;
            }
        }

        /// <summary>
        /// Retrieve all items from the database with access check
        /// </summary>
        /// <param name="username">User name</param>
        /// <returns>List of database model objects</returns>
        public async Task<IList<ConfigCode>> GetAsync(string username)
        {
            AdminDto admin = null;
            try
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    throw new ArgumentException("Value for username is invalid", "username");
                }

                var resultList = new List<ConfigCode>();

                // Access check
                admin = await _unitOfWork.AdminRepository.GetByUsernameAsync(username).ConfigureAwait(false);
                // Non-global admin can only delete own district
                if (!_adminAuthorizationService.IsAuthorized(admin, AdminType.DistrictAdmin))
                    throw new UnauthorizedAccessException("Unauthorized to retrieve config codes.");

                switch (admin.AdminType)
                {
                    case AdminType.GlobalAdmin:
                        resultList = (List<ConfigCode>)await GetAsync().ConfigureAwait(false);
                        break;
                    case AdminType.DistrictAdmin:
                        resultList = (List<ConfigCode>)await GetByDistrictIdAsync(admin.District.DistrictId).ConfigureAwait(false);
                        break;
                }

                return resultList;
            }
            catch (Exception ex)
            {
                var userId = (admin != null) ? admin.User.UserID.ToString() : null;
                PEMSEventSource.Log.ConfigCodeServiceException(ex.Message, userId: userId, logRequest: new LogRequest { Exception = ex });
                throw;
            }
        }

        /// <summary>
        /// Retrieve an item from the database with access check
        /// </summary>
        /// <param name="username">User name</param>
        /// <param name="key">Unique database item identifier, i.e. value of primary key</param>
        /// <returns>A database model object</returns>
        public async Task<ConfigCode> GetByIdAsync(string username, Guid key)
        {
            AdminDto admin = null;
            try
            {
                if (key == Guid.Empty)
                {
                    throw new ArgumentException("Value for ConfigCodeId is invalid", "key");
                }

                if (string.IsNullOrWhiteSpace(username))
                {
                    throw new ArgumentException("Value for username is invalid", "username");
                }

                var configCode = await GetByIdAsync(key).ConfigureAwait(false);

                // Access check
                admin = await _unitOfWork.AdminRepository.GetByUsernameAsync(username).ConfigureAwait(false);
                // Non-global admin can only delete own district
                if (!_adminAuthorizationService.IsAuthorized(admin, configCode.DistrictId))
                    throw new UnauthorizedAccessException("Unauthorized to retrieve the config code.");

                return await GetByIdAsync(key).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var userId = (admin != null) ? admin.User.UserID.ToString() : null;
                PEMSEventSource.Log.ConfigCodeServiceException(ex.Message, configCodeId: key.ToString(), 
                    userId: userId, logRequest: new LogRequest { Exception = ex });
                throw;
            }
        }

        /// <summary>
        /// Add new item to the database with access check
        /// </summary>
        /// <param name="username">User name</param>
        /// <param name="entity">New database model object</param>
        /// <returns>Updated database model object, e.g. with identity primary key populated</returns>
        public async Task<ConfigCode> CreateAsync(string username, ConfigCode entity)
        {
            AdminDto admin = null;
            try
            {
                if (entity == null || entity.ConfigCodeID == Guid.Empty)
                {
                    throw new ArgumentException("Value for entity or entity ConfigCodeId is invalid", "entity");
                }

                if (string.IsNullOrWhiteSpace(username))
                {
                    throw new ArgumentException("Value for username is invalid", "username");
                }

                // Access check
                admin = await _unitOfWork.AdminRepository.GetByUsernameAsync(username).ConfigureAwait(false);
                // Non-global admin can only delete own district
                if (!_adminAuthorizationService.IsAuthorized(admin, entity.DistrictId))
                    throw new UnauthorizedAccessException("Unauthorized to create the config code.");

                return await CreateAsync(entity).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var userId = (admin != null) ? admin.User.UserID.ToString() : null;
                PEMSEventSource.Log.ConfigCodeServiceException(ex.Message, configCodeId: entity.ConfigCodeID.ToString(), 
                    districtId: entity.DistrictId.ToString(), userId: userId, logRequest: new LogRequest { Exception = ex });
                throw;
            }
        }

        /// <summary>
        /// Update existing item in the database with access check
        /// </summary>
        /// <param name="username">User name</param>
        /// <param name="entity">Database model object</param>
        /// <returns>Database model object</returns>
        public async Task<ConfigCode> UpdateAsync(string username, ConfigCode entity)
        {
            AdminDto admin = null;
            try
            {
                if (entity == null || entity.ConfigCodeID == Guid.Empty)
                {
                    throw new ArgumentException("Value for entity or entity ConfigCodeId is invalid", "entity");
                }

                if (string.IsNullOrWhiteSpace(username))
                {
                    throw new ArgumentException("Value for username is invalid", "username");
                }

                // Access check
                admin = await _unitOfWork.AdminRepository.GetByUsernameAsync(username).ConfigureAwait(false);
                // Non-global admin can only delete own district
                if (!_adminAuthorizationService.IsAuthorized(admin, entity.DistrictId))
                    throw new UnauthorizedAccessException("Unauthorized to update the config code.");

                return await UpdateAsync(entity).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var userId = (admin != null) ? admin.User.UserID.ToString() : null;
                if (entity != null)
                {
                    PEMSEventSource.Log.ConfigCodeServiceException(ex.Message, configCodeId: entity.ConfigCodeID.ToString(),
                     districtId: entity.DistrictId.ToString(), userId: userId, logRequest: new LogRequest { Exception = ex });
                }
                else
                {
                    PEMSEventSource.Log.ConfigCodeServiceException(ex.Message, userId: userId, logRequest: new LogRequest { Exception = ex });
                }                
                throw;
            }
        }
    }
}
