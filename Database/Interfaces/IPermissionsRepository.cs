using Appointments.Database.Dto;
using System.Collections.Generic;

namespace Appointments.Database.Interfaces
{
    public interface IPermissionsRepository
    {
        List<SecurityGroupsDto> GetGroups(out string sMsg);
        List<PermissionsDto> GetPermissionList(int groupID, out string sMsg);
        bool SavePermissions(int groupID, List<PermissionsDto> permList, out string sMsg);
        public string GetAccessMask(string UserName, string TableName, out string sMsg);
        bool IsUserAdmin(string sAMAccountName, out string sMsg);
    }
}
