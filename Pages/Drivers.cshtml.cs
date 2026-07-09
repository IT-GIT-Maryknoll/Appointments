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
    public class DriversModel(ILogger<DriversModel> logger, IConfiguration configuration, IDriversRepository driversData) : PageModel
    {
        private readonly ILogger<DriversModel> _logger = logger;
        private readonly IConfiguration _configuration = configuration;
        private readonly IDriversRepository _driversData = driversData;
        
        public required UserDto UserInfo;
        public List<DriversDto> lstDrivers = new List<DriversDto>();

        public void OnGet()
        {
            string sMsg = "";

            UserInfo = GetUserInfo(GetUserName());
            lstDrivers = _driversData.LoadDrivers(out sMsg);

        }
        private string GetUserName()
        {
            if (HttpContext == null) return null;

            var userName = HttpContext.User.Identity.Name;
            string sUserConfig = configuration.GetValue<string>("UName");
            if (sUserConfig != null && sUserConfig.Trim().Length > 0) userName = sUserConfig;
            if (string.IsNullOrEmpty(userName) || userName.Length < 0)
            {
                return "";
            }
            else
            {
                var arrName = userName.Split("\\");
                return arrName[1];
            }
        }
        public UserDto GetUserInfo(string sUser)
        {
            if (UserInfo is object) return UserInfo;
            using (var context = new PrincipalContext(ContextType.Domain))
            {
                var userPrincipal = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, sUser);
                if (userPrincipal != null)
                {
                    DirectoryEntry? directoryEntry = userPrincipal.GetUnderlyingObject() as DirectoryEntry;
                    UserDto oRet = new UserDto();
                    oRet.sAMAccountName = sUser;
                    oRet.FirstName = userPrincipal.GivenName;
                    oRet.MI = userPrincipal.MiddleName;
                    oRet.LastName = userPrincipal.Surname;
                    oRet.DisplayName = userPrincipal.DisplayName;
                    oRet.MbrEmpIndicator = directoryEntry.Properties["company"].Value == null ? "" : directoryEntry.Properties["company"].Value.ToString();
                    oRet.MbrEmpNumber = userPrincipal.EmployeeId;
                    if (oRet.MbrEmpNumber == null) oRet.MbrEmpNumber = (directoryEntry.Properties["title"].Value == null ? "" : directoryEntry.Properties["title"].Value.ToString());
                    oRet.EmailAddress = userPrincipal.EmailAddress;
                    oRet.ADSDescription = userPrincipal.Description;
                    oRet.msExchHideFromAddressLists = (directoryEntry.Properties["msExchHideFromAddressLists"].Value == null ? false : directoryEntry.Properties["msExchHideFromAddressLists"].Value.ToString().ToLower().Equals("true"));
                    if (oRet.msExchHideFromAddressLists) oRet.EmailAddress = "";

                    UserInfo = oRet;
                    return oRet;
                }
            }
            return null;
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
