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

namespace Procurement
{
    public class BuyPO_Program
    {
        public bool isDev = false;
        public string devMode = null;
        //private SqlConnection OScon = GlobalConfig.MyConfig.Connection_OS_BuySheet;
        Common_Share shareCommon = new Common_Share();
        Get_Share shareGet = new Get_Share();


        public BuyPO_Program()
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(GlobalConfig.MyConfig.Connection_OS_BuySheet.ConnectionString);

            if (builder.InitialCatalog.ToLower().Contains("dev"))
            {
                isDev = true;
                devMode = "You are using PLAY mode for Buy app now.";
            }
        }


        public SP_Return SetNotBuy(string cluster, string selected)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<int> viewIDList = (selected.Replace("[", "").Replace("]", "").Split(',').ToList()).Select(int.Parse).ToList();
            List<Buy> buyList = new List<Buy>();

            switch (cluster)
            {
                case "OS":
                    buyList = GlobalVariables.MySession.List_Buy_OSSheet.Where(s => viewIDList.Contains(s.viewID)).ToList();
                    break;
                case "Domestic":
                    buyList = GlobalVariables.MySession.List_Buy_DomesticSheet.Where(s => viewIDList.Contains(s.viewID)).ToList();
                    break;
                case "FTB":
                    buyList = GlobalVariables.MySession.List_Buy_FTBSheet.Where(s => viewIDList.Contains(s.viewID)).ToList();
                    break;
                default:
                    break;
            }

            if (buyList.Where(l => !string.IsNullOrWhiteSpace(l.PO_no) && l.PO_no.Contains("TBD")).Count() > 0)
            {
                ResultModel.r = 0;
                ResultModel.msg = "Some of Buys in your selection has been created PO in P21 already.";
                return ResultModel;
            }

            //double check
            switch (cluster)
            {
                case "OS":
                    ResultModel = shareCommon.checkAuthorized(19);
                    break;
                case "Domestic":
                    ResultModel = shareCommon.checkAuthorized(31);
                    break;
                case "FTB":
                    ResultModel = shareCommon.checkAuthorized(107);
                    break;
                default:
                    ResultModel.r = 0;
                    ResultModel.msg = "Unavailable Cluster.";
                    break;
            }
            if (ResultModel.r == 0)
                return ResultModel;

