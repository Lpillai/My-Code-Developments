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

namespace IT
{
    public class Inventory_Program
    {
        public bool isDev = false;
        public string devMode = null;
        //private SqlConnection SBScon = GlobalConfig.MyConfig.Connection_SBS;
        Common_Share shareCommon = new Common_Share();
        Get_Share shareGet = new Get_Share();


        public Inventory_Program()
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(GlobalConfig.MyConfig.Connection_SBS.ConnectionString);

            if (builder.InitialCatalog.ToLower().Contains("dev"))
            {
                isDev = true;
                devMode = "You are using PLAY mode for Inventory app now.";
            }
        }


        #region Bin

        public Inventory_Bin Inventory_GetOneBin(string pm_dpt, int pm_uid)
        {
            if (GlobalVariables.MySession.List_InventoryBin == null || GlobalVariables.MySession.List_InventoryBin.Where(w => w.dpt == pm_dpt).Count() <= 0)
                return Inventory_FetchBinData(pm_dpt).theList.Find(f => f.bin_uid == pm_uid);
            else
                return GlobalVariables.MySession.List_InventoryBin.Find(f => f.dpt == pm_dpt && f.bin_uid == pm_uid);
        }


        public List<Inventory_Bin> Inventory_Locations(string pm_dpt)
        {
            if (GlobalVariables.MySession.List_InventoryBin == null || GlobalVariables.MySession.List_InventoryBin.Where(w => w.dpt == pm_dpt).Count() <= 0)
                return Inventory_FetchBinData(pm_dpt).theList.FindAll(f => f.parent_uid == 0);
            else
                return GlobalVariables.MySession.List_InventoryBin.FindAll(f => f.dpt == pm_dpt && f.parent_uid == 0);
        }


        public List<Inventory_Bin> Inventory_GetAvailableBinList(string pm_dpt)
        {
            if (GlobalVariables.MySession.List_InventoryBin == null || GlobalVariables.MySession.List_InventoryBin.Where(w => w.dpt == pm_dpt).Count() <= 0)
                return Inventory_FetchBinData(pm_dpt).theList.FindAll(f => !string.IsNullOrWhiteSpace(f.storage_code));
            else
                return GlobalVariables.MySession.List_InventoryBin.FindAll(f => f.dpt == pm_dpt && !string.IsNullOrWhiteSpace(f.storage_code));
        }


        public Inventory_BinData Inventory_FetchBinData(string pm_dpt)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            Inventory_BinData theData = new Inventory_BinData() { theList = new List<Inventory_Bin>(), theTree = new List<Inventory_BinTree>() };

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pm_dpt", SqlDbType = SqlDbType.VarChar, Value = pm_dpt}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "prIntranet_Inventory_GetBinList", wkParm);
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
            {
                theData.theList = JsonConvert.DeserializeObject<List<Inventory_Bin>>(ResultModel.JsonData);
                GlobalVariables.MySession.List_InventoryBin = null;
                GlobalVariables.MySession.List_InventoryBin = theData.theList;
                foreach (var item in GlobalVariables.MySession.List_InventoryBin)
                    item.dpt = pm_dpt;
                foreach (var item in theData.theList.Where(w => w.parent_uid == 0).ToList())
                {
                    theData.theTree.Add(new Inventory_BinTree()
                    {
                        bin_name = item.z_memo,
                        child_bins = Inventory_ToBinTree(theData.theList.FindAll(w => w.parent_uid == item.bin_uid))
                    });
                }
            }

