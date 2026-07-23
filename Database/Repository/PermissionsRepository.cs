using Appointments.Database.Context;
using Appointments.Database.Dto;
using Appointments.Database.Interfaces;
using Dapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Appointments.Database.Repository
{
    public class PermissionsRepository : IPermissionsRepository
    {
        private readonly DbContext _context;
        private readonly ILogger<PermissionsRepository> _logger;
        public PermissionsRepository(ILogger<PermissionsRepository> logger, DbContext context)
        {
            _context = context;
            _logger = logger;
        }

        public List<SecurityGroupsDto> GetGroups(out string sMsg)
        {
            sMsg = string.Empty;
            try
            {
                var query = @"SELECT [GroupID],	[Label] FROM [dbo].[tbl_groups] ORDER BY [dbo].[tbl_groups].GroupID";
                using var connection = _context.CreateConnection();
                var dioList = connection.Query<SecurityGroupsDto>(query);
                return (List<SecurityGroupsDto>)dioList;
            }
            catch (Exception e)
            {
                _logger.LogError("Error occurred getting group list", e);
                sMsg = "ERROR: " + e.Message + " " + (e.InnerException == null ? "" : e.InnerException.Message);
                throw new SystemException(sMsg);
            }
        }
        public List<PermissionsDto> GetPermissionList(int groupID, out string sMsg)
        {
            sMsg = string.Empty;
            List<PermissionsDto> lst = new List<PermissionsDto>();

            try
            {
                var query = @"SELECT [TableName],[GroupID],[AccessMask]  FROM [dbo].[tbl_grouprights] WHERE [GroupID] = @GroupID";
                using var connection = _context.CreateConnection();
                var dioList = connection.Query<PermissionsDto>(query, new { GroupID = groupID });
                return (List<PermissionsDto>)dioList;
            }
            catch (Exception e)
            {
                _logger.LogError("Error occurred getting group list", e);
                sMsg = "ERROR: " + e.Message + " " + (e.InnerException == null ? "" : e.InnerException.Message);
                throw new SystemException(sMsg);
            }
        }

        public bool SavePermissions(int GroupID, List<PermissionsDto> permList, out string sMsg)
        {
            sMsg = string.Empty;
            string sql = "";
            bool bExists = false;
            using var connection = _context.CreateConnection();
            try
            {
                foreach (PermissionsDto perm in permList)
                {
                    if (perm.AccessMask.Trim().Length > 0)
                    {
                        sql = "SELECT COUNT(*)  FROM [dbo].[tbl_grouprights] WHERE [GroupID] = @GroupID AND [TableName] = @TableName";
                        int count = connection.ExecuteScalar<int>(sql, new { GroupID = GroupID, TableName = perm.TableName });
                        bExists = count > 0;
                        if (bExists)
                        {
                            sql = "UPDATE [dbo].[tbl_grouprights] SET [AccessMask] = @AccessMask WHERE [GroupID] = @GroupID AND [TableName] = @TableName";
                            connection.Execute(sql, new { AccessMask = perm.AccessMask, GroupID = GroupID, TableName = perm.TableName });
                        }
                        else
                        {
                            sql = "INSERT INTO [dbo].[tbl_grouprights] ([TableName],[GroupID],[AccessMask]) VALUES (@TableName,@GroupID,@AccessMask)";
                            connection.Execute(sql, new { TableName = perm.TableName, GroupID = GroupID, AccessMask = perm.AccessMask });
                        }
                    }
                    else
                    {
                        sql = "DELETE FROM [dbo].[tbl_grouprights] WHERE [GroupID] = @GroupID AND [TableName] = @TableName";
                        connection.Execute(sql, new { GroupID = GroupID , TableName = perm.TableName });
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError("Error occurred saving permissions", e);
                sMsg = "ERROR: " + e.Message + " " + (e.InnerException == null ? "" : e.InnerException.Message);
                return false;
            }
        }
        public string GetAccessMask(string UserName, string TableName, out string sMsg)
        {
            sMsg = string.Empty;
            try
            {
//                var query = @"SELECT STUFF((
//SELECT '' + [AccessMask] FROM [dbo].[tbl_grouprights] WHERE  [TableName] = @TableName
//AND GroupID IN (SELECT  m.GroupID
//FROM tbl_groupmembers m join tbl_grouprights r on m.GroupID = r.GroupID
//WHERE m.UserName = @UserName)
//FOR XML PATH('')), 1, 1, '') ";

                var query = @"
SELECT '' + [AccessMask] FROM [dbo].[tbl_grouprights] WHERE  [TableName] = @TableName
AND GroupID IN (SELECT distinct m.GroupID
FROM tbl_groupmembers m join tbl_grouprights r on m.GroupID = r.GroupID
WHERE m.UserName = @UserName)
FOR XML PATH('')";

                using var connection = _context.CreateConnection();
                var accessMask = connection.ExecuteScalar<string>(query, new { UserName = UserName, TableName = TableName });
                string result = string.Empty;
                if (accessMask != null && accessMask.Trim() != string.Empty)
                {
                    accessMask = accessMask.Trim();
                    for (int i = 0; i < accessMask.Length; i++)
                    {
                        if (result.IndexOf(accessMask[i]) < 0)
                        {
                            result += accessMask[i];
                        }
                    }
                }
                return result;
            }
            catch (Exception e)
            {
                _logger.LogError("Error occurred getting access mask", e);
                sMsg = "ERROR: " + e.Message + " " + (e.InnerException == null ? "" : e.InnerException.Message);
                return string.Empty;
            }
        }
        public bool IsUserAdmin(string sAMAccountName, out string sMsg)
        {
            sMsg = string.Empty;
            try
            {
                var query = @"SELECT COUNT(*) FROM [dbo].[tbl_groupmembers] WHERE [UserName] = @UserName AND [GroupID] = -1";
                using var connection = _context.CreateConnection();
                int count = connection.ExecuteScalar<int>(query, new { UserName = sAMAccountName });
                return count > 0;
            }
            catch (Exception e)
            {
                _logger.LogError("Error occurred checking if user is admin", e);
                sMsg = "ERROR: " + e.Message + " " + (e.InnerException == null ? "" : e.InnerException.Message);
                return false;
            }
        }
    }
}