using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

using PSoC.ManagementService.Data.DataMapper;
using PSoC.ManagementService.Data.Interfaces;
using PSoC.ManagementService.Data.Models;
using PSoC.ManagementService.Data.QueryFactory;

namespace PSoC.ManagementService.Data.Repositories
{
    public class SchoolRepository : Repository<SchoolDto, SchoolQuery, SchoolDataMapper, Guid>, ISchoolRepository
    {
        public async Task<IList<SchoolDto>> GetByDistrictIdAsync(Guid key)
        {
            var paramList = new List<SqlParameter>
            {
                new SqlParameter("@DistrictId", SqlDbType.UniqueIdentifier) { Value = key}
            };

            var query = GetSelectQuery("WHERE d.DistrictID = @DistrictId", loadNestedTypes: true);
            var result = await GetQueryResultAsync(query.QueryString, paramList, true).ConfigureAwait(false);

            return result;
        }
    }
}