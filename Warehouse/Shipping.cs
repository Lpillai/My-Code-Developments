using Newtonsoft.Json;
using RuntimeConfig;
using RuntimeVariables;
using SharedScrpits;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Warehouse
{
    public class Shipping_Program
    {
        public bool isDev = false;
        public string devMode = null;
        //private SqlConnection SBScon = GlobalConfig.MyConfig.Connection_SBS;
        Common_Share shareCommon = new Common_Share();


        public Shipping_Program()
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(GlobalConfig.MyConfig.Connection_SBS.ConnectionString);

            if (builder.InitialCatalog.ToLower().Contains("dev"))
            {
                isDev = true;
                devMode = "You are using PLAY mode for Shipping app now.";
            }
        }


        public List<GoodsNote_Shipping_hdr> FetchGoodsNote_Shipping(string pmInvoices)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<GoodsNote_Shipping_hdr> theList = new List<GoodsNote_Shipping_hdr>();
            string pmJSON = "{'pmInvoices':'" + pmInvoices + "'}";
            pmJSON = pmJSON.Replace("'", "\"");

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.VarChar, Value = pmJSON}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "P21.prGoodsNote_GetShipping", wkParm);
            if (ResultModel.r == 1 && !String.IsNullOrWhiteSpace(ResultModel.JsonData))
                theList = JsonConvert.DeserializeObject<List<GoodsNote_Shipping_hdr>>(ResultModel.JsonData);

            return theList;
        }

    }
}
