using Newtonsoft.Json;
using RuntimeVariables;
using RuntimeConfig;
using SharedScrpits;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Admin
{
    public class Account_Program
    {
        //private SqlConnection SBScon = GlobalConfig.MyConfig.Connection_SBS;
        Common_Share shareCommon = new Common_Share();


        public List<AccountItem> GetAccountList()
        {
            List<AccountItem> theList = new List<AccountItem>();

            if (GlobalVariables.MySession.List_Account == null)
                theList = FetchAccountList();
            else
                theList = GlobalVariables.MySession.List_Account;

            return theList.Where(w => w.z_status != "D").ToList();
        }


        public List<AccountItem> FetchAccountList()
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<AccountItem> theList = new List<AccountItem>();

            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "z.prIntranet_GetAccountList");
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
            {
                theList = JsonConvert.DeserializeObject<List<AccountItem>>(ResultModel.JsonData);

                foreach (var item in theList)
                    item.full_name = item.first_name + " " + item.last_name;

                GlobalVariables.MySession.List_Account = null;
                GlobalVariables.MySession.List_Account = theList;
            }

            return theList;
        }


        public SP_Return ManageAccount(AccountItem pm_Account)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };

            //double check
            ResultModel = shareCommon.checkAuthorized(13);
            if (ResultModel.r == 0)
                return ResultModel;

            pm_Account.z_usr = GlobalVariables.MySession.Account;

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.NVarChar, Value = JsonConvert.SerializeObject(pm_Account)}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "z.prIntranet_ManageAccount", wkParm);
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
                ResultModel = JsonConvert.DeserializeObject<SP_Return>(ResultModel.JsonData);

            return ResultModel;
        }


        public SP_Return SendWelcome(string pm_id)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };

            //double check
            ResultModel = shareCommon.checkAuthorized(13);
            if (ResultModel.r == 0)
                return ResultModel;

            string json_parm = "{ \"pm_id\": \"" + pm_id + "\", \"pm_z_usr\": \"" + GlobalVariables.MySession.Account + "\" }";

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.NVarChar, Value = json_parm}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "z.prIntranet_WelcomeSetPassword", wkParm);
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
                ResultModel = JsonConvert.DeserializeObject<SP_Return>(ResultModel.JsonData);

            return ResultModel;
        }


        public SP_Return ResetPassword(string pm_id)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };

            //double check
            ResultModel = shareCommon.checkAuthorized(13);
            if (ResultModel.r == 0)
                return ResultModel;

            string json_parm = "{ \"pm_id\": \"" + pm_id + "\", \"pm_z_usr\": \"" + GlobalVariables.MySession.Account + "\" }";

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.NVarChar, Value = json_parm}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "z.prIntranet_ResetPassword", wkParm);
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
                ResultModel = JsonConvert.DeserializeObject<SP_Return>(ResultModel.JsonData);

            return ResultModel;
        }


        public List<GroupItem> WhichGroupIsIn(string pm_AccountID)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<GroupItem> theList = new List<GroupItem>();
            string json_parm = "{\"pm_kind\": 3, \"pm_group_id\": 0, \"pm_account_id\": \"" + pm_AccountID + "\"}";

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.NVarChar, Value = json_parm}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "z.prIntranet_GetGroupData", wkParm);
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
                theList = JsonConvert.DeserializeObject<List<GroupItem>>(ResultModel.JsonData);

            return theList;
        }

    }
}
