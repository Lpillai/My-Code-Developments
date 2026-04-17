using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuntimeVariables
{

    public class AccountItem
    {
        [DisplayName("No")]
        public int no { get; set; }

        [DisplayName("Account")]
        [Required]
        [StringLength(20)]
        public string id { get; set; }

        [DisplayName("First Name")]
        [Required]
        [StringLength(20)]
        public string first_name { get; set; }

        [DisplayName("Last Name")]
        [Required]
        [StringLength(20)]
        public string last_name { get; set; }

        [DisplayName("Email")]
        [Required]
        [StringLength(40)]
        public string email { get; set; }

        [DisplayName("Ext Number")]
        public int ext { get; set; }

        [DataType(DataType.Date)]
        [DisplayName("Expired Date")]
        //[Required]
        //[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}", ConvertEmptyStringToNull = true, NullDisplayText = "")]
        public DateTime expired { get; set; }

        [DisplayName("Area")]
        [Required]
        public int area { get; set; }

        [DisplayName("Memo")]
        public string z_memo { get; set; }

        [DisplayName("Disable")]
        [StringLength(1)]
        public string z_status { get; set; }

        [DisplayName("Last Updated Date")]
        public DateTime z_date { get; set; }

        [DisplayName("Last Updated Program")]
        public string z_pgm { get; set; }

        [DisplayName("Last Updated Account")]
        public string z_usr { get; set; }

        //---------------------

        public bool account_chk { get; set; }

        [DisplayName("Full Name")]
        public string full_name { get; set; }
    }


    public class MenuData
    {
        public List<MenuTreeItem> theItem { get; set; }

        public List<MenuItem> theList { get; set; }
    }

    public class MenuTreeItem
    {
        [DisplayName("Menu ID")]
        public int menu_id { get; set; }

        [DisplayName("Menu Name")]
        public string menu_name { get; set; }

        public List<MenuTreeItem> child_menu { get; set; }

        [DisplayName("Sort")]
        public string menu_sort { get; set; }

        [DisplayName("Status")]
        public string z_status { get; set; }

        [DisplayName("Read Only")]
        public bool chk_read { get; set; }
    }

    public class MenuItem
    {
        [DisplayName("No")]
        public int no { get; set; }

        [DisplayName("Menu ID")]
        [Required]
        public int menu_id { get; set; }

        [DisplayName("Menu Name")]
        [StringLength(40)]
        public string menu_name { get; set; }

        [DisplayName("Parent Menu ID")]
        [Required]
        public int parent_menu { get; set; }

        [DisplayName("Parent Name")]
        public string parent_menu_name { get; set; }

        [DisplayName("Controller")]
        [StringLength(40)]
        public string folder { get; set; }

        [DisplayName("Action")]
        [StringLength(40)]
        public string run { get; set; }

        [DisplayName("Parameters")]
        [StringLength(255)]
        public string param { get; set; }

        [DisplayName("Sort")]
        [Required]
        [StringLength(5)]
        public string menu_sort { get; set; }

        [DisplayName("Memo")]
        public string z_memo { get; set; }

        [DisplayName("Status")]
        [StringLength(1)]
        public string z_status { get; set; }

        [DisplayName("Last Updated Date")]
        public DateTime z_date { get; set; }

        [DisplayName("Last Updated Program")]
        public string z_pgm { get; set; }

        [DisplayName("Last Updated Account")]
        public string z_usr { get; set; }

        //----------

        [DisplayName("isFolder")]
        public bool isFolder { get; set; }

        public bool menu_chk { get; set; }
        [DisplayName("Read Only")]
        public bool menu_chk_read { get; set; }
    }

    
    public class ReportItem
    {
        [DisplayName("No")]
        public int no { get; set; }

        [DisplayName("Report ID")]
        [Required]
        public int report_id { get; set; }

        [DisplayName("Cluster")]
        public string cluster { get; set; }

        [DisplayName("Report Name")]
        [StringLength(50)]
        public string report_name { get; set; }

        [DisplayName("Action")]
        [StringLength(40)]
        public string run { get; set; }

        [DisplayName("Parameters")]
        [StringLength(255)]
        public string param { get; set; }

        [DisplayName("Output")]
        [Required]
        public string output { get; set; }

        [DisplayName("SSRS ID")]
        public string ssrs_id { get; set; }

        [DisplayName("Memo")]
        public string z_memo { get; set; }

        [DisplayName("Status")]
        [StringLength(1)]
        public string z_status { get; set; }

        [DisplayName("Last Updated Date")]
        public DateTime z_date { get; set; }

        [DisplayName("Last Updated Program")]
        public string z_pgm { get; set; }

        [DisplayName("Last Updated Account")]
        public string z_usr { get; set; }

        //----------

        public bool rpt_chk { get; set; }
        [DisplayName("Read Only")]
        public bool rpt_chk_read { get; set; }
    }
    

    public class AuthorityData
    {
        public List<GroupItem> distinctPermissionList { get; set; }

        public List<PermissionItem> distinctIndividualList { get; set; }

        public List<GroupItem> distinctGroupList { get; set; }
    }


    public class PermissionItem
    {
        [DisplayName("Group ID")]
        public int group_id { get; set; }

        [DisplayName("Group Name")]
        [StringLength(50)]
        public string group_name { get; set; }

        [DisplayName("Account ID")]
        [StringLength(20)]
        public string account_id { get; set; }

        [DisplayName("Menu ID")]
        [Required]
        public int menu_id { get; set; }

        [DisplayName("Menu Name")]
        public string menu_name { get; set; }

        [DisplayName("Kind")]
        [StringLength(1)]
        public string kind { get; set; }

        [DisplayName("Read Only")]
        public bool read_only { get; set; }

        public string z_status { get; set; }
    }

    public class PermissionData
    {
        public List<PermissionItem> permissionList { get; set; }

        public List<ReportPermissionItem> ReportPermissionList { get; set; }

        public List<MenuItem> menuList { get; set; }

        public List<ReportItem> reportList { get; set; }
    }
    
    public class ReportPermissionItem
    {
        [DisplayName("Group ID")]
        public int group_id { get; set; }

        [DisplayName("Group Name")]
        [StringLength(50)]
        public string group_name { get; set; }

        [DisplayName("Account ID")]
        [StringLength(20)]
        public string account_id { get; set; }

        [DisplayName("Report ID")]
        [Required]
        public int report_id { get; set; }

        [DisplayName("Report Name")]
        public string report_name { get; set; }

        [DisplayName("Kind")]
        [StringLength(1)]
        public string kind { get; set; }

        [DisplayName("Read Only")]
        public bool read_only { get; set; }

        public string z_status { get; set; }
    }
    

    public class GroupData
    {
        public GroupItem group { get; set; }

        public List<GroupItem> memberList { get; set; }

        public List<AccountItem> accountList { get; set; }
    }

    public class GroupItem
    {
        [DisplayName("No")]
        public int no { get; set; }

        [DisplayName("Group ID")]
        [Required]
        public int group_id { get; set; }

        [DisplayName("Group Name")]
        [StringLength(50)]
        public string group_name { get; set; }

        [DisplayName("Account ID")]
        [Required]
        [StringLength(20)]
        public string account_id { get; set; }

        [DisplayName("Memo")]
        public string z_memo { get; set; }

        [DisplayName("Status")]
        [StringLength(1)]
        public string z_status { get; set; }

        [DisplayName("Last Updated Date")]
        public DateTime z_date { get; set; }

        [DisplayName("Last Updated Program")]
        public string z_pgm { get; set; }

        [DisplayName("Last Updated Account")]
        public string z_usr { get; set; }
    }


    public class QueryData
    {
        public List<MenuTreeItem> theMenus { get; set; }

        public List<ReportItem> theReports { get; set; }
    }

}
