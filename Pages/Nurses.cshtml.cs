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
    public class NursesModel(ILogger<DoctorsModel> logger, IConfiguration configuration, IStaffRepository nursesData, IPermissionsRepository permissionsRepository) : 
        BasePageModel(logger, configuration, permissionsRepository)
    {
        private readonly ILogger<DoctorsModel> _logger = logger;
        private readonly IConfiguration _configuration = configuration;
        private readonly IStaffRepository _nursesData = nursesData;

        public List<StaffDto> lstNurses = new List<StaffDto>();
        public string sMask = "";

        public void OnGet()
        {
            OnGetBase();
            sMask = ViewData["NursesAccessMask"]?.ToString() ?? "";
            lstNurses = _nursesData.LoadNurses(out sMsg);

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
