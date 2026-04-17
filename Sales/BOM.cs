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
using System.Reflection;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;

namespace Sales
{
    public class BOM_Program
    {
        public bool isDev = false;
        public string devMode = null;
        //private SqlConnection SBScon = GlobalConfig.MyConfig.Connection_SBS;
        //private SqlConnection OScon = GlobalConfig.MyConfig.Connection_OS_BuySheet;
        Common_Share shareCommon = new Common_Share();


        public BOM_Program()
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(GlobalConfig.MyConfig.Connection_SBS.ConnectionString);

            if (builder.InitialCatalog.ToLower().Contains("dev"))
            {
                isDev = true;
                devMode = "You are using PLAY mode for BOM app now.";
            }
        }


        public List<BOM_income> GetBOM_forecast_incomes(int i, int pm_week)
        {
            List<BOM_income> theIncomes = new List<BOM_income>();
            string weekName = "week" + pm_week.ToString("00");

            BOM_forecast theForecast = GlobalVariables.MySession.List_BOM_forecast.Find(f => f.i == i);
            theIncomes = ((BOM_week)theForecast.GetType().GetProperty(weekName).GetValue(theForecast, null)).forecast_incomes;

            return theIncomes;
        }


        public BOM_data FetchBOM_ASMLTW()
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            BOM_data theData = new BOM_data();

            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "P21.prBOM_ASMLTW");
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
            {
                theData = JsonConvert.DeserializeObject<BOM_data>(ResultModel.JsonData);
                GlobalVariables.MySession.List_BOM_POs = null;
                GlobalVariables.MySession.List_BOM_POs = theData.pos;
            }

            return theData;
        }


        public SP_Return PrepareBOM_ASMLTW()
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };

            if (GlobalVariables.MySession.List_BOM_forecast != null)
                return ResultModel;

            List<BOM_forecast> theBOM = new List<BOM_forecast>();
            BOM_data theData = FetchBOM_ASMLTW();
            DateTime thisWeekStart = theData.thisWeekStart;
            //DateTime thisWeekPreviousDate = theData.thisWeekPreviousDate;
            int i = 0;
            BOM_week wkWeek = new BOM_week();

            if (theData.items == null || theData.items.Count() <= 0)
            {
                ResultModel.r = 0;
                ResultModel.msg = "Items not found.";
                return ResultModel;
            }
            if (theData.pos == null || theData.pos.Count() <= 0)
            {
                ResultModel.r = 0;
                ResultModel.msg = "PO not found.";
                //return ResultModel;
            }

            theBOM.AddRange(theData.items.Select(s => new BOM_forecast { i = ++i, customer_part_no = s.customer_part_no, item_id = s.item_id, item_desc = s.item_id + System.Environment.NewLine + s.item_desc, qty_on_hand = Decimal.ToInt32(s.qty_on_hand), qty_past_due = Decimal.ToInt32(s.qty_past_due), UsagePerWeek = Decimal.ToInt32(s.UsagePerWeek), ShortageWeek = 1000, RollingROP = Decimal.ToInt32(s.UsagePerWeek) * 13, week01 = new BOM_week { final_qty = 0, income = false, income_qty = 0, forecast_incomes = new List<BOM_income>() } }));
            for (int j = 0; j < theBOM.Count(); j++)
                theBOM[j] = CalculateBOMforOneItem(theBOM[j], thisWeekStart);

            GlobalVariables.MySession.List_BOM_forecast = null;
            GlobalVariables.MySession.List_BOM_forecast = theBOM;
            return ResultModel;
        }


        public BOM_forecast CalculateBOMforOneItem(BOM_forecast oneItem, DateTime thisWeekStart)
        {
            DateTime wkDateStart, wkDateEnd;
            int tmp_qty_on_hand;
            string weekName = "";
            BOM_week wkWeek = new BOM_week();

            wkDateStart = thisWeekStart;
            wkDateEnd = thisWeekStart.AddDays(-1).AddDays(7);
            //item.week01.start = wkDateStart;
            //item.week01.end = wkDateEnd;
            oneItem.UsageModified = 0;
            wkWeek.period = wkDateStart.ToString("yyyy/MM/dd") + System.Environment.NewLine + wkDateEnd.ToString("yyyy/MM/dd");
            wkWeek.forecast_incomes = new List<BOM_income>();
            wkWeek.forecast_incomes.AddRange(GlobalVariables.MySession.List_BOM_POs.FindAll(f => f.customer_part_no == oneItem.customer_part_no && wkDateStart <= f.expected_date && f.expected_date <= wkDateEnd).Select(s => new BOM_income { po_no = s.po_no, expected_date = s.expected_date, po_qty = s.po_qty }));
            if (wkWeek.forecast_incomes.Count() > 0)
            {
                wkWeek.income = true;
                wkWeek.income_qty = Decimal.ToInt32(wkWeek.forecast_incomes.Sum(s => s.po_qty));
            }
            tmp_qty_on_hand = oneItem.qty_on_hand;
            wkWeek.final_qty = tmp_qty_on_hand + wkWeek.income_qty - oneItem.UsagePerWeek;
            if (oneItem.ShortageWeek == 1000 && wkWeek.final_qty <= 0)
                oneItem.ShortageWeek = 1;
            tmp_qty_on_hand = wkWeek.final_qty;
            oneItem.week01 = wkWeek;

            for (int j = 2; j <= 52; j++)
            {
                wkDateStart = wkDateStart.AddDays(7);
                wkDateEnd = wkDateEnd.AddDays(7);

                wkWeek = new BOM_week();
                //wkWeek.start = wkDateStart;
                //wkWeek.end = wkDateEnd;
                wkWeek.period = wkDateStart.ToString("yyyy/MM/dd") + System.Environment.NewLine + wkDateEnd.ToString("yyyy/MM/dd");
                wkWeek.final_qty = 0;
                wkWeek.income = false;
                wkWeek.income_qty = 0;
                wkWeek.forecast_incomes = new List<BOM_income>();
                wkWeek.forecast_incomes.AddRange(GlobalVariables.MySession.List_BOM_POs.FindAll(f => f.customer_part_no == oneItem.customer_part_no && wkDateStart <= f.expected_date && f.expected_date <= wkDateEnd).Select(s => new BOM_income { po_no = s.po_no, expected_date = s.expected_date, po_qty = s.po_qty }));
                if (wkWeek.forecast_incomes.Count() > 0)
                {
                    wkWeek.income = true;
                    wkWeek.income_qty = Decimal.ToInt32(wkWeek.forecast_incomes.Sum(s => s.po_qty));
                }
                wkWeek.final_qty = tmp_qty_on_hand + wkWeek.income_qty - oneItem.UsagePerWeek;
                if (oneItem.ShortageWeek == 1000 && wkWeek.final_qty <= 0)
                    oneItem.ShortageWeek = j;
                tmp_qty_on_hand = wkWeek.final_qty;

                weekName = "week" + j.ToString("00");
                oneItem.GetType().GetProperty(weekName, BindingFlags.Public | BindingFlags.Instance).SetValue(oneItem, wkWeek, null);
            }

            return oneItem;
        }


        public void ReCalculateBOM(List<BOM_forecast> pm_BOM, DateTime pm_WeekStart)
        {
            //List<BOM_forecast> wkBOM = pm_BOM.Where(w => w.UsageModified == 1).ToList();

            for (int j = 0; j < pm_BOM.Count(); j++)
                if (pm_BOM[j].UsageModified != pm_BOM[j].UsagePerWeek)
                {
                    pm_BOM[j].UsagePerWeek = pm_BOM[j].UsageModified;
                    pm_BOM[j].ShortageWeek = 1000;
                    pm_BOM[j].RollingROP = pm_BOM[j].UsagePerWeek * 13;
                    GlobalVariables.MySession.List_BOM_forecast[j] = CalculateBOMforOneItem(pm_BOM[j], pm_WeekStart);
                }
        }


        public MemoryStream SheetToExcel_BOM_ASMLTW()
        {
            List<BOM_forecast> theExcelData = GlobalVariables.MySession.List_BOM_forecast;
            var stream = new MemoryStream();
            int i, j;

            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            using (var package = new ExcelPackage(stream))
            {
                ExcelWorksheet workSheet = package.Workbook.Worksheets.Add(theExcelData.First().week01.period.Substring(0, 10).Replace("/", ""));

                //Second row of header
                workSheet.Cells[2, 1].Value = "Customer Part No";
                workSheet.Cells[2, 2].Value = "Usage";
                workSheet.Cells[2, 3].Value = "Rolling ROP";
                workSheet.Cells[2, 4].Value = "Qty on hand";
                workSheet.Cells[2, 5].Value = "Qty past due";
                for (j = 1; j <= 52; j++)
                    workSheet.Cells[2, j + 5].Value = j.ToString("00");

                i = 2;  //2 rows header
                foreach (var item in theExcelData)
                {
                    i++;
                    workSheet.Cells[i, 1].Value = item.customer_part_no;
                    workSheet.Cells[i, 2].Value = item.UsagePerWeek;
                    workSheet.Cells[i, 3].Value = item.RollingROP;
                    workSheet.Cells[i, 4].Value = item.qty_on_hand;
                    workSheet.Cells[i, 5].Value = item.qty_past_due;

                    for (j = 1; j <= 52; j++)
                    {
                        var thisWeek = (BOM_week)item.GetType().GetProperties().First(w => w.Name == "week" + j.ToString("00")).GetValue(item, null);
                        workSheet.Cells[i, j + 5].Value = thisWeek.final_qty;

                        if (thisWeek.final_qty <= (item.UsagePerWeek * 8))
                        {
                            workSheet.Cells[i, j + 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            workSheet.Cells[i, j + 5].Style.Fill.BackgroundColor.SetColor(Color.LightPink);
                        }
                        else if (thisWeek.final_qty <= (item.UsagePerWeek * 13))
                        {
                            workSheet.Cells[i, j + 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            workSheet.Cells[i, j + 5].Style.Fill.BackgroundColor.SetColor(Color.LightYellow);
                        }

                        if (thisWeek.final_qty <= 0)
                            workSheet.Cells[i, j + 5].Style.Font.Color.SetColor(Color.Red);
                        if (thisWeek.income || thisWeek.final_qty <= 0)
                            workSheet.Cells[i, j + 5].Style.Font.Bold = true;

                        //First row of header
                        workSheet.Cells[1, j + 5].Value = thisWeek.period;
                        workSheet.Cells[1, j + 5].Style.WrapText = true;
                    }
                }

                workSheet.Column(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Column(1).Width = 16;
                workSheet.Column(2).Style.Numberformat.Format = "#,##0";
                workSheet.Column(2).Width = 8;
                workSheet.Column(3).Style.Numberformat.Format = "#,##0";
                workSheet.Column(3).Width = 8;
                workSheet.Column(4).Style.Numberformat.Format = "#,##0";
                workSheet.Column(4).Width = 8;
                workSheet.Column(5).Style.Numberformat.Format = "#,##0";
                workSheet.Column(5).Width = 8;
                //workSheet.Column(7).Style.Numberformat.Format = "yyyy-MM-dd";
                for (j = 1; j <= 52; j++)
                {
                    workSheet.Column(j + 5).Style.Numberformat.Format = "#,##0";
                    workSheet.Column(j + 5).Width = 10.5;
                }

                workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Row(2).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                //workSheet.Cells[workSheet.Dimension.Address].AutoFitColumns();
                workSheet.View.FreezePanes(3, 3);
                //workSheet.Cells[workSheet.Dimension.Address].AutoFilter = true;
                package.Save();
            }
            stream.Position = 0;

            return stream;
        }


        public BOM_ProcureInfo GetBOM_Procure_Info(int i, int pm_ShortageWeek)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            BOM_forecast theItem = GlobalVariables.MySession.List_BOM_forecast[i];
            BOM_ProcureInfo theInfo = new BOM_ProcureInfo();
            string shortage_date = "";
            string pmJson = "";

            var thisWeek = (BOM_week)theItem.GetType().GetProperties().First(w => w.Name == "week" + pm_ShortageWeek.ToString("00")).GetValue(theItem, null);
            shortage_date = thisWeek.period.Substring(0, 10);

            pmJson = "{'item_id':'" + theItem.item_id.Replace(System.Environment.NewLine, ",") + "', 'shortage_date':'" + shortage_date + "', 'shortage_qty':" + theItem.week52.final_qty.ToString() + "}";
            pmJson = pmJson.Replace("'", "\"");
            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.VarChar, Value = pmJson}
            };

            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "P21.prBOM_ASMLTW_ProcureInfo", wkParm);
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
                theInfo = JsonConvert.DeserializeObject<BOM_ProcureInfo>(ResultModel.JsonData);
            theInfo.customer_part_no = theItem.customer_part_no;
            theInfo.ShortageWeek = pm_ShortageWeek;

            return theInfo;
        }


        public SP_Return PostNewBuy_BOM_ASMLTW(BOM_ProcureInfo pm_Info)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            string pmJSON = "";

            //double check
            ResultModel = shareCommon.checkAuthorized(78);
            if (ResultModel.r == 0)
                return ResultModel;

            pmJSON += "'entryID': '" + Convert.ToString(Guid.NewGuid()).ToUpper() + "', ";
            pmJSON += "'ItemID': '" + pm_Info.item_id + "', ";
            pmJSON += "'BuyToLoc': '13', ";
            pmJSON += "'Sourcing': 'N', ";
            pmJSON += "'NewQuantity': '" + pm_Info.BuyQty.ToString() + "', ";
            pmJSON += "'required_date': '" + pm_Info.RequireDate.ToString("yyyy-MM-dd") + "' ";
            pmJSON = "{'pm_usr': '" + GlobalVariables.MySession.Account + "', 'NewBuys': [{" + pmJSON + "}]}";
            pmJSON = pmJSON.Replace("'", "\"");

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.VarChar, Value = pmJSON}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_OS_BuySheet, "sbs_sp_OSBUY_PostFromBOM", wkParm);
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
                ResultModel = JsonConvert.DeserializeObject<SP_Return>(ResultModel.JsonData);

            return ResultModel;
        }

    }
}
