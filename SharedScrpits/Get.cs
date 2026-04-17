using Newtonsoft.Json;
using RuntimeConfig;
using RuntimeVariables;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;


namespace SharedScrpits
{
    public class Get_Share
    {
        //private SqlConnection SBScon = GlobalConfig.MyConfig.Connection_SBS;
        Common_Share shareCommon = new Common_Share();


        public string GetProjectName()
        {
            //return System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).Replace("file:\\", "").Replace("bin", "UploadedFiles") + "\\";

            if (!string.IsNullOrWhiteSpace(GlobalVariables.MySession.ProjectName))
                return GlobalVariables.MySession.ProjectName;

            string folderPath1 = System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).Replace("bin", ""));
            string folderPath2 = System.IO.Path.GetDirectoryName(folderPath1.Substring(0, folderPath1.Length - 1)).Replace("\\Home", "");
            string folderPath3 = "";
            if (GlobalVariables.MySession.isDebug)
                folderPath3 = folderPath2.Replace(System.IO.Path.GetDirectoryName(folderPath2), "").Replace("\\", "");
            else
                folderPath3 = folderPath1.Replace(System.IO.Path.GetDirectoryName(folderPath1), "").Replace("\\", "");

            GlobalVariables.MySession.ProjectName = folderPath3;
            return folderPath3;
        }


        public SP_Return isLinkedServerConnected(string pm_server_name)
        {
            int isConnected = 0;
            SP_Return ResultModel = new SP_Return() { r = 1 };
            List<CodeValue> theServerList = FetchCodeList("IT_ConnectionStatus", 2);

            if (!int.TryParse(theServerList.Find(f => f.Value == pm_server_name).Code, out isConnected) || isConnected == 0)
            {
                ResultModel.r = isConnected;
                ResultModel.msg = "Server '" + pm_server_name + "' is currently disconnected. Please try again latter.";
            }

            return ResultModel;
        }


        public List<CodeValue> FetchCodeList(string pm_Cluster, int pm_Type = 1)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<CodeValue> theList = new List<CodeValue>();
            string pmJSON = "{'pm_Type':" + pm_Type.ToString() + ", 'pm_Cluster':'" + pm_Cluster + "'}";
            pmJSON = pmJSON.Replace("'", "\"");

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.VarChar, Value = pmJSON}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "z.prIntranet_GetCodeList", wkParm);
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
                theList = JsonConvert.DeserializeObject<List<CodeValue>>(ResultModel.JsonData);

            return theList;
        }


        public IEnumerable<SelectListItem> Fetch_ReportClusterList()
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<SelectListItem> theList = new List<SelectListItem>();
            string json_parm = "{'pm_kind': 1, 'pm_account_id': '', 'pm_group_id': 0, 'pm_report_id': 0}";
            json_parm = json_parm.Replace("'", "\"");

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.NVarChar, Value = json_parm}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "z.prIntranet_GetReportPermissionData", wkParm);
            if (ResultModel.r == 1)
                theList = JsonConvert.DeserializeObject<List<SelectListItem>>(ResultModel.JsonData);

            return theList;
        }


        public IEnumerable<SelectListItem> Fetch_SSRSList()
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<SelectListItem> theList = new List<SelectListItem>();

            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "z.prIntranet_GetSSRSList");
            if (ResultModel.r == 1)
            {
                theList = JsonConvert.DeserializeObject<List<SelectListItem>>(ResultModel.JsonData);
                theList.Insert(0, new SelectListItem() { Value = "", Text = "" });
            }

            return theList;
        }


        public List<theLatestExchangeRate> Fetch_LatestExchangeRate(int pm_currency_id = 0)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<theLatestExchangeRate> theRate = new List<theLatestExchangeRate>();
            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmDate", SqlDbType = SqlDbType.Date, Value = DateTime.Today},
                new SqlParameter() {ParameterName = "@pmCurrency", SqlDbType = SqlDbType.Int, Value = pm_currency_id}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "prIntranet_Get_latestExchangeRate", wkParm);
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
                theRate = JsonConvert.DeserializeObject<List<theLatestExchangeRate>>(ResultModel.JsonData);

            return theRate;
        }


        public List<Location> GetList_Location()
        {
            if (GlobalVariables.MySession.List_Location == null)
                FetchList_Location();

            List<Location> theLocations = GlobalVariables.MySession.List_Location;
            //theLocations.Insert(0, new List_Location { company_id = "", id_name = "", location_id = "", location_name = "" });

            return theLocations;
        }


        public void FetchList_Location()
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<Location> theList = new List<Location>();

            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "prIntranet_GetList_Location");
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
            {
                theList = JsonConvert.DeserializeObject<List<Location>>(ResultModel.JsonData);
                theList.Insert(0, new Location { company_id = "", id_name = "", location_id = "", location_name = "" });
                GlobalVariables.MySession.List_Location = theList;
            }
        }


        public List<Country> FetchList_Country()
        {
            if (GlobalVariables.MySession.List_Country == null)
            {
                List<Country> theList = new List<Country>();
                SP_Return ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "prIntranet_GetList_Country");
                if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
                    theList = JsonConvert.DeserializeObject<List<Country>>(ResultModel.JsonData);

                GlobalVariables.MySession.List_Country = theList;
            }

            return GlobalVariables.MySession.List_Country;
        }


        public List<Location> FetchList_inv_loc(string pm_item_id)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<Location> theList = new List<Location>();
            string pmJSON = "{'pm_item_id':'" + pm_item_id + "'}";
            pmJSON = pmJSON.Replace("'", "\"");

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.VarChar, Value = pmJSON}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "P21.prGet_inv_loc", wkParm);
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
                theList = JsonConvert.DeserializeObject<List<Location>>(ResultModel.JsonData);

            theList.Insert(0, new Location { company_id = "", id_name = "", location_id = "", location_name = "" });
            return theList;
        }


        public IEnumerable<SelectListItem> GetList_CardSystemHours()
        {
            List<SelectListItem> theList = new List<SelectListItem>();
            for (int i = 0; i < 24; i++)
            {
                theList.Add(new SelectListItem { Text = i.ToString("00") + ":00", Value = i.ToString("00") + ":00" });
                theList.Add(new SelectListItem { Text = i.ToString("00") + ":30", Value = i.ToString("00") + ":30" });
            }

            return theList;
        }


        public List<Currency> FetchAllRates_Currency()
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<Currency> RateList = null;

            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "P21Integration.prIntranet_Rates_GetCurrencies");
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
            {
                RateList = JsonConvert.DeserializeObject<List<Currency>>(ResultModel.JsonData);
                GlobalVariables.MySession.List_Rates_Currency = null;
                GlobalVariables.MySession.List_Rates_Currency = RateList;
            }

            return RateList;
        }


        public byte[] GetCurrencyChart(string pm_currency_name)
        {
            List<Currency> RateList = GlobalVariables.MySession.List_Rates_Currency.Where(w => w.to_currency_name == pm_currency_name).ToList();
            int digiCount = BitConverter.GetBytes(decimal.GetBits((decimal)RateList.Average(s => s.exchange_rate))[3])[2];
            double maxY = Math.Round(RateList.Max(s => s.exchange_rate), digiCount, MidpointRounding.AwayFromZero);
            double minY = Math.Round(RateList.Min(s => s.exchange_rate), digiCount, MidpointRounding.AwayFromZero);
            double midY = Math.Round(Queryable.Average((new double[] { maxY, minY }).AsQueryable()), digiCount, MidpointRounding.AwayFromZero);
            maxY += maxY - midY;
            minY -= midY - minY;
            if (minY < 0)
                minY = 0;

            var myChart = new System.Web.Helpers.Chart(width: 1280, height: 720)
                .AddTitle("Exchange Rates")
                .AddSeries(
                    name: pm_currency_name,
                    chartType: "Line",
                    xValue: RateList, xField: "exchange_date",
                    yValues: RateList, yFields: "exchange_rate"
                )
                .SetYAxis(
                    min: minY,
                    max: maxY
                )
                .GetBytes("png");

            return myChart;
        }


        public double GetWorkingHourDifference(DateTime datetime_s, DateTime datetime_e)
        {
            double theHours = 0;
            TimeSpan span = datetime_e.Subtract(datetime_s);
            
            if (datetime_s.Date == datetime_e.Date)
            {
                //theHours = (double)span.TotalSeconds / 60 / 60;
                theHours = (double)span.TotalHours;

                if (datetime_s.Hour <= 12 && 13 <= datetime_e.Hour)
                    theHours -= 1;
            }
            else
            {
                theHours = ((datetime_e.Date - datetime_s.Date).Days - 1) * 8;
                
                //the first day
                theHours += (datetime_s.Date.Add(new TimeSpan(17, 00, 00)) - datetime_s).TotalSeconds / 60 / 60;
                if (datetime_s.Hour <= 13)
                    theHours -= 1;

                //the last day
                theHours += (datetime_e - datetime_e.Date.Add(new TimeSpan(08, 00, 00))).TotalSeconds / 60 / 60;
                if (12 <= datetime_e.Hour)
                    theHours -= 1;
            }

            //return Math.Round(theHours, 1, MidpointRounding.AwayFromZero);
            theHours = Math.Floor(theHours * 10) / 10;
            return theHours;
        }

    }
}
