using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RuntimeVariables;
using RuntimeConfig;
using SharedScrpits;


namespace Taiwan
{
    public class Employee_Program
    {
        public bool isDev = false;
        public string devMode = null;
        //private SqlConnection SBScon = GlobalConfig.MyConfig.Connection_SBS;
        Common_Share shareCommon = new Common_Share();
        Get_Share shareGet = new Get_Share();


        public Employee_Program()
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(GlobalConfig.MyConfig.Connection_SBS.ConnectionString);

            if (builder.InitialCatalog.ToLower().Contains("dev"))
            {
                isDev = true;
                devMode = "You are using PLAY mode for Taiwan Employee app now.";
            }
        }


        public bool isHR()
        {
            return (shareGet.FetchCodeList("TW_HR").FindAll(f => f.Value == GlobalVariables.MySession.Account).Count() > 0) ? true : false;
        }


        public tw_employee Get_TW_employee(string pm_account_id)
        {
            return GlobalVariables.MySession.List_TW_Employee.Where(w => w.account_id == pm_account_id).FirstOrDefault();
        }


        public List<tw_employee> FetchList_TW_employee()
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<tw_employee> theEmployees = new List<tw_employee>();

            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "tw.prGetList_Employee");
            if (ResultModel.r == 1)
            {
                theEmployees = JsonConvert.DeserializeObject<List<tw_employee>>(ResultModel.JsonData);
                GlobalVariables.MySession.List_TW_Employee = theEmployees;
            }

            return theEmployees;
        }


        public SP_Return Update_TW_employee(tw_employee pm_Emp)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };

            //double check
            ResultModel = shareCommon.checkAuthorized(68);
            if (ResultModel.r == 0)
                return ResultModel;

            pm_Emp.account_id = pm_Emp.account_id.Trim().ToLower();
            pm_Emp.z_usr = GlobalVariables.MySession.Account;
            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJson", SqlDbType = SqlDbType.NVarChar, Value = JsonConvert.SerializeObject(pm_Emp)}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "tw.prManage_Employee", wkParm);
            if (ResultModel.r == 1)
                ResultModel = JsonConvert.DeserializeObject<SP_Return>(ResultModel.JsonData);

            return ResultModel;
        }

    }
}
