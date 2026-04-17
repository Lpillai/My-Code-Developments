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

namespace Transfer
{
    public class CommercialInvoice_Program
    {
        public bool isDev = false;
        public string devMode = null;
        //private SqlConnection SBScon = GlobalConfig.MyConfig.Connection_SBS;
        Common_Share shareCommon = new Common_Share();


        public CommercialInvoice_Program()
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(GlobalConfig.MyConfig.Connection_SBS.ConnectionString);

            if (builder.InitialCatalog.ToLower().Contains("dev"))
            {
                isDev = true;
                devMode = "You are using PLAY mode for Commercial Invoice app now.";
            }
        }


        public Transfer_CommercialInvoice FetchCommercialInvoice(string pmPickTickets)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            Transfer_CommercialInvoice theList = new Transfer_CommercialInvoice();
            string pmJSON = "{'pm_usr':'" + GlobalVariables.MySession.Account + "', 'pmPickTickets':'" + pmPickTickets.Replace(" ", "") + "'}";
            pmJSON = pmJSON.Replace("'", "\"");

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.VarChar, Value = pmJSON}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "P21.prCI_GetCommercialInvoice3", wkParm);
            if (ResultModel.r == 1 && !String.IsNullOrWhiteSpace(ResultModel.JsonData))
            {
                GlobalVariables.MySession.CommercialInvoice = null;
                theList = JsonConvert.DeserializeObject<Transfer_CommercialInvoice>(ResultModel.JsonData);
                GlobalVariables.MySession.CommercialInvoice = theList;
            }

            return theList;
        }

        /*
        public SP_Return LoadDefaultCommercialInvoice()
        {
            return SaveCommercialInvoice(true, null);
        }
        

        public SP_Return UpdateCommercialInvoiceSession(Transfer_CommercialInvoice pmCI)
        {
            return SaveCommercialInvoice(false, pmCI);
        }


        public SP_Return SaveCommercialInvoice(bool pmReset, Transfer_CommercialInvoice pmCI)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            string pmJSON = "";
            if (pmReset)
                pmJSON = "{'pm_usr':'" + GlobalVariables.MySession.Account + "', 'pmInvoices':'" + GlobalVariables.MySession.CommercialInvoice.inv_string + "', 'pmReset': 1}";
            else
                pmJSON = "{'pm_usr':'" + GlobalVariables.MySession.Account + "', 'pmInvoices':'" + GlobalVariables.MySession.CommercialInvoice.inv_string + "', 'pmReset': 0, 'ci_hdr': " + JsonConvert.SerializeObject(pmCI.ci_hdr) + ", 'ci_line': " + JsonConvert.SerializeObject(pmCI.ci_line) + "}";
            pmJSON = pmJSON.Replace("'", "\"");

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.VarChar, Value = pmJSON}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "P21.prCI_SetCommercialInvoice_Line", wkParm);
            if (ResultModel.r == 1)
            {
                ResultModel = JsonConvert.DeserializeObject<SP_Return>(ResultModel.JsonData);
                if (ResultModel.r == 1)
                {
                    GlobalVariables.MySession.CommercialInvoice.ci_hdr = null;
                    GlobalVariables.MySession.CommercialInvoice.ci_line = null;
                    GlobalVariables.MySession.CommercialInvoice = null;
                }
            }

            return ResultModel;
        }
        */
    }
}
