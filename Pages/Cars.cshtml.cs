using Appointments.Database.Dto;
using Appointments.Database.Interfaces;
using Appointments.Database.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;

namespace Appointments.Pages
{
    public class CarsModel(ILogger<CarsModel> logger, IConfiguration configuration, ICarsRepository carsData, IPermissionsRepository permissionsRepository) :
        BasePageModel(logger, configuration, permissionsRepository)

    {
        private readonly ICarsRepository _carsData = carsData;

            public List<CarDto> lstCars = new List<CarDto>();
            public string sMask = "";

        public void OnGet()
        {
            OnGetBase();
            lstCars = _carsData.LoadCars(out sMsg);
            sMask = ViewData["CarsAccessMask"]?.ToString() ?? "";
        }
            public JsonResult OnGetGetCarById(int id)
            {
                string sMsg = "";
                CarDto oRet = _carsData.GetCarById(id, out sMsg);
                if ((TempData["Message"] == null || (TempData["Message"]?.ToString()?.Length ?? 0) == 0) && sMsg != null && sMsg.Trim().Length > 0) TempData["Message"] = sMsg;
                else TempData["Message"] += sMsg;
                sMsg = "";
                return new JsonResult(oRet);
            }
            public JsonResult OnGetDeleteCarById(int id)
            {
                string sMsg = "";
                bool bRet = _carsData.DeleteCar(id, out sMsg);
                if ((TempData["Message"] == null || (TempData["Message"]?.ToString()?.Length ?? 0) == 0) && sMsg != null && sMsg.Trim().Length > 0) TempData["Message"] = sMsg;
                else TempData["Message"] += sMsg;
                sMsg = "";
                return new JsonResult(bRet);
            }
            public JsonResult OnGetSaveCar(string carInfo, string carMake, int carNum, string carModel, string carType, bool isNew)
            {
                string sMsg = "";
                CarDto car = new CarDto();
                car.CarInfo = carInfo; car.CarMake = carMake; car.CarNum = carNum; car.CarModel = carModel ; car.Type = carType; 
                bool bRet = _carsData.SaveCar(car, isNew, out sMsg);
                if ((TempData["Message"] == null || (TempData["Message"]?.ToString()?.Length ?? 0) == 0) && sMsg != null && sMsg.Trim().Length > 0) TempData["Message"] = sMsg;
                else TempData["Message"] += sMsg;
                sMsg = "";
                return new JsonResult(bRet);
            }
    }
}
