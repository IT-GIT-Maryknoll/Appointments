using Appointments.Database.Dto;
using Appointments.Database.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;

namespace Appointments.Pages
{
    public class PermissionsModel(ILogger<PermissionsModel> logger, IConfiguration config, IPermissionsRepository permissionsRepository) : BasePageModel(logger, config, permissionsRepository)
    {
        private readonly ILogger<PermissionsModel> _logger = logger;
        private readonly IConfiguration _configuration=config;
        private readonly IPermissionsRepository _permissionsRepository=permissionsRepository;

        public List<SecurityGroupsDto> lstGroups { get; private set; } = new List<SecurityGroupsDto>();
       // public List<string> lstTables { get; private set; } = new List<string>();

        public void OnGet()
        {
            base.OnGetBase();
            lstGroups = _permissionsRepository.GetGroups(out string sMsg);
            if ((TempData["Message"] == null || (TempData["Message"]?.ToString()?.Length ?? 0) == 0) && sMsg != null && sMsg.Trim().Length > 0) TempData["Message"] = sMsg;
            else TempData["Message"] += sMsg;
            sMsg = "";

        }
        public JsonResult OnGetGetPermissionList(int GroupId)
        {
            string sMsg = "";
            List<PermissionsDto> oRet = _permissionsRepository.GetPermissionList(GroupId, out sMsg);
            if ((TempData["Message"] == null || (TempData["Message"]?.ToString()?.Length ?? 0) == 0) && sMsg != null && sMsg.Trim().Length > 0) TempData["Message"] = sMsg;
            else TempData["Message"] += sMsg;
            sMsg = "";
            return new JsonResult(oRet);
        }
        public JsonResult OnGetSavePermissions(int GroupID, string PermList)
        {
            string sMsg = "";
            List <PermissionsDto> oPermList = new List<PermissionsDto>();
            if (PermList != null && PermList.Trim().Length > 0)
            {
                try
                {
                    Dictionary<string, string> dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(PermList);
                    foreach(var o in dict)
                    {
                        PermissionsDto perm = new PermissionsDto();
                        perm.TableName = o.Key.Replace("[", "").Replace("]", "").Trim();
                        perm.AccessMask = o.Value.Trim();
                        perm.GroupID = GroupID;
                        oPermList.Add(perm);
                    }
                }
                catch (Exception ex)
                {
                    sMsg = "ERROR: " + ex.Message + " " + (ex.InnerException == null ? "" : ex.InnerException.Message);
                    _logger.LogError("Error occurred deserializing permission list", ex);
                    return new JsonResult(false);
                }
            }
            bool bRes = _permissionsRepository.SavePermissions(GroupID, oPermList, out sMsg);
            if ((TempData["Message"] == null || (TempData["Message"]?.ToString()?.Length ?? 0) == 0) && sMsg != null && sMsg.Trim().Length > 0) TempData["Message"] = sMsg;
            else TempData["Message"] += sMsg;
            sMsg = "";
            return new JsonResult(bRes);
        }

    }
}
