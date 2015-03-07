using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PSoC.ManagementService.Data.Helpers;
using PSoC.ManagementService.Data.Interfaces;
using PSoC.ManagementService.Data.Models;
using PSoC.ManagementService.Data.Types;

namespace PSoC.ManagementService.Data.QueryFactory
{
    public class ClassroomQuery : IQueryFactory<ClassroomDto, Guid>
    {
        public QueryObject GetDeleteManyQuery(Guid[] keys)
        {
            throw new NotImplementedException();
        }

        public QueryObject GetDeleteQuery(Guid key)
        {
            throw new NotImplementedException();
        }

        public QueryObject GetInsertQuery(ClassroomDto entity)
        {
            string query = @"INSERT INTO [dbo].[Classroom]
                               ([ClassroomID]
                               ,[ClassroomName]
                               ,[BuildingName]
                               ,[DistrictID]
                               ,[SchoolID]
                               ,[ClassroomAnnotation]
                               ,[Created])
                         VALUES
                               (@ClassroomID
                               ,@ClassroomName
                               ,@BuildingName
                               ,@DistrictID
                               ,@SchoolID
                               ,@ClassroomAnnotation
                               ,SYSUTCDATETIME())";

            var paramList = GetParameterList(entity);

            return new QueryObject { QueryString = query, SqlParameters = paramList };
        }

        public IList<SqlParameter> GetParameterList(ClassroomDto entity)
        {
            Guid districtId = new Guid();
            Guid schoolId = new Guid();

            if (entity.School != null)
            {
                schoolId = entity.School.SchoolID;
                if (entity.School.District != null)
                {
                    districtId = entity.School.District.DistrictId;
                }
            }

            var paramList = new List<SqlParameter>
            {
                new SqlParameter("@ClassroomID", SqlDbType.UniqueIdentifier) { Value =  entity.ClassroomID},
                new SqlParameter("@ClassroomName", SqlDbType.NVarChar) { Value =  entity.ClassroomName.NullIfEmpty()},
                new SqlParameter("@BuildingName", SqlDbType.NVarChar) { Value =  entity.BuildingName.NullIfEmpty()},
                new SqlParameter("@DistrictID", SqlDbType.UniqueIdentifier) { Value =  districtId.NullIfEmpty()},
                new SqlParameter("@SchoolID", SqlDbType.UniqueIdentifier) { Value =  schoolId.NullIfEmpty()},
                new SqlParameter("@ClassroomAnnotation", SqlDbType.NVarChar) { Value =  entity.ClassroomAnnotation.NullIfEmpty()}
            };

            return paramList;
        }

        public QueryObject GetSelectQuery(string whereClause = "", ICollection<SqlParameter> parameters = null, bool loadNestedTypes = false)
        {
            string query = @"SELECT [ClassroomID]
                              ,[ClassroomName]
                              ,[BuildingName]
                              ,[DistrictID]
                              ,[SchoolID]
                              ,[ClassroomAnnotation]
                              ,[Created]
                          FROM [dbo].[Classroom]"
               + Environment.NewLine
               + whereClause;

            return new QueryObject { QueryString = query, SqlParameters = parameters };
        }

        public QueryObject GetUpdateQuery(ClassroomDto entity)
        {
            throw new NotImplementedException();
        }
    }
}