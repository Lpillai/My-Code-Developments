using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;

namespace RuntimeVariables
{
    public class Local_P21_ID
    {
        [DisplayName("ID")]
        public int address_id { get; set; }

        [DisplayName("Name")]
        public string address_name { get; set; }
    }

    public class Local_Information
    {
        [DisplayName("P21 Address ID")]
        public int address_id { get; set; }

        [DisplayName("P21 Address Name")]
        public string address_name { get; set; }

        public List<Local_Address> theAddress { get; set; }

        public List<Local_Contact> theContact { get; set; }
    }
    public class Local_Address
    {
        [DisplayName("UID")]
        public int address_uid { get; set; }

        [DisplayName("P21 ID")]
        public int p21_id { get; set; }

        [DisplayName("Type")]
        public string address_type { get; set; }

        [DisplayName("Type")]
        public string type_val { get; set; }

        [DisplayName("Language")]
        public string address_language { get; set; }

        [DisplayName("Language")]
        public string lan_val { get; set; }

        [DisplayName("Address Name")]
        public string address_name { get; set; }

        [DisplayName("Address Context")]
        public string address_context { get; set; }

        [DisplayName("Zip")]
        public string address_zip { get; set; }

        [DisplayName("Country")]
        public string address_country { get; set; }

        [DisplayName("Tax ID")]
        public string tax_id { get; set; }

        [DisplayName("Delete")]
        public bool chk_deleted { get; set; } = false;
    }
    public class Local_Contact
    {
        [DisplayName("UID")]
        public int contact_uid { get; set; }

        [DisplayName("P21 ID")]
        public int p21_id { get; set; }

        [DisplayName("Type")]
        public string contact_type { get; set; }

        [DisplayName("Type")]
        public string type_val { get; set; }

        [DisplayName("Language")]
        public string contact_language { get; set; }

        [DisplayName("Language")]
        public string lan_val { get; set; }

        [DisplayName("Contact Name")]
        public string contact_name { get; set; }

        [DisplayName("Title")]
        public string contact_title { get; set; }

        [DisplayName("Phone")]
        public string contact_phone { get; set; }

        [DisplayName("Fax")]
        public string contact_fax { get; set; }

        [DisplayName("eMail")]
        public string contact_mail { get; set; }

        [DisplayName("Delete")]
        public bool chk_deleted { get; set; } = false;
    }


    public class AR_Item
    {
        [DisplayName("Invoice Period")]
        public int invoice_period { get; set; }

        [DisplayName("Invoice Date")]
        //[DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd}")]
        public DateTime invoice_date { get; set; }

        [DisplayName("Location")]
        public int location_id { get; set; }

        [DisplayName("Taker Group")]
        public string taker_group { get; set; }

        [DisplayName("Taker")]
        public string taker { get; set; }

        [DisplayName("Customer")]
        public int customer_id { get; set; }

        [DisplayName("Customer Name")]
        public string customer_name { get; set; }

        [DisplayName("Invoice No")]
        [Required]
        public int invoice_no { get; set; }

        [DisplayName("PO No")]
        public string po_no { get; set; }

        [DisplayName("Total Amount")]
        [DisplayFormat(DataFormatString = "{0:N2}", ConvertEmptyStringToNull = true)]
        public decimal total_amount { get; set; }

        [DisplayName("Currency")]
        public int currency_id { get; set; }

        [DisplayName("Exchange Rate")]
        [DisplayFormat(DataFormatString = "{0:N4}", ConvertEmptyStringToNull = true, NullDisplayText = "")]
        public decimal exchange_rate { get; set; }

        [DisplayName("Paid Date")]
        //[DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd}")]
        public DateTime default_paid_date { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd}")]
        [JsonIgnore]
        public DateTime override_paid_date { get; set; }

        [DisplayName("GUI Number")]
        [StringLength(20)]
        public string gui { get; set; } = "";

        [DisplayName("Memo")]
        public string z_memo { get; set; } = "";
        
        [DisplayName("Archive")]
        public string z_status { get; set; } = "";

        [DisplayName("Progress")]
        public int progress { get; set; }

        public string progress_name { get; set; }

        [DisplayName("Attachment")]
        public int att_count { get; set; } = 0;
    }
    public class AR_Detail
    {
        [DisplayName("No")]
        public int line_no { get; set; }

        [DisplayName("Customer Part Number")]
        public string customer_part_number { get; set; }

        [DisplayName("Item ID")]
        public string item_id { get; set; }

        [DisplayName("Item Desc")]
        public string item_desc { get; set; }

        [DisplayName("Unit Price")]
        [DisplayFormat(DataFormatString = "{0:N4}", ConvertEmptyStringToNull = true)]
        public decimal unit_price { get; set; }

