using Microsoft.Reporting.WebForms;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using RuntimeVariables;
using SharedScrpits;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transfer;

namespace Reports
{
    public class RPT_Transfer_Program
    {
        Common_Share shareCommon = new Common_Share();
        FileActions_Share shareFileActions = new FileActions_Share();
        Reports_Program pgmReports = new Reports_Program();
        CommercialInvoice_Program pgmCI = new CommercialInvoice_Program();
        

        public SP_Return Go_Audit_preload()
        {
            string wkPath = "@pm_query='RPT/audit_preloads?mail_to=" + GlobalVariables.MySession.Email + "', @out_result=''";
            return shareCommon.AddScheduleForAPI("SBS.z.prSBS_API_GET2", wkPath, false);
        }


        public MemoryStream Get_PackingList_lines()
        {
            string version = "20231228";
            var stream = new MemoryStream();

            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            using (var package = new ExcelPackage(stream))
            {
                var workSheet = package.Workbook.Worksheets.Add(version);
                workSheet.Cells["A1"].LoadFromCollection(GlobalVariables.MySession.PackingList.packing_list_line, true);

                workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Column(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Column(2).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Column(4).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Column(8).Style.Numberformat.Format = "#,##0";
                workSheet.Column(9).Style.Numberformat.Format = "#,##0";
                workSheet.Column(11).Style.Numberformat.Format = "#,##0.00";
                workSheet.Column(12).Style.Numberformat.Format = "#,##0.00";

                workSheet.View.FreezePanes(2, 1);
                workSheet.Cells[workSheet.Dimension.Address].AutoFitColumns();
                package.Save();
            }
            stream.Position = 0;

            return stream;
        }


        public MemoryStream RPT_PackingList(string rpt)
        {
            string pmJSON = JsonConvert.SerializeObject(GlobalVariables.MySession.PackingList);
            ReportParameterCollection parms = new ReportParameterCollection();
            parms.Add(new ReportParameter("pmJSON", pmJSON));

            return pgmReports.RenderReportToStream(pgmReports.GenerateReport(rpt, parms), 0);
        }


        public MemoryStream RPT_PalletsMark(string rpt, string pmPickTickets)
        {
            ReportParameterCollection parms = new ReportParameterCollection();
            parms.Add(new ReportParameter("pm_pick_ticket_no", pmPickTickets));

            return pgmReports.RenderReportToStream(pgmReports.GenerateReport(rpt, parms), 0);
        }


        public SP_Return RPT_CommercialInvoice(string pmPickTickets, bool isKits, bool isUOM100)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            Transfer_CommercialInvoice theCI = pgmCI.FetchCommercialInvoice(pmPickTickets);
            if (theCI == null)
                ResultModel.msg = "Querying failed.";
            if (theCI.ci_line == null || theCI.ci_line.Count() == 0)
                ResultModel.msg = "No data found.";
            if (!string.IsNullOrWhiteSpace(ResultModel.msg))
            {
                ResultModel.r = 0;
                return ResultModel;
            }

            pmPickTickets = string.IsNullOrWhiteSpace(theCI.inv_string) ? "PT-" + pmPickTickets : theCI.inv_string;  //sorted in invoice no order
            string templatePath = shareFileActions.GetTemplateFolder() + "Commercial Invoice.xlsx";
            var stream = File.OpenRead(templatePath);
            string OutputFile = "CI-" + pmPickTickets + ".xlsx";
            string OutputPath = shareFileActions.GetUploadFolder() + OutputFile;
            System.IO.FileInfo wkFile = new System.IO.FileInfo(OutputPath);
            pmPickTickets = pmPickTickets.Replace(",", ", ");

            int theFirstDataRow = 14;
            //int tmp_same_box_no = 0;
            //int tmp_component_sequence = 0;
            //int wkSameBoxStartRow = 0;
            //int wkSubtotalBox = 0;
            //int wkGrandTotalBox = 0;
            int i;
            int wkComponentStartRow = 0;
            string tmp_box = "";

            try
            {
                if (!isKits)
                    theCI.ci_line = theCI.ci_line.Where(w => !w.isComponent).ToList();

                if (isUOM100)
                {
                    foreach (Transfer_CI_line aLine in theCI.ci_line)
                    {
                        aLine.subtotal_qty = aLine.subtotal_qty / (decimal)100.0;
                        aLine.unit_price = aLine.unit_price * (decimal)100.0;
                        aLine.uom = 100;
                        //aLine.ext_sell *= 100;
                    }
                }

                if (wkFile.Exists)
                    wkFile.Delete();

                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                using (var package = new ExcelPackage(stream))
                {
                    var ws = package.Workbook.Worksheets[0];

                    //line
                    var myCI = theCI.ci_line.Select(s => new { s.ccac, s.ship_to_id, s.po_no, s.item_id, s.item_desc, s.customer_part_number, s.origin_hs_code, s.dest_hs_code, s.annex_hs_code, s.material_type, s.coo, s.subtotal_box, s.subtotal_qty, s.unit_weight, s.subtotal_net, s.unit_price, s.uom, s.ext_sell }).ToList();
                    ws.Column(13).Style.Numberformat.Format = (isUOM100 ? "#,##0.00" : "#,##0");
                    if (myCI.Count() > 1)
                        ws.InsertRow(theFirstDataRow + 1, myCI.Count() - 1, theFirstDataRow);   //InsertRow(int rowFrom, int rows, int copyStylesFromRow)
                    ws.Cells["A" + theFirstDataRow.ToString()].LoadFromCollection(myCI, false);

                    if (isKits)
                    {
                        for (i = 0; i < theCI.ci_line.Count(); i++)
                        {
                            if (theCI.ci_line[i].component_sequence > 0)
                            {
                                ws.Cells["N" + (i+theFirstDataRow).ToString()].Value = "";
                                ws.Cells["O" + (i + theFirstDataRow).ToString()].Value = "";
                                ws.Cells["R" + (i + theFirstDataRow).ToString()].Value = "";
                            }

                            if (theCI.ci_line[i].component_sequence == 0 && wkComponentStartRow > 0)
                            {
                                ws.Cells["A" + (wkComponentStartRow-1).ToString() + ":A" + (i + theFirstDataRow - 1).ToString()].Merge = true;
                                ws.Cells["A" + (wkComponentStartRow-1).ToString()].Value = "";
                                ws.Cells["B" + wkComponentStartRow.ToString() + ":B" + (i + theFirstDataRow - 1).ToString()].Merge = true;
                                ws.Cells["B" + wkComponentStartRow.ToString()].Value = "";
                                ws.Cells["C" + wkComponentStartRow.ToString() + ":C" + (i + theFirstDataRow - 1).ToString()].Merge = true;
                                ws.Cells["C" + wkComponentStartRow.ToString()].Value = "";
                                ws.Cells["L" + (wkComponentStartRow-1).ToString() + ":L" + (i + theFirstDataRow - 1).ToString()].Merge = true;
                                ws.Cells["L" + (wkComponentStartRow-1).ToString()].Value = tmp_box;
                                wkComponentStartRow = 0;
                            }

                            if (theCI.ci_line[i].hasComponents)
                            {
                                tmp_box = theCI.ci_line[i].subtotal_box.ToString();
                                wkComponentStartRow = i + theFirstDataRow + 1;
                            }
                        }
                        if (wkComponentStartRow > 0)
                        {
                            ws.Cells["A" + (wkComponentStartRow-1).ToString() + ":A" + ws.Dimension.End.Row.ToString()].Merge = true;
                            ws.Cells["A" + (wkComponentStartRow-1).ToString()].Value = "";
                            ws.Cells["B" + wkComponentStartRow.ToString() + ":B" + ws.Dimension.End.Row.ToString()].Merge = true;
                            ws.Cells["B" + wkComponentStartRow.ToString()].Value = "";
                            ws.Cells["C" + wkComponentStartRow.ToString() + ":C" + ws.Dimension.End.Row.ToString()].Merge = true;
                            ws.Cells["C" + wkComponentStartRow.ToString()].Value = "";
                            ws.Cells["L" + (wkComponentStartRow-1).ToString() + ":L" + ws.Dimension.End.Row.ToString()].Merge = true;
                            ws.Cells["L" + (wkComponentStartRow-1).ToString()].Value = tmp_box;
                        }
                        /*
                        for (i = 0; i < theCI.ci_line.Count(); i++)
                        {
                            if (tmp_same_box_no != theCI.ci_line[i].same_box_no || (isKits && tmp_same_box_no == 0 && theCI.ci_line[i].component_sequence == 0))
                            {
                                if (tmp_same_box_no > 0 || (isKits && tmp_same_box_no == 0 && tmp_component_sequence > theCI.ci_line[i].component_sequence))
                                {
                                    ws.Cells["L" + wkSameBoxStartRow.ToString() + ":L" + (i + theFirstDataRow - 1).ToString()].Merge = true;
                                    ws.Cells["L" + wkSameBoxStartRow.ToString()].Value = wkSubtotalBox;
                                }

                                tmp_same_box_no = theCI.ci_line[i].same_box_no;
                                wkSameBoxStartRow = i + theFirstDataRow;
                                wkSubtotalBox = (tmp_same_box_no > 0 ? 1 : theCI.ci_line[i].subtotal_box);
                                wkGrandTotalBox += wkSubtotalBox;
                            }

                            tmp_component_sequence = theCI.ci_line[i].component_sequence;
                            //if (tmp_same_box_no != theCI.ci_line[i].same_box_no)
                            //{
                            //    if (tmp_same_box_no > 0)
                            //    {
                            //        ws.Cells["L" + wkSameBoxStartRow.ToString() + ":L" + (i + theFirstDataRow - 1).ToString()].Merge = true;
                            //        if (tmp_same_box_no > 0)
                            //            ws.Cells["L" + wkSameBoxStartRow.ToString()].Value = 1;
                            //        else
                            //            ws.Cells["L" + wkSameBoxStartRow.ToString()].Value = theCI.ci_line[i-1].subtotal_box;
                            //    }

                            //    tmp_same_box_no = theCI.ci_line[i].same_box_no;
                            //    wkSameBoxStartRow = i + theFirstDataRow;
                            //}
                        }
                        if (tmp_same_box_no > 0 || tmp_component_sequence > 0)
                        {
                            ws.Cells["L" + wkSameBoxStartRow.ToString() + ":L" + ws.Dimension.End.Row.ToString()].Merge = true;
                            ws.Cells["L" + wkSameBoxStartRow.ToString()].Value = wkSubtotalBox;
                        }
                        //if (tmp_same_box_no > 0)
                        //{
                        //    ws.Cells["L" + wkSameBoxStartRow.ToString() + ":L" + ws.Dimension.End.Row.ToString()].Merge = true;
                        //    if (tmp_same_box_no > 0)
                        //        ws.Cells["L" + wkSameBoxStartRow.ToString()].Value = 1;
                        //    else
                        //        ws.Cells["L" + wkSameBoxStartRow.ToString()].Value = theCI.ci_line[i - 1].subtotal_box;
                        //}
                        */
                    }

                    //header
                    ws.Cells["E2"].Value = theCI.ci_hdr.vendor_name;
                    ws.Cells["E3"].Value = theCI.ci_hdr.vendor_address;
                    ws.Cells["H2"].Value = theCI.ci_hdr.purchaser_name;
                    ws.Cells["H3"].Value = theCI.ci_hdr.purchaser_address;
                    ws.Cells["H6"].Value = theCI.ci_hdr.ship2_name;
                    ws.Cells["H7"].Value = theCI.ci_hdr.ship2_address;

                    ws.Cells["M4"].Value = theCI.ci_hdr.invoice_date.ToString("dd-MMM-yy", new CultureInfo("en-US"));
                    ws.Cells["O4"].Value = theCI.inv_string;
                    ws.Cells["O5"].Value = theCI.ci_hdr.incoterm;
                    ws.Cells["O6"].Value = theCI.ci_hdr.payment_terms;
                    ws.Cells["O7"].Value = theCI.ci_hdr.currency;
                    ws.Cells["O8"].Value = theCI.ci_hdr.carrier_name;
                    ws.Cells["O9"].Value = theCI.ci_hdr.tracking_no;
                    ws.Cells["O10"].Value = theCI.ci_hdr.aes_no;

                    ws.Cells["A10"].Value = theCI.ci_hdr.instructions;
                    ws.Cells["K9"].Value = theCI.ci_hdr.summary_box;
                    //ws.Cells["K9"].Formula = "SUM(L" + theFirstDataRow.ToString() + ":L" + ws.Dimension.End.Row + ")"; //Won't be correct if column merged
                    ws.Cells["K11"].Value = theCI.ci_hdr.summary_gross;
                    //ws.Cells["O11"].Value = theCI.ci_hdr.summary_net;
                    ws.Cells["O11"].Formula = "SUM(O" + theFirstDataRow.ToString() + ":O" + ws.Dimension.End.Row + ")";
                    //ws.Cells["R11"].Value = theCI.ci_hdr.summary_sell;
                    ws.Cells["R11"].Formula = "SUM(R" + theFirstDataRow.ToString() + ":R" + ws.Dimension.End.Row + ")";

                    //General
                    ws.PrinterSettings.PrintArea = ws.Cells[1, 1, ws.Dimension.End.Row + 1, ws.Dimension.End.Column];
                    ws.View.FreezePanes(theFirstDataRow, 1);
                    ws.HeaderFooter.OddHeader.CenteredText = (string.IsNullOrWhiteSpace(theCI.inv_string) ? "Pick Tickets: " + pmPickTickets : theCI.inv_string);
                    ws.HeaderFooter.OddHeader.RightAlignedText = ExcelHeaderFooter.CurrentDate + " " + ExcelHeaderFooter.CurrentTime + " by " + GlobalVariables.MySession.Account;
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

    }
}
