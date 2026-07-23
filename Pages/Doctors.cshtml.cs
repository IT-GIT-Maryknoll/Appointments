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
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;

namespace Appointments.Pages
{
    public class DoctorsModel(ILogger<DoctorsModel> logger, IConfiguration configuration, IDoctorsRepository doctorsData, IPermissionsRepository permissionsRepository) :
        BasePageModel(logger, configuration, permissionsRepository)
    {
        private readonly ILogger<DoctorsModel> _logger = logger;
        private readonly IConfiguration _configuration = configuration;
        private readonly IDoctorsRepository _doctorsData = doctorsData;

        //public required UserDto UserInfo;
        public List<LocationDto> lstLocations = new List<LocationDto>();
        public List<DoctorsDto> lstDoctors = new List<DoctorsDto>();
        public int iLoadActive = 1;
        //public bool bIsAdmin = false;
        //public List<string> lstTables = new List<string>();
        public string sMask = "";

        //public void OnGet(bool bActive = false)
        //{
        //    OnGetBase();
        //    //string sMsg = "";

        //    //UserInfo = GetUserInfo(GetUserName());
        //    lstDoctors = _doctorsData.LoadDoctors(bActive, out sMsg);
        //    sMask = ViewData["DoctorsAccessMask"]?.ToString() ?? "";
        //}
        public void OnGet(string sActive)
        {
            int iActive = 1; 
            if(!int.TryParse(sActive, out iActive)) iActive= 1;
            OnGetBase();
            //string sMsg = "";

            //UserInfo = GetUserInfo(GetUserName());
            iLoadActive = iActive;
            lstDoctors = _doctorsData.LoadDoctors(iActive, out sMsg);
            sMask = ViewData["DoctorsAccessMask"]?.ToString() ?? "";
        }
        public JsonResult OnGetGetDoctorById(int id)
        {
            string sMsg = "";
            DoctorsDto oRet = _doctorsData.GetDoctorById(id, out sMsg);
            if ((TempData["Message"] == null || (TempData["Message"]?.ToString()?.Length ?? 0) == 0) && sMsg != null && sMsg.Trim().Length > 0) TempData["Message"] = sMsg;
            else TempData["Message"] += sMsg;
            sMsg = "";
            return new JsonResult(oRet);
        }
        public JsonResult OnGetDeleteDoctorById(int id)
        {
            string sMsg = "";
           DoctorsDto doctor =  _doctorsData.GetDoctorById(id, out sMsg);
            bool bRet = _doctorsData.DeleteDoctor(id, out sMsg);
            if (bRet && doctor != null)
            {
                SaveAudit(GetUserName(), GetIpValue(), "Doctors", "Delete", "Deleted Doctor: " + doctor.Last + ", " + doctor.First + " Company: " + doctor.Company);
                if (bRet && doctor.Locations != null && doctor.Locations.Count > 0)
                {
                    foreach (LocationDto location in doctor.Locations)
                    {
                        SaveAudit(GetUserName(), GetIpValue(), "Locations", "Delete", "Deleted Location: " + location.Street + ", " + location.City + " Phone: " + location.Phone + " Fax: " + location.Fax);
                    }
                }
            }
            if ((TempData["Message"] == null || (TempData["Message"]?.ToString()?.Length ?? 0) == 0) && sMsg != null && sMsg.Trim().Length > 0) TempData["Message"] = sMsg;
            else TempData["Message"] += sMsg;
            sMsg = "";

            return new JsonResult(bRet);
        }
        public JsonResult OnPostSaveDoctor(DoctorsDto oDoctor)
        {
            string sAuditDesc = ""; DoctorsDto oExisting = null;
            string sMsg = ""; int iTemp = 0; //DateTime dTemp = new DateTime();
            DoctorsDto doctor = new DoctorsDto();
            if (int.TryParse(Request.Form["hdnDoctorKey"], out iTemp)) doctor.DoctorKey = iTemp;
            if (doctor.DoctorKey > 0)
            {
                //get existing doctor to update
                string sTempMsg = "";
                oExisting = _doctorsData.GetDoctorById(doctor.DoctorKey, out sTempMsg);
                if (oExisting != null)
                {
                    doctor = oExisting.Clone();
                    if ((TempData["Message"] == null || (TempData["Message"]?.ToString()?.Length ?? 0) == 0) && sTempMsg != null && sTempMsg.Trim().Length > 0) TempData["Message"] = sTempMsg;
                    else TempData["Message"] += sTempMsg;
                    sTempMsg = "";
                }
            }
            if (TempData["Message"] != null && TempData["Message"].ToString().Trim().Length > 0) return new JsonResult(false);
            doctor.Last = Request.Form["txtLast"];
            if (doctor.Last != oExisting?.Last || oExisting == null)
                sAuditDesc = sAuditDesc + "Last " + (oExisting == null ? "" : "[old]: " + oExisting?.Last) + " [new] " + doctor.Last + "\n";
            doctor.First = Request.Form["txtFirst"];
            if (doctor.First != oExisting?.First || oExisting == null)
                sAuditDesc = sAuditDesc + "First " + (oExisting == null ? "" : "[old]: " + oExisting?.First) + " [new] " + doctor.First + "\n";
            doctor.Company = Request.Form["txtCompany"];
            if (doctor.Company != oExisting?.Company || oExisting == null)
                sAuditDesc = sAuditDesc + "Company " + (oExisting == null ? "" : "[old]: " + oExisting?.Company) + " [new] " + doctor.Company + "\n";
            doctor.Specialty = Request.Form["txtSpecialty"];
            if (doctor.Specialty != oExisting?.Specialty || oExisting == null)
                sAuditDesc = sAuditDesc + "Specialty " + (oExisting == null ? "" : "[old]: " + oExisting?.Specialty) + " [new] " + doctor.Specialty + "\n";
            doctor.Title = Request.Form["txtTitle"];
            if (doctor.Title != oExisting?.Title || oExisting == null)
                sAuditDesc = sAuditDesc + "Title " + (oExisting == null ? "" : "[old]: " + oExisting?.Title) + " [new] " + doctor.Title + "\n";
            doctor.IsDeleted = Request.Form["chkDeletedF"].Equals("on");
            if (doctor.IsDeleted != oExisting?.IsDeleted || oExisting == null)
                sAuditDesc = sAuditDesc + "Deleted " + (oExisting == null ? "" : "[old]: " + oExisting?.IsDeleted) + " [new] " + doctor.IsDeleted + "\n";



            if (doctor.DoctorKey == 0)
            {
                doctor.CreatedBy = GetUserName();
                if (doctor.CreatedBy != oExisting?.CreatedBy || oExisting == null)
                    sAuditDesc = sAuditDesc + "CreatedBy " + (oExisting == null ? "" : "[old]: " + oExisting?.CreatedBy) + " [new] " + doctor.CreatedBy + "\n";
                doctor.AddDate = DateTime.Now;
                if (doctor.AddDate != oExisting?.AddDate || oExisting == null)
                    sAuditDesc = sAuditDesc + "AddDate " + (oExisting == null ? "" : "[old]: " + oExisting?.AddDate) + " [new] " + doctor.AddDate + "\n";
            }
            doctor.ModifiedBy = GetUserName();
            if (doctor.ModifiedBy != oExisting?.ModifiedBy || oExisting == null)
                sAuditDesc = sAuditDesc + "ModifiedBy "+  (oExisting == null ? "" : "[old]: " + oExisting?.ModifiedBy) + " [new] " + doctor.ModifiedBy + "\n";
            doctor.ModDate = DateTime.Now;
            if (doctor.ModDate != oExisting?.ModDate || oExisting == null)
                sAuditDesc = sAuditDesc + "ModDate "+ (oExisting == null ? "" : " [old]: " + oExisting?.ModDate) + " [new] " + doctor.ModDate + "\n";

            List<LocationDto> locations = new List<LocationDto>();
            string strLocationsString = Request.Form["hdnLocations"];
            List<string> sAudits = new List<string>();
            if(strLocationsString != null && strLocationsString.Trim().Length > 0)
            {
            JsonDocument docLocations = JsonDocument.Parse(strLocationsString);
            foreach(JsonElement root in docLocations.RootElement.EnumerateArray()) {
                    string sAudit = "";
                    LocationDto location = new LocationDto();
                    int iID = 0; int.TryParse(root.GetProperty("id").GetString(), out iID);
                    location.LocationID = iID;
                    location.Street = root.GetProperty("street").GetString();
                    location.City = root.GetProperty("city").GetString(); 
                    location.Phone = root.GetProperty("phone").GetString(); 
                    location.Fax = root.GetProperty("fax").GetString(); 
                    location.InHouse = root.GetProperty("inHouse").GetBoolean();
                    locations.Add(location); 
                    if(iID == 0){
                        sAudit = "Create_Strreet [new]: " + location.Street + " City [new]: " + location.City + " Phone [new]: " + location.Phone + " Fax [new]: " + location.Fax + " InHouse [new]: " + location.InHouse; 
                    }
                    else if (iID < 0)
                    {
                        sAudit = "Delete_Strreet [old]: " + location.Street + " City [old]: " + location.City + " Phone [old]: " + location.Phone + " Fax [old]: " + location.Fax + " InHouse [old]: " + location.InHouse;
                    }
                    else {
                        LocationDto oExistingLocation = null;
                        string sTempMsg = "";
                        oExistingLocation = _doctorsData.GetLocationById(iID, out sTempMsg);
                        if (oExistingLocation != null) {
                            if (location.Street != oExistingLocation?.Street || oExistingLocation == null)
                                sAudit = sAudit + "Street " + (oExistingLocation == null ? "" : "[old]: " + oExistingLocation?.Street) + " [new] " + location.Street + "\n";
                            if (location.City != oExistingLocation?.City || oExistingLocation == null)
                                sAudit = sAudit + "City " + (oExistingLocation == null ? "" : "[old]: " + oExistingLocation?.City) + " [new] " + location.City + "\n";
                            if (location.Phone != oExistingLocation?.Phone || oExistingLocation == null)
                                sAudit = sAudit + "Phone " + (oExistingLocation == null ? "" : "[old]: " + oExistingLocation?.Phone) + " [new] " + location.Phone + "\n";
                            if (location.Fax != oExistingLocation?.Fax || oExistingLocation == null)
                                sAudit = sAudit + "Fax " + (oExistingLocation == null ? "" : "[old]: " + oExistingLocation?.Fax) + " [new] " + location.Fax + "\n";
                            if (location.InHouse != oExistingLocation?.InHouse || oExistingLocation == null)
                                sAudit = sAudit + "InHouse " + (oExistingLocation == null ? "" : "[old]: " + oExistingLocation?.InHouse) + " [new] " + location.InHouse + "\n";
                        }
                        if (sAudit.Trim().Length > 0) sAudit = "Update_" + sAudit;
                    }
                    if (sAudit.Trim().Length > 0) sAudits.Add(sAudit);
                }
                doctor.Locations = locations;
            }


            bool bRet = _doctorsData.SaveDoctor(doctor, out sMsg);
            if (bRet) {
                SaveAudit(GetUserName(), GetIpValue(), "Doctors", doctor.DoctorKey == 0 ? "Create" : "Update", sAuditDesc);
                foreach(string sA in sAudits)
                {
                    string sAction = sA.Split('_')[0];
                    string sDescr = sA.Split('_')[1];
                    SaveAudit(GetUserName(), GetIpValue(), "Locations", sAction, sDescr);
                }
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

                _doctorsData.SaveAudit(audit, out string sMsg);
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

