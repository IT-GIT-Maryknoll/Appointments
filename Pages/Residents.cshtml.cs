using Appointments.Database.Dto;
using Appointments.Database.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;

namespace Appointments.Pages
{
    public class ResidentsModel : PageModel
    {
        private readonly ILogger<ResidentsModel> _logger;
        private readonly IConfiguration _configuration;
        private readonly IResidentsRepository _residentsData;

        public required UserDto UserInfo;
        public List<StatusDto> lstStatuses = new List<StatusDto>();
        public List<ResidentsDto> lstResidents = new List<ResidentsDto>();
        public List<string> lstCountries = new List<string>();

        public ResidentsModel(ILogger<ResidentsModel> logger, IConfiguration configuration, IResidentsRepository residentsData)
        {
            _logger = logger;
            _configuration = configuration;
            _residentsData = residentsData;
        }

        public void OnGet()
        {
            string sMsg = "";

            UserInfo = GetUserInfo(GetUserName());
            lstResidents = _residentsData.LoadResidents(out sMsg);
            lstStatuses = _residentsData.LoadStatuses(out sMsg);
            lstCountries = _residentsData.LoadCountries(out sMsg);
        }
        private string GetUserName()
        {
            if (HttpContext == null) return null;

            var userName = HttpContext.User.Identity.Name;
            string sUserConfig = _configuration.GetValue<string>("UName");
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
                    //oRet.IsAdmin = _voteList.IsUserAdmin(sUser);
                    //if (!oRet.IsAdmin) oRet.IsRegional = _voteList.IsUserRegional(sUser);
                    //if (oRet.IsRegional) oRet.Regions = _voteList.GetRegions(sUser);

                    UserInfo = oRet;
                    return oRet;
                }
            }
            return null;
        }
        public JsonResult OnGetGetResidentById(int id)
        {
            string sMsg = "";
            ResidentsDto oRet = _residentsData.LoadResidentById  (id, out sMsg);
            if ((TempData["Message"] == null || (TempData["Message"]?.ToString()?.Length ?? 0) == 0) && sMsg != null && sMsg.Trim().Length > 0) TempData["Message"] = sMsg;
            else TempData["Message"] += sMsg;
            sMsg = "";
            return new JsonResult(oRet);
        }
        public JsonResult OnGetDeleteResidentById(int id)
        {
            string sMsg = "";
            bool bRet = _residentsData.DeleteResident(id, out sMsg);
            if ((TempData["Message"] == null || (TempData["Message"]?.ToString()?.Length ?? 0) == 0) && sMsg != null && sMsg.Trim().Length > 0) TempData["Message"] = sMsg;
            else TempData["Message"] += sMsg;
            sMsg = "";
            return new JsonResult(bRet);
        }
        public JsonResult OnPostSaveResident(ResidentsDto oResident)
        {
            string sAuditDesc = ""; ResidentsDto oExisting = null;
            string sMsg = ""; int iTemp = 0; //DateTime dTemp = new DateTime();
            ResidentsDto resident = new ResidentsDto();
            if (int.TryParse(Request.Form["hdnResidentID"], out iTemp)) resident.ID = iTemp;
            if (resident.ID > 0)
            {
                //get existing resident to update
                string sTempMsg = "";
                oExisting = _residentsData.LoadResidentById(resident.ID, out sTempMsg);
                if (oExisting != null)
                {
                    resident = oExisting.Clone();
                    if ((TempData["Message"] == null || (TempData["Message"]?.ToString()?.Length ?? 0) == 0) && sTempMsg != null && sTempMsg.Trim().Length > 0) TempData["Message"] = sTempMsg;
                    else TempData["Message"] += sTempMsg;
                    sTempMsg = "";
                }
            }
            if (TempData["Message"] != null && TempData["Message"].ToString().Trim().Length > 0) return new JsonResult(false);
            resident.Res_FirstName = Request.Form["txtFirst"];
            if (resident.Res_FirstName != oExisting?.Res_FirstName || oExisting == null)
                sAuditDesc = sAuditDesc + "First " + (oExisting == null ? "" : "[old]: " + oExisting?.Res_FirstName) + " [new] " + resident.Res_FirstName + "\n";
            resident.Res_LastName = Request.Form["txtLast"];
            if (resident.Res_LastName != oExisting?.Res_LastName || oExisting == null)
                sAuditDesc = sAuditDesc + "Last " + (oExisting == null ? "" : "[old]: " + oExisting?.Res_LastName) + " [new] " + resident.Res_LastName + "\n";
            resident.Res_MiddleInt = Request.Form["txtMiddle"];
            if (resident.Res_MiddleInt != oExisting?.Res_MiddleInt || oExisting == null)
                sAuditDesc = sAuditDesc + "Middle " + (oExisting == null ? "" : "[old]: " + oExisting?.Res_MiddleInt) + " [new] " + resident.Res_MiddleInt + "\n";
            resident.Res_Title = Request.Form["txtTitle"];
            if (resident.Res_Title != oExisting?.Res_Title || oExisting == null)
                sAuditDesc = sAuditDesc + "Title " + (oExisting == null ? "" : "[old]: " + oExisting?.Res_Title) + " [new] " + resident.Res_Title + "\n";
            resident.OnStaff = Request.Form["chkStaff"].Equals("on");
            if (resident.OnStaff != oExisting?.OnStaff || oExisting == null)
                sAuditDesc = sAuditDesc + "On Staff " + (oExisting == null ? "" : "[old]: " + oExisting?.OnStaff) + " [new] " + resident.OnStaff + "\n";
            resident.Name = Request.Form["txtName"];
            if (resident.Name != oExisting?.Name || oExisting == null)
                sAuditDesc = sAuditDesc + "Name " + (oExisting == null ? "" : "[old]: " + oExisting?.Name) + " [new] " + resident.Name + "\n";
            resident.Room = Request.Form["txtRoom"];
            if (resident.Room != oExisting?.Room || oExisting == null)
                sAuditDesc = sAuditDesc + "Room " + (oExisting == null ? "" : "[old]: " + oExisting?.Room) + " [new] " + resident.Room + "\n";
            resident.Ext = Request.Form["txtExt"];
            if (resident.Ext != oExisting?.Ext || oExisting == null)
                sAuditDesc = sAuditDesc + "Title " + (oExisting == null ? "" : "[old]: " + oExisting?.Ext) + " [new] " + resident.Ext + "\n";
            resident.DateAssigned = Request.Form["txtDateAssigned"];
            if (resident.DateAssigned != oExisting?.DateAssigned || oExisting == null)
                sAuditDesc = sAuditDesc + "Date Assigned " + (oExisting == null ? "" : "[old]: " + oExisting?.DateAssigned) + " [new] " + resident.DateAssigned + "\n";
            resident.Status = Request.Form["cmbStatus"];
            if (resident.Status != oExisting?.Status || oExisting == null)
                sAuditDesc = sAuditDesc + "Status " + (oExisting == null ? "" : "[old]: " + oExisting?.Status) + " [new] " + resident.Status + "\n";
            resident.ResidentNotes = Request.Form["txtNotes"];
            if (resident.ResidentNotes != oExisting?.ResidentNotes || oExisting == null)
                sAuditDesc = sAuditDesc + "Date ResidentNotes " + (oExisting == null ? "" : "[old]: " + oExisting?.ResidentNotes) + " [new] " + resident.ResidentNotes + "\n";
            resident.DOB = Request.Form["txtDOB"];
            if (resident.DOB != oExisting?.DOB || oExisting == null)
                sAuditDesc = sAuditDesc + "DOB " + (oExisting == null ? "" : "[old]: " + oExisting?.DOB) + " [new] " + resident.DOB + "\n";
            resident.Location = Request.Form["txtLocation"];
            if (resident.Location != oExisting?.Location || oExisting == null)
                sAuditDesc = sAuditDesc + "Location " + (oExisting == null ? "" : "[old]: " + oExisting?.Location) + " [new] " + resident.Location + "\n";
            resident.Country = Request.Form["txtCountry"];
            if (resident.Country != oExisting?.Country || oExisting == null)
                sAuditDesc = sAuditDesc + "Location " + (oExisting == null ? "" : "[old]: " + oExisting?.Country) + " [new] " + resident.Country + "\n";


            bool bRet = _residentsData.SaveResident(resident, out sMsg);
            if (bRet)
            {
                SaveAudit(GetUserName(), GetIpValue(), "Residents", resident.ResidentKey == 0 ? "Create" : "Update", sAuditDesc);
            }
            if ((TempData["Message"] == null || (TempData["Message"]?.ToString()?.Length ?? 0) == 0) && sMsg != null && sMsg.Trim().Length > 0) TempData["Message"] = sMsg;
            else TempData["Message"] += sMsg;
            sMsg = "";
            return new JsonResult(bRet);
            //return new OkResult();
        }
        public bool SaveAudit(string uID, string ip, string tblName, string action, string description)
        {
            try
            {
                AuditDto audit = new AuditDto();

                audit.Ip = ip;
                audit.Table = tblName;
                audit.Action = action;
                audit.User = uID;
                audit.Description = description;
                audit.Datetime = DateTime.Now;

                _residentsData.SaveAudit(audit, out string sMsg);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        private string GetIpValue()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "";
        }

    }
}
