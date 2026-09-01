using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using Appointments.Database.Dto;
using Appointments.Database.Interfaces;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Appointments.Pages
    {
    public class AppointmentDetailsModel(ILogger<AppointmentDetailsModel> logger,IConfiguration configuration,IAppointmentsRepository appointmentsData,IPermissionsRepository permissionsRepository):
        BasePageModel(logger,configuration,permissionsRepository)
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
        public bool SharedMessage { get; set; }
        public bool canSeeAll { get; set; }

        public bool adminCheck;

        public string sMask = "";

        public void OnGet()
            {
            OnGetBase();
            sMask=ViewData["AppointmentDetailsAccessMask"]?.ToString()??"";
            }

        public async Task<JsonResult> OnGetAppointments(string start,string end,bool sharedMessage,string driverName)
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
            if(!sharedMessage)
                {
                sFilter+=$" And [DriverName] Like '%{driverName}%'";
                }
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

                    bool inHouse = sharedMessage ? (string.Equals(d.ApptType,"In-House",StringComparison.OrdinalIgnoreCase)||(string.IsNullOrEmpty(d.ApptType)&&string.Equals(d.Notes,"In House",StringComparison.OrdinalIgnoreCase))) : false;

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
                            driverName = d.DriverName,
                            notes = d.Notes,
                            nursesaid=d.NursesAideAccompaniment==true ? "Yes" : "No",
                            }
                        };
                }).ToList();

                return new JsonResult(apptDetailsList);
                }
            }
        }
    }