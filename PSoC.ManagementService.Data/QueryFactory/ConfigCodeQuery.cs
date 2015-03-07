using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

using PSoC.ManagementService.Data.Helpers;
using PSoC.ManagementService.Data.Interfaces;
using PSoC.ManagementService.Data.Models;
using PSoC.ManagementService.Data.Types;

namespace PSoC.ManagementService.Data.QueryFactory
{
    public class ConfigCodeQuery : IQueryFactory<ConfigCodeDto, Guid>
    {
        public QueryObject GetDeleteManyQuery(Guid[] keys)
        {
            var dt = new DataTable();
            dt.Columns.Add("Item", typeof(Guid));

            foreach (var key in keys)
                dt.Rows.Add(key);

            var paramList = new List<SqlParameter>
            {
                 new SqlParameter("@idList", SqlDbType.Structured) { TypeName = "dbo.GuidListTableType", Value = dt }
            };

            const string query = "DELETE FROM [dbo].[ConfigCode] WHERE ConfigCodeID IN (SELECT il.Item FROM @idList il)";

            return new QueryObject { QueryString = query, SqlParameters = paramList };
        }

        public QueryObject GetDeleteQuery(Guid key)
        {
            string query = @"DELETE FROM [dbo].[ConfigCode] WHERE ConfigCodeID = @ConfigCodeID";

            var paramList = new List<SqlParameter>
            {
                new SqlParameter("@ConfigCodeID", SqlDbType.UniqueIdentifier) { Value =  key},
            };

            return new QueryObject { QueryString = query, SqlParameters = paramList };
        }

        public QueryObject GetInsertQuery(ConfigCodeDto entity)
        {
            string query = @"INSERT INTO [dbo].[ConfigCode]
                           ([ConfigCodeID]
                           ,[ConfigCodeName]
                           ,[Active]
                           ,[CreatedBy]
                           ,[DistrictID]
                           ,[ConfigCodeAnnotation])
                     VALUES
                           (@ConfigCodeID
                           ,@ConfigCodeName
                           ,@Active
                           ,@CreatedBy
                           ,@DistrictID
                           ,@ConfigCodeAnnotation)"
                + Environment.NewLine;

            var paramList = GetParameterList(entity);

            return new QueryObject { QueryString = query, SqlParameters = paramList };
        }

        public QueryObject GetSelectQuery(string whereClause = "", ICollection<SqlParameter> parameters = null, bool loadNestedTypes = false)
        {
            string query = @"SELECT [ConfigCodeID]
                              ,[ConfigCodeName]
                              ,[Active]
                              ,[CreatedBy]
                              ,[Created]
                              ,[DistrictID]
                              ,[ConfigCodeAnnotation]
                          FROM [dbo].[ConfigCode]"
                + Environment.NewLine
                + whereClause;

            return new QueryObject { QueryString = query, SqlParameters = parameters };
        }

        public QueryObject GetUpdateQuery(ConfigCodeDto entity)
        {
            string query = @"UPDATE [dbo].[ConfigCode]
                   SET [ConfigCodeName] = @ConfigCodeName
                      ,[Active] = @Active
                      ,[CreatedBy] = @CreatedBy
                      ,[DistrictID] = @DistrictID
                      ,[ConfigCodeAnnotation] = @ConfigCodeAnnotation
                 WHERE [ConfigCodeID] = @ConfigCodeID"
                + Environment.NewLine;
            var paramList = GetParameterList(entity);

            return new QueryObject { QueryString = query, SqlParameters = paramList };
        }

        public IList<SqlParameter> GetParameterList(ConfigCodeDto entity)
        {           
            var paramList = new List<SqlParameter>
            {
                new SqlParameter("@ConfigCodeID", SqlDbType.UniqueIdentifier) { Value =  entity.ConfigCodeID},
                new SqlParameter("@ConfigCodeName", SqlDbType.NVarChar) { Value =  entity.ConfigCodeName },
                new SqlParameter("@Active", SqlDbType.Bit) { Value =  entity.Active},
                new SqlParameter("@CreatedBy", SqlDbType.UniqueIdentifier) { Value =  entity.CreatedBy.UserID},
                new SqlParameter("@DistrictID", SqlDbType.UniqueIdentifier) { Value =  entity.District.DistrictId},
                new SqlParameter("@ConfigCodeAnnotation", SqlDbType.NVarChar) { Value =  entity.ConfigCodeAnnotation.NullIfEmpty(), IsNullable=true}
            };

            return paramList;
        }
    }
}
