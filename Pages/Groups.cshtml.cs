using Appointments.Database.Dto;
using Appointments.Database.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Configuration;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Threading.Tasks;

namespace Appointments.Pages
{
    public class GroupsModel(ILogger<GroupsModel> logger, IConfiguration configuration, IGroupsRepository groupsRepository, IPermissionsRepository permissionsRepository) : 
        BasePageModel(logger, configuration, permissionsRepository)
    {
        private readonly ILogger<GroupsModel> _logger = logger;
        private readonly IConfiguration _configuration = configuration;
        private readonly IGroupsRepository _groupsRepository = groupsRepository;
        private readonly IPermissionsRepository _permissionsRepository = permissionsRepository;

        public void OnGet()
        {
            OnGetBase();
        }

        public async Task<JsonResult> OnGetGetGroups()
        {
            var groupList = await _groupsRepository.GetGroups();
            return new JsonResult(groupList.ToArray());
        }
        public async Task<JsonResult> OnGetGetUsers(int GroupID)
        {
            var userList = await _groupsRepository.GetUsers(GroupID);
            return new JsonResult(userList.ToArray());
        }
        public async Task<JsonResult> OnGetGetMembers(int GroupID)
        {
            var userList = await _groupsRepository.GetMembers(GroupID);
            return new JsonResult(userList.ToArray());
        }
        public JsonResult OnGetAddMembers(int GroupID, string loginList)
        {
            var Res = _groupsRepository.AddMembers(GroupID, loginList);
            return new JsonResult(Res);
        }
        public JsonResult OnGetRemoveMembers(int GroupID, string loginList)
        {
            var Res = _groupsRepository.RemoveMembers(GroupID, loginList);
            return new JsonResult(Res);
        }
        public JsonResult OnGetChangeGroupList(int GroupID, string sLabel, bool bDelete)
        {
            var Res = _groupsRepository.ChangeGroupList(GroupID, sLabel, bDelete);
            return new JsonResult(Res);
        }
    }
}
