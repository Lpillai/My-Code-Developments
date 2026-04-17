using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuntimeVariables
{

    public class LoginReport
    {
        //public int report_id { get; set; }

        public string cluster { get; set; }

        [DisplayName("Cluster Name")]
        public string cluster_name { get; set; }

        [DisplayName("Reports")]
        public string report_name { get; set; }

        public string run { get; set; }

        public string param { get; set; }

        public string path { get; set; }

        public string output { get; set; }
    }


    #region OSBuy

    public class Lot_CartonString
    {
        public decimal lot_no { get; set; }

        public string mfg_lot { get; set; }

        public string carton_string { get; set; }
    }
    public class Lot_CartonQty
    {
        public decimal lot_no { get; set; }

        public string mfg_lot { get; set; }

        public int qty { get; set; }
    }
    public class Lot_PO
    {
        [DisplayName("PO No")]
        public int po_no { get; set; }

        [DisplayName("PO Line")]
        public int po_line { get; set; }

        public int lot_count { get; set; }

        public int rrn { get; set; }

        [DisplayName("Part No")]
        public string item_id { get; set; }

        [DisplayName("Lot No")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N0}")]
        public decimal lot_no { get; set; }

        [DisplayName("Mfg No")]
        public string mfg_lot { get; set; }

        [DisplayName("Line Quantity")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N0}")]
        public int line_qty { get; set; }

        [DisplayName("Print Quantity")]
        public string carton_string { get; set; }
    }

    public class OSBuyHistory
    {
        public int q1 { get; set; }

        public int q2 { get; set; }

        public List<OSBuyHistory_Q4> theQ4 { get; set; }

        public List<OSBuyHistory_Sheet> theSheet { get; set; }

        public List<OSBuyHistory_Quote> theQuotes { get; set; }
    }
    public class OSBuyHistory_Q4
    {
        [DisplayName("0 Sup")]
        public int s0 { get; set; }

        [DisplayName("1 Sup")]
        public int s1 { get; set; }

        [DisplayName("2 Sup")]
        public int s2 { get; set; }

        [DisplayName("3 Sup")]
        public int s3 { get; set; }

        [DisplayName("4 Sup")]
        public int s4 { get; set; }

        [DisplayName("5 Sup")]
        public int s5 { get; set; }

        [DisplayName("6 Sup")]
        public int s6 { get; set; }

        [DisplayName("7 Sup")]
        public int s7 { get; set; }

        [DisplayName("8 Sup")]
        public int s8 { get; set; }
    }
    public class OSBuyHistory_Sheet
    {
        [DisplayName("Buy ID")]
        public int BuyID { get; set; }

        [DisplayName("Part No")]
        public string ItemID { get; set; }

        [DisplayName("Item Desc")]
        public string item_desc { get; set; }

        [DisplayName("Extended Desc")]
        public string extended_desc { get; set; }

        [DisplayName("Planner")]
        public string planner { get; set; }

        [DisplayName("Sourcing")]
        public string sourcing { get; set; }

        [DisplayName("Sourcing VN")]
        public string sourcing_vn { get; set; }

        [DisplayName("Sourcing IN")]
        public string sourcing_in { get; set; }

        [DisplayName("Quantity")]
        public int new_quantity { get; set; }

        [DisplayName("Part Assignment")]
        public string part_assignment { get; set; }

        [DisplayName("Purchase Class")]
        public string purchase_class { get; set; }

        [DisplayName("PPAP")]
        public string PPAP { get; set; }

        [DisplayName("Program")]
        public string program { get; set; }

        [DisplayName("True Primary Supplier")]
        public string TruePrimarySupplier { get; set; }

        [DisplayName("Last Vendor")]
        public string last_vendor { get; set; }

        [DisplayName("Quote Count")]
        public int quote_count { get; set; }

        [DisplayName("Vendor Approve")]
        public string vendor_approve { get; set; }

        [DisplayName("PO No")]
        public int? po_no { get; set; }

        [DisplayName("TBD")]
        public int tbd { get; set; }

        [DisplayName("Not To Buy")]
        public int not2buy { get; set; }

        [DisplayName("Archive")]
        public bool archive { get; set; }

        [DisplayName("Date Created")]
        public DateTime date_created { get; set; }
    }
    public class OSBuyHistory_Quote
    {
        public int rrn { get; set; }

        [DisplayName("Buy ID")]
        public int BuyID { get; set; }

        [DisplayName("Quote Seq")]
        public int quote_seq { get; set; }

        [DisplayName("Supplier ID")]
        public int supplier_id { get; set; }

        [DisplayName("Supplier Name")]
        public string supplier_name { get; set; }

        [DisplayName("Price (USD)")]
        public decimal price_usd { get; set; }

        [DisplayName("Tariff")]
        public decimal tariff { get; set; }

        [DisplayName("Price All In")]
        public decimal price_all_in { get; set; }

        [DisplayName("MOQ")]
        public int moq { get; set; }

        [DisplayName("Lead")]
        public string lead { get; set; }

        [DisplayName("Supplier Info")]
        public string supplier_info { get; set; }

        [DisplayName("Approved")]
        public bool approved { get; set; }
    }

    #endregion


    #region MX Warehouse

    public class Bosal_Invoice
    {
        [DisplayName("Invoice No")]
        public string invoice_no { get; set; }

        [DisplayName("Line No")]
        public int line_no { get; set; }

        [DisplayName("Item ID")]
        public string item_id { get; set; }

        [DisplayName("Customer Part No")]
        public string customer_part_number { get; set; }

        [DisplayName("Line Quantity")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N0}")]
        public int qty_shipped { get; set; }

        [DisplayName("Quantity per Box")]
        public int box_qty { get; set; }
    }

    #endregion


    #region US Warehouse

    public class SPORTECH_PickTicket
    {
        //[DisplayName("Pick Ticket No")]
        //public string pick_ticket_no { get; set; }

        //[DisplayName("PO")]
        //public string po_no { get; set; }

        [DisplayName("Line No")]
        public int line_number { get; set; }

        [DisplayName("Item ID")]
        public string item_id { get; set; }

        //[DisplayName("Customer Part No")]
        //public string customer_part_number { get; set; }

        //[DisplayName("Item Desc")]
        //public string item_desc { get; set; }

        [DisplayName("Lot No")]
        public string lot_cd { get; set; }

        [DisplayName("Lot Quantity")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N0}")]
        public int qty_shipped { get; set; }

        [DisplayName("Quantity per Box")]
        public int box_qty { get; set; }
    }

    #endregion


    #region VN Warehouse

    public class GoodsNote_Receiving
    {
        public string receipts_string { get; set; }
        public List<GoodsNote_Receiving_hdr> theReceipts { get; set; }
    }
    public class GoodsNote_Receiving_hdr
    {
        public int receipt_number { get; set; }
        public string rec_date { get; set; }
        public string supplier_name { get; set; }
        public string supplier_address { get; set; }
        public string po_number { get; set; }
        public List<GoodsNote_Receiving_line> theLines { get; set; }
    }
    public class GoodsNote_Receiving_line
    {
        [DisplayName("Line No")]
        public int line_no { get; set; }

        [DisplayName("Item ID")]
        public string item_id { get; set; }

        [DisplayName("Item Desc")]
        public string item_desc_vn { get; set; }

        [DisplayName("Unit")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N0}")]
        public int unit_size { get; set; }

        [DisplayName("Documented Quantity")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N0}")]
        public int qty_received { get; set; }

        [DisplayName("Actual Quantity")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N0}")]
        public int qty_actual { get; set; }

        [DisplayName("Unit Price")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N4}")]
        public float unit_cost { get; set; }

        [DisplayName("Total Amount")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N2}")]
        public float extended_cost { get; set; }
    }


    public class GoodsNote_Shipping_hdr
    {
        public string invoice_no { get; set; }
        public string invoice_date { get; set; }
        public string customer_name { get; set; }
        public string customer_address { get; set; }
        public string po_no { get; set; }
        public List<GoodsNote_Shipping_line> theLines { get; set; }
    }
    public class GoodsNote_Shipping_line
    {
        [DisplayName("Line No")]
        public int line_no { get; set; }

        [DisplayName("Item ID")]
        public string customer_part_number { get; set; }

        [DisplayName("Item Desc")]
        public string item_desc_vn { get; set; }

        [DisplayName("Unit")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N0}")]
        public int unit_size { get; set; }

        [DisplayName("Documented Quantity")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N0}")]
        public int qty_shipped { get; set; }

        [DisplayName("Actual Quantity")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N0}")]
        public int qty_actual { get; set; }

        [DisplayName("Unit Price")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N4}")]
        public float unit_price { get; set; }

        [DisplayName("Total Amount")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N2}")]
        public float total_amount { get; set; }
    }

    #endregion


    #region Procurement

    public class RPT_InspectionAggregate
    {
        public string theYM { get; set; }

        public List<RPT_SupplierInspect> theTab1 { get; set; }

        public List<RPT_SupplierInspect> theTab2 { get; set; }
    }
    public class RPT_SupplierInspect
    {
        [DisplayName("Country")]
        public string country { get; set; }

        [DisplayName("Supplier")]
        public string supplier { get; set; }

        [DisplayName("Inspect")]
        public string inspect { get; set; }

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
        public int ym13 { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N0}")]
        public int ym14 { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N0}")]
        public int ym15 { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N0}")]
        public int ym16 { get; set; }
    }

    #endregion

}
