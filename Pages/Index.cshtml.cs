using Appointments.Database;
using Appointments.Database.Dto;
using Appointments.Database.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Text.Json;
using System.Threading.Tasks;
using DirectoryEntry = System.DirectoryServices.DirectoryEntry;

namespace Appointments.Pages
{
    public class IndexModel(ILogger<IndexModel> logger, IConfiguration configuration, IAppointmentsRepository appointmentsData) : PageModel
    {
        public string lblDate = "";
        public string hdnResident = "";
        public string txtResident = "";
        public string hdnDoctor = "";
        public string txtDoctor = "";
        public string cmbApptType = "";
        public string bFilter = "false";

        private readonly ILogger<IndexModel> _logger = logger;
        private readonly IConfiguration _configuration = configuration;
        private readonly IAppointmentsRepository _appointmentsData = appointmentsData;

        public required UserDto UserInfo;
        public List<AppointmentTypeDto> lstAppointmentTypes = new List<AppointmentTypeDto>();
        public List<AppointmentDto> lstAppointments = new List<AppointmentDto>();
        public List<DoctorsDto> lstDoctors = new List<DoctorsDto>();
        public List<ResidentsDto> lstResidents = new List<ResidentsDto>();
        public List<DriversDto> lstDrivers = new List<DriversDto>();
        public List<PrepNamesDto> lstPreps = new List<PrepNamesDto>();
        public List<CarDto> lstCars = new List<CarDto>();

        private string sFilter = "";

