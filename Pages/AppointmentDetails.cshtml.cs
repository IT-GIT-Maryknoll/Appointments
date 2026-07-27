using Appointments.Database.Dto;
using Appointments.Database.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Appointments.Pages
    {
    public class AppointmentDetailsModel(ILogger<AppointmentDetailsModel> logger, IConfiguration configuration,IAppointmentsRepository appointmentsData, IPermissionsRepository permissionsRepository) :
        BasePageModel(logger, configuration, permissionsRepository)
    {
        //public UserDto UserInfo;
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

        public string sMask = "";

        public void OnGet()
            {
            OnGetBase();
            sMask = ViewData["AppointmentDetailsAccessMask"]?.ToString() ?? "";
            //UserInfo=GetUserInfo(GetUserName());

        }

        //public UserDto GetUserInfo(string sUser)
        //    {
        //    if(UserInfo is object)
        //        return UserInfo;
        //    using var context = new PrincipalContext(ContextType.Domain);
        //    var userPrincipal = UserPrincipal.FindByIdentity(context,IdentityType.SamAccountName,sUser);
        //    if(userPrincipal!=null)
        //        {
        //        DirectoryEntry? directoryEntry = userPrincipal.GetUnderlyingObject() as DirectoryEntry;
        //        UserDto oRet = new UserDto();
        //        oRet.sAMAccountName=sUser;
        //        oRet.FirstName=userPrincipal.GivenName;
        //        oRet.MI=userPrincipal.MiddleName;
        //        oRet.LastName=userPrincipal.Surname;
        //        oRet.DisplayName=userPrincipal.DisplayName;
        //        oRet.MbrEmpIndicator=directoryEntry.Properties["company"].Value==null ? "" : directoryEntry.Properties["company"].Value.ToString();
        //        oRet.MbrEmpNumber=userPrincipal.EmployeeId;
        //        if(oRet.MbrEmpNumber==null)
        //            oRet.MbrEmpNumber=(directoryEntry.Properties["title"].Value==null ? "" : directoryEntry.Properties["title"].Value.ToString());
        //        oRet.EmailAddress=userPrincipal.EmailAddress;
        //        oRet.ADSDescription=userPrincipal.Description;
        //        oRet.msExchHideFromAddressLists=(directoryEntry.Properties["msExchHideFromAddressLists"].Value==null ? false : directoryEntry.Properties["msExchHideFromAddressLists"].Value.ToString().ToLower().Equals("true"));
        //        if(oRet.msExchHideFromAddressLists)
        //            oRet.EmailAddress="";

        //        UserInfo=oRet;
        //        return oRet;
        //        }
        //    return null;
        //    }

        //private string GetUserName()
        //    {
        //    if(HttpContext==null)
        //        return null;

        //    var userName = HttpContext.User.Identity.Name;
        //    string sUserConfig = configuration.GetValue<string>("UName");
        //    if(sUserConfig!=null&&sUserConfig.Trim().Length>0)
        //        userName=sUserConfig;
        //    if(string.IsNullOrEmpty(userName)||userName.Length<0)
        //        {
        //        return "";
        //        }
        //    else
        //        {
        //        var arrName = userName.Split("\\");
        //        return arrName[1];
        //        }
        //    }

        public async Task<JsonResult> OnGetGetAppointments(string start,string end)
            {
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
            lstAppointments=_appointmentsData.LoadAppointments(sFilter,out string sMsg);
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
                        extendedProps = new
                            {
                            apptime = d.ApptTm,
                            nursenotes = d.Notes,
                            wait = d.Wait==true ? "Yes" : "No",
                            badgeText = inHouse ? "I" : "N",
                            badgeClass = inHouse ? "badge-danger" : "",
                            inhouse = inHouse,
                            flag = inHouse ? false : true,
                            doctorName = d.DoctorName,
                            doctorAddress = d.Street+" "+d.City,
                            driverName = d.DriverName
                            }
                        };
                }).ToList();

                return new JsonResult(apptDetailsList);
                }
            }
        }
    }