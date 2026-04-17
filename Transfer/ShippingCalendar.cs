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
    public class ShippingCalendar_Program
    {
        public bool isDev = false;
        public string devMode = null;
        //private SqlConnection SBScon = GlobalConfig.MyConfig.Connection_SBS;
        Common_Share shareCommon = new Common_Share();


        public ShippingCalendar_Program()
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(GlobalConfig.MyConfig.Connection_SBS.ConnectionString);

            if (builder.InitialCatalog.ToLower().Contains("dev"))
            {
                isDev = true;
                devMode = "You are using PLAY mode for Shipping Calendar app now.";
            }
        }


        private void FetchList_ShippingLocGrp()
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<Transfer_ShippingLocGrp> theList = new List<Transfer_ShippingLocGrp>();

            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "prIntranet_GetList_ShippingLocGrp");
            if (ResultModel.r == 1)
            {
                theList = JsonConvert.DeserializeObject<List<Transfer_ShippingLocGrp>>(ResultModel.JsonData);
                GlobalVariables.MySession.List_ShippingLocGrp = null;
                GlobalVariables.MySession.List_ShippingLocGrp = theList;
            }
        }


        public List<Transfer_ShippingCountry> GetList_ShippingLocGrp_Country()
        {
            if (GlobalVariables.MySession.List_ShippingLocGrp == null)
                FetchList_ShippingLocGrp();

            return GlobalVariables.MySession.List_ShippingLocGrp.GroupBy(g => new { g.country_cd, g.country_name }).Select(s => s.FirstOrDefault()).Select(x => new Transfer_ShippingCountry { country_cd = x.country_cd, country_name = x.country_name }).ToList();
        }


        public List<Transfer_ShippingLocation> GetList_ShippingLocGrp_Location(string pm_country_cd)
        {
            if (GlobalVariables.MySession.List_ShippingLocGrp == null)
                FetchList_ShippingLocGrp();

            return GlobalVariables.MySession.List_ShippingLocGrp.Where(w => w.country_cd == pm_country_cd).Select(x => new Transfer_ShippingLocation { group_id = x.group_id, loc_cd = x.loc_cd, location_id = x.location_id }).Distinct().ToList();
        }


        public Transfer_ShippingCalendar_Method FetchShippingCalendar2(string pm_country_cd, int pm_loc_cd)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            Transfer_ShippingCalendar_Method theList = new Transfer_ShippingCalendar_Method();
            string pmJSON = "{'pm_country_cd':'" + pm_country_cd + "', 'pm_loc_cd':" + pm_loc_cd.ToString() + "}";
            pmJSON = pmJSON.Replace("'", "\"");

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.VarChar, Value = pmJSON}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "prIntranet_ShippingCalendar_Get2", wkParm);
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
                theList = JsonConvert.DeserializeObject<Transfer_ShippingCalendar_Method>(ResultModel.JsonData);

            return theList;
        }


        public SP_Return GoCreateShippingCalendar(string pm_country_cd, int pm_location_cd, string pm_ship_method, DateTime pm_ship_date)
        {
            List<Transfer_ShippingCalendar> pmTransfer = new List<Transfer_ShippingCalendar>();
            pmTransfer.Add(new Transfer_ShippingCalendar { date_uid = -1, ship_date = pm_ship_date, ship_method = pm_ship_method });

            return GoSaveShippingCalendar(pm_country_cd, pm_location_cd, pmTransfer);
        }


        public SP_Return GoSaveShippingCalendar(string pm_country_cd, int pm_location_cd, List<Transfer_ShippingCalendar> pmTransfer)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };

            //double check
            ResultModel = shareCommon.checkAuthorized(57);
            if (ResultModel.r == 0)
                return ResultModel;

            string pmJSON = JsonConvert.SerializeObject(pmTransfer);
            pmJSON = "{'pm_country_cd':'" + pm_country_cd + "', 'pm_location_cd':" + pm_location_cd.ToString() + ", 'pm_z_usr':'" + GlobalVariables.MySession.Account + "', 'pm_dates':" + pmJSON + "}";
            pmJSON = pmJSON.Replace("'", "\"");

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.VarChar, Value = pmJSON}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "prIntranet_ShippingCalendar_Update", wkParm);
            if (ResultModel.r == 1)
                ResultModel = JsonConvert.DeserializeObject<SP_Return>(ResultModel.JsonData);

            return ResultModel;
        }


        public SP_Return GoDeleteShippingCalendar(int pm_date_uid)
        {
            List<Transfer_ShippingCalendar> pmTransfer = new List<Transfer_ShippingCalendar>();
            pmTransfer.Add(new Transfer_ShippingCalendar { date_uid = pm_date_uid, z_status = "D" });

            return GoSaveShippingCalendar("", 0, pmTransfer);
        }

    }
}
