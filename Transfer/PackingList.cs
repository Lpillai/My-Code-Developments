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
    public class PackingList_Program
    {
        public bool isDev = false;
        public string devMode = null;
        //private SqlConnection SBScon = GlobalConfig.MyConfig.Connection_SBS;
        Common_Share shareCommon = new Common_Share();


        public PackingList_Program()
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(GlobalConfig.MyConfig.Connection_SBS.ConnectionString);

            if (builder.InitialCatalog.ToLower().Contains("dev"))
            {
                isDev = true;
                devMode = "You are using PLAY mode for Packing List app now.";
            }
        }


        public Transfer_PackingList GetPackingList(string pmPickTickets)
        {
            if (GlobalVariables.MySession.PackingList == null || GlobalVariables.MySession.PackingList.pick_ticket_no != pmPickTickets)
                FetchPackingList(pmPickTickets);

            return GlobalVariables.MySession.PackingList;
        }


        public SP_Return FetchPackingList(string pmPickTickets)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            Transfer_PackingList theList = new Transfer_PackingList();
            string pmJSON = "{'pmPickTickets':'" + pmPickTickets.Replace(" ", "") + "'}";
            pmJSON = pmJSON.Replace("'", "\"");

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.VarChar, Value = pmJSON}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "P21.prPickTicket_GetPackingList", wkParm);
            GlobalVariables.MySession.PackingList = null;
            if (ResultModel.r == 1 && !String.IsNullOrWhiteSpace(ResultModel.JsonData))
            {
                theList = JsonConvert.DeserializeObject<Transfer_PackingList>(ResultModel.JsonData);
                GlobalVariables.MySession.PackingList = theList;
                GlobalVariables.MySession.PackingList.pick_ticket_no = pmPickTickets;
                GlobalVariables.MySession.PackingList.pm_usr = GlobalVariables.MySession.Account;
                GlobalVariables.MySession.PackingList.pm_usr_name = GlobalVariables.MySession.FirstName + " " + GlobalVariables.MySession.LastName;
                RecalculatePackingList(theList.packing_list_line);
            }

            return ResultModel;
        }


        public void RecalculatePackingList(List<Transfer_PackingList_line> pmList)
        {
            foreach (var item in pmList)
            {
                item.qty = item.box * item.qty_per_box;
                item.net = item.qty * item.unit_weight;
                item.gross = item.net + (decimal)(item.box * 0.5);
            }

            Transfer_PackingList_sum wk_sum = new Transfer_PackingList_sum();
            wk_sum.ctn = pmList.Where(w => w.same_box_no == 0).Sum(s => s.box) + pmList.Where(w => w.same_box_no > 0).Select(s => s.same_box_no).Distinct().Count();
            wk_sum.qty = pmList.Sum(s => s.qty);
            wk_sum.net = pmList.Sum(s => s.net);
            wk_sum.gross = pmList.Sum(s => s.gross);
            wk_sum.plt = pmList.Max(m => m.pallet_no) * 20;
            wk_sum.kgs = wk_sum.gross + wk_sum.plt;

            GlobalVariables.MySession.PackingList.packing_list_line = pmList;
            GlobalVariables.MySession.PackingList.packing_list_sum = wk_sum;
        }


        public void SetPackingListShip(Transfer_PackingList_hdr pm_packing_list, int plt)
        {
            /*
            GlobalVariables.MySession.PackingList.packing_list_hdr.FirstOrDefault().shipped_per = pm_packing_list.shipped_per;
            GlobalVariables.MySession.PackingList.packing_list_hdr.FirstOrDefault().ship_from_address = pm_packing_list.ship_from_address;
            GlobalVariables.MySession.PackingList.packing_list_hdr.FirstOrDefault().port = pm_packing_list.port;
            GlobalVariables.MySession.PackingList.packing_list_hdr.FirstOrDefault().requested_date = pm_packing_list.requested_date;
            GlobalVariables.MySession.PackingList.packing_list_hdr.FirstOrDefault().final_des = pm_packing_list.final_des;
            GlobalVariables.MySession.PackingList.packing_list_hdr.FirstOrDefault().sailing_notice_no = pm_packing_list.sailing_notice_no;
            GlobalVariables.MySession.PackingList.packing_list_hdr.FirstOrDefault().broker_name = pm_packing_list.broker_name;
            */
            foreach (var theHdr in GlobalVariables.MySession.PackingList.packing_list_hdr)
            {
                theHdr.shipped_per = pm_packing_list.shipped_per;
                theHdr.ship_from_address = pm_packing_list.ship_from_address;
                theHdr.port = pm_packing_list.port;
                theHdr.requested_date = pm_packing_list.requested_date;
                theHdr.final_des = pm_packing_list.final_des;
                theHdr.sailing_notice_no = pm_packing_list.sailing_notice_no;
                theHdr.broker_name = pm_packing_list.broker_name;
            }

            GlobalVariables.MySession.PackingList.packing_list_sum.plt = plt;
            GlobalVariables.MySession.PackingList.packing_list_sum.kgs = GlobalVariables.MySession.PackingList.packing_list_sum.gross + GlobalVariables.MySession.PackingList.packing_list_sum.plt;
        }


        public SP_Return SavePackingList()
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            string pmJSON = JsonConvert.SerializeObject(GlobalVariables.MySession.PackingList);

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.NVarChar, Value = pmJSON}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "P21.prPickTicket_SetPackingList", wkParm);
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
                ResultModel = JsonConvert.DeserializeObject<SP_Return>(ResultModel.JsonData);

            return ResultModel;
        }


        public SP_Return LoadDefaultPackingList()
        {
            GlobalVariables.MySession.PackingList.packing_list_hdr = null;
            GlobalVariables.MySession.PackingList.packing_list_line = null;
            GlobalVariables.MySession.PackingList.packing_list_po_mark = null;
            GlobalVariables.MySession.PackingList.packing_list_sum = null;

            SP_Return ResultModel = SavePackingList();
            GlobalVariables.MySession.PackingList = null;

            return ResultModel;
        }


        public List<Transfer_Pallet> GetPalletList(string pmPickTickets)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<Transfer_Pallet> theList = new List<Transfer_Pallet>();
            string pmJSON = "{'pmPickTickets':'" + pmPickTickets.Replace(" ", "") + "'}";
            pmJSON = pmJSON.Replace("'", "\"");

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.VarChar, Value = pmJSON}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "P21.prPickTicket_GetPallets", wkParm);
            GlobalVariables.MySession.PackingList = null;
            if (ResultModel.r == 1 && !String.IsNullOrWhiteSpace(ResultModel.JsonData))
                theList = JsonConvert.DeserializeObject<List<Transfer_Pallet>>(ResultModel.JsonData);

            return theList;
        }


        public SP_Return SavePalletList(string pmPickTickets, List<Transfer_Pallet> pm_Pallets)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            string pmJSON = JsonConvert.SerializeObject(pm_Pallets);
            pmJSON = ("{'pick_ticket_no':'" + pmPickTickets + "', 'pm_usr':'" + GlobalVariables.MySession.Account + "', 'thePallets':" + pmJSON + "}").Replace("'", "\"");

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.NVarChar, Value = pmJSON}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "P21.prPickTicket_SetPallets", wkParm);
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
            {
                ResultModel = JsonConvert.DeserializeObject<SP_Return>(ResultModel.JsonData);
                if (ResultModel.r == 1 && GlobalVariables.MySession.PackingList != null)
                {
                    GlobalVariables.MySession.PackingList.packing_list_hdr = null;
                    GlobalVariables.MySession.PackingList.packing_list_line = null;
                    GlobalVariables.MySession.PackingList.packing_list_po_mark = null;
                    GlobalVariables.MySession.PackingList.packing_list_sum = null;
                    GlobalVariables.MySession.PackingList = null;
                    FetchPackingList(pmPickTickets);
                }
            }

            return ResultModel;
        }

    }
}
