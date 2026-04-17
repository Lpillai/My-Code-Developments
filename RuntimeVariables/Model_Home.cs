using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuntimeVariables
{

    #region Home

    public class ActionLog
    {
        public string keys { get; set; }
        public string field_name { get; set; }
        public string old_value { get; set; }
        public string new_value { get; set; }
    }


    public class ViewBagPageInfo
    {
        public string Title { get; set; }
        public string PageName { get; set; }
        public string Path { get; set; }
    }


    public class theLatestExchangeRate
    {
        [DisplayName("Currency ID")]
        public int currency_id { get; set; }

        [DisplayName("Rates")]
        //[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N2}")]
        public decimal exchange_rate { get; set; }
    }


    public class Currency
    {
        public int grp { get; set; }
        public int rrn { get; set; }

        [DisplayName("Exchange Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd}")]
        public DateTime exchange_date { get; set; }

        [DisplayName("Currency ID")]
        public int to_currency_cd { get; set; }

        [DisplayName("Currency Name")]
        public string to_currency_name { get; set; }

        [DisplayName("Rates")]
        //[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N2}")]
        public float exchange_rate { get; set; }
    }


    public class CodeCluster
    {
        public int no { get; set; }

        public string Cluster { get; set; }
    }


    public class CodeStore
    {
        public int no { get; set; }

        public string Cluster { get; set; }

        public int Code { get; set; }

        public string Value { get; set; }

        [DisplayName("Notes")]
        public string z_memo { get; set; }

        [DisplayName("Status")]
        public string z_status { get; set; }

        [DisplayName("Create Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd}")]
        public DateTime z_createDate { get; set; }

        [DisplayName("Latest Modifying Program")]
        public string z_pgm { get; set; }

        [DisplayName("Latest Modifying Account")]
        public string z_usr { get; set; }

        [DisplayName("Latest Modifying Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd}")]
        public DateTime z_updateDate { get; set; }
    }


    public class CodeValue
    {
        public int no { get; set; }

        public string Code { get; set; }

        public string Value { get; set; }
    }


    public class Location
    {
        public string company_id { get; set; }

        public string location_id { get; set; }

        public string location_name { get; set; }

        public string id_name { get; set; }
    }


    public class Country
    {
        public string country_code { get; set; }

        public string country_name { get; set; }

        public int dail_code { get; set; }
    }


    public class Filters
    {
        public string key { get; set; }

        public string key_name { get; set; }

        public string op { get; set; }

        public string value_type { get; set; }

        public string value_s { get; set; }

        public string value_e { get; set; }
    }


    public class Sorts
    {
        public int order_no { get; set; }

        public string field_name { get; set; }

        public bool descend { get; set; }
    }


    public class SP_Return
    {
        public int r { get; set; }

        public string msg { get; set; }

        public string JsonData { get; set; }
    }

    #endregion

    #region Login

    public class Login
    {
        [DisplayName("No.")]
        public int no { get; set; }

        [DisplayName("Id")]
        public int menu_id { get; set; }

        [DisplayName("Menu Name")]
        public string menu_name { get; set; }

        [DisplayName("Parent")]
        public int parent_menu { get; set; }

        [DisplayName("Controller")]
        public string folder { get; set; }

        [DisplayName("Action")]
        public string run { get; set; }

        [DisplayName("Parameters")]
        public string param { get; set; }

        [DisplayName("From")]
        public string kind { get; set; }

        [DisplayName("Sort")]
        public string menu_sort { get; set; }

        public string menu_path { get; set; }
    }


    public class LoginTreeItem
    {
        public int parent_id { get; set; }

        public int menu_id { get; set; }

        public string menu_name { get; set; }

        public string folder { get; set; }

        public string run { get; set; }

        public string param { get; set; }

        //public string path { get; set; }

        public List<LoginTreeItem> child_menu { get; set; }

        public string kind { get; set; }
    }


    public class Profile
    {
        [DisplayName("Account")]
        public string id { get; set; }

        [DisplayName("First Name")]
        public string first_name { get; set; }

        [DisplayName("Last Name")]
        public string last_name { get; set; }

        [DisplayName("Email")]
        public string email { get; set; }

        [DisplayName("Area")]
        public string area { get; set; }

        [DisplayName("Password expired date")]
        public DateTime expired { get; set; }
    }

    #endregion

}
