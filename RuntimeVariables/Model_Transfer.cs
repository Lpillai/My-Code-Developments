using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuntimeVariables
{
    public class Transfer_PlannedRecptDate_Override
    {
        [DisplayName("Transfer No")]
        public int transfer_no { get; set; }

        [DisplayName("Override Planned Recpt Date")]
        public DateTime? override_planned_recpt_date { get; set; }

        [DisplayName("Container/Tracking")]
        public string tracking_no { get; set; }
    }

    public class Transfer_PlannedRecptDate
    {
        [DisplayName("No")]
        public int no { get; set; }

        [DisplayName("Company")]
        public string company_id { get; set; }

        [DisplayName("From")]
        public int from_location_id { get; set; }

        [DisplayName("To")]
        public int to_location_id { get; set; }

        [DisplayName("Transfer No")]
        public int transfer_no { get; set; }

        [DisplayName("Container/Tracking")]
        public string tracking_no { get; set; }

        [DisplayName("Shipping Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime shipping_date { get; set; }

        [DisplayName("Planned Recpt Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime planned_recpt_date { get; set; }

        [DisplayName("Override Planned Recpt Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime? override_planned_recpt_date { get; set; }
        //public string EDT_override_planned_recpt_date { get; set; }

        [DisplayName("Days Late")]
        public int days_late { get; set; }

        [DisplayName("Item Count")]
        public int items { get; set; }

        [DisplayName("Quantity")]
        [DisplayFormat(DataFormatString = "{0:N0}", ConvertEmptyStringToNull = true)]
        public decimal transfer_qty { get; set; }
    }


    public class Transfer_ShippingCountry
    {
        public string country_cd { get; set; }

        public string country_name { get; set; }
    }
    public class Transfer_ShippingLocation
    {

        public int group_id { get; set; }

        public string loc_cd { get; set; }

        public string location_id { get; set; }
    }
    public class Transfer_ShippingLocGrp
    {
        public string country_cd { get; set; }

        public string country_name { get; set; }

        public int group_id { get; set; }

        public string loc_cd { get; set; }

        public string location_id { get; set; }
    }

    public class Transfer_ShippingCalendar
    {
        public int i { get; set; }

        public int date_uid { get; set; }

        [DisplayName("Ship Date")]
        public DateTime ship_date { get; set; }

        public string z_status { get; set; }

        public string ship_method { get; set; }
    }
    public class Transfer_ShippingCalendar_Method
    {
        public List<Transfer_ShippingCalendar> Ocean { get; set; }
        public List<Transfer_ShippingCalendar> Air { get; set; }
    }


    public class Transfer_Preload
    {
        [DisplayName("Loc")]
        public int location_id { get; set; }

        [DisplayName("Customer")]
        public int customer_id { get; set; }

        [DisplayName("Ship to Name")]
        public string ship2_name { get; set; }

        [DisplayName("Order Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime order_date { get; set; }

        [DisplayName("Order No")]
        public string order_no { get; set; }

        [DisplayName("PO No")]
        //[DisplayFormat(DataFormatString = "{0:N0}", ConvertEmptyStringToNull = true)]
        public int po_no { get; set; }

        [DisplayName("Lines")]
        public int lines { get; set; }

        [DisplayName("Cut off Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime cut_off_date { get; set; }

        [DisplayName("Ship Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime ship_date { get; set; }

        [DisplayName("Expected Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime expected_date { get; set; }

        [DisplayName("Weight(lb)")]
        //[DisplayFormat(DataFormatString = "{0:N0}", ConvertEmptyStringToNull = true)]
        public decimal wgt { get; set; }

        [DisplayName("Carrier")]
        public string carrier { get; set; }

        [DisplayName("Route Code")]
        public string route_code { get; set; }

        [DisplayName("Approved SO")]
        public string appr_so { get; set; }

        [DisplayName("Approved PO")]
        public string appr_po { get; set; }
    }


    public class Shipping_Orders
    {
        public int P21URL { get; set; }
        public DateTime required_date { get; set; }
        public int shift_days { get; set; }
        public int from_loc { get; set; }
        public List<ShippingOrder_Preload> preloads { get; set; }
        public List<ShippingOrder_PurchaseOrder> pos { get; set; }
    }

    public class ShippingOrder_Preload
    {
        public bool chk { get; set; }

        [DisplayName("No")]
        public int no { get; set; }

        [DisplayName("Order No")]
        public string order_no { get; set; }

        [DisplayName("Customer")]
        public int customer_id { get; set; }

        [DisplayName("Taker")]
        public string taker { get; set; }

        [DisplayName("Requested Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime requested_date { get; set; }

        [DisplayName("Expedite Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime expedite_date { get; set; }

        [DisplayName("PO No")]
        public int po_no { get; set; }

        [DisplayName("Required Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime required_date { get; set; }

        [DisplayName("Expected Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime expected_date { get; set; }

        [DisplayName("Expected Ship Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime expected_ship_date { get; set; }
    }

    public class ShippingOrder_PurchaseOrder
    {
        public bool chk { get; set; }

        [DisplayName("No")]
        public int no { get; set; }

        [DisplayName("PO No")]
        public int po_no { get; set; }

        [DisplayName("Vendor")]
        public int vendor_id { get; set; }

        [DisplayName("To")]
        public int location_id { get; set; }

        [DisplayName("Line No")]
        public int line_no { get; set; }

        [DisplayName("Part No")]
        public string item_id { get; set; }

        [DisplayName("Extended Desc")]
        public string extended_desc { get; set; }

        [DisplayName("Quantity")]
        [DisplayFormat(DataFormatString = "{0:N0}", ConvertEmptyStringToNull = true)]
        public decimal qty_ordered { get; set; }

        [DisplayName("Required Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime required_date { get; set; }

        [DisplayName("Expected Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime expected_date { get; set; }

        [DisplayName("Expected Ship Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime expected_ship_date { get; set; }
    }


    public class Transfer_ShipmentData
    {
        public List<Transfer_Shipment_hdr> hdr { get; set; }

        public List<Transfer_Shipment_line> line { get; set; }
    }
    public class Transfer_Shipment_hdr
    {
        [DisplayName("PT No")]
        public int pick_ticket_no { get; set; }

        [DisplayName("Order No")]
        public string order_no { get; set; }

        [DisplayName("Invoice No")]
        public string invoice_no { get; set; }

        [DisplayName("Vendor Name")]
        public string vendor_name { get; set; }

        [DisplayName("Vendor Address")]
        public string vendor_address { get; set; }

        [DisplayName("Vendor Phone")]
        public string vendor_phone { get; set; }

        [DisplayName("Ship to Name")]
        public string ship2_name { get; set; }

        [DisplayName("Ship to Address")]
        public string ship2_address { get; set; }

        [DisplayName("Ship to Phone")]
        public string ship_to_phone { get; set; }

        [DisplayName("Invoice Name")]
        public string invoice_name { get; set; }

        [DisplayName("Invoice Address")]
        public string invoice_address { get; set; }

        [DisplayName("Invoice Phone")]
        public string invoice_phone { get; set; }

        [DisplayName("Invoice Email")]
        public string invoice_email { get; set; }

        [DisplayName("Invoice Reference")]
        public string invoice_reference_no { get; set; }

        [DisplayName("Tracking No")]
        public string tracking_no { get; set; }

        [DisplayName("Transportation")]
        public string transportation { get; set; }

        [DisplayName("Currency")]
        public string currency { get; set; }

        [DisplayName("Freight")]
        public string freight_desc { get; set; }
    }
    public class Transfer_Shipment_line
    {
        [DisplayName("Supplier ID")]
        public int supplier_id { get; set; }

        [DisplayName("Supplier Name")]
        public string supplier_name { get; set; }

        [DisplayName("PO No")]
        public string po_no { get; set; }

        [DisplayName("Item ID")]
        public string item_id { get; set; }

        [DisplayName("Item Desc")]
        public string item_desc { get; set; }

        [DisplayName("Ship to Name")]
        public string ship2_name { get; set; }

        [DisplayName("Customer Part No")]
        public string customer_part_number { get; set; }

        [DisplayName("Order No")]
        public string order_no { get; set; }

        [DisplayName("HS Code")]
        public string hs_code { get; set; }

        [DisplayName("HS Code USA")]
        public string hs_code_usa { get; set; }

        [DisplayName("HS Code Canada")]
        public string hs_code_can { get; set; }

        [DisplayName("HS Code Mexico")]
        public string hs_code_mex { get; set; }

        [DisplayName("HS Code Taiwan")]
        public string hs_code_twn { get; set; }

        [DisplayName("HS Code Finland")]
        public string hs_code_fin { get; set; }

        [DisplayName("Material Type")]
        public string material_type { get; set; }

        [DisplayName("COO")]
        public string coo { get; set; }

        [DisplayName("Qty UOM_100")]
        [DisplayFormat(DataFormatString = "{0:N2}", ConvertEmptyStringToNull = true)]
        public decimal qty_uom_100 { get; set; }

        [DisplayName("Wgt Unit_KG")]
        [DisplayFormat(DataFormatString = "{0:N4}", ConvertEmptyStringToNull = true)]
        public decimal wgt_unit { get; set; }

        [DisplayName("Ext Wgt_100")]
        [DisplayFormat(DataFormatString = "{0:N4}", ConvertEmptyStringToNull = true)]
        public decimal ext_wgt_100 { get; set; }

        [DisplayName("Price UOM_100")]
        [DisplayFormat(DataFormatString = "{0:N4}", ConvertEmptyStringToNull = true)]
        public decimal price_uom_100 { get; set; }

        [DisplayName("UOM_100")]
        [DisplayFormat(DataFormatString = "{0:N0}", ConvertEmptyStringToNull = true)]
        public decimal uom_100 { get; set; }

        [DisplayName("Sum Ext Sell")]
        [DisplayFormat(DataFormatString = "{0:N2}", ConvertEmptyStringToNull = true)]
        public decimal sum_ext_sell { get; set; }

        [DisplayName("Pick Ticket")]
        public int pick_ticket_no { get; set; }

        [DisplayName("Line No")]
        public int line_number { get; set; }

        [DisplayName("OE Line No")]
        public int oe_line_no { get; set; }

        [DisplayName("Customer")]
        public int customer_id { get; set; }

        [DisplayName("Duty Rate Canada")]
        [DisplayFormat(DataFormatString = "{0:N4}", ConvertEmptyStringToNull = true)]
        public decimal duty_rate_can { get; set; }

        [DisplayName("Anti Dump Canada")]
        [DisplayFormat(DataFormatString = "{0:N4}", ConvertEmptyStringToNull = true)]
        public decimal anti_dump_can { get; set; }

        [DisplayName("Verify Canada")]
        public string ver_can { get; set; }

        [DisplayName("Verify USA")]
        public string ver_usa { get; set; }

        [DisplayName("Verify Mexico")]
        public string ver_mex { get; set; }

        [DisplayName("Verify Taiwan")]
        public string ver_twn { get; set; }

        [DisplayName("Verify Finland")]
        public string ver_fin { get; set; }

        [DisplayName("Print Quantity")]
        [DisplayFormat(DataFormatString = "{0:N0}", ConvertEmptyStringToNull = true)]
        public decimal print_quantity { get; set; }

        [DisplayName("Unit Price")]
        [DisplayFormat(DataFormatString = "{0:N4}", ConvertEmptyStringToNull = true)]
        public decimal unit_price { get; set; }

        [DisplayName("Unit Size")]
        [DisplayFormat(DataFormatString = "{0:N0}", ConvertEmptyStringToNull = true)]
        public decimal unit_size { get; set; }
    }


    public class Transfer_Pallet
    {
        [DisplayName("Pallet No")]
        public int pallet_no { get; set; }

        [DisplayName("Container")]
        public string container_cd { get; set; }
    }

    public class Transfer_PackingList
    {
        public string pick_ticket_no { get; set; }
        public string pm_usr { get; set; }
        public string pm_usr_name { get; set; }
        public List<Transfer_PackingList_hdr> packing_list_hdr { get; set; }
        public List<Transfer_PackingList_line> packing_list_line { get; set; }
        public Transfer_PackingList_po_mark packing_list_po_mark { get; set; }
        public Transfer_PackingList_sum packing_list_sum { get; set; }
    }
    public class Transfer_PackingList_hdr
    {
        [DisplayName("Pick Ticket No")]
        public int pick_ticket_no { get; set; }

        public int ship_to_id { get; set; }

        [DisplayName("MESSRS")]
        public string ship_to_name { get; set; }

        [DisplayName("Ship to Address")]
        public string ship_to_addr { get; set; }

        public int ship_from_id { get; set; }

        public string ship_from_name { get; set; }

        public string ship_from_country { get; set; }

        [DisplayName("From")]
        public string ship_from_address { get; set; }

        [DisplayName("Shipped Per")]
        public string shipped_per { get; set; }

        [DisplayName("Arrived on or about")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd}")]
        public DateTime requested_date { get; set; }

        [DisplayName("Port")]
        public string port { get; set; }

        [DisplayName("Final Des")]
        public string final_des { get; set; }

        [DisplayName("Sailing Notice No")]
        public string sailing_notice_no { get; set; }

        [DisplayName("Broker Name")]
        public string broker_name { get; set; }
    }
    public class Transfer_PackingList_line
    {
        [DisplayName("Pick Ticket No")]
        public int pick_ticket_no { get; set; }

        [DisplayName("PLT No")]
        public int pallet_no { get; set; }

        [DisplayName("PO No")]
        public string po_no { get; set; }

        [DisplayName("Lot No")]
        public string lot_cd { get; set; }

        [DisplayName("Item ID")]
        public string item_id { get; set; }

        [DisplayName("Description")]
        public string item_desc { get; set; }

        [DisplayName("Qty per Box")]
        [DisplayFormat(DataFormatString = "{0:N0}", ConvertEmptyStringToNull = true)]
        public int qty_per_box { get; set; }

        [DisplayName("Total CTN")]
        public int box { get; set; }

        [DisplayName("Quantity(PCS)")]
        [DisplayFormat(DataFormatString = "{0:N0}", ConvertEmptyStringToNull = true)]
        public int qty { get; set; }

        [DisplayName("Unit Weight")]
        [DisplayFormat(DataFormatString = "{0:N2}", ConvertEmptyStringToNull = true)]
        public decimal unit_weight { get; set; }

        [DisplayName("N.W./KGS")]
        [DisplayFormat(DataFormatString = "{0:N2}", ConvertEmptyStringToNull = true)]
        public decimal net { get; set; }

        [DisplayName("G.W./KGS")]
        [DisplayFormat(DataFormatString = "{0:N2}", ConvertEmptyStringToNull = true)]
        public decimal gross { get; set; }

        [DisplayName("Same Box No")]
        public int same_box_no { get; set; }

        public decimal unit_price { get; set; }

        [DisplayName("Container Code")]
        public string container_cd { get; set; }
    }
    public class Transfer_PackingList_po_mark
    {
        [DisplayName("PO Mark")]
        public string mark { get; set; }
    }
    public class Transfer_PackingList_sum
    {
        [DisplayName("Carton")]
        public int ctn { get; set; }

        [DisplayName("Quantity")]
        public int qty { get; set; }

        [DisplayName("Net Weight")]
        [DisplayFormat(DataFormatString = "{0:N4}", ConvertEmptyStringToNull = true)]
        public decimal net { get; set; }

        [DisplayName("Gross Weight")]
        [DisplayFormat(DataFormatString = "{0:N4}", ConvertEmptyStringToNull = true)]
        public decimal gross { get; set; }

        [DisplayName("Plate Weight")]
        [DisplayFormat(DataFormatString = "{0:N0}", ConvertEmptyStringToNull = true)]
        public int plt { get; set; }

        [DisplayName("KGS+PLT")]
        [DisplayFormat(DataFormatString = "{0:N4}", ConvertEmptyStringToNull = true)]
        public decimal kgs { get; set; }
    }


    public class Transfer_CommercialInvoice
    {
        public string inv_string { get; set; }
        public string pm_usr { get; set; }
        public Transfer_CI_hdr ci_hdr { get; set; }
        public List<Transfer_CI_line> ci_line { get; set; }
    }
    public class Transfer_CI_hdr
    {
        [DisplayName("Invoice No")]
        public string invoice_no { get; set; }
        //public string inv_string { get; set; }

        [DisplayName("Invoice Date")]
        //[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMMM-y}")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM-dd-yyyy}")]
        public DateTime invoice_date { get; set; }

        [DisplayName("Vendor")]
        public string vendor_id { get; set; }
        public string vendor_name { get; set; }
        public string vendor_address { get; set; }

        [DisplayName("Purchaser")]
        public string purchaser_id { get; set; }
        public string purchaser_name { get; set; }
        public string purchaser_address { get; set; }

        [DisplayName("Ship to")]
        public int ship2_id { get; set; }
        public string ship2_name { get; set; }
        public string ship2_address { get; set; }

        [DisplayName("Incoterm")]
        public string incoterm { get; set; }

        [DisplayName("Terms of Payment")]
        public string payment_terms { get; set; }

        [DisplayName("Currency")]
        public string currency { get; set; }

        [DisplayName("Carrier Name")]
        public string carrier_name { get; set; }

        [DisplayName("Tracking No")]
        public string tracking_no { get; set; }

        [DisplayName("AES Filling No")]
        public string aes_no { get; set; }

        //[DisplayName("Special Instructions")]
        [DisplayName("Instructions")]
        public string instructions { get; set; }

        [DisplayName("Total # of Boxes")]
        [DisplayFormat(DataFormatString = "{0:N0}", ConvertEmptyStringToNull = true)]
        public int summary_box { get; set; }

        [DisplayName("Total Gross Weight (KGS)")]
        [DisplayFormat(DataFormatString = "{0:N4}", ConvertEmptyStringToNull = true)]
        public decimal summary_gross { get; set; }

        [DisplayName("Net Weight (KGS)")]
        [DisplayFormat(DataFormatString = "{0:N4}", ConvertEmptyStringToNull = true)]
        public decimal summary_net { get; set; }

        [DisplayName("Invoice Total")]
        [DisplayFormat(DataFormatString = "{0:N2}", ConvertEmptyStringToNull = true)]
        public decimal summary_sell { get; set; }

        public string ship2_country { get; set; }
        public int pick_ticket_no { get; set; }
        public string order_no { get; set; }
    }
    public class Transfer_CI_line
    {
        public string inv_string { get; set; }

        public int i { get; set; }

        [DisplayName("CCAC")]
        public string ccac { get; set; }

        [DisplayName("Loc")]
        public int ship_to_id { get; set; }

        [DisplayName("PO No")]
        public string po_no { get; set; }

        [DisplayName("SBS Part Number")]
        public string item_id { get; set; }

        [DisplayName("Description")]
        public string item_desc { get; set; }

        [DisplayName("Customer Part Number")]
        public string customer_part_number { get; set; }

        [DisplayName("Origin HS")]
        public string origin_hs_code { get; set; }

        [DisplayName("Dest HS")]
        public string dest_hs_code { get; set; }

        [DisplayName("EUC/Annex HS")]
        public string annex_hs_code { get; set; }

        [DisplayName("Material")]
        public string material_type { get; set; }

        [DisplayName("COO")]
        public string coo { get; set; }

        [DisplayName("Boxes")]
        [DisplayFormat(DataFormatString = "{0:N0}", ConvertEmptyStringToNull = true)]
        public int subtotal_box { get; set; }

        [DisplayName("Qty/UOM")]
        [DisplayFormat(DataFormatString = "{0:N2}", ConvertEmptyStringToNull = true)]
        public decimal subtotal_qty { get; set; }

        [DisplayName("Wgt/un (Kgs)")]
        [DisplayFormat(DataFormatString = "{0:N4}", ConvertEmptyStringToNull = true)]
        public decimal unit_weight { get; set; }

        [DisplayName("Ext Wgt (Kgs)")]
        [DisplayFormat(DataFormatString = "{0:N2}", ConvertEmptyStringToNull = true)]
        public decimal subtotal_net { get; set; }

        [DisplayName("Sell Price/UOM")]
        [DisplayFormat(DataFormatString = "{0:N4}", ConvertEmptyStringToNull = true)]
        public decimal unit_price { get; set; }

        [DisplayName("U/M")]
        public int uom { get; set; }

        [DisplayName("Ext Sell")]
        [DisplayFormat(DataFormatString = "{0:N2}", ConvertEmptyStringToNull = true)]
        public decimal ext_sell { get; set; }

        //public int same_box_no { get; set; }
        public bool hasComponents { get; set; }
        public bool isComponent { get; set; }
        public int component_sequence { get; set; }
    }


    public class BuySheet_TransitTime_Country
    {
        public int group_count { get; set; }
        public int i { get; set; }

        [DisplayName("From Country")]
        public string from_country_id { get; set; }

        [DisplayName("From Country Name")]
        public string from_country_name { get; set; }

        [DisplayName("To Country")]
        public string to_country { get; set; }

        [DisplayName("To Location")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N0}", ConvertEmptyStringToNull = true)]
        public decimal to_loc_id { get; set; }

        [DisplayName("To Location Name")]
        public string to_loc_name { get; set; }

        //[DisplayName("Transit Days")]
        [DisplayName("Days")]
        public int transit_days { get; set; }

        [DisplayName("Ship Method")]
        public string ship_method { get; set; }
    }
    public class BuySheet_TransitTime_Country_Method
    {
        public List<BuySheet_TransitTime_Country> Ocean { get; set; }
        public List<BuySheet_TransitTime_Country> Air { get; set; }
    }


    public class BuySheet_TransitTime_Location
    {
        public int group_count { get; set; }
        public int i { get; set; }

        [DisplayName("From Location ID")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N0}", ConvertEmptyStringToNull = true)]
        public decimal from_loc_id { get; set; }

        [DisplayName("From Location Name")]
        public string from_loc_name { get; set; }

        [DisplayName("To Location ID")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N0}", ConvertEmptyStringToNull = true)]
        public decimal to_loc_id { get; set; }

        [DisplayName("To Location Name")]
        public string to_loc_name { get; set; }

        [DisplayName("Transit Days")]
        public int transit_days { get; set; }
    }


    public class BuySheet_TransitTime_DefaultAir
    {
        [DisplayName("Item ID")]
        public string item_id { get; set; }

        [DisplayName("Description")]
        public string item_desc { get; set; }

        [DisplayName("From Country")]
        public string from_country_id { get; set; }

        [DisplayName("From Country Name")]
        public string from_country_name { get; set; }

        [DisplayName("To Location")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N0}", ConvertEmptyStringToNull = true)]
        public decimal to_loc_id { get; set; }

        [DisplayName("To Location Name")]
        public string to_loc_name { get; set; }

        public string z_status { get; set; }
    }

}
