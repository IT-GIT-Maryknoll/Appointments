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

        private string sFilter = "";

        public void OnGet()
        {
            string sMsg = "";

            UserInfo = GetUserInfo(GetUserName());
            if (lstAppointmentTypes.Count() == 0)
            {
                lstAppointmentTypes = _appointmentsData.LoadAppointmentTypes(out sMsg);
                if ((TempData["Message"] == null || (TempData["Message"]?.ToString()?.Length ?? 0) == 0) && sMsg != null && sMsg.Trim().Length > 0) TempData["Message"] = sMsg;
                else TempData["Message"] = sMsg;
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
            //lstAppointments = _appointmentsData.LoadAppointments(sFilter, out sMsg);
            //if ((TempData["Message"] == null || (TempData["Message"]?.ToString()?.Length ?? 0) == 0) && sMsg != null && sMsg.Trim().Length > 0) TempData["Message"] = sMsg;
            //else TempData["Message"] += sMsg;
            //sMsg = "";

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
                sFilter += (sFilter.Length > 0 ? sWord : "") + " [ResidentKey] in (SELECT ResidentKey FROM qryAppointmentNameList where Upper(FullName) like Upper('%" + sTemp.Trim().Replace("'", "''") + "%'))";
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
                sFilter += (sFilter.Length > 0 ? sWord : "") + " [ResidentKey] in (SELECT ResidentKey FROM qryAppointmentNameList where Upper(FullName) like Upper('%" + sTemp.Trim().Replace("'", "''") + "%'))";
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
                        color = "#378006";
                        break;
                    case "Consult":
                        color = "#0078D7";
                        break;
                    case "In-house":
                        color = "#FF8C00";
                        break;
                    case "Diagnostics":
                        color = "#9B111E";
                        break;
                    default:
                        color = "#434343";
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
                    Subject = $"<span style='font-size:16px;'><span style=' font-weight:bold; text-decoration: underline;'>{((DateTime)item.ApptTime).ToShortTimeString()} {item.FullName}<br/> - {item.DoctorName}</span><br/><span style='font-style:normal;'>{item.City},<br/> {item.Phone} {item.ApptType}<br/>{item.DriverName} Depart {dpt}</span><br/></span>",
                    //Start = DateTime.Now.ToString("o"),
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
    }
}
