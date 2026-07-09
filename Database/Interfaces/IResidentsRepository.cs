using Appointments.Database.Context;
using Appointments.Database.Dto;
using Dapper;
using System;
using System.Collections.Generic;

namespace Appointments.Database.Interfaces
{
    public interface IResidentsRepository
    {
        public List<ResidentsDto> LoadResidents(out string sMsg);
        public ResidentsDto LoadResidentById(int id, out string sMsg);
        public bool SaveResident(ResidentsDto resident, out string sMsg);
        public bool DeleteResident(int id, out string sMsg);
        public List<StatusDto> LoadStatuses(out string sMsg);
        public List<string> LoadCountries(out string sMsg);
        public bool SaveAudit(AuditDto audit, out string sMsg);

    }
}
