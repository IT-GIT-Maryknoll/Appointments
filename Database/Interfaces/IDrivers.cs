using Appointments.Database.Context;
using Appointments.Database.Dto;
using Dapper;
using System;
using System.Collections.Generic;

namespace Appointments.Database.Interfaces
{
    public interface IDriversRepository
    {
        public bool DeleteDriver(int id, out string sMsg);
        public bool SaveDriver(DriversDto driver, bool isNew, out string sMsg);
        public List<DriversDto> LoadDrivers(out string sMsg);
        public DriversDto GetDriverById(int id, out string sMsg);
    }
}
