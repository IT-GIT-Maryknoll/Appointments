using System;
using System.Collections.Generic;
using System.Configuration;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

using Appointments.Database.Dto;
using Appointments.Database.Interfaces;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace Appointments.Pages
    {
    public class AppointmentDetailsModel(IConfiguration configuration,IAppointmentsRepository appointmentsData):PageModel
        {
        public UserDto UserInfo;
        private readonly IConfiguration _configuration = configuration;
        public List<AppointmentDto> lstAppointments = [];
        private readonly IAppointmentsRepository _appointmentsData = appointmentsData;
        private string sFilter = "";
        public string lblDate = "";
        public string hdnResident = "";
        public string txtResident = "";
        public string hdnDoctor = "";
        public string txtDoctor = "";
        public string cmbApptType = "";
        public string bFilter = "false";
        public bool ShowInhouse { get; set; }

        public void OnGet()
            {
            string sMsg = "";

            UserInfo=GetUserInfo(GetUserName());
            }

        public UserDto GetUserInfo(string sUser)
            {
            if(UserInfo is object)
                return UserInfo;
            using(var context = new PrincipalContext(ContextType.Domain))
                {
                var userPrincipal = UserPrincipal.FindByIdentity(context,IdentityType.SamAccountName,sUser);
                if(userPrincipal!=null)
                    {
                    DirectoryEntry? directoryEntry = userPrincipal.GetUnderlyingObject() as DirectoryEntry;
                    UserDto oRet = new UserDto();
                    oRet.sAMAccountName=sUser;
                    oRet.FirstName=userPrincipal.GivenName;
                    oRet.MI=userPrincipal.MiddleName;
                    oRet.LastName=userPrincipal.Surname;
                    oRet.DisplayName=userPrincipal.DisplayName;
                    oRet.MbrEmpIndicator=directoryEntry.Properties["company"].Value==null ? "" : directoryEntry.Properties["company"].Value.ToString();
                    oRet.MbrEmpNumber=userPrincipal.EmployeeId;
                    if(oRet.MbrEmpNumber==null)
                        oRet.MbrEmpNumber=(directoryEntry.Properties["title"].Value==null ? "" : directoryEntry.Properties["title"].Value.ToString());
                    oRet.EmailAddress=userPrincipal.EmailAddress;
                    oRet.ADSDescription=userPrincipal.Description;
                    oRet.msExchHideFromAddressLists=(directoryEntry.Properties["msExchHideFromAddressLists"].Value==null ? false : directoryEntry.Properties["msExchHideFromAddressLists"].Value.ToString().ToLower().Equals("true"));
                    if(oRet.msExchHideFromAddressLists)
                        oRet.EmailAddress="";

                    UserInfo=oRet;
                    return oRet;
                    }
                }
            return null;
            }

        private string GetUserName()
            {
            if(HttpContext==null)
                return null;

            var userName = HttpContext.User.Identity.Name;
            string sUserConfig = configuration.GetValue<string>("UName");
            if(sUserConfig!=null&&sUserConfig.Trim().Length>0)
                userName=sUserConfig;
            if(string.IsNullOrEmpty(userName)||userName.Length<0)
                {
                return "";
                }
            else
                {
                var arrName = userName.Split("\\");
                return arrName[1];
                }
            }

        public async Task<JsonResult> OnGetGetAppointments(string start,string end)
            {
            string sMsg = "";
         
            if(!DateTimeOffset.TryParse(start,CultureInfo.InvariantCulture,DateTimeStyles.AssumeUniversal,out var startDt)||
           !DateTimeOffset.TryParse(end,CultureInfo.InvariantCulture,DateTimeStyles.AssumeUniversal,out var endDt))
                {
                return new JsonResult(new { error = "Invalid date format" });
                }

            DateTime? startOfDayFilter = startDt.UtcDateTime.Date;
            DateTime? endofDayFilter = endDt.UtcDateTime.Date;

            sFilter=$"[ApptTime] >= '{startOfDayFilter:yyyy-MM-dd HH:mm:ss}' And [ApptTime] <= '{endofDayFilter:yyyy-MM-dd HH:mm:ss}'";
            sFilter=sFilter+"And [Status]='Booked'";
            sFilter=sFilter+"And [FullName] Is Not Null";
            //if(sFilter==null)
            //    GetFilterIn();
            lstAppointments=_appointmentsData.LoadAppointments(sFilter,out sMsg);
            if((TempData["Message"]==null||(TempData["Message"]?.ToString()?.Length??0)==0)&&sMsg!=null&&sMsg.Trim().Length>0)
                TempData["Message"]=sMsg;
            else
                TempData["Message"]+=sMsg;
            sMsg="";
            var list = new List<object>();
            if(lstAppointments==null||lstAppointments.Count==0)
                {
                return new JsonResult(new List<object>());

                //log error
                }
            else
                {
                var apptDetailsList = lstAppointments.Select(d =>
                {
                    var departDayTime = d.Depart!=null ? d.ApptTime.GetValueOrDefault().Date+d.Depart.GetValueOrDefault().TimeOfDay : d.ApptTime.GetValueOrDefault().Date+d.ApptTm.GetValueOrDefault().TimeOfDay;
                    bool inHouse = string.Equals(d.ApptType,"In-House",StringComparison.OrdinalIgnoreCase)
                    ||
                    (string.IsNullOrEmpty(d.ApptType)&&string.Equals(d.Notes,"In House",StringComparison.OrdinalIgnoreCase));
                    return new
                        {
                        id = (int)d.ID,
                        title = d.FullName,
                        start = departDayTime,
                        end = d.ApptTm,
                        //start = start.ToString("yyyy-MM-ddTHH:mm:ss"),
                        //end = end.ToString("yyyy-MM-ddTHH:mm:ss"),
                        // backgroundColor = bgColor,
                        //textColor = "#000000",

                        extendedProps = new
                            {
                            apptime = d.ApptTm,
                            nursenotes = d.Notes,
                            wait = d.Wait,
                            badgeText = inHouse ? "I" : "N",
                            badgeClass = inHouse ? "badge-danger" : "",
                            inhouse = inHouse,
                            flag = inHouse ? false : true,
                            doctorName = d.DoctorName,
                            doctorAddress = d.Street+" "+d.City,
                            //badgeText="IH",
                            //        badgeClass="badge-danger",
                            //location = d.Description,
                            //description = d.OwnerID,
                            //starttime = departDayTime.ToString("hh:mm tt",CultureInfo.InvariantCulture),
                            //endTime = d.ApptTime.Value.ToString("hh:mm tt",CultureInfo.InvariantCulture),
                            }
                        };
                }).ToList();

                return new JsonResult(apptDetailsList);
                }

            //foreach(var item in lstAppointments)
            //    {
            //    var dpt = "";
            //    var color = "";
            //    switch(item.ApptType)
            //        {
            //        case "Follow-up":
            //            color="#c4eda9";  // "#378006";
            //            break;

            //        case "Consult":
            //            color="#afd5f5"; // "#0078D7";
            //            break;

            //        case "In-house":
            //            color="#ffcd90";  // "#FF8C00";
            //            break;

            //        case "Diagnostic":
            //            color="#ffabb3";  // "#9B111E";
            //            break;

            //        default:
            //            color="#c7c7c7";  // "#434343";
            //            break;
            //        }
            //    if(item.ApptTime==null)
            //        item.ApptTime=DateTime.MinValue;
            //    if(item.Depart.HasValue)
            //        dpt=((DateTime)item.Depart).ToShortTimeString();
            //    var displayEvent = new
            //        {
            //        //Description = $"<span style='font-size:16px;'>{((DateTime)item.ApptTime).ToShortTimeString()} {item.FullName}<br/> - {item.DoctorName}<br/>{item.City},<br/> {item.Phone} {item.ApptType}<br/>{item.DriverName} Depart {dpt}<br/></span>",
            //        //Description=$"abc",
            //        End = "",
            //        EventID = item.ID,
            //        IsFullDay = false,
            //        Subject = $"<span style='font-size:16px;color:black;'><span style=' font-weight:bold; text-decoration: underline;'>{((DateTime)item.ApptTm).ToShortTimeString()} {item.FullName}<br/> - {item.DoctorName}</span><br/><span style='font-style:normal;'>{item.City},<br/> {item.Phone} {item.ApptType}<br/>{item.DriverName} Depart {dpt}</span><br/></span>",
            //        Start = item.ApptTime==null ? "" : String.Format("{0:u}",((DateTime)item.ApptTime).AddDays(1)),
            //        ThemeColor = color
            //        };
            //    list.Add(displayEvent);
            //    }
            }

        private void GetFilterIn()
            {
            sFilter="";
            string sTemp = "";
            ViewData.Clear();
            string sWord = " and ";
            sTemp=lblDate;
            if(sTemp==null||sTemp.Trim().Length==0)
                sTemp=DateTime.Now.Year+"-"+DateTime.Now.Month.ToString().PadLeft(2,'0')+"-"+DateTime.Now.Day.ToString().PadLeft(2,'0');
            else
                sTemp=sTemp.Trim()+"-01";
            sFilter+=(sFilter.Length>0 ? sWord : "")+" MONTH([ApptTime]) = MONTH('"+sTemp+"') AND YEAR([ApptTime]) = YEAR('"+sTemp+"')";
            ViewData.Add("lblDate",System.Web.HttpUtility.HtmlDecode(sTemp));

            sTemp=hdnResident;
            if(sTemp!=null&&sTemp.Trim().Length>0)
                {
                sFilter+=(sFilter.Length>0 ? sWord : "")+" [ResidentKey] = "+sTemp;
                ViewData.Add("hdnResident",System.Web.HttpUtility.HtmlDecode(sTemp));
                }
            sTemp=txtResident;
            if(sTemp!=null&&sTemp.Trim().Length>0&&(hdnResident is null||hdnResident.Trim().Length==0))
                {
                sFilter+=(sFilter.Length>0 ? sWord : "")+" [ResidentKey] in (SELECT ResidentKey FROM qryAppointmentNameListChina where Upper(FullName) like Upper('%"+sTemp.Trim().Replace("'","''")+"%'))";
                ViewData.Add("txtResident",System.Web.HttpUtility.HtmlDecode(sTemp));
                }

            sTemp=hdnDoctor;
            if(sTemp!=null&&sTemp.Trim().Length>0)
                {
                sFilter+=(sFilter.Length>0 ? sWord : "")+" [DoctorKey] = "+sTemp;
                ViewData.Add("hdnDoctor",System.Web.HttpUtility.HtmlDecode(sTemp));
                }
            sTemp=txtDoctor;
            if(sTemp!=null&&sTemp.Trim().Length>0&&(hdnDoctor is null||hdnDoctor.Trim().Length==0))
                {
                sFilter+=(sFilter.Length>0 ? sWord : "")+" [DoctorKey] in (SELECT DoctorKey FROM qryDoctors where Upper(DoctorName) like Upper('%"+sTemp.Trim().Replace("'","''")+"%'))";
                ViewData.Add("txtDoctor",System.Web.HttpUtility.HtmlDecode(sTemp));
                }
            sTemp=cmbApptType;
            if(sTemp!=null&&sTemp.Trim().Length>0)
                {
                sFilter+=(sFilter.Length>0 ? sWord : "")+" ApptType = '"+sTemp.Trim().Replace("'","''")+"'";
                ViewData.Add("cmbApptType",System.Web.HttpUtility.HtmlDecode(sTemp));
                }
            ViewData["filter"]=JsonSerializer.Serialize(ViewData);
            HttpContext.Session.SetString("sFilter",sFilter);
            }
        }
    }