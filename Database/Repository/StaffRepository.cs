using Appointments.Database.Context;
using Appointments.Database.Dto;
using Appointments.Database.Interfaces;
using Dapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Appointments.Database.Repository
{
    public class StaffRepository: IStaffRepository
    {
        private readonly DbContext _context;
        private readonly ILogger<StaffRepository> _logger;

        public StaffRepository(DbContext context, ILogger<StaffRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public bool DeleteNurse(string id, out string sMsg)
        {
            sMsg = string.Empty;
            try
            {
                using var connection = _context.CreateConnection();
                var query = "DELETE FROM tblStaff WHERE EmployeeID = @Id";
                connection.Execute(query, new { Id = id });
                sMsg = $"The Record is successfully deleted ";
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting nurse with Id {id}", id);
                sMsg = "ERROR: " + ex.Message + " " + (ex.InnerException == null ? "" : ex.InnerException.Message);
                return false;
            }
        }
        public bool SaveNurse(StaffDto staff, bool isNew, out string sMsg)
        {
            sMsg = string.Empty;
            var query = "";

            if (string.IsNullOrEmpty(staff.EmployeeID.ToString()))
            {
                staff.EmployeeID = GetNextID(out sMsg);
                if(sMsg.Trim().Length > 0)  return false;
            }
            if (isNew && CheckExistingID(staff.EmployeeID.ToString(), out sMsg))
            {
                sMsg = $"ERROR: The EmployeeID {staff.EmployeeID} already exists. Please choose a different ID.";
                return false;
            }
            else if(sMsg.Trim().Length > 0) return false;

            var parameters = new DynamicParameters();
            parameters.Add("EmployeeID", staff.EmployeeID);
            parameters.Add("LastName", staff.LastName);
            parameters.Add("FirstName", staff.FirstName);

            try
            {
                using var connection = _context.CreateConnection();
                if (!isNew)
                {
                    query = @"UPDATE [dbo].[tblStaff] SET [LastName] = @LastName ,[FirstName]= @FirstName
                              WHERE EmployeeID = @EmployeeID";
                }
                else
                {
                    query = @"INSERT INTO [dbo].[tblStaff] (EmployeeID, [LastName], [FirstName])
                              VALUES (@EmployeeID, @LastName, @FirstName)";
                }

                connection.Execute(query, parameters);
                sMsg = $"The Nurse record is successfully saved ";
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving nurse for {ID}", staff.EmployeeID);
                sMsg = "ERROR: " + ex.Message + " " + (ex.InnerException == null ? "" : ex.InnerException.Message);
                return false;
            }
        }
        public string GetNextID(out string sMsg)
        {
            sMsg = string.Empty;
            try
            {
                using var connection = _context.CreateConnection();
                var query = "SELECT ISNULL(MAX(CAST(replace(replace(ltrim(rtrim(EmployeeID)),'E',''),'M','') as int)), 0) + 1 FROM tblStaff where EmployeeID like 'E%' or EmployeeID like 'M%'";
                var sRet = connection.ExecuteScalar<string>(query);
                return "E" + sRet;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting next EmployeeID");
                sMsg = "ERROR: " + ex.Message + " " + (ex.InnerException == null ? "" : ex.InnerException.Message);
                return "1"; // Return 1 as a default if there's an error
            }
        }
        public bool CheckExistingID(string sID, out string sMsg)
        {
            sMsg = string.Empty;
            try
            {
                using var connection = _context.CreateConnection();
                var query = "SELECT COUNT(*)  FROM tblStaff WHERE replace(replace(ltrim(rtrim(EmployeeID)),'E',''),'M','')  = '" + sID.Replace("E","").Replace("M","") + "'" ;
                int count = connection.ExecuteScalar<int>(query);
                return count > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting existing EmployeeID");
                sMsg = "ERROR: " + ex.Message + " " + (ex.InnerException == null ? "" : ex.InnerException.Message);
                return false; // Return false as a default if there's an error
            }
        }
        public List<StaffDto> LoadNurses(out string sMsg)
        {
            sMsg = string.Empty;
            try
            {
                using var connection = _context.CreateConnection();
                var query = "SELECT [EmployeeID],[LastName],[FirstName] FROM tblStaff ORDER BY LastName";
                var nurses = connection.QueryAsync<StaffDto>(query);
                return nurses.Result.ToList();
            }
            catch (Exception ex)
            {
                sMsg = "ERROR: " + ex.Message + " " + (ex.InnerException == null ? "" : ex.InnerException.Message);
                _logger.LogError(ex, "Error loading nurses");
                return new List<StaffDto>();
            }
        }
        public StaffDto GetNurseById(int id, out string sMsg)
        {
            sMsg = string.Empty;
            try
            {
                using var connection = _context.CreateConnection();
                var query = "SELECT [EmployeeID] ,[LastName] ,[FirstName] FROM tblStaff WHERE EmployeeID = @Id";
                var nurse = connection.QuerySingleOrDefault<StaffDto>(query, new { Id = id });
 
                return nurse ?? new StaffDto();
            }
            catch (Exception ex)
            {
                sMsg = "ERROR: " + ex.Message + " " + (ex.InnerException == null ? "" : ex.InnerException.Message);
                _logger.LogError(ex, "Error loading nurse for ID {Id}", id);
                return new StaffDto();
            }

        }
    }
}
