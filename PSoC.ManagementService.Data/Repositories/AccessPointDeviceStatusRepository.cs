using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq.Expressions;
using System.Threading.Tasks;

using PSoC.ManagementService.Core;
using PSoC.ManagementService.Core.Extensions;
using PSoC.ManagementService.Core.SearchFilter;
using PSoC.ManagementService.Data.Helpers;
using PSoC.ManagementService.Data.Interfaces;
using PSoC.ManagementService.Data.Models;
using PSoC.ManagementService.Security;

namespace PSoC.ManagementService.Data.Repositories
{
    public class AccessPointDeviceStatusRepository : IAccessPointDeviceStatusRepository
    {
        public async Task<Tuple<List<DeviceDto>, int>> GetByAdminTypeAsync(AdminType type,
            Guid? id,
            int pageSize,
            int startIndex,
            IReadOnlyDictionary<FilterType, SearchFilter> filterList = null)
        {
            IDictionary<FilterType, SearchFilter> d = new Dictionary<FilterType, SearchFilter>();
            Guid guidId = id ?? Guid.Empty;
            if (type != AdminType.GlobalAdmin && guidId == Guid.Empty)
                return new Tuple<List<DeviceDto>, int>(new List<DeviceDto>(), 0);
            if (type != AdminType.GlobalAdmin && type != AdminType.DistrictAdmin && type != AdminType.SchoolAdmin)
                return new Tuple<List<DeviceDto>, int>(new List<DeviceDto>(), 0);
            if (pageSize <= 0) pageSize = 10; // Set invalid page size to default value (10)
            if (startIndex < 0) startIndex = 0; // Set invalid start index to default start index (0
            return await GetResultAsync(type, guidId, pageSize, startIndex, filterList).ConfigureAwait(false);
        }

        private async Task<Tuple<List<DeviceDto>, int>> GetResultAsync(AdminType type,
            Guid id,
            int pageSize,
            int startIndex,
            IReadOnlyDictionary<FilterType, SearchFilter> filterList)
        {
            string innerJoinsClause;
            var paramList = new List<SqlParameter>
            {
                new SqlParameter("@startIndex", SqlDbType.Int) {Value = startIndex},
                new SqlParameter("@pageSize", SqlDbType.Int) {Value = pageSize}
            };

            switch (type)
            {
                    //TODO: [Performance]Create index for lr.[WifiBSSID] if there's a performance issue for district/school admin
                case AdminType.DistrictAdmin:
                    innerJoinsClause = @"
                        INNER JOIN [dbo].[AccessPoint] ap ON lr.[WifiBSSID] = ap.[WifiBSSID] AND ap.[DistrictID] = @districtId
                    ";
                    break;

                case AdminType.SchoolAdmin:
                    innerJoinsClause = @"
                        INNER JOIN [dbo].[AccessPoint] ap ON lr.[WifiBSSID] = ap.[WifiBSSID] AND ap.[SchoolID] = @schoolId
                    ";
                    break;

                default:
                    innerJoinsClause = string.Empty;
                    break;
            }

            string filtersWhereClause = ApplyFilters(type, paramList, filterList);

            //TODO: [Performance]Create index for #TempTable is we get large amount of records at one time
            string query = String.Format(@"
                CREATE TABLE #TempTable (
		                [LicenseRequestID]		UNIQUEIDENTIFIER,
		                [TotalRows]				INT
                )
                ---------------------------------------------------------- 
                INSERT INTO #TempTable (
		                [LicenseRequestID],
		                [TotalRows]
                )
                SELECT  [LicenseRequestID],
		                COUNT(1) OVER() AS [TotalRows]
                FROM (
	                SELECT
		                lr.[Created],
		                lr.[LicenseRequestID],
		                ROW_NUMBER() OVER(
						                PARTITION BY lr.[DeviceID] 
					                    ORDER BY lr.[Created] DESC,
								                 lr.[LicenseRequestTypeID] ASC
					                 ) [SEQ]
		                FROM [dbo].[LicenseRequest] lr	
                        {0}
                        {1}
	                ) r
                WHERE r.[SEQ] = 1	
                ORDER BY r.[Created] DESC
                OFFSET @startIndex ROWS
                FETCH NEXT @pageSize ROWS ONLY
                ---------------------------------------------------------- 
                DECLARE @utcToday DateTime
                SET @utcToday = GETUTCDATE();
                SELECT  lr.[Created],
                        dis.[DistrictName],
		                s.[SchoolName],
		                lr.[DeviceID],
		                d.[DeviceName],
		                d.[DeviceType],
		                d.[DeviceOSVersion],
		                u.[Username],
		                u.[UserType],
                        d.[ConfiguredGrades],
                        lr.[LocationName],
		                lr.[WifiBSSID],
		                ap.[WifiSSID],
		                lr.[LicenseRequestTypeID],
                        lr.[LearningContentQueued],
                        [CanRevoke] = 
                        CASE 
                            WHEN     c.[LicenseRequestID] IS NOT NULL 
				                 AND c.[LicenseExpiryDateTime] > @utcToday 
			                THEN 1
                            ELSE 0
                        END,
                        tmp.[TotalRows]
                FROM #TempTable tmp
                    INNER JOIN  [dbo].[LicenseRequest]  lr  ON tmp.[licenseRequestID] = lr.[LicenseRequestID]
	                LEFT JOIN   [dbo].[Device]          d   ON lr.[DeviceID] = d.[DeviceID]		
                    LEFT JOIN   [dbo].[User]            u   ON lr.[UserID] = u.[UserID]
                    LEFT JOIN   [dbo].[License]         c   ON lr.[LicenseRequestID] = c.[LicenseRequestID]
                    LEFT JOIN   [dbo].[AccessPoint]     ap  ON lr.[WifiBSSID] = ap.[WifiBSSID]
                    LEFT JOIN   [dbo].[District]        dis ON ap.[DistrictID] = dis.[DistrictID]
                    LEFT JOIN   [dbo].[School]          s   ON ap.[SchoolID]   = s.[SchoolID]
                ORDER BY lr.[Created] DESC
                ----------------------------------------------------------           
                DROP TABLE #TempTable
            ", innerJoinsClause,
                filtersWhereClause);

