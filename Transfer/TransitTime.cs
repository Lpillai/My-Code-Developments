using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;
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
using System.Web.Mvc;

namespace Transfer
{
    public class TransitTime_Program
    {
        public bool isDev = false;
        public string devMode = null;
        //private SqlConnection OScon = GlobalConfig.MyConfig.Connection_OS_BuySheet;
        Common_Share shareCommon = new Common_Share();
        Get_Share shareGet = new Get_Share();


        public TransitTime_Program()
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(GlobalConfig.MyConfig.Connection_OS_BuySheet.ConnectionString);

            if (builder.InitialCatalog.ToLower().Contains("dev"))
            {
                isDev = true;
                devMode = "You are using PLAY mode for Transit Time app now.";
            }
        }


        #region Transit Time Country

        public SelectList getTransitTime_SourceCountryList()
        {
            List<string> SourceCountry = GlobalVariables.MySession.TransitTime_Country_Method.Ocean.Select(x => x.from_country_id.ToString()).Distinct().ToList();
            List<Country> TransitList = shareGet.FetchList_Country().Where(s => SourceCountry.Any(f => f == s.country_code)).ToList();

            return new SelectList(TransitList, "country_code", "country_name");
        }


        public SelectList getTransitTime_TargetCountryList()
        {
            List<string> TargetCountry = GlobalVariables.MySession.TransitTime_Country_Method.Ocean.Select(x => x.from_country_id.ToString()).Distinct().ToList();
            List<Country> TransitList = GlobalVariables.MySession.List_Country.Where(s => !TargetCountry.Any(f => f == s.country_code)).ToList();

            return new SelectList(TransitList, "country_code", "country_name");
        }


        public List<BuySheet_TransitTime_Country> getTransitTime_CountryBulkList(string pm_from_country_id, string pm_ship_method)
        {
            List<BuySheet_TransitTime_Country> theTransitTime = new List<BuySheet_TransitTime_Country>();
            if (pm_ship_method == "Air")
                theTransitTime = GlobalVariables.MySession.TransitTime_Country_Method.Air.Where(s => s.from_country_id == pm_from_country_id).ToList();
            else
                theTransitTime = GlobalVariables.MySession.TransitTime_Country_Method.Ocean.Where(s => s.from_country_id == pm_from_country_id).ToList();
            theTransitTime.ForEach(t => t.i = 0);

            return theTransitTime;
        }


