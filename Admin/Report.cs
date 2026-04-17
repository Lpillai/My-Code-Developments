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
    public class Report_Program
    {
        //private SqlConnection SBScon = GlobalConfig.MyConfig.Connection_SBS;
        Common_Share shareCommon = new Common_Share();


        public List<ReportItem> GetReportList()
        {
            return GlobalVariables.MySession.List_Report == null ? FetchReportList() : GlobalVariables.MySession.List_Report;
        }


        public List<ReportItem> FetchReportList()
        {
            List<ReportItem> theEntireList = new List<ReportItem>();
            SP_Return ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "z.prIntranet_GetReportList");
            if (ResultModel.r == 1)
            {
                theEntireList = JsonConvert.DeserializeObject<List<ReportItem>>(ResultModel.JsonData);
                for (int rrn = 0; rrn < theEntireList.Count(); rrn++)
                    theEntireList[rrn].no = rrn + 1;

                GlobalVariables.MySession.List_Report = null;
                GlobalVariables.MySession.List_Report = theEntireList;
            }

            return theEntireList;
        }


        public SP_Return UpdateReport(ReportItem pm_Report, string pmCRUD)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            string pmJSON = "";

            //double check
            ResultModel = shareCommon.checkAuthorized(49);
            if (ResultModel.r == 0)
                return ResultModel;

            if ((pm_Report.output == "download" || pm_Report.output == "query") &&
                string.IsNullOrWhiteSpace(pm_Report.run))
            {
                ResultModel.r = 0;
                ResultModel.msg = "You have to give some value in 'Action'.";
            }
            if ((pm_Report.output == "embed" || pm_Report.output == "tab") &&
                string.IsNullOrWhiteSpace(pm_Report.ssrs_id))
            {
                ResultModel.r = 0;
                ResultModel.msg = "You have to select a report in 'SSRS ID'.";
            }
            if (ResultModel.r == 0)
                return ResultModel;

            if (pm_Report.output == "query")
                pm_Report.ssrs_id = null;
            if (pm_Report.output == "embed" || pm_Report.output == "tab")
                pm_Report.run = "";

            pm_Report.z_usr = GlobalVariables.MySession.Account;
            pmJSON = JsonConvert.SerializeObject(pm_Report);
            pmJSON = "{\"pm_CRUD\":\"" + pmCRUD + "\", \"pm_rpts\":[" + pmJSON + "]}";

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.NVarChar, Value = pmJSON}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "z.prIntranet_ManageReport", wkParm);
            if (ResultModel.r == 1)
            {
                ResultModel = JsonConvert.DeserializeObject<SP_Return>(ResultModel.JsonData);
                if (ResultModel.r == 0) //successful
                    GlobalVariables.MySession.List_Report = null;
            }

            return ResultModel;
        }


        public SP_Return DeleteReport(int pm_ReportID)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };

            //double check
            ResultModel = shareCommon.checkAuthorized(49);
            if (ResultModel.r == 0)
                return ResultModel;

            ReportItem theReport = GlobalVariables.MySession.List_Report.Where(a => a.report_id == pm_ReportID).SingleOrDefault();
            return UpdateReport(theReport, "D");
        }


        public SP_Return CopyReport(ReportItem pm_Report)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            string pmJSON = "";

            //double check
            ResultModel = shareCommon.checkAuthorized(49);
            if (ResultModel.r == 0)
                return ResultModel;

            pm_Report.z_usr = GlobalVariables.MySession.Account;
            pmJSON = JsonConvert.SerializeObject(pm_Report);

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.NVarChar, Value = pmJSON}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "z.prIntranet_CopyReport", wkParm);
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
            {
                ResultModel = JsonConvert.DeserializeObject<SP_Return>(ResultModel.JsonData);
                if (ResultModel.r == 0) //successful
                    GlobalVariables.MySession.List_Report = null;
            }

            //ReportItem theReport = GlobalVariables.MySession.List_Report.Where(a => a.report_id == pm_ReportID).SingleOrDefault();
            return ResultModel;
        }


        public List<ReportPermissionItem> WhoHasReportPermission(string pm_ReportID)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<ReportPermissionItem> theList = new List<ReportPermissionItem>();
            string pmJSON = "{'pm_kind': 3, 'pm_account_id': '', 'pm_group_id': 0, 'pm_report_id': " + pm_ReportID + "}";
            pmJSON = pmJSON.Replace("'", "\"");

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.NVarChar, Value = pmJSON}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "z.prIntranet_GetReportPermissionData", wkParm);
            if (ResultModel.r == 1)
                theList = JsonConvert.DeserializeObject<List<ReportPermissionItem>>(ResultModel.JsonData);

            return theList;
        }

    }
}
