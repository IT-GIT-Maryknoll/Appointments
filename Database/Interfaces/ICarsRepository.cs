using Appointments.Database.Context;
using Appointments.Database.Dto;
using Dapper;
using System;
using System.Collections.Generic;

namespace Appointments.Database.Interfaces
{
    public interface ICarsRepository
    {
        public bool DeleteCar(int id, out string sMsg);
        public bool SaveCar(CarDto car, bool isNew, out string sMsg);
        public List<CarDto> LoadCars(out string sMsg);
        public CarDto GetCarById(int id, out string sMsg);
    }
}
