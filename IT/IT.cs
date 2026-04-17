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
    public class IT_Program
    {
        public bool isDev = false;
        public string devMode = null;
        //private SqlConnection SBScon = GlobalConfig.MyConfig.Connection_SBS;
        Common_Share shareCommon = new Common_Share();


        public IT_Program()
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(GlobalConfig.MyConfig.Connection_SBS.ConnectionString);

            if (builder.InitialCatalog.ToLower().Contains("dev"))
            {
                isDev = true;
                devMode = "You are using PLAY mode for IT app now.";
            }
        }


        public List<P21Session> FetchP21SessionList()
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<P21Session> theList = new List<P21Session>();

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmType", SqlDbType = SqlDbType.Int, Value = 1}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "P21.prP21_DeleteSelectedSessions", wkParm);
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
                theList = JsonConvert.DeserializeObject<List<P21Session>>(ResultModel.JsonData);

            return theList;
        }


        public SP_Return DeleteSessions(List<P21Session> pmSessionList)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };

            //double check
            ResultModel = shareCommon.checkAuthorized(36);
            if (ResultModel.r == 0)
                return ResultModel;

            List<P21Session> wkList = pmSessionList.FindAll(a => a.chk == true);

            if (wkList.Count() <= 0)
            {
                ResultModel.r = 0;
                ResultModel.msg = "None of sessions are selected.";
                return ResultModel;
            }

            string json_parm = "{'pm_Sessions':[ " + string.Join(", ", wkList.Select(a => "{'UserId':'" + a.UserId + "', 'SessionId':'" + a.SessionId + "'}").ToArray()) + " ]}";
            json_parm = json_parm.Replace("'", "\"");

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.NVarChar, Value = json_parm}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "P21.prP21_DeleteSelectedSessions", wkParm);
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
                ResultModel = JsonConvert.DeserializeObject<SP_Return>(ResultModel.JsonData);

            return ResultModel;
        }

    }
}
