using Dapper;
using Npgsql;
using NpgsqlTypes;
using RavelDev.Core.Extensions;
using RavelDev.Core.Interfaces;
using RavelDev.KentMapping.Police.Core.Models;
using RavelDev.KentMapping.Police.Core.Models.Web;
using System.Data;


namespace RavelDev.KentMapping.Police.Core.Repo
{



    public class KentPoliceRepository
    {
        IRepositoryConfig _config;
        public KentPoliceRepository(IRepositoryConfig config)
        {
            _config = config;


        }

        public int InsertIncidentFile(string fileName)
        {
            int fileId;
            using var conn = new NpgsqlConnection(_config.ConnectionString);
            var sql = @"SELECT * FROM insertpoliceincidentfile(@filename)";
            var p = new DynamicParameters();
            p.Add("@filename", fileName);

            fileId = conn.ExecuteScalar<int>(sql, p);

            return fileId;

        }

        public int InsertPoliceReportFile(string fileName)
        {
            int fileId;
            using var conn = new NpgsqlConnection(_config.ConnectionString);
            var sql = @"SELECT * FROM insertpolicereportfile(@filename)";
            var p = new DynamicParameters();
            p.Add("@filename", fileName);

            fileId = conn.ExecuteScalar<int>(sql, p);

            return fileId;
        }
        public void SetCoordinatesForAddress(decimal latitude, decimal longitude, int addressId)
        {
            using var conn = new NpgsqlConnection(_config.ConnectionString);
            var sql = "CALL \"SetCoordinatesForAddress\"(@lat, @lng, @addressid)";
            var p = new DynamicParameters();

            p.Add("@lat", latitude);
            p.Add("@lng", longitude);
            p.Add("@addressid", addressId);
            var res = conn.Execute(sql, p);
        }
        public List<KentPoliceIncident> GetIncidentsForTypeAndDateRange(int incidentTypeId, DateTime? startDate, DateTime? endDate)
        {
            using var conn = new NpgsqlConnection(_config.ConnectionString);

            var sql = @"SELECT * FROM getIncidentsForTypeAndDateRange(@incidentType, @startDate, @endDate)";
            var p = new DynamicParameters();
            p.Add("@startDate", startDate);
            p.Add("@endDate", endDate);
            p.Add("@incidentType", incidentTypeId);
            var result = conn.Query<KentPoliceIncident>(sql, p).ToList();
            return result;
        }

        internal void UpdatePlaceNameForAddress(string placeName, int addressId)
        {
            using var conn = new NpgsqlConnection(_config.ConnectionString);

            var sql = @"UPDATE ""Addresses"" SET ""PlaceName"" = @placeName WHERE ""AddressId""=@addressId";
            var p = new DynamicParameters();
            p.Add("@placeName", placeName);
            p.Add("@addressId", addressId);
            conn.Execute(sql, p);
        }

        public PoliceIncidentDates GetIncidentDateInfo()
        {
            using var conn = new NpgsqlConnection(_config.ConnectionString);
            var sql = @"SELECT * FROM getminandmaxdateforpoliceincidents()";
            var p = new DynamicParameters();
            var result = conn.QuerySingle<PoliceIncidentDates>(sql, p);
            return result;
        }

        public List<PoliceIncidentModel> GetIncidentDescriptions()
        {
            using var conn = new NpgsqlConnection(_config.ConnectionString);
            var sql = @"SELECT * FROM getpoliceincidentdescriptions()";
            var p = new DynamicParameters();

            var result = conn.Query<PoliceIncidentModel>(sql, p).ToList();
            return result;
        }

        public List<AddressModel> GetAllAddressesWithoutPlaceNames()
        {
            using var conn = new NpgsqlConnection(_config.ConnectionString);
            var sql = @"
            SELECT 
                *, 
                st_x(""Coordinate"") as Latitude,
                st_y(""Coordinate"") as Longitude
            FROM 
                ""Addresses""
            WHERE 
                ""PlaceName"" = '' AND ""Coordinate"" IS NOT NULL";
            var p = new DynamicParameters();

            var result = conn.Query<AddressModel>(sql, p).ToList();
            return result;
        }

        public List<AddressModel> GetAllAddressesWithoutCoordinates()
        {
            using var conn = new NpgsqlConnection(_config.ConnectionString);
            var sql = @"SELECT * FROM getaddresseswihtoutcoordinates()";
            var p = new DynamicParameters();

            var result = conn.Query<AddressModel>(sql, p).ToList();
            return result;
        }

