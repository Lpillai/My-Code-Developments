using Microsoft.Reporting.WebForms;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Procurement;
using RuntimeVariables;
using RuntimeConfig;
using SharedScrpits;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reports
{
    public class RPT_Procurement_Program
    {
        //private SqlConnection SBScon = GlobalConfig.MyConfig.Connection_SBS;
        //private SqlConnection OScon = GlobalConfig.MyConfig.Connection_OS_BuySheet;
        Common_Share shareCommon = new Common_Share();
        Reports_Program pgmReports = new Reports_Program();
        SupplierScorecard_Program pgmSupplierScorecard = new SupplierScorecard_Program();


        #region BuySheet

        public List<Lot_PO> Lot_GetPOData(string pm_PO)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<Lot_PO> thePOList = new List<Lot_PO>();
            string pmJSON = "";

            pmJSON = "{'pm_PO':" + pm_PO + "}";
            pmJSON = pmJSON.Replace("'", "\"");

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.NVarChar, Value = pmJSON}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_OS_BuySheet, "prLot_GetLotFromPO", wkParm);
            if (ResultModel.r == 1)
                thePOList = JsonConvert.DeserializeObject<List<Lot_PO>>(ResultModel.JsonData);
            if (thePOList != null)
                thePOList = thePOList.Where(w => w.line_qty > 0).ToList();

            return thePOList;
        }


        private SP_Return Lot_GoUpdateMFG(string pmJson)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };

            //double check
            ResultModel = shareCommon.checkReportAuthorized(1);
            if (ResultModel.r == 0)
                return ResultModel;

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.NVarChar, Value = pmJson}
            };

            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_OS_BuySheet, "prLot_UpdateMFGLot", wkParm);
            if (ResultModel.r == 1)
                ResultModel = JsonConvert.DeserializeObject<SP_Return>(ResultModel.JsonData);

            return ResultModel;
        }


        public MemoryStream Lot_OpenLabel_Carton(string rpt, string pm_PO, List<Lot_PO> pm_Lots)
        {
            string pmJSON = "";
            List<string> box_qty = new List<string>();
            List<Lot_CartonQty> List_box = new List<Lot_CartonQty>();

            List<Lot_CartonString> List_Lot = (
                    from data in pm_Lots.Where(l => string.IsNullOrWhiteSpace(l.carton_string) != true)
                    select new Lot_CartonString()
                    {
                        lot_no = data.lot_no,
                        mfg_lot = data.mfg_lot,
                        carton_string = data.carton_string.Replace(" ", "")
                    }
                ).ToList();
            foreach (var item in List_Lot)
            {
                box_qty.Clear();
                box_qty = item.carton_string.Replace(" ", "").Split(',').Where(q => string.IsNullOrWhiteSpace(q) != true).ToList();
                List_box.AddRange((
                            from data in box_qty
                            select new Lot_CartonQty()
                            {
                                lot_no = item.lot_no,
                                mfg_lot = item.mfg_lot,
                                qty = int.Parse(data)
                            }
                        ).ToList()
                    );
            }
            pmJSON = "{'pm_usr':'" + GlobalVariables.MySession.Account + "','pm_PO':" + pm_PO + ",'pm_carton_qty':" + JsonConvert.SerializeObject(List_box) + "}";
            pmJSON = pmJSON.Replace("'", "\"");

            if (Lot_GoUpdateMFG(pmJSON).r == 0)
                return null;
            else
            {
                ReportParameterCollection parms = new ReportParameterCollection();
                parms.Add(new ReportParameter("pmJSON", pmJSON));

                return pgmReports.RenderReportToStream(pgmReports.GenerateReport(rpt, parms), 0);
            }
        }


        public SP_Return Go_Audit_item_maintenance()
        {
            return shareCommon.AddScheduleForAPI("SBS.z.prSBS_API_GET2", "@pm_query='RPT/audit_item_maintenance_os', @out_result=''", false);
        }


        public MemoryStream OSBuyHistory_ProduceSheet(DateTime pm_date_start, DateTime pm_date_end)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            OSBuyHistory theHistory = Fetch_OSBuyHistory(pm_date_start, pm_date_end);
            var stream = new MemoryStream();
            int i, j, rrnCount;

            try
            {
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                using (var package = new ExcelPackage(stream))
                {
                    //1st tab
                    var workSheet = package.Workbook.Worksheets.Add("Questions");
                    workSheet.Cells[1, 1].Value = "Date Start = " + pm_date_start.ToShortDateString();
                    workSheet.Cells[2, 1].Value = "Date End = " + pm_date_end.ToShortDateString();
                    workSheet.Cells[4, 1].Value = "How many parts did we source and then buy?";
                    workSheet.Cells[5, 1].Value = theHistory.q1;
                    workSheet.Cells[5, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    workSheet.Cells[5, 1].Style.Numberformat.Format = "#,##0";
                    workSheet.Cells[7, 1].Value = "How many parts did we source and not buy?";
                    workSheet.Cells[8, 1].Value = theHistory.q2;
                    workSheet.Cells[8, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    workSheet.Cells[8, 1].Style.Numberformat.Format = "#,##0";
                    workSheet.Cells[10, 1].Value = "Which suppliers were included in the sourcing?";
                    workSheet.Cells[11, 1].Value = "Please see “Quotation” sheet";
                    workSheet.Cells[13, 1].Value = "How many items were sourced with only one supplier?  How many items were sourced with two suppliers etc…";
                    workSheet.Cells["A14"].LoadFromCollection(theHistory.theQ4, true);
                    workSheet.Column(14).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    for (i = 1; i <= 9; i++)
                    {
                        workSheet.Cells[15, i].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        workSheet.Cells[15, i].Style.Numberformat.Format = "#,##0";
                    }

                    //2nd tab
                    workSheet = null;
                    workSheet = package.Workbook.Worksheets.Add("Buy Sheet");
                    workSheet.Cells["A1"].LoadFromCollection(theHistory.theSheet, true);
                    workSheet.Column(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    workSheet.Column(5).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    workSheet.Column(6).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    workSheet.Column(7).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    workSheet.Column(8).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    workSheet.Column(9).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    workSheet.Column(9).Style.Numberformat.Format = "#,##0";
                    workSheet.Column(10).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    workSheet.Column(11).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    workSheet.Column(12).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    workSheet.Column(13).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    workSheet.Column(16).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    workSheet.Column(18).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    workSheet.Column(19).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    workSheet.Column(20).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    workSheet.Column(21).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    workSheet.Column(22).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    workSheet.Column(22).Style.Numberformat.Format = "MM/dd/yyyy HH:mm AM/PM";
                    workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    workSheet.View.FreezePanes(2, 3);
                    workSheet.Cells[workSheet.Dimension.Address].AutoFitColumns();
                    workSheet.Column(4).Width = 70;

                    //3rd tab
                    workSheet = null;
                    workSheet = package.Workbook.Worksheets.Add("Quotation");
                    workSheet.Cells["A1"].LoadFromCollection(theHistory.theQuotes, true);
                    workSheet.DeleteColumn(1);
                    workSheet.Column(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    workSheet.Column(2).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    workSheet.Column(3).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    workSheet.Column(5).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    workSheet.Column(5).Style.Numberformat.Format = "#,##0.0000";
                    workSheet.Column(6).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    workSheet.Column(6).Style.Numberformat.Format = "##0";
                    workSheet.Column(7).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    workSheet.Column(7).Style.Numberformat.Format = "#,##0.0000";
                    workSheet.Column(8).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    workSheet.Column(8).Style.Numberformat.Format = "#,##0";
                    workSheet.Column(9).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    workSheet.Column(11).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    i = 1;
                    j = 1;
                    while (i <= theHistory.theQuotes.LastOrDefault().rrn)
                    {
                        rrnCount = theHistory.theQuotes.FindAll(f => f.rrn == i).Count();
                        using (var range = workSheet.Cells[j + 1, 1, j + rrnCount, workSheet.Dimension.End.Column])
                        {
                            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            range.Style.Fill.BackgroundColor.SetColor(Color.AliceBlue);
                        }

                        j += rrnCount + theHistory.theQuotes.FindAll(f => f.rrn == i + 1).Count();
                        i += 2;
                    }

                    workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    workSheet.View.FreezePanes(2, 4);
                    workSheet.Cells[workSheet.Dimension.Address].AutoFitColumns();

                    package.Save();
                }
                stream.Position = 0;
            }
            catch (Exception ex)
            {
                ResultModel.r = 0;
                ResultModel.msg = ex.ToString();
            }

            return stream;
        }


        public OSBuyHistory Fetch_OSBuyHistory(DateTime pm_date_start, DateTime pm_date_end)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            OSBuyHistory theHistory = new OSBuyHistory();
            string pmJson = ("{'date_start':'" + pm_date_start.ToString("yyyy-MM-dd") + "', 'date_end':'" + pm_date_end.ToString("yyyy-MM-dd") + "'}").Replace("'", "\"");

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.VarChar, Value = pmJson}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "reporting.OS_BuyHistory", wkParm);
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
                theHistory = JsonConvert.DeserializeObject<OSBuyHistory>(ResultModel.JsonData);

            return theHistory;
        }

        #endregion

        #region Supplier Scorecard

        public MemoryStream SupplierScorecard_CalculateSheet(int pm_supplier_id, int pm_target)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            SupplierScorecard_raw theList = pgmSupplierScorecard.SupplierScorecard_FetchList(pm_supplier_id);
            string version = "v20231115";
            var stream = new MemoryStream();
            int i, j, k;
            string tmp_item_id = "";
            decimal tmp_unit_price = 0;
            decimal wk_target = (decimal)((100.0 - pm_target) / 100.0);

            try
            {
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                using (var package = new ExcelPackage(stream))
                {
                    var workSheet = package.Workbook.Worksheets.Add(version);

                    //header
                    workSheet.Cells[1, 1].Value = "Category \\ YYMM";
                    i = 2;
                    foreach (string wkYM in theList.ym.Split(','))
                    {
                        workSheet.Cells[1, i].Value = wkYM;
                        i++;
                    }
                    workSheet.Cells[1, 14].Value = "Sub Total";
                    workSheet.Cells[1, 15].Value = "Grand Total";
                    workSheet.Cells[1, 16].Value = "Target -" + pm_target.ToString() + "%";
                    workSheet.Cells[1, 17].Value = "Actual Percentage";

                    //Spend
                    workSheet.Cells["A2:Q2"].Merge = true;
                    workSheet.Cells["A2"].Value = "Spend";
                    workSheet.Row(2).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    workSheet.Cells[3, 1].Value = "$ of total PO placed";
                    workSheet.Cells[4, 1].Value = "YoY Change";
                    workSheet.Cells[5, 1].Value = "# of PO lines issued";
                    workSheet.Cells[6, 1].Value = "YoY Change";
                    for (i = 0; i < theList.spend.Count(); i++)
                    {
                        for (k = 2; k <= 13; k++)
                            if (theList.spend[i].spend_ym.ToString() == workSheet.Cells[1, k].Value.ToString())
                            {
                                workSheet.Cells[3, k].Value = theList.spend[i].total_po_placed;
                                workSheet.Cells[4, k].Value = theList.spend[i].yoy_total_po_placed;
                                workSheet.Cells[5, k].Value = theList.spend[i].po_lines_issued;
                                workSheet.Cells[6, k].Value = theList.spend[i].yoy_po_lines_issued;
                            }
                    }
                    using (var range = workSheet.Cells[3, 1, 6, workSheet.Dimension.End.Column])
                    {
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(Color.AliceBlue);
                    }

                    //Pricing top 5 parts
                    j = 7;
                    workSheet.Cells["A" + j.ToString() + ":Q" + j.ToString()].Merge = true;
                    workSheet.Cells["A" + j.ToString()].Value = "Pricing top 5 parts";
                    workSheet.Row(j).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    for (i = 0; i < theList.price.Count(); i++)
                    {
                        if (tmp_item_id != theList.price[i].item_id || tmp_unit_price != theList.price[i].unit_price)
                        {
                            j++;
                            workSheet.Cells[j, 1].Value = theList.price[i].item_id + " (" + theList.price[i].unit_price.ToString() + ")";
                            workSheet.Cells[j, 14].Value = theList.price[i].sub_total;

                            if (tmp_item_id == theList.price[i].item_id)
                                workSheet.Cells[j, 17].Value = (theList.price[i].unit_price - tmp_unit_price) / theList.price[i].unit_price * 100;  //Actual Percentage
                            else
                            {
                                workSheet.Cells[j, 15].Value = theList.price[i].grand_total;
                                workSheet.Cells[j, 16].Value = theList.price[i].unit_price * wk_target; //target price
                            }

                            tmp_item_id = theList.price[i].item_id;
                            tmp_unit_price = theList.price[i].unit_price;
                        }

                        for (k = 2; k <= 13; k++)
                            if (theList.price[i].spend_ym.ToString() == workSheet.Cells[1, k].Value.ToString())
                                workSheet.Cells[j, k].Value = theList.price[i].extened_value;
                    }
                    using (var range = workSheet.Cells[8, 1, j, workSheet.Dimension.End.Column])
                    {
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(Color.AliceBlue);
                    }

                    //Lines Received
                    j++;
                    workSheet.Cells["A" + j.ToString() + ":Q" + j.ToString()].Merge = true;
                    workSheet.Cells["A" + j.ToString()].Value = "Lines Received";
                    workSheet.Row(j).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    workSheet.Cells[j + 1, 1].Value = "# of total lines";
                    workSheet.Cells[j + 2, 1].Value = "# of lines late";
                    workSheet.Cells[j + 3, 1].Value = "# of lines early (-7days)";
                    workSheet.Cells[j + 4, 1].Value = "% of lines on time";
                    for (i = 0; i < theList.receive.Count(); i++)
                    {
                        for (k = 2; k <= 13; k++)
                            if (theList.receive[i].completed_ym.ToString() == workSheet.Cells[1, k].Value.ToString())
                            {
                                workSheet.Cells[j + 1, k].Value = theList.receive[i].total_lines;
                                workSheet.Cells[j + 2, k].Value = theList.receive[i].lines_late;
                                workSheet.Cells[j + 3, k].Value = theList.receive[i].lines_early;
                                workSheet.Cells[j + 4, k].Value = theList.receive[i].lines_on_time;
                            }
                    }
                    using (var range = workSheet.Cells[j + 1, 1, j + 4, workSheet.Dimension.End.Column])
                    {
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(Color.AliceBlue);
                    }

                    for (i = 2; i <= workSheet.Dimension.End.Column; i++)
                        workSheet.Column(i).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    for (i = 2; i <= 17; i++)
                        workSheet.Column(i).Style.Numberformat.Format = "#,##0.00";
                    workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    workSheet.View.FreezePanes(2, 2);
                    workSheet.Cells[workSheet.Dimension.Address].AutoFitColumns();
                    package.Save();
                }
                stream.Position = 0;
            }
            catch (Exception ex)
            {
                ResultModel.r = 0;
                ResultModel.msg = ex.ToString();
            }

            return stream;
        }
        

        public SP_Return Go_SupplierScoreCard_Summary(string pm_isLive)
        {
            string wkPath = "@pm_query='RPT/SupplierScorecard_Summary?mail_to=" + GlobalVariables.MySession.Email + "&isLive=" + pm_isLive + "', @out_result=''";
            return shareCommon.AddScheduleForAPI("SBS.z.prSBS_API_GET2", wkPath, false);
        }


        public MemoryStream PurchaseOrder(string rpt, int pm_po_no)
        {
            ReportParameterCollection parms = new ReportParameterCollection();
            parms.Add(new ReportParameter("po_no", pm_po_no.ToString()));

            return pgmReports.RenderReportToStream(pgmReports.GenerateReport(rpt, parms), 0);
        }


        public MemoryStream AggregateInspection_ProduceSheet()
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            RPT_InspectionAggregate theList = Fetch_InspectionAggregate();
            List<string> theYM = theList.theYM.Split(',').ToList();
            var stream = new MemoryStream();
            int i, ym_start;

            try
            {
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                using (var package = new ExcelPackage(stream))
                {
                    //1st tab
                    var workSheet = package.Workbook.Worksheets.Add("Summary");
                    workSheet.Cells["A1"].LoadFromCollection(theList.theTab1, true);
                    workSheet.DeleteColumn(2);
                    ym_start = 3;
                    for (i = 0; i < theYM.Count(); i++)
                        workSheet.Cells[1, ym_start + i].Value = theYM[i];

                    workSheet.Column(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    workSheet.Column(2).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    for (i = ym_start; i <= workSheet.Dimension.End.Column; i++)
                    {
                        workSheet.Column(i).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        workSheet.Column(i).Style.Numberformat.Format = "#,##0";
                    }
                    workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    i = 2;
                    while (i <= workSheet.Dimension.End.Row)
                    {
                        using (var range = workSheet.Cells[i, 1, i + 2, workSheet.Dimension.End.Column])
                        {
                            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            range.Style.Fill.BackgroundColor.SetColor(Color.AliceBlue);
                        }

                        i += 6;
                    }

                    workSheet.View.FreezePanes(2, ym_start);
                    workSheet.Cells[workSheet.Dimension.Address].AutoFitColumns();


                    //2nd tab
                    workSheet = null;
                    workSheet = package.Workbook.Worksheets.Add("By Supplier");
                    workSheet.Cells["A1"].LoadFromCollection(theList.theTab2, true);
                    ym_start = 4;
                    for (i = 0; i < theYM.Count(); i++)
                        workSheet.Cells[1, ym_start + i].Value = theYM[i];

                    workSheet.Column(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    workSheet.Column(3).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    for (i = ym_start; i <= workSheet.Dimension.End.Column; i++)
                    {
                        workSheet.Column(i).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        workSheet.Column(i).Style.Numberformat.Format = "#,##0";
                    }
                    workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    i = 2;
                    while (i <= workSheet.Dimension.End.Row)
                    {
                        using (var range = workSheet.Cells[i, 1, i + 2, workSheet.Dimension.End.Column])
                        {
                            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            range.Style.Fill.BackgroundColor.SetColor(Color.AliceBlue);
                        }

                        i += 6;
                    }

                    workSheet.View.FreezePanes(2, ym_start);
                    workSheet.Cells[workSheet.Dimension.Address].AutoFitColumns();

                    package.Save();
                }
                stream.Position = 0;
            }
            catch (Exception ex)
            {
                ResultModel.r = 0;
                ResultModel.msg = ex.ToString();
            }

            return stream;
        }


        public RPT_InspectionAggregate Fetch_InspectionAggregate()
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            RPT_InspectionAggregate theAggregation = new RPT_InspectionAggregate();

            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "reporting.AggregateInspectionReport");
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
                theAggregation = JsonConvert.DeserializeObject<RPT_InspectionAggregate>(ResultModel.JsonData);

            return theAggregation;
        }

        #endregion

    }
}
