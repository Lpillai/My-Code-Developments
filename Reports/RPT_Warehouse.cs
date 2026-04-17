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
using Warehouse;
using OfficeOpenXml;
using System.Globalization;

namespace Reports
{
    public class RPT_Warehouse_Program
    {
        //private SqlConnection SBScon = GlobalConfig.MyConfig.Connection_SBS;
        Common_Share shareCommon = new Common_Share();
        FileActions_Share shareFileActions = new FileActions_Share();
        Reports_Program pgmReports = new Reports_Program();
        Receiving_Program pgmReceiving = new Receiving_Program();
        Shipping_Program pgmShipping = new Shipping_Program();

        #region Regular

        public MemoryStream Get_StringToBarcode(string pm_kind, string pm_string, bool isText)
        {
            string rpt = "/Label/StringTo" + pm_kind + (isText ? "_withText" : "_onlyBarcode");

            ReportParameterCollection parms = new ReportParameterCollection();
            parms.Add(new ReportParameter("pmString", pm_string));

            return pgmReports.RenderReportToStream(pgmReports.GenerateReport(rpt, parms), 0);
        }

        /*
        public MemoryStream Get_CleanedPart(string rpt, string pm_lot, string pm_qty)
        {
            ReportParameterCollection parms = new ReportParameterCollection();
            parms.Add(new ReportParameter("lot", pm_lot));
            parms.Add(new ReportParameter("qty", pm_qty));

            return pgmReports.RenderReportToStream(pgmReports.GenerateReport(rpt, parms), 0);
        }
        */
        public MemoryStream Get_CleanedPart2(string rpt, string pm_customer, string pm_lot, string pm_loc_grp, string pm_qty)
        {
            if (string.IsNullOrEmpty(pm_customer))
                pm_customer = "0";

            ReportParameterCollection parms = new ReportParameterCollection();
            parms.Add(new ReportParameter("customer", pm_customer));
            parms.Add(new ReportParameter("lot", pm_lot));
            parms.Add(new ReportParameter("loc_grp", pm_loc_grp));
            parms.Add(new ReportParameter("qty", pm_qty));

            return pgmReports.RenderReportToStream(pgmReports.GenerateReport(rpt, parms), 0);
        }
        public MemoryStream Get_CleanedPart3(string rpt, string pm_process_x_transaction_uid, string pm_lot, string pm_qty)
        {
            ReportParameterCollection parms = new ReportParameterCollection();
            parms.Add(new ReportParameter("pt", pm_process_x_transaction_uid));
            parms.Add(new ReportParameter("lot", pm_lot));
            parms.Add(new ReportParameter("qty", pm_qty));

            return pgmReports.RenderReportToStream(pgmReports.GenerateReport(rpt, parms), 0);
        }


        public MemoryStream Get_LocationBins(string rpt, string pm_loc_grp, string pm_bin_id_start, string pm_bin_id_end, string pm_font_size)
        {
            ReportParameterCollection parms = new ReportParameterCollection();
            parms.Add(new ReportParameter("loc_grp", pm_loc_grp));
            parms.Add(new ReportParameter("bin_id_start", pm_bin_id_start));
            parms.Add(new ReportParameter("bin_id_end", pm_bin_id_end));
            parms.Add(new ReportParameter("FontSize", pm_font_size));

            return pgmReports.RenderReportToStream(pgmReports.GenerateReport(rpt, parms), 0);
        }


        public MemoryStream Get_RecPkgBulk(string rpt, string pm_location_id, string pm_lot_cd, string pm_qty)
        {
            ReportParameterCollection parms = new ReportParameterCollection();
            parms.Add(new ReportParameter("location_id", pm_location_id));
            parms.Add(new ReportParameter("lot_cd", pm_lot_cd));
            parms.Add(new ReportParameter("qty", pm_qty));

            return pgmReports.RenderReportToStream(pgmReports.GenerateReport(rpt, parms), 0);
        }


        public MemoryStream Get_ShipByPickTicket(string rpt, string pm_pick_ticket_no)
        {
            string pmJson = "{'pm_type': 'PT', 'pm_pick_ticket_no': " + pm_pick_ticket_no + "}";
            pmJson = pmJson.Replace("'", "\"");

            ReportParameterCollection parms = new ReportParameterCollection();
            //parms.Add(new ReportParameter("pick_ticket_no", pm_pick_ticket_no));
            parms.Add(new ReportParameter("pmJson", pmJson));

            return pgmReports.RenderReportToStream(pgmReports.GenerateReport(rpt, parms), 0);
        }


