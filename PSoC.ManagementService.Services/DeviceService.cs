using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using PSoC.ManagementService.Core;
using PSoC.ManagementService.Core.SearchFilter;
using PSoC.ManagementService.Data.Models;
using PSoC.ManagementService.Data.Interfaces;
using PSoC.ManagementService.Services.Interfaces;
using PSoC.ManagementService.Services.Logging;
using PSoC.ManagementService.Services.Models;

namespace PSoC.ManagementService.Services
{
    public class DeviceService : /*DataService<DeviceDto, Guid>,*/ IDeviceService
    {
        private readonly IDeviceInstalledCourseRepository _deviceInstalledCourseRepository;
        private readonly IAccessPointDeviceStatusRepository _accessPointDeviceStatusRepository;

        //TODO: Add this back in once services is switched to using the new datalayer
        //public DeviceService(IUnitOfWork unitOfWork)
        //    : base(unitOfWork)
        //{
        //}

        public DeviceService(IDeviceInstalledCourseRepository deviceInstalledCourseRepository,
                             IAccessPointDeviceStatusRepository accessPointDeviceStatusRepository)
        {
            _deviceInstalledCourseRepository = deviceInstalledCourseRepository;
            _accessPointDeviceStatusRepository = accessPointDeviceStatusRepository;
        }
        
        public async Task SaveDownloadStatusAsync(Guid deviceId, List<Course> courses, LogRequest logRequest = null)
        {
            var dicDtos = new List<DeviceInstalledCourseDto>();
            try
            {
                foreach (var course in courses)
                {
                    var dicDto = (DeviceInstalledCourseDto) course;
                    dicDto.Device = new DeviceDto
                    {
                        DeviceID = deviceId
                    };
                    dicDtos.Add(dicDto);
                }

                bool result = await _deviceInstalledCourseRepository.ImportDataAsync(deviceId, dicDtos).ConfigureAwait(false);
                if (!result)
                {
                    throw new Exception(String.Format("Device service database error with device id {0}: No New Record add to deviceInstalledCourse table.", deviceId));
                }     
            }
            catch (Exception ex)
            {
                logRequest = logRequest ?? new LogRequest();
                logRequest.Exception = ex;
                PEMSEventSource.Log.DeviceServiceSaveDownloadStatusException(ex.Message, deviceId.ToString(), logRequest);
                throw;
            }           
        }

        public async Task<Tuple<List<AccessPointDeviceStatus>, int>> GetAccessPointDeviceStatusAsync(
                                                                            AdminType type, 
                                                                            Guid? id,
                                                                            int pageSize,
                                                                            int startIndex,
                                                                            IReadOnlyDictionary<FilterType, SearchFilter> filters = null,
                                                                            LogRequest logRequest = null)
            {
                if ((type == AdminType.DistrictAdmin || type == AdminType.SchoolAdmin) && id == null)
                {
                    throw new Exception(String.Format("GetAccessPointDeviceStatus Failed: Invalid id for {0}.",
                                                        type == AdminType.DistrictAdmin ? "District Admin" :
                                                        type == AdminType.SchoolAdmin   ? "School Admin"   : type.ToString()));
                }

            try
            {
                var result = await _accessPointDeviceStatusRepository.GetByAdminTypeAsync(type, id, pageSize, startIndex, filters)
                                                                     .ConfigureAwait(false);
                
                return new Tuple<List<AccessPointDeviceStatus>, int>(
                    result.Item1.Select(dto => new AccessPointDeviceStatus(dto)).ToList(),
                    result.Item2
                );
            }
            catch (Exception ex)
            {
                logRequest = logRequest ?? new LogRequest();
                logRequest.Exception = ex;
                logRequest.DistrictId = (type == AdminType.DistrictAdmin) ? ((id.HasValue) ? id.ToString() : logRequest.DistrictId) : logRequest.DistrictId;
                logRequest.SchoolId = (type == AdminType.SchoolAdmin) ? ((id.HasValue) ? id.ToString() : logRequest.SchoolId) : logRequest.SchoolId;
                PEMSEventSource.Log.DeviceServiceGetAccessPointDeviceStatusException(String.Format("GetAccessPointDeviceStatus Failed: {0}.", ex.Message), logRequest);
                throw;
            }
        }
    }
}
