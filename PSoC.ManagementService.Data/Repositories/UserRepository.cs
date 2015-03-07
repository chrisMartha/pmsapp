using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

using PSoC.ManagementService.Data.DataMapper;
using PSoC.ManagementService.Data.Helpers;
using PSoC.ManagementService.Data.Interfaces;
using PSoC.ManagementService.Data.Models;
using PSoC.ManagementService.Data.QueryFactory;

namespace PSoC.ManagementService.Data.Repositories
{
    public class UserRepository : Repository<UserDto, UserQuery, UserDataMapper, Guid>, IUserRepository
    {
        public async Task<IList<UserDto>> GetIdsAsync(int maxRecords = 0)
        {
            string query = @"SELECT " + (maxRecords > 0 ? string.Format(" TOP {0}", maxRecords) : "") + @" 
                                [UserID]
                            FROM [dbo].[User]";

            var result = new List<UserDto>();

            using (SqlDataReader dr = await DataAccessHelper.GetDataReaderAsync(query).ConfigureAwait(false))
            {
                while (await dr.ReadAsync().ConfigureAwait(false))
                {

                    var dto = new UserDto
                    {
                        UserID = dr.GetGuid(0)
                    };

                    result.Add(dto);
                }
            }

            return result;
        }
    }
}