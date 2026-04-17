using Newtonsoft.Json;
using OfficeOpenXml;
using RuntimeVariables;
using RuntimeConfig;
using SharedScrpits;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Accounting
{
    public class LocalInformation_Program
    {
        public bool isDev = false;
        public string devMode = null;
        //private SqlConnection SBScon = GlobalConfig.MyConfig.Connection_SBS;
        Common_Share shareCommon = new Common_Share();


        public LocalInformation_Program()
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(GlobalConfig.MyConfig.Connection_SBS.ConnectionString);

            if (builder.InitialCatalog.ToLower().Contains("dev"))
            {
                isDev = true;
                devMode = "You are using PLAY mode for Local Information app now.";
            }
        }

        #region Local Information

        public Local_P21_ID FetchP21AddressInformation(int pm_address_id)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            Local_P21_ID theID = new Local_P21_ID();
            string pmJSON = "{'address_id':" + pm_address_id.ToString() + "}";
            pmJSON = pmJSON.Replace("'", "\"");

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.VarChar, Value = pmJSON}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "P21.pr_Local_GetAddress", wkParm);
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
                theID = JsonConvert.DeserializeObject<Local_P21_ID>(ResultModel.JsonData);

            GlobalVariables.MySession.new_P21_ID = theID;
            return theID;
        }


        public Local_Information GetLocalInformation(int pm_address_id)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            Local_Information theInfo = new Local_Information();
            GlobalVariables.MySession.theLocalInformation = null;
            string pmJSON = "{'address_id':" + pm_address_id.ToString() + "}";
            pmJSON = pmJSON.Replace("'", "\"");

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.VarChar, Value = pmJSON}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "prIntranet_Local_GetInformation", wkParm);
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
                theInfo = JsonConvert.DeserializeObject<Local_Information>(ResultModel.JsonData);

            GlobalVariables.MySession.theLocalInformation = theInfo;
            return theInfo;
        }


        public List<Local_P21_ID> FetchP21List()
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<Local_P21_ID> theID = new List<Local_P21_ID>();

            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "prIntranet_Local_FetchP21List");
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
                theID = JsonConvert.DeserializeObject<List<Local_P21_ID>>(ResultModel.JsonData);

            return theID;
        }

        /*
        public SP_Return UpdateLocalInformation(Local_Information pm_LocalInformation)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            string pmJSON = "";

            //double check
            ResultModel = shareCommon.checkAuthorized(62);
            if (ResultModel.r == 0)
                return ResultModel;

            pmJSON = "{'pm_usr':'" + GlobalVariables.MySession.Account + "', 'theGUI':" + JsonConvert.SerializeObject(pm_LocalInformation) + "}";
            pmJSON = pmJSON.Replace("'", "\"");
            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.NVarChar, Value = pmJSON}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "prIntranet_AR_Management", wkParm);
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
                ResultModel = JsonConvert.DeserializeObject<SP_Return>(ResultModel.JsonData);

            return ResultModel;
        }
        */
        public SP_Return UpdateLocalInformation(string pmJSON)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };

            //double check
            ResultModel = shareCommon.checkAuthorized(125);
            if (ResultModel.r == 0)
                return ResultModel;

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.NVarChar, Value = pmJSON}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "prIntranet_Local_UpdateInformation", wkParm);
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
                ResultModel = JsonConvert.DeserializeObject<SP_Return>(ResultModel.JsonData);

            return ResultModel;
        }

        #endregion

    }
}
