using Appointments.Database.Context;
using Appointments.Database.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Appointments.Database.Interfaces
{
    public interface IGroupsRepository
    {
        Task<List<SecurityGroupsDto>> GetGroups();
        Task<List<SecurityUsersDto>> GetUsers(int GroupID);
        Task<List<SecurityUsersDto>> GetMembers(int GroupID);
        bool RemoveMembers(int GroupID, string loginList);
        bool AddMembers(int GroupID, string loginList);
        bool ChangeGroupList(int iGroupID, string sLabel, bool bDelete);
    }
}