        [DisplayName("Qty Shipped")]
        [DisplayFormat(DataFormatString = "{0:N0}", ConvertEmptyStringToNull = true)]
        public int qty_shipped { get; set; }

        [DisplayName("Unit")]
        public string unit_of_measure { get; set; }

        [DisplayName("Extended Price")]
        [DisplayFormat(DataFormatString = "{0:N2}", ConvertEmptyStringToNull = true)]
        public decimal extended_price { get; set; }

        [DisplayName("Order")]
        public string oe { get; set; }
    }


    public class AP_Item
    {
        [DisplayName("Year Period")]
        public int year_period { get; set; }

        [DisplayName("Location")]
        public int location_id { get; set; }

        [DisplayName("Receipt No")]
        public int receipt_number { get; set; }

        [DisplayName("Receipt Stamp")]
        public string receipt_stamp { get; set; }

        [DisplayName("PO No")]
        public int po_number { get; set; }

        [DisplayName("Lines")]
        [DisplayFormat(DataFormatString = "{0:N0}", ConvertEmptyStringToNull = true)]
        public int lines { get; set; }

        [DisplayName("Currency")]
        public int currency_id { get; set; }

        [DisplayName("Exchange Rate")]
        [DisplayFormat(DataFormatString = "{0:N4}", ConvertEmptyStringToNull = true, NullDisplayText = "")]
        public decimal exchange_rate { get; set; }

        [DisplayName("Ext. Price")]
        [DisplayFormat(DataFormatString = "{0:N2}", ConvertEmptyStringToNull = true)]
        public decimal value_display { get; set; }

        [DisplayName("Ext. Price USD")]
        [DisplayFormat(DataFormatString = "{0:N2}", ConvertEmptyStringToNull = true)]
        public decimal value_in_usd { get; set; }

        [DisplayName("Terms")]
        public string terms { get; set; }

        [DisplayName("Terms Desc")]
        public string terms_desc { get; set; }

        [DisplayName("Supplier")]
        public int supplier_id { get; set; }

        [DisplayName("Supplier Name")]
        public string supplier_name { get; set; }

        [DisplayName("Buyer")]
        public int buyer_id { get; set; }

        [DisplayName("Buyer Name")]
        public string buyer_name { get; set; }

        [DisplayName("Default Paid Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd}")]
        public DateTime default_paid_date { get; set; }

        [JsonIgnore]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd}")]
        public DateTime override_paid_date { get; set; }

        [DisplayName("Invoice No")]
        [StringLength(10)]
        [DisplayFormat(DataFormatString = "{0:N0}", ConvertEmptyStringToNull = true)]
        public int invoice_no { get; set; }

        [DisplayName("Invoice Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd}")]
        public DateTime invoice_date { get; set; }

        [DisplayName("GUI No")]
        [StringLength(20)]
        public string gui { get; set; } = "";

        [DisplayName("Memo")]
        public string z_memo { get; set; } = "";

        [DisplayName("Archive")]
        public string z_status { get; set; } = "";

        public bool chk_archive { get; set; } = false;
    }
    public class AP_Detail
    {
        [DisplayName("No")]
        public int line_number { get; set; }

        [DisplayName("Item ID")]
        public string item_id { get; set; }

        [DisplayName("Qty Received")]
        [DisplayFormat(DataFormatString = "{0:N0}", ConvertEmptyStringToNull = true)]
        public int qty_received { get; set; }

        [DisplayName("Unit")]
        public string unit_of_measure { get; set; }

        [DisplayName("Unit Price")]
        [DisplayFormat(DataFormatString = "{0:N4}", ConvertEmptyStringToNull = true)]
        public decimal unit_cost { get; set; }

        [DisplayName("Extended Price")]
        [DisplayFormat(DataFormatString = "{0:N2}", ConvertEmptyStringToNull = true)]
        public decimal extended_cost_display { get; set; }

        [DisplayName("PO")]
        public string po { get; set; }
    }


    public class AccountingAttachment
    {
        public int attach_uid { get; set; }

        public string accounting_type { get; set; }

        public int accounting_uid { get; set; }

        [DisplayName("File Name")]
        public string attach_name { get; set; }

        public string attach_ext { get; set; }

        public string attach_path { get; set; }

        [JsonIgnore]
        public HttpPostedFileBase attachedFile { get; set; }

        public byte[] attach_file { get; set; }

        [DisplayName("Tag")]
        public string attach_tag { get; set; }

        [DisplayName("Tag")]
        public List<string> tags { get; set; }
        //public string tags { get; set; }

        public string z_status { get; set; }
    }

}
