using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuntimeVariables
{

    #region Supplier Scorecard

    public class SupplierScorecard_raw
    {
        public string ym { get; set; }
        public List<SupplierScorecard_raw_spend> spend { get; set; }
        public List<SupplierScorecard_raw_price> price { get; set; }
        public List<SupplierScorecard_raw_receive> receive { get; set; }
        public List<SupplierScorecard_raw_dmr> dmr { get; set; }
        public List<SupplierScorecard_raw_pod> pod { get; set; }
    }
    public class SupplierScorecard_raw_spend
    {
        public int spend_ym { get; set; }
        public decimal total_po_placed { get; set; }
        public decimal yoy_total_po_placed { get; set; }
        public int po_lines_issued { get; set; }
        public int yoy_po_lines_issued { get; set; }
    }
    public class SupplierScorecard_raw_price
    {
        public string item_id { get; set; }
        public string supplier_part_no { get; set; }
        public decimal unit_price { get; set; }
        public int spend_ym { get; set; }
        public decimal extened_value { get; set; }
        public decimal sub_total { get; set; }
        public decimal grand_total { get; set; }
    }
    public class SupplierScorecard_raw_receive
    {
        public int completed_ym { get; set; }
        public int total_lines { get; set; }
        public int lines_late { get; set; }
        public int lines_early { get; set; }
        public int on_time_summary { get; set; }
        public decimal lines_on_time { get; set; }
    }
    public class SupplierScorecard_raw_dmr
    {
        public int dmrYM { get; set; }
        public int Freight_Damages_found_at_Receipt { get; set; }
        public int shipments_Missing_Documents { get; set; }
        public int shipments_with_quantity_discrepancies { get; set; }
        public int Parts_with_Quality_Defects { get; set; }
        public int corrective_actions { get; set; }
    }
    public class SupplierScorecard_raw_pod
    {
        public int podYM { get; set; }
        public int rfq_sent { get; set; }
        public int recommended_forward { get; set; }
        public int response_time { get; set; }
    }


    public class SupplierScorecard_finished
    {
        public SP_Return DataFlag { get; set; }
        public List<string> ym { get; set; }

        public SupplierScorecard_finished_category_decimal total_po_placed { get; set; }
        public SupplierScorecard_finished_category_decimal yoy_total_po_placed { get; set; }
        public SupplierScorecard_finished_category_int po_lines_issued { get; set; }
        public SupplierScorecard_finished_category_int yoy_po_lines_issued { get; set; }

        public List<SupplierScorecard_finished_pricing> price { get; set; }

        public SupplierScorecard_finished_category_int total_lines { get; set; }
        public SupplierScorecard_finished_category_int lines_late { get; set; }
        public SupplierScorecard_finished_category_int lines_early { get; set; }
        public SupplierScorecard_finished_category_decimal lines_on_time { get; set; }

        public SupplierScorecard_finished_category_int Freight_Damages_found_at_Receipt { get; set; }
        public SupplierScorecard_finished_category_int shipments_Missing_Documents { get; set; }
        public SupplierScorecard_finished_category_int shipments_with_quantity_discrepancies { get; set; }

        public SupplierScorecard_finished_category_int Parts_with_Quality_Defects { get; set; }
        public SupplierScorecard_finished_category_int corrective_actions { get; set; }

        public SupplierScorecard_finished_category_int number_of_rfq_sent { get; set; }
        public SupplierScorecard_finished_category_int response_time { get; set; }
        public SupplierScorecard_finished_category_int number_of_rfq_recommended_forward { get; set; }
    }
    public class SupplierScorecard_finished_category_int
    {
        public string category { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N0}")]
        public int ym01 { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N0}")]
        public int ym02 { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N0}")]
        public int ym03 { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N0}")]
        public int ym04 { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N0}")]
        public int ym05 { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N0}")]
        public int ym06 { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N0}")]
        public int ym07 { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N0}")]
        public int ym08 { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N0}")]
        public int ym09 { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N0}")]
        public int ym10 { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N0}")]
        public int ym11 { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N0}")]
        public int ym12 { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N0}")]
        public int subtotal { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N1}")]
        public decimal average { get; set; }
    }
    public class SupplierScorecard_finished_category_decimal
    {
        public string category { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N2}")]
        public decimal ym01 { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N2}")]
        public decimal ym02 { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N2}")]
        public decimal ym03 { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N2}")]
        public decimal ym04 { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N2}")]
        public decimal ym05 { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N2}")]
        public decimal ym06 { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N2}")]
        public decimal ym07 { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N2}")]
        public decimal ym08 { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N2}")]
        public decimal ym09 { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N2}")]
        public decimal ym10 { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N2}")]
        public decimal ym11 { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N2}")]
        public decimal ym12 { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N2}")]
        public decimal subtotal { get; set; }
    }
    public class SupplierScorecard_finished_pricing
    {
        [DisplayName("Item ID")]
        public string item { get; set; }

        [DisplayName("Supplier Part No")]
        public string supplier_part_no { get; set; }

        [DisplayName("Unit Price")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N4}")]
        public decimal unit_price { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N2}")]
        public decimal ym01 { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N2}")]
        public decimal ym02 { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N2}")]
        public decimal ym03 { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N2}")]
        public decimal ym04 { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N2}")]
        public decimal ym05 { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N2}")]
        public decimal ym06 { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N2}")]
        public decimal ym07 { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N2}")]
        public decimal ym08 { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N2}")]
        public decimal ym09 { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N2}")]
        public decimal ym10 { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N2}")]
        public decimal ym11 { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N2}")]
        public decimal ym12 { get; set; }

        [DisplayName("Sub Total")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N2}")]
        public decimal sub_total { get; set; }

        [DisplayName("Grand Total")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N2}")]
        public decimal grand_total { get; set; }

        [DisplayName("Target Price")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N2}")]
        public decimal target { get; set; }

        [DisplayName("Actual Percentage")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N2}")]
        public decimal actual { get; set; }
    }

    #endregion

    #region BuySheet Lot

    public class Lot_VendorGroup
    {
        public bool selected { get; set; }

        public int i { get; set; }

        [DisplayName("Group ID")]
        public int group_id { get; set; }

        [DisplayName("Group Name")]
        public string group_name { get; set; }

        [DisplayName("Vendor ID")]
        [DisplayFormat(ApplyFormatInEditMode = false, DataFormatString = "{0:N}")]
        public int? vendor_id { get; set; }

        [DisplayName("Vendor Name")]
        public string vendor_name { get; set; }
    }


    public class Lot_Number
    {
        public int i { get; set; }

        [DisplayName("Lot UID")]
        public int lot_uid { get; set; }

        [DisplayName("Group ID")]
        public int group_id { get; set; }

        [DisplayName("Group Name")]
        public string group_name { get; set; }

        [DisplayName("Range Start")]
        public int range_start { get; set; }

        [DisplayName("Range End")]
        public int range_end { get; set; }

        [DisplayName("Current Used")]
        //[DisplayFormat(ApplyFormatInEditMode = false, DataFormatString = "{0:N}")]
        public int current_used { get; set; }
    }

    #endregion

    #region BuySheet

    public class Buy_ArchiveItem
    {
        [DisplayName("No")]
        public int rrn { get; set; }

        [DisplayName("Cluster")]
        public string cluster { get; set; }

        public string entryID { get; set; }

        [DisplayName("Buy ID")]
        public int viewID { get; set; }

        [DisplayName("Part Number")]
        public string ItemID { get; set; }

        [DisplayName("Planner")]
        public string planner { get; set; }

        [DisplayName("PO Number")]
        public string PO_no { get; set; }

        [DisplayName("PO to Loc")]
        [DisplayFormat(DataFormatString = "{0:N0}", ConvertEmptyStringToNull = true)]
        public decimal po_to_loc { get; set; }

        [DisplayName("Package")]
        public string package { get; set; }

        [DisplayName("Total Value (USD)")]
        [DisplayFormat(DataFormatString = "{0:N2}", ConvertEmptyStringToNull = false)]
        public float total_value { get; set; }

        [DisplayName("Total Weight (KG)")]
        [DisplayFormat(DataFormatString = "{0:N2}", ConvertEmptyStringToNull = false)]
        public float total_weight { get; set; }

        [DisplayName("PPAP")]
        public string PPAP { get; set; }

        [DisplayName("Supplier")]
        public string supplier_name { get; set; }

        [DisplayName("Order Date")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ConvertEmptyStringToNull = true, NullDisplayText = "")]
        public DateTime? order_date { get; set; }

        [DisplayName("Unit Price")]
        [DisplayFormat(DataFormatString = "{0:N4}", ConvertEmptyStringToNull = true)]
        public float unit_price { get; set; }

        [DisplayName("currency")]
        public string currency { get; set; }

        [DisplayName("Quantity Ordered")]
        [DisplayFormat(DataFormatString = "{0:N0}", ConvertEmptyStringToNull = false)]
        public float qty_ordered { get; set; }

        [DisplayName("TWD")]
        [DisplayFormat(DataFormatString = "{0:N5}", ConvertEmptyStringToNull = false)]
        public float twd { get; set; }

        [DisplayName("Carbon Steel")]
        [DisplayFormat(DataFormatString = "{0:N1}", ConvertEmptyStringToNull = false)]
        public float carbon_steel { get; set; }

        [DisplayName("Stainless Steel")]
        [DisplayFormat(DataFormatString = "{0:N2}", ConvertEmptyStringToNull = false)]
        public float stainless_steel { get; set; }

        [DisplayName("Nickel")]
        [DisplayFormat(DataFormatString = "{0:N0}", ConvertEmptyStringToNull = false)]
        public float nickel { get; set; }

        [DisplayName("Copper")]
        [DisplayFormat(DataFormatString = "{0:N0}", ConvertEmptyStringToNull = false)]
        public float copper { get; set; }

        public string noteCount { get; set; }

        public string Splits { get; set; }

        public int quotes_count { get; set; }
    }


    public class Buy
    {
        [DisplayName("No")]
        public int no { get; set; }

        [DisplayName("Buy ID")]
        public int viewID { get; set; }

        public string entryID { get; set; }

        [DisplayName("Part Number")]
        public string ItemID { get; set; }

        [DisplayName("Description")]
        public string item_desc { get; set; }

        [DisplayName("Ext Desc")]
        public string extended_desc { get; set; }

        [DisplayName("Part Type")]
        public string part_assignment { get; set; }

        [DisplayName("Purchase Class")]
        public string purchase_class { get; set; }

        [DisplayName("PPAP")]
        public string PPAP { get; set; }

        [DisplayName("Program")]
        public string program { get; set; }

        [DisplayName("Date Created")]
        public DateTime date_created { get; set; }

        [DisplayName("Planner")]
        public string planner { get; set; }

        [DisplayName("Buy To Loc")]
        public string buy_to_loc { get; set; }

        [DisplayName("Package")]
        public string package { get; set; }

        [DisplayName("Preload Quantity")]
        public string preload_quantity { get; set; }

        [DisplayName("Preload Loc")]
        public string preload_loc { get; set; }

        [DisplayName("New Quantity")]
        [DisplayFormat(ApplyFormatInEditMode = false, DataFormatString = "{0:N0}", ConvertEmptyStringToNull = true)]
        public int? new_quantity { get; set; }

        [DisplayName("Unit Weight (LB)")]
        public string unit_weight { get; set; }

        [DisplayName("Total Value (USD)")]
        public string total_value { get; set; }

        [DisplayName("Total Weight (KG)")]
        public string total_weight { get; set; }

        //should be supplier_approve_name for approved
        [DisplayName("Vendor Approve")]
        public string vendor_approve { get; set; }

        [DisplayName("PO Number")]
        public string PO_no { get; set; }

        [DisplayName("Cost/EA")]
        public string standard_cost { get; set; }

        [DisplayName("True Supplier Cost/EA")]
        public string supplier_standard_cost { get; set; }

        [DisplayName("Loc Supplier Cost/EA")]
        public string loc_supplier_standard_cost { get; set; }

        [DisplayName("True Lead Time")]
        public string LeadTime { get; set; }

        [DisplayName("True Primary Supplier")]
        public string TruePrimarySupplier { get; set; }

        [DisplayName("Primary Supplier Currency")]
        public int primary_supplier_currency { get; set; }

        [DisplayName("Loc Primary Supplier")]
        public string loc_primary_supplier_name { get; set; }

        [DisplayName("Last Vendor")]
        public string last_vendor { get; set; }

        [DisplayName("Last PO Date")]
        [DisplayFormat(ConvertEmptyStringToNull = true, NullDisplayText = "", ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime? last_PO_date { get; set; }

        [DisplayName("Last Price/C")]
        public string last_price { get; set; }

        [DisplayName("Last Quantity")]
        public string previous_quantity { get; set; }

        [DisplayName("Sourcing")]
        public string sourcing { get; set; }

        [DisplayName("PO Quantity")]
        public string PO_order_quantity_1 { get; set; }

        [DisplayName("Preload PO/SO")]
        public string preload_PO_SO { get; set; }

        [DisplayName("Preload SO")]
        public string preload_so { get; set; }

        [DisplayName("Preload PO")]
        public string preload_po { get; set; }

        [DisplayName("True MOQ Primary Supplier")]
        public string MOQSupplierLoc1 { get; set; }

        //should be supplier_approve_id for approved
        [DisplayName("Vendor ID")]
        public decimal? vendor_id { get; set; }

        public string cluster { get; set; }

        [DisplayName("Company ID")]
        public string company_id { get; set; }

        [DisplayName("Required Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime required_date { get; set; }

        [DisplayName("PO to Loc")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N0}", ConvertEmptyStringToNull = true)]
        public decimal po_to_loc { get; set; }

        [DisplayName("Expected Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime expected_date { get; set; }

        [DisplayName("Primary Supplier ID")]
        public decimal? primary_supplier_id { get; set; }

        [DisplayName("Urgent")]
        public Boolean urgent { get; set; }

        [DisplayName("Creator")]
        public string creator { get; set; }

        public DateTime z_date { get; set; }

        [DisplayName("Sourcing IN")]
        public string sourcing_in { get; set; }

        [DisplayName("Sourcing VN")]
        public string sourcing_vn { get; set; }

        [DisplayName("New Note")]
        public string new_note { get; set; }

        [DisplayName("Note Count")]
        public string noteCount { get; set; }

        public string Splits { get; set; }


        [DisplayName("Vendor Name")]
        public string vendor_name { get; set; }

        [DisplayName("Supplier Name")]
        public string supplier_name { get; set; }

        [DisplayName("Unit Price")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N4}", ConvertEmptyStringToNull = true)]
        public decimal unit_price { get; set; }

        [DisplayName("Currency")]
        public int currency_id { get; set; }

        [DisplayName("Exchange Rate")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N4}", ConvertEmptyStringToNull = true)]
        public decimal currency_rate { get; set; }

        [DisplayName("Tariff")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N2}", ConvertEmptyStringToNull = true)]
        public decimal tariff { get; set; }

        [DisplayName("Price All In")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N4}", ConvertEmptyStringToNull = true)]
        public decimal price_all_in { get; set; }

        [DisplayName("MOQ")]
        public int moq { get; set; }

        [DisplayName("Lead Time")]
        public string lead { get; set; }

        [DisplayName("Supplier Info")]
        public string supplier_info { get; set; }

        [DisplayName("Grand Total Value")]
        public string grand_total_value { get; set; }

        [DisplayName("Quotes Count")]
        public int quotes_count { get; set; }

        [DisplayName("Standard Cost Current")]
        public decimal standard_cost_current { get; set; }

        [DisplayName("Buyer Name")]
        public string buyer_name { get; set; }

        public List<BuyQuote> theQuotes { get; set; }

        public BuyFTB theFTB { get; set; }


        public string filter_sourcing { get; set; }
        public string filter_buyer { get; set; }
    }


    public class BuyNote
    {
        [DisplayName("Buy ID")]
        public int viewID { get; set; }

        public string entryID { get; set; }

        [DisplayName("Note ID")]
        public int notesID { get; set; }

        [DisplayName("Note")]
        public string notes { get; set; }

        [DisplayName("User")]
        public string user { get; set; }

        [DisplayName("Date Created")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime createDate { get; set; }

        [DisplayName("Read")]
        public string read { get; set; }
    }


    public class BuySplit
    {
        public bool selected { get; set; }

        [DisplayName("Buy ID")]
        public int viewID { get; set; }

        [DisplayName("Split ID")]
        public string splitID { get; set; }

        public string entryID { get; set; }

        [DisplayName("Ship Method")]
        public string ship_method { get; set; }

        [DisplayName("Required Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime? release_date { get; set; }
        public string EDT_release_date { get; set; }

        [DisplayName("Expected Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime? expected_date { get; set; }
        public string EDT_expected_date { get; set; }

        [DisplayName("New Quantity")]
        [DisplayFormat(DataFormatString = "{0:N0}", ApplyFormatInEditMode = true)]
        public int? quantity { get; set; }

        [DisplayName("PO Quantity")]
        [DisplayFormat(DataFormatString = "{0:N0}", ApplyFormatInEditMode = true)]
        public int po_quantity { get; set; }

        [DisplayName("Preload SO")]
        public string preload_so { get; set; }

        [DisplayName("Preload PO")]
        public string preload_po { get; set; }

        [DisplayName("Preload PO/SO")]
        public string preload_po_so { get; set; }

        [DisplayName("Summary Qty")]
        [DisplayFormat(DataFormatString = "{0:N0}", ApplyFormatInEditMode = true)]
        public int? sumQuant { get; set; }

        [DisplayName("Summary PO Qty")]
        [DisplayFormat(DataFormatString = "{0:N0}", ApplyFormatInEditMode = true)]
        public int po_sumQuant { get; set; }
    }


    public class BuyQuote
    {
        public int viewID { get; set; }

        [DisplayName("No")]
        public int quote_seq { get; set; }

        [DisplayName("Supplier")]
        public int supplier_id { get; set; }

        [DisplayName("Supplier")]
        public string supplier_name { get; set; }

        [DisplayName("Country")]
        public string supplier_country { get; set; }

        [DisplayName("Vendor")]
        public int vendor_id { get; set; }

        [DisplayName("Vendor")]
        public string vendor_name { get; set; }

        public string vendor_terms { get; set; }

        [DisplayName("Unit Price")]
        [DisplayFormat(DataFormatString = "{0:N4}", ConvertEmptyStringToNull = true, NullDisplayText = "")]
        public decimal unit_price { get; set; }

        [DisplayName("Currency")]
        public int currency_id { get; set; }

        [DisplayName("Currency")]
        public string currency_name { get; set; }

        [DisplayName("Currency Date")]
        [DisplayFormat(DataFormatString = "{0:N4}", ConvertEmptyStringToNull = true, NullDisplayText = "")]
        public decimal currency_rate { get; set; }

        [DisplayName("Price USD")]
        [DisplayFormat(DataFormatString = "{0:N4}", ConvertEmptyStringToNull = true, NullDisplayText = "")]
        public decimal price_usd { get; set; }

        [DisplayName("Tariff %")]
        [DisplayFormat(DataFormatString = "{0:N2}", ConvertEmptyStringToNull = true, NullDisplayText = "")]
        public float tariff { get; set; }

        [DisplayName("Price All In (USD)")]
        [DisplayFormat(DataFormatString = "{0:N4}", ConvertEmptyStringToNull = true, NullDisplayText = "")]
        public decimal price_all_in { get; set; }

        [DisplayName("Price Diff %")]
        [DisplayFormat(DataFormatString = "{0:N2}", ConvertEmptyStringToNull = true, NullDisplayText = "")]
        public float price_diff { get; set; }

        [DisplayName("MOQ")]
        public int moq { get; set; }

        [DisplayName("Lead Time")]
        public string lead { get; set; }

        [DisplayName("Supplier Info")]
        public string supplier_info { get; set; }

        [DisplayName("Status")]
        public string z_status { get; set; }
    }


    public class BuyFTB
    {
        [DisplayName("Buy ID")]
        public int viewID { get; set; }

        [DisplayName("Sourced By")]
        public string sourced_by { get; set; }

        [DisplayName("Sourced By")]
        public string sourcing { get; set; }

        [DisplayName("POD Number")]
        public int? pod_no { get; set; }

        [DisplayName("Buyer")]
        public string buyer { get; set; }

        [DisplayName("Buyer")]
        public string procure { get; set; }

        [DisplayName("Ship Method")]
        public string ship_method { get; set; }

        [DisplayName("CPAL Required")]
        public bool cpal_required { get; set; }

        [DisplayName("CPAL Complete")]
        public bool cpal_complete { get; set; }

        [DisplayName("CPAL Ship Approval")]
        public bool cpal_ship_approval { get; set; }

        [DisplayName("PPAP Document")]
        public string ppap_level { get; set; }

        [DisplayName("PPAP Cost")]
        public decimal? ppap_cost { get; set; }

        [DisplayName("PPAP Note")]
        public string ppap_note { get; set; }

        [DisplayName("Tooling Charge")]
        public bool tc { get; set; }

        [DisplayName("TC Cost")]
        public decimal? tc_cost { get; set; }

        [DisplayName("TC Note")]
        public string tc_note { get; set; }

        [DisplayName("Drawing")]
        public bool drawing { get; set; }

        [DisplayName("Print No")]
        public string print_no { get; set; }

        [DisplayName("Revision No")]
        public string rev_no { get; set; }

        [DisplayName("Customer Provides Sample")]
        public bool? customer_provide_sample { get; set; }

        [DisplayName("Sample Tracking Number")]
        public string sample_tracking_number { get; set; }

        [DisplayName("Deviation Comfirmed (If flagged on POD)")]
        public bool deviation_comfirm { get; set; }

        [DisplayName("Step")]
        public int step { get; set; }

        //[DisplayName("Status")]
        //public string sts { get; set; }

        [DisplayName("Sourcing Review")]
        public string sourcing_review { get; set; }

        [DisplayName("Follow-up Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime follow_up_date { get; set; }

        public List<BuyFTB_document> theDocuments { get; set; }

        public List<BuyFTB_StepLogs> theStepLogs { get; set; }
    }


    public class BuyFTB_document
    {
        [DisplayName("Link Name")]
        public string link_name { get; set; }

        [DisplayName("Link Path")]
        [System.Web.Mvc.AllowHtml]
        public string link_path { get; set; }

        [DisplayName("Folder")]
        public string folder_path { get; set; }

        [System.Web.Mvc.AllowHtml]
        [DisplayName("File Name")]
        public string file_name { get; set; }

        [DisplayName("Last Maintained By")]
        public string last_maintained_by { get; set; }

        [DisplayName("Last Modified Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime date_last_modified { get; set; }
    }


    public class BuyFTB_StepLogs
    {
        public int log_id { get; set; }

        [DisplayName("Log")]
        public string desc { get; set; }

        [DisplayName("Time Stamp")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy HH:mm}")]
        public DateTime stamp { get; set; }
    }


    public class FTB_OpenPO
    {
        [DisplayName("PO No")]
        public int po_no { get; set; }

        [DisplayName("Loc")]
        public int location_id { get; set; }

        //[DisplayName("Vendor ID")]
        public int vendor_id { get; set; }

        [DisplayName("Vendor")]
        public string vendor_name { get; set; }

        [DisplayName("Buyer")]
        public string buyer { get; set; }

        [DisplayName("Line No")]
        public int line_no { get; set; }

        [DisplayName("Part No")]
        public string item_id { get; set; }

        [DisplayName("Item Desc")]
        public string item_desc { get; set; }

        [DisplayName("Quantity")]
        [DisplayFormat(DataFormatString = "{0:N0}", ConvertEmptyStringToNull = true)]
        public decimal qty_ordered { get; set; }

        [DisplayName("Required Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime required_date { get; set; }

        [DisplayName("Expected Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime expected_date { get; set; }

        [DisplayName("Expedite Notes")]
        public string expedite_notes { get; set; }

        [DisplayName("Part Assignment")]
        public string part_assignment { get; set; }

        [DisplayName("Hot Part")]
        public string hot_part { get; set; }

        [DisplayName("Follow up Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}", NullDisplayText = "")]
        public DateTime? ftb_followup_date { get; set; }

        [DisplayName("Risk to Ship Date")]
        public string ftb_risk_to_shipdate { get; set; }

        //[DisplayName("Follow up Comment")]
        //public string ftb_followup_comment { get; set; }

        public int line_count { get; set; }

        public int note_count { get; set; }

        public string color { get; set; }
    }


    public class Charge_Tooling
    {
        public int po_line_ud_uid { get; set; }

        [DisplayName("PO No")]
        public int po_no { get; set; }

        [DisplayName("Location")]
        public int location_id { get; set; }

        [DisplayName("Vendor")]
        public int vendor_id { get; set; }
        public string vendor_name { get; set; }
        public string vendors { get; set; }

        [DisplayName("Line No")]
        public int line_no { get; set; }

        [DisplayName("Item ID")]
        public string item_id { get; set; }

        [DisplayName("Real Part Number")]
        public string real_part_no { get; set; }

        [DisplayName("Charge Line")]
        public int charge_line { get; set; }

        [DisplayName("Tooling Charge")]
        [DisplayFormat(DataFormatString = "{0:N0}", ConvertEmptyStringToNull = true, NullDisplayText = "")]
        public int cost { get; set; }

        [DisplayName("Refunded Qty")]
        [DisplayFormat(DataFormatString = "{0:N0}", ConvertEmptyStringToNull = true, NullDisplayText = "")]
        public int tooling_qty { get; set; }

        [DisplayName("Refund")]
        public Boolean refund { get; set; }

        [DisplayName("Sum Received")]
        [DisplayFormat(DataFormatString = "{0:N0}", ConvertEmptyStringToNull = true, NullDisplayText = "")]
        public uint sum_received { get; set; }

        [DisplayName("Overflow")]
        public string overflow { get; set; }

        [DisplayName("Refund Date")]
        public DateTime refund_date { get; set; }
    }


    public class Charge_ToolingReceipts
    {
        [DisplayName("Receipt No")]
        public int receipt_number { get; set; }

        [DisplayName("Receipt Line No")]
        public int line_number { get; set; }

        [DisplayName("Receipt Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime receipt_date { get; set; }

        [DisplayName("PO No")]
        public int po_number { get; set; }

        [DisplayName("PO Line No")]
        public int po_line_number { get; set; }

        [DisplayName("Qty Received")]
        [DisplayFormat(DataFormatString = "{0:N0}", ConvertEmptyStringToNull = true, NullDisplayText = "")]
        public int qty_received { get; set; }
    }


    public class BuySheet_Rates_Metals
    {
        public int group { get; set; }
        public int i { get; set; }

        [DisplayName("Metal ID")]
        public int metal_id { get; set; }

        [DisplayName("Metal Name")]
        public string metal_name { get; set; }

        [DisplayName("Activated From")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd}")]
        public DateTime rate_date_start { get; set; }

        [DisplayName("Rates")]
        [DisplayFormat(ApplyFormatInEditMode = false, DataFormatString = "{0:N2}")]
        public float metal_rate { get; set; }
    }


    public class BuySheet_Preload
    {
        public int? order_no { get; set; }

        public int? po_no { get; set; }

        public DateTime? requested_date { get; set; }

        public DateTime? expected_date { get; set; }

        public string carrier { get; set; }

        public string preload_po_so { get; set; }
    }


    public class BuySheet_BuyID
    {
        public int GroupNo { get; set; }

        public string viewID { get; set; }

        public string PO_no { get; set; }
    }


    public class BuySheet_Excel
    {
        [DisplayName("Buy ID")]
        public int viewID { get; set; }

        [DisplayName("Part Number")]
        public string ItemID { get; set; }

        [DisplayName("Date Created")]
        public DateTime date_created { get; set; }

        [DisplayName("Description")]
        public string item_desc { get; set; }

        [DisplayName("Ext Desc")]
        public string extended_desc { get; set; }

        [DisplayName("Buy To Loc")]
        public string buy_to_loc { get; set; }

        [DisplayName("PPAP")]
        public string PPAP { get; set; }

        [DisplayName("Package")]
        public string package { get; set; }

        //[DisplayName("Ship Out Date")]
        //public DateTime? ship_out_date { get; set; }

        [DisplayName("Required Date")]
        public DateTime? required_date { get; set; }

        [DisplayName("New Quantity")]
        public int? new_quantity { get; set; }

        [DisplayName("PO Number")]
        public string PO_no { get; set; }

        [DisplayName("Program")]
        public string program { get; set; }

        [DisplayName("Buyer Name")]
        public string buyer_name { get; set; }

        [DisplayName("True Primary Supplier")]
        public string TruePrimarySupplier { get; set; }

        [DisplayName("True Lead Time")]
        public string LeadTime { get; set; }

        //should be supplier_approve_name for approved
        [DisplayName("Vendor Approve")]
        public string vendor_approve { get; set; }

        [DisplayName("Unit Price (C)")]
        public string unit_price { get; set; }

        [DisplayName("USD/C")]
        public string new_price_usd { get; set; }

        public List<BuySplit> BuyRelease { get; set; }

    }


    public class BuySheet_CreatePO
    {
        [DisplayName("No")]
        public int no { get; set; }

        [DisplayName("Create")]
        public string go { get; set; }

        [DisplayName("Vendor Approve")]
        public string vendor_approve { get; set; }

        [DisplayName("PO To Loc")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N0}", ConvertEmptyStringToNull = true)]
        public decimal po_to_loc { get; set; }

        public List<BuySheet_CreatePOLine> POLine { get; set; }
    }


    public class BuySheet_CreatePOLine
    {
        [DisplayName("No")]
        public int no { get; set; }

        [DisplayName("Status")]
        public string status { get; set; }

        public Buy BuySheet { get; set; }

        public List<BuySplit> BuyRelease { get; set; }
    }

    #endregion

    #region BuySheet P21

    public class cmd_Result
    {
        public int GroupNo { get; set; }
        public int Succeeded { get; set; }
        public List<int> viewID { get; set; }
        public string msg { get; set; }
    }


    public class cmd_Response
    {
        public SP_Return ResultModel { get; set; }
        public List<cmd_Result> ResultList { get; set; }
    }

    #endregion

    #region BuySheet Vendor/Supplier

    public class BuySheet_SupplierInfo
    {
        [DisplayName("Location ID")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N0}", ConvertEmptyStringToNull = true)]
        public decimal location_id { get; set; }

        [DisplayName("Part Number")]
        public string item_id { get; set; }

        [DisplayName("Supplier ID")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N0}", ConvertEmptyStringToNull = true)]
        public decimal supplier_id { get; set; }

        [DisplayName("Supplier Country")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N0}", ConvertEmptyStringToNull = true)]
        public string phys_country { get; set; }

        [DisplayName("Supplier Part No")]
        public string supplier_part_no { get; set; }

        [DisplayName("Cost")]
        public decimal cost { get; set; }

        [DisplayName("Currency")]
        public int currency_id { get; set; }
    }


    public class Vendor
    {
        public decimal vendor_id { get; set; }

        public string vendor_name { get; set; }

        public string terms_desc { get; set; }
    }


    public class Vendor_PaymentTerms
    {
        [DisplayName("Company")]
        public string company_id { get; set; }

        [DisplayName("Vendor")]
        public int vendor_id { get; set; }

        [DisplayName("Vendor Name")]
        public string vendor_name { get; set; }

        [DisplayName("Terms")]
        public string terms_desc { get; set; }

        [DisplayName("Default Invoice Desc")]
        public string default_invoice_desc { get; set; }

        [DisplayName("Country")]
        public string phys_country { get; set; }

        [DisplayName("Created Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd}")]
        public string date_created { get; set; }
    }


    public class Asia_Country
    {
        [DisplayName("Country Code")]
        public string country_code { get; set; }

        [DisplayName("Country Name")]
        public string country_name { get; set; }

        [DisplayName("Asia")]
        public bool is_asia { get; set; }
    }


    public class Supplier
    {
        [DisplayName("Supplier ID")]
        public int supplier_id { get; set; }

        [DisplayName("Supplier Name")]
        public string supplier_name { get; set; }
    }

    #endregion

}
