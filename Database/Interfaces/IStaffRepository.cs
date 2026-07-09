using Appointments.Database.Context;
using Appointments.Database.Dto;
using Dapper;
using System;
using System.Collections.Generic;

namespace Appointments.Database.Interfaces
{
    public interface IStaffRepository
    {
        public bool DeleteNurse(string id, out string sMsg);
        public bool SaveNurse(StaffDto staff, bool isNew, out string sMsg);
        public string GetNextID(out string sMsg);
        public bool CheckExistingID(string sID, out string sMsg);
        public List<StaffDto> LoadNurses(out string sMsg);
        public StaffDto GetNurseById(int id, out string sMsg);
    }
}
