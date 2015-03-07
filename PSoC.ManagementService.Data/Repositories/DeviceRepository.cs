using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using PSoC.ManagementService.Data.Helpers;
using PSoC.ManagementService.Data.Types;
using PSoC.ManagementService.Data.DataMapper;
using PSoC.ManagementService.Data.Interfaces;
using PSoC.ManagementService.Data.Models;
using PSoC.ManagementService.Data.QueryFactory;

namespace PSoC.ManagementService.Data.Repositories
{
    public class DeviceRepository : Repository<DeviceDto, DeviceQuery, DeviceDataMapper, Guid>, IDeviceRepository
    {
        public async Task<IList<DeviceDto>> GetIdsAsync(int maxRecords = 0)
        {
            string query = @"SELECT " + (maxRecords > 0 ? string.Format(" TOP {0}", maxRecords) : "") + @" 
                                 [DeviceID]                                
                                ,[DistrictID]
                                ,[SchoolID]                               
                            FROM [dbo].[Device]";

            var result = new List<DeviceDto>() ;

            using (SqlDataReader dr = await DataAccessHelper.GetDataReaderAsync(query).ConfigureAwait(false))
            {
                while (await dr.ReadAsync().ConfigureAwait(false))
                {
                    SchoolDto school = null;
                    DistrictDto district = null;

                    if (!dr.IsDBNull(1))
                    {
                        district = new DistrictDto
                        {
                            DistrictId = dr.GetGuid(1)
                        };
                    }

                    if (!dr.IsDBNull(2))
                    {
                        school = new SchoolDto
                        {
                            SchoolID = dr.GetGuid(2),
                            District = district
                        };
                    }

                    var dto = new DeviceDto
                    {
                        DeviceID = dr.GetGuid(0),
                        School = school
                    };

                    result.Add(dto);
                }
            }

            return result;
        }
    }
}