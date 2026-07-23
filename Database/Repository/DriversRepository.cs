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
       public class DriversRepository : IDriversRepository
        {
            private readonly DbContext _context;
            private readonly ILogger<DriversRepository> _logger;

            public DriversRepository(DbContext context, ILogger<DriversRepository> logger)
            {
                _context = context;
                _logger = logger;
            }

            public bool DeleteDriver(int id, out string sMsg)
            {
                sMsg = string.Empty;
                try
                {
                    using var connection = _context.CreateConnection();
                    var query = "DELETE FROM tblDrivers WHERE DriverID = @Id";
                    connection.Execute(query, new { Id = id });
                    sMsg = $"The Record is successfully deleted ";
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deleting driver with Id {id}", id);
                    sMsg = "ERROR: " + ex.Message + " " + (ex.InnerException == null ? "" : ex.InnerException.Message);
                    return false;
                }
            }
            public bool SaveDriver(DriversDto driver, bool isNew, out string sMsg)
            {
                sMsg = string.Empty;
                var query = "";

                //if (isNew && CheckExistingID(driver.DriverID.ToString(), out sMsg))
                //{
                //    sMsg = $"ERROR: The DriverID {driver.DriverID} already exists. Please choose a different ID.";
                //    return false;
                //}
                //else if (sMsg.Trim().Length > 0) return false;

                var parameters = new DynamicParameters();
                parameters.Add("DriverID", driver.DriverID);
                parameters.Add("Last", driver.Last);
                parameters.Add("First", driver.First);
                parameters.Add("Title", driver.Title);
                parameters.Add("Priority", driver.Priority);    
                parameters.Add("InActive", driver.InActive);

            try
                {
                    using var connection = _context.CreateConnection();
                    if (!isNew)
                    {
                        query = @"UPDATE [dbo].[tblDrivers] SET [Last] = @Last ,[First]= @First, [Title]= @Title, [Priority]= @Priority, [InActive]= @InActive
                              WHERE DriverID = @DriverID";
                    }
                    else
                    {
                        query = @"INSERT INTO [dbo].[tblDrivers] (DriverID, [Last], [First], [Title], [Priority], [InActive])
                              VALUES ((select max(DriverID) from [dbo].[tblDrivers]) + 1, @Last, @First, @Title, @Priority, @InActive)";
                    }

                    connection.Execute(query, parameters);
                    sMsg = $"The Driver record is successfully saved ";
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving driver for {ID}", driver.DriverID);
                    sMsg = "ERROR: " + ex.Message + " " + (ex.InnerException == null ? "" : ex.InnerException.Message);
                    return false;
                }
            }
            //public string GetNextID(out string sMsg)
            //{
            //    sMsg = string.Empty;
            //    try
            //    {
            //        using var connection = _context.CreateConnection();
            //        var query = "SELECT ISNULL(MAX(CAST(replace(replace(ltrim(rtrim(EmployeeID)),'E',''),'M','') as int)), 0) + 1 FROM tblStaff where EmployeeID like 'E%' or EmployeeID like 'M%'";
            //        var sRet = connection.ExecuteScalar<string>(query);
            //        return "E" + sRet;
            //    }
            //    catch (Exception ex)
            //    {
            //        _logger.LogError(ex, "Error getting next EmployeeID");
            //        sMsg = "ERROR: " + ex.Message + " " + (ex.InnerException == null ? "" : ex.InnerException.Message);
            //        return "1"; // Return 1 as a default if there's an error
            //    }
            //}
            //public bool CheckExistingID(string sID, out string sMsg)
            //{
            //    sMsg = string.Empty;
            //    try
            //    {
            //        using var connection = _context.CreateConnection();
            //        var query = "SELECT COUNT(*)  FROM tblStaff WHERE replace(replace(ltrim(rtrim(EmployeeID)),'E',''),'M','')  = '" + sID.Replace("E", "").Replace("M", "") + "'";
            //        int count = connection.ExecuteScalar<int>(query);
            //        return count > 0;
            //    }
            //    catch (Exception ex)
            //    {
            //        _logger.LogError(ex, "Error getting existing EmployeeID");
            //        sMsg = "ERROR: " + ex.Message + " " + (ex.InnerException == null ? "" : ex.InnerException.Message);
            //        return false; // Return false as a default if there's an error
            //    }
            //}
            public List<DriversDto> LoadDrivers(int iActive, out string sMsg)
            {
            string sWhereClause = string.Empty;
            switch (iActive)
            {
                case 1: // Active
                    sWhereClause = " WHERE [InActive] = 0 ";
                    break;
                case 2: // Inactive
                    sWhereClause = " WHERE [InActive] = 1 ";
                    break;
                default: // All
                    sWhereClause = string.Empty;
                    break;
            }
            sMsg = string.Empty;
                try
                {
                    using var connection = _context.CreateConnection();
                    var query = "SELECT [DriverID],[Last],[First],[Title],[Priority],[InActive] FROM tblDrivers " + sWhereClause + " ORDER BY [Last]";
                    var drivers = connection.QueryAsync<DriversDto>(query);
                    return drivers.Result.ToList();
                }
                catch (Exception ex)
                {
                    sMsg = "ERROR: " + ex.Message + " " + (ex.InnerException == null ? "" : ex.InnerException.Message);
                    _logger.LogError(ex, "Error loading drivers");
                    return new List<DriversDto>();
                }
            }
            public DriversDto GetDriverById(int id, out string sMsg)
            {
                sMsg = string.Empty;
                try
                {
                    using var connection = _context.CreateConnection();
                    var query = "SELECT [DriverID],[Last],[First],[Title],[Priority],[InActive] FROM tblDrivers WHERE DriverID = @Id";
                    var driver = connection.QuerySingleOrDefault<DriversDto>(query, new { Id = id });

                    return driver ?? new DriversDto();
                }
                catch (Exception ex)
                {
                    sMsg = "ERROR: " + ex.Message + " " + (ex.InnerException == null ? "" : ex.InnerException.Message);
                    _logger.LogError(ex, "Error loading driver for ID {Id}", id);
                    return new DriversDto();
                }

            }
        }
    }
