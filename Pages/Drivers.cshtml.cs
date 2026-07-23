using Appointments.Database.Dto;
using Appointments.Database.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Configuration;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;

namespace Appointments.Pages
{
    public class DriversModel(ILogger<DriversModel> logger, IConfiguration configuration, IDriversRepository driversData, IPermissionsRepository permissionsRepository) :
        BasePageModel(logger, configuration, permissionsRepository)
    {
        private readonly ILogger<DriversModel> _logger = logger;
        private readonly IConfiguration _configuration = configuration;
        private readonly IDriversRepository _driversData = driversData;
        
        public List<DriversDto> lstDrivers = new List<DriversDto>();
        public string sMask = "";
        public int iLoadActive = 1;

        public void OnGet(string sActive)
        {
            int iActive = 1;
            if (!int.TryParse(sActive, out iActive)) iActive = 1;
            OnGetBase();
            //string sMsg = "";

            //UserInfo = GetUserInfo(GetUserName());
            iLoadActive = iActive;
            lstDrivers = _driversData.LoadDrivers(iActive, out sMsg);
            sMask = ViewData["DriversAccessMask"]?.ToString() ?? "";
        }

        public JsonResult OnGetGetDriverById(int id)
        {
            string sMsg = "";
            DriversDto oRet = _driversData.GetDriverById(id, out sMsg);
            if ((TempData["Message"] == null || (TempData["Message"]?.ToString()?.Length ?? 0) == 0) && sMsg != null && sMsg.Trim().Length > 0) TempData["Message"] = sMsg;
            else TempData["Message"] += sMsg;
            sMsg = "";
            return new JsonResult(oRet);
        }
        public JsonResult OnGetDeleteDriverById(int id)
        {
            string sMsg = "";
            bool bRet = _driversData.DeleteDriver(id, out sMsg);
            if ((TempData["Message"] == null || (TempData["Message"]?.ToString()?.Length ?? 0) == 0) && sMsg != null && sMsg.Trim().Length > 0) TempData["Message"] = sMsg;
            else TempData["Message"] += sMsg;
            sMsg = "";
            return new JsonResult(bRet);
        }
        public JsonResult OnGetSaveDriver(string firstName, string lastName, int driverId, string title, int priority, bool inactive, bool isNew)
        {
            string sMsg = "";
            DriversDto driver = new DriversDto();
            driver.First = firstName; driver.Last = lastName; driver.DriverID = driverId; driver.Title = title; driver.Priority = priority; driver.InActive = inactive;
            bool bRet = _driversData.SaveDriver(driver, isNew, out sMsg);
            if ((TempData["Message"] == null || (TempData["Message"]?.ToString()?.Length ?? 0) == 0) && sMsg != null && sMsg.Trim().Length > 0) TempData["Message"] = sMsg;
            else TempData["Message"] += sMsg;
            sMsg = "";
            return new JsonResult(bRet);
        }
     }
}
