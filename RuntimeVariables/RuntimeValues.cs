using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace RuntimeVariables
{
    public class GlobalVariables
    {
        private GlobalVariables()
        {
            /*
            //Config
            Validity = true;
            Connection_SBS = new SqlConnection("Data Source=192.100.100.245;Initial Catalog=SBS_dev;Persist Security Info=True;User ID=AppBot;Password=47Y@p88");
            Connection_OS_BuySheet = new SqlConnection("Data Source=192.100.100.245;Initial Catalog=OS_BuySheet_dev;Persist Security Info=True;User ID=AppBot;Password=47Y@p88");
            SSRSReportUrl = "http://SBS-DB02-21:80/ReportServer/";
            SSRSdomain = "specialtybolt.com";
            SSRSuser = "SBS";
            SSRSpassword = "shipBIGscreen0*";
            */
            //Development
            Context = "";
            isDEV = true;
            isDebug = false;
            ProjectName = "";

            //SP_Return
            List_Rates_Currency = null;

            //Accounting
            new_P21_ID = null;
            theLocalInformation = null;
            List_AccountsReceivable = null;
            List_AccountsPayable = null;
            List_AccountsReceivable_Filters = null;
            List_AccountsPayable_Filters = null;
            List_AccountingAttachment = null;

            //Admin
            List_Account = null;
            List_Menu = null;
            List_Report = null;
            List_Permission = null;
            List_ReportPermission = null;
            List_Group = null;
            
            //IT
            List_CodeStore = null;
            List_InventoryBin = null;
            List_InventoryItem = null;

            //Login
            Account = "";
            FirstName = "";
            LastName = "";
            Email = "";
            LoginMenuList = null;
            LoginMenuString = "";
            LoginReportList = null;

            //OSBuy
            List_Location = null;
            List_Country = null;
            List_Buy_OSSheet = null;
            List_Buy_DomesticSheet = null;
            List_Buy_FTBSheet = null;
            List_Buy_OSFilters = null;
            List_Buy_DomesticFilters = null;
            List_Buy_FTBFilters = null;
            List_Buy_OSOrder = null;
            List_Buy_DomesticOrder = null;
            List_Buy_FTBOrder = null;
            List_Buy_OSNotes = null;
            List_Buy_DomesticNotes = null;
            List_Buy_FTBNotes = null;
            List_Buy_OSSplits = null;
            List_Buy_DomesticSplits = null;
            List_Buy_FTBSplits = null;
            List_ArchiveBuy_OSFilters = null;
            List_ArchiveBuy_DomesticFilters = null;
            List_ArchiveBuy_OSNotes = null;
            List_ArchiveBuy_DomesticNotes = null;
            List_ArchiveBuy_OSSplits = null;
            List_ArchiveBuy_DomesticSplits = null;
            TransitTime_Country_Method = null;
            List_TransitTime_Location = null;
            List_Rates_Metal = null;
            //List_Charge_Tooling = null;
            List_Charge_ToolingReceipts = null;
            List_Charge_ToolingFilters = null;
            List_OpenPO_FTB = null;
            
            //Sales
            List_SO_OpenOrder = null;
            List_SO_OpenOrder_Filters = null;
            List_BOM_forecast = null;
            List_BOM_POs = null;
            
            //Transfer
            List_ShippingLocGrp = null;
            PackingList = null;
            CommercialInvoice = null;
            List_TransitTime_DefaultAir = null;
            
            //Taiwan
            List_TW_Employee = null;
            List_TW_DayApply = null;
            List_TW_DayAvailable = null;
            List_TW_DayAproval = null;
            List_TW_CardStamp = null;
            
            //Warehouse
            packing_person = "";
            List_PalletizingRecord2_Line = null;
        }


        public static GlobalVariables MySession
        {
            get
            {
                GlobalVariables session = (GlobalVariables)HttpContext.Current.Session["MySession"];
                if (session == null)
                {
                    session = new GlobalVariables();
                    HttpContext.Current.Session["MySession"] = session;
                }
                return session;
            }
        }


        /*
        //Config
        public bool Validity { get; set; }
        public SqlConnection Connection_SBS { get; set; }
        public SqlConnection Connection_OS_BuySheet { get; set; }
        public string SSRSReportUrl { get; set; }
        public string SSRSdomain { get; set; }
        public string SSRSuser { get; set; }
        public string SSRSpassword { get; set; }
        */
        //Development
        public string Context { get; set; }
        public bool isDEV { get; set; }
        public bool isDebug { get; set; }
        public string ProjectName { get; set; }

        //Shared Code
        public List<CodeStore> List_CodeStore { get; set; }
        public List<Country> List_Country { get; set; }
        public List<Currency> List_Rates_Currency { get; set; }
        public List<Location> List_Location { get; set; }

        //Accounting
        public Local_P21_ID new_P21_ID { get; set; }
        public Local_Information theLocalInformation { get; set; }
        public List<AR_Item> List_AccountsReceivable { get; set; }
        public List<AP_Item> List_AccountsPayable { get; set; }
        public List<Filters> List_AccountsReceivable_Filters { get; set; }
        public List<Filters> List_AccountsPayable_Filters { get; set; }
        public List<AccountingAttachment> List_AccountingAttachment { get; set; }

        //Admin
        public List<AccountItem> List_Account { get; set; }
        public List<MenuItem> List_Menu { get; set; }
        public List<ReportItem> List_Report { get; set; }
        public List<PermissionItem> List_Permission { get; set; }
        public List<ReportPermissionItem> List_ReportPermission { get; set; }
        public List<GroupItem> List_Group { get; set; }
        
        //IT
        public List<Inventory_Bin> List_InventoryBin { get; set; }
        public List<Inventory_Item> List_InventoryItem { get; set; }

        //Login
        public string Account { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public List<Login> LoginMenuList { get; set; }
        public List<LoginTreeItem> LoginTreeList { get; set; }
        public string LoginMenuString { get; set; }
        public List<ReportItem> LoginReportList { get; set; }
        
        //OSBuy
        public List<Buy> List_Buy_OSSheet { get; set; }
        public List<Buy> List_Buy_DomesticSheet { get; set; }
        public List<Buy> List_Buy_FTBSheet { get; set; }
        public List<Filters> List_Buy_OSFilters { get; set; }
        public List<Filters> List_Buy_DomesticFilters { get; set; }
        public List<Filters> List_Buy_FTBFilters { get; set; }
        public List<Sorts> List_Buy_OSOrder { get; set; }
        public List<Sorts> List_Buy_DomesticOrder { get; set; }
        public List<Sorts> List_Buy_FTBOrder { get; set; }
        public List<BuyNote> List_Buy_OSNotes { get; set; }
        public List<BuyNote> List_Buy_DomesticNotes { get; set; }
        public List<BuyNote> List_Buy_FTBNotes { get; set; }
        public List<BuySplit> List_Buy_OSSplits { get; set; }
        public List<BuySplit> List_Buy_DomesticSplits { get; set; }
        public List<BuySplit> List_Buy_FTBSplits { get; set; }
        public List<Filters> List_ArchiveBuy_OSFilters { get; set; }
        public List<Filters> List_ArchiveBuy_DomesticFilters { get; set; }
        public List<Filters> List_ArchiveBuy_FTBFilters { get; set; }
        public List<BuyNote> List_ArchiveBuy_OSNotes { get; set; }
        public List<BuyNote> List_ArchiveBuy_DomesticNotes { get; set; }
        public List<BuyNote> List_ArchiveBuy_FTBNotes { get; set; }
        public List<BuySplit> List_ArchiveBuy_OSSplits { get; set; }
        public List<BuySplit> List_ArchiveBuy_DomesticSplits { get; set; }
        public List<BuySplit> List_ArchiveBuy_FTBSplits { get; set; }
        public List<BuySheet_Rates_Metals> List_Rates_Metal { get; set; }
        //public List<Charge_Tooling> List_Charge_Tooling { get; set; }
        public List<Charge_ToolingReceipts> List_Charge_ToolingReceipts { get; set; }
        public List<Filters> List_Charge_ToolingFilters { get; set; }
        public List<FTB_OpenPO> List_OpenPO_FTB { get; set; }
        
        //Sales
        public List<SO_open_order> List_SO_OpenOrder { get; set; }
        public List<Filters> List_SO_OpenOrder_Filters { get; set; }
        public List<BOM_forecast> List_BOM_forecast { get; set; }
        public List<BOM_PO> List_BOM_POs { get; set; }
        
        //Transfer
        public List<Transfer_ShippingLocGrp> List_ShippingLocGrp { get; set; }
        public Transfer_PackingList PackingList { get; set; }
        public Transfer_CommercialInvoice CommercialInvoice { get; set; }
        public BuySheet_TransitTime_Country_Method TransitTime_Country_Method { get; set; }
        public List<BuySheet_TransitTime_Location> List_TransitTime_Location { get; set; }
        public List<BuySheet_TransitTime_DefaultAir> List_TransitTime_DefaultAir { get; set; }
        
        //Taiwan
        public List<tw_employee> List_TW_Employee { get; set; }
        public List<tw_day_apply> List_TW_DayApply { get; set; }
        public List<tw_day_available> List_TW_DayAvailable { get; set; }
        public List<tw_LeaveApproval> List_TW_DayAproval { get; set; }
        public List<tw_CardStamp> List_TW_CardStamp { get; set; }
        
        //Warehouse
        public string packing_person { get; set; }
        public List<palletizing_record2_line> List_PalletizingRecord2_Line { get; set; }
        
    }
}