            return theData;
        }


        public List<Inventory_BinTree> Inventory_ToBinTree(List<Inventory_Bin> pmList)
        {
            List<Inventory_BinTree> wkTree = new List<Inventory_BinTree>();

            foreach (var item in pmList)
            {
                wkTree.Add(new Inventory_BinTree()
                {
                    bin_uid = item.bin_uid,
                    bin_name = item.z_memo,
                    child_bins = Inventory_ToBinTree(GlobalVariables.MySession.List_InventoryBin.FindAll(w => w.parent_uid == item.bin_uid))
                });
            }

            return wkTree;
        }


        public SP_Return Inventory_UpdateBin(Inventory_Bin pm_Bin)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            string JsonString = "{'z_usr':'" + GlobalVariables.MySession.Account + "', 'theBins':" + JsonConvert.SerializeObject(pm_Bin) + "}";
            JsonString = JsonString.Replace("'", "\"");

            //double check
            ResultModel = shareCommon.checkAuthorized(112);
            if (ResultModel.r == 0)
                return ResultModel;

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.NVarChar, Value = JsonString}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "prIntranet_Inventory_ManageBins", wkParm);
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
                ResultModel = JsonConvert.DeserializeObject<SP_Return>(ResultModel.JsonData);

            return ResultModel;
        }

        #endregion

        #region Item

        public List<Inventory_History> Inventory_GetOneItemHistory(int pm_item_uid)
        {
            return GlobalVariables.MySession.List_InventoryItem.Find(f => f.item_uid == pm_item_uid).history;
        }


        public List<Inventory_Item> Inventory_GetItemList(string pm_dpt, string pm_bin_cd)
        {
            if (GlobalVariables.MySession.List_InventoryItem == null || GlobalVariables.MySession.List_InventoryItem.Where(w => w.dpt == pm_dpt).Count() <= 0)
                return Inventory_FetchItemList(pm_dpt).FindAll(f => f.loc_id == pm_bin_cd);
            else
                return GlobalVariables.MySession.List_InventoryItem.FindAll(f => f.dpt == pm_dpt && f.loc_id == pm_bin_cd);
        }


        public List<Inventory_Item> Inventory_FetchItemList(string pm_dpt)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<Inventory_Item> theItems = new List<Inventory_Item>();

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pm_dpt", SqlDbType = SqlDbType.VarChar, Value = pm_dpt}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "prIntranet_Inventory_GetItemList", wkParm);
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
            {
                theItems = JsonConvert.DeserializeObject<List<Inventory_Item>>(ResultModel.JsonData);
                GlobalVariables.MySession.List_InventoryItem = null;
                GlobalVariables.MySession.List_InventoryItem = theItems;
                foreach (var item in GlobalVariables.MySession.List_InventoryItem)
                {
                    item.dpt = pm_dpt;
                    item.qty = (item.amount == null ? 0 : item.amount.Sum(s => s.quantity));
                }
            }

            return theItems;
        }


        public SP_Return Inventory_CreateItem(Inventory_Item pm_item, string pm_storage_code)
        {
            pm_item.history = null;
            pm_item.item_desc = pm_item.model;
            if (pm_item.dpt == "IT" && Int32.Parse(pm_item.category) < 10)
            {
                pm_item.qty = 1;
                //pm_item.item_desc += ", OS: " + pm_item.thePC.operating_system + ", Office: " + pm_item.thePC.office + ", TeamViewer: " + pm_item.thePC.team_viewer;
                pm_item.item_desc += ", OS: " + pm_item.thePC.operating_system + ", Office: " + pm_item.thePC.office;
            }
            pm_item.category = shareGet.FetchCodeList(pm_item.dpt + "_Category", 1).First(f => f.Code == pm_item.category).Value;

            Inventory_History theHistory = new Inventory_History()
            {
                item_uid = 0,
                item_id = pm_item.item_id,
                behavior = "Purchase",
                requisition_usr = null,
                storage_code = pm_storage_code,
                quantity = pm_item.qty,
                z_memo = "",
                inv_account = GlobalVariables.MySession.Account,
                requisition_date = pm_item.purchase_date,
                dpt = pm_item.dpt
            };

            return Inventory_AppendHistory(theHistory, JsonConvert.SerializeObject(pm_item));
        }


        public SP_Return Inventory_AppendHistory(Inventory_History pm_history, string pm_json_item)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };

            //double check
            ResultModel = shareCommon.checkAuthorized(113);
            if (ResultModel.r == 0)
                return ResultModel;

            //pm_history.requisition_date = DateTime.Now;
            if (pm_history.quantity == 0)
                pm_history.quantity = 1;
            pm_history.inv_account = GlobalVariables.MySession.Account;

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.VarChar, Value = JsonConvert.SerializeObject(pm_history)},
                new SqlParameter() {ParameterName = "@pmJSON_item", SqlDbType = SqlDbType.VarChar, Value = pm_json_item}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "prIntranet_Inventory_AppendHistory", wkParm);
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
            {
                ResultModel = JsonConvert.DeserializeObject<SP_Return>(ResultModel.JsonData);
                GlobalVariables.MySession.List_InventoryItem = null;
            }

            return ResultModel;
        }

        #endregion

    }
}