        public void InsertPoliceIncident(List<KentPoliceIncident> incidents, int incidentFileId)
        {
            using var conn = new NpgsqlConnection(_config.ConnectionString);
            var sql = @"CALL insertpoliceincident(@incidentdescription, @incidentdate, @incidentaddress, @crnumber, @incidentFileId)";

            foreach (var incident in incidents)
            {
                if (string.IsNullOrEmpty(incident.CrNumber)) continue;
                var p = new DynamicParameters();
                p.Add("@incidentdescription", incident.IncidentDescription);
                p.Add("@incidentdate", incident.IncidentDate);
                p.Add("@incidentaddress", incident.IncidentAddress);
                p.Add("@crnumber", incident.CrNumber);
                p.Add("@incidentFileId", incidentFileId);
                conn.Execute(sql, p);
            }
        }
        public void InsertCaseReport(List<KentPoliceCaseReport> incidents, int policeCaseReportFileId)
        {
            using var conn = new NpgsqlConnection(_config.ConnectionString);
            var sql = @"CALL insertpolicecasereport(@crnumber, @reporteddate, @criminaloffense, @streetname, @disposition, @policecasereportfileid)";

            foreach (var caseReport in incidents)
            {
                if (string.IsNullOrEmpty(caseReport.CrNumber)) continue;
                var p = new DynamicParameters();
                p.Add("@reporteddate", caseReport.ReportedDateTime);
                p.Add("@crnumber", caseReport.CrNumber);
                p.Add("@criminaloffense", caseReport.CriminalOffense);
                p.Add("@streetname", caseReport.StreetName);
                p.Add("@disposition", caseReport.Disposition);
                p.Add("@policecasereportfileid", policeCaseReportFileId);

                conn.Execute(sql, p);
            }
        }
        public void BulkInsertPoliceIncident(List<KentPoliceIncident> data)
        {
            try
            {
                using var conn = new NpgsqlConnection(_config.ConnectionString);
                conn.Open();
                var dataTable = new DataTable();
                dataTable.TableName = "KentPoliceIncident";

                dataTable.Columns.Add("KentPoliceIncidentId", typeof(int));
                dataTable.Columns.Add("IncidentDescription", typeof(string));
                dataTable.Columns.Add("IncidentDate", typeof(DateTime));
                dataTable.Columns.Add("IncidentAddress", typeof(string));
                dataTable.Columns.Add("CrNumber", typeof(string));
                foreach (var incident in data)
                {
                    var nameRow = dataTable.NewRow();

                    nameRow["IncidentDescription"] = incident.IncidentDescription;
                    nameRow["IncidentDate"] = incident.IncidentDate.HasValue ? incident.IncidentDate : DBNull.Value;
                    nameRow["CrNumber"] = incident.CrNumber;
                    nameRow["IncidentAddress"] = incident.IncidentAddress;
                    dataTable.Rows.Add(nameRow);
                }
                int colCount = dataTable.Columns.Count;

                NpgsqlDbType[] types = new NpgsqlDbType[colCount];
                int[] lengths = new int[colCount];
                string[] fieldNames = new string[colCount];


                using (var writer = conn.BeginBinaryImport($"COPY \"KentPoliceIncident\"(\"IncidentDescription\", \"IncidentDate\", \"IncidentAddress\", \"CrNumber\") FROM STDIN (FORMAT BINARY)"))
                {
                    for (int rowNumber = 0; rowNumber < dataTable.Rows.Count; rowNumber++)
                    {
                        DataRow dataRow = dataTable.Rows[rowNumber];
                        writer.StartRow();

                        for (int dataTableColumn = 0; dataTableColumn < colCount; dataTableColumn++)
                        {

                            switch (dataTableColumn)
                            {
                                case 1:
                                    writer.Write((string)dataRow[dataTableColumn], NpgsqlDbType.Text);
                                    break;
                                case 2:
                                    if (dataRow[dataTableColumn] == DBNull.Value)
                                        writer.WriteNull();
                                    else
                                        writer.Write(((DateTime)dataRow[dataTableColumn]).GetAsDateTimeKind(DateTimeKind.Utc), NpgsqlDbType.TimestampTz);
                                    break;
                                case 3:
                                    writer.Write((string)dataRow[dataTableColumn], NpgsqlDbType.Text);
                                    break;
                                case 4:
                                    writer.Write((string)dataRow[dataTableColumn], NpgsqlDbType.Text);
                                    break;
                                default:
                                    break;
                            }

                        }
                    }
                    writer.Complete();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error executing NpgSqlBulkCopy.WriteToServer().  See inner exception for details", ex);
            }


        }
    }
}
