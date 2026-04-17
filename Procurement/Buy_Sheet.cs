using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using RuntimeVariables;
using RuntimeConfig;
using SharedScrpits;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Procurement
{
    public class BuySheet_Program
    {
        public bool isDev = false;
        public string devMode = null;
        //private SqlConnection OScon = GlobalConfig.MyConfig.Connection_OS_BuySheet;
        Common_Share shareCommon = new Common_Share();
        Filters_Share shareFilters = new Filters_Share();
        Get_Share shareGet = new Get_Share();
        //Buy_Fileter filterBuy = new Buy_Fileter();
        BuyNote_Program pgmBuyNote = new BuyNote_Program();
        BuySplits_Program pgmBuySplits = new BuySplits_Program();
        BuyOthers_Program pgmBuyOverSeaOthers = new BuyOthers_Program();


        public BuySheet_Program()
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(GlobalConfig.MyConfig.Connection_OS_BuySheet.ConnectionString);

            if (builder.InitialCatalog.ToLower().Contains("dev"))
            {
                isDev = true;
                devMode = "You are using PLAY mode for Buy app now.";
            }
        }


        public string GetHeaderLink(string cluster, string theField)
        {
            string htmlText = "";
            //string htmlText = "<a href=\"https://localhost:44388/OSBuy/SortSheet?cluster=OS&sort_field=viewID\">buy_id</a>";
            MemberInfo property = typeof(Buy).GetProperty(theField);
            string DisplayName = "";
            Sorts theSort = new Sorts();
            List<Sorts> theOrder = new List<Sorts>();

            if (cluster != "Domestic" && theField == "required_date")
                DisplayName = "Supplier Ship Date (Req'd Date)";
            else
                DisplayName = property.GetCustomAttributes(typeof(DisplayNameAttribute), true).Cast<DisplayNameAttribute>().Single().DisplayName;

            if (GlobalVariables.MySession.isDebug)
                htmlText = "<a class=\"link\" href=\"/Procurement/SortSheet?cluster=" + cluster + "&sort_field=" + theField + "\">" + DisplayName + "</a>";
            else
                //htmlText = "<a class=\"link\" href=\"/" + GlobalConfig.MyConfig.ProjectName + "/Procurement/SortSheet?cluster=" + cluster + "&sort_field=" + theField + "\">" + DisplayName + "</a>";
                htmlText = "<a class=\"link\" href=\"/" + shareGet.GetProjectName() + "/Procurement/SortSheet?cluster=" + cluster + "&sort_field=" + theField + "\">" + DisplayName + "</a>";

            switch (cluster)
            {
                case "OS":
                    if (GlobalVariables.MySession.List_Buy_OSOrder == null)
                        SetOrder(cluster, "viewID");
                    theOrder = GlobalVariables.MySession.List_Buy_OSOrder;
                    break;
                case "Domestic":
                    if (GlobalVariables.MySession.List_Buy_DomesticOrder == null)
                        SetOrder(cluster, "viewID");
                    theOrder = GlobalVariables.MySession.List_Buy_DomesticOrder;
                    break;
                case "FTB":
                    if (GlobalVariables.MySession.List_Buy_FTBOrder == null)
                        SetOrder(cluster, "viewID");
                    theOrder = GlobalVariables.MySession.List_Buy_FTBOrder;
                    break;
                default:
                    break;
            }

            if (theOrder.Exists(e => e.field_name == theField))
            {
                theSort = theOrder.FirstOrDefault(o => o.field_name == theField);
                htmlText += " " + (theSort.descend == true ? "↑" : "↓") + theSort.order_no.ToString();
            }

            return htmlText;
        }


        public string GetHeaderLink_FTB(string theField)
        {
            string htmlText = "";
            //string htmlText = "<a href=\"https://localhost:44388/OSBuy/SortSheet?cluster=OS&sort_field=viewID\">buy_id</a>";
            MemberInfo property = typeof(BuyFTB).GetProperty(theField);
            string DisplayName = property.GetCustomAttributes(typeof(DisplayNameAttribute), true).Cast<DisplayNameAttribute>().Single().DisplayName;
            Sorts theSort = new Sorts();
            List<Sorts> theOrder = new List<Sorts>();

            if (GlobalVariables.MySession.isDebug)
                htmlText = "<a class=\"link\" href=\"/Procurement/SortSheet?cluster=FTB&sort_field=" + theField + "\">" + DisplayName + "</a>";
            else
                //htmlText = "<a class=\"link\" href=\"/" + GlobalConfig.MyConfig.ProjectName + "/Procurement/SortSheet?cluster=FTB&sort_field=" + theField + "\">" + DisplayName + "</a>";
                htmlText = "<a class=\"link\" href=\"/" + shareGet.GetProjectName() + "/Procurement/SortSheet?cluster=FTB&sort_field=" + theField + "\">" + DisplayName + "</a>";

            if (GlobalVariables.MySession.List_Buy_FTBOrder == null)
                SetOrder("FTB", "viewID");
            theOrder = GlobalVariables.MySession.List_Buy_FTBOrder;

            if (theOrder.Exists(e => e.field_name == theField))
            {
                theSort = theOrder.FirstOrDefault(o => o.field_name == theField);
                htmlText += " " + (theSort.descend == true ? "↑" : "↓") + theSort.order_no.ToString();
            }

            return htmlText;
        }


        public List<Vendor> GetList_Vendor(string pm_supplier_id, string pm_company_id)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<Vendor> theList = null;
            string pmJSON = "{'pm_supplier_id': " + pm_supplier_id + ", 'pm_company_id': " + pm_company_id + "}";
            pmJSON = pmJSON.Replace("'", "\"");

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.NVarChar, Value = pmJSON}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_OS_BuySheet, "sbs_sp_OSBUY_GetVendorList", wkParm);
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
                theList = JsonConvert.DeserializeObject<List<Vendor>>(ResultModel.JsonData);

            return theList;
        }


        public List<Vendor> GetList_Supplier(string pm_item_id, string pm_location_id)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<Vendor> theList = new List<Vendor>();
            string pmJSON = "{'pm_item_id': '" + pm_item_id + "', 'pm_location_id': " + pm_location_id + "}";
            pmJSON = pmJSON.Replace("'", "\"");

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.NVarChar, Value = pmJSON}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_OS_BuySheet, "sbs_sp_OSBUY_GetSupplierList", wkParm);
            if (ResultModel.r == 1)
                theList = JsonConvert.DeserializeObject<List<Vendor>>(ResultModel.JsonData);

            return theList;
        }


        public BuySheet_SupplierInfo Get_SupplierInfo(string pm_location_id, string pm_item_id, string pm_supplier_id)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            BuySheet_SupplierInfo theInfo = new BuySheet_SupplierInfo();
            string pmJSON = "{'pm_location_id': " + pm_location_id + ", 'pm_item_id': '" + pm_item_id + "', 'pm_supplier_id': " + pm_supplier_id + "}";
            pmJSON = pmJSON.Replace("'", "\"");

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.NVarChar, Value = pmJSON}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_OS_BuySheet, "sbs_sp_OSBUY_GetSupplierInfo", wkParm);
            if (ResultModel.r == 1)
                theInfo = JsonConvert.DeserializeObject<BuySheet_SupplierInfo>(ResultModel.JsonData);

            return theInfo;
        }


        public Buy GetPartInfo(Buy pmBuy)
        {
            SqlConnection OScon = new SqlConnection(GlobalConfig.MyConfig.Connection_OS_BuySheet.ConnectionString);
            SqlCommand cmd = new SqlCommand("", OScon);
            decimal wkDecimal = 0;
            int wkInt = 0;
            Buy buyModel = new Buy();
            buyModel.cluster = pmBuy.cluster;
            buyModel.ItemID = pmBuy.ItemID;
            buyModel.buy_to_loc = pmBuy.buy_to_loc;
            int purchase_pricing_unit_size = 1;

            try
            {
                cmd.CommandText = "sbs_sp_OSBUY_PartInfo1";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Parameters.AddWithValue("@item", pmBuy.ItemID);
                cmd.Parameters.AddWithValue("@loc", pmBuy.buy_to_loc);
                OScon.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        //buyModel.planner = (DBNull.Value.Equals(reader["product_group_desc"])) ? "" : reader["product_group_desc"].ToString();
                        buyModel.item_desc = (DBNull.Value.Equals(reader["item_desc"])) ? "" : reader["item_desc"].ToString();
                        buyModel.extended_desc = (DBNull.Value.Equals(reader["extended_desc"])) ? "" : reader["extended_desc"].ToString();
                        buyModel.part_assignment = (DBNull.Value.Equals(reader["part_assignment"])) ? "" : reader["part_assignment"].ToString();
                        buyModel.purchase_class = (DBNull.Value.Equals(reader["purchase_class"])) ? "" : reader["purchase_class"].ToString();
                        buyModel.PPAP = (DBNull.Value.Equals(reader["PPAP"])) ? "" : reader["PPAP"].ToString();
                        buyModel.program = (DBNull.Value.Equals(reader["program"])) ? "" : reader["program"].ToString();
                        buyModel.standard_cost = (DBNull.Value.Equals(reader["standard_cost"])) ? "" : reader["standard_cost"].ToString();
                        buyModel.supplier_standard_cost = (DBNull.Value.Equals(reader["supplier_standard_cost"])) ? "" : reader["supplier_standard_cost"].ToString();
                        buyModel.loc_supplier_standard_cost = (DBNull.Value.Equals(reader["loc_supplier_standard_cost"])) ? "" : reader["loc_supplier_standard_cost"].ToString();
                        buyModel.unit_weight = (DBNull.Value.Equals(reader["unit_weight"])) ? "" : reader["unit_weight"].ToString();
                        buyModel.MOQSupplierLoc1 = (DBNull.Value.Equals(reader["MOQSupplierLoc1"])) ? "" : reader["MOQSupplierLoc1"].ToString();
                        buyModel.LeadTime = (DBNull.Value.Equals(reader["LeadTime"])) ? "" : reader["LeadTime"].ToString();
                        buyModel.primary_supplier_id = decimal.TryParse(reader["primary_supplier_id"].ToString(), out wkDecimal) ? wkDecimal : 0;
                        buyModel.primary_supplier_currency = int.TryParse(reader["primary_supplier_currency"].ToString(), out wkInt) ? wkInt : 1;
                        buyModel.TruePrimarySupplier = (DBNull.Value.Equals(reader["TruePrimarySupplier"])) ? "" : reader["TruePrimarySupplier"].ToString();
                        buyModel.loc_primary_supplier_name = (DBNull.Value.Equals(reader["loc_primary_supplier_name"])) ? "" : reader["loc_primary_supplier_name"].ToString();
                        buyModel.last_vendor = (DBNull.Value.Equals(reader["last_vendor"])) ? "" : reader["last_vendor"].ToString();
                        if ((reader["last_PO_date"].ToString() == "") || (((DateTime)reader["last_PO_date"]).ToString("yyyy-MM-dd") == "1900-01-01"))
                        {
                            buyModel.last_PO_date = null;
                            //buyModel.EDT_last_PO_date = "";
                        }
                        else
                        {
                            buyModel.last_PO_date = ((DateTime)reader["last_PO_date"]);
                            //buyModel.EDT_last_PO_date = ((DateTime)reader["last_PO_date"]).ToString("yyyy-MM-dd");
                        }
                        if (buyModel.cluster == "Domestic")
                            buyModel.last_price = (DBNull.Value.Equals(reader["last_price"])) ? "" : (((decimal)reader["last_price"]) / 100).ToString();
                        else
                            buyModel.last_price = (DBNull.Value.Equals(reader["last_price"])) ? "" : reader["last_price"].ToString();
                        buyModel.previous_quantity = (DBNull.Value.Equals(reader["previous_quantity"])) ? "" : reader["previous_quantity"].ToString();
                        buyModel.package = (DBNull.Value.Equals(reader["supplier_package_qty"])) ? "" : reader["supplier_package_qty"].ToString();
                        buyModel.standard_cost_current = decimal.TryParse(reader["standard_cost_current"].ToString(), out wkDecimal) ? wkDecimal : 0;
                        buyModel.buyer_name = (DBNull.Value.Equals(reader["buyer_name"])) ? "" : reader["buyer_name"].ToString();
                    }
                    if (pmBuy.cluster != "OS")
                    {
                        if (pmBuy.cluster == "FTB") //FTB = C, Domestic = EA
                            purchase_pricing_unit_size = 100;

                        decimal exchange_rate = shareGet.Fetch_LatestExchangeRate().Find(f => f.currency_id == buyModel.primary_supplier_currency).exchange_rate;
                        decimal unit_price = Decimal.Parse(buyModel.supplier_standard_cost) * purchase_pricing_unit_size;
                        decimal price_usd = unit_price / exchange_rate;
                        decimal price_diff = buyModel.cluster == "FTB" || buyModel.last_price == "" || Decimal.Parse(buyModel.last_price) == 0 ? 0 : Math.Round((price_usd / Decimal.Parse(buyModel.last_price) - 1) * 100, 2, MidpointRounding.AwayFromZero);

                        buyModel.theQuotes = null;
                        buyModel.theQuotes = new List<BuyQuote>();
                        buyModel.theQuotes.Add(new BuyQuote() { quote_seq = 0, lead = "0" });
                        buyModel.theQuotes.Add(new BuyQuote() { 
                            quote_seq = 1, 
                            z_status = "", 
                            supplier_id = Convert.ToInt32(buyModel.primary_supplier_id), 
                            supplier_name = buyModel.TruePrimarySupplier, 
                            moq = int.Parse(buyModel.MOQSupplierLoc1.Replace(",", "")), 
                            unit_price = unit_price, 
                            lead = buyModel.LeadTime, 
                            currency_id = buyModel.primary_supplier_currency, 
                            currency_rate = exchange_rate,
                            price_usd = Math.Round(price_usd, 4, MidpointRounding.AwayFromZero),
                            tariff = 0,
                            price_all_in = Math.Round(price_usd + 0, 4, MidpointRounding.AwayFromZero),
                            price_diff = (float)price_diff
                        });
                    }
                    if (pmBuy.cluster == "FTB")
                    {
                        buyModel.theQuotes[1].z_status = "A";
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (OScon.State != ConnectionState.Closed) { OScon.Close(); }
                cmd.Dispose();
            }

            List<Location> LocationList = shareGet.GetList_Location();
            buyModel.company_id = LocationList.SingleOrDefault(l => l.location_id == buyModel.buy_to_loc).company_id;
            buyModel.po_to_loc = Decimal.Parse(buyModel.buy_to_loc);

            buyModel.required_date = DateTime.Now.Date;
            //buyModel.ship_out_date = buyModel.required_date;
            buyModel.expected_date = buyModel.required_date;

            if (buyModel.cluster == "FTB")
            {
                buyModel.theFTB = new BuyFTB();
                buyModel.theFTB.theDocuments = GetList_FTB_Doucments(buyModel.ItemID);
                if (buyModel.PPAP == "PPAP1" || buyModel.PPAP == "PPAP2" || buyModel.PPAP == "PPAP3" || buyModel.PPAP == "PPAP4")
                    buyModel.theFTB.ppap_level = buyModel.PPAP;
            }

            return buyModel;
        }


        public List<BuyFTB_document> GetList_FTB_Doucments(string pm_item_id)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<BuyFTB_document> buyList = new List<BuyFTB_document>();

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pm_item_id", SqlDbType = SqlDbType.VarChar, Value = pm_item_id}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_OS_BuySheet, "sbs_sp_OSBUY_PartInfo_documents", wkParm);
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
            {
                buyList = JsonConvert.DeserializeObject<List<BuyFTB_document>>(ResultModel.JsonData);
                foreach (var doc_item in buyList)
                {
                    doc_item.folder_path = Path.GetDirectoryName(doc_item.link_path);
                    doc_item.file_name = Path.GetFileName(doc_item.link_path);
                }
            }

            return buyList;
        }


        public Buy getOneBuy(string cluster, string pm_entryID)
        {
            if (string.IsNullOrWhiteSpace(pm_entryID))
                return null;

            decimal decimalVaule;
            decimal theCurrency;
            Buy theOne = new Buy();
            switch (cluster)
            {
                case "OS":
                    theOne = GlobalVariables.MySession.List_Buy_OSSheet.FirstOrDefault(x => x.entryID == pm_entryID);
                    break;
                case "Domestic":
                    theOne = GlobalVariables.MySession.List_Buy_DomesticSheet.FirstOrDefault(x => x.entryID == pm_entryID);
                    break;
                case "FTB":
                    theOne = GlobalVariables.MySession.List_Buy_FTBSheet.FirstOrDefault(x => x.entryID == pm_entryID);

                    //decimalVaule = (decimal)(Int32.Parse(theOne.PO_order_quantity_1) * theOne.theQuotes[0].unit_price / 100 + theOne.theFTB.ppap_cost + theOne.theFTB.tc_cost);
                    if (theOne.theQuotes.Where(w => w.z_status == "A").Count() == 1)
                    {
                        decimalVaule = theOne.theQuotes.Find(f => f.z_status == "A").unit_price;
                        theCurrency = theOne.theQuotes.Find(f => f.z_status == "A").currency_rate;
                    }
                    else
                    {
                        decimalVaule = theOne.theQuotes[0].unit_price;
                        theCurrency = theOne.theQuotes[0].currency_rate;
                    }
                    decimalVaule = (decimal)(Int32.Parse(theOne.PO_order_quantity_1) * decimalVaule / 100 + theOne.theFTB.ppap_cost + theOne.theFTB.tc_cost);
                    decimalVaule /= theCurrency;

                    theOne.grand_total_value = decimalVaule.ToString("#,##0.##");
                    break;
                default:
                    theOne = null;
                    break;
            }

            return theOne;
        }


        public List<Buy> GetBuySheet(string cluster)
        {
            shareCommon.CleanCache();
            List<Buy> buyList = new List<Buy>();
            List<Filters> FilterList = new List<Filters>();
            List<Sorts> SortList = new List<Sorts>();
            int i = 0;

            switch (cluster)
            {
                case "OS":
                    if (GlobalVariables.MySession.List_Buy_OSSheet == null)
                        GlobalVariables.MySession.List_Buy_OSSheet = FetchAllBuys(cluster).ToList();
                    if (GlobalVariables.MySession.List_Buy_OSOrder != null)
                        SortList = GlobalVariables.MySession.List_Buy_OSOrder;
                    buyList = GlobalVariables.MySession.List_Buy_OSSheet;
                    break;
                case "Domestic":
                    if (GlobalVariables.MySession.List_Buy_DomesticSheet == null)
                        GlobalVariables.MySession.List_Buy_DomesticSheet = FetchAllBuys(cluster).ToList();
                    if (GlobalVariables.MySession.List_Buy_DomesticOrder != null)
                        SortList = GlobalVariables.MySession.List_Buy_DomesticOrder;
                    buyList = GlobalVariables.MySession.List_Buy_DomesticSheet;
                    break;
                case "FTB":
                    if (GlobalVariables.MySession.List_Buy_FTBSheet == null)
                    {
                        GlobalVariables.MySession.List_Buy_FTBSheet = FetchAllBuys(cluster).ToList();
                        if (GlobalVariables.MySession.List_Buy_FTBOrder == null)
                        {
                            GlobalVariables.MySession.List_Buy_FTBOrder = new List<Sorts>();
                            GlobalVariables.MySession.List_Buy_FTBOrder.Add(new Sorts() { descend = true, field_name = "viewID", order_no = 1 });
                        }
                    }
                    if (GlobalVariables.MySession.List_Buy_FTBOrder != null)
                        SortList = GlobalVariables.MySession.List_Buy_FTBOrder;
                    buyList = GlobalVariables.MySession.List_Buy_FTBSheet;
                    break;
                default:
                    buyList = null;
                    break;
            }


            if (buyList != null &&
                ((cluster == "Domestic" && GlobalVariables.MySession.List_Buy_DomesticFilters != null) ||
                (cluster == "FTB" && GlobalVariables.MySession.List_Buy_FTBFilters != null) ||
                (cluster == "OS" && GlobalVariables.MySession.List_Buy_OSFilters != null)))
            {
                switch (cluster)
                {
                    case "OS":
                        FilterList = GlobalVariables.MySession.List_Buy_OSFilters;
                        break;
                    case "Domestic":
                        FilterList = GlobalVariables.MySession.List_Buy_DomesticFilters;
                        break;
                    case "FTB":
                        FilterList = GlobalVariables.MySession.List_Buy_FTBFilters;
                        break;
                    default:
                        break;
                }

                foreach (var item in FilterList)
                {
                    if (String.IsNullOrEmpty(item.op))
                        continue;

                    switch (item.value_type)
                    {
                        case "int":
                            //buyList = filterBuy.parseFilters_Int(buyList, item);
                            buyList = shareFilters.parseFilters_Int<Buy>(buyList, item);
                            break;
                        case "number":
                            //buyList = filterBuy.parseFilters_Decimal(buyList, item);
                            buyList = shareFilters.parseFilters_Decimal<Buy>(buyList, item);
                            break;
                        case "date":
                            //buyList = filterBuy.parseFilters_DateTime(buyList, item);
                            buyList = shareFilters.parseFilters_DateTime<Buy>(buyList, item);
                            break;
                        default:
                            //buyList = filterBuy.parseFilters_String(buyList, item);
                            buyList = shareFilters.parseFilters_String<Buy>(buyList, item);
                            break;
                    }
                }
            }

            buyList = SortSheet(buyList, SortList);
            i = 0;
            foreach (var buy in buyList)
            {
                i++;
                buy.no = i;
            }

            return buyList;
        }


        public List<Buy> FetchAllBuys(string cluster)
        {
            //string codePoint = "1F4AC";//💬
            string codePoint = "1F5CE";//🗎
            int code = int.Parse(codePoint, System.Globalization.NumberStyles.HexNumber);
            string unicodeString = char.ConvertFromUtf32(code);

            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<Buy> buyList = new List<Buy>();
            int i;

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@cluster", SqlDbType = SqlDbType.VarChar, Value = cluster}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_OS_BuySheet, "sbs_sp_OSBUY_FetchAllBuys", wkParm);
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
                buyList = JsonConvert.DeserializeObject<List<Buy>>(ResultModel.JsonData);


            List<BuyFTB> FTBList = new List<BuyFTB>();
            if (cluster == "FTB")
                FTBList = pgmBuyOverSeaOthers.FetchAllFTB();


            var find = from data in buyList
                       select new BuySheet_BuyID() { viewID = data.viewID.ToString() + ", " };
            List<BuySheet_BuyID> pmFetch = find.ToList();
            string theseBuys = string.Join("", pmFetch.Select(a => a.viewID).ToArray()).Replace(",", " ");

            List<BuyNote> NotesList = pgmBuyNote.FetchAllNotes(theseBuys);
            List<BuySplit> SplitsList = pgmBuySplits.FetchAllSplits(theseBuys);
            switch (cluster)
            {
                case "OS":
                    GlobalVariables.MySession.List_Buy_OSNotes = NotesList;
                    GlobalVariables.MySession.List_Buy_OSSplits = SplitsList;
                    break;
                case "Domestic":
                    GlobalVariables.MySession.List_Buy_DomesticNotes = NotesList;
                    GlobalVariables.MySession.List_Buy_DomesticSplits = SplitsList;
                    break;
                case "FTB":
                    GlobalVariables.MySession.List_Buy_FTBNotes = NotesList;
                    GlobalVariables.MySession.List_Buy_FTBSplits = SplitsList;
                    break;
                default:
                    break;
            }

            i = 0;
            while (i < buyList.Count())
            {
                //decimal tmpValue = decimal.Parse(buyList[i].total_value.Replace(" ","").Replace(",", ""));
                //buyList[i].total_value = String.Format("{0:#,0.##}", tmpValue);
                if (!string.IsNullOrEmpty(buyList[i].total_value))
                    buyList[i].total_value = String.Format("{0:#,0.##}", decimal.Parse(buyList[i].total_value.Replace(" ", "").Replace(",", "")));

                if (cluster == "FTB")
                {
                    if (FTBList.Find(f => f.viewID == buyList[i].viewID) == null)
                    {
                        buyList.Remove(buyList[i]);
                        continue;
                    }

                    buyList[i].theFTB = FTBList.First(f => f.viewID == buyList[i].viewID);
                    buyList[i].filter_sourcing = buyList[i].theFTB.sourcing;
                    buyList[i].filter_buyer = buyList[i].theFTB.procure;
                }

                if (NotesList == null)
                    buyList[i].noteCount = unicodeString + "0";
                else
                {
                    if (buyList[i].noteCount == "y")
                        buyList[i].noteCount = NotesList.FindAll(x => x.viewID == buyList[i].viewID).Count().ToString() + "/n";
                    else
                        buyList[i].noteCount = unicodeString + NotesList.FindAll(x => x.viewID == buyList[i].viewID).Count().ToString();
                }

                if (SplitsList == null)
                    buyList[i].Splits = "0";
                else
                    buyList[i].Splits = SplitsList.FindAll(x => x.viewID.Equals(buyList[i].viewID)).Count().ToString();

                i++;
            }

            return buyList;
        }


        #region Edit

        public SP_Return ImportData(string pm_Path, string cluster)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };

            //double check
            switch (cluster)
            {
                case "OS":
                    ResultModel = shareCommon.checkAuthorized(18);
                    break;
                case "Domestic":
                    ResultModel = shareCommon.checkAuthorized(30);
                    break;
                case "FTB":
                    ResultModel = shareCommon.checkAuthorized(106);
                    break;
                default:
                    ResultModel.r = 0;
                    ResultModel.msg = "Unavailable Cluster.";
                    break;
            }
            if (ResultModel.r == 0)
                return ResultModel;

            string json_parm = "";
            string wkMsg = "";
            string wkDate_string = "";
            double wkDate_double = 0;
            int rowCount = 0;
            ExcelWorksheet wkSheet;
            bool hasHeader = true;
            DataTable wkTable = new DataTable();

            //To avoid showing message: "Please set the ExcelPackage.LicenseContext property"
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            using (var excelPackage = new OfficeOpenXml.ExcelPackage())
            {
                try
                {
                    using (var stream = File.OpenRead(pm_Path))
                    {
                        excelPackage.Load(stream);
                    }
                    wkSheet = excelPackage.Workbook.Worksheets[0];

                    foreach (var firstRowCell in wkSheet.Cells[1, 1, 1, wkSheet.Dimension.End.Column])
                        wkTable.Columns.Add(hasHeader ? firstRowCell.Text : string.Format("Column {0}", firstRowCell.Start.Column));

                    var startRow = hasHeader ? 2 : 1;
                    for (int rowNum = startRow; rowNum <= wkSheet.Dimension.End.Row; rowNum++)
                    {
                        var wsRow = wkSheet.Cells[rowNum, 1, rowNum, wkSheet.Dimension.End.Column];
                        DataRow row = wkTable.Rows.Add();
                        foreach (var cell in wsRow)
                            //row[cell.Start.Column - 1] = cell.Text;
                            row[cell.Start.Column - 1] = cell.Value;
                    }

                    foreach (DataRow dr in wkTable.Rows)
                    {
                        wkDate_string = "";
                        wkDate_double = 0;

                        if (!string.IsNullOrWhiteSpace(dr["Item ID"].ToString()) && !string.IsNullOrWhiteSpace(dr["Buy to Loc"].ToString()))
                        {
                            wkDate_string = dr["Required Ship Out Date"].ToString().Replace("/", "-");
                            if (double.TryParse(wkDate_string, out wkDate_double))
                                wkDate_string = DateTime.FromOADate(wkDate_double).ToString("yyyy-MM-dd"); //grab date from number format to correct string

                            json_parm += "{";
                            json_parm += "'entryID': '" + Convert.ToString(Guid.NewGuid()) + "', ";
                            //json_parm += "'Planner': '" + dr["Planner"].ToString().Trim() + "', ";
                            json_parm += "'ItemID': '" + dr["Item ID"].ToString().Trim() + "', ";
                            json_parm += "'BuyToLoc': '" + dr["Buy to Loc"].ToString().Trim() + "', ";
                            json_parm += "'Sourcing': '" + dr["Sourcing (Y/N)"].ToString().Trim() + "', ";
                            json_parm += "'NewQuantity': '" + dr["New Quantity"].ToString().Trim() + "', ";
                            json_parm += "'RequiredShipOutDate': '" + wkDate_string + "', ";
                            json_parm += "'PreloadQuantity': '" + dr["Preload Quantity"].ToString().Trim() + "', ";
                            json_parm += "'PreloadLoc': '" + dr["Preload Loc"].ToString().Trim() + "', ";
                            json_parm += "'Package': '" + dr["Package"].ToString().Trim() + "'";
                            json_parm += "}";

                            rowCount++;
                        }
                    }
                    json_parm = json_parm.Replace("}{", "},{");
                    json_parm = "{'cluster': '" + cluster + "', 'pm_usr': '" + GlobalVariables.MySession.Account + "', 'NewBuys': [" + json_parm + "]}";
                    json_parm = json_parm.Replace("'", "\"");

                    List<SqlParameter> wkParm = new List<SqlParameter>()
                    {
                        new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.NVarChar, Value = json_parm}
                    };
                    ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_OS_BuySheet, "sbs_sp_OSBUY_UploadExcel", wkParm);
                    if (ResultModel.r == 1)
                        ResultModel = JsonConvert.DeserializeObject<SP_Return>(ResultModel.JsonData);
                }
                catch (Exception ex)
                {
                    //throw ex;
                    ResultModel.r = 0;
                    ResultModel.msg = ex.ToString();
                }
                finally
                {
                    File.Delete(pm_Path);
                }
            }

            if (ResultModel.r == 1)
            {
                wkMsg = ResultModel.msg + " of " + rowCount.ToString() + " rows was imported completely.";
                ResultModel = UpdateForImported(cluster);
            }

            if (ResultModel.r == 1)
            {
                ResultModel.msg = wkMsg;

                switch (cluster)
                {
                    case "OS":
                        GlobalVariables.MySession.List_Buy_OSSheet = null;
                        break;
                    case "Domestic":
                        GlobalVariables.MySession.List_Buy_DomesticSheet = null;
                        break;
                    case "FTB":
                        GlobalVariables.MySession.List_Buy_FTBSheet = null;
                        break;
                    default:
                        break;
                }
            }

            return ResultModel;
        }


        public SP_Return UpdateForImported(string pm_cluster)
        {
            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pm_cluster", SqlDbType = SqlDbType.NVarChar, Value = pm_cluster},
                new SqlParameter() {ParameterName = "@pm_usr", SqlDbType = SqlDbType.NVarChar, Value = GlobalVariables.MySession.Account}
            };

            return shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_OS_BuySheet, "sbs_sp_OSBUY_UpdateBulkUploads1", wkParm, true);
        }


        public List<BuySheet_Excel> ExportSheet(string cluster, string selected)
        {
            List<BuySheet_Excel> theExcel = new List<BuySheet_Excel>();
            List<int> viewIDList = (selected.Replace("[", "").Replace("]", "").Split(',').ToList()).Select(int.Parse).ToList();
            List<Buy> buyList = new List<Buy>();
            List<BuySplit> splitList = new List<BuySplit>();

            buyList = GetBuySheet(cluster).Where(b => viewIDList.Contains(b.viewID)).ToList();
            switch (cluster)
            {
                case "OS":
                    splitList = GlobalVariables.MySession.List_Buy_OSSplits.Where(b => viewIDList.Contains(b.viewID)).ToList();
                    break;
                case "Domestic":
                    splitList = GlobalVariables.MySession.List_Buy_DomesticSplits.Where(b => viewIDList.Contains(b.viewID)).ToList();
                    break;
                case "FTB":
                    splitList = GlobalVariables.MySession.List_Buy_FTBSplits.Where(b => viewIDList.Contains(b.viewID)).ToList();
                    break;
                default:
                    break;
            }

            var find = from data in buyList
                       select new BuySheet_Excel()
                       {
                           date_created = data.date_created,
                           viewID = data.viewID,
                           ItemID = data.ItemID,
                           item_desc = data.item_desc,
                           extended_desc = data.extended_desc,
                           buy_to_loc = data.buy_to_loc,
                           PPAP = data.PPAP,
                           package = data.package,
                           new_quantity = data.new_quantity,
                           required_date = data.required_date,
                           //ship_out_date = data.ship_out_date,
                           PO_no = data.PO_no,
                           program = data.program,
                           buyer_name = data.buyer_name,
                           TruePrimarySupplier = data.TruePrimarySupplier,
                           LeadTime = data.LeadTime,
                           vendor_approve = data.vendor_approve,
                           unit_price = (data.theQuotes == null || data.theQuotes.FirstOrDefault(w => w.z_status == "A") == null ? "" : data.theQuotes.FirstOrDefault(w => w.z_status == "A").unit_price.ToString()),
                           new_price_usd = (data.theQuotes == null || data.theQuotes.FirstOrDefault(w => w.z_status == "A") == null ? "" : data.theQuotes.FirstOrDefault(w => w.z_status == "A").price_usd.ToString())
                       };
            theExcel = find.ToList();

            foreach (var item in theExcel)
                item.BuyRelease = splitList.Where(e => e.viewID == item.viewID).OrderByDescending(s => s.release_date).ToList();

            return theExcel;
        }


        public SP_Return SheetToExcel(string cluster, string pm_Path, List<BuySheet_Excel> theExcel)
        {
            SP_Return ExportModel = new SP_Return();
            ExportModel.r = 1;
            ExportModel.msg = "";
            int i, b;
            decimal decimalValue;
            System.IO.FileInfo wkFile = new System.IO.FileInfo(pm_Path);

            try
            {
                if (wkFile.Exists)
                    wkFile.Delete();

                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                using (ExcelPackage pck = new ExcelPackage(wkFile))
                {
                    ExcelWorksheet ws = pck.Workbook.Worksheets.Add(cluster);
                    ws.Cells["A1"].LoadFromCollection(theExcel, true);
                    //ws.Cells["M1"].LoadFromArrays(buyList.Select(r => r.BuyRelease.ToArray()));

                    ws.View.FreezePanes(2, 3);
                    //ws.Cells[ws.Dimension.Address].AutoFilter = true;
                    ws.Cells[ws.Dimension.Address].AutoFitColumns();
                    ws.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    for (i = 2; i <= ws.Dimension.End.Row; i++)
                        ws.Row(i).Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    ws.Cells[1, 19].Value = ""; //Header of BuyRelease
                    using (var range = ws.Cells[1, 1, 1, 18])
                    {
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(Color.LightSteelBlue);
                    }

                    ws.Column(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Column(3).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Column(3).Style.Numberformat.Format = "yyyy-MM-dd";
                    ws.Column(4).Width = 50;
                    ws.Column(4).Style.WrapText = true;
                    ws.Column(5).Width = 60;
                    ws.Column(5).Style.WrapText = true;
                    ws.Column(6).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Column(7).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Column(8).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Column(9).Style.Numberformat.Format = "yyyy-MM-dd";
                    ws.Column(9).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Column(10).Style.Numberformat.Format = "#,##0";
                    ws.Column(11).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Column(12).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Column(12).Width = 30;
                    ws.Column(13).Width = 20;
                    ws.Column(14).Width = 50;
                    ws.Column(14).Style.WrapText = true;
                    ws.Column(15).Width = 15;
                    ws.Column(15).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Column(17).Style.Numberformat.Format = "#,##0.000";
                    ws.Column(17).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Column(18).Style.Numberformat.Format = "#,##0.000";
                    ws.Column(18).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    for (i = 2; i <= ws.Dimension.End.Row; i++)
                    {
                        if (!string.IsNullOrEmpty(ws.Cells[i, 17].Value.ToString()) && Decimal.TryParse(ws.Cells[i, 17].Value.ToString(), out decimalValue))
                            ws.Cells[i, 17].Value = decimalValue;
                        if (!string.IsNullOrEmpty(ws.Cells[i, 18].Value.ToString()) && Decimal.TryParse(ws.Cells[i, 18].Value.ToString(), out decimalValue))
                            ws.Cells[i, 18].Value = decimalValue;
                    }
                    ws.Cells[ws.Dimension.Address].AutoFilter = true;
                    //ws.Cells[ws.Dimension.Address].AutoFitColumns();

                    //Splits
                    for (b = theExcel.Count; b > 0; b--)
                    {
                        if (theExcel[b - 1].BuyRelease.Count > 0)
                        {
                            foreach (var item in theExcel[b - 1].BuyRelease)
                            {
                                ws.InsertRow(b + 2, 1);
                                ws.Cells[b + 2, 9].Value = item.release_date;
                                ws.Cells[b + 2, 10].Value = item.quantity;
                            }
                            ws.InsertRow(b + 2, 1);
                            ws.Cells[b + 2, 9].Value = "Release Date";
                            ws.Cells[b + 2, 10].Value = "Release Quantity";
                        }
                        ws.Cells[b + 1, ws.Dimension.End.Column].Value = "";

                        if (b % 2 == 0)
                        {
                            using (var range = ws.Cells[b + 1, 1, b + 1 + theExcel[b - 1].BuyRelease.Count + (theExcel[b - 1].BuyRelease.Count == 0 ? 0 : 1), ws.Dimension.End.Column - 1])
                            {
                                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                                range.Style.Fill.BackgroundColor.SetColor(Color.AliceBlue);
                            }
                        }
                    }

                    //pck.SaveAs(f);
                    pck.Save();
                }
            }
            catch (Exception ex)
            {
                ExportModel.r = 0;
                ExportModel.msg = ex.ToString();
            }

            return ExportModel;
        }


        public MemoryStream GetExampleSheet()
        {
            string version = "20220818";
            var stream = new MemoryStream();

            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            using (var package = new ExcelPackage(stream))
            {
                var workSheet = package.Workbook.Worksheets.Add(version);
                workSheet.Column(2).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Column(3).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Column(4).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                workSheet.Column(4).Style.Numberformat.Format = "#,##0";
                workSheet.Column(5).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Column(5).Style.Numberformat.Format = "yyyy-MM-dd";
                workSheet.Column(6).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                workSheet.Column(6).Style.Numberformat.Format = "#,##0";
                workSheet.Column(7).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                //header
                workSheet.Cells[1, 1].Value = "Item ID";
                workSheet.Cells[1, 2].Value = "Buy to Loc";
                workSheet.Cells[1, 3].Value = "Sourcing (Y/N)";
                workSheet.Cells[1, 4].Value = "New Quantity";
                workSheet.Cells[1, 5].Value = "Required Ship Out Date";
                workSheet.Cells[1, 6].Value = "Preload Quantity";
                workSheet.Cells[1, 7].Value = "Preload Loc";
                workSheet.Cells[1, 8].Value = "Package";

                //data row 1
                workSheet.Cells[2, 1].Value = "example item 1";
                workSheet.Cells[2, 2].Value = "1";
                workSheet.Cells[2, 3].Value = "";
                workSheet.Cells[2, 4].Value = 2500;
                workSheet.Cells[2, 5].Value = "2021-12-31";
                workSheet.Cells[2, 6].Value = 0;
                workSheet.Cells[2, 7].Value = "";
                workSheet.Cells[2, 8].Value = "";

                //data row 2
                workSheet.Cells[3, 1].Value = "example item 2";
                workSheet.Cells[3, 2].Value = "1";
                workSheet.Cells[3, 3].Value = "Y";
                workSheet.Cells[3, 4].Value = 10000;
                workSheet.Cells[3, 5].Value = "2022-01-01";
                workSheet.Cells[3, 6].Value = 0;
                workSheet.Cells[3, 7].Value = "";
                workSheet.Cells[3, 8].Value = "";

                workSheet.Cells[workSheet.Dimension.Address].AutoFitColumns();
                workSheet.View.FreezePanes(2, 1);
                package.Save();
            }
            stream.Position = 0;

            return stream;
        }


        public SP_Return DeleteOneBuy(string cluster, string entryID)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };

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

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@entryId", SqlDbType = SqlDbType.NVarChar, Value = entryID}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_OS_BuySheet, "sbs_sp_OSBUY_DeleteBuys", wkParm, true);

            return ResultModel;
        }


        public SP_Return UpdateOneBuy(Buy buyModel)
        {
            SqlConnection OScon = new SqlConnection(GlobalConfig.MyConfig.Connection_OS_BuySheet.ConnectionString);
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            int tryInt = 0;
            string wkPO = "";
            string wkVendor = "";
            string QuoteJson = "";
            string FTBJson = "";
            if (!string.IsNullOrWhiteSpace(buyModel.PO_no))
                wkPO = buyModel.PO_no.ToUpper().Replace("TBD", "").Replace(" ", "");

            List<Location> LocationList = shareGet.GetList_Location();
            if (buyModel.po_to_loc > 0 && buyModel.po_to_loc.ToString() != buyModel.buy_to_loc)
                buyModel.company_id = LocationList.SingleOrDefault(l => l.location_id == buyModel.po_to_loc.ToString()).company_id;
            else
                buyModel.company_id = LocationList.SingleOrDefault(l => l.location_id == buyModel.buy_to_loc).company_id;

            List<Vendor> SupplierList = GetList_Supplier(buyModel.ItemID, buyModel.po_to_loc.ToString());
            SupplierList.Insert(0, new Vendor { vendor_id = 0, vendor_name = "" });
            buyModel.vendor_approve = SupplierList.FirstOrDefault(s => s.vendor_id == buyModel.vendor_id).vendor_name ?? "";

            if ((buyModel.new_quantity == null ? 0 : buyModel.new_quantity) <= 0)
                ResultModel.msg = "New Quantity must be greater than zero.";

            if (!string.IsNullOrWhiteSpace(wkPO) &&
                (!int.TryParse(wkPO, out tryInt) || tryInt < 4000000 || tryInt > 5000000))
                ResultModel.msg = "Error PO number.";

            if (!string.IsNullOrWhiteSpace(buyModel.preload_loc) && Convert.ToInt32(buyModel.preload_loc) > 0 && (!Int32.TryParse(buyModel.preload_quantity, out tryInt) || tryInt <= 0))
                ResultModel.msg = "Preload quantity must be greater than zero if preload location is set.";

            if (!string.IsNullOrWhiteSpace(buyModel.preload_loc) && buyModel.po_to_loc == Decimal.Parse(buyModel.preload_loc))
                ResultModel.msg = "Preload location shouldn't be the same to PO location.";

            if (!string.IsNullOrWhiteSpace(ResultModel.msg))
            {
                ResultModel.r = 0;
                return ResultModel;
            }

            if (buyModel.cluster == "FTB")
            {
                if ((buyModel.theFTB.customer_provide_sample ?? false) && string.IsNullOrWhiteSpace(buyModel.theFTB.sample_tracking_number))
                    ResultModel.msg = "Tracking number is required if customer provides sample.";

                if (!string.IsNullOrWhiteSpace(ResultModel.msg))
                {
                    ResultModel.r = 0;
                    return ResultModel;
                }
            }

            //double check
            switch (buyModel.cluster)
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

            if (!Int32.TryParse(buyModel.PO_order_quantity_1, out tryInt) || buyModel.new_quantity > tryInt)
                buyModel.PO_order_quantity_1 = buyModel.new_quantity.ToString();

            if (buyModel.theQuotes != null)
            {
                buyModel.theQuotes.Remove(buyModel.theQuotes.SingleOrDefault(r => r.quote_seq == 0));
                foreach (var oneQuote in buyModel.theQuotes)
                {
                    if (oneQuote.vendor_id > 0 && !string.IsNullOrWhiteSpace(oneQuote.vendor_name))
                    {
                        wkVendor = oneQuote.vendor_name;
                        oneQuote.vendor_terms = wkVendor.Substring(wkVendor.IndexOf(" [", 0), wkVendor.IndexOf("]", 0) - wkVendor.IndexOf(" [", 0) + 1);
                        oneQuote.vendor_name = oneQuote.vendor_name.Replace(oneQuote.vendor_terms, "");
                        oneQuote.vendor_terms = oneQuote.vendor_terms.Replace(" [", "").Replace("]", "");
                    }
                }

                QuoteJson = JsonConvert.SerializeObject(buyModel.theQuotes);
            }

            if (buyModel.theFTB != null)
            {
                if (buyModel.theFTB.step == 0)
                    buyModel.theFTB.step = 1;
                if (buyModel.theFTB.step == 1 && !string.IsNullOrWhiteSpace(buyModel.theFTB.sourcing_review))
                    buyModel.theFTB.step = 2;
                buyModel.theFTB.theDocuments = null;
                buyModel.theFTB.theStepLogs = null;
                FTBJson = JsonConvert.SerializeObject(buyModel.theFTB);
            }

            SqlCommand cmd = new SqlCommand("sbs_sp_OSBUY_UpdateBuys", OScon) { CommandType = CommandType.StoredProcedure, CommandTimeout = 0 };
            try
            {
                cmd.Parameters.AddWithValue("@entryId", (object)buyModel.entryID ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@vendor_approve", (object)buyModel.vendor_approve ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@PO_no", String.IsNullOrEmpty(buyModel.PO_no) ? "" : buyModel.PO_no.Replace("-", "").Replace("\t", " ").ToUpper());
                cmd.Parameters.AddWithValue("@new_quantity", buyModel.new_quantity);
                cmd.Parameters.AddWithValue("@buy_to_loc", (object)buyModel.buy_to_loc ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@preload_quantity", (object)buyModel.preload_quantity ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@preload_loc", String.IsNullOrEmpty(buyModel.preload_loc) ? "" : buyModel.preload_loc);
                cmd.Parameters.AddWithValue("@preload_so", String.IsNullOrEmpty(buyModel.preload_so) ? "" : buyModel.preload_so);
                cmd.Parameters.AddWithValue("@package", (object)buyModel.package ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@PO_order_quantity_1", (object)buyModel.PO_order_quantity_1 ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@total_value", (object)buyModel.total_value ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@total_weight", (object)buyModel.total_weight ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@vendor_id", (object)buyModel.vendor_id ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@company_id", String.IsNullOrEmpty(buyModel.company_id) ? "" : buyModel.company_id);
                cmd.Parameters.AddWithValue("@required_date", buyModel.required_date);
                cmd.Parameters.AddWithValue("@po_to_loc", (object)buyModel.po_to_loc ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@expected_date", buyModel.expected_date);
                cmd.Parameters.AddWithValue("@urgent", buyModel.urgent);
                cmd.Parameters.AddWithValue("@pm_usr", GlobalVariables.MySession.Account);

                cmd.Parameters.AddWithValue("@pm_quotes", QuoteJson);
                if (buyModel.cluster == "FTB")
                    cmd.Parameters.AddWithValue("@pm_ftb", FTBJson);

                OScon.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                ResultModel.r = 0;
                ResultModel.msg = ex.ToString();
            }
            finally
            {
                if (OScon.State != ConnectionState.Closed) { OScon.Close(); }
                cmd.Dispose();
            }

            return ResultModel;
        }

        /*
        public SP_Return CreateOneBuy(Buy buyModel)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            string newEntryID = Convert.ToString(Guid.NewGuid());
            int tryInt = 0;
            string wkVendor = "";
            string QuoteJson = "";
            string FTBJson = "";

            if (buyModel.required_date == null)
                ResultModel.msg = "The Required Date cannot be null.";

            if ((buyModel.new_quantity == null ? 0 : buyModel.new_quantity) <= 0)
                ResultModel.msg = "The New Quantity must be greater than zero.";

            if (buyModel.po_to_loc != decimal.Parse(buyModel.buy_to_loc))
            {
                if (buyModel.po_to_loc == 0)
                    buyModel.po_to_loc = Decimal.Parse(buyModel.buy_to_loc);

                List<Location> LocationList = shareGet.GetList_Location();
                buyModel.company_id = LocationList.SingleOrDefault(l => l.location_id == buyModel.po_to_loc.ToString()).company_id;
            }

            if (!string.IsNullOrWhiteSpace(buyModel.preload_loc) && buyModel.po_to_loc == Decimal.Parse(buyModel.preload_loc))
                ResultModel.msg = "Preload location shouldn't be the same to PO location.";

            if (buyModel.vendor_id != null)
            {
                List<Vendor> SupplierList = GetList_Supplier(buyModel.ItemID, buyModel.po_to_loc.ToString());
                if (SupplierList.Count() <= 0)
                    ResultModel.msg = "Can't find any supplier for this part number and location id.";
                else
                {
                    SupplierList.Insert(0, new Vendor { vendor_id = 0, vendor_name = "" });
                    buyModel.vendor_approve = SupplierList.FirstOrDefault(s => s.vendor_id == buyModel.vendor_id).vendor_name ?? "";
                }
            }

            if (buyModel.cluster == "FTB")
            {
                if (string.IsNullOrWhiteSpace(buyModel.total_value) || buyModel.total_value == "NaN" || string.IsNullOrWhiteSpace(buyModel.total_weight) || buyModel.total_weight == "NaN")
                    ResultModel.msg = "Please build correct part info in P21.";

                if (string.IsNullOrWhiteSpace(buyModel.theFTB.sourced_by))
                    ResultModel.msg = "Sourced By account is required.";

                if (buyModel.theFTB.pod_no <= 0)
                    ResultModel.msg = "POD number is required.";

                if (string.IsNullOrWhiteSpace(buyModel.theFTB.buyer))
                    ResultModel.msg = "Buyer account is required.";

                if (buyModel.primary_supplier_id == 0 || buyModel.vendor_id == 0 || (buyModel.theQuotes != null && buyModel.theQuotes.FindAll(f => f.z_status == "A").Count() != 1))
                    ResultModel.msg = "There must be at least one approved supplier.";
            }

            if (!string.IsNullOrWhiteSpace(ResultModel.msg))
            {
                ResultModel.r = 0;
                return ResultModel;
            }

            //double check
            switch (buyModel.cluster)
            {
                case "OS":
                    ResultModel = shareCommon.checkAuthorized(18);
                    break;
                case "Domestic":
                    ResultModel = shareCommon.checkAuthorized(30);
                    break;
                case "FTB":
                    ResultModel = shareCommon.checkAuthorized(106);
                    break;
                default:
                    ResultModel.r = 0;
                    ResultModel.msg = "Unavailable Cluster.";
                    break;
            }
            if (ResultModel.r == 0)
                return ResultModel;

            if (!Int32.TryParse(buyModel.PO_order_quantity_1, out tryInt) || buyModel.new_quantity > tryInt)
                buyModel.PO_order_quantity_1 = buyModel.new_quantity.ToString();

            if (buyModel.expected_date == null)
                buyModel.expected_date = buyModel.required_date;

            if (buyModel.theQuotes != null)
            {
                buyModel.theQuotes.Remove(buyModel.theQuotes.SingleOrDefault(r => r.quote_seq == 0));
                foreach (var oneQuote in buyModel.theQuotes)
                {
                    if (oneQuote.vendor_id > 0 && !string.IsNullOrWhiteSpace(oneQuote.vendor_name))
                    {
                        wkVendor = oneQuote.vendor_name;
                        oneQuote.vendor_terms = wkVendor.Substring(wkVendor.IndexOf(" [", 0), wkVendor.IndexOf("]", 0) - wkVendor.IndexOf(" [", 0) + 1);
                        oneQuote.vendor_name = oneQuote.vendor_name.Replace(oneQuote.vendor_terms, "");
                        oneQuote.vendor_terms = oneQuote.vendor_terms.Replace(" [", "").Replace("]", "");
                    }
                }

                QuoteJson = JsonConvert.SerializeObject(buyModel.theQuotes);
            }

            if (buyModel.theFTB != null)
            {
                buyModel.theFTB.theDocuments = null;
                FTBJson = JsonConvert.SerializeObject(buyModel.theFTB);
            }

            SqlConnection OScon = new SqlConnection(GlobalConfig.MyConfig.Connection_OS_BuySheet.ConnectionString);
            SqlCommand cmd = new SqlCommand("sbs_sp_OSBUY_SaveBuys", OScon) { CommandType = CommandType.StoredProcedure, CommandTimeout = 0 };
            try
            {
                cmd.Parameters.AddWithValue("@entryId", newEntryID);
                cmd.Parameters.AddWithValue("@planner", GlobalVariables.MySession.FirstName + " " + GlobalVariables.MySession.LastName);
                cmd.Parameters.AddWithValue("@ItemID", buyModel.ItemID);
                cmd.Parameters.AddWithValue("@item_desc", String.IsNullOrEmpty(buyModel.item_desc) ? "" : buyModel.item_desc);
                cmd.Parameters.AddWithValue("@extended_desc", String.IsNullOrEmpty(buyModel.extended_desc) ? "" : buyModel.extended_desc);
                cmd.Parameters.AddWithValue("@part_assignment", String.IsNullOrEmpty(buyModel.part_assignment) ? "" : buyModel.part_assignment);
                cmd.Parameters.AddWithValue("@purchase_class", String.IsNullOrEmpty(buyModel.purchase_class) ? "" : buyModel.purchase_class);
                cmd.Parameters.AddWithValue("@PPAP", String.IsNullOrEmpty(buyModel.PPAP) ? "" : buyModel.PPAP);
                cmd.Parameters.AddWithValue("@class_id2", String.IsNullOrEmpty(buyModel.class_id2) ? "" : buyModel.class_id2);
                cmd.Parameters.AddWithValue("@standard_cost", String.IsNullOrEmpty(buyModel.standard_cost) ? "" : buyModel.standard_cost);
                cmd.Parameters.AddWithValue("@supplier_standard_cost", String.IsNullOrEmpty(buyModel.supplier_standard_cost) ? "" : buyModel.supplier_standard_cost);
                cmd.Parameters.AddWithValue("@loc_supplier_standard_cost", String.IsNullOrEmpty(buyModel.loc_supplier_standard_cost) ? "" : buyModel.loc_supplier_standard_cost);
                cmd.Parameters.AddWithValue("@unit_weight", String.IsNullOrEmpty(buyModel.unit_weight) ? "" : buyModel.unit_weight);
                cmd.Parameters.AddWithValue("@MOQSupplierLoc1", String.IsNullOrEmpty(buyModel.MOQSupplierLoc1) ? "" : buyModel.MOQSupplierLoc1);
                cmd.Parameters.AddWithValue("@LeadTime", String.IsNullOrEmpty(buyModel.LeadTime) ? "" : buyModel.LeadTime);
                cmd.Parameters.AddWithValue("@TruePrimarySupplier", String.IsNullOrEmpty(buyModel.TruePrimarySupplier) ? "" : buyModel.TruePrimarySupplier);
                cmd.Parameters.AddWithValue("@loc_primary_supplier_name", String.IsNullOrEmpty(buyModel.loc_primary_supplier_name) ? "" : buyModel.loc_primary_supplier_name);
                cmd.Parameters.AddWithValue("@last_vendor", String.IsNullOrEmpty(buyModel.last_vendor) ? "" : buyModel.last_vendor);
                cmd.Parameters.AddWithValue("@last_PO_date", buyModel.last_PO_date);
                cmd.Parameters.AddWithValue("@last_price", String.IsNullOrEmpty(buyModel.last_price) ? "" : buyModel.last_price);
                cmd.Parameters.AddWithValue("@package", String.IsNullOrEmpty(buyModel.package) ? "" : buyModel.package);
                cmd.Parameters.AddWithValue("@previous_quantity", String.IsNullOrEmpty(buyModel.previous_quantity) ? "" : buyModel.previous_quantity);
                cmd.Parameters.AddWithValue("@PO_order_quantity_1", String.IsNullOrEmpty(buyModel.PO_order_quantity_1) ? "" : buyModel.PO_order_quantity_1);
                cmd.Parameters.AddWithValue("@vendor_approve", String.IsNullOrEmpty(buyModel.vendor_approve) ? "" : buyModel.vendor_approve);
                cmd.Parameters.AddWithValue("@PO_no", String.IsNullOrEmpty(buyModel.PO_no) ? "" : buyModel.PO_no.ToUpper());
                cmd.Parameters.AddWithValue("@preload_PO_SO", String.IsNullOrEmpty(buyModel.preload_PO_SO) ? "" : buyModel.preload_PO_SO);
                cmd.Parameters.AddWithValue("@new_quantity", buyModel.new_quantity.ToString());
                cmd.Parameters.AddWithValue("@buy_to_loc", String.IsNullOrEmpty(buyModel.buy_to_loc) ? "" : buyModel.buy_to_loc);
                cmd.Parameters.AddWithValue("@preload_quantity", String.IsNullOrEmpty(buyModel.preload_quantity) ? "" : buyModel.preload_quantity);
                cmd.Parameters.AddWithValue("@preload_loc", String.IsNullOrEmpty(buyModel.preload_loc) ? "" : buyModel.preload_loc);
                cmd.Parameters.AddWithValue("@sourcing", String.IsNullOrEmpty(buyModel.sourcing) ? "" : buyModel.sourcing);
                cmd.Parameters.AddWithValue("@total_value", String.IsNullOrEmpty(buyModel.total_value) ? "" : buyModel.total_value);
                cmd.Parameters.AddWithValue("@total_weight", String.IsNullOrEmpty(buyModel.total_weight) ? "" : buyModel.total_weight);
                cmd.Parameters.AddWithValue("@vendor_id", (object)buyModel.vendor_id ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@cluster", String.IsNullOrEmpty(buyModel.cluster) ? "" : buyModel.cluster);
                cmd.Parameters.AddWithValue("@company_id", String.IsNullOrEmpty(buyModel.company_id) ? "" : buyModel.company_id);
                cmd.Parameters.AddWithValue("@required_date", buyModel.required_date);
                cmd.Parameters.AddWithValue("@po_to_loc", (object)buyModel.po_to_loc ?? buyModel.buy_to_loc);
                cmd.Parameters.AddWithValue("@expected_date", buyModel.expected_date);
                cmd.Parameters.AddWithValue("@primary_supplier_id", (object)buyModel.primary_supplier_id ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@urgent", buyModel.urgent);
                cmd.Parameters.AddWithValue("@creator", GlobalVariables.MySession.Account);
                cmd.Parameters.AddWithValue("@sourcing_in", String.IsNullOrEmpty(buyModel.sourcing_in) ? "" : buyModel.sourcing_in);
                cmd.Parameters.AddWithValue("@sourcing_vn", String.IsNullOrEmpty(buyModel.sourcing_vn) ? "" : buyModel.sourcing_vn);

                cmd.Parameters.AddWithValue("@pm_quotes", QuoteJson);
                cmd.Parameters.AddWithValue("@pm_ftb", FTBJson);

                OScon.Open();
                cmd.ExecuteNonQuery();

                if (!String.IsNullOrEmpty(buyModel.new_note))
                {
                    BuyNote noteModel = new BuyNote();
                    noteModel.entryID = newEntryID;
                    noteModel.notes = buyModel.new_note;
                    ResultModel = pgmBuyNote.CreateNewNote(buyModel.cluster, noteModel, "BuyCreate");
                }

                if (ResultModel.r == 1)
                    ResultModel.msg = newEntryID;
            }
            catch (Exception ex)
            {
                ResultModel.r = 0;
                ResultModel.msg = ex.ToString();
            }
            finally
            {
                if (OScon.State != ConnectionState.Closed) { OScon.Close(); }
                cmd.Dispose();
            }

            return ResultModel;
        }
        */
        public SP_Return CreateOneBuy2(Buy buyModel)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            string newEntryID = Convert.ToString(Guid.NewGuid());
            int tryInt = 0;
            string wkVendor = "";
            string QuoteJson = "";
            string FTBJson = "";

            if (buyModel.required_date == null)
                ResultModel.msg = "The Required Date cannot be null.";

            if ((buyModel.new_quantity == null ? 0 : buyModel.new_quantity) <= 0)
                ResultModel.msg = "The New Quantity must be greater than zero.";

            if (buyModel.po_to_loc != decimal.Parse(buyModel.buy_to_loc))
            {
                if (buyModel.po_to_loc == 0)
                    buyModel.po_to_loc = Decimal.Parse(buyModel.buy_to_loc);

                List<Location> LocationList = shareGet.GetList_Location();
                buyModel.company_id = LocationList.SingleOrDefault(l => l.location_id == buyModel.po_to_loc.ToString()).company_id;
            }

            if (!string.IsNullOrWhiteSpace(buyModel.preload_loc) && Convert.ToInt32(buyModel.preload_loc) > 0 && (!Int32.TryParse(buyModel.preload_quantity,out tryInt) || tryInt <= 0))
                ResultModel.msg = "Preload quantity must be greater than zero if preload location is set.";

            if (!string.IsNullOrWhiteSpace(buyModel.preload_loc) && buyModel.po_to_loc == Decimal.Parse(buyModel.preload_loc))
                ResultModel.msg = "Preload location shouldn't be the same to PO location.";

            if (buyModel.vendor_id != null)
            {
                List<Vendor> SupplierList = GetList_Supplier(buyModel.ItemID, buyModel.po_to_loc.ToString());
                if (SupplierList.Count() <= 0)
                    ResultModel.msg = "Can't find any supplier for this part number and location id.";
                else
                {
                    SupplierList.Insert(0, new Vendor { vendor_id = 0, vendor_name = "" });
                    buyModel.vendor_approve = SupplierList.FirstOrDefault(s => s.vendor_id == buyModel.vendor_id).vendor_name ?? "";
                }
            }

            if (buyModel.cluster == "FTB")
            {
                if (string.IsNullOrWhiteSpace(buyModel.total_value) || buyModel.total_value == "NaN" || string.IsNullOrWhiteSpace(buyModel.total_weight) || buyModel.total_weight == "NaN")
                    ResultModel.msg = "Please build correct part info in P21.";

                if (string.IsNullOrWhiteSpace(buyModel.theFTB.sourced_by))
                    ResultModel.msg = "Sourced By account is required.";

                if (buyModel.theFTB.pod_no <= 0)
                    ResultModel.msg = "POD number is required.";

                if (string.IsNullOrWhiteSpace(buyModel.theFTB.buyer))
                    ResultModel.msg = "Buyer account is required.";

                if (!buyModel.theFTB.customer_provide_sample.HasValue)
                    ResultModel.msg = "Customer Provides Sample is required.";

                if ((buyModel.theFTB.customer_provide_sample ?? false) && string.IsNullOrWhiteSpace(buyModel.theFTB.sample_tracking_number))
                    ResultModel.msg = "Tracking number is required if customer provides sample.";

                if (buyModel.primary_supplier_id == 0 || buyModel.vendor_id == 0 || (buyModel.theQuotes != null && buyModel.theQuotes.FindAll(f => f.z_status == "A").Count() != 1))
                    ResultModel.msg = "There must be at least one approved supplier.";

                ResultModel.msg = CheckBeforeSave(buyModel).msg;
            }

            if (!string.IsNullOrWhiteSpace(ResultModel.msg))
            {
                ResultModel.r = 0;
                return ResultModel;
            }

            //double check
            switch (buyModel.cluster)
            {
                case "OS":
                    ResultModel = shareCommon.checkAuthorized(18);
                    break;
                case "Domestic":
                    ResultModel = shareCommon.checkAuthorized(30);
                    break;
                case "FTB":
                    ResultModel = shareCommon.checkAuthorized(106);
                    break;
                default:
                    ResultModel.r = 0;
                    ResultModel.msg = "Unavailable Cluster.";
                    break;
            }
            if (ResultModel.r == 0)
                return ResultModel;

            buyModel.entryID = newEntryID;
            buyModel.planner = GlobalVariables.MySession.FirstName + " " + GlobalVariables.MySession.LastName;
            buyModel.creator = GlobalVariables.MySession.Account;

            if (!Int32.TryParse(buyModel.PO_order_quantity_1, out tryInt) || buyModel.new_quantity > tryInt)
                buyModel.PO_order_quantity_1 = buyModel.new_quantity.ToString();

            if (buyModel.expected_date == null)
                buyModel.expected_date = buyModel.required_date;

            if (buyModel.theQuotes != null)
            {
                buyModel.theQuotes.Remove(buyModel.theQuotes.SingleOrDefault(r => r.quote_seq == 0));
                foreach (var oneQuote in buyModel.theQuotes)
                {
                    if (oneQuote.vendor_id > 0 && !string.IsNullOrWhiteSpace(oneQuote.vendor_name))
                    {
                        wkVendor = oneQuote.vendor_name;
                        oneQuote.vendor_terms = wkVendor.Substring(wkVendor.IndexOf(" [", 0), wkVendor.IndexOf("]", 0) - wkVendor.IndexOf(" [", 0) + 1);
                        oneQuote.vendor_name = oneQuote.vendor_name.Replace(oneQuote.vendor_terms, "");
                        oneQuote.vendor_terms = oneQuote.vendor_terms.Replace(" [", "").Replace("]", "");
                    }
                }

                QuoteJson = JsonConvert.SerializeObject(buyModel.theQuotes);
            }

            if (buyModel.theFTB != null)
            {
                buyModel.theFTB.theDocuments = null;
                FTBJson = JsonConvert.SerializeObject(buyModel.theFTB);
            }

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.NVarChar, Value = JsonConvert.SerializeObject(buyModel)},
                new SqlParameter() {ParameterName = "@pm_quotes", SqlDbType = SqlDbType.NVarChar, Value = QuoteJson},
                new SqlParameter() {ParameterName = "@pm_ftb", SqlDbType = SqlDbType.NVarChar, Value = FTBJson}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_OS_BuySheet, "sbs_sp_OSBUY_SaveBuys2", wkParm);
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
                ResultModel = JsonConvert.DeserializeObject<SP_Return>(ResultModel.JsonData);

            if (ResultModel.r == 1 && !String.IsNullOrEmpty(buyModel.new_note))
            {
                SP_Return NoteResult = new SP_Return() { r = 1, msg = "", JsonData = "" };
                BuyNote noteModel = new BuyNote();
                noteModel.entryID = newEntryID;
                noteModel.notes = buyModel.new_note;
                NoteResult = pgmBuyNote.CreateNewNote(buyModel.cluster, noteModel, "BuyCreate");
                if (NoteResult.r != 1)
                    ResultModel.msg += System.Environment.NewLine + NoteResult.msg;
            }

            return ResultModel;
        }


        public Buy UpdatePOLoc(Buy buyModel)
        {
            List<Location> LocationList = shareGet.GetList_Location();
            buyModel.company_id = LocationList.SingleOrDefault(l => l.location_id == buyModel.po_to_loc.ToString()).company_id;

            buyModel.vendor_id = 0;
            buyModel.vendor_approve = "";

            buyModel.theQuotes.Clear();
            buyModel.theQuotes = null;
            if (buyModel.cluster == "FTB")
            {
                buyModel.theQuotes = new List<BuyQuote>();
                buyModel.theQuotes.Add(new BuyQuote());
            }

            buyModel.preload_loc = "";
            buyModel.preload_po = "";
            buyModel.preload_so = "";
            buyModel.preload_PO_SO = "";
            buyModel.preload_quantity = "";

            return buyModel;
        }


        public SP_Return FTB_GoUpdate(BuyFTB pm_FTB)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };

            //double check
            ResultModel = shareCommon.checkAuthorized(107);
            if (ResultModel.r == 0)
                return ResultModel;

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pm_viewID", SqlDbType = SqlDbType.Int, Value = pm_FTB.viewID},
                new SqlParameter() {ParameterName = "@pm_z_usr", SqlDbType = SqlDbType.VarChar, Value = GlobalVariables.MySession.Account},
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.VarChar, Value = JsonConvert.SerializeObject(pm_FTB)},
                new SqlParameter() {ParameterName = "@isReturn", SqlDbType = SqlDbType.Bit, Value = true}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_OS_BuySheet, "sbs_sp_OSBUY_UpdateFTB", wkParm);
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
                ResultModel = JsonConvert.DeserializeObject<SP_Return>(ResultModel.JsonData);

            return ResultModel;
        }


        public SP_Return CheckBeforeSave(Buy thisBuy)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            string theJson = "{'supplier_id':" + thisBuy.primary_supplier_id.ToString() + ", 'item_id':'" + thisBuy.ItemID + "'}";

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.VarChar, Value = theJson.Replace("'", "\"")}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_OS_BuySheet, "sbs_sp_OSBUY_CheckBeforeSave", wkParm);
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
                ResultModel = JsonConvert.DeserializeObject<SP_Return>(ResultModel.JsonData);

            return ResultModel;
        }

        #endregion

        #region Sorting

        public void ClearSort(string cluster)
        {
            switch (cluster)
            {
                case "OS":
                    GlobalVariables.MySession.List_Buy_OSOrder = null;
                    break;
                case "Domestic":
                    GlobalVariables.MySession.List_Buy_DomesticOrder = null;
                    break;
                case "FTB":
                    GlobalVariables.MySession.List_Buy_FTBOrder = null;
                    break;
                default:
                    break;
            }
        }


        public void SetOrder(string cluster, string sort_field)
        {
            List<Sorts> theOrder = new List<Sorts>();
            Sorts newSort = new Sorts();
            int i = 0;

            switch (cluster)
            {
                case "OS":
                    if (GlobalVariables.MySession.List_Buy_OSOrder != null)
                        theOrder = GlobalVariables.MySession.List_Buy_OSOrder;
                    break;
                case "Domestic":
                    if (GlobalVariables.MySession.List_Buy_DomesticOrder != null)
                        theOrder = GlobalVariables.MySession.List_Buy_DomesticOrder;
                    break;
                case "FTB":
                    if (GlobalVariables.MySession.List_Buy_FTBOrder != null)
                        theOrder = GlobalVariables.MySession.List_Buy_FTBOrder;
                    break;
                default:
                    break;
            }

            if (theOrder.Exists(o => o.field_name == sort_field))
            {
                newSort = theOrder.FirstOrDefault(o => o.field_name == sort_field);
                newSort.descend = !newSort.descend;
            }
            else
            {
                newSort.field_name = sort_field;
                newSort.descend = false;
            }
            theOrder.RemoveAll(s => s.field_name == sort_field);
            //theOrder.Insert(0, newSort);
            theOrder.Add(newSort);

            i = theOrder.Count();
            foreach (var item in theOrder)
            {
                item.order_no = i;
                i--;
            }

            switch (cluster)
            {
                case "OS":
                    GlobalVariables.MySession.List_Buy_OSOrder = theOrder;
                    break;
                case "Domestic":
                    GlobalVariables.MySession.List_Buy_DomesticOrder = theOrder;
                    break;
                case "FTB":
                    GlobalVariables.MySession.List_Buy_FTBOrder = theOrder;
                    break;
                default:
                    break;
            }
        }


        public List<Buy> SortSheet(List<Buy> theSheet, List<Sorts> theOrder)
        {
            foreach (var item in theOrder)
            {
                if (item.descend)
                    theSheet = theSheet.OrderByDescending(s => s.GetType().GetProperty(item.field_name).GetValue(s)).ToList();
                else
                    theSheet = theSheet.OrderBy(s => s.GetType().GetProperty(item.field_name).GetValue(s)).ToList();
            }

            return theSheet;
        }

        #endregion

    }
}
