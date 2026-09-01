using Appointments.Database.Context;
using Appointments.Database.Dto;
using Appointments.Database.Interfaces;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Appointments.Database.Repository
{
    public class AppointmentsRepository : IAppointmentsRepository
    {
        private readonly DbContext _context;
        private readonly ILogger<AppointmentsRepository> _logger;

        public AppointmentsRepository(DbContext context, ILogger<AppointmentsRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public bool DeleteAppointment(int id, out string sMsg)
        {
            sMsg = string.Empty;
            try
            {
                using var connection = _context.CreateConnection();
                var query = "DELETE FROM tblAppointments WHERE Id = @Id";
                connection.Execute(query, new { Id = id });
                sMsg = $"The Appointment is successfully deleted ";
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting appointment with Id {id}", id);
                sMsg = "ERROR: " + ex.Message + " " + (ex.InnerException == null ? "" : ex.InnerException.Message);
                return false;
            }
        }
        public bool SaveAppointment(AppointmentDto appointment, out string sMsg)
        {
            sMsg = string.Empty;
            var query = "";
            var parameters = new DynamicParameters();
            parameters.Add("ID", appointment.ID);
            parameters.Add("ResidentKey", appointment.ResidentKey);
            parameters.Add("DoctorKey", appointment.DoctorKey);
            parameters.Add("DriverKey", appointment.DriverKey);
            parameters.Add("Status", appointment.Status);
            parameters.Add("PrepID", appointment.PrepID);
            parameters.Add("Notes", appointment.Notes);
            parameters.Add("LocationID", appointment.LocationID);
            parameters.Add("ApptTime", appointment.ApptTime);
            parameters.Add("Depart", appointment.Depart);
            parameters.Add("AddDate", appointment.AddDate);
            parameters.Add("ModDate", appointment.ModDate);
            parameters.Add("CreatedBy", appointment.CreatedBy);
            parameters.Add("ModifiedBy", appointment.ModifiedBy);
            parameters.Add("ApptType", appointment.ApptType);
            parameters.Add("InHouseVisit", appointment.InHouseVisit);
            parameters.Add("NursesAideAccompaniment", appointment.NursesAideAccompaniment);
            parameters.Add("MakeAppointment", appointment.MakeAppointment);
            parameters.Add("ConfirmedAppointment", appointment.ConfirmedAppointment);
            parameters.Add("CarNum", appointment.CarNum);
            parameters.Add("Wait", appointment.Wait);

            try
            {
                using var connection = _context.CreateConnection();
                if (appointment.ID > 0)
                {
                    query = @"UPDATE [dbo].[tblAppointments] SET [ApptTime] = @ApptTime, [DoctorKey] = @DoctorKey, [DriverKey] = @DriverKey, [Notes] = @Notes, [Depart] = @Depart, 
[ResidentKey] = @ResidentKey, [ModifiedBy] = @ModifiedBy, [ModDate] = @ModDate, [Status] = @Status, [PrepID] = @PrepID, [LocationID] = @LocationID, [ApptType] = @ApptType, 
[InHouseVisit] = @InHouseVisit, [NursesAideAccompaniment] = @NursesAideAccompaniment, [MakeAppointment]=@MakeAppointment, [ConfirmedAppointment]=@ConfirmedAppointment, [CarNum]=@CarNum, [Wait]=@Wait
                              WHERE ID = @ID";
                }
                else
                {
                    query = @"INSERT INTO [dbo].[tblAppointments] (ID, [ApptTime], [DoctorKey], [DriverKey], [Notes], [Depart], [ResidentKey], [CreatedBy], [ModifiedBy], [AddDate], [ModDate], 
[Status], [PrepID], [LocationID], [ApptType], [InHouseVisit], [NursesAideAccompaniment], [MakeAppointment], [ConfirmedAppointment], [CarNum], [Wait])
                              VALUES ((select max(ID)+1 from [dbo].[tblAppointments]), @ApptTime, @DoctorKey, @DriverKey, @Notes, @Depart, @ResidentKey, @CreatedBy, @ModifiedBy, @AddDate, @ModDate, 
@Status, @PrepID, @LocationID, @ApptType, @InHouseVisit, @NursesAideAccompaniment, @MakeAppointment, @ConfirmedAppointment, @CarNum, @Wait)";
                }
                connection.Execute(query, parameters);
                sMsg = $"The Appointment is successfully saved ";
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving appointment for {ID}", appointment.ID);
                sMsg = "ERROR: " + ex.Message + " " + (ex.InnerException == null ? "" : ex.InnerException.Message);
                return false;
            }
        }
        public List<AppointmentTypeDto> LoadAppointmentTypes(out string sMsg)
        {
            sMsg = string.Empty;
            try
            {
                using var connection = _context.CreateConnection();
                var query = "SELECT DISTINCT tblApptType.ID, tblApptType.Type FROM tblApptType;";
                var appointmentTypes = connection.QueryAsync<AppointmentTypeDto>(query);
                return appointmentTypes.Result.ToList();
            }
            catch (Exception ex)
            {
                sMsg = "ERROR: " + ex.Message + " " + (ex.InnerException == null ? "" : ex.InnerException.Message);
                _logger.LogError(ex, "Error loading appointment types");
                return new List<AppointmentTypeDto>();
            }
        }
        public List<DoctorsDto> LoadDoctors(out string sMsg)
        {
            sMsg = string.Empty;
            try
            {
                using var connection = _context.CreateConnection();
                var query = "SELECT DoctorKey,  DoctorName, Last FROM qryDoctors WHERE IsDeleted <> 1 AND DoctorName is not null AND LEN(LTRIM(RTRIM(DoctorName))) > 0 ORDER BY Last;";
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
        public List<ResidentsDto> LoadResidents(out string sMsg)
        {
            sMsg = string.Empty;
            try
            {
                using var connection = _context.CreateConnection();
                var query = "SELECT ResidentKey,  FullName FROM qryAppointmentNameListChina WHERE FullName is not null AND LEN(LTRIM(RTRIM(FullName))) > 0 ORDER BY FullName;";
                var residents = connection.QueryAsync<ResidentsDto>(query);
                return residents.Result.ToList();
            }
            catch (Exception ex)
            {
                sMsg = "ERROR: " + ex.Message + " " + (ex.InnerException == null ? "" : ex.InnerException.Message);
                _logger.LogError(ex, "Error loading residents");
                return new List<ResidentsDto>();
            }
        }
        public List<AppointmentDto> LoadAppointments(string sFilter, out string sMsg)
        {
            sMsg = string.Empty;
            try
            {
                using var connection = _context.CreateConnection();
                var query = @"SELECT [ID]
                    ,CAST([ApptTime] AS DATE) ApptTime
                    ,[ApptTime] ApptTm
                    ,[FullName]
 --                   ,[Room]
                    ,[DoctorKey]
                    ,[DoctorName]
                    ,[Specialty]
                    ,[DriverKey]
                    ,[DriverName]
                    ,[CarNum]
                    ,[Car_Info]
                    ,[Street]
                    ,[City]
                    ,[Phone]
                    ,[Fax]
                    ,[Notes]
                    ,[Depart]
                    ,[ResidentKey]
                    ,[MakeAppointment]
                    ,[ConfirmedAppointment]
                    ,[Nurse_Name]
                    ,[CreatedBy]
                    ,[ModifiedBy]
                    ,[AddDate]
                    ,[ModDate]
                    ,[Status]
                    ,[PrepID]
                    ,[InHouseVisit]
                    ,[NursesAideAccompaniment]
                    ,[ApptType]
                    FROM [dbo].[qryAppointmentsChina]"; 
                if (sFilter is not null && sFilter.Trim().Length > 0) query += " WHERE " + sFilter;
                query += " ORDER BY CAST([ApptTime] AS DATE) ASC, CAST([ApptTime] AS TIME) ASC, FullName,DoctorName";
                Task<IEnumerable<AppointmentDto>> appointments = connection.QueryAsync<AppointmentDto>(query);
                return appointments.Result.ToList();
            }
            catch (Exception ex)
            {
                sMsg = "ERROR: " + ex.Message + " " + (ex.InnerException == null ? "" : ex.InnerException.Message);
                _logger.LogError(ex, "Error loading appointments with filter {Filter}", sFilter);
                return new List<AppointmentDto>();
            }
        }
        public List<DriversDto> LoadDrivers(out string sMsg)
        {
            sMsg = string.Empty;
            try
            {
                using var connection = _context.CreateConnection();
                var query = "SELECT DriverID, DriverName FROM qryDrivers WHERE InActive = 0 ORDER BY DriverName;";
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
        public List<CarDto> LoadCars(out string sMsg)
        {
            sMsg = string.Empty;
            try
            {
                using var connection = _context.CreateConnection();
                var query = "SELECT CarNum, Car_Make + ' ' + Car_Model AS CarName FROM tblCars ORDER BY CarName;";
                var cars = connection.QueryAsync<CarDto>(query);
                return cars.Result.ToList();
            }
            catch (Exception ex)
            {
                sMsg = "ERROR: " + ex.Message + " " + (ex.InnerException == null ? "" : ex.InnerException.Message);
                _logger.LogError(ex, "Error loading cars");
                return new List<CarDto>();
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
        public AppointmentDto GetAppointmentById(int id, out string sMsg)
        {
            sMsg = string.Empty;
            try
            {
                using (var connection = _context.CreateConnection())
                {

                    var query = @"SELECT ID, ApptTime, ApptType, DoctorKey, DoctorName, Specialty, LocationID, DriverKey, DriverName,CarNum,Street, City,Phone,NursesAideAccompaniment, --Car_Info,
                                    Fax, Notes, Depart, ResidentKey, MakeAppointment, ConfirmedAppointment, Nurse_Name, CreatedBy, ModifiedBy,AddDate,ModDate,Status, PrepID, Car_Model, FullName, Wait,
                                    InHouseVisit
                    FROM [dbo].[qryAppointments_UPD] WHERE ID = @Id";
                    var appointment = connection.QuerySingleOrDefault<AppointmentDto>(query, new { Id = id });
                    if (appointment != null)
                    {
                        appointment.Locations = GetDoctorLocations(appointment.DoctorKey, out sMsg);
                    }
                    return appointment;
                }
            }
            catch (Exception ex)
            {
                sMsg = "ERROR: " + ex.Message + " " + (ex.InnerException == null ? "" : ex.InnerException.Message);
                _logger.LogError(ex, "Error loading appointment with ID {Id}", id);
                return new AppointmentDto();
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
                var query = "SELECT DoctorKey,  DoctorName, Last, Specialty FROM qryDoctors WHERE DoctorKey = @Id";
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
        public List<PrepNamesDto> LoadPreps(out string sMsg)
        {
            sMsg = string.Empty;
            try
            {
                using var connection = _context.CreateConnection();
                var query = "SELECT PrepID, PrepName FROM tblPrepNames ORDER BY PrepName;";
                var preps = connection.QueryAsync<PrepNamesDto>(query);
                return preps.Result.ToList();
            }
            catch (Exception ex)
            {
                sMsg = "ERROR: " + ex.Message + " " + (ex.InnerException == null ? "" : ex.InnerException.Message);
                _logger.LogError(ex, "Error loading preps");
                return new List<PrepNamesDto>();
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

  