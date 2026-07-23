using Appointments.Database.Context;
using Appointments.Database.Dto;
using Appointments.Database.Interfaces;
using Dapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace Appointments.Database.Repository
{
    public class DoctorsRepository: IDoctorsRepository
    {
        private readonly DbContext _context;
        private readonly ILogger<DoctorsRepository> _logger;

        public DoctorsRepository(DbContext context, ILogger<DoctorsRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public bool DeleteDoctor(int id, out string sMsg)
        {
            sMsg = string.Empty;
            try
            {
                using var connection = _context.CreateConnection();
                //var queryLocations = "DELETE FROM tblLocations WHERE DoctorKey = @Id";
                //connection.Execute(queryLocations, new { Id = id });
                //var query = "DELETE FROM tblDoctors WHERE Id = @Id";
                var query = "UPDATE tblDoctors SET IsDeleted = 1 WHERE DoctorKey = @Id";
                connection.Execute(query, new { Id = id });
                sMsg = $"The Doctor is successfully deleted ";
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting appointment with Id {id}", id);
                sMsg = "ERROR: " + ex.Message + " " + (ex.InnerException == null ? "" : ex.InnerException.Message);
                return false;
            }
        }
        public bool SaveDoctor(DoctorsDto doctor, out string sMsg)
        {
            sMsg = string.Empty;
            var query = "";
            var parameters = new DynamicParameters();
            parameters.Add("DoctorKey", doctor.DoctorKey);
            parameters.Add("Last", doctor.Last);
            parameters.Add("First", doctor.First);
            parameters.Add("Company", doctor.Company);
            parameters.Add("Title", doctor.Title);
            parameters.Add("Specialty", doctor.Specialty);
            if (doctor.DoctorKey <= 0) parameters.Add("AddDate", doctor.AddDate);
            parameters.Add("ModDate", doctor.ModDate);
            if (doctor.DoctorKey <= 0) parameters.Add("CreatedBy", doctor.CreatedBy);
            parameters.Add("ModifiedBy", doctor.ModifiedBy);
            parameters.Add("IsDeleted", doctor.IsDeleted);

            try
            {
                using var connection = _context.CreateConnection();
                if (doctor.DoctorKey > 0)
                {
                    query = @"UPDATE [dbo].[tblDoctors] SET [Last] = @Last ,[First]= @First ,[Company]= @Company ,[Title]= @Title,[Specialty]= @Specialty,
[ModDate]= @ModDate,[ModifiedBy]= @ModifiedBy, [IsDeleted]=@IsDeleted
                              WHERE DoctorKey = @DoctorKey";
                    connection.Execute(query, parameters);
                }
                else
                {
                    query = @"INSERT INTO [dbo].[tblDoctors] (DoctorKey, [Last], [First], [Company], [Title], [Specialty], [AddDate], [ModDate], [CreatedBy], [ModifiedBy])
                              VALUES ((select max(DoctorKey)+1 from [dbo].[tblDoctors]), @Last, @First, @Company, @Title, @Specialty, @AddDate, @ModDate, @CreatedBy, @ModifiedBy)";
                    connection.Execute(query, parameters);
                        var newDoctor = connection.QuerySingle<DoctorsDto>("SELECT TOP 1 * FROM [dbo].[tblDoctors] ORDER BY DoctorKey DESC");
                    doctor.DoctorKey = newDoctor.DoctorKey;
                }
                foreach (var location in doctor.Locations)
                {
                    location.DoctorKey = doctor.DoctorKey;
                    SaveDoctorLocation(location, out string locationMsg);
                    if (!string.IsNullOrEmpty(locationMsg))
                    {
                        _logger.LogError("Error saving doctor location for DoctorKey {DoctorKey}: {Message}", doctor.DoctorKey, locationMsg);
                        sMsg += $"Error saving location for DoctorKey {doctor.DoctorKey}: {locationMsg} ";
                    }
                }
                sMsg = $"The Doctor is successfully saved ";
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving doctor for {ID}", doctor.DoctorKey);
                sMsg = "ERROR: " + ex.Message + " " + (ex.InnerException == null ? "" : ex.InnerException.Message);
                return false;
            }
        }

        private void SaveDoctorLocation(LocationDto location, out string locationMsg)
        {
            locationMsg = string.Empty;
            var query = "";
            var parameters = new DynamicParameters();
            parameters.Add("DoctorKey", location.DoctorKey);
            parameters.Add("LocationID", location.LocationID);
            parameters.Add("Street", location.Street);
            parameters.Add("City", location.City);
            parameters.Add("State", location.State);
            parameters.Add("Zip", location.Zip);
            parameters.Add("Phone", location.Phone);
            parameters.Add("Fax", location.Fax);
            parameters.Add("FMC", location.FMC);
            parameters.Add("AdmittingFacility", location.AdmittingFacility);
            parameters.Add("InHouse", location.InHouse);

            try
            {
                using var connection = _context.CreateConnection();
                if (location.LocationID > 0)
                {
                    query = @"UPDATE [dbo].[tblLocations] SET [Street] = @Street ,[City]= @City ,[State]= @State ,[Zip]= @Zip,[Phone]= @Phone,[Fax]= @Fax,[FMC]= @FMC,[AdmittingFacility]= @AdmittingFacility,[InHouse]= @InHouse
                              WHERE LocationID = @LocationID";
                    connection.Execute(query, parameters);
                }
                else if (location.LocationID == 0)
                {
                    query = @"INSERT INTO [dbo].[tblLocations] (LocationID, DoctorKey, [Street], [City], [State], [Zip], [Phone], [Fax], [FMC], [AdmittingFacility], [InHouse])
                              VALUES ((select max(LocationID)+1 from [dbo].[tblLocations]),@DoctorKey, @Street, @City, @State, @Zip, @Phone, @Fax, @FMC, @AdmittingFacility, @InHouse)"; 
                    connection.Execute(query, parameters);
                    var newLocation = connection.QuerySingle<LocationDto>("SELECT TOP 1 * FROM [dbo].[tblLocations] ORDER BY LocationID DESC");
                    location.LocationID = newLocation.LocationID;
                }
                else
                {
                    int iLocationId = Math.Abs(location.LocationID);
                    query = "DELETE FROM tblLocations WHERE LocationID = @LocationID";
                    connection.Execute(query, new { LocationID = iLocationId });
                }
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving location for {ID}", location.LocationID);
                locationMsg = "ERROR: " + ex.Message + " " + (ex.InnerException == null ? "" : ex.InnerException.Message);
            }
        }

        //public List<DoctorsDto> LoadDoctors(bool bActive, out string sMsg)
        //{
        //    sMsg = string.Empty;
        //    try
        //    {
        //        using var connection = _context.CreateConnection();
        //        var query = "SELECT [DoctorKey],[Last],[First],[Company],[Title],[Specialty],[IsDeleted] FROM qryDoctors " + (bActive ? " WHERE IsDeleted = 0 " : "") + " ORDER BY Last;";
        //        var doctors = connection.QueryAsync<DoctorsDto>(query, new { IsDeleted = !bActive });
        //        return doctors.Result.ToList();
        //    }
        //    catch (Exception ex)
        //    {
        //        sMsg = "ERROR: " + ex.Message + " " + (ex.InnerException == null ? "" : ex.InnerException.Message);
        //        _logger.LogError(ex, "Error loading doctors");
        //        return new List<DoctorsDto>();
        //    }
        //}
        public List<DoctorsDto> LoadDoctors(int iActive, out string sMsg)
        {
            sMsg = string.Empty;
            string sWhereClause = string.Empty;
            switch (iActive)
            {
                case 1: // Active
                    sWhereClause = " WHERE IsDeleted = 0 ";
                    break;
                case 2: // Inactive
                    sWhereClause = " WHERE IsDeleted = 1 ";
                    break;
                default: // All
                    sWhereClause = string.Empty;
                    break;
            }
            try
            {
                using var connection = _context.CreateConnection();
                var query = "SELECT [DoctorKey],[Last],[First],[Company],[Title],[Specialty],[IsDeleted] FROM qryDoctors " + sWhereClause + " ORDER BY Last;";
                var doctors = connection.QueryAsync<DoctorsDto>(query);
                return doctors.Result.ToList();
            }
            catch (Exception ex)
            {
                sMsg = "ERROR: " + ex.Message + " " + (ex.InnerException == null ? "" : ex.InnerException.Message);
                _logger.LogError(ex, "Error loading doctors");
                return new List<DoctorsDto>();
            }
        }
        public List<LocationDto> GetDoctorLocations(int id, out string sMsg)
        {
            sMsg = string.Empty;
            try
            {
                using var connection = _context.CreateConnection();
                var query = @"SELECT LocationID,Street,City,State,Zip,Phone,Fax,FMC,AdmittingFacility,InHouse from tblLocations where DoctorKey = @Id";
                Task<IEnumerable<LocationDto>> locations = connection.QueryAsync<LocationDto>(query, new { Id = id });
                return locations.Result.ToList();
            }
            catch (Exception ex)
            {
                sMsg = "ERROR: " + ex.Message + " " + (ex.InnerException == null ? "" : ex.InnerException.Message);
                _logger.LogError(ex, "Error loading doctor locations with ID {Id}", id);
                return new List<LocationDto>();
            }
        }
         public LocationDto GetLocationById(int id, out string sMsg)
        {
            sMsg = string.Empty;
            try
            {
                using var connection = _context.CreateConnection();
                var query = @"SELECT LocationID,Street,City,State,Zip,Phone,Fax,FMC,AdmittingFacility,InHouse from tblLocations where LocationID = @Id";
                var location = connection.QuerySingleOrDefault<LocationDto>(query, new { Id = id });
                return location ?? new LocationDto();
            }
            catch (Exception ex)
            {
                sMsg = "ERROR: " + ex.Message + " " + (ex.InnerException == null ? "" : ex.InnerException.Message);
                _logger.LogError(ex, "Error loading location with ID {Id}", id);
                return new LocationDto();
            }
        }
        public DoctorsDto GetDoctorById(int id, out string sMsg)
        {
            sMsg = string.Empty;
            try
            {
                using var connection = _context.CreateConnection();
                var query = "SELECT [DoctorKey] ,[Last] ,[First] ,[Company] ,[Title] ,[Specialty],[AddDate],[CreatedBy],[ModDate],[ModifiedBy],[IsDeleted] FROM qryDoctors WHERE DoctorKey = @Id";
                var doctor = connection.QuerySingleOrDefault<DoctorsDto>(query, new { Id = id });
                if (doctor != null)
                {
                    doctor.Locations = GetDoctorLocations(doctor.DoctorKey, out sMsg);
                }
                return doctor ?? new DoctorsDto();
            }
            catch (Exception ex)
            {
                sMsg = "ERROR: " + ex.Message + " " + (ex.InnerException == null ? "" : ex.InnerException.Message);
                _logger.LogError(ex, "Error loading doctor for ID {Id}", id);
                return new DoctorsDto();
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
