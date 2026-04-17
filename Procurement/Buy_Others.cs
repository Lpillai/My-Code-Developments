using Newtonsoft.Json;
using RuntimeVariables;
using RuntimeConfig;
using SharedScrpits;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace Procurement
{
    public class BuyOthers_Program
    {
        //private SqlConnection OScon = GlobalConfig.MyConfig.Connection_OS_BuySheet;
        Common_Share shareCommon = new Common_Share();
        Filters_Share shareFilters = new Filters_Share();
        //ModelGetter getModel = new ModelGetter();


        #region Buy

        public Buy FetchOneBuy(int pm_viewID)
        {
            Buy buyModel = new Buy();
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@viewID", SqlDbType = SqlDbType.Int, Value = pm_viewID}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_OS_BuySheet, "sbs_sp_OSBUY_FetchOneBuy", wkParm);
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
            {
                buyModel = JsonConvert.DeserializeObject<Buy>(ResultModel.JsonData);
                if (buyModel.cluster == "FTB")
                    buyModel.theFTB = FetchAllFTB(pm_viewID).FirstOrDefault();
            }

            return buyModel;
        }


        public List<BuyFTB> FetchAllFTB(int? pm_viewID = null)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<BuyFTB> buyList = new List<BuyFTB>();

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pm_viewID", SqlDbType = SqlDbType.Int, Value = pm_viewID},
                new SqlParameter() {ParameterName = "@pm_account_id", SqlDbType = SqlDbType.VarChar, Value = GlobalVariables.MySession.Account}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_OS_BuySheet, "sbs_sp_OSBUY_FetchFTB", wkParm);
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
            {
                buyList = JsonConvert.DeserializeObject<List<BuyFTB>>(ResultModel.JsonData);
                foreach (var buy_item in buyList)
                {
                    if (buy_item.theDocuments == null)
                        buy_item.theDocuments = new List<BuyFTB_document>();

                    for (int i = 0; i < buy_item.theDocuments.Count(); i++)
                    {
                        buy_item.theDocuments[i].folder_path = Path.GetDirectoryName(buy_item.theDocuments[i].link_path);
                        buy_item.theDocuments[i].file_name = Path.GetFileName(buy_item.theDocuments[i].link_path);
                    }
                }
            }

            return buyList;
        }


        public List<BuySheet_Preload> GetList_PreloadCalendar(string pm_from_loc, string pm_to_loc, DateTime pm_required_date)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<BuySheet_Preload> theList = new List<BuySheet_Preload>();
            string pmJSON = "{'pm_from_loc': " + pm_from_loc + ", 'pm_to_loc': " + pm_to_loc + ", 'pm_required_date': '" + pm_required_date.ToString("yyyy-MM-dd") + "'}";
            pmJSON = pmJSON.Replace("'", "\"");

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.NVarChar, Value = pmJSON}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_OS_BuySheet, "sbs_sp_TransitTime_GetPreloadCalendar", wkParm);
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
                theList = JsonConvert.DeserializeObject<List<BuySheet_Preload>>(ResultModel.JsonData);

            return theList;
        }

        #endregion

        #region Rates

        public string GetRates_MetalsUnit(int pm_metal_id)
        {
            string MetalsUnit;

            switch (pm_metal_id)
            {
                case 1: //LME Copper
                    MetalsUnit = "(USD/ton)";
                    break;
                case 2: //LME Nickel
                    MetalsUnit = "(USD/ton)";
                    break;
                case 21: //Stainless Steel
                    MetalsUnit = "(TWD/KG)";
                    break;
                case 31: //Carbon Steel
                    MetalsUnit = "(TWD/KG)";
                    break;
                default:
                    MetalsUnit = "";
                    break;
            }

            return MetalsUnit;
        }

        
        public byte[] GetRates_MetalsChart(string pm_metal_name, string pm_metal_unit)
        {
            List<BuySheet_Rates_Metals> RateList = GlobalVariables.MySession.List_Rates_Metal.Where(w => w.metal_name == pm_metal_name).ToList();
            int digiCount = BitConverter.GetBytes(decimal.GetBits((decimal)RateList.Average(s => s.metal_rate))[3])[2];
            double maxY = Math.Round(RateList.Max(s => s.metal_rate), digiCount, MidpointRounding.AwayFromZero);
            double minY = Math.Round(RateList.Min(s => s.metal_rate), digiCount, MidpointRounding.AwayFromZero);
            double midY = Math.Round(Queryable.Average((new double[] { maxY, minY }).AsQueryable()), digiCount, MidpointRounding.AwayFromZero);
            maxY += maxY - midY;
            minY -= midY - minY;
            if (minY < 0)
                minY = 0;
            /*
            string temp = @"<Chart>
                      <ChartAreas>
                        <ChartArea Name=""Default"" _Template_=""All"">
                          <AxisY LineColor=""64, 64, 64, 64"" Interval=""10000"">
                            <LabelStyle Font=""Verdana, 20px"" />
                          </AxisY>
                          <AxisX>
                            <LabelStyle Font=""Verdana, 20px"" />
                          </AxisX>
                        </ChartArea>
                      </ChartAreas>
                    </Chart>";
            */
            //byte[] myChart = new System.Web.Helpers.Chart(width: 1280, height: 720, theme: temp)
            byte[] myChart = new System.Web.Helpers.Chart(width: 1280, height: 720)
            //byte[] myChart = new System.Web.Helpers.Chart(width: 1280, height: 720, theme: System.Web.Helpers.ChartTheme.Vanilla)
                .AddTitle("Metal Rates")
                .AddSeries(
                    name: pm_metal_name,
                    chartType: "Line",
                    xValue: RateList, xField: "rate_date_start",
                    yValues: RateList, yFields: "metal_rate"
                )
                .SetYAxis(
                    min: minY,
                    max: maxY,
                    title: pm_metal_unit
                )
                .GetBytes("png");

            return myChart;
        }
        

        public List<BuySheet_Rates_Metals> FetchAllRates_Metals()
        {
            int grp = 0;
            int tmpID = 0;
            List<BuySheet_Rates_Metals> RateList = null;
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };

            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_OS_BuySheet, "sbs_sp_Rates_GetMetals");
            if (ResultModel.r == 1)
            {
                RateList = JsonConvert.DeserializeObject<List<BuySheet_Rates_Metals>>(ResultModel.JsonData);
                for (int rrn = 0; rrn < RateList.Count(); rrn++)
                {
                    if (tmpID != RateList[rrn].metal_id)
                    {
                        grp += 1;
                        tmpID = RateList[rrn].metal_id;
                    }
                    RateList[rrn].i = rrn + 1;
                    RateList[rrn].group = grp;
                }

                GlobalVariables.MySession.List_Rates_Metal = null;
                GlobalVariables.MySession.List_Rates_Metal = RateList;
            }

            return RateList;
        }


        public SP_Return GoSaveRates_Metal(string pmCRUD, BuySheet_Rates_Metals pmRate)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            string pmJSON = "";

            //double check
            ResultModel = shareCommon.checkAuthorized(42);
            if (ResultModel.r == 0)
                return ResultModel;

            pmJSON = "{'pm_CRUD':'" + pmCRUD + "', 'pm_metal_id':" + pmRate.metal_id + ", 'pm_rate_date_start':'" + pmRate.rate_date_start.ToString("yyyy-MM-dd") + "', 'pm_metal_rate':" + pmRate.metal_rate.ToString() + ", 'pm_usr':'" + GlobalVariables.MySession.Account + "'}";
            pmJSON = pmJSON.Replace("'", "\"");

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.NVarChar, Value = pmJSON}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_OS_BuySheet, "sbs_sp_Rates_ManageMetals", wkParm);
            if (ResultModel.r == 1)
                ResultModel = JsonConvert.DeserializeObject<SP_Return>(ResultModel.JsonData);

            return ResultModel;
        }

        #endregion

        #region Lot

        public List<Lot_Number> FetchAllLot_AssignedNumber()
        {
            List<Lot_Number> LotList = null;
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };

            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_OS_BuySheet, "prLot_GetAssignedNumber");
            if (ResultModel.r == 1)
            {
                LotList = JsonConvert.DeserializeObject<List<Lot_Number>>(ResultModel.JsonData);
                for (int rrn = 0; rrn < LotList.Count(); rrn++)
                    LotList[rrn].i = rrn + 1;
            }

            return LotList;
        }


        public List<Lot_VendorGroup> Fetch_GroupVendor(int pm_group_id)
        {
            List<Lot_VendorGroup> VendorList = null;
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            string pmJSON = "";

            pmJSON = "{'pm_group_id':" + pm_group_id.ToString() + "}";
            pmJSON = pmJSON.Replace("'", "\"");

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.NVarChar, Value = pmJSON}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_OS_BuySheet, "prLot_GetVenodrGroup", wkParm);
            if (ResultModel.r == 1)
            {
                VendorList = JsonConvert.DeserializeObject<List<Lot_VendorGroup>>(ResultModel.JsonData);
                for (int rrn = 0; rrn < VendorList.Count(); rrn++)
                {
                    VendorList[rrn].selected = true;
                    VendorList[rrn].i = rrn + 1;
                }
            }

            return VendorList;
        }


        public SP_Return AddLotNumber(int pm_group_id, int pm_amount)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            string pmJSON = "";

            //double check
            ResultModel = shareCommon.checkAuthorized(45);
            if (ResultModel.r == 0)
                return ResultModel;

            pmJSON = "{'pm_group_id':" + pm_group_id.ToString() + ", 'pm_amount':" + pm_amount.ToString() + ", 'pm_z_usr':'" + GlobalVariables.MySession.Account + "'}";
            pmJSON = pmJSON.Replace("'", "\"");

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.NVarChar, Value = pmJSON}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_OS_BuySheet, "prLot_AddLotNumber", wkParm);
            if (ResultModel.r == 1)
                ResultModel = JsonConvert.DeserializeObject<SP_Return>(ResultModel.JsonData);

            return ResultModel;
        }


        public SP_Return UpdateVendorGroup(List<Lot_VendorGroup> pm_Vendors)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };

            //double check
            ResultModel = shareCommon.checkAuthorized(45);
            if (ResultModel.r == 0)
                return ResultModel;

            List<Lot_VendorGroup> theVendors = pm_Vendors.FindAll(f => f.selected == true);
            string pmJSON = "";
            string vendors = String.Join(", ", theVendors.Select(v => "{'vendor_id':" + v.vendor_id.ToString() + "}").ToArray());

            pmJSON = "{'pm_group_id':" + theVendors[0].group_id.ToString() + ", 'pm_group_name':'" + theVendors[0].group_name + "', 'pm_vendors':[" + vendors + "], 'pm_z_usr':'" + GlobalVariables.MySession.Account + "'}";
            pmJSON = pmJSON.Replace("'", "\"");

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.NVarChar, Value = pmJSON}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_OS_BuySheet, "prLot_UpdateVendorGroup", wkParm);
            if (ResultModel.r == 1)
                ResultModel = JsonConvert.DeserializeObject<SP_Return>(ResultModel.JsonData);

            return ResultModel;
        }


        public SP_Return OccupyLotNumber(int pm_lot_uid, int pm_po_no, int pm_po_line, int pm_amount)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            string pmJSON = "";

            //double check
            ResultModel = shareCommon.checkAuthorized(45);
            if (ResultModel.r == 0)
                return ResultModel;

            pmJSON = "{'pm_lot_uid':" + pm_lot_uid.ToString() + ", 'pm_po_no':" + pm_po_no.ToString() + ", 'pm_po_line':" + pm_po_line.ToString() + ", 'pm_amount':" + pm_amount.ToString() + ", 'pm_z_usr':'" + GlobalVariables.MySession.Account + "'}";
            pmJSON = pmJSON.Replace("'", "\"");

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.NVarChar, Value = pmJSON}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_OS_BuySheet, "prLot_OccupyLotNumber", wkParm);
            if (ResultModel.r == 1)
                ResultModel = JsonConvert.DeserializeObject<SP_Return>(ResultModel.JsonData);

            return ResultModel;
        }

        #endregion

        #region Tooling Charge
        /*
        public List<Charge_Tooling> parseFilters_Int(List<Charge_Tooling> pmBuyList, Filters pmFilter)
        {
            var parameterExpression = Expression.Parameter(typeof(Charge_Tooling), "w");
            var property = Expression.Property(parameterExpression, pmFilter.key);

            int pmConstant_s = 0;
            if (!string.IsNullOrEmpty(pmFilter.value_s))
                pmConstant_s = Int32.Parse(pmFilter.value_s);

            int pmConstant_e = 0;
            if (!string.IsNullOrEmpty(pmFilter.value_e))
                pmConstant_e = Int32.Parse(pmFilter.value_e);

            switch (pmFilter.op)
            {
                case ">":
                    pmBuyList = pmBuyList.Where((Expression.Lambda<Func<Charge_Tooling, bool>>(Expression.GreaterThan(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case ">=":
                    pmBuyList = pmBuyList.Where((Expression.Lambda<Func<Charge_Tooling, bool>>(Expression.GreaterThanOrEqual(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case "<":
                    pmBuyList = pmBuyList.Where((Expression.Lambda<Func<Charge_Tooling, bool>>(Expression.LessThan(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case "<=":
                    pmBuyList = pmBuyList.Where((Expression.Lambda<Func<Charge_Tooling, bool>>(Expression.LessThanOrEqual(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case "!=":
                    pmBuyList = pmBuyList.Where((Expression.Lambda<Func<Charge_Tooling, bool>>(Expression.NotEqual(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case "~":
                    pmBuyList = pmBuyList.Where((Expression.Lambda<Func<Charge_Tooling, bool>>(Expression.GreaterThanOrEqual(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    pmBuyList = pmBuyList.Where((Expression.Lambda<Func<Charge_Tooling, bool>>(Expression.LessThanOrEqual(property, Expression.Constant(pmConstant_e)), parameterExpression)).Compile()).ToList();
                    break;
                default:
                    pmBuyList = pmBuyList.Where((Expression.Lambda<Func<Charge_Tooling, bool>>(Expression.Equal(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
            }

            return pmBuyList;
        }


        public List<Charge_Tooling> parseFilters_DateTime(List<Charge_Tooling> pmBuyList, Filters pmFilter)
        {
            var parameterExpression = Expression.Parameter(typeof(Charge_Tooling), "w");
            var property = Expression.Property(parameterExpression, pmFilter.key);

            DateTime? pmConstant_s = null;
            if (string.IsNullOrEmpty(pmFilter.value_s))
                return pmBuyList;
            pmConstant_s = Convert.ToDateTime(pmFilter.value_s);

            DateTime? pmConstant_e = null;
            if (!string.IsNullOrEmpty(pmFilter.value_e))
                pmConstant_e = Convert.ToDateTime(pmFilter.value_e);

            switch (pmFilter.op)
            {
                case ">":
                    pmBuyList = pmBuyList.Where((Expression.Lambda<Func<Charge_Tooling, bool>>(Expression.GreaterThan(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case ">=":
                    pmBuyList = pmBuyList.Where((Expression.Lambda<Func<Charge_Tooling, bool>>(Expression.GreaterThanOrEqual(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case "<":
                    pmBuyList = pmBuyList.Where((Expression.Lambda<Func<Charge_Tooling, bool>>(Expression.LessThan(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case "<=":
                    pmBuyList = pmBuyList.Where((Expression.Lambda<Func<Charge_Tooling, bool>>(Expression.LessThanOrEqual(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case "!=":
                    pmBuyList = pmBuyList.Where((Expression.Lambda<Func<Charge_Tooling, bool>>(Expression.NotEqual(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case "~":
                    pmBuyList = pmBuyList.Where((Expression.Lambda<Func<Charge_Tooling, bool>>(Expression.GreaterThanOrEqual(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    pmBuyList = pmBuyList.Where((Expression.Lambda<Func<Charge_Tooling, bool>>(Expression.LessThanOrEqual(property, Expression.Constant(pmConstant_e)), parameterExpression)).Compile()).ToList();
                    break;
                default:
                    pmBuyList = pmBuyList.Where((Expression.Lambda<Func<Charge_Tooling, bool>>(Expression.Equal(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
            }

            return pmBuyList;
        }


        public List<Charge_Tooling> parseFilters_Decimal(List<Charge_Tooling> pmBuyList, Filters pmFilter)
        {
            var parameterExpression = Expression.Parameter(typeof(Charge_Tooling), "w");
            var property = Expression.Property(parameterExpression, pmFilter.key);

            decimal pmConstant_s = 0;
            if (!string.IsNullOrEmpty(pmFilter.value_s))
                pmConstant_s = Decimal.Parse(pmFilter.value_s);

            decimal pmConstant_e = 0;
            if (!string.IsNullOrEmpty(pmFilter.value_e))
                pmConstant_e = Decimal.Parse(pmFilter.value_e);

            switch (pmFilter.op)
            {
                case ">":
                    pmBuyList = pmBuyList.Where((Expression.Lambda<Func<Charge_Tooling, bool>>(Expression.GreaterThan(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case ">=":
                    pmBuyList = pmBuyList.Where((Expression.Lambda<Func<Charge_Tooling, bool>>(Expression.GreaterThanOrEqual(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case "<":
                    pmBuyList = pmBuyList.Where((Expression.Lambda<Func<Charge_Tooling, bool>>(Expression.LessThan(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case "<=":
                    pmBuyList = pmBuyList.Where((Expression.Lambda<Func<Charge_Tooling, bool>>(Expression.LessThanOrEqual(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case "!=":
                    pmBuyList = pmBuyList.Where((Expression.Lambda<Func<Charge_Tooling, bool>>(Expression.NotEqual(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
                case "~":
                    pmBuyList = pmBuyList.Where((Expression.Lambda<Func<Charge_Tooling, bool>>(Expression.GreaterThanOrEqual(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    pmBuyList = pmBuyList.Where((Expression.Lambda<Func<Charge_Tooling, bool>>(Expression.LessThanOrEqual(property, Expression.Constant(pmConstant_e)), parameterExpression)).Compile()).ToList();
                    break;
                default:
                    pmBuyList = pmBuyList.Where((Expression.Lambda<Func<Charge_Tooling, bool>>(Expression.Equal(property, Expression.Constant(pmConstant_s)), parameterExpression)).Compile()).ToList();
                    break;
            }

            return pmBuyList;
        }


        public List<Charge_Tooling> parseFilters_String(List<Charge_Tooling> pmBuyList, Filters pmFilter)
        {
            List<Charge_Tooling> result = new List<Charge_Tooling>();
            List<Charge_Tooling> rejectResult = new List<Charge_Tooling>();
            string pmConstant_s = pmFilter.value_s == null ? "" : pmFilter.value_s;
            var getter = getModel.GetPropertyGetter(typeof(Charge_Tooling).ToString(), pmFilter.key);

            switch (pmFilter.op)
            {
                case ">=":
                    result = pmBuyList.Where(i => ((getter(i) as string) == null ? "" : (getter(i) as string)).StartsWith(pmConstant_s)).ToList();
                    break;
                case "<=":
                    result = pmBuyList.Where(i => ((getter(i) as string) == null ? "" : (getter(i) as string)).EndsWith(pmConstant_s)).ToList();
                    break;
                case "%":
                    result = pmBuyList.Where(i => ((getter(i) as string) == null ? "" : (getter(i) as string)).ToLower().Contains(pmConstant_s.ToLower())).ToList();
                    break;
                case "!%":
                    rejectResult = pmBuyList.Where(i => ((getter(i) as string) == null ? "" : (getter(i) as string)).ToLower().Contains(pmConstant_s.ToLower())).ToList();
                    result = pmBuyList.Except(rejectResult).ToList();
                    break;
                case "=":
                    result = pmBuyList.FindAll(i => ((getter(i) as string) == null ? "" : (getter(i) as string)).Equals(pmConstant_s));
                    break;
                case "!=":
                    result = pmBuyList.Where(i => ((getter(i) as string) == null ? "" : (getter(i) as string)) != pmConstant_s).ToList();
                    break;
                default:
                    result = pmBuyList;
                    break;
            }

            return result;
        }
        */

        public MemoryStream Get_ReceiptsSheet()
        {
            string version = "20230914";
            var stream = new MemoryStream();

            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            using (var package = new ExcelPackage(stream))
            {
                var workSheet = package.Workbook.Worksheets.Add(version);
                workSheet.Cells["A1"].LoadFromCollection(GlobalVariables.MySession.List_Charge_ToolingReceipts, true);

                workSheet.Column(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Column(2).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                workSheet.Column(3).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Column(3).Style.Numberformat.Format = "yyyy-MM-dd";
                workSheet.Column(4).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Column(5).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                workSheet.Column(6).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                workSheet.Column(6).Style.Numberformat.Format = "#,##0";

                workSheet.Cells[workSheet.Dimension.Address].AutoFitColumns();
                workSheet.View.FreezePanes(2, 1);
                package.Save();
            }
            stream.Position = 0;

            return stream;
        }


        public List<Charge_ToolingReceipts> FetchToolingChargerReceipts(string pm_vendors, string pm_item_id, string pm_real_part_no, DateTime pm_refund_date)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<Charge_ToolingReceipts> theReceipts = new List<Charge_ToolingReceipts>();
            string pmJSON = "{'pm_vendors':'" + pm_vendors + "', 'pm_item_id':'" + pm_item_id + "', 'pm_real_part_no':'" + pm_real_part_no + "', 'pm_refund_date':'" + pm_refund_date.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'}";
            pmJSON = pmJSON.Replace("'", "\"");

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.VarChar, Value = pmJSON}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_OS_BuySheet, "prToolingCharge_Received", wkParm);
            if (ResultModel.r == 1)
                theReceipts = JsonConvert.DeserializeObject<List<Charge_ToolingReceipts>>(ResultModel.JsonData);

            GlobalVariables.MySession.List_Charge_ToolingReceipts = null;
            GlobalVariables.MySession.List_Charge_ToolingReceipts = theReceipts;
            return theReceipts;
        }


        public List<Charge_Tooling> FetchToolingCharge_Archive()
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<Charge_Tooling> theCharge = new List<Charge_Tooling>();

            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_OS_BuySheet, "prToolingCharge_Archive");
            if (ResultModel.r == 1)
                theCharge = JsonConvert.DeserializeObject<List<Charge_Tooling>>(ResultModel.JsonData);

            return theCharge;
        }


        public List<Filters> getToolingChargeFilters()
        {
            List<Filters> FilterList = GlobalVariables.MySession.List_Charge_ToolingFilters;

            if (FilterList == null)
                FilterList = new List<Filters>();

            if (FilterList.Count() <= 0)
            {
                FilterList.Add(new Filters() { key = "po_no", key_name = "PO no", op = "", value_type = "int", value_s = "", value_e = "" });
                FilterList.Add(new Filters() { key = "item_id", key_name = "Item ID", op = "", value_type = "text", value_s = "", value_e = "" });
                FilterList.Add(new Filters() { key = "real_part_no", key_name = "Real part no", op = "", value_type = "text", value_s = "", value_e = "" });
                FilterList.Add(new Filters() { key = "vendors", key_name = "Supplier ID", op = "", value_type = "text", value_s = "", value_e = "" });
            }

            return FilterList;
        }


        public void PostToolingChargeFilter(List<Filters> pmFilterList)
        {
            GlobalVariables.MySession.List_Charge_ToolingFilters = null;
            if (pmFilterList != null)
                GlobalVariables.MySession.List_Charge_ToolingFilters = pmFilterList.ToList();
        }


        public List<Charge_Tooling> Get_ToolingCharge()
        {
            List<Charge_Tooling> TCList = FetchToolingCharge();
            List<Filters> FilterList = getToolingChargeFilters();

            if (TCList != null && FilterList != null)
            {
                foreach (var item in FilterList)
                {
                    if (String.IsNullOrEmpty(item.op))
                        continue;

                    switch (item.value_type)
                    {
                        case "int":
                            TCList = shareFilters.parseFilters_Int<Charge_Tooling>(TCList, item);
                            break;
                        case "number":
                            TCList = shareFilters.parseFilters_Decimal<Charge_Tooling>(TCList, item);
                            break;
                        case "date":
                            TCList = shareFilters.parseFilters_DateTime<Charge_Tooling>(TCList, item);
                            break;
                        default:
                            TCList = shareFilters.parseFilters_String<Charge_Tooling>(TCList, item);
                            break;
                    }
                }
            }

            return TCList;
        }


        private List<Charge_Tooling> FetchToolingCharge()
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<Charge_Tooling> theCharge = new List<Charge_Tooling>();

            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_OS_BuySheet, "prToolingCharge");
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
                theCharge = JsonConvert.DeserializeObject<List<Charge_Tooling>>(ResultModel.JsonData);

            //GlobalVariables.MySession.List_Charge_Tooling = null;
            //GlobalVariables.MySession.List_Charge_Tooling = theCharge;
            return theCharge;
        }


        /*
        public SP_Return Go_RefundedToolingCharge(List<Charge_Tooling> pm_tc)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };

            //double check
            ResultModel = shareCommon.checkAuthorized(65);
            if (ResultModel.r == 0)
                return ResultModel;

            List<Charge_Tooling> pmUpdate = pm_tc.Where(w => w.refund).ToList();
            string pmJSON = "{'pm_usr':'" + GlobalVariables.MySession.Account + "', 'theTC':" + JsonConvert.SerializeObject(pmUpdate) + "}";
            pmJSON = pmJSON.Replace("'", "\"");

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.VarChar, Value = pmJSON}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_OS_BuySheet, "prToolingCharge_Refund", wkParm);
            if (ResultModel.r == 1)
                ResultModel = JsonConvert.DeserializeObject<SP_Return>(ResultModel.JsonData);

            GlobalVariables.MySession.List_Charge_Tooling = null;
            if (ResultModel.r == 1)
                GlobalVariables.MySession.List_Charge_Tooling = pm_tc.Except(pmUpdate).ToList();
            else
                GlobalVariables.MySession.List_Charge_Tooling = pm_tc;

            return ResultModel;
        }
        */
        public async Task<SP_Return> Go_RefundedToolingCharge2(List<Charge_Tooling> pm_tc)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };

            //double check
            ResultModel = shareCommon.checkAuthorized(65);
            if (ResultModel.r == 0)
                return ResultModel;

            List<Charge_Tooling> pmUpdate = pm_tc.Where(w => w.refund).ToList();
            string pmJSON = "{'cmd_RefundTC':{'P21URL':1, 'pm_JSON':'{'pm_usr':'" + GlobalVariables.MySession.Account + "', 'theTC':" + JsonConvert.SerializeObject(pmUpdate) + "}'}}";
            pmJSON = pmJSON.Replace("'", "\"");

            ResultModel = await shareCommon.callSBS_API_Post("BuyApp/RefundTCAsync", pmJSON);
            /*
            GlobalVariables.MySession.List_Charge_Tooling = null;
            if (ResultModel.r == 1)
                GlobalVariables.MySession.List_Charge_Tooling = pm_tc.Except(pmUpdate).ToList();
            else
                GlobalVariables.MySession.List_Charge_Tooling = pm_tc;
            */
            return ResultModel;
        }

        #endregion

        #region Asia Country

        public List<Asia_Country> Fetch_AsiaCountry()
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<Asia_Country> theCountry = new List<Asia_Country>();

            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_OS_BuySheet, "prAsiaCountry_GetList");
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
                theCountry = JsonConvert.DeserializeObject<List<Asia_Country>>(ResultModel.JsonData);

            return theCountry;
        }


        public SP_Return Save_AsiaCountries(List<Asia_Country> pm_countries)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };

            //double check
            ResultModel = shareCommon.checkAuthorized(79);
            if (ResultModel.r == 0)
                return ResultModel;

            List<Asia_Country> pmUpdate = pm_countries.Where(w => w.is_asia).ToList();
            string pmJSON = "{'pm_usr':'" + GlobalVariables.MySession.Account + "', 'pm_countries':" + JsonConvert.SerializeObject(pmUpdate) + "}";
            pmJSON = pmJSON.Replace("'", "\"");

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.NVarChar, Value = pmJSON}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_OS_BuySheet, "prAsiaCountry_UpdateList", wkParm);
            if (ResultModel.r == 1)
                ResultModel = JsonConvert.DeserializeObject<SP_Return>(ResultModel.JsonData);

            return ResultModel;
        }

        #endregion

        #region Payment Terms

        public List<Vendor_PaymentTerms> Fetch_VendorPaymentTerms()
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<Vendor_PaymentTerms> thePaymentTerms = new List<Vendor_PaymentTerms>();

            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_OS_BuySheet, "SBS.P21.pr_OSBuy_VendorPaymentTerms_Get");
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
                thePaymentTerms = JsonConvert.DeserializeObject<List<Vendor_PaymentTerms>>(ResultModel.JsonData);

            return thePaymentTerms;
        }

        #endregion

        #region Inspection

        public List<Supplier> Fetch_InspectionFree()
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<Supplier> theSuppliers = new List<Supplier>();

            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_OS_BuySheet, "prInspectionFree_GetList");
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
                theSuppliers = JsonConvert.DeserializeObject<List<Supplier>>(ResultModel.JsonData);

            return theSuppliers;
        }


        public SP_Return Update_InspectionFree(string pm_option, int pm_suppler_id)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };

            //double check
            ResultModel = shareCommon.checkAuthorized(103);
            if (ResultModel.r == 0)
                return ResultModel;

            string pmJSON = "{'usr':'" + GlobalVariables.MySession.Account + "', 'opt':'" + pm_option + "', 'supplier_id':" + pm_suppler_id.ToString() + "}";
            pmJSON = pmJSON.Replace("'", "\"");

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.NVarChar, Value = pmJSON}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_OS_BuySheet, "prInspectionFree_UpdateList", wkParm);
            if (ResultModel.r == 1)
                ResultModel = JsonConvert.DeserializeObject<SP_Return>(ResultModel.JsonData);

            return ResultModel;
        }

        #endregion

    }
}
