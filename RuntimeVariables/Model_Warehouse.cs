using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuntimeVariables
{
    public class palletizing_record
    {
        [DisplayName("No")]
        public int rrn { get; set; }

        public int line_number { get; set; }

        [DisplayName("Lot")]
        public string lot_cd { get; set; }

        [DisplayName("Item ID")]
        public string item_id { get; set; }

        [DisplayName("Qty/Box")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int qty_per_box { get; set; }

        [DisplayName("Total")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int total_boxes { get; set; }

        [DisplayName("Pallet")]
        public string pallet_no { get; set; }
    }

    public class palletizing_record2
    {
        public int palletizing_no { get; set; }
        public int line_number { get; set; }
        public int line_seq { get; set; }
        public int oe_line_no { get; set; }
        public int inv_mast_uid { get; set; }
        public string item_id { get; set; }
        public string customer_part_number { get; set; }
        public int print_quantity { get; set; }
        public string lot_cd { get; set; }
        public string country_of_origin { get; set; }
        public string bin_id { get; set; }
        public int qty_allocated_delta { get; set; }
        public int qty_per_box { get; set; }
        public int total_boxes { get; set; }
        public int same_box_no { get; set; }
        public string pallet_no { get; set; }
        public int bin_seq { get; set; }
    }

    public class palletizing_record2_line
    {
        [DisplayName("Line")]
        public int line_number { get; set; }

        public int oe_line_no { get; set; }

        public int inv_mast_uid { get; set; }

        [DisplayName("Item ID")]
        public string item_id { get; set; }

        [DisplayName("Customer Part No")]
        public string customer_part_number { get; set; }

        [DisplayName("Quantity")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int print_quantity { get; set; }

        [DisplayName("Person")]
        public string packing_person { get; set; }

        public List<palletizing_record2_bin> bins { get; set; }
    }
    public class palletizing_record2_bin
    {
        public int palletizing_no { get; set; }

        public int line_seq { get; set; }

        [DisplayName("Lot")]
        public string lot_cd { get; set; }

        [DisplayName("CO")]
        public string country_of_origin { get; set; }

        [DisplayName("Bin")]
        public string bin_id { get; set; }

        [DisplayName("Allocated")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int qty_allocated_delta { get; set; }

        [DisplayName("Qty/Box")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int qty_per_box { get; set; }

        [DisplayName("Total")]
        public int total_boxes { get; set; }

        [DisplayName("Same Box No")]
        public int same_box_no { get; set; }

        [DisplayName("Pallet")]
        public string pallet_no { get; set; }

        public int bin_seq { get; set; }

        public int summary { get; set; }
    }


    public class palletizing_data
    {
        public int pick_ticket_no { get; set; }
        public palletizing_hdr hdr { get; set; }
        public List<palletizing_detail> detail { get; set; }
        public List<palletizing_summary1> summary1 { get; set; }
        public palletizing_summary2 summary2 { get; set; }
    }

    public class palletizing_hdr
    {
        [DisplayName("Checking Manager")]
        public string checking_manager { get; set; }

        [DisplayName("Checking Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd hh:mm:ss}", ConvertEmptyStringToNull = true)]
        public DateTime? checking_date { get; set; }

        [DisplayName("Order No")]
        public string order_no { get; set; }

        [DisplayName("PO No")]
        public string po_no { get; set; }

        [DisplayName("Invoice No")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int invoice_no { get; set; }

        [DisplayName("Customer ID")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int customer_id { get; set; }

        [DisplayName("Delivery Instructions")]
        public string delivery_instructions { get; set; }
    }

    public class palletizing_detail
    {
        [DisplayName("No")]
        public int line_number { get; set; }

        [DisplayName("Packing Person")]
        public string packing_person { get; set; }

        [DisplayName("Item ID")]
        public string item_id { get; set; }

        [DisplayName("Print Quantity")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal print_quantity { get; set; }

        public List<palletizing_lot> lots { get; set; }
    }

    public class palletizing_lot
    {
        [DisplayName("Lot")]
        public string lot_cd { get; set; }

        [DisplayName("Bin")]
        public string bin_cd { get; set; }

        [DisplayName("Bin Qty")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal qty_allocated_delta { get; set; }

        public List<palletizing_pallet> plts { get; set; }
    }

    public class palletizing_pallet
    {
        [DisplayName("Qty/Box")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int qty_per_box { get; set; }

        [DisplayName("Total Boxes")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int total_boxes { get; set; }

        [DisplayName("Pallet No")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int pallet_no { get; set; }
    }

    public class palletizing_summary1
    {
        [DisplayName("Pallet No")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int pallet_no { get; set; }

        [DisplayName("Subtotal Quantity")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int sub_qty { get; set; }

        [DisplayName("Subtotal Boxes")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int sub_boxes { get; set; }
    }

    public class palletizing_summary2
    {
        [DisplayName("Subtotal Quantity")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int sum_qty { get; set; }

        [DisplayName("Subtotal Boxes")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int sum_boxes { get; set; }
    }

}
