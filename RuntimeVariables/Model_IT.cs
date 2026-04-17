using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RuntimeVariables
{
    public class P21SessionData
    {
        public List<P21Session> sessionList { get; set; }
    }

    public class P21Session
    {
        [DisplayName("No")]
        public int rrn { get; set; }

        public string SessionId { get; set; }

        [DisplayName("User ID")]
        public string UserId { get; set; }

        [DisplayName("Session Start")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy hh:mm}")]
        public DateTime SessionStart { get; set; }

        public int sessionAge { get; set; }

        [DisplayName("Session Age")]
        public string age { get; set; }

        [DisplayName("Last Access")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy hh:mm}")]
        public DateTime LastAccess { get; set; }

        public int idleTime { get; set; }
        [DisplayName("Idle Time")]
        public string idle { get; set; }

        public Boolean deleteFlag { get; set; }

        public Boolean chk { get; set; }
    }


    public class Inventory_Item
    {
        [DisplayName("Loc")]
        public string loc_id { get; set; }

        [DisplayName("Location")]
        public string loc_name { get; set; }

        [DisplayName("UID")]
        public int item_uid { get; set; }

        [DisplayName("Device Name")]
        public string item_id { get; set; }

        [DisplayName("Description")]
        public string item_desc { get; set; }

        //[DisplayName("Item QR")]
        //public byte[] item_qr { get; set; }

        [DisplayName("Service Tag")]
        public string SN { get; set; }

        [DisplayName("Category")]
        public string category { get; set; }

        [DisplayName("P21 Item ID")]
        public string p21_item_id { get; set; }

        public int qty { get; set; }

        [DisplayName("Consumable")]
        public bool isConsumable { get; set; }

        [DisplayName("Note")]
        public string z_memo { get; set; }

        public string dpt { get; set; }

        [DisplayName("Model")]
        public string model { get; set; }

        [DisplayName("Purchase Date")]
        public DateTime purchase_date { get; set; }

        public Inventory_PC thePC { get; set; }

        [DisplayName("Storage and Quantity")]
        public List<Inventory_Amount> amount { get; set; }

        public List<Inventory_History> history { get; set; }
    }
    public class Inventory_PC
    {
        [DisplayName("OS")]
        public string operating_system { get; set; }

        [DisplayName("Office")]
        public string office { get; set; }

        //[DisplayName("Team Viewer")]
        //public string team_viewer { get; set; }
    }
    public class Inventory_Amount
    {
        public string storage_code { get; set; }

        [DisplayName("Lot Hierarchy")]
        public string bin_hierarchy { get; set; }

        [DisplayName("Qty")]
        public int quantity { get; set; }
    }
    public class Inventory_History
    {
        public int item_uid { get; set; }
        public string item_id { get; set; }
        public string behavior { get; set; }
        public string requisition_usr { get; set; }
        public string original_storage_code { get; set; }
        public string storage_code { get; set; }
        public int quantity { get; set; }
        public string z_memo { get; set; }

        [DisplayName("Inventoried By")]
        public string inv_account { get; set; }

        [DisplayName("Inventory Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime? requisition_date { get; set; }

        [DisplayName("History")]
        public string history_text { get; set; }

        public string dpt { get; set; }
    }


    public class Inventory_BinData
    {
        public List<Inventory_Bin> theList { get; set; }

        public List<Inventory_BinTree> theTree { get; set; }
    }
    public class Inventory_Bin
    {
        [DisplayName("No")]
        public int rrn { get; set; }

        [DisplayName("UID")]
        public int bin_uid { get; set; }

        [DisplayName("Parent")]
        public int parent_uid { get; set; }

        [DisplayName("Bin Code")]
        public string bin_cd { get; set; }

        [DisplayName("Bin Name")]
        public string z_memo { get; set; }

        [DisplayName("Storage Code")]
        public string storage_code { get; set; }

        [DisplayName("Bin Hierarchy")]
        public string bin_hierarchy { get; set; }

        [DisplayName("Last Updated Time")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy hh:mm}")]
        public DateTime z_date { get; set; }

        [DisplayName("Last Updated By")]
        public string z_usr { get; set; }

        public string dpt { get; set; }
    }
    public class Inventory_BinTree
    {
        [DisplayName("UID")]
        public int bin_uid { get; set; }

        [DisplayName("Bin Name")]
        public string bin_name { get; set; }

        public List<Inventory_BinTree> child_bins { get; set; }
    }

}