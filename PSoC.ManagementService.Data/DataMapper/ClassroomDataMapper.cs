using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PSoC.ManagementService.Data.Interfaces;
using PSoC.ManagementService.Data.Models;

namespace PSoC.ManagementService.Data.DataMapper
{
    public class ClassroomDataMapper : IDataMapper<ClassroomDto>
    {
        public async Task<IList<ClassroomDto>> ToEntityListAsync(DbDataReader dr, bool loadNestedTypes = false)
        {
            var results = new List<ClassroomDto>();

            if (dr.HasRows)
            {
                while (await dr.ReadAsync().ConfigureAwait(false))
                {
                    SchoolDto school = null;
                    DistrictDto district = null;

                    if (!dr.IsDBNull(3))
                    {
                        district = new DistrictDto
                        {
                            DistrictId = dr.GetGuid(3)
                        };
                    }

                    if (!dr.IsDBNull(4))
                    {
                        school = new SchoolDto
                        {
                            SchoolID = dr.GetGuid(4),
                            District = district
                        };
                    }

                    var dto = new ClassroomDto
                    {
                        ClassroomID = dr.GetGuid(0),
                        ClassroomName = dr.IsDBNull(1) ? null : dr.GetString(1),
                        BuildingName = dr.IsDBNull(2) ? null : dr.GetString(2),
                        School = school,
                        ClassroomAnnotation = dr.IsDBNull(5) ? null : dr.GetString(5)
                    };

                    results.Add(dto);
                }
            }
            return results;

        }
    }
}