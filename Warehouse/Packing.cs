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

namespace Warehouse
{
    public class Packing_Program
    {
        public bool isDev = false;
        public string devMode = null;
        //private SqlConnection SBScon = GlobalConfig.MyConfig.Connection_SBS;
        Common_Share shareCommon = new Common_Share();


        public Packing_Program()
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(GlobalConfig.MyConfig.Connection_SBS.ConnectionString);

            if (builder.InitialCatalog.ToLower().Contains("dev"))
            {
                isDev = true;
                devMode = "You are using PLAY mode for Packing app now.";
            }
        }


        public void SetSession_packing_person(string pm_packing_person)
        {
            if (pm_packing_person != "tw_warehouse" && pm_packing_person != "mx_warehouse") //shared account
                GlobalVariables.MySession.packing_person = pm_packing_person;
        }


        public palletizing_data Fetch_PalletizingData(int pm_pick_ticket_no)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            palletizing_data theData = new palletizing_data();

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pm_pick_ticket_no", SqlDbType = SqlDbType.Decimal, Value = pm_pick_ticket_no}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "P21.prPickTicket_GetPalletizing", wkParm);
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
                theData = JsonConvert.DeserializeObject<palletizing_data>(ResultModel.JsonData);
            theData.pick_ticket_no = pm_pick_ticket_no;

            return theData;
        }


        public SP_Return SavePalletizingChecking(int pm_pick_ticket_no)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };

            //double check
            ResultModel = shareCommon.checkAuthorized(77);
            if (ResultModel.r == 0)
                return ResultModel;

            string theJson = "{'pick_ticket_no':" + pm_pick_ticket_no.ToString() + ", 'z_usr':'" + GlobalVariables.MySession.Account + "'}";
            theJson = theJson.Replace("'", @"""");

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.NVarChar, Value = theJson}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "P21.prPickTicket_SavePalletizingChecking", wkParm);
            if (ResultModel.r == 1)
                ResultModel = JsonConvert.DeserializeObject<SP_Return>(ResultModel.JsonData);

            return ResultModel;
        }


        public List<palletizing_record2_line> Fetch_PalletizingRecord2(int pm_pick_ticket_no)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<palletizing_record2_line> theRecords = new List<palletizing_record2_line>();

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmPickTicket", SqlDbType = SqlDbType.Decimal, Value = pm_pick_ticket_no},
                new SqlParameter() {ParameterName = "@pm_z_usr", SqlDbType = SqlDbType.VarChar, Value = GlobalVariables.MySession.Account}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "P21.prPickTicket_GetPickTicket", wkParm);
            GlobalVariables.MySession.List_PalletizingRecord2_Line = null;
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
            {
                theRecords = JsonConvert.DeserializeObject<List<palletizing_record2_line>>(ResultModel.JsonData);

                foreach (palletizing_record2_line oneLine in theRecords)
                {
                    foreach (palletizing_record2_bin oneBin in oneLine.bins)
                    {
                        if (oneBin.palletizing_no > 0)
                            oneBin.summary = oneLine.bins.Where(w => w.line_seq == oneBin.line_seq).Sum(s => s.qty_per_box * s.total_boxes);
                    }
                }

                GlobalVariables.MySession.List_PalletizingRecord2_Line = theRecords;
            }

            return theRecords;
        }


        public SP_Return Set_PalletizingRecord2(int pm_pick_ticket_no, List<palletizing_record2> pm_lines)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };

            //double check
            ResultModel = shareCommon.checkAuthorized(88);
            if (ResultModel.r == 0)
                return ResultModel;

            int i = 0;
            List<palletizing_record2> wkLines = pm_lines.OrderByDescending(o1 => o1.palletizing_no).OrderBy(o2 => o2.line_seq).ToList();
            foreach (palletizing_record2 oneItem in wkLines)
            {
                i++;
                oneItem.bin_seq = i;
            }

            palletizing_record2 pmFirstLine = pm_lines.First();
            int pm_line_number = pmFirstLine.line_number;
            List<palletizing_record2_line> wkPT = GlobalVariables.MySession.List_PalletizingRecord2_Line;
            List<palletizing_record2_bin> wkBins = wkLines
                                                    .Select(s => new palletizing_record2_bin() { palletizing_no = s.palletizing_no, line_seq = s.line_seq, lot_cd = s.lot_cd, country_of_origin = s.country_of_origin, bin_id = s.bin_id, qty_allocated_delta = s.qty_allocated_delta, qty_per_box = s.qty_per_box, total_boxes = s.total_boxes, same_box_no = s.same_box_no, pallet_no = s.pallet_no, bin_seq = s.bin_seq })
                                                    //.OrderByDescending(o2 => o2.palletizing_no)
                                                    //.OrderBy(o1 => o1.line_seq)
                                                    .ToList();
            palletizing_record2_line wkLine = new palletizing_record2_line() { line_number = pmFirstLine.line_number, oe_line_no = pmFirstLine.oe_line_no, inv_mast_uid = pmFirstLine.inv_mast_uid, item_id = pmFirstLine.item_id, customer_part_number = pmFirstLine.customer_part_number, packing_person = GlobalVariables.MySession.packing_person, print_quantity = pmFirstLine.print_quantity, bins = wkBins };
            wkPT.RemoveAll(r => r.line_number == pm_line_number);
            wkPT.Add(wkLine);
            GlobalVariables.MySession.List_PalletizingRecord2_Line = null;
            GlobalVariables.MySession.List_PalletizingRecord2_Line = wkPT.OrderBy(o => o.line_number).ToList();

            string theJson = JsonConvert.SerializeObject(wkLines);
            theJson = "{'pick_ticket_no':" + pm_pick_ticket_no.ToString() + ", 'line_number':" + pm_line_number.ToString() + ", 'packing_person':'" + GlobalVariables.MySession.packing_person + "', 'z_usr':'" + GlobalVariables.MySession.Account + "', 'details':" + theJson + "}";
            theJson = theJson.Replace("'", @"""");

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.NVarChar, Value = theJson}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "P21.prPickTicket_SetPickTicket", wkParm);
            if (ResultModel.r == 1)
                ResultModel = JsonConvert.DeserializeObject<SP_Return>(ResultModel.JsonData);

            return ResultModel;
        }

    }
}
