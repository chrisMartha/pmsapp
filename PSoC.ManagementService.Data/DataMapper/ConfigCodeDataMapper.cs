using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

using PSoC.ManagementService.Data.Interfaces;
using PSoC.ManagementService.Data.Models;
using PSoC.ManagementService.Security;
namespace PSoC.ManagementService.Data.DataMapper
{
    public class ConfigCodeDataMapper : IDataMapper<ConfigCodeDto>
    {
        public async Task<IList<ConfigCodeDto>> ToEntityListAsync(DbDataReader dr, bool loadNestedTypes = false)
        {
            var results = new List<ConfigCodeDto>();
            if (dr.HasRows)
            {
                while (await dr.ReadAsync().ConfigureAwait(false))
                {
                    var dto = new ConfigCodeDto
                    {
                        ConfigCodeID = dr.GetGuid(0),
                        ConfigCodeName = dr.GetString(1),
                        Active = dr.GetBoolean(2),
                        CreatedBy = new UserDto {  UserID = dr.GetGuid(3)},
                        Created = dr.GetDateTime(4),
                        District = new DistrictDto { DistrictId = dr.GetGuid(5)},
                        ConfigCodeAnnotation = dr.IsDBNull(6) ? null : dr.GetString(6)
                    };

                    results.Add(dto);
                }
            }

            return results;
        }
    }
}
