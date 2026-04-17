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


namespace IT
{
    public class Code_Program
    {
        public bool isDev = false;
        public string devMode = null;
        Common_Share shareCommon = new Common_Share();


        public Code_Program()
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(GlobalConfig.MyConfig.Connection_SBS.ConnectionString);

            if (builder.InitialCatalog.ToLower().Contains("dev"))
            {
                isDev = true;
                devMode = "You are using PLAY mode for Code app now.";
            }
        }


        public List<CodeCluster> Code_FetchClusterList()
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<CodeCluster> theClusterList = new List<CodeCluster>();
            string pmJSON = "";

            pmJSON = "{'pm_Cluster':'', 'pm_Type':0}";
            pmJSON = pmJSON.Replace("'", "\"");

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.NVarChar, Value = pmJSON}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "z.prIntranet_GetCodeList", wkParm);
            if (ResultModel.r == 1)
                theClusterList = JsonConvert.DeserializeObject<List<CodeCluster>>(ResultModel.JsonData);

            return theClusterList;
        }


        public List<CodeStore> Code_FetchClusterDetails(string pm_cluster)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<CodeStore> theCodeList = new List<CodeStore>();
            string pmJSON = "";

            pmJSON = "{'pm_Cluster':'" + pm_cluster + "', 'pm_Type':3}";
            pmJSON = pmJSON.Replace("'", "\"");

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.NVarChar, Value = pmJSON}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "z.prIntranet_GetCodeList", wkParm);
            if (ResultModel.r == 1)
            {
                theCodeList = JsonConvert.DeserializeObject<List<CodeStore>>(ResultModel.JsonData);
                GlobalVariables.MySession.List_CodeStore = null;
                GlobalVariables.MySession.List_CodeStore = theCodeList;
            }

            return theCodeList;
        }


        public SP_Return Code_SetCode(CodeStore pm_code)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };

            //double check
            ResultModel = shareCommon.checkAuthorized(58);
            if (ResultModel.r == 0)
                return ResultModel;

            pm_code.z_usr = GlobalVariables.MySession.Account;

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJson", SqlDbType = SqlDbType.NVarChar, Value = JsonConvert.SerializeObject(pm_code)}
            };

            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "z.prIntranet_ManageCodeList", wkParm);
            if (ResultModel.r == 1)
                ResultModel = JsonConvert.DeserializeObject<SP_Return>(ResultModel.JsonData);

            return ResultModel;
        }

    }
}
