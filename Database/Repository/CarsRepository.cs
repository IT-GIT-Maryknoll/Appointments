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
    public class CarsRepository: ICarsRepository
    {
        private readonly DbContext _context;
        private readonly ILogger<CarsRepository> _logger;

        public CarsRepository(DbContext context, ILogger<CarsRepository> logger)
        {
            _context = context;
            _logger = logger;
        }
        public bool DeleteCar(int id, out string sMsg)
        {
            sMsg = string.Empty;
            try
            {
                using var connection = _context.CreateConnection();
                var query = "DELETE FROM tblCars WHERE CarNum = @Id";
                connection.Execute(query, new { Id = id });
                sMsg = $"The Record is successfully deleted ";
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting car with Id {id}", id);
                sMsg = "ERROR: " + ex.Message + " " + (ex.InnerException == null ? "" : ex.InnerException.Message);
                return false;
            }
        }
        public bool SaveCar(CarDto car, bool isNew, out string sMsg)
        {
            sMsg = string.Empty;
            var query = "";

            var parameters = new DynamicParameters();
            parameters.Add("CarNum", car.CarNum);
            parameters.Add("CarMake", car.CarMake);
            parameters.Add("CarModel", car.CarModel);
            parameters.Add("Type", car.Type);
            parameters.Add("CarInfo", car.CarInfo);

            try
            {
                using var connection = _context.CreateConnection();
                if (!isNew)
                {
                    query = @"UPDATE [dbo].[tblCars] SET [Car_Make] = @CarMake, [Car_Model] = @CarModel, [Type] = @Type, [Car_Info] = @CarInfo
                              WHERE CarNum = @CarNum";
                }
                else
                {
                    query = @"INSERT INTO [dbo].[tblCars] (CarNum, [Car_Make], [Car_Model], [Type], [Car_Info])
                              VALUES ((select max(CarNum) from [dbo].[tblCars]) + 1, @CarMake, @CarModel, @Type, @CarInfo)";
                }

                connection.Execute(query, parameters);
                sMsg = $"The Car record is successfully saved ";
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving car for {ID}", car.CarNum);
                sMsg = "ERROR: " + ex.Message + " " + (ex.InnerException == null ? "" : ex.InnerException.Message);
                return false;
            }
        }

        public List<CarDto> LoadCars(out string sMsg)
        {
            sMsg = string.Empty;
            try
            {
                using var connection = _context.CreateConnection();
                var query = "SELECT [CarNum],[Car_Make] CarMake,[Car_Model] CarModel,[Type],[Car_Info] CarInfo FROM tblCars ORDER BY [Car_Make]";
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
        public CarDto GetCarById(int id, out string sMsg)
        {
            sMsg = string.Empty;
            try
            {
                using var connection = _context.CreateConnection();
                var query = "SELECT [CarNum],[Car_Make],[Car_Model],[Type],[Car_Info] FROM tblCars WHERE CarNum = @Id";
                var car = connection.QuerySingleOrDefault<CarDto>(query, new { Id = id });

                return car ?? new CarDto();
            }
            catch (Exception ex)
            {
                sMsg = "ERROR: " + ex.Message + " " + (ex.InnerException == null ? "" : ex.InnerException.Message);
                _logger.LogError(ex, "Error loading car for ID {Id}", id);
                return new CarDto();
            }

        }
    }
}