        public MemoryStream Get_ShipByOrderNo(string rpt, string pm_lot_cd, string pm_order_no, string pm_qty)
        {
            string pmJson = "{'pm_type': 'OE', 'pm_order_no': " + pm_order_no + ", 'pm_lot_cd': '" + pm_lot_cd + "', 'pm_qty': " + pm_qty + "}";
            pmJson = pmJson.Replace("'", "\"");

            ReportParameterCollection parms = new ReportParameterCollection();
            //parms.Add(new ReportParameter("lot_cd", pm_lot_cd));
            //parms.Add(new ReportParameter("order_no", pm_order_no));
            //parms.Add(new ReportParameter("qty", pm_qty));
            parms.Add(new ReportParameter("pmJson", pmJson));

            return pgmReports.RenderReportToStream(pgmReports.GenerateReport(rpt, parms), 0);
        }


        public MemoryStream Get_CustomerAddress(string rpt, string pm_id)
        {
            ReportParameterCollection parms = new ReportParameterCollection();
            parms.Add(new ReportParameter("id", pm_id));

            return pgmReports.RenderReportToStream(pgmReports.GenerateReport(rpt, parms), 0);
        }

        /*
        public MemoryStream Get_KitParts(string rpt, string pm_item_id)
        {
            ReportParameterCollection parms = new ReportParameterCollection();
            parms.Add(new ReportParameter("item_id", pm_item_id));

            return pgmReports.RenderReportToStream(pgmReports.GenerateReport(rpt, parms), 0);
        }
        */
        public MemoryStream Get_KitParts2(string rpt, string pm_customer, string pm_item_id)
        {
            ReportParameterCollection parms = new ReportParameterCollection();
            parms.Add(new ReportParameter("item_id", pm_item_id));
            parms.Add(new ReportParameter("customer_id", string.IsNullOrWhiteSpace(pm_customer) ? null : pm_customer.ToString()));

            return pgmReports.RenderReportToStream(pgmReports.GenerateReport(rpt, parms), 0);
        }


        public MemoryStream Get_Components(string rpt, string pm_item_id)
        //public MemoryStream Get_Components(string rpt, string pm_customer, string pm_item_id)
        {
            ReportParameterCollection parms = new ReportParameterCollection();
            parms.Add(new ReportParameter("pm_item_id", pm_item_id));
            //parms.Add(new ReportParameter("pm_customer_id", string.IsNullOrWhiteSpace(pm_customer) ? null : pm_customer.ToString()));

            return pgmReports.RenderReportToStream(pgmReports.GenerateReport(rpt, parms), 0);
        }


        public MemoryStream Get_Textlabel(string rpt, string pm_text)
        {
            ReportParameterCollection parms = new ReportParameterCollection();
            parms.Add(new ReportParameter("pm_text", pm_text));

            return pgmReports.RenderReportToStream(pgmReports.GenerateReport(rpt, parms), 0);
        }


        public MemoryStream Get_BoxLabel(string rpt, string pm_order_no, string pm_label_count)
        {
            ReportParameterCollection parms = new ReportParameterCollection();
            parms.Add(new ReportParameter("OrderNumber", pm_order_no));
            parms.Add(new ReportParameter("Label", pm_label_count));

            return pgmReports.RenderReportToStream(pgmReports.GenerateReport(rpt, parms), 0);
        }


        public MemoryStream Get_PartBin(string rpt, string pm_part_no)
        {
            ReportParameterCollection parms = new ReportParameterCollection();
            parms.Add(new ReportParameter("PartNo", pm_part_no));

            return pgmReports.RenderReportToStream(pgmReports.GenerateReport(rpt, parms), 0);
        }

        #endregion

        #region Label Contract Bin

        public MemoryStream Get_ContractItem2(string rpt, string pmJson)
        {
            ReportParameterCollection parms = new ReportParameterCollection();
            parms.Add(new ReportParameter("pmJSON", pmJson));

            return pgmReports.RenderReportToStream(pgmReports.GenerateReport(rpt, parms), 0);
        }

        #endregion

        #region KR Warehouse

        public MemoryStream Get_ContractItem_Building(string rpt, string pm_building, string pm_contract_bin_id)
        {
            ReportParameterCollection parms = new ReportParameterCollection();
            parms.Add(new ReportParameter("building", pm_building));
            parms.Add(new ReportParameter("contract_bin_id", pm_contract_bin_id));

            return pgmReports.RenderReportToStream(pgmReports.GenerateReport(rpt, parms), 0);
        }

