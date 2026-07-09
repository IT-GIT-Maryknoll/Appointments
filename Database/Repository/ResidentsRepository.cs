using Appointments.Database.Context;
using Appointments.Database.Dto;
using Appointments.Database.Interfaces;
using Dapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Appointments.Database.Repository
{
    public class ResidentsRepository : IResidentsRepository
    {
        private readonly DbContext _context;
        private readonly ILogger<ResidentsRepository> _logger;

        public ResidentsRepository(DbContext context, ILogger<ResidentsRepository> logger)
        {
            _context = context;
            _logger = logger;
        }


        public List<ResidentsDto> LoadResidents(out string sMsg)
        {
            sMsg = string.Empty;
            try
            {
                using var connection = _context.CreateConnection();
                var query = @"SELECT qryResidents_tblResidentsdata.ID, qryResidents_tblResidentsdata.[Perm#] Perm, qryResidents_tblResidentsdata.Res_Title, qryResidents_tblResidentsdata.Name, qryResidents_tblResidentsdata.Ext, qryResidents_tblResidentsdata.Room, qryResidents_tblResidentsdata.ResidentNotes
FROM qryResidents_tblResidentsdata
ORDER BY qryResidents_tblResidentsdata.Name";
                List<ResidentsDto> residents = connection.Query<ResidentsDto>(query).AsList();
                return residents;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving residents");
                sMsg = "ERROR: " + ex.Message + " " + (ex.InnerException == null ? "" : ex.InnerException.Message);
                return new List<ResidentsDto>();
            }
        }
        public ResidentsDto LoadResidentById(int id, out string sMsg)
        {
            sMsg = string.Empty;
            try
            {
                using var connection = _context.CreateConnection();
                var query = @"SELECT qryResidents_tblResidentsdata.ID, qryResidents_tblResidentsdata.[Perm#], qryResidents_tblResidentsdata.Name, qryResidents_tblResidentsdata.Ext, qryResidents_tblResidentsdata.Room, qryResidents_tblResidentsdata.ResidentNotes,
    qryResidents_tblResidentsdata.Country, qryResidents_tblResidentsdata.DOB, qryResidents_tblResidentsdata.OnStaff, qryResidents_tblResidentsdata.DateAssigned,  qryResidents_tblResidentsdata.Location, qryResidents_tblResidentsdata.Status, qryResidents_tblResidentsdata.Res_Title, qryResidents_tblResidentsdata.Res_LastName, qryResidents_tblResidentsdata.Res_Firstname, qryResidents_tblResidentsdata.Res_MiddleInt
FROM qryResidents_tblResidentsdata
WHERE qryResidents_tblResidentsdata.ID = @ID";
                ResidentsDto resident = connection.QuerySingleOrDefault<ResidentsDto>(query, new { ID = id });
                return resident;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving resident with ID {ID}", id);
                sMsg = "ERROR: " + ex.Message + " " + (ex.InnerException == null ? "" : ex.InnerException.Message);
                return null;
            }
        }
        public bool SaveResident(ResidentsDto resident, out string sMsg)
        {
            sMsg = string.Empty;
            var query = "";
            var parameters = new DynamicParameters();
            parameters.Add("ID", resident.ID);
            parameters.Add("Room", resident.Room);
            parameters.Add("Country", resident.Country);
            parameters.Add("DOB", resident.DOB);
            parameters.Add("DateAssigned", resident.DateAssigned);
            parameters.Add("ResidentNotes", resident.ResidentNotes);
            parameters.Add("Location", resident.Location);
            parameters.Add("Status", resident.Status);
            parameters.Add("Res_Title", resident.Res_Title);
            parameters.Add("Res_LastName", resident.Res_LastName);
            parameters.Add("Res_FirstName", resident.Res_FirstName);
            parameters.Add("Res_MiddleInt", resident.Res_MiddleInt);
            parameters.Add("Ext", resident.Ext);
            parameters.Add("OnStaff", resident.OnStaff);

            try
            {
                using var connection = _context.CreateConnection();
                if (resident.ID > 0)
                {
                    query = @"UPDATE [dbo].[qryResidents_tblResidentsdata] SET [Room] = @Room, [Country] = @Country, [DOB] = @DOB, [DateAssigned] = @DateAssigned, 
[ResidentNotes] = @ResidentNotes, [Location] = @Location, [Status] = @Status, [Res_Title] = @Res_Title, 
[Res_LastName] = @Res_LastName, [Res_FirstName] = @Res_FirstName, [Res_MiddleInt] = @Res_MiddleInt, [Ext] = @Ext, [OnStaff] = @OnStaff
                              WHERE ID = @ID";
                    connection.Execute(query, parameters);
                }
                else
                {
                    query = @"INSERT INTO [dbo].[qryResidents_tblResidentsdata] ((select max(ID)+1 from [dbo].[tblResidents]), [Room], [Country], [DOB], [DateAssigned], [ResidentNotes], [Location], [Status], 
[Res_Title], [Res_LastName], [Res_Firstname], [Res_MiddleInt], [Ext], [OnStaff])
                              VALUES ((select max(ResidentKey)+1 from [dbo].[tblResidents]), @Room, @Country, @DOB, @DateAssigned, @ResidentNotes, @Location, @Status, 
@Res_Title, @Res_LastName, @Res_Firstname, @Res_MiddleInt, @Ext, @OnStaff)";
                    connection.Execute(query, parameters);
                    var newResident = connection.QuerySingle<ResidentsDto>("SELECT TOP 1 * FROM [dbo].[tblResidents] ORDER BY ResidentKey DESC");
                    resident.ResidentKey = newResident.ResidentKey;
                }

                sMsg = $"The Resident is successfully saved ";
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving resident for {ID}", resident.ResidentKey);
                sMsg = "ERROR: " + ex.Message + " " + (ex.InnerException == null ? "" : ex.InnerException.Message);
                return false;
            }
        }
        public bool DeleteResident(int id, out string sMsg)
        {
            sMsg = string.Empty;
            var query = @"DELETE FROM [dbo].[qryResidents_tblResidentsdata] WHERE ID = @ID";
            try
            {
                using var connection = _context.CreateConnection();
                connection.Execute(query, new { ID = id });
                sMsg = $"The Resident with ID {id} is successfully deleted ";
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting resident with ID {ID}", id);
                sMsg = "ERROR: " + ex.Message + " " + (ex.InnerException == null ? "" : ex.InnerException.Message);
                return false;
            }
        }
        public List<StatusDto> LoadStatuses(out string sMsg)
        {
            sMsg = string.Empty;
            try
            {
                using var connection = _context.CreateConnection();
                var query = @"SELECT ID, StatusName FROM [MKL_MINT].[dbo].[tblSTTResidentStatus]";
                List<StatusDto> statuses = connection.Query<StatusDto>(query).AsList();
                return statuses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving resident statuses");
                sMsg = "ERROR: " + ex.Message + " " + (ex.InnerException == null ? "" : ex.InnerException.Message);
                return new List<StatusDto>();
            }
        }
        public List<string> LoadCountries(out string sMsg)
        {
            sMsg = string.Empty;
            try
            {
                using var connection = _context.CreateConnection();
                var query = @"SELECT DISTINCT qryResidents_tblResidentsdata.Country FROM qryResidents_tblResidentsdata 
WHERE qryResidents_tblResidentsdata.Country IS NOT NULL ORDER BY qryResidents_tblResidentsdata.Country";
                List<string> countries = connection.Query<string>(query).AsList();
                return countries;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving countries");
                sMsg = "ERROR: " + ex.Message + " " + (ex.InnerException == null ? "" : ex.InnerException.Message);
                return new List<string>();
            }
        }
        public bool SaveAudit(AuditDto audit, out string sMsg)
        {
            sMsg = string.Empty;
            try
            {
                using var connection = _context.CreateConnection();
                var query = @"INSERT INTO [dbo].[Appointments_audit] ([datetime],[ip],[user],[table],[action],[description])
                              VALUES (@Datetime, @Ip, @User, @Table, @Action, @Description)";
                var parameters = new DynamicParameters();
                parameters.Add("ip", audit.Ip);
                parameters.Add("datetime", audit.Datetime);
                parameters.Add("user", audit.User);
                parameters.Add("table", audit.Table);
                parameters.Add("action", audit.Action);
                parameters.Add("description", audit.Description);
                connection.Execute(query, parameters);
                return true;
            }
            catch (Exception ex)
            {
                sMsg = "ERROR: " + ex.Message + " " + (ex.InnerException == null ? "" : ex.InnerException.Message);
                _logger.LogError(ex, "Error saving audit for appointment description {Description}", audit.Description);
                return false;
            }
        }

    }
}
