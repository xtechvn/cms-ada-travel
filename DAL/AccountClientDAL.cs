using DAL.Generic;
using DAL.StoreProcedure;
using Entities.Models;
using Microsoft.Data.SqlClient;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace DAL
{
   public class AccountClientDAL : GenericService<AccountClient>
    {
        private static DbWorker _DbWorker;

        public AccountClientDAL(string connection) : base(connection) {

            _DbWorker = new DbWorker(connection);
        }
        public int CreateAccountClient(AccountClient model)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {

                    var deta = _DbContext.AccountClients.Add(model);
                    _DbContext.SaveChanges();


                }
                return 1;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CreateAccountClient - AccountClientDAL: " + ex);
                return 0;
            }
        }
        public long GetMainAccountClientByClientId(long client_id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {

                    var main_account =  _DbContext.AccountClients.FirstOrDefault(x => x.ClientId == client_id);
                    if (main_account != null)
                    {
                        return main_account.Id;
                    }


                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetMainAccountClientByClientId - AccountClientDAL: " + ex.ToString());

            }
            return -1;

        }
        public AccountClient AccountClientByClientId(long client_id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {

                    //var main_account = _DbContext.AccountClients.FirstOrDefault(x => x.ClientId == client_id);
                    //if (main_account != null)
                    //{
                    //    return main_account;
                    //}
                    SqlParameter[] objParam = new SqlParameter[]
                    {
                        new SqlParameter("@ClientId", client_id)

                    };
                    var data= _DbWorker.GetDataTable(StoreProcedureConstant.GetDetailAccountClientByClientId, objParam);
                    if (data != null && data.Rows.Count > 0)
                    {
                        var list = data.ToList<AccountClient>();
                        if(list!=null && list.Count > 0)
                        {
                            return list.First();
                        }
                       
                    }

                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("AccountClientByClientId - AccountClientDAL: " + ex.ToString());

            }
            return null;

        }
        public async Task<int> UpdataAccountClient(AccountClient model)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {

                 
                    _DbContext.AccountClients.Update(model);
                     await _DbContext.SaveChangesAsync();

                    return 1;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdataAccountClient - AccountClientDAL: " + ex.ToString());

            }
            return 0;

        }
        
    }
}