        #endregion

        #region TW Warehouse

        public MemoryStream Get_ContractItem(string rpt, string pm_contract_bin_id)
        {
            ReportParameterCollection parms = new ReportParameterCollection();
            parms.Add(new ReportParameter("contract_bin_id", pm_contract_bin_id));

            return pgmReports.RenderReportToStream(pgmReports.GenerateReport(rpt, parms), 0);
        }

        #endregion

        #region MX Warehouse

        public List<Bosal_Invoice> Bosal_FetchInvoiceLine(string pm_invoice_no)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<Bosal_Invoice> theLineList = new List<Bosal_Invoice>();
            string pmJSON = "";

            pmJSON = "{'pm_invoice_no':" + pm_invoice_no + "}";
            pmJSON = pmJSON.Replace("'", "\"");

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.VarChar, Value = pmJSON}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "prIntranet_GetList_InvoiceLine", wkParm);
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
            {
                theLineList = JsonConvert.DeserializeObject<List<Bosal_Invoice>>(ResultModel.JsonData);
                foreach (var wkLine in theLineList)
                    wkLine.box_qty = 50;
            }

            return theLineList;
        }


        public MemoryStream Get_Shipping_Bosal_Box(string rpt, List<Bosal_Invoice> pmLineList)
        {
            ReportParameterCollection parms = new ReportParameterCollection();
            parms.Add(new ReportParameter("pmJSON", JsonConvert.SerializeObject(pmLineList)));

            return pgmReports.RenderReportToStream(pgmReports.GenerateReport(rpt, parms), 0);
        }


        public MemoryStream Get_BRPProduction(string rpt, string pm_job_no, string pm_item_id, string pm_lot_no, string pm_qty)
        {
            ReportParameterCollection parms = new ReportParameterCollection();
            parms.Add(new ReportParameter("JobNo", pm_job_no));
            parms.Add(new ReportParameter("ItemID", pm_item_id));
            parms.Add(new ReportParameter("LotNo", pm_lot_no));
            parms.Add(new ReportParameter("Qty", pm_qty));

            return pgmReports.RenderReportToStream(pgmReports.GenerateReport(rpt, parms), 0);
        }

        public MemoryStream Get_FinlandProduction(string rpt, string pm_job_no, string pm_item_id, string pm_lot)
        {
            ReportParameterCollection parms = new ReportParameterCollection();

            parms.Add(new ReportParameter("JobNo", pm_job_no));
            parms.Add(new ReportParameter("ItemID", pm_item_id));
            parms.Add(new ReportParameter("LotNo", pm_lot));

            return pgmReports.RenderReportToStream(
                pgmReports.GenerateReport(rpt, parms),
                0
            );
        }

        public MemoryStream Get_LotAttribute(string rpt, string pm_lot_no, string pm_location_id)
        {
            ReportParameterCollection parms = new ReportParameterCollection();

            parms.Add(new ReportParameter("LotNo", pm_lot_no));
            parms.Add(new ReportParameter("LocationID", pm_location_id));

            return pgmReports.RenderReportToStream(
                pgmReports.GenerateReport(rpt, parms),
                0
            );
        }

        #endregion

        #region US Warehouse

        public List<SPORTECH_PickTicket> SPORTECH_FetchPickTicketLine(string pm_pick_ticket_no)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<SPORTECH_PickTicket> theLineList = new List<SPORTECH_PickTicket>();
            string pmJSON = "";

            pmJSON = "{'pick_ticket_no':" + pm_pick_ticket_no + "}";
            pmJSON = pmJSON.Replace("'", "\"");

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.VarChar, Value = pmJSON}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "prIntranet_GetList_PickTicketLine", wkParm);
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
            {
                theLineList = JsonConvert.DeserializeObject<List<SPORTECH_PickTicket>>(ResultModel.JsonData);
                //foreach (var wkLine in theLineList)
                //    wkLine.box_qty = 50;
            }

            return theLineList;
        }


        public MemoryStream Get_Shipping_SPORTECH_Box(string rpt, string pick_ticket_no, List<SPORTECH_PickTicket> pmLineList)
        {
            string pmJSON = "{'pick_ticket_no':'" + pick_ticket_no + "', 'lines':" + JsonConvert.SerializeObject(pmLineList) + "}";
            pmJSON = pmJSON.Replace("'", "\"");

            ReportParameterCollection parms = new ReportParameterCollection();
            parms.Add(new ReportParameter("pmJSON", pmJSON));

            return pgmReports.RenderReportToStream(pgmReports.GenerateReport(rpt, parms), 0);
        }

        #endregion

        #region VN Warehouse

        public SP_Return RPT_GoodsNote_Receiving(string receipt_date)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            GoodsNote_Receiving wkReceipts = pgmReceiving.FetchGoodsNote_Receiving(receipt_date);
            List<GoodsNote_Receiving_hdr> theNotes = wkReceipts.theReceipts;
            if (theNotes == null)
                ResultModel.msg = "No data found.";
            if (!string.IsNullOrWhiteSpace(ResultModel.msg))
            {
                ResultModel.r = 0;
                return ResultModel;
            }

            string templatePath = shareFileActions.GetTemplateFolder() + "GoodsNote_Receiving.xlsx";
            var stream = File.OpenRead(templatePath);
            string OutputFile = "GoodsNote_Receiving (" + wkReceipts.receipts_string + ").xlsx";
            string OutputPath = shareFileActions.GetUploadFolder() + OutputFile;
            System.IO.FileInfo wkFile = new System.IO.FileInfo(OutputPath);

            int i;
            int theFirstDataRow = 12;
            List<GoodsNote_Receiving_line> myLines = new List<GoodsNote_Receiving_line>();
            try
            {
                if (wkFile.Exists)
                    wkFile.Delete();

                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                using (var package = new ExcelPackage(stream))
                {
                    var ws = package.Workbook.Worksheets[0];
                    ws.Name = theNotes[0].receipt_number.ToString();
                    for (i = 1; i < theNotes.Count(); i++)
                        package.Workbook.Worksheets.Add(theNotes[i].receipt_number.ToString(), ws);

                    for (i = 0; i < theNotes.Count(); i++)
                    {
                        ws = package.Workbook.Worksheets[i];

                        //line
                        myLines.Clear();
                        myLines = theNotes[i].theLines;
                        //ws.Column(13).Style.Numberformat.Format = (isUOM100 ? "#,##0.00" : "#,##0");
                        if (myLines.Count() > 1)
                            ws.InsertRow(theFirstDataRow + 1, myLines.Count() - 1, theFirstDataRow);   //InsertRow(int rowFrom, int rows, int copyStylesFromRow)
                        ws.Cells["A" + theFirstDataRow.ToString()].LoadFromCollection(myLines, false);

                        //header
                        ws.Cells["H2"].Value = theNotes[i].receipt_number;
                        ws.Cells["H3"].Value = theNotes[i].rec_date;
                        ws.Cells["C6"].Value = theNotes[i].supplier_name;
                        ws.Cells["C7"].Value = theNotes[i].supplier_address;
                        ws.Cells["C9"].Value = theNotes[i].po_number;
                        //ws.Cells["O11"].Formula = "SUM(O" + theFirstDataRow.ToString() + ":O" + ws.Dimension.End.Row + ")";

                        //total
                        ws.Cells["H" + (theFirstDataRow + myLines.Count()).ToString()].Formula = "SUM(H" + theFirstDataRow.ToString() + ":H" + (theFirstDataRow + myLines.Count() - 1).ToString() + ")";

                        //General
                        //ws.PrinterSettings.PrintArea = ws.Cells[1, 1, ws.Dimension.End.Row + 1, ws.Dimension.End.Column];
                        ws.View.FreezePanes(theFirstDataRow, 1);
                        //ws.HeaderFooter.OddHeader.CenteredText = (string.IsNullOrWhiteSpace(theCI.inv_string) ? "Pick Tickets: " + pmPickTickets : theCI.inv_string);
                        //ws.HeaderFooter.OddHeader.RightAlignedText = ExcelHeaderFooter.CurrentDate + " " + ExcelHeaderFooter.CurrentTime + " by " + GlobalVariables.MySession.Account;
                    }
                    package.SaveAs(wkFile);
                    //package.Save();
                }

                //stream.Position = 0;
                ResultModel.JsonData = OutputFile;
            }
            catch (Exception ex)
            {
                ResultModel.r = 0;
                ResultModel.msg = ex.ToString();
            }

            return ResultModel;
        }


        public SP_Return RPT_GoodsNote_Shipping(string pmInvoices, string pmType)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<GoodsNote_Shipping_hdr> theNotes = pgmShipping.FetchGoodsNote_Shipping(pmInvoices);
            if (theNotes == null || theNotes.Count() <= 0)
                ResultModel.msg = "No data found.";
            if (!string.IsNullOrWhiteSpace(ResultModel.msg))
            {
                ResultModel.r = 0;
                return ResultModel;
            }

            string templatePath = "";
            string OutputFile = "";
            string OutputPath = "";
            int i, j;
            int theFirstDataRow = 12;

            if (pmType == "GoodsNote")
            {
                templatePath = shareFileActions.GetTemplateFolder() + "GoodsNote_Shipping.xlsx";
                OutputFile = "GoodsNote_Shipping-" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx";
                OutputPath = shareFileActions.GetUploadFolder() + OutputFile;
            }
            if (pmType == "DeliveryNote")
            {
                templatePath = shareFileActions.GetTemplateFolder() + "GoodsNote_Delivery.xlsx";
                OutputFile = "GoodsNote_Delivery-" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx";
                OutputPath = shareFileActions.GetUploadFolder() + OutputFile;
            }

            try
            {
                System.IO.FileInfo wkFile = new System.IO.FileInfo(OutputPath);
                var stream = File.OpenRead(templatePath);
                if (wkFile.Exists)
                    wkFile.Delete();

                List<GoodsNote_Shipping_line> myLines = new List<GoodsNote_Shipping_line>();
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                using (var package = new ExcelPackage(stream))
                {
                    var ws = package.Workbook.Worksheets[0];
                    ws.Name = theNotes[0].invoice_no;
                    for (i = 1; i < theNotes.Count(); i++)
                        package.Workbook.Worksheets.Add(theNotes[i].invoice_no, ws);

                    for (i = 0; i < theNotes.Count(); i++)
                    {
                        ws = package.Workbook.Worksheets[i];

                        //line
                        myLines.Clear();
                        myLines = theNotes[i].theLines;
                        //ws.Column(13).Style.Numberformat.Format = (isUOM100 ? "#,##0.00" : "#,##0");
                        if (myLines.Count() > 1)
                            ws.InsertRow(theFirstDataRow + 1, myLines.Count() - 1, theFirstDataRow);   //InsertRow(int rowFrom, int rows, int copyStylesFromRow)

                        if (pmType == "GoodsNote")
                        {
                            ws.Cells["A" + theFirstDataRow.ToString()].LoadFromCollection(myLines, false);
                        }
                        if (pmType == "DeliveryNote")
                        {
                            for (j = 0; j < myLines.Count(); j++)
                            {
                                ws.Cells["A" + (j + theFirstDataRow).ToString()].Value = myLines[j].line_no;
                                ws.Cells["B" + (j + theFirstDataRow).ToString()].Value = myLines[j].customer_part_number;
                                ws.Cells["C" + (j + theFirstDataRow).ToString()].Value = myLines[j].item_desc_vn;
                                ws.Cells["G" + (j + theFirstDataRow).ToString()].Value = myLines[j].unit_size;
                                ws.Cells["H" + (j + theFirstDataRow).ToString()].Value = myLines[j].qty_actual;
                            }
                        }

                        //header
                        ws.Cells["H2"].Value = theNotes[i].invoice_no;
                        //ws.Cells["H3"].Value = theNotes[i].invoice_date;
                        ws.Cells["C6"].Value = theNotes[i].customer_name;
                        ws.Cells["C7"].Value = theNotes[i].customer_name;
                        ws.Cells["C9"].Value = theNotes[i].po_no;

                        //total
                        ws.Cells["H" + (theFirstDataRow + myLines.Count()).ToString()].Formula = "SUM(H" + theFirstDataRow.ToString() + ":H" + (theFirstDataRow + myLines.Count() - 1).ToString() + ")";

                        //General
                        //ws.PrinterSettings.PrintArea = ws.Cells[1, 1, ws.Dimension.End.Row + 1, ws.Dimension.End.Column];
                        ws.View.FreezePanes(theFirstDataRow, 1);
                    }
                    package.SaveAs(wkFile);
                    //package.Save();
                }

                //stream.Position = 0;
                ResultModel.JsonData = OutputFile;
            }
            catch (Exception ex)
            {
                ResultModel.r = 0;
                ResultModel.msg = ex.ToString();
            }

            return ResultModel;
        }

        #endregion

    }
}
