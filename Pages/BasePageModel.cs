using Appointments.Database.Dto;
using Appointments.Database.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Configuration;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;

namespace Appointments.Pages
{
    public class BasePageModel(ILogger<BasePageModel> logger, IConfiguration configuration, IPermissionsRepository permissionsRepository) : PageModel
    {
        public required UserDto UserInfo;
        public bool IsAdmin = false;
        public List<string> lstTables = new List<string>();
        public List<string> lstTableIDs = new List<string>();
        public string sMsg = "";

        private readonly ILogger<BasePageModel> _logger = logger;
        private readonly IConfiguration _configuration = configuration;
        private readonly IPermissionsRepository _permissionsRepository = permissionsRepository;

        public void OnGetBase()
        {
            UserInfo = GetUserInfo(GetUserName());
            lstTables = GetTables("Tables");
            lstTableIDs = GetTables("TableIDs");
            foreach (var table in lstTableIDs)
            {
                ViewData[table + "AccessMask"] = _permissionsRepository.GetAccessMask(UserInfo.sAMAccountName, table, out sMsg);
            }
            IsAdmin = _permissionsRepository.IsUserAdmin(UserInfo.sAMAccountName, out sMsg);
        }
        public string GetUserName()
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
        public List<string> GetTables(string sSection)
        {
            List<string> lstTables = new List<string>();
            foreach (var table in _configuration.GetSection(sSection).GetChildren())
            {
                lstTables.Add(table.Value);
            }
            return lstTables;
        }
    }
}
