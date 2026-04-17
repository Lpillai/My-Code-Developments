using Newtonsoft.Json;
using RuntimeVariables;
using RuntimeConfig;
using SharedScrpits;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Procurement
{
    public class SupplierScorecard_Program
    {
        public bool isDev = false;
        public string devMode = null;
        //private SqlConnection SBScon = GlobalConfig.MyConfig.Connection_SBS;
        Common_Share shareCommon = new Common_Share();


        public SupplierScorecard_Program()
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(GlobalConfig.MyConfig.Connection_SBS.ConnectionString);

            if (builder.InitialCatalog.ToLower().Contains("dev"))
            {
                isDev = true;
                devMode = "You are using PLAY mode for Supplier Scorecard app now.";
            }
        }


        public SupplierScorecard_raw SupplierScorecard_FetchList(int pm_supplier_id)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            SupplierScorecard_raw theList = new SupplierScorecard_raw();

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pm_supplier_id", SqlDbType = SqlDbType.Decimal, Value = pm_supplier_id}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "P21.pr_Supplier_Scorecard", wkParm);
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
            {
                theList = JsonConvert.DeserializeObject<SupplierScorecard_raw>(ResultModel.JsonData);
                theList.price = theList.price.OrderByDescending(o => o.grand_total).ThenBy(o => o.spend_ym).ThenBy(o => o.unit_price).ToList();
            }

            return theList;
        }


        public SP_Return SupplierScorecard_BeforeChecking(int pm_supplier_id)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pm_supplier_id", SqlDbType = SqlDbType.Decimal, Value = pm_supplier_id}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "P21.pr_SupplierScorecard_BeofreChecking", wkParm);
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
                ResultModel = JsonConvert.DeserializeObject<SP_Return>(ResultModel.JsonData);

            return ResultModel;
        }


        public SupplierScorecard_finished SupplierScorecard_Calculation(int pm_supplier_id, int pm_target)
        {
            SupplierScorecard_finished theSheet = new SupplierScorecard_finished();
            theSheet.DataFlag = SupplierScorecard_BeforeChecking(pm_supplier_id);
            if (theSheet.DataFlag.r != 1)
                return theSheet;

            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            SupplierScorecard_raw theList = SupplierScorecard_FetchList(pm_supplier_id);

            if (theList == null || string.IsNullOrWhiteSpace(theList.ym))
            {
                theSheet.DataFlag.r = 0;
                theSheet.DataFlag.msg = "No data found.";
                return theSheet;
            }

            int i, j, k;
            string monthName, tmp_item_id = "";
            decimal tmp_unit_price = 0;
            decimal wk_target = (decimal)((100.0 - pm_target) / 100.0);

            theSheet.ym = theList.ym.Split(',').ToList();

            //Spend
            theSheet.total_po_placed = new SupplierScorecard_finished_category_decimal();
            theSheet.total_po_placed.category = "$ of total PO placed";
            theSheet.yoy_total_po_placed = new SupplierScorecard_finished_category_decimal();
            theSheet.yoy_total_po_placed.category = "YoY Change";
            theSheet.po_lines_issued = new SupplierScorecard_finished_category_int();
            theSheet.po_lines_issued.category = "# of PO lines issued";
            theSheet.yoy_po_lines_issued = new SupplierScorecard_finished_category_int();
            theSheet.yoy_po_lines_issued.category = "YoY Change";
            if (theList.spend != null)
            {
                theSheet.total_po_placed.subtotal = theList.spend.Sum(s => s.total_po_placed);
                theSheet.yoy_total_po_placed.subtotal = theList.spend.Sum(s => s.yoy_total_po_placed);
                theSheet.po_lines_issued.subtotal = theList.spend.Sum(s => s.po_lines_issued);
                theSheet.yoy_po_lines_issued.subtotal = theList.spend.Sum(s => s.yoy_po_lines_issued);
                for (i = 0; i < theList.spend.Count(); i++)
                {
                    for (k = 0; k < 12; k++)
                        if (theList.spend[i].spend_ym.ToString() == theSheet.ym[k])
                        {
                            monthName = "ym" + (k + 1).ToString("00");
                            theSheet.total_po_placed.GetType().GetProperty(monthName, BindingFlags.Public | BindingFlags.Instance).SetValue(theSheet.total_po_placed, theList.spend[i].total_po_placed, null);
                            theSheet.yoy_total_po_placed.GetType().GetProperty(monthName, BindingFlags.Public | BindingFlags.Instance).SetValue(theSheet.yoy_total_po_placed, theList.spend[i].yoy_total_po_placed, null);
                            theSheet.po_lines_issued.GetType().GetProperty(monthName, BindingFlags.Public | BindingFlags.Instance).SetValue(theSheet.po_lines_issued, theList.spend[i].po_lines_issued, null);
                            theSheet.yoy_po_lines_issued.GetType().GetProperty(monthName, BindingFlags.Public | BindingFlags.Instance).SetValue(theSheet.yoy_po_lines_issued, theList.spend[i].yoy_po_lines_issued, null);
                        }
                }
            }

            //Pricing top 5 parts
            if (theList.price != null && theList.price.Count() > 0)
            {
                j = 0;
                theSheet.price = new List<SupplierScorecard_finished_pricing>();
                for (i = 0; i < theList.price.Count(); i++)
                {
                    if (tmp_item_id != theList.price[i].item_id || tmp_unit_price != theList.price[i].unit_price)
                    {
                        theSheet.price.Add(new SupplierScorecard_finished_pricing());
                        j = theSheet.price.Count() - 1;
                        theSheet.price[j].item = theList.price[i].item_id;
                        theSheet.price[j].supplier_part_no = theList.price[i].supplier_part_no;
                        theSheet.price[j].unit_price = theList.price[i].unit_price;
                        theSheet.price[j].sub_total = theList.price[i].sub_total;

                        if (tmp_item_id == theList.price[i].item_id)
                            theSheet.price[j].actual = (theList.price[i].unit_price - tmp_unit_price) / theList.price[i].unit_price * 100;  //Actual Percentage
                                                                                                                                            //theSheet.price[j].actual = (theList.price[i].unit_price == 0 ? 0 : ((theList.price[i].unit_price - tmp_unit_price) / theList.price[i].unit_price * 100));  //Actual Percentage
                        else
                        {
                            theSheet.price[j].grand_total = theList.price[i].grand_total;
                            theSheet.price[j].target = theList.price[i].unit_price * wk_target; //target price
                        }

                        tmp_item_id = theList.price[i].item_id;
                        tmp_unit_price = theList.price[i].unit_price;
                    }

                    for (k = 0; k < 12; k++)
                        if (theList.price[i].spend_ym.ToString() == theSheet.ym[k])
                        {
                            monthName = "ym" + (k + 1).ToString("00");
                            theSheet.price[j].GetType().GetProperty(monthName, BindingFlags.Public | BindingFlags.Instance).SetValue(theSheet.price[j], theList.price[i].extened_value, null);
                        }
                }
            }

            //Lines Received
            theSheet.total_lines = new SupplierScorecard_finished_category_int();
            theSheet.total_lines.category = "# of total lines";
            theSheet.lines_late = new SupplierScorecard_finished_category_int();
            theSheet.lines_late.category = "# of lines late";
            theSheet.lines_early = new SupplierScorecard_finished_category_int();
            theSheet.lines_early.category = "# of lines early (-7days)";
            theSheet.lines_on_time = new SupplierScorecard_finished_category_decimal();
            theSheet.lines_on_time.category = "% of lines on time";
            if (theList.receive != null && theList.receive.Count() > 0)
            {
                theSheet.total_lines.subtotal = theList.receive.Sum(s => s.total_lines);
                theSheet.lines_late.subtotal = theList.receive.Sum(s => s.lines_late);
                theSheet.lines_early.subtotal = theList.receive.Sum(s => s.lines_early);
                theSheet.lines_on_time.subtotal = (decimal)(theList.receive.Sum(s => s.on_time_summary) * 100.0 / theList.receive.Sum(s => s.total_lines));
                for (i = 0; i < theList.receive.Count(); i++)
                {
                    for (k = 0; k < 12; k++)
                        if (theList.receive[i].completed_ym.ToString() == theSheet.ym[k])
                        {
                            monthName = "ym" + (k + 1).ToString("00");
                            theSheet.total_lines.GetType().GetProperty(monthName, BindingFlags.Public | BindingFlags.Instance).SetValue(theSheet.total_lines, theList.receive[i].total_lines, null);
                            theSheet.lines_late.GetType().GetProperty(monthName, BindingFlags.Public | BindingFlags.Instance).SetValue(theSheet.lines_late, theList.receive[i].lines_late, null);
                            theSheet.lines_early.GetType().GetProperty(monthName, BindingFlags.Public | BindingFlags.Instance).SetValue(theSheet.lines_early, theList.receive[i].lines_early, null);
                            theSheet.lines_on_time.GetType().GetProperty(monthName, BindingFlags.Public | BindingFlags.Instance).SetValue(theSheet.lines_on_time, theList.receive[i].lines_on_time, null);
                        }
                }
            }

            //Receipt Discrepancies
            theSheet.Freight_Damages_found_at_Receipt = new SupplierScorecard_finished_category_int();
            theSheet.Freight_Damages_found_at_Receipt.category = "# of Freight Damages found at Receipt";
            theSheet.shipments_Missing_Documents = new SupplierScorecard_finished_category_int();
            theSheet.shipments_Missing_Documents.category = "# of Shipments Missing Documents";
            theSheet.shipments_with_quantity_discrepancies = new SupplierScorecard_finished_category_int();
            theSheet.shipments_with_quantity_discrepancies.category = "# of Shipments with Quantity Discrepancies";
            //Quality
            theSheet.Parts_with_Quality_Defects = new SupplierScorecard_finished_category_int();
            theSheet.Parts_with_Quality_Defects.category = "# of Parts with Quality Defects";
            theSheet.corrective_actions = new SupplierScorecard_finished_category_int();
            theSheet.corrective_actions.category = "# of Corrective Actions";
            if (theList.dmr != null && theList.dmr.Count() > 0)
            {
                theSheet.Freight_Damages_found_at_Receipt.subtotal = theList.dmr.Sum(s => s.Freight_Damages_found_at_Receipt);
                theSheet.shipments_Missing_Documents.subtotal = theList.dmr.Sum(s => s.shipments_Missing_Documents);
                theSheet.shipments_with_quantity_discrepancies.subtotal = theList.dmr.Sum(s => s.shipments_with_quantity_discrepancies);

                theSheet.Parts_with_Quality_Defects.subtotal = theList.dmr.Sum(s => s.Parts_with_Quality_Defects);
                theSheet.corrective_actions.subtotal = theList.dmr.Sum(s => s.corrective_actions);

                for (i = 0; i < theList.dmr.Count(); i++)
                {
                    for (k = 0; k < 12; k++)
                        if (theList.dmr[i].dmrYM.ToString() == theSheet.ym[k])
                        {
                            monthName = "ym" + (k + 1).ToString("00");
                            theSheet.Freight_Damages_found_at_Receipt.GetType().GetProperty(monthName, BindingFlags.Public | BindingFlags.Instance).SetValue(theSheet.Freight_Damages_found_at_Receipt, theList.dmr[i].Freight_Damages_found_at_Receipt, null);
                            theSheet.shipments_Missing_Documents.GetType().GetProperty(monthName, BindingFlags.Public | BindingFlags.Instance).SetValue(theSheet.shipments_Missing_Documents, theList.dmr[i].shipments_Missing_Documents, null);
                            theSheet.shipments_with_quantity_discrepancies.GetType().GetProperty(monthName, BindingFlags.Public | BindingFlags.Instance).SetValue(theSheet.shipments_with_quantity_discrepancies, theList.dmr[i].shipments_with_quantity_discrepancies, null);
                            theSheet.Parts_with_Quality_Defects.GetType().GetProperty(monthName, BindingFlags.Public | BindingFlags.Instance).SetValue(theSheet.Parts_with_Quality_Defects, theList.dmr[i].Parts_with_Quality_Defects, null);
                            theSheet.corrective_actions.GetType().GetProperty(monthName, BindingFlags.Public | BindingFlags.Instance).SetValue(theSheet.corrective_actions, theList.dmr[i].corrective_actions, null);
                        }
                }
            }

            //Reponsiveness
            theSheet.number_of_rfq_sent = new SupplierScorecard_finished_category_int();
            theSheet.number_of_rfq_sent.category = "# of RFQ's Sent";
            theSheet.response_time = new SupplierScorecard_finished_category_int();
            theSheet.response_time.category = "Response Time (Days)";
            theSheet.number_of_rfq_recommended_forward = new SupplierScorecard_finished_category_int();
            theSheet.number_of_rfq_recommended_forward.category = "# of RFQ's Recommended Forward";
            if (theList.pod != null && theList.pod.Count() > 0)
            {
                theSheet.number_of_rfq_sent.subtotal = theList.pod.Sum(s => s.rfq_sent);
                theSheet.response_time.subtotal = theList.pod.Sum(s => s.response_time);
                theSheet.response_time.average = Convert.ToDecimal(theList.pod.Sum(s => s.response_time)) / Convert.ToDecimal(theSheet.number_of_rfq_sent.subtotal);
                theSheet.number_of_rfq_recommended_forward.subtotal = theList.pod.Sum(s => s.recommended_forward);
                for (i = 0; i < theList.pod.Count(); i++)
                {
                    for (k = 0; k < 12; k++)
                        if (theList.pod[i].podYM.ToString() == theSheet.ym[k])
                        {
                            monthName = "ym" + (k + 1).ToString("00");
                            theSheet.number_of_rfq_sent.GetType().GetProperty(monthName, BindingFlags.Public | BindingFlags.Instance).SetValue(theSheet.number_of_rfq_sent, theList.pod[i].rfq_sent, null);
                            theSheet.response_time.GetType().GetProperty(monthName, BindingFlags.Public | BindingFlags.Instance).SetValue(theSheet.response_time, theList.pod[i].response_time, null);
                            theSheet.number_of_rfq_recommended_forward.GetType().GetProperty(monthName, BindingFlags.Public | BindingFlags.Instance).SetValue(theSheet.number_of_rfq_recommended_forward, theList.pod[i].recommended_forward, null);
                        }
                }
            }

            return theSheet;
        }

    }
}
