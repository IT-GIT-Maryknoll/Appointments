using Appointments.Database.Context;
using Appointments.Database.Dto;
using Dapper;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Appointments.Database.Interfaces
{
    public interface IDoctorsRepository
    {
        public bool DeleteDoctor(int id, out string sMsg);
        public bool SaveDoctor(DoctorsDto doctor, out string sMsg);
        //private void SaveDoctorLocation(LocationDto location, out string locationMsg);
        public List<DoctorsDto> LoadDoctors(bool bActive, out string sMsg);
        public List<LocationDto> GetDoctorLocations(int id, out string sMsg);
        public LocationDto GetLocationById(int id, out string sMsg);
        public DoctorsDto GetDoctorById(int id, out string sMsg);
        public bool SaveAudit(AuditDto audit, out string sMsg);
    }
}
