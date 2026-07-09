using Appointments.Database.Dto;
using Appointments.Database.Interfaces;
using Microsoft.AspNetCore.Http;
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
    public class NursesModel(ILogger<DoctorsModel> logger, IConfiguration configuration, IStaffRepository nursesData) : PageModel
    {
        private readonly ILogger<DoctorsModel> _logger = logger;
        private readonly IConfiguration _configuration = configuration;
        private readonly IStaffRepository _nursesData = nursesData;

        public required UserDto UserInfo;
        public List<StaffDto> lstNurses = new List<StaffDto>();

        public void OnGet()
        {
            string sMsg = "";

            UserInfo = GetUserInfo(GetUserName());
            lstNurses = _nursesData.LoadNurses(out sMsg);

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
        public JsonResult OnGetGetNurseById(int id)
        {
            string sMsg = "";
            StaffDto oRet = _nursesData.GetNurseById(id, out sMsg);
            if ((TempData["Message"] == null || (TempData["Message"]?.ToString()?.Length ?? 0) == 0) && sMsg != null && sMsg.Trim().Length > 0) TempData["Message"] = sMsg;
            else TempData["Message"] += sMsg;
            sMsg = "";
            return new JsonResult(oRet);
        }
        public JsonResult OnGetDeleteNurseById(string id)
        {
            string sMsg = "";
            bool bRet = _nursesData.DeleteNurse(id, out sMsg);
            if ((TempData["Message"] == null || (TempData["Message"]?.ToString()?.Length ?? 0) == 0) && sMsg != null && sMsg.Trim().Length > 0) TempData["Message"] = sMsg;
            else TempData["Message"] += sMsg;
            sMsg = "";
            return new JsonResult(bRet);
        }
        public JsonResult OnGetSaveNurse(string firstName, string lastName, string employeeId, bool isNew)
        {
            string sMsg = "";
            StaffDto staff = new StaffDto();
            staff.FirstName = firstName; staff.LastName = lastName; staff.EmployeeID = employeeId;
            bool bRet = _nursesData.SaveNurse(staff, isNew, out sMsg);
            if ((TempData["Message"] == null || (TempData["Message"]?.ToString()?.Length ?? 0) == 0) && sMsg != null && sMsg.Trim().Length > 0) TempData["Message"] = sMsg;
            else TempData["Message"] += sMsg;
            sMsg = "";
            return new JsonResult(bRet);
        }
    }
}