            string entryID_list = "{'ApplyList': [" + string.Join(", ", buyList.Select(p => "{'entryID': '" + p.entryID + "'}").ToArray()) + "]}";
            entryID_list = entryID_list.Replace("'", "\"");

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pm_entryId", SqlDbType = SqlDbType.VarChar, Value = entryID_list},
                new SqlParameter() {ParameterName = "@pm_PO_no", SqlDbType = SqlDbType.VarChar, Value = "666"},
                new SqlParameter() {ParameterName = "@pm_vendor_id", SqlDbType = SqlDbType.Decimal, Value = 40000},
                new SqlParameter() {ParameterName = "@pm_usr", SqlDbType = SqlDbType.VarChar, Value = GlobalVariables.MySession.Account}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_OS_BuySheet, "sbs_sp_OSBUY_UpdatePO2", wkParm);
            if (ResultModel.r == 1)
                ResultModel = JsonConvert.DeserializeObject<SP_Return>(ResultModel.JsonData);

            return ResultModel;
        }


        public List<BuySheet_CreatePO> GetList_PrepareCreatingPO(string cluster, string selected)
        {
            int i, j;
            string status;
            List<int> viewIDList = (selected.Replace("[", "").Replace("]", "").Split(',').ToList()).Select(int.Parse).ToList();
            List<Buy> wkBuyList = null;
            List<Buy> buyList = new List<Buy>();
            List<BuySplit> splitList = new List<BuySplit>();
            List<BuySheet_CreatePOLine> POLine = new List<BuySheet_CreatePOLine>();
            List<BuySheet_CreatePO> POList = new List<BuySheet_CreatePO>();

            switch (cluster)
            {
                case "OS":
                    buyList = GlobalVariables.MySession.List_Buy_OSSheet.Where(b => viewIDList.Contains(b.viewID)).ToList();
                    splitList = GlobalVariables.MySession.List_Buy_OSSplits.Where(s => viewIDList.Contains(s.viewID)).ToList();
                    break;
                case "Domestic":
                    buyList = GlobalVariables.MySession.List_Buy_DomesticSheet.Where(b => viewIDList.Contains(b.viewID)).ToList();
                    splitList = GlobalVariables.MySession.List_Buy_DomesticSplits.Where(s => viewIDList.Contains(s.viewID)).ToList();
                    break;
                case "FTB":
                    buyList = GlobalVariables.MySession.List_Buy_FTBSheet.Where(b => viewIDList.Contains(b.viewID)).ToList();
                    splitList = GlobalVariables.MySession.List_Buy_FTBSplits.Where(s => viewIDList.Contains(s.viewID)).ToList();
                    break;
                default:
                    break;
            }

            i = 0;
            POList = buyList.GroupBy(d => new { d.vendor_approve, d.po_to_loc }).Select(m => new BuySheet_CreatePO() { vendor_approve = m.Key.vendor_approve, po_to_loc = m.Key.po_to_loc }).ToList();
            foreach (var PO in POList)
            {
                wkBuyList = null;
                wkBuyList = buyList.FindAll(w => w.vendor_approve == PO.vendor_approve && w.po_to_loc == PO.po_to_loc).ToList();

                j = 0;
                List<BuySheet_CreatePOLine> wkPOLine = new List<BuySheet_CreatePOLine>();
                foreach (var item in wkBuyList)
                {
                    j++;
                    status = "";

                    if (status == "" && (item.vendor_id ?? 0) == 0) //No vendor approved
                        status = "V";
                    if (status == "" && ((decimal?)item.po_to_loc ?? 0) == 0) //Invalid po_to_loc
                        status = "L";
                    if (status == "" && !string.IsNullOrEmpty(item.PO_no)) //PO has been created already
                        status = "P";
                    if (status == "" && (item.required_date == null || item.required_date < DateTime.Today)) //Invalid required date
                        status = "R";
                    if (status == "" && Int32.Parse(item.PO_order_quantity_1) < item.new_quantity) //Not enough PO Quantity
                        status = "Q";
                    if (status == "" && cluster == "FTB" && item.theFTB.step < 3) //on buyer's call
                        status = "F";

                    wkPOLine.Add(
                        new BuySheet_CreatePOLine()
                        {
                            no = j,
                            status = status,
                            BuySheet = item,
                            BuyRelease = splitList.FindAll(x => x.viewID == item.viewID).OrderBy(s => s.release_date).ToList()
                        }
                    );
                }

                PO.POLine = wkPOLine;
                PO.go = (wkPOLine.Where(l => l.status != "").Count() > 0) ? "FALSE" : "True";
                PO.no = 0;
                if (PO.go == "True")
                {
                    i++;
                    PO.no = i;
                }
            }
            POList = POList.OrderByDescending(o => o.go).ThenBy(o => o.po_to_loc).ThenBy(o => o.vendor_approve).ToList();

            return POList;
        }


        public List<BuySheet_CreatePO> GetList_CreatingPOResult(string cluster, List<cmd_Result> pm_ResultList)
        {
            int j;
            List<int> viewIDList = new List<int>();
            List<Buy> wkBuyList = null;
            List<Buy> buyList = new List<Buy>();
            List<BuySplit> splitList = new List<BuySplit>();
            List<BuySheet_CreatePO> POList = new List<BuySheet_CreatePO>();

            foreach (var rl in pm_ResultList)
                viewIDList.AddRange(new List<int>(rl.viewID));

            switch (cluster)
            {
                case "OS":
                    buyList = GlobalVariables.MySession.List_Buy_OSSheet.Where(b => viewIDList.Contains(b.viewID)).ToList();
                    splitList = GlobalVariables.MySession.List_Buy_OSSplits.Where(s => viewIDList.Contains(s.viewID)).ToList();
                    break;
                case "Domestic":
                    buyList = GlobalVariables.MySession.List_Buy_DomesticSheet.Where(b => viewIDList.Contains(b.viewID)).ToList();
                    splitList = GlobalVariables.MySession.List_Buy_DomesticSplits.Where(s => viewIDList.Contains(s.viewID)).ToList();
                    break;
                case "FTB":
                    buyList = GlobalVariables.MySession.List_Buy_FTBSheet.Where(b => viewIDList.Contains(b.viewID)).ToList();
                    splitList = GlobalVariables.MySession.List_Buy_FTBSplits.Where(s => viewIDList.Contains(s.viewID)).ToList();
                    break;
                default:
                    break;
            }

            foreach (var result in pm_ResultList)
            {
                wkBuyList = null;
                wkBuyList = buyList.Where(b => result.viewID.Contains(b.viewID)).ToList();

                j = 0;
                List<BuySheet_CreatePOLine> wkPOLine = new List<BuySheet_CreatePOLine>();
                foreach (var item in wkBuyList)
                {
                    wkPOLine.Add(
                        new BuySheet_CreatePOLine()
                        {
                            no = j++,
                            status = "",
                            BuySheet = item,
                            BuyRelease = splitList.FindAll(x => x.viewID == item.viewID).OrderBy(s => s.release_date).ToList()
                        }
                    );
                }

                POList.Add(new BuySheet_CreatePO()
                {
                    no = result.GroupNo,
                    go = result.msg.Replace("\r\n", "<br>"),
                    vendor_approve = buyList.First(b => b.viewID == result.viewID[0]).vendor_approve,
                    po_to_loc = buyList.First(b => b.viewID == result.viewID[0]).po_to_loc,
                    POLine = wkPOLine
                }
                );

            }

            return POList;
        }


        public async Task<SP_Return> GoApplyPOAsync(string cluster, string pm_entryID, string pm_PO_no)
        {
            SP_Return ApplyModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            int testPO = 0;
            int theURL = !isDev ? 1 : 2;
            string pm_parm = "";
            List<Buy> buyList = new List<Buy>();

            //double check
            switch (cluster)
            {
                case "OS":
                    ApplyModel = shareCommon.checkAuthorized(19);
                    break;
                case "Domestic":
                    ApplyModel = shareCommon.checkAuthorized(31);
                    break;
                case "FTB":
                    ApplyModel = shareCommon.checkAuthorized(107);
                    break;
                default:
                    ApplyModel.r = 0;
                    ApplyModel.msg = "Unavailable Cluster.";
                    break;
            }
            if (ApplyModel.r == 0)
                return ApplyModel;

            if (!int.TryParse(pm_PO_no, out testPO) || int.Parse(pm_PO_no) < 4000000 || int.Parse(pm_PO_no) > 5000000)
            {
                ApplyModel.r = 0;
                ApplyModel.msg = "Error PO number.";
                return ApplyModel;
            }

            if (string.IsNullOrWhiteSpace(pm_entryID))
            {
                ApplyModel.r = 0;
                ApplyModel.msg = "None of PO can be applied.";
                return ApplyModel;
            }

            List<string> list_entryID = pm_entryID.Replace("[", "").Replace("]", "").Replace("\"", "").Split(',').ToList();
            switch (cluster)
            {
                case "OS":
                    buyList = GlobalVariables.MySession.List_Buy_OSSheet.Where(b => list_entryID.Contains(b.entryID)).ToList();
                    break;
                case "Domestic":
                    buyList = GlobalVariables.MySession.List_Buy_DomesticSheet.Where(b => list_entryID.Contains(b.entryID)).ToList();
                    break;
                case "FTB":
                    buyList = GlobalVariables.MySession.List_Buy_FTBSheet.Where(b => list_entryID.Contains(b.entryID)).ToList();
                    break;
                default:
                    break;
            }

            if (buyList.Where(w => w.vendor_id == 0).ToList().Count() > 0)
            {
                ApplyModel.r = 0;
                ApplyModel.msg = "None of vendors is approved.";
                return ApplyModel;
            }

            if (buyList.Where(w => w.PO_no.Trim() != "").ToList().Count() > 0)
            {
                ApplyModel.r = 0;
                ApplyModel.msg = "Some of selected buys have PO number already.";
                return ApplyModel;
            }

            if (buyList.Select(s => s.vendor_id).Distinct().ToList().Count() != 1)
            {
                ApplyModel.r = 0;
                ApplyModel.msg = "Not all of selected buys belong to one approved vendor.";
                return ApplyModel;
            }

            ApplyModel = shareGet.isLinkedServerConnected("CLOUD");
            if (ApplyModel.r == 0)
                return ApplyModel;

            for (int i = 0; i < buyList.Count(); i++)
            {
                if (i > 0)
                    pm_parm += ",";
                pm_parm += buyList[i].viewID.ToString();
            }
            pm_parm = "{ 'P21URL': " + theURL.ToString() + ", 'pm_creator': '" + GlobalVariables.MySession.Account + "', 'pm_po_no': " + pm_PO_no + ", 'pm_buyID': '" + pm_parm + "' }";
            pm_parm = pm_parm.Replace("'", "\"");

            ApplyModel = await shareCommon.callSBS_API_Post("BuyApp/ApplyPOAsync", pm_parm);
            ApplyModel.msg = ApplyModel.JsonData;

            return ApplyModel;
        }


        public async Task<cmd_Response> GoCreatePOAsync(string cluster, List<BuySheet_BuyID> pm_Sheet)
        {
            cmd_Response ResponseModel = new cmd_Response();
            string pm_parm = "";
            int theURL = !isDev ? 1 : 2;

            if (pm_Sheet.Count() <= 0)
            {
                ResponseModel.ResultModel.r = 0;
                ResponseModel.ResultModel.msg = "None of PO can be created.";
                return ResponseModel;
            }

            //double check
            switch (cluster)
            {
                case "OS":
                    ResponseModel.ResultModel = shareCommon.checkAuthorized(19);
                    break;
                case "Domestic":
                    ResponseModel.ResultModel = shareCommon.checkAuthorized(31);
                    break;
                case "FTB":
                    ResponseModel.ResultModel = shareCommon.checkAuthorized(107);
                    break;
                default:
                    ResponseModel.ResultModel.r = 0;
                    ResponseModel.ResultModel.msg = "Unavailable Cluster.";
                    break;
            }
            if (ResponseModel.ResultModel.r == 0)
                return ResponseModel;

            ResponseModel.ResultModel = CheckProcess(string.Join(", ", pm_Sheet.Select(s => s.viewID).ToList()));
            if (ResponseModel.ResultModel.r == 0)
                return ResponseModel;

            ResponseModel.ResultModel = shareGet.isLinkedServerConnected("CLOUD");
            if (ResponseModel.ResultModel.r == 0)
                return ResponseModel;

            for (int i = 0; i < pm_Sheet.Count(); i++)
            {
                if (i > 0)
                    pm_parm += ", ";
                pm_parm += "{'GroupNo': " + pm_Sheet[i].GroupNo.ToString() + ", 'viewID': " + pm_Sheet[i].viewID.ToString() + "}";
            }
            pm_parm = "{ 'P21URL': " + theURL.ToString() + ", 'pmCreator': '" + GlobalVariables.MySession.Account + "', 'pmPOcount': " + pm_Sheet[pm_Sheet.Count() - 1].GroupNo.ToString() + ", 'pm_buyID': [" + pm_parm + "] }";
            pm_parm = pm_parm.Replace("'", "\"");

            ResponseModel.ResultModel = await shareCommon.callSBS_API_Post("BuyApp/CreatePOAsync", pm_parm);
            if (ResponseModel.ResultModel.r == 1)
                ResponseModel.ResultList = new List<cmd_Result>(JsonConvert.DeserializeObject<List<cmd_Result>>(ResponseModel.ResultModel.JsonData));

            return ResponseModel;
        }


        private SP_Return CheckProcess(string pmViewList)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmViewList", SqlDbType = SqlDbType.VarChar, Value = pmViewList}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_OS_BuySheet, "sbs_sp_OSBUY_CheckProcess", wkParm);
            if (ResultModel.r == 1)
                ResultModel = JsonConvert.DeserializeObject<SP_Return>(ResultModel.JsonData);

            return ResultModel;
        }


        public SP_Return GoCutPO(string cluster, string selected)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            string pmCommand = "";

            List<int> viewIDList = (selected.Replace("[", "").Replace("]", "").Split(',').ToList()).Select(int.Parse).ToList();
            List<Buy> buyList = new List<Buy>();
            List<Buy> excludeBuyList = new List<Buy>();

            switch (cluster)
            {
                case "OS":
                    buyList = GlobalVariables.MySession.List_Buy_OSSheet.Where(s => viewIDList.Contains(s.viewID)).ToList();
                    excludeBuyList = GlobalVariables.MySession.List_Buy_OSSheet.Where(s => viewIDList.All(v => v != s.viewID)).ToList();
                    break;
                case "Domestic":
                    buyList = GlobalVariables.MySession.List_Buy_DomesticSheet.Where(s => viewIDList.Contains(s.viewID)).ToList();
                    excludeBuyList = GlobalVariables.MySession.List_Buy_DomesticSheet.Where(s => viewIDList.All(v => v != s.viewID)).ToList();
                    break;
                case "FTB":
                    buyList = GlobalVariables.MySession.List_Buy_FTBSheet.Where(s => viewIDList.Contains(s.viewID)).ToList();
                    excludeBuyList = GlobalVariables.MySession.List_Buy_FTBSheet.Where(s => viewIDList.All(v => v != s.viewID)).ToList();
                    break;
                default:
                    break;
            }

            List<string> POList = buyList.GroupBy(d => new { d.PO_no }).Select(m => m.Key.PO_no).ToList();
            if (POList.Where(l => string.IsNullOrEmpty(l) == true).Count() > 0)
            {
                ResultModel.r = 0;
                ResultModel.msg = "Some of Buys in your selection has not created PO in P21 yet.";
                return ResultModel;
            }

            List<string> excludePOList = excludeBuyList.GroupBy(d => new { d.PO_no }).Select(m => m.Key.PO_no).ToList();
            List<BuySheet_BuyID> shouldSelect = excludeBuyList.Where(eb => POList.Contains(eb.PO_no)).Select(e => new BuySheet_BuyID() { viewID = e.viewID.ToString(), PO_no = e.PO_no }).ToList();
            if (shouldSelect.Count() > 0)
            {
                ResultModel.r = 0;
                ResultModel.msg =
                    "Accroding to all of PO numbers you chose, following " + shouldSelect.Count().ToString() + " Buy should be in as well. Please make sure your selection is correct." +
                    "\r\n" + "\r\n" +
                    string.Join("\r\n", shouldSelect.Select(a => "Buy ID: " + a.viewID + ", PO number: " + a.PO_no.Replace(" TBD", "")).ToArray());
                return ResultModel;
            }

            //double check
            switch (cluster)
            {
                case "OS":
                    ResultModel = shareCommon.checkAuthorized(19);
                    break;
                case "Domestic":
                    ResultModel = shareCommon.checkAuthorized(31);
                    break;
                case "FTB":
                    ResultModel = shareCommon.checkAuthorized(107);
                    break;
                default:
                    ResultModel.r = 0;
                    ResultModel.msg = "Unavailable Cluster.";
                    break;
            }
            if (ResultModel.r == 0)
                return ResultModel;

            pmCommand = string.Join(", ", POList.Select(p => "{'po_no': '" + p + "'}").ToArray());
            pmCommand = "{ 'pm_po': [" + pmCommand.Replace("TBD", "").Replace("\t", "").Replace(" ", "") + "], 'pm_usr': '" + GlobalVariables.MySession.Account + "' }";
            pmCommand = pmCommand.Replace("'", "\"");

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.NVarChar, Value = pmCommand}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_OS_BuySheet, "sbs_sp_OSBUY_CutPO", wkParm);
            if (ResultModel.r == 1)
                ResultModel = JsonConvert.DeserializeObject<SP_Return>(ResultModel.JsonData);

            return ResultModel;
        }


        public List<FTB_OpenPO> FTB_FetchOpenPO()
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<FTB_OpenPO> buyList = new List<FTB_OpenPO>();

            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "P21.pr_FTB_OpenPO");
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
            {
                buyList = JsonConvert.DeserializeObject<List<FTB_OpenPO>>(ResultModel.JsonData);
                foreach (var item in buyList)
                {
                    item.expedite_notes = string.IsNullOrWhiteSpace(item.expedite_notes) ? "" : item.expedite_notes.Replace("\r\n", "<br>");
                    //item.ftb_followup_comment = string.IsNullOrWhiteSpace(item.ftb_followup_comment) ? "" : item.ftb_followup_comment.Replace("\r\n", "<br>");
                }
                GlobalVariables.MySession.List_OpenPO_FTB = null;
                GlobalVariables.MySession.List_OpenPO_FTB = buyList;
            }

            return buyList;
        }

    }
}
