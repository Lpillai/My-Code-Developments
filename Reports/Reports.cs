using Microsoft.Reporting.WebForms;
using Newtonsoft.Json;
using RuntimeVariables;
using RuntimeConfig;
using SharedScrpits;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reports
{
    public class Reports_Program
    {
        public string devMode = null;
        //private SqlConnection SBScon = GlobalConfig.MyConfig.Connection_SBS;
        Common_Share shareCommon = new Common_Share();
        string[] file_type = { "PDF", "EXCELOPENXML", "EXCEL" }; //{".pdf", ".xlsx", ".xls"}


        public Reports_Program()
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(GlobalConfig.MyConfig.Connection_SBS.ConnectionString);
            string db = builder.InitialCatalog.ToLower();
            if (db.Contains("dev"))
                devMode = "You are using PLAY mode for Reports app now.";
        }


        public ReportViewer GenerateReport(string path, ReportParameterCollection parm)
        {
            //string ssrsUrl = ConfigurationManager.AppSettings["SSRSReportUrl"].ToString();
            //string ssrsDomain = ConfigurationManager.AppSettings["SSRSdomain"].ToString();
            //string ssrsUser = ConfigurationManager.AppSettings["SSRSuser"].ToString();
            //string ssrsPassword = ConfigurationManager.AppSettings["SSRSpassword"].ToString();
            string ssrsUrl = GlobalConfig.MyConfig.SSRSReportUrl;
            string ssrsDomain = GlobalConfig.MyConfig.SSRSdomain;
            string ssrsUser = GlobalConfig.MyConfig.SSRSuser;
            string ssrsPassword = GlobalConfig.MyConfig.SSRSpassword;
            //CustomReportCredentials irsc = new CustomReportCredentials(userName, password, domain); // if you dont have a domain enter computer name
            CustomReportCredentials irsc = new CustomReportCredentials(ssrsUser, ssrsPassword, ssrsDomain);
            ReportViewer viewer = new ReportViewer();

            viewer.ProcessingMode = ProcessingMode.Remote;
            viewer.ServerReport.ReportServerCredentials = irsc;
            viewer.ServerReport.ReportServerUrl = new Uri(ssrsUrl);
            viewer.ServerReport.ReportPath = path;
            viewer.SizeToReportContent = true;
            viewer.ZoomMode = ZoomMode.FullPage;
            viewer.AsyncRendering = true;
            if (parm != null)
                viewer.ServerReport.SetParameters(parm);
            viewer.ServerReport.Refresh();

            return viewer;
        }


        public ReportViewer EmbedSSRS(string pm_path, string pm_parm)
        {
            ReportParameterCollection parms = new ReportParameterCollection();
            List<string> theParmStrings = new List<string>();

            if (!string.IsNullOrWhiteSpace(pm_parm))
            {
                theParmStrings = pm_parm.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).ToList();
                foreach (string pm in theParmStrings)
                    parms.Add(new ReportParameter(pm.Split('=')[0], pm.Split('=')[1]));
            }

            return GenerateReport(pm_path, parms);
        }


        public MemoryStream NewTab(string pm_path, string pm_parm)
        {
            return RenderReportToStream(EmbedSSRS(pm_path, pm_parm), 0);
        }


        public MemoryStream DownloadFile(string pm_rptPath, string pm_parm, int pm_fileType)
        {
            return RenderReportToStream(EmbedSSRS(pm_rptPath, pm_parm), pm_fileType);
        }


        public byte[] RenderReportToByteArray(ReportViewer pm_rpt, int pm_type)
        {
            Warning[] warnings;
            string[] streamids;
            string mimeType;
            string encoding;
            string filenameExtension;
            byte[] bytes = pm_rpt.ServerReport.Render(file_type[pm_type], null, out mimeType, out encoding, out filenameExtension, out streamids, out warnings);

            return bytes;
        }


        public MemoryStream RenderReportToStream(ReportViewer pm_rpt, int pm_type)
        {
            string mimeType;
            string filenameExtension;
            MemoryStream workStream = new MemoryStream();
            //public System.IO.Stream Render (string format, string deviceInfo, System.Collections.Specialized.NameValueCollection urlAccessParameters, out string mimeType, out string fileNameExtension); 
            pm_rpt.ServerReport.Render(file_type[pm_type], null, null, out mimeType, out filenameExtension).CopyTo(workStream);
            workStream.Position = 0;

            return workStream;
        }


        public List<LoginReport> FetchReportList(string pm_cluster)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<LoginReport> theReportList = new List<LoginReport>();
            string pmJSON = "";

            pmJSON = "{'pm_id':'" + GlobalVariables.MySession.Account + "', 'pm_cluster':'" + pm_cluster + "'}";
            pmJSON = pmJSON.Replace("'", "\"");

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.NVarChar, Value = pmJSON}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "z.prIntranet_GetLoginReport", wkParm);
            if (ResultModel.r == 1)
                theReportList = JsonConvert.DeserializeObject<List<LoginReport>>(ResultModel.JsonData);

            return theReportList;
        }

    }
}
