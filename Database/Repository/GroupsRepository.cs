using Appointments.Database.Context;
using Appointments.Database.Dto;
using Appointments.Database.Interfaces;
using Dapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Appointments.Database.Repository
{
    public class GroupsRepository: IGroupsRepository
    {
        private readonly DbContext _context;
        private readonly ILogger<GroupsRepository> _logger;
        public GroupsRepository(ILogger<GroupsRepository> logger, DbContext context)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<List<SecurityGroupsDto>> GetGroups()
        {
            try
            {
                var query = @"SELECT [GroupID],	[Label] FROM [dbo].[tbl_groups] ORDER BY [dbo].[tbl_groups].GroupID";
                using var connection = _context.CreateConnection();
                var dioList = await connection.QueryAsync<SecurityGroupsDto>(query);
                return (List<SecurityGroupsDto>)dioList;
            }
            catch (Exception e)
            {
                _logger.LogError("Error occurred getting group list", e);
                throw new SystemException("ERROR: " + e.Message + " " + (e.InnerException == null ? "" : e.InnerException.Message));
            }
        }
        public async Task<List<SecurityUsersDto>> GetUsers(int GroupID)
        {
            try
            {
                var query = @"SELECT u.LastName, u.FirstName, u.Middle, u.displayName, u.sAMAccountName, u.Employee_Member_Ind, u.Employee_Member_Number 
					FROM [MKL_Apostolic_projects].[dbo].[vw_AP_SEC_SystemUsers] u WHERE u.sAMAccountName not in (SELECT UserName FROM [dbo].[tbl_groupmembers] where GroupID = " +
                    GroupID + ") ORDER BY displayName";
                using var connection = _context.CreateConnection();
                var dioList = await connection.QueryAsync<SecurityUsersDto>(query);
                return (List<SecurityUsersDto>)dioList;
            }
            catch (Exception e)
            {
                _logger.LogError("Error occurred getting security member list", e);
                throw new SystemException("ERROR: " + e.Message + " " + (e.InnerException == null ? "" : e.InnerException.Message));
            }
        }
        public async Task<List<SecurityUsersDto>> GetMembers(int GroupID)
        {
            try
            {
                var query = @"SELECT u.LastName, u.FirstName, u.Middle, u.displayName, u.sAMAccountName, u.Employee_Member_Ind, u.Employee_Member_Number 
					FROM [MKL_Apostolic_projects].[dbo].[vw_AP_SEC_SystemUsers] u JOIN [dbo].[tbl_groupmembers] m on u.sAMAccountName = m.UserName where m.GroupID = " +
                    GroupID + " ORDER BY displayName";
                using var connection = _context.CreateConnection();
                var dioList = await connection.QueryAsync<SecurityUsersDto>(query);
                return (List<SecurityUsersDto>)dioList;
            }
            catch (Exception e)
            {
                _logger.LogError("Error occurred getting security user list", e);
                throw new SystemException("ERROR: " + e.Message + " " + (e.InnerException == null ? "" : e.InnerException.Message));
            }
        }
        public bool RemoveMembers(int GroupID, string loginList)
        {
            if (loginList == null || loginList.Trim().Length == 0) return true;
            try
            {
                loginList = loginList.Replace(" ", "");
                loginList = "'" + loginList.Replace(",", "','") + "'";

                var query = @"delete from [dbo].[tbl_groupmembers] where UserName in (" + loginList + ") and GroupID = " + GroupID;
                using var connection = _context.CreateConnection();
                connection.Query(query);
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError("Error occurred removing group members from the group " + GroupID.ToString(), e);
                throw new SystemException("ERROR: " + e.Message + " " + (e.InnerException == null ? "" : e.InnerException.Message));
            }
        }
        public bool AddMembers(int GroupID, string loginList)
        {
            if (loginList == null || loginList.Trim().Length == 0) return true;
            try
            {
                loginList = loginList.Replace(" ", "");
                string[] loginNames = loginList.Split(',');

                using var connection = _context.CreateConnection();
                foreach (string sName in loginNames)
                {
                    string sN = sName.Trim().ToLower();
                    var query = @"insert into [dbo].[tbl_groupmembers] ([UserName],[GroupID]) select '" + sN + "', " + GroupID.ToString() +
                        " where not exists (select 1 from  [dbo].[tbl_groupmembers] where UserName = '" + sN + "' and GroupID = " + GroupID.ToString() + ")";
                    connection.Query(query);
                }

                return true;
            }
            catch (Exception e)
            {
                _logger.LogError("Error occurred adding group members to the group " + GroupID.ToString(), e);
                throw new SystemException("ERROR: " + e.Message + " " + (e.InnerException == null ? "" : e.InnerException.Message));
            }
        }
        public bool ChangeGroupList(int iGroupID, string sLabel, bool bDelete)
        {
            try
            {
                var query = "";
                using var connection = _context.CreateConnection();

                if (bDelete && iGroupID > 0)
                {
                    query = @"delete from [dbo].[tbl_groupmembers] where GroupID = " + iGroupID.ToString();
                    connection.Query(query);
                    query = @"delete from [dbo].[tbl_groups] where GroupID = " + iGroupID.ToString();
                    connection.Query(query);
                    return true;
                }
                else if (iGroupID == 0)
                {
                    query = @"insert into [dbo].[tbl_groups] (Label) values ('" + sLabel + "')";
                    connection.Query(query);
                    return true;
                }
                else if (iGroupID > 0 && sLabel != null && sLabel.Trim().Length > 0)
                {
                    query = @"update [dbo].[tbl_groups] set Label = '" + sLabel + "' where GroupID = " + iGroupID.ToString();
                    connection.Query(query);
                    return true;

                }
                else return false;
            }
            catch (Exception e)
            {
                _logger.LogError("Error occurred while updating the group " + iGroupID.ToString(), e);
                throw new SystemException("ERROR: " + e.Message + " " + (e.InnerException == null ? "" : e.InnerException.Message));
            }

        }

    }
}
