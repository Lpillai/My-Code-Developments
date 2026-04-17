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
    public class BuySplits_Program
    {
        //private SqlConnection OScon = GlobalConfig.MyConfig.Connection_OS_BuySheet;
        Common_Share shareCommon = new Common_Share();
        BuyOthers_Program pgmBuyOthers = new BuyOthers_Program();


        public List<BuySplit> FetchAllSplits(string pm_viewID)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<BuySplit> SplitList = null;

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pm_viewID", SqlDbType = SqlDbType.VarChar, Value = pm_viewID}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_OS_BuySheet, "sbs_sp_OSBUY_GetSplits1", wkParm);
            if (ResultModel.r == 1)
                SplitList = JsonConvert.DeserializeObject<List<BuySplit>>(ResultModel.JsonData);

            if (SplitList == null)
                SplitList = new List<BuySplit>();
            else
            {
                foreach (var item in SplitList)
                {
                    item.selected = true;
                    if ((item.release_date.ToString() == "") || (((DateTime)item.release_date).ToString("yyyy-MM-dd") == "1900-01-01"))
                        item.EDT_release_date = "";
                    else
                        item.EDT_release_date = ((DateTime)item.release_date).ToString("yyyy-MM-dd");
                    if ((item.expected_date.ToString() == "") || (((DateTime)item.expected_date).ToString("yyyy-MM-dd") == "1900-01-01"))
                        item.EDT_expected_date = "";
                    else
                        item.EDT_expected_date = ((DateTime)item.expected_date).ToString("yyyy-MM-dd");
                }
            }

            return SplitList;
        }

        /*
        public List<BuySplit> GetSplitsForOneBuy(string pm_EntryID, int pm_viewID)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<BuySplit> SplitsList = new List<BuySplit>();
            Buy theBuy = pgmBuyOthers.FetchOneBuy(pm_viewID);

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@entryId", SqlDbType = SqlDbType.NVarChar, Value = pm_EntryID}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_OS_BuySheet, "sbs_sp_OSBUY_GetSplits", wkParm);
            if (ResultModel.r == 1)
            {
                SplitsList = JsonConvert.DeserializeObject<List<BuySplit>>(ResultModel.JsonData);
                foreach (var item in SplitsList)
                {
                    if (item.release_date == null || ((DateTime)item.release_date).ToString("yyyy-MM-dd") == "1900-01-01")
                        item.EDT_release_date = "";
                    else
                        item.EDT_release_date = ((DateTime)item.release_date).ToString("yyyy-MM-dd");
                    if (item.expected_date == null || ((DateTime)item.expected_date).ToString("yyyy-MM-dd") == "1900-01-01")
                        item.EDT_expected_date = "";
                    else
                        item.EDT_expected_date = ((DateTime)item.expected_date).ToString("yyyy-MM-dd");

                    if (!string.IsNullOrWhiteSpace(theBuy.preload_loc) && int.Parse(theBuy.preload_loc) > 0)
                        item.preload_po_so = pgmBuyOthers.GetList_PreloadCalendar(theBuy.po_to_loc.ToString(), theBuy.preload_loc, (DateTime)item.release_date).FirstOrDefault(f => f.order_no.ToString() == item.preload_so).preload_po_so ?? "";
                }
            }

            return SplitsList;
        }
        */

        public SP_Return ManageSplits(string cluster, List<BuySplit> pm_splitList)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            SP_Return SplitModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            int i;
            int? subtotal = 0;
            int po_subtotal = 0;
            int? wkQuant = pm_splitList[0].sumQuant;
            int wk_poQuant = pm_splitList[0].po_sumQuant;

            for (i = 0; i < pm_splitList.Count(); i++)
            {
                if (pm_splitList[i].selected)   //enable this row
                {
                    if ((pm_splitList[i].quantity == null ? 0 : pm_splitList[i].quantity) <= 0 || pm_splitList[i].po_quantity <= 0)
                    {
                        ResultModel.r = 0;
                        ResultModel.msg = "Quantity must be greater than zero.";
                        break;
                    }

                    subtotal += pm_splitList[i].quantity;
                    po_subtotal += pm_splitList[i].po_quantity;
                }
            }

            if ((ResultModel.r == 1) && (subtotal != wkQuant))
            {
                ResultModel.r = 0;
                ResultModel.msg = "The subtotal of Splits is not equal to New Quantity of Buy #" + pm_splitList[0].viewID.ToString() + ".";
            }
            if ((ResultModel.r == 1) && (po_subtotal != wk_poQuant))
            {
                ResultModel.r = 0;
                ResultModel.msg = "The subtotal of Splits for PO is not equal to PO Quantity of Buy #" + pm_splitList[0].viewID.ToString() + ".";
            }
            if (ResultModel.r == 0)
                return ResultModel;

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

            for (i = 0; i < pm_splitList.Count(); i++)
            {
                if (pm_splitList[i].selected)                               //enable this row
                {/*
                    if (string.IsNullOrWhiteSpace(pm_splitList[i].splitID)) //this row is new split
                        SplitModel = CreateNewSplit(pm_splitList[i]);
                    else
                        SplitModel = EditOneSplit(pm_splitList[i]);
                    */
                    SplitModel = UpdateSplit(pm_splitList[i]);
                }
                else
                if (!pm_splitList[i].selected &&                            //disable this row
                    !string.IsNullOrWhiteSpace(pm_splitList[i].splitID))    //this row is old split
                {
                    SplitModel = DeleteOneSplit(pm_splitList[i]);
                }

                if (SplitModel.r == 0)
                {
                    ResultModel.r = 0;
                    ResultModel.msg += SplitModel.msg + "\r\n";
                }
            }

            if (ResultModel.r == 1)
                //regenerateSplits(cluster, pm_splitList[0].entryID, pm_splitList[0].viewID);
                regenerateSplits(cluster, pm_splitList[0].viewID);

            return ResultModel;
        }

        /*
        public SP_Return CreateNewSplit(BuySplit splitModel)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@entryId", SqlDbType = SqlDbType.VarChar, Value = splitModel.entryID},
                new SqlParameter() {ParameterName = "@ship_method", SqlDbType = SqlDbType.VarChar, Value = splitModel.ship_method},
                new SqlParameter() {ParameterName = "@release_date", SqlDbType = SqlDbType.Date, Value = splitModel.release_date},
                new SqlParameter() {ParameterName = "@expected_date", SqlDbType = SqlDbType.Date, Value = splitModel.expected_date},
                new SqlParameter() {ParameterName = "@quantity", SqlDbType = SqlDbType.Int, Value = splitModel.quantity},
                new SqlParameter() {ParameterName = "@po_quantity", SqlDbType = SqlDbType.Int, Value = splitModel.po_quantity},
                new SqlParameter() {ParameterName = "@preload_so", SqlDbType = SqlDbType.VarChar, Value = (splitModel.preload_so == null ? "" : splitModel.preload_so)}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_OS_BuySheet, "sbs_sp_OSBUY_SaveSplits", wkParm, true);

            return ResultModel;
        }


        public SP_Return EditOneSplit(BuySplit splitModel)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@entryId", SqlDbType = SqlDbType.VarChar, Value = splitModel.entryID},
                new SqlParameter() {ParameterName = "@ship_method", SqlDbType = SqlDbType.VarChar, Value = splitModel.ship_method},
                new SqlParameter() {ParameterName = "@splitId", SqlDbType = SqlDbType.VarChar, Value = splitModel.splitID},
                new SqlParameter() {ParameterName = "@release_date", SqlDbType = SqlDbType.Date, Value = splitModel.release_date},
                new SqlParameter() {ParameterName = "@expected_date", SqlDbType = SqlDbType.Date, Value = splitModel.expected_date},
                new SqlParameter() {ParameterName = "@quantity", SqlDbType = SqlDbType.Int, Value = splitModel.quantity},
                new SqlParameter() {ParameterName = "@po_quantity", SqlDbType = SqlDbType.Int, Value = splitModel.po_quantity},
                new SqlParameter() {ParameterName = "@preload_so", SqlDbType = SqlDbType.VarChar, Value = (splitModel.preload_so == null ? "" : splitModel.preload_so)}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_OS_BuySheet, "sbs_sp_OSBUY_UpdateSplits", wkParm, true);

            return ResultModel;
        }
        */

        public SP_Return UpdateSplit(BuySplit splitModel)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.NVarChar, Value = JsonConvert.SerializeObject(splitModel)}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_OS_BuySheet, "sbs_sp_OSBUY_UpdateSplits2", wkParm, true);

            return ResultModel;
        }


        public SP_Return DeleteOneSplit(BuySplit splitModel)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@entryId", SqlDbType = SqlDbType.VarChar, Value = splitModel.entryID},
                new SqlParameter() {ParameterName = "@splitId", SqlDbType = SqlDbType.VarChar, Value = splitModel.splitID}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_OS_BuySheet, "sbs_sp_OSBUY_DeleteSplits", wkParm, true);

            return ResultModel;
        }


        //public void regenerateSplits(string cluster, string pm_entryID, int pm_viewID)
        public void regenerateSplits(string cluster, int pm_viewID)
        {
            List<Buy> BuyList = new List<Buy>();
            List<BuySplit> SplitsListAllBuy = new List<BuySplit>();
            //List<BuySplit> newSplitsThisBuy = GetSplitsForOneBuy(pm_entryID, pm_viewID);
            List<BuySplit> newSplitsThisBuy = FetchAllSplits(pm_viewID.ToString());
            shareCommon.CleanCache();

            switch (cluster)
            {
                case "OS":
                    SplitsListAllBuy = GlobalVariables.MySession.List_Buy_OSSplits;
                    SplitsListAllBuy.RemoveAll(n => n.viewID == pm_viewID);
                    SplitsListAllBuy.AddRange(newSplitsThisBuy);
                    GlobalVariables.MySession.List_Buy_OSSplits = null;
                    GlobalVariables.MySession.List_Buy_OSSplits = SplitsListAllBuy;

                    BuyList = GlobalVariables.MySession.List_Buy_OSSheet;
                    BuyList.Where(b => b.viewID == pm_viewID).ToList().ForEach(b => b.Splits = newSplitsThisBuy.Count().ToString());
                    GlobalVariables.MySession.List_Buy_OSSheet = null;
                    GlobalVariables.MySession.List_Buy_OSSheet = BuyList;
                    break;
                case "Domestic":
                    SplitsListAllBuy = GlobalVariables.MySession.List_Buy_DomesticSplits;
                    SplitsListAllBuy.RemoveAll(n => n.viewID == pm_viewID);
                    SplitsListAllBuy.AddRange(newSplitsThisBuy);
                    GlobalVariables.MySession.List_Buy_DomesticSplits = null;
                    GlobalVariables.MySession.List_Buy_DomesticSplits = SplitsListAllBuy;

                    BuyList = GlobalVariables.MySession.List_Buy_DomesticSheet;
                    BuyList.Where(b => b.viewID == pm_viewID).ToList().ForEach(b => b.Splits = newSplitsThisBuy.Count().ToString());
                    GlobalVariables.MySession.List_Buy_DomesticSheet = null;
                    GlobalVariables.MySession.List_Buy_DomesticSheet = BuyList;
                    break;
                case "FTB":
                    SplitsListAllBuy = GlobalVariables.MySession.List_Buy_FTBSplits;
                    SplitsListAllBuy.RemoveAll(n => n.viewID == pm_viewID);
                    SplitsListAllBuy.AddRange(newSplitsThisBuy);
                    GlobalVariables.MySession.List_Buy_FTBSplits = null;
                    GlobalVariables.MySession.List_Buy_FTBSplits = SplitsListAllBuy;

                    BuyList = GlobalVariables.MySession.List_Buy_FTBSheet;
                    BuyList.Where(b => b.viewID == pm_viewID).ToList().ForEach(b => b.Splits = newSplitsThisBuy.Count().ToString());
                    GlobalVariables.MySession.List_Buy_FTBSheet = null;
                    GlobalVariables.MySession.List_Buy_FTBSheet = BuyList;
                    break;
                default:
                    break;
            }
        }

    }
}
