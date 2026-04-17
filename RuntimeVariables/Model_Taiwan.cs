using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace RuntimeVariables
{
    public class tw_employee
    {
        [DisplayName("帳號")]
        public string account_id { get; set; }

        [DisplayName("名")]
        public string first_name { get; set; }

        [DisplayName("姓")]
        public string last_name { get; set; }

        [DisplayName("英文名")]
        public string full_name { get; set; }

        [DisplayName("到職日")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd}")]
        public DateTime onboard_date { get; set; }

        [DisplayName("部門")]
        public string dept { get; set; }

        [DisplayName("主管帳號")]
        public string manager_id { get; set; }

        [DisplayName("私人信箱")]
        public string personal_email { get; set; }

        [DisplayName("SBS信箱")]
        public string SBS_email { get; set; }

        [DisplayName("班別")]
        public int shift { get; set; }

        [DisplayName("卡片序號")]
        public string card_cd { get; set; }

        [DisplayName("特休啟算日")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd}")]
        public DateTime annual_start { get; set; }

        [DisplayName("出生日期")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd}")]
        public DateTime birthday { get; set; }

        [DisplayName("一")]
        public string emergency_name1 { get; set; }

        public string emergency_contact1 { get; set; }

        [DisplayName("二")]
        public string emergency_name2 { get; set; }

        public string emergency_contact2 { get; set; }

        [DisplayName("停用")]
        public string z_status { get; set; }

        public string z_usr { get; set; }
    }


    public class tw_LeaveSetting
    {
        public int day_id { get; set; }
        public string day_name { get; set; }
        public Boolean attach_required { get; set; }
        public string z_memo { get; set; }
    }


    public class tw_LeaveTaking
    {
        public List<tw_CardStamp> theCards { get; set; }
        public List<tw_day_apply> theApplys { get; set; }
        public List<tw_day_static> theStatics { get; set; }
        public List<tw_day_off_today> DayOffToday { get; set; }
        public List<tw_day_available> theAvailable { get; set; }
    }
    public class tw_day_apply
    {
        [DisplayName("單號")]
        //[DisplayFormat(DataFormatString = "{0:N0}", ConvertEmptyStringToNull = true, NullDisplayText = "")]
        [DisplayFormat(ConvertEmptyStringToNull = true, NullDisplayText = "")]
        public int apply_uid { get; set; }

        [DisplayName("帳號")]
        public string account_id { get; set; }

        [DisplayName("歸屬年度")]
        public int day_year { get; set; }

        public int day_id { get; set; }

        [DisplayName("假別")]
        public string day_name { get; set; }

        [DisplayName("起始時間")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd HH:mm}")]
        public DateTime day_start { get; set; }

        [DisplayName("截止時間")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd HH:mm}")]
        public DateTime day_end { get; set; }

        [DisplayName("請休時數")]
        [DisplayFormat(DataFormatString = "{0:N1}", ConvertEmptyStringToNull = true, NullDisplayText = "0")]
        public decimal day_hours { get; set; }

        [DisplayName("事由")]
        public string day_reason { get; set; }

        [DisplayName("Attached")]
        public bool attach_added { get; set; }

        public int? attach_uid { get; set; }

        [DisplayName("附件")]
        public string attachment { get; set; }

        [JsonIgnore]
        public HttpPostedFileBase attachedFile { get; set; }

        [DisplayName("Manager Approval")]
        public int approval_manager { get; set; }

        [DisplayName("HR Approval")]
        public int approval_hr { get; set; }

        [Display(Name = "Steps")]
        public int steps { get; set; }

        [DisplayName("Status")]
        public string sts { get; set; }

        [DisplayName("Memo")]
        public string z_memo { get; set; }

        [DisplayName("刪除")]
        public string z_status { get; set; }
        [JsonIgnore]
        public bool z_status_chk { get; set; }

        public string z_usr { get; set; }
    }
    public class tw_day_static
    {
        public int day_id { get; set; }

        [DisplayName("假別")]
        public string day_name { get; set; }

        [DisplayName("總計時數")]
        [DisplayFormat(DataFormatString = "{0:N1}", ConvertEmptyStringToNull = true, NullDisplayText = "0")]
        public decimal sumDays { get; set; }
    }
    public class tw_day_off_today
    {
        [DisplayName("就是這個人")]
        public string name { get; set; }

        [DisplayName("起始時間")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd HH:mm}")]
        public DateTime day_start { get; set; }

        [DisplayName("截止時間")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd HH:mm}")]
        public DateTime day_end { get; set; }
    }
    public class tw_day_available
    {
        public int day_id { get; set; }

        public string account_id { get; set; }

        [DisplayName("假別")]
        public string day_name { get; set; }

        [DisplayName("歸屬年度")]
        public int day_year { get; set; }

        [DisplayName("可用起始日")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd}")]
        public DateTime available_start { get; set; }

        [DisplayName("可用截止日")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd}")]
        public DateTime available_end { get; set; }

        [DisplayName("可用時數")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N1}", ConvertEmptyStringToNull = true, NullDisplayText = "0")]
        public decimal available_hours { get; set; }

        [DisplayName("備註")]
        public string z_memo { get; set; }

        [DisplayName("刪除")]
        public string z_status { get; set; }
        [JsonIgnore]
        public bool z_status_chk { get; set; }

        public string z_usr { get; set; }
    }


    public class tw_day_HR_apply
    {
        [DisplayName("歸屬年度")]
        public int day_year { get; set; }

        [DisplayName("單號")]
        [DisplayFormat(ConvertEmptyStringToNull = true, NullDisplayText = "")]
        public int apply_uid { get; set; }

        [DisplayName("姓名")]
        public string full_name { get; set; }

        [DisplayName("假別")]
        public string day_name { get; set; }

        [DisplayName("起始時間")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd HH:mm}")]
        public DateTime day_start { get; set; }

        [DisplayName("截止時間")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd HH:mm}")]
        public DateTime day_end { get; set; }

        [DisplayName("時數")]
        [DisplayFormat(DataFormatString = "{0:N1}", ConvertEmptyStringToNull = true, NullDisplayText = "0")]
        public decimal day_hours { get; set; }

        [DisplayName("事由")]
        public string day_reason { get; set; }

        public int steps { get; set; }

        [DisplayName("Status")]
        public string sts { get; set; }

        [DisplayName("Attached")]
        public bool attach_added { get; set; }

        public int? attach_uid { get; set; }

        [DisplayName("附件")]
        public string attachment { get; set; }
    }


    public class tw_LeaveAttachment
    {
        public int apply_uid { get; set; }
        public byte[] attach_file { get; set; }
        public string attach_name { get; set; }
        public string attach_ext { get; set; }
    }


    public class tw_LeaveApproval
    {
        public int apply_uid { get; set; }

        public string account_id { get; set; }

        [DisplayName("姓名Name")]
        public string emp_name { get; set; }

        [DisplayName("歸屬年度 Year")]
        public int day_year { get; set; }

        public int day_id { get; set; }

        [DisplayName("假別 Type")]
        public string day_name { get; set; }

        [DisplayName("起始時間 Start Time")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd HH:mm}")]
        public DateTime day_start { get; set; }

        [DisplayName("截止時間 End Time")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd HH:mm}")]
        public DateTime day_end { get; set; }

        [DisplayName("請休時數 Hours")]
        [DisplayFormat(DataFormatString = "{0:N1}", ConvertEmptyStringToNull = true, NullDisplayText = "0")]
        public decimal day_hours { get; set; }

        [DisplayName("事由 Reason")]
        public string day_reason { get; set; }

        [DisplayName("附件 Attachment")]
        public bool attach_added { get; set; }

        public int attach_uid { get; set; }

        public decimal av_hours { get; set; }
    }


    public class tw_Calendar
    {
        [DisplayName("日期")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd}")]
        public DateTime holiday { get; set; }

        [DisplayName("起始時間")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:HH:mm}", NullDisplayText = "")]
        public DateTime? start_time { get; set; }

        [DisplayName("結束時間")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:HH:mm}", NullDisplayText = "")]
        public DateTime? end_time { get; set; }

        [DisplayName("備註原因")]
        public string z_memo { get; set; }

        public string z_status { get; set; }
        public string z_usr { get; set; }
    }


    public class tw_CardStamp
    {
        [DisplayName("卡號")]
        public string card_cd { get; set; }

        [DisplayName("打卡時間")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd HH:mm}")]
        public DateTime card_stamp { get; set; }

        [DisplayName("認定時間")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd HH:mm}")]
        public DateTime? override_stamp { get; set; }

        [DisplayName("姓名")]
        //[JsonIgnore]
        public string full_name { get; set; }
        public string account_id { get; set; }

        [DisplayName("刪除")]
        [JsonIgnore]
        public string chk_z_status { get; set; }

        public string z_status { get; set; }

        public string z_usr { get; set; }
    }


    public class tw_CardStamp_Matching
    {
        public bool chk { get; set; }

        public string account_id { get; set; }

        [DisplayName("Name")]
        public string full_name { get; set; }

        [DisplayName("Check Stamp")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd HH:mm}", NullDisplayText = "")]
        public DateTime check_stamp { get; set; }

        [DisplayName("Message")]
        public string msg { get; set; }
    }


    public class tw_DayOffList
    {
        public List<tw_off> theOffs { get; set; }
        public List<tw_usable> theUsable { get; set; }
    }
    public class tw_off
    {
        [DisplayName("單號")]
        //[DisplayFormat(DataFormatString = "{0:N0}", ConvertEmptyStringToNull = true, NullDisplayText = "")]
        [DisplayFormat(ConvertEmptyStringToNull = true, NullDisplayText = "")]
        public int apply_uid { get; set; }

        [DisplayName("姓名")]
        public string full_name { get; set; }

        [DisplayName("假別名稱")]
        public string day_name { get; set; }

        [DisplayName("假別性質")]
        public string day_type { get; set; }

        [DisplayName("請假單位(分)")]
        public int off_unit { get; set; }

        [DisplayName("起始時間")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd HH:mm}")]
        public DateTime day_start { get; set; }

        [DisplayName("截止時間")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd HH:mm}")]
        public DateTime day_end { get; set; }

        [DisplayName("請假年月")]
        public int current_ym { get; set; }

        [DisplayName("請假時數")]
        [DisplayFormat(DataFormatString = "{0:N1}", ConvertEmptyStringToNull = true, NullDisplayText = "0")]
        public decimal day_hours { get; set; }
    }
    public class tw_usable
    {
        [DisplayName("姓名")]
        public string full_name { get; set; }

        [DisplayName("可用年度")]
        public int day_year { get; set; }

        [DisplayName("可用起日")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd}")]
        public DateTime available_start { get; set; }

        [DisplayName("可用迄日")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd}")]
        public DateTime available_end { get; set; }

        [DisplayName("剩餘時數")]
        [DisplayFormat(DataFormatString = "{0:N1}", ConvertEmptyStringToNull = true, NullDisplayText = "0")]
        public decimal remaining_hours { get; set; }

        [DisplayName("特休起算日")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd}")]
        public DateTime annual_start { get; set; }
    }

}