        public void OnGet()
        {
            string sMsg = "";

            UserInfo = GetUserInfo(GetUserName());
            if (lstAppointmentTypes.Count() == 0)
            {
                lstAppointmentTypes = _appointmentsData.LoadAppointmentTypes(out sMsg);
                if ((TempData["Message"] == null || (TempData["Message"]?.ToString()?.Length ?? 0) == 0) && sMsg != null && sMsg.Trim().Length > 0) TempData["Message"] = sMsg;
                else TempData["Message"] += sMsg;
                sMsg = "";
            }
            if (lstDoctors.Count() == 0)
            {
                lstDoctors = _appointmentsData.LoadDoctors(out sMsg);
                if ((TempData["Message"] == null || (TempData["Message"]?.ToString()?.Length ?? 0) == 0) && sMsg != null && sMsg.Trim().Length > 0) TempData["Message"] = sMsg;
                else TempData["Message"] += sMsg;
                sMsg = "";
            }
            if (lstResidents.Count() == 0)
            {
                lstResidents = _appointmentsData.LoadResidents(out sMsg);
                if ((TempData["Message"] == null || (TempData["Message"]?.ToString()?.Length ?? 0) == 0) && sMsg != null && sMsg.Trim().Length > 0) TempData["Message"] = sMsg;
                else TempData["Message"] += sMsg;
                sMsg = "";
            }
            if (lstDrivers.Count() == 0)
            {
                lstDrivers = _appointmentsData.LoadDrivers(out sMsg);
                if ((TempData["Message"] == null || (TempData["Message"]?.ToString()?.Length ?? 0) == 0) && sMsg != null && sMsg.Trim().Length > 0) TempData["Message"] = sMsg;
                else TempData["Message"] += sMsg;
                sMsg = "";
            }
            if (lstPreps.Count() == 0)
            {
                lstPreps = _appointmentsData.LoadPreps(out sMsg);
                if ((TempData["Message"] == null || (TempData["Message"]?.ToString()?.Length ?? 0) == 0) && sMsg != null && sMsg.Trim().Length > 0) TempData["Message"] = sMsg;
                else TempData["Message"] += sMsg;
                sMsg = "";
            }
            if (lstCars.Count() == 0)
            {
                lstCars = _appointmentsData.LoadCars(out sMsg);
                if ((TempData["Message"] == null || (TempData["Message"]?.ToString()?.Length ?? 0) == 0) && sMsg != null && sMsg.Trim().Length > 0) TempData["Message"] = sMsg;
                else TempData["Message"] += sMsg;
                sMsg = "";
            }

            string s = ViewData["filter"] == null ? "" : ViewData["filter"].ToString();

            if (s != null && s.Trim().Length > 0)
            {
                JsonElement e = new JsonElement();
                JsonDocument doc = JsonDocument.Parse(s);
                if (doc.RootElement.TryGetProperty("lblDate", out e)) { lblDate = e.GetString().Substring(0, 7); bFilter = "true"; }
                if (doc.RootElement.TryGetProperty("hdnResident", out e)) { hdnResident = e.GetString(); bFilter = "true"; }
                if (doc.RootElement.TryGetProperty("txtResident", out e)) { txtResident = e.GetString(); bFilter = "true"; }
                if (doc.RootElement.TryGetProperty("hdnDoctor", out e)) { hdnDoctor = e.GetString(); bFilter = "true"; }
                if (doc.RootElement.TryGetProperty("txtDoctor", out e)) { txtDoctor = e.GetString(); bFilter = "true"; }
                if (doc.RootElement.TryGetProperty("cmbApptType", out e)) { cmbApptType = e.GetString(); bFilter = "true"; }
                GetFilterIn();
            }
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
                    //oRet.IsAdmin = _voteList.IsUserAdmin(sUser);
                    //if (!oRet.IsAdmin) oRet.IsRegional = _voteList.IsUserRegional(sUser);
                    //if (oRet.IsRegional) oRet.Regions = _voteList.GetRegions(sUser);

                    UserInfo = oRet;
                    return oRet;
                }
            }
            return null;
        }
        private void GetFilter()
        {
            string sTemp = ""; ViewData.Clear(); sFilter = ""; string sTempKey = "";
            string sWord = " and ";

            sTemp = Request.Form["lblDate"];
            if (sTemp != null && sTemp.Trim().Length > 0)
            {
                sTemp = sTemp.Trim() + "-01";
                sFilter += (sFilter.Length > 0 ? sWord : "") + " MONTH([ApptTime]) = MONTH('" + sTemp + "') AND YEAR([ApptTime]) = YEAR('" + sTemp + "')";
                ViewData.Add("lblDate", System.Web.HttpUtility.HtmlDecode(sTemp));
            }

            sTempKey = Request.Form["hdnResident"];
            sTemp = Request.Form["txtResident"];
            if (sTempKey != null && sTempKey.Trim().Length > 0)
            {
                sFilter += (sFilter.Length > 0 ? sWord : "") + " [ResidentKey] = " + sTempKey;
                ViewData.Add("hdnResident", System.Web.HttpUtility.HtmlDecode(sTempKey));
                ViewData.Add("txtResident", System.Web.HttpUtility.HtmlDecode(sTemp));
            }
            else if (sTemp != null && sTemp.Trim().Length > 0 && !sTemp.Equals("Please Select"))
            {
                sFilter += (sFilter.Length > 0 ? sWord : "") + " [ResidentKey] in (SELECT ResidentKey FROM qryAppointmentNameListChina where Upper(FullName) like Upper('%" + sTemp.Trim().Replace("'", "''") + "%'))";
                ViewData.Add("txtResident", System.Web.HttpUtility.HtmlDecode(sTemp));
            }

            sTemp = Request.Form["txtDoctor"];
            sTempKey = Request.Form["hdnDoctor"];
            if (sTempKey != null && sTempKey.Trim().Length > 0)
            {
                sFilter += (sFilter.Length > 0 ? sWord : "") + " [DoctorKey] = " + sTempKey;
                ViewData.Add("hdnDoctor", System.Web.HttpUtility.HtmlDecode(sTempKey));
                ViewData.Add("txtDoctor", System.Web.HttpUtility.HtmlDecode(sTemp));
            }
            else if (sTemp != null && sTemp.Trim().Length > 0 && !sTemp.Equals("Please Select"))
            {
                sFilter += (sFilter.Length > 0 ? sWord : "") + " [DoctorKey] in (SELECT DoctorKey FROM qryDoctors where Upper(DoctorName) like Upper('%" + sTemp.Trim().Replace("'", "''") + "%'))";
                ViewData.Add("txtDoctor", System.Web.HttpUtility.HtmlDecode(sTemp));
            }
            sTemp = Request.Form["cmbApptType"];
            if (sTemp != null && sTemp.Trim().Length > 0)
            {
                sFilter += (sFilter.Length > 0 ? sWord : "") + " ApptType = '" + sTemp.Trim().Replace("'", "''") + "'";
                ViewData.Add("cmbApptType", System.Web.HttpUtility.HtmlDecode(sTemp));
            }
            string s = JsonSerializer.Serialize(ViewData);
            ViewData["filter"] = s;
            if (s.Trim().Length > 0)
            {
                HttpContext.Session.SetString("filter", s);
                HttpContext.Session.SetString("sFilter", sFilter);
            }

        }
        private void GetFilterIn()
        {
            sFilter = "";
            string sTemp = ""; ViewData.Clear(); string sWord = " and ";
            sTemp = lblDate;
            if (sTemp == null || sTemp.Trim().Length == 0) sTemp = DateTime.Now.Year + "-" + DateTime.Now.Month.ToString().PadLeft(2, '0') + "-" + DateTime.Now.Day.ToString().PadLeft(2, '0');
            else sTemp = sTemp.Trim() + "-01";
            sFilter += (sFilter.Length > 0 ? sWord : "") + " MONTH([ApptTime]) = MONTH('" + sTemp + "') AND YEAR([ApptTime]) = YEAR('" + sTemp + "')";
            ViewData.Add("lblDate", System.Web.HttpUtility.HtmlDecode(sTemp));

            sTemp = hdnResident;
            if (sTemp != null && sTemp.Trim().Length > 0)
            {
                sFilter += (sFilter.Length > 0 ? sWord : "") + " [ResidentKey] = " + sTemp;
                ViewData.Add("hdnResident", System.Web.HttpUtility.HtmlDecode(sTemp));
            }
            sTemp = txtResident;
            if (sTemp != null && sTemp.Trim().Length > 0 && (hdnResident is null || hdnResident.Trim().Length == 0))
            {
                sFilter += (sFilter.Length > 0 ? sWord : "") + " [ResidentKey] in (SELECT ResidentKey FROM qryAppointmentNameListChina where Upper(FullName) like Upper('%" + sTemp.Trim().Replace("'", "''") + "%'))";
                ViewData.Add("txtResident", System.Web.HttpUtility.HtmlDecode(sTemp));
            }

            sTemp = hdnDoctor;
            if (sTemp != null && sTemp.Trim().Length > 0)
            {
                sFilter += (sFilter.Length > 0 ? sWord : "") + " [DoctorKey] = " + sTemp;
                ViewData.Add("hdnDoctor", System.Web.HttpUtility.HtmlDecode(sTemp));
            }
            sTemp = txtDoctor;
            if (sTemp != null && sTemp.Trim().Length > 0 && (hdnDoctor is null || hdnDoctor.Trim().Length == 0))
            {
                sFilter += (sFilter.Length > 0 ? sWord : "") + " [DoctorKey] in (SELECT DoctorKey FROM qryDoctors where Upper(DoctorName) like Upper('%" + sTemp.Trim().Replace("'", "''") + "%'))";
                ViewData.Add("txtDoctor", System.Web.HttpUtility.HtmlDecode(sTemp));
            }
            sTemp = cmbApptType;
            if (sTemp != null && sTemp.Trim().Length > 0)
            {
                sFilter += (sFilter.Length > 0 ? sWord : "") + " ApptType = '" + sTemp.Trim().Replace("'", "''") + "'";
                ViewData.Add("cmbApptType", System.Web.HttpUtility.HtmlDecode(sTemp));
            }
            ViewData["filter"] = JsonSerializer.Serialize(ViewData);
            HttpContext.Session.SetString("sFilter", sFilter);
        }
        public void OnPost()
        {
            GetFilter();
            OnGet();
        }
        public async Task<JsonResult> OnGetGetAppointments()
        {
            string sMsg = "";
            //GetFilterIn();
            sFilter = HttpContext.Session.GetString("sFilter");
            if (sFilter == null) GetFilterIn();
            lstAppointments = _appointmentsData.LoadAppointments(sFilter, out sMsg);
            if ((TempData["Message"] == null || (TempData["Message"]?.ToString()?.Length ?? 0) == 0) && sMsg != null && sMsg.Trim().Length > 0) TempData["Message"] = sMsg;
            else TempData["Message"] += sMsg;
            sMsg = "";

            if (lstAppointments == null || lstAppointments.Count == 0)
            {
                return new JsonResult(new List<object>());

                //log error
            }
            var list = new List<object>();

            foreach (var item in lstAppointments)
            {
                var dpt = "";
                var color = "";
                switch (item.ApptType)
                {
                    case "Follow-up":
                        color = "#c4eda9";  // "#378006";
                        break;
                    case "Consult":
                        color = "#afd5f5"; // "#0078D7";
                        break;
                    case "In-house":
                        color = "#ffcd90";  // "#FF8C00";
                        break;
                    case "Diagnostic":
                        color = "#ffabb3";  // "#9B111E";
                        break;
                    default:
                        color = "#c7c7c7";  // "#434343";
                        break;
                }
                if (item.ApptTime == null) item.ApptTime = DateTime.MinValue;
                if (item.Depart.HasValue) dpt = ((DateTime)item.Depart).ToShortTimeString();
                var displayEvent = new
                {
                    //Description = $"<span style='font-size:16px;'>{((DateTime)item.ApptTime).ToShortTimeString()} {item.FullName}<br/> - {item.DoctorName}<br/>{item.City},<br/> {item.Phone} {item.ApptType}<br/>{item.DriverName} Depart {dpt}<br/></span>",
                    //Description=$"abc",
                    End = "",
                    EventID = item.ID,
                    IsFullDay = false,
                    Subject = $"<span style='font-size:16px;color:black;'><span style=' font-weight:bold; text-decoration: underline;'>{((DateTime)item.ApptTm).ToShortTimeString()} {item.FullName}<br/> - {item.DoctorName}</span><br/><span style='font-style:normal;'>{item.City},<br/> {item.Phone} {item.ApptType}<br/>{item.DriverName} Depart {dpt}</span><br/></span>",
                    Start = item.ApptTime == null ? "" : String.Format("{0:u}", ((DateTime)item.ApptTime).AddDays(1)),
                    ThemeColor = color
                };
                list.Add(displayEvent);
            }

            return new JsonResult(list.ToArray());
        }
        public JsonResult OnGetGetAppointmentById(int id)
        {
            string sMsg = "";
            AppointmentDto oRet = _appointmentsData.GetAppointmentById(id, out sMsg);
            if ((TempData["Message"] == null || (TempData["Message"]?.ToString()?.Length ?? 0) == 0) && sMsg != null && sMsg.Trim().Length > 0) TempData["Message"] = sMsg;
            else TempData["Message"] += sMsg;
            sMsg = "";
            return new JsonResult(oRet);
        }
        public JsonResult OnGetGetLocationById(int id)
        {
            string sMsg = "";
            LocationDto oRet = _appointmentsData.GetLocationById(id, out sMsg);
            if ((TempData["Message"] == null || (TempData["Message"]?.ToString()?.Length ?? 0) == 0) && sMsg != null && sMsg.Trim().Length > 0) TempData["Message"] = sMsg;
            else TempData["Message"] += sMsg;
            sMsg = "";
            return new JsonResult(oRet);
        }
        public JsonResult OnGetGetDoctorById(int id)
        {
            string sMsg = "";
            DoctorsDto oRet = _appointmentsData.GetDoctorById(id, out sMsg);
            if ((TempData["Message"] == null || (TempData["Message"]?.ToString()?.Length ?? 0) == 0) && sMsg != null && sMsg.Trim().Length > 0) TempData["Message"] = sMsg;
            else TempData["Message"] += sMsg;
            sMsg = "";
            return new JsonResult(oRet);
        }
        public JsonResult OnGetDeleteAppointment(int id)
        {
            string sMsg = ""; string sTempMsg = ""; string sAuditDesc = "";

            AppointmentDto oExisting = _appointmentsData.GetAppointmentById(id, out sTempMsg);
            if (oExisting != null)
            {
                if ((TempData["Message"] == null || (TempData["Message"]?.ToString()?.Length ?? 0) == 0) && sTempMsg != null && sTempMsg.Trim().Length > 0) TempData["Message"] = sTempMsg;
                else TempData["Message"] += sTempMsg;
                sTempMsg = "";
            }
            if(oExisting == null)
            {
                TempData["Message"] += "ERROR: The Appointment with the ID = " + id.ToString() + " is not found.";
                return new JsonResult(false);
            }
            sAuditDesc += "ApptTime [old]: " + oExisting?.ApptTime + "\n";
            sAuditDesc += "ResidentKey [old]: " + oExisting?.ResidentKey + "\n";
            sAuditDesc += "DoctorKey [old]: " + oExisting?.DoctorKey + "\n";
            sAuditDesc += "Status [old]: " + oExisting?.Status + "\n";
            sAuditDesc += "LocationID [old]: " + oExisting?.LocationID + "\n";
            sAuditDesc += "DriverKey [old]: " + oExisting?.DriverKey + "\n";
            sAuditDesc += "PrepID [old]: " + oExisting?.PrepID + "\n";
            sAuditDesc += "Depart [old]: " + oExisting?.Depart + "\n";
            sAuditDesc += "ApptType [old]: " + oExisting?.ApptType + "\n";
            sAuditDesc += "Notes [old]: " + oExisting?.Notes + "\n";
            sAuditDesc += "CreatedBy [old]: " + oExisting?.CreatedBy + "\n";
            sAuditDesc += "AddDate [old]: " + oExisting?.AddDate + "\n";
            sAuditDesc += "ModifiedBy [old]: " + oExisting?.ModifiedBy + "\n";
            sAuditDesc += "ModDate [old]: " + oExisting?.ModDate + "\n";



            bool bRet = _appointmentsData.DeleteAppointment(id, out sMsg);
            if (bRet) SaveAudit(GetUserName(), GetIpValue(), "Appointments", "Delete", sAuditDesc);

            if ((TempData["Message"] == null || (TempData["Message"]?.ToString()?.Length ?? 0) == 0) && sMsg != null && sMsg.Trim().Length > 0) TempData["Message"] = sMsg;
            else TempData["Message"] += sMsg;
            sMsg = "";
            return new JsonResult(bRet);
        }
        public StatusCodeResult OnPostSaveAppointment()
        {
            string sAuditDesc = ""; AppointmentDto oExisting = null;
            string sMsg = ""; int iTemp = 0; DateTime dTemp = new DateTime();
            AppointmentDto appointment = new AppointmentDto();
            if (int.TryParse(Request.Form["hdnEventID"], out iTemp)) appointment.ID = iTemp;
            if (appointment.ID > 0)
            {
                //get existing appointment to update
                string sTempMsg = "";
                oExisting = _appointmentsData.GetAppointmentById(appointment.ID, out sTempMsg);
                if (oExisting != null)
                {
                    appointment = oExisting.Clone();
                    if ((TempData["Message"] == null || (TempData["Message"]?.ToString()?.Length ?? 0) == 0) && sTempMsg != null && sTempMsg.Trim().Length > 0) TempData["Message"] = sTempMsg;
                    else TempData["Message"] += sTempMsg;
                    sTempMsg = "";
                }
            }
            if (DateTime.TryParse(Request.Form["txtMDate"] + " " + Request.Form["txtMTime"], out dTemp)) appointment.ApptTime = dTemp;
            if (appointment.ApptTime != oExisting?.ApptTime || oExisting == null)
                sAuditDesc = sAuditDesc + "ApptTime " + (oExisting == null ? "" : "[old]: " + oExisting?.ApptTime) + " [new] " + appointment.ApptTime + "\n";

            if (int.TryParse(Request.Form["hdnMRes"], out iTemp)) appointment.ResidentKey = iTemp;
            if (appointment.ResidentKey != oExisting?.ResidentKey || oExisting == null)
                sAuditDesc = sAuditDesc + "ResidentKey " + (oExisting == null ? "" : "[old]: " + oExisting?.ResidentKey) + " [new] " + appointment.ResidentKey + "\n";
            if (int.TryParse(Request.Form["hdnMDoc"], out iTemp)) appointment.DoctorKey = iTemp;
            if (appointment.DoctorKey != oExisting?.DoctorKey || oExisting == null)
                sAuditDesc = sAuditDesc + "DoctorKey " + (oExisting == null ? "" : "[old]: " + oExisting?.DoctorKey) + " [new] " + appointment.DoctorKey + "\n";
            appointment.Status = Request.Form["cmbMStatus"] ;
            if (appointment.Status != oExisting?.Status || oExisting == null)
                sAuditDesc = sAuditDesc + "Status " + (oExisting == null ? "" : "[old]: " + oExisting?.Status) + " [new] " + appointment.Status + "\n";
            if (int.TryParse(Request.Form["cmbMAddr"], out iTemp))  appointment.LocationID = iTemp;
            if (appointment.LocationID != oExisting?.LocationID || oExisting == null)
                sAuditDesc = sAuditDesc + "LocationID " + (oExisting == null ? "" : "[old]: " + oExisting?.LocationID) + " [new] " + appointment.LocationID + "\n";
            if (int.TryParse(Request.Form["hdnMDriver"], out iTemp)) appointment.DriverKey = (iTemp == 0? null : iTemp);
            if (appointment.DriverKey != oExisting?.DriverKey || oExisting == null)
                sAuditDesc = sAuditDesc + "DriverKey " + (oExisting == null ? "" : "[old]: " + oExisting?.DriverKey) + " [new] " + appointment.DriverKey + "\n";
            if (int.TryParse(Request.Form["cmbMCar"], out iTemp)) appointment.CarNum = (iTemp == 0 ? null : iTemp);
            if (appointment.CarNum != oExisting?.CarNum || oExisting == null)
                sAuditDesc = sAuditDesc + "CarNum " + (oExisting == null ? "" : "[old]: " + oExisting?.CarNum) + " [new] " + appointment.CarNum + "\n";
            if (int.TryParse(Request.Form["cmbMPrep"], out iTemp)) appointment.PrepID = (iTemp == 0 ? null : iTemp);
            if (appointment.PrepID != oExisting?.PrepID || oExisting == null)
                sAuditDesc = sAuditDesc + "PrepID " + (oExisting == null ? "" : "[old]: " + oExisting?.PrepID) + " [new] " + appointment.PrepID + "\n";
            string sTemp = Request.Form["txtPickup"];
            if (sTemp == null || sTemp.Trim().Length == 0 || !DateTime.TryParse("1899-12-30 " + Request.Form["txtPickup"], out dTemp))
                appointment.Depart = null;
            else appointment.Depart = dTemp;
            if (appointment.Depart != oExisting?.Depart || oExisting == null)
                sAuditDesc = sAuditDesc + "Depart " + (oExisting == null ? "" : "[old]: " + oExisting?.Depart) + " [new] " + appointment.Depart + "\n";

            appointment.ApptType = Request.Form["cmbMApptType"];
            if (appointment.ApptType != oExisting?.ApptType || oExisting == null)
                sAuditDesc = sAuditDesc + "ApptType " + (oExisting == null ? "" : "[old]: " + oExisting?.ApptType) + " [new] " + appointment.ApptType + "\n";
            appointment.Notes = Request.Form["txtMNotes"];
            if (appointment.Notes != oExisting?.Notes || oExisting == null)
                sAuditDesc = sAuditDesc + "Notes " + (oExisting == null ? "" : "[old]: " + oExisting?.Notes) + " [new] " + appointment.Notes + "\n";

            appointment.MakeAppointment = Request.Form["chkMakeAppointment"].Equals("on");
            if (appointment.MakeAppointment != oExisting?.MakeAppointment || oExisting == null)
                sAuditDesc = sAuditDesc + "MakeAppointment " + (oExisting == null ? "" : "[old]: " + oExisting?.MakeAppointment) + " [new] " + appointment.MakeAppointment + "\n";
            appointment.ConfirmedAppointment = Request.Form["chkConfirmedAppointment"].Equals("on");
            if (appointment.ConfirmedAppointment != oExisting?.ConfirmedAppointment || oExisting == null)
                sAuditDesc = sAuditDesc + "ConfirmedAppointment " + (oExisting == null ? "" : "[old]: " + oExisting?.ConfirmedAppointment) + " [new] " + appointment.ConfirmedAppointment + "\n";
            appointment.NursesAideAccompaniment = Request.Form["chkNursesAideAccompaniment"].Equals("on");
            if (appointment.NursesAideAccompaniment != oExisting?.NursesAideAccompaniment || oExisting == null)
                sAuditDesc = sAuditDesc + "NursesAideAccompaniment " + (oExisting == null ? "" : "[old]: " + oExisting?.NursesAideAccompaniment) + " [new] " + appointment.NursesAideAccompaniment + "\n";
            appointment.Wait = Request.Form["chkWait"].Equals("on");
            if (appointment.Wait != oExisting?.Wait || oExisting == null)
                sAuditDesc = sAuditDesc + "Wait " + (oExisting == null ? "" : "[old]: " + oExisting?.Wait) + " [new] " + appointment.Wait + "\n";
            appointment.InHouseVisit = Request.Form["chkInHouseVisit"].Equals("on");
            if (appointment.InHouseVisit != oExisting?.InHouseVisit || oExisting == null)
                sAuditDesc = sAuditDesc + "InHouseVisit " + (oExisting == null ? "" : "[old]: " + oExisting?.InHouseVisit) + " [new] " + appointment.InHouseVisit + "\n";


            if (appointment.ID == 0)
            {
                appointment.CreatedBy = GetUserName();
                if (appointment.CreatedBy  != oExisting?.CreatedBy || oExisting == null)
                    sAuditDesc = sAuditDesc + "CreatedBy " + (oExisting == null ? "" : "[old]: " + oExisting?.CreatedBy) + " [new] " + appointment.CreatedBy + "\n";
                appointment.AddDate = DateTime.Now;
                if (appointment.AddDate != oExisting?.AddDate || oExisting == null)
                    sAuditDesc = sAuditDesc + "AddDate " + (oExisting == null ? "" : "[old]: " + oExisting?.AddDate) + " [new] " + appointment.AddDate + "\n";
            }
            appointment.ModifiedBy = GetUserName();
            if (appointment.ModifiedBy != oExisting?.ModifiedBy || oExisting == null)
                sAuditDesc = sAuditDesc + "ModifiedBy " + (oExisting == null ? "" : "[old]: " + oExisting?.ModifiedBy) + " [new] " + appointment.ModifiedBy + "\n";
            appointment.ModDate = DateTime.Now;
            if (appointment.ModDate != oExisting?.ModDate || oExisting == null)
                sAuditDesc = sAuditDesc + "ModDate " + (oExisting == null ? "" : " [old]: " + oExisting?.ModDate) + " [new] " + appointment.ModDate + "\n";

            bool bRet = _appointmentsData.SaveAppointment(appointment, out sMsg);
            if (bRet) SaveAudit(GetUserName(), GetIpValue(), "Appointments", appointment.ID == 0 ? "Create" : "Update", sAuditDesc);
            if ((TempData["Message"] == null || (TempData["Message"]?.ToString()?.Length ?? 0) == 0) && sMsg != null && sMsg.Trim().Length > 0) TempData["Message"] = sMsg;
            else TempData["Message"] += sMsg;
            sMsg = "";

            return new OkResult();
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

                    _appointmentsData.SaveAudit(audit, out string sMsg);
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
