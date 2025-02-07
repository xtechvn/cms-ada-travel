using DAL.Generic;
using DAL.StoreProcedure;
using Entities.Models;
using Entities.ViewModels;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace DAL
{
    public class UserRoleDAL : GenericService<UserRole>
    {
        private static DbWorker _DbWorker;

        public UserRoleDAL(string connection) : base(connection)
        {
            _DbWorker = new DbWorker(connection);
        }
       
        public async Task<List<Role>> GetUserActiveRoleList(int user_id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var list_role_id = await _DbContext.UserRole.AsNoTracking().Where(s => s.UserId == user_id).ToListAsync();
                    if (list_role_id != null && list_role_id.Count > 0)
                    {
                        return await _DbContext.Role.Where(s => list_role_id.Select(x => x.RoleId).Contains(s.Id)).ToListAsync();
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetById - UserPositionDAL: " + ex);
                return null;
            }
        }
        public async Task<List<RolePermissionViewModel>> GetListRolePermissionByUserAndRole(long UserId, List<long> RoleIds)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[2];
                objParam[0] = new SqlParameter("@UserId", UserId);
                objParam[1] = new SqlParameter("@RoleId", string.Join(',', RoleIds));
                DataTable dt = _DbWorker.GetDataTable(StoreProcedureConstant.SP_GetListRolePermissionByUserAndRole, objParam);
                if (dt != null && dt.Rows.Count > 0)
                {
                    var data = dt.ToList<RolePermissionViewModel>();
                    return data;
                }

                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListRolePermissionByUserAndRole - UserRoleDAL. " + ex);
                return null;
            }
        }
        public async Task<int> GetManagerByUserId(long UserId)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[1];
                objParam[0] = new SqlParameter("@UserId", UserId);

                DataTable dt = _DbWorker.GetDataTable(StoreProcedureConstant.SP_GetManagerByUserId, objParam);
                if (dt != null && dt.Rows.Count > 0)
                {
                    var data = dt.ToList<RolePermissionViewModel>();
                    var id = Convert.ToInt32(dt.Rows[0]["UserId"]);
                    return id;
                }

                return 0;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListRolePermissionByUserAndRole - UserRoleDAL. " + ex);
                return 0;
            }
        }
        public async Task<int> GetLeaderByUserId(long UserId)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[1];
                objParam[0] = new SqlParameter("@UserId", UserId);

                DataTable dt = _DbWorker.GetDataTable(StoreProcedureConstant.Sp_GetLeaderByUserId, objParam);
                if (dt != null && dt.Rows.Count > 0)
                {
                    var data = dt.ToList<RolePermissionViewModel>();
                    var id = Convert.ToInt32(dt.Rows[0]["UserId"]);
                    return id;
                }

                return 0;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListRolePermissionByUserAndRole - UserRoleDAL. " + ex);
                return 0;
            }
        }
      


        public async Task<List<int>> GetUserRoleId(int userId)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return await _DbContext.UserRole.AsNoTracking().Where(x => x.UserId == userId).Select(s => s.RoleId).ToListAsync();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetUserRoleId - UserRoleDAL: " + ex);
                return new List<int>();
            }
        }
       
        public List<User> GetListUserByRole(int role_id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var user_role = _DbContext.UserRole.Where(s => s.RoleId == role_id).ToList();
                    var userRoleIds = user_role.Select(n => n.UserId).ToList();
                    if (userRoleIds.Count > 0)
                    {
                        return _DbContext.User.Where(s => userRoleIds.Contains(s.Id) && s.Status == 0).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListUserByRole - UserRoleDAL: " + ex);
            }
            return new List<User>();
        }
        public int InsertUserRole(UserRole role)
        {
            try
            {

                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@UserID", role.UserId),
                    new SqlParameter("@RoleId", role.RoleId)
                };
                var id = _DbWorker.ExecuteNonQuery(StoreProcedureConstant.InsertUserRole, parameters);
                role.Id = id;
                return id;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpsertUserRole - UserDAL: " + ex);
                return -1;
            }
        }
        public int UpdateUserRole(UserRole role)
        {
            try
            {

                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@Id", role.Id),
                    new SqlParameter("@UserID", role.UserId),
                    new SqlParameter("@UserRole", role.RoleId)
                };
                var id = _DbWorker.ExecuteNonQuery(StoreProcedureConstant.UpdateUserRole, parameters);
                role.Id = id;
                return id;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpsertUserRole - UserDAL: " + ex);
                return -1;
            }
        }
        public int DeleteUserRole(int user_id, int[] roles)
        {
            try
            {
                if(roles!=null && roles.Count() > 0)
                {
                    using (var _DbContext = new EntityDataContext(_connection))
                    {
                        var user_role = _DbContext.UserRole.AsNoTracking().Where(s => s.UserId == user_id && roles.Contains(s.RoleId)).ToList();
                        if (user_role != null && user_role.Count > 0)
                        {
                            foreach(var delete_role in user_role)
                            {
                                delete_role.UserId *= -1;
                                UpdateUserRole(delete_role);
                            }
                        }
                    }
                }
                return user_id;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("DeleteUserRole - UserDAL: " + ex);
                return -1;
            }
        }
        public async Task<List<UserRole>> GetByUserId(int userId)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return await _DbContext.UserRole.AsNoTracking().Where(x => x.UserId == userId).ToListAsync();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByUserId - UserRoleDAL: " + ex);
                return new List<UserRole>();
            }
        }
    }
}