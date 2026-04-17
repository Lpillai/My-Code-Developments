using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuntimeVariables
{

    public class SO_open_order
    {
        [DisplayName("Order No")]
        public string order_no { get; set; }

        [DisplayName("Taker")]
        public string taker { get; set; }

        [DisplayName("Loc")]
        public int location_id { get; set; }

        [DisplayName("PO")]
        public string po_no { get; set; }

        [DisplayName("Ship To")]
        public int ship2_id { get; set; }

        [DisplayName("Ship to Name")]
        public string ship2_name { get; set; }

        [DisplayName("Requested Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime requested_date { get; set; }

        [DisplayName("Line No")]
        public int line_no { get; set; }

        [DisplayName("inv_mast_uid")]
        public int inv_mast_uid { get; set; }

        [DisplayName("Item ID")]
        public string item_id { get; set; }

        [DisplayName("Customer Part Number")]
        public string customer_part_number { get; set; }

        [DisplayName("Expedite Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime expedite_date { get; set; }

        [DisplayName("Qty Available")]
        [DisplayFormat(DataFormatString = "{0:N0}", ConvertEmptyStringToNull = true)]
        public decimal qty_available { get; set; }

        [DisplayName("Release No")]
        public int release_no { get; set; }

        [DisplayName("Release Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}", NullDisplayText = "")]
        public DateTime release_date { get; set; }

        [DisplayName("Qty Release")]
        [DisplayFormat(DataFormatString = "{0:N0}", ConvertEmptyStringToNull = true, NullDisplayText = "")]
        public decimal qty_release { get; set; }

        [DisplayName("Qty Allocated")]
        [DisplayFormat(DataFormatString = "{0:N0}", ConvertEmptyStringToNull = true, NullDisplayText = "")]
        public decimal qty_allocated { get; set; }

        [DisplayName("Qty Picked")]
        [DisplayFormat(DataFormatString = "{0:N0}", ConvertEmptyStringToNull = true, NullDisplayText = "")]
        public decimal qty_picked { get; set; }

        [DisplayName("Qty Invoiced")]
        [DisplayFormat(DataFormatString = "{0:N0}", ConvertEmptyStringToNull = true, NullDisplayText = "")]
        public decimal qty_invoiced { get; set; }

        [DisplayName("Disp")]
        public string disposition { get; set; }

        [DisplayName("Open Qty")]
        [DisplayFormat(DataFormatString = "{0:N0}", ConvertEmptyStringToNull = true, NullDisplayText = "")]
        public decimal open_qty { get; set; }

        [DisplayName("Line Feed")]
        public string line_feed { get; set; }

        [DisplayName("Line Station")]
        public string line_station { get; set; }

        [DisplayName("Approved")]
        public string approved { get; set; }
    }


    public class SO_item_inquiry
    {
        [DisplayName("Part Number")]
        public string item_id { get; set; }

        [DisplayName("Description")]
        public string item_desc { get; set; }

        [DisplayName("Qty on Hand")]
        [DisplayFormat(DataFormatString = "{0:N0}", ConvertEmptyStringToNull = true)]
        public decimal qty_on_hand { get; set; }

        [DisplayName("Qty Allocated")]
        [DisplayFormat(DataFormatString = "{0:N0}", ConvertEmptyStringToNull = true)]
        public decimal qty_allocated { get; set; }

        [DisplayName("Qty Backordered")]
        [DisplayFormat(DataFormatString = "{0:N0}", ConvertEmptyStringToNull = true)]
        public decimal qty_backordered { get; set; }

        [DisplayName("Qty Non-Pickable")]
        [DisplayFormat(DataFormatString = "{0:N0}", ConvertEmptyStringToNull = true)]
        public decimal qty_non_pickable { get; set; }

        [DisplayName("Qty Quarantined")]
        [DisplayFormat(DataFormatString = "{0:N0}", ConvertEmptyStringToNull = true)]
        public decimal qty_quarantined { get; set; }

        [DisplayName("Qty Frozen")]
        [DisplayFormat(DataFormatString = "{0:N0}", ConvertEmptyStringToNull = true)]
        public decimal qty_frozen { get; set; }

        [DisplayName("Qty Available")]
        [DisplayFormat(DataFormatString = "{0:N0}", ConvertEmptyStringToNull = true)]
        public decimal qty_available { get; set; }

        [DisplayName("Qty on Release Schedule")]
        [DisplayFormat(DataFormatString = "{0:N0}", ConvertEmptyStringToNull = true)]
        public decimal qty_on_release_schedule { get; set; }

        [DisplayName("Qty on Future Orders")]
        [DisplayFormat(DataFormatString = "{0:N0}", ConvertEmptyStringToNull = true)]
        public decimal qty_on_future_orders { get; set; }

        [DisplayName("Net Qty Available")]
        [DisplayFormat(DataFormatString = "{0:N0}", ConvertEmptyStringToNull = true)]
        public decimal net_qty_available { get; set; }

        [DisplayName("Qty on Purchase Order")]
        [DisplayFormat(DataFormatString = "{0:N0}", ConvertEmptyStringToNull = true)]
        public decimal qty_on_purchase_order { get; set; }

        [DisplayName("Qty in Vessel")]
        [DisplayFormat(DataFormatString = "{0:N0}", ConvertEmptyStringToNull = true)]
        public decimal qty_in_vessel { get; set; }

        [DisplayName("Qty on Transfer")]
        [DisplayFormat(DataFormatString = "{0:N0}", ConvertEmptyStringToNull = true)]
        public decimal qty_on_transfer { get; set; }

        [DisplayName("Qty on Production Orders")]
        [DisplayFormat(DataFormatString = "{0:N0}", ConvertEmptyStringToNull = true)]
        public decimal qty_on_production_orders { get; set; }

        [DisplayName("Qty on Secondary Process")]
        [DisplayFormat(DataFormatString = "{0:N0}", ConvertEmptyStringToNull = true)]
        public decimal qty_on_secondary_process { get; set; }

        [DisplayName("Qty to Transfer")]
        [DisplayFormat(DataFormatString = "{0:N0}", ConvertEmptyStringToNull = true)]
        public decimal qty_to_transfer { get; set; }

        [DisplayName("Qty for Production")]
        [DisplayFormat(DataFormatString = "{0:N0}", ConvertEmptyStringToNull = true)]
        public decimal qty_for_production { get; set; }

        [DisplayName("Qty for Secondary Process")]
        [DisplayFormat(DataFormatString = "{0:N0}", ConvertEmptyStringToNull = true)]
        public decimal qty_for_secondary_process { get; set; }

        [DisplayName("Qty on Sales Order")]
        [DisplayFormat(DataFormatString = "{0:N0}", ConvertEmptyStringToNull = true)]
        public decimal qty_on_sales_order { get; set; }
    }


    public class BOM_forecast
    {
        public int i { get; set; }

        [DisplayName("Customer Part No")]
        public string customer_part_no { get; set; }

        [DisplayName("Part Number")]
        public string item_id { get; set; }

        [DisplayName("Description")]
        public string item_desc { get; set; }

        [DisplayName("On Hand Qty")]
        [DisplayFormat(DataFormatString = "{0:N0}", ConvertEmptyStringToNull = true)]
        public int qty_on_hand { get; set; }

        [DisplayName("Past Due Qty")]
        [DisplayFormat(DataFormatString = "{0:N0}", ConvertEmptyStringToNull = true)]
        public int qty_past_due { get; set; }

        [DisplayName("Usage")]
        [DisplayFormat(DataFormatString = "{0:N0}", ConvertEmptyStringToNull = true)]
        public int UsagePerWeek { get; set; }
        public int UsageModified { get; set; }

        [DisplayName("Rolling ROP")]
        [DisplayFormat(DataFormatString = "{0:N0}", ConvertEmptyStringToNull = true)]
        public int RollingROP { get; set; }

        public int ShortageWeek { get; set; }

        public BOM_week week01 { get; set; }
        public BOM_week week02 { get; set; }
        public BOM_week week03 { get; set; }
        public BOM_week week04 { get; set; }
        public BOM_week week05 { get; set; }
        public BOM_week week06 { get; set; }
        public BOM_week week07 { get; set; }
        public BOM_week week08 { get; set; }
        public BOM_week week09 { get; set; }
        public BOM_week week10 { get; set; }
        public BOM_week week11 { get; set; }
        public BOM_week week12 { get; set; }
        public BOM_week week13 { get; set; }
        public BOM_week week14 { get; set; }
        public BOM_week week15 { get; set; }
        public BOM_week week16 { get; set; }
        public BOM_week week17 { get; set; }
        public BOM_week week18 { get; set; }
        public BOM_week week19 { get; set; }
        public BOM_week week20 { get; set; }
        public BOM_week week21 { get; set; }
        public BOM_week week22 { get; set; }
        public BOM_week week23 { get; set; }
        public BOM_week week24 { get; set; }
        public BOM_week week25 { get; set; }
        public BOM_week week26 { get; set; }
        public BOM_week week27 { get; set; }
        public BOM_week week28 { get; set; }
        public BOM_week week29 { get; set; }
        public BOM_week week30 { get; set; }
        public BOM_week week31 { get; set; }
        public BOM_week week32 { get; set; }
        public BOM_week week33 { get; set; }
        public BOM_week week34 { get; set; }
        public BOM_week week35 { get; set; }
        public BOM_week week36 { get; set; }
        public BOM_week week37 { get; set; }
        public BOM_week week38 { get; set; }
        public BOM_week week39 { get; set; }
        public BOM_week week40 { get; set; }
        public BOM_week week41 { get; set; }
        public BOM_week week42 { get; set; }
        public BOM_week week43 { get; set; }
        public BOM_week week44 { get; set; }
        public BOM_week week45 { get; set; }
        public BOM_week week46 { get; set; }
        public BOM_week week47 { get; set; }
        public BOM_week week48 { get; set; }
        public BOM_week week49 { get; set; }
        public BOM_week week50 { get; set; }
        public BOM_week week51 { get; set; }
        public BOM_week week52 { get; set; }
    }

    public class BOM_week
    {
        //public DateTime start { get; set; }
        //public DateTime end { get; set; }
        public string period { get; set; }

        [DisplayName("Final Qty")]
        [DisplayFormat(DataFormatString = "{0:N0}", ConvertEmptyStringToNull = true)]
        public int final_qty { get; set; }

        public bool income { get; set; }

        public int income_qty { get; set; }

        public List<BOM_income> forecast_incomes { get; set; }
    }

    public class BOM_income
    {
        public string entryID { get; set; }
        public string cluster { get; set; }

        [DisplayName("PO No")]
        public string po_no { get; set; }

        [DisplayName("Expected Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd}")]
        public DateTime expected_date { get; set; }

        [DisplayName("Quantity")]
        [DisplayFormat(DataFormatString = "{0:N0}", ConvertEmptyStringToNull = true)]
        public decimal po_qty { get; set; }
    }

    public class BOM_data
    {
        public DateTime thisWeekStart { get; set; }
        public DateTime thisWeekPreviousDate { get; set; }
        public List<BOM_item> items { get; set; }
        public List<BOM_PO> pos { get; set; }
    }

    public class BOM_item
    {
        public string customer_part_no { get; set; }
        public string item_id { get; set; }
        public string item_desc { get; set; }
        public decimal qty_on_hand { get; set; }
        public decimal qty_past_due { get; set; }
        public decimal UsagePerWeek { get; set; }
    }

    public class BOM_PO
    {
        public string customer_part_no { get; set; }
        public string po_no { get; set; }
        public DateTime expected_date { get; set; }
        public decimal po_qty { get; set; }
    }


    public class BOM_ProcureInfo
    {
        [DisplayName("Customer Part No")]
        public string customer_part_no { get; set; }

        [DisplayName("Purchase Part No")]
        public string item_id { get; set; }

        [DisplayName("Supplier Name")]
        public string SupplierName { get; set; }

        [DisplayName("Lead Time")]
        public int LeadTime { get; set; }

        [DisplayName("Order Min Qty")]
        public int OrderMinQty { get; set; }

        [DisplayName("Shortage Week")]
        public int ShortageWeek { get; set; }

        [DisplayName("Require Date")]
        public DateTime RequireDate { get; set; }

        [DisplayName("Buy Qty")]
        public int BuyQty { get; set; }
    }

}
