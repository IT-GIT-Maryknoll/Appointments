using Appointments.Database.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Appointments.Database.Interfaces
{
    public interface IAppointmentsRepository
    {
        bool DeleteAppointment(int id, out string sMsg);
        List<AppointmentTypeDto> LoadAppointmentTypes(out string sMsg);
        List<DoctorsDto> LoadDoctors(out string sMsg);
        List<ResidentsDto> LoadResidents(out string sMsg);
        List<AppointmentDto> LoadAppointments(string sFilter, out string sMsg);
        List<DriversDto> LoadDrivers(out string sMsg);
        List<PrepNamesDto> LoadPreps(out string sMsg);
        AppointmentDto GetAppointmentById(int id, out string sMsg);
        LocationDto GetLocationById(int id, out string sMsg);
        DoctorsDto GetDoctorById(int id, out string sMsg);
        bool SaveAppointment(AppointmentDto appointment, out string sMsg);
        bool SaveAudit(AuditDto audit, out string sMsg);
        List<CarDto> LoadCars(out string sMsg);
    }
}