            switch (type)
            {
                case AdminType.DistrictAdmin:
                    paramList.Add(new SqlParameter("@districtId", SqlDbType.UniqueIdentifier) {Value = id});
                    break;
                case AdminType.SchoolAdmin:
                    paramList.Add(new SqlParameter("@schoolId", SqlDbType.UniqueIdentifier) {Value = id});
                    break;
            }

            var result = new List<DeviceDto>();
            int totalRows = 0;
            bool getTotalRows = false;
            using (var dr = await DataAccessHelper.GetDataReaderAsync(query, paramList).ConfigureAwait(false))
            {
                if (dr.HasRows)
                {
                    while (await dr.ReadAsync().ConfigureAwait(false))
                    {
                        var dto = new DeviceDto
                        {
                            DeviceID = dr.GetGuid(3),
                            DeviceNameEnc = dr.IsDBNull(4) ? null : new EncrypedField<string>((byte[]) dr.GetValue(4)),
                            DeviceType = dr.IsDBNull(5) ? null : dr.GetString(5),
                            DeviceOSVersion = dr.IsDBNull(6) ? null : dr.GetString(6),
                            ConfiguredGrades = dr.IsDBNull(9) ? null : dr.GetString(9),
                            LastLicenseRequest = new LicenseRequestDto
                            {
                                User = new UserDto
                                {
                                    UsernameEnc =
                                        dr.IsDBNull(7) ? null : new EncrypedField<string>((byte[]) dr.GetValue(7)),
                                    UserTypeEnc =
                                        dr.IsDBNull(8) ? null : new EncrypedField<string>((byte[]) dr.GetValue(8))
                                },
                                AccessPoint = new AccessPointDto
                                {
                                    WifiBSSID = dr.GetString(11),
                                    WifiSSID = dr.GetString(12),
                                    District = dr.IsDBNull(1) ? null : new DistrictDto {DistrictName = dr.GetString(1)},
                                    School = dr.IsDBNull(2) ? null : new SchoolDto {SchoolName = dr.GetString(2)}
                                },
                                LocationName = dr.IsDBNull(10) ? null : dr.GetString(10),
                                LicenseRequestType = (LicenseRequestType) dr.GetInt32(13),
                                LearningContentQueued = dr.IsDBNull(14) ? (int?) null : dr.GetInt32(14),
                                Created = dr.GetDateTime(0)
                            },
                            CanRevoke = dr.GetInt32(15) == 1,
                        };

                        result.Add(dto);

                        if (!getTotalRows)
                        {
                            totalRows = dr.GetInt32(16);
                            getTotalRows = true;
                        }
                    }
                }
            }
            return new Tuple<List<DeviceDto>, int>(result, totalRows);
        }

        private static string ApplyFilters(AdminType adminType, ICollection<SqlParameter> paramList,
            IReadOnlyDictionary<FilterType, SearchFilter> filterList)
        {
            if (!filterList.HasElements()) return string.Empty;

            const string whereClause = " WHERE ";
            const string multiSelectFiltersConjunction = " OR ";
            //This is only needed for a Global Admin as other admin types are already joining the AccessPoint table
            bool isGlobalAdmin = adminType == AdminType.GlobalAdmin;
            bool isDistrictAdmin = adminType == AdminType.DistrictAdmin;
            IList<string> multiSelectFilters = new List<string>();

            string baseClause = isGlobalAdmin
                ? @" INNER JOIN [dbo].[AccessPoint] ap ON lr.[WifiBSSID] = ap.[WifiBSSID] "
                : string.Empty;
        
            foreach (var filter in filterList)
            {
                switch (filter.Key)
                {
                    case FilterType.DistrictId:
                        var districtFilter = filter.Value as DistrictFilter;
                        bool applyDistrictFilter = isGlobalAdmin && districtFilter != null && districtFilter.IsEnabled;

                        if (applyDistrictFilter)
                        {
                            const string districtClause = @"
                                        EXISTS (SELECT 1 FROM @dtDistricts filDistrict WHERE ap.[DistrictID] = filDistrict.[Item]) ";
                            AddMultiSelectFilterParam(paramList, FilterType.DistrictId, districtFilter.IdValues);
                            multiSelectFilters.Add(districtClause);
                        }
                        break;
                    case FilterType.SchoolId:
                        var schoolFilter = filter.Value as SchoolFilter;
                        bool applySchoolFilter = (isGlobalAdmin || isDistrictAdmin) &&
                                                 schoolFilter != null && schoolFilter.IsEnabled;

                        if (applySchoolFilter)
                        {
                            const string schoolClause = @" EXISTS (SELECT 1 FROM @dtSchools filSchool WHERE 
                                                    ap.[SchoolID] = filSchool.[Item]) ";
                            AddMultiSelectFilterParam(paramList, FilterType.SchoolId, schoolFilter.IdValues);
                            multiSelectFilters.Add(schoolClause);
                        }
                        break;
                }
            }

            return multiSelectFilters.HasElements()
                ? baseClause + whereClause + string.Join(multiSelectFiltersConjunction, multiSelectFilters)
                : string.Empty;
        }

        private static void AddMultiSelectFilterParam(ICollection<SqlParameter> paramList, FilterType filterType,
            IEnumerable<Guid> idValues)
        {
            string paramName = string.Empty;
            switch (filterType)
            {
                case FilterType.DistrictId:
                    paramName = "@dtDistricts";
                    break;
                case FilterType.SchoolId:
                    paramName = "@dtSchools";
                    break;
            }

            const string paramTypeName = "[dbo].[GuidListTableType]";
            const string columnName = "Item";
            var columnType = typeof (Guid);

            var dtTableValues = new DataTable();
            dtTableValues.Columns.Add(columnName, columnType);
            foreach (var id in idValues)
            {
                DataRow rowdtTableValues = dtTableValues.NewRow();
                rowdtTableValues[0] = id;
                dtTableValues.Rows.Add(rowdtTableValues);
            }

            var param = new SqlParameter(paramName, SqlDbType.Structured)
            {
                Direction = ParameterDirection.Input,
                TypeName = paramTypeName,
                Value = dtTableValues,
            };

            paramList.Add(param);
        }

        public Task<bool> DeleteAsync(Guid key)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteAsync(Guid[] keys)
        {
            throw new NotImplementedException();
        }

        public Task<IList<DeviceDto>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<DeviceDto> GetByIdAsync(Guid key)
        {
            throw new NotImplementedException();
        }

        public Task<IList<DeviceDto>> GetAsync(Expression<Func<IEnumerable<DeviceDto>, IEnumerable<DeviceDto>>> predicate)
        {
            throw new NotImplementedException();
        }

        public Task<DeviceDto> InsertAsync(DeviceDto entity)
        {
            throw new NotImplementedException();
        }

        public Task<DeviceDto> UpdateAsync(DeviceDto entity)
        {
            throw new NotImplementedException();
        }
    }
}
