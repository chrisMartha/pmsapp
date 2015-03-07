using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

using PSoC.ManagementService.Data.DataMapper;
using PSoC.ManagementService.Data.Helpers;
using PSoC.ManagementService.Data.Interfaces;
using PSoC.ManagementService.Data.Models;
using PSoC.ManagementService.Data.QueryFactory;
using PSoC.ManagementService.Core;

namespace PSoC.ManagementService.Data.Repositories
{
    public class LicenseRequestRepository : Repository<LicenseRequestDto, LicenseRequestQuery, LicenseRequestDataMapper, Guid>, ILicenseRequestRepository
    {
        public new async Task<LicenseRequestDto> GetByIdAsync(Guid key)
        {
            var paramList = new List<SqlParameter>
            {
                new SqlParameter("@LicenseRequestID", SqlDbType.UniqueIdentifier) { Value = key}
            };

            var query = GetSelectQuery("WHERE lr.LicenseRequestID = @LicenseRequestID", loadNestedTypes: true);
            var result = await GetQueryResultAsync(query.QueryString, paramList, loadNestedTypes: true).ConfigureAwait(false);

            return result.FirstOrDefault();
        }

        public new async Task<LicenseRequestDto> InsertAsync(LicenseRequestDto entity)
        {
            entity.LicenseRequestID = entity.LicenseRequestID.CreateIfEmpty();

            var query = GetInsertQuery(entity);
            query.QueryString += Environment.NewLine
             + GetSelectQuery("WHERE lr.LicenseRequestID = @LicenseRequestID", loadNestedTypes: true).QueryString;

            var result = await GetQueryResultAsync(query.QueryString, query.SqlParameters, loadNestedTypes: true).ConfigureAwait(false);

            return result.FirstOrDefault();
        }

        public new async Task<LicenseRequestDto> UpdateAsync(LicenseRequestDto entity)
        {
            var query = GetUpdateQuery(entity);
            query.QueryString += Environment.NewLine
             + GetSelectQuery("WHERE lr.LicenseRequestID = @LicenseRequestID", loadNestedTypes: true).QueryString;

            var result = await GetQueryResultAsync(query.QueryString, query.SqlParameters, loadNestedTypes: true).ConfigureAwait(false);

            return result.FirstOrDefault();
        }

        public async Task<bool> ArchiveLicenseRequests(int days, int batchSize)
        {
            bool result = true;

            string query = @"SELECT R.[LicenseRequestID]
                              ,R.[PrevLicenseRequestID]
                              ,R.[DistrictID]
                              ,R.[SchoolID]
                              ,R.[ConfigCode]
                              ,R.[WifiBSSID]
                              ,R.[LicenseRequestTypeID]
                              ,R.[DeviceID]
                              ,R.[UserID]
                              ,R.[RequestDateTime]
                              ,R.[Response]
                              ,R.[ResponseDateTime]
                              ,R.[LocationID]
                              ,R.[LocationName]
                              ,R.[LearningContentQueued]
                              ,R.[Created]
						 FROM [dbo].[LicenseRequest] R
                         LEFT JOIN [dbo].[License] L 
                        ON R.[LicenseRequestID]= L.[LicenseRequestID]
                        WHERE L.[LicenseRequestID] IS NULL AND R.[Created] <= @ArchiveDate
                        OPTION (MAXDOP 1)";

            var paramList = new List<SqlParameter>
            {
                new SqlParameter("@ArchiveDate", SqlDbType.DateTime) {Value = DateTime.UtcNow.AddDays(days * -1)}
            };

            List<Guid> deletedIds = new List<Guid>();
            
            //Using datatable because we need the record set twice.
            using (DataTable dt = await DataAccessHelper.GetDataTableAsync(query, paramList).ConfigureAwait(false))
            {
                if (dt != null && dt.Rows.Count > 0)
                {
                    string connectionString = ConfigurationManager.ConnectionStrings["PemsArchiveConnectionString"].ConnectionString;

                    using (SqlConnection destinationConnection = new SqlConnection(connectionString))
                    {
                        destinationConnection.Open();
                        using (SqlBulkCopy bulkCopy = new SqlBulkCopy(destinationConnection, 
                            SqlBulkCopyOptions.TableLock | SqlBulkCopyOptions.UseInternalTransaction, null))
                        {
                            bulkCopy.BulkCopyTimeout = GlobalAppSettings.GetInt(GlobalAppSettings.BulkCopyTimeoutInSecounds);
                            bulkCopy.BatchSize = GlobalAppSettings.GetInt(GlobalAppSettings.BulkCopyBatchSize);
                            bulkCopy.EnableStreaming = true;
                            bulkCopy.DestinationTableName = "dbo.LicenseRequest";

                            // Write from the source to the destination.
                            using (var dataTableReader = new DataTableReader(dt))
                            {
                                await bulkCopy.WriteToServerAsync(dataTableReader).ConfigureAwait(false);
                            }

                        }                     
                    }

                    // Get list of ID's to delete
                    foreach(DataRow row in dt.Rows)
                    {
                        deletedIds.Add((Guid)row[0]);
                    }
                }
            }

            if (deletedIds.Count > 0)
            {
                List<Guid> batchDeletedIds = new List<Guid>();
                int deleteCount = 0;

                foreach (var key in deletedIds)
                {
                    batchDeletedIds.Add(key);
                    deleteCount++;

                    //LicenseRequestArchiveBatchSize

                    // Send delete request in batches of batchSize for performance.
                    if (deleteCount >= batchSize)
                    {
                        deleteCount = 0;
                        if (await DeleteAsync(batchDeletedIds.ToArray()).ConfigureAwait(false) == false)
                            result = false;

                        batchDeletedIds = new List<Guid>();
                    }
                }

                if (deleteCount > 0)
                {
                    if (await DeleteAsync(batchDeletedIds.ToArray()).ConfigureAwait(false) == false)
                        result = false;
                }
            }

            return result;
        }
    }
}