        public BuySheet_TransitTime_Country_Method FetchAllTransitTime_Country()
        {
            BuySheet_TransitTime_Country_Method TransitList = null;
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };

            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_OS_BuySheet, "sbs_sp_TransitTime_GetCountry2");
            if (ResultModel.r == 1)
            {
                TransitList = JsonConvert.DeserializeObject<BuySheet_TransitTime_Country_Method>(ResultModel.JsonData);

                for (int rrn = 1; rrn <= TransitList.Ocean.Count(); rrn++)
                    TransitList.Ocean[rrn - 1].i = rrn;
                for (int rrn = 1; rrn <= TransitList.Air.Count(); rrn++)
                    TransitList.Air[rrn - 1].i = rrn;

                foreach (var grp in TransitList.Ocean.GroupBy(g => g.from_country_id))
                    TransitList.Ocean.Where(w => w.from_country_id == grp.Key).ToList().ForEach(e => e.group_count = grp.Count());
                foreach (var grp in TransitList.Air.GroupBy(g => g.from_country_id))
                    TransitList.Air.Where(w => w.from_country_id == grp.Key).ToList().ForEach(e => e.group_count = grp.Count());

                GlobalVariables.MySession.TransitTime_Country_Method = null;
                GlobalVariables.MySession.TransitTime_Country_Method = TransitList;
            }

            return TransitList;
        }


        public SP_Return GoSaveTransitTime_Country(string pmCRUD, BuySheet_TransitTime_Country pmTransit)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };

            //double check
            ResultModel = shareCommon.checkAuthorized(38);
            if (ResultModel.r == 0)
                return ResultModel;

            string pmJSON = "{'pm_CRUD':'" + pmCRUD + "', 'pm_from_country_id':'" + pmTransit.from_country_id + "', 'pm_to_loc_id':" + pmTransit.to_loc_id.ToString() + ", 'pm_transit_days':" + pmTransit.transit_days.ToString() + ", 'pm_ship_method':'" + pmTransit.ship_method + "', 'pm_usr':'" + GlobalVariables.MySession.Account + "'}";
            pmJSON = pmJSON.Replace("'", "\"");

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.NVarChar, Value = pmJSON}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_OS_BuySheet, "sbs_sp_TransitTime_SaveCountry", wkParm);
            if (ResultModel.r == 1)
                ResultModel = JsonConvert.DeserializeObject<SP_Return>(ResultModel.JsonData);

            return ResultModel;
        }


        public SP_Return GoMirrorTransitTime_Country(string source_country_id, string target_country_id)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };

            //double check
            ResultModel = shareCommon.checkAuthorized(38);
            if (ResultModel.r == 0)
                return ResultModel;

            string pmJSON = "{'pm_mirrorFrom_country_id':'" + source_country_id + "', 'pm_mirrorTo_country_id':'" + target_country_id + "', 'pm_usr':'" + GlobalVariables.MySession.Account + "'}";
            pmJSON = pmJSON.Replace("'", "\"");

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.NVarChar, Value = pmJSON}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_OS_BuySheet, "sbs_sp_TransitTime_MirrorCountry", wkParm);
            if (ResultModel.r == 1)
                ResultModel = JsonConvert.DeserializeObject<SP_Return>(ResultModel.JsonData);

            return ResultModel;
        }


        public SP_Return GoSaveTransitTime_CountryBulk(List<BuySheet_TransitTime_Country> pmTransit)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<BuySheet_TransitTime_Country> theTransitTime = pmTransit.Where(s => s.i == 1).ToList();

            if (theTransitTime.Count() == 0)
            {
                ResultModel.r = 0;
                ResultModel.msg = "No data updated.";
                return ResultModel;
            }

            //double check
            ResultModel = shareCommon.checkAuthorized(38);
            if (ResultModel.r == 0)
                return ResultModel;

            string json_parm = "";
            json_parm = JsonConvert.SerializeObject(theTransitTime);
            json_parm = json_parm.Replace("}{", "},{");
            json_parm = json_parm.Replace("\"i\":1,", "");
            json_parm = json_parm.Replace("\"from_country_name\":null,", "");
            json_parm = json_parm.Replace("\"to_country\":null,", "");
            json_parm = json_parm.Replace("\"to_loc_name\":null,", "");
            json_parm = "'array': " + json_parm;
            json_parm = "'pm_usr': '" + GlobalVariables.MySession.Account + "', " + json_parm;
            json_parm = "{" + json_parm + "}";
            json_parm = json_parm.Replace("'", "\"");

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.NVarChar, Value = json_parm}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_OS_BuySheet, "sbs_sp_TransitTime_SaveCountryBulk", wkParm);
            if (ResultModel.r == 1)
                ResultModel = JsonConvert.DeserializeObject<SP_Return>(ResultModel.JsonData);

            return ResultModel;
        }

        #endregion

        #region Transit Time Location

        public SelectList getTransitTime_SourceLocationList()
        {
            List<string> SourceLocation = GlobalVariables.MySession.List_TransitTime_Location.Select(x => x.from_loc_id.ToString()).Distinct().ToList();
            List<Location> TransitList = shareGet.GetList_Location().Where(s => SourceLocation.Any(f => f == s.location_id)).ToList();

            return new SelectList(TransitList, "location_id", "id_name");
        }


        public SelectList getTransitTime_TargetLocationList()
        {
            List<string> TargetLocation = GlobalVariables.MySession.List_TransitTime_Location.Select(x => x.from_loc_id.ToString()).Distinct().ToList();
            List<Location> TransitList = GlobalVariables.MySession.List_Location.Where(s => !TargetLocation.Any(f => f == s.location_id)).ToList();

            return new SelectList(TransitList, "location_id", "id_name");
        }


        public List<BuySheet_TransitTime_Location> getTransitTime_LocationBulkList(decimal pm_from_loc_id)
        {
            List<BuySheet_TransitTime_Location> theTransitTime = GlobalVariables.MySession.List_TransitTime_Location.Where(s => s.from_loc_id == pm_from_loc_id).ToList();
            theTransitTime.ForEach(t => t.i = 0);

            return theTransitTime;
        }


        public List<BuySheet_TransitTime_Location> FetchAllTransitTime_Location()
        {
            List<BuySheet_TransitTime_Location> TransitList = null;
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };

            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_OS_BuySheet, "sbs_sp_TransitTime_GetLocation");
            if (ResultModel.r == 1)
            {
                TransitList = JsonConvert.DeserializeObject<List<BuySheet_TransitTime_Location>>(ResultModel.JsonData);

                for (int rrn = 1; rrn <= TransitList.Count(); rrn++)
                    TransitList[rrn - 1].i = rrn;

                foreach (var grp in TransitList.GroupBy(g => g.from_loc_id))
                    TransitList.Where(w => w.from_loc_id == grp.Key).ToList().ForEach(e => e.group_count = grp.Count());

                GlobalVariables.MySession.List_TransitTime_Location = null;
                GlobalVariables.MySession.List_TransitTime_Location = TransitList;
            }

            return TransitList;
        }


        public SP_Return GoSaveTransitTime_Location(string pmCRUD, BuySheet_TransitTime_Location pmTransit)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };

            //double check
            ResultModel = shareCommon.checkAuthorized(40);
            if (ResultModel.r == 0)
                return ResultModel;

            string pmJSON = "{'pm_CRUD':'" + pmCRUD + "', 'pm_from_loc_id':'" + pmTransit.from_loc_id + "', 'pm_to_loc_id':" + pmTransit.to_loc_id.ToString() + ", 'pm_transit_days':" + pmTransit.transit_days.ToString() + ", 'pm_usr':'" + GlobalVariables.MySession.Account + "'}";
            pmJSON = pmJSON.Replace("'", "\"");

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.NVarChar, Value = pmJSON}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_OS_BuySheet, "sbs_sp_TransitTime_SaveLocation", wkParm);
            if (ResultModel.r == 1)
                ResultModel = JsonConvert.DeserializeObject<SP_Return>(ResultModel.JsonData);

            return ResultModel;
        }


        public SP_Return GoMirrorTransitTime_Location(decimal pmSource_loc_id, decimal pmTarget_loc_id)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };

            //double check
            ResultModel = shareCommon.checkAuthorized(40);
            if (ResultModel.r == 0)
                return ResultModel;

            string pmJSON = "{'pm_mirrorFrom_loc_id':" + pmSource_loc_id.ToString() + ", 'pm_mirrorTo_loc_id':" + pmTarget_loc_id.ToString() + ", 'pm_usr':'" + GlobalVariables.MySession.Account + "'}";
            pmJSON = pmJSON.Replace("'", "\"");

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.NVarChar, Value = pmJSON}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_OS_BuySheet, "sbs_sp_TransitTime_MirrorLocation", wkParm);
            if (ResultModel.r == 1)
                ResultModel = JsonConvert.DeserializeObject<SP_Return>(ResultModel.JsonData);

            return ResultModel;
        }


        public SP_Return GoSaveTransitTime_LocationBulk(List<BuySheet_TransitTime_Location> pmTransit)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<BuySheet_TransitTime_Location> theTransitTime = pmTransit.Where(s => s.i == 1).ToList();

            if (theTransitTime.Count() == 0)
            {
                ResultModel.r = 0;
                ResultModel.msg = "No data updated.";
                return ResultModel;
            }

            //double check
            ResultModel = shareCommon.checkAuthorized(40);
            if (ResultModel.r == 0)
                return ResultModel;

            string json_parm = "";
            json_parm = JsonConvert.SerializeObject(theTransitTime);
            json_parm = json_parm.Replace("}{", "},{");
            json_parm = json_parm.Replace("\"i\":1,", "");
            json_parm = json_parm.Replace("\"from_loc_name\":null,", "");
            json_parm = json_parm.Replace("\"to_loc_name\":null,", "");
            json_parm = "'array': " + json_parm;
            json_parm = "'pm_usr': '" + GlobalVariables.MySession.Account + "', " + json_parm;
            json_parm = "{" + json_parm + "}";
            json_parm = json_parm.Replace("'", "\"");

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.NVarChar, Value = json_parm}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_OS_BuySheet, "sbs_sp_TransitTime_SaveLocationBulk", wkParm);
            if (ResultModel.r == 1)
                ResultModel = JsonConvert.DeserializeObject<SP_Return>(ResultModel.JsonData);

            return ResultModel;
        }

        #endregion

        #region Transit Default Air

        public List<BuySheet_TransitTime_DefaultAir> FetchAllTransit_DefaultAir()
        {
            List<BuySheet_TransitTime_DefaultAir> TransitList = new List<BuySheet_TransitTime_DefaultAir>();
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };

            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_OS_BuySheet, "sbs_sp_TransitTime_GetDefaultAir");
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
            {
                TransitList = JsonConvert.DeserializeObject<List<BuySheet_TransitTime_DefaultAir>>(ResultModel.JsonData);
                GlobalVariables.MySession.List_TransitTime_DefaultAir = null;
                GlobalVariables.MySession.List_TransitTime_DefaultAir = TransitList;
            }

            return TransitList;
        }


        public MemoryStream ExportDefaultAirSheet()
        {
            var stream = new MemoryStream();

            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            using (var package = new ExcelPackage(stream))
            {
                var workSheet = package.Workbook.Worksheets.Add("DefaultAir");
                workSheet.Cells["A1"].LoadFromCollection(GlobalVariables.MySession.List_TransitTime_DefaultAir, true);
                workSheet.DeleteColumn(7); //z_status
                workSheet.Column(3).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Column(5).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells[workSheet.Dimension.Address].AutoFitColumns();
                workSheet.View.FreezePanes(2, 1);
                package.Save();
            }
            stream.Position = 0;

            return stream;
        }


        public SP_Return GoSaveTransit_DefaultAir(BuySheet_TransitTime_DefaultAir pmDefaultAir)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };

            //double check
            ResultModel = shareCommon.checkAuthorized(91);
            if (ResultModel.r == 0)
                return ResultModel;

            string json_parm = "";
            json_parm = JsonConvert.SerializeObject(pmDefaultAir);
            json_parm = json_parm.Replace("}", ", 'z_usr': '" + GlobalVariables.MySession.Account + "'} ");
            json_parm = json_parm.Replace("'", "\"");

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.NVarChar, Value = json_parm}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_OS_BuySheet, "sbs_sp_TransitTime_SaveDefaultAir", wkParm);
            if (ResultModel.r == 1)
                ResultModel = JsonConvert.DeserializeObject<SP_Return>(ResultModel.JsonData);

            return ResultModel;
        }


        public SP_Return DeleteTransit_DefaultAir(string pm_item_id, string pm_from_country_id, decimal pm_to_loc_id)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };

            //double check
            ResultModel = shareCommon.checkAuthorized(91);
            if (ResultModel.r == 0)
                return ResultModel;

            BuySheet_TransitTime_DefaultAir theTransit = new BuySheet_TransitTime_DefaultAir();
            theTransit.item_id = pm_item_id;
            theTransit.from_country_id = pm_from_country_id;
            theTransit.to_loc_id = pm_to_loc_id;
            theTransit.z_status = "D";

            return GoSaveTransit_DefaultAir(theTransit);
        }

        #endregion

    }
}
