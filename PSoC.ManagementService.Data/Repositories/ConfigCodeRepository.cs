using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

using PSoC.ManagementService.Data.DataMapper;
using PSoC.ManagementService.Data.Interfaces;
using PSoC.ManagementService.Data.Models;
using PSoC.ManagementService.Data.QueryFactory;

namespace PSoC.ManagementService.Data.Repositories
{
    public class ConfigCodeRepository : Repository<ConfigCodeDto, ConfigCodeQuery, ConfigCodeDataMapper, Guid>, IConfigCodeRepository
    {
        public async Task<IList<ConfigCodeDto>> GetByDistrictIdAsync(Guid districtId)
        {
            var paramList = new List<SqlParameter>
            {
                new SqlParameter("@DistrictID", SqlDbType.UniqueIdentifier) { Value = districtId}
            };

            var query = GetSelectQuery("WHERE DistrictID = @DistrictID", loadNestedTypes: true);
            var result = await GetQueryResultAsync(query.QueryString, paramList, loadNestedTypes: true).ConfigureAwait(false);

            return result;
        }
    }
}
