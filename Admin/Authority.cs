using Newtonsoft.Json;
using RuntimeVariables;
using RuntimeConfig;
using SharedScrpits;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Admin
{
    public class Authority_Program
    {
        //private SqlConnection SBScon = GlobalConfig.MyConfig.Connection_SBS;
        Common_Share shareCommon = new Common_Share();
        Menu_Program pgmMenu = new Menu_Program();
        Report_Program pgmReport = new Report_Program();


        public AuthorityData FetchAuthorityData()
        {
            AuthorityData theData = new AuthorityData();
            List<PermissionItem> entirePermission = new List<PermissionItem>();
            List<ReportPermissionItem> entireReportPermission = new List<ReportPermissionItem>();
            List<GroupItem> entireGroup = new List<GroupItem>();

            entirePermission = FetchPermissionData(0, 0, "", 0);
            GlobalVariables.MySession.List_Permission = null;
            GlobalVariables.MySession.List_Permission = entirePermission;

            entireReportPermission = FetchReportPermissionData(0, 0, "", 0);
            GlobalVariables.MySession.List_ReportPermission = null;
            GlobalVariables.MySession.List_ReportPermission = entireReportPermission;

            entirePermission = FetchPermissionData(1, 0, "", 0);
            theData.distinctPermissionList = FetchGroupData(1, 0, "");
            theData.distinctIndividualList = entirePermission.FindAll(p => p.kind == "I").ToList();

            theData.distinctGroupList = theData.distinctPermissionList;
            entireGroup = FetchGroupData(0, 0, "");
            GlobalVariables.MySession.List_Group = null;
            GlobalVariables.MySession.List_Group = entireGroup;

            return theData;
        }

        #region Permission 

        public PermissionData GetPermissionData(int pm_group_id, string pm_account_id, bool pm_isNew)
        {
            PermissionData theData = new PermissionData();
            List<PermissionItem> thePermission = new List<PermissionItem>();
            List<ReportPermissionItem> theReportPermission = new List<ReportPermissionItem>();
            List<MenuItem> theMenus = pgmMenu.GetMenuItem().FindAll(m => m.z_status != "D" && m.isFolder == false);
            List<ReportItem> theReports = pgmReport.GetReportList().FindAll(r => r.z_status != "D");

            foreach (var a in theMenus)
            {
                a.menu_chk = false;
                a.menu_chk_read = false;
            }
            foreach (var a in theReports)
            {
                a.rpt_chk = false;
                a.rpt_chk_read = false;
            }
            theData.menuList = theMenus;
            theData.reportList = theReports;

            if (pm_isNew)
            {
                theData.permissionList = thePermission;
                theData.ReportPermissionList = theReportPermission;
            }
            else
            {
                thePermission = GlobalVariables.MySession.List_Permission;
                if (!string.IsNullOrEmpty(pm_account_id))
                    theData.permissionList = thePermission.FindAll(p => p.account_id == pm_account_id);
                else
                    theData.permissionList = thePermission.FindAll(p => p.group_id == pm_group_id);

                foreach (var p in theData.permissionList)
                {
                    var member = theMenus.FirstOrDefault(m => m.menu_id == p.menu_id);
                    if (member != null)
                    {
                        member.menu_chk = true;
                        member.menu_chk_read = p.read_only;
                    }
                }
                
                theReportPermission = GlobalVariables.MySession.List_ReportPermission;
                if (!string.IsNullOrEmpty(pm_account_id))
                    theData.ReportPermissionList = theReportPermission.FindAll(p => p.account_id == pm_account_id);
                else
                    theData.ReportPermissionList = theReportPermission.FindAll(p => p.group_id == pm_group_id);

                foreach (var rp in theData.ReportPermissionList)
                {
                    var item = theReports.FirstOrDefault(r => r.report_id == rp.report_id);
                    if (item != null)
                    {
                        item.rpt_chk = true;
                        item.rpt_chk_read = rp.read_only;
                    }
                }
            }

            return theData;
        }


        public List<PermissionItem> FetchPermissionData(int pm_kind, int pm_group_id, string pm_account_id, int pm_menu_id)
        {
            SP_Return resultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<PermissionItem> theList = new List<PermissionItem>();
            string json_parm = "{'pm_kind': " + pm_kind.ToString() + ", 'pm_account_id': '" + pm_account_id + "', 'pm_group_id': " + pm_group_id.ToString() + ", 'pm_menu_id': " + pm_menu_id.ToString() + "}";
            json_parm = json_parm.Replace("'", "\"");

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.NVarChar, Value = json_parm}
            };
            resultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "z.prIntranet_GetPermissionData", wkParm);
            if (resultModel.r == 1 && !string.IsNullOrWhiteSpace(resultModel.JsonData))
                theList = JsonConvert.DeserializeObject<List<PermissionItem>>(resultModel.JsonData);

            return theList;
        }

        
        public List<ReportPermissionItem> FetchReportPermissionData(int pm_kind, int pm_group_id, string pm_account_id, int pm_report_id)
        {
            SP_Return resultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<ReportPermissionItem> theList = new List<ReportPermissionItem>();
            string json_parm = "{'pm_kind': " + pm_kind.ToString() + ", 'pm_account_id': '" + pm_account_id + "', 'pm_group_id': " + pm_group_id.ToString() + ", 'pm_report_id': " + pm_report_id.ToString() + "}";
            json_parm = json_parm.Replace("'", "\"");

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.NVarChar, Value = json_parm}
            };
            resultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "z.prIntranet_GetReportPermissionData", wkParm);
            if (resultModel.r == 1 && !string.IsNullOrWhiteSpace(resultModel.JsonData))
                theList = JsonConvert.DeserializeObject<List<ReportPermissionItem>>(resultModel.JsonData);

            return theList;
        }
        

        public SP_Return RemovePermission(int pm_group_id, string pm_account_id)
        {
            SP_Return resultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };

            //double check
            resultModel = shareCommon.checkAuthorized(15);
            if (resultModel.r == 0)
                return resultModel;

            string json_parm = "{ 'pm_type': 0, 'pm_group_id': " + pm_group_id.ToString() + ", 'pm_account_id': '" + pm_account_id + "', 'pm_z_usr': '" + GlobalVariables.MySession.Account + "' }";
            json_parm = json_parm.Replace("'", "\"");

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.NVarChar, Value = json_parm}
            };
            resultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "z.prIntranet_ManagePermission2", wkParm);
            if (resultModel.r == 1 && !string.IsNullOrWhiteSpace(resultModel.JsonData))
                resultModel = JsonConvert.DeserializeObject<SP_Return>(resultModel.JsonData);

            return resultModel;
        }


        public SP_Return UpdatePermission(int pm_group_id, string pm_account_id, List<MenuItem> pmMenuList, List<ReportItem> pmReportList)
        {
            SP_Return resultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };

            //double check
            resultModel = shareCommon.checkAuthorized(15);
            if (resultModel.r == 0)
                return resultModel;

            List<MenuItem> PermissionList = pmMenuList.FindAll(a => a.menu_chk == true).ToList();
            List<ReportItem> ReportPermissionList = pmReportList.FindAll(a => a.rpt_chk == true).ToList();
            string theMenus = "";
            string theReports = "";
            string json_parm = "";

            if (PermissionList.Count() <= 0)
            {
                resultModel.r = 0;
                resultModel.msg = "None of menus are selected.";
                return resultModel;
            }

            foreach (var p in PermissionList)
                theMenus += "{'menu_id':" + p.menu_id + ", 'read_only':'" + p.menu_chk_read.ToString() + "'},";
            theMenus = "[" + theMenus.TrimEnd(',') + "]";
            
            foreach (var r in ReportPermissionList)
                theReports += "{'report_id':" + r.report_id + ", 'read_only':'" + r.rpt_chk_read.ToString() + "'},";
            theReports = "[" + theReports.TrimEnd(',') + "]";
            
            json_parm = "{ 'pm_type': 1, 'pm_group_id': " + pm_group_id.ToString() + ", 'pm_account_id': '" + pm_account_id + "', 'pm_menus': " + theMenus + ", 'pm_reports': " + theReports + ", 'pm_z_usr': '" + GlobalVariables.MySession.Account + "' }";
            json_parm = json_parm.Replace("'", "\"");

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.NVarChar, Value = json_parm}
            };
            resultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "z.prIntranet_ManagePermission2", wkParm);
            if (resultModel.r == 1 && !string.IsNullOrWhiteSpace(resultModel.JsonData))
                resultModel = JsonConvert.DeserializeObject<SP_Return>(resultModel.JsonData);

            return resultModel;
        }

        #endregion

        #region Group 

        public List<GroupItem> FetchGroupData(int pm_kind, int pm_group_id, string pm_account_id)
        {
            SP_Return resultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<GroupItem> theList = new List<GroupItem>();
            string json_parm = "{\"pm_kind\": " + pm_kind.ToString() + ", \"pm_group_id\": " + pm_group_id.ToString() + ", \"pm_account_id\": \"" + pm_account_id + "\"}";

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.NVarChar, Value = json_parm}
            };
            resultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "z.prIntranet_GetGroupData", wkParm);
            if (resultModel.r == 1 && !string.IsNullOrWhiteSpace(resultModel.JsonData))
            {
                theList = JsonConvert.DeserializeObject<List<GroupItem>>(resultModel.JsonData);

                for (int i = 0; i < theList.Count(); i++)
                    theList[i].no = i + 1;
            }

            return theList;
        }


        public SP_Return RemoveGroup(int pm_group_id)
        {
            SP_Return resultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };

            //double check
            resultModel = shareCommon.checkAuthorized(15);
            if (resultModel.r == 0)
                return resultModel;

            string json_parm = "{ 'pm_type': 0, 'pm_group_id': " + pm_group_id.ToString() + ", 'pm_z_usr': '" + GlobalVariables.MySession.Account + "' }";
            json_parm = json_parm.Replace("'", "\"");

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.NVarChar, Value = json_parm}
            };
            resultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "z.prIntranet_ManageGroup", wkParm);
            if (resultModel.r == 1 && !string.IsNullOrWhiteSpace(resultModel.JsonData))
                resultModel = JsonConvert.DeserializeObject<SP_Return>(resultModel.JsonData);

            return resultModel;
        }


        public SP_Return UpdateGroup(int pm_group_id, string pm_group_name, List<AccountItem> pmAccountList)
        {
            SP_Return resultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };

            //double check
            resultModel = shareCommon.checkAuthorized(15);
            if (resultModel.r == 0)
                return resultModel;

            if (pmAccountList.FindAll(a => a.account_chk == true).Count() <= 0)
            {
                resultModel.r = 0;
                resultModel.msg = "None of accounts are selected.";
                return resultModel;
            }

            string theseAccounts = "[{'id': '" + string.Join("'}, {'id': '", pmAccountList.FindAll(a => a.account_chk == true).Select(a => a.id).ToArray()) + "'}]";
            string json_parm = "{ 'pm_type': 1, 'pm_group_id': " + pm_group_id.ToString() + ", 'pm_group_name': '" + pm_group_name + "', 'pm_account_id': " + theseAccounts + ", 'pm_z_usr': '" + GlobalVariables.MySession.Account + "' }";
            json_parm = json_parm.Replace("'", "\"");

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.NVarChar, Value = json_parm}
            };
            resultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "z.prIntranet_ManageGroup", wkParm);
            if (resultModel.r == 1 && !string.IsNullOrWhiteSpace(resultModel.JsonData))
                resultModel = JsonConvert.DeserializeObject<SP_Return>(resultModel.JsonData);

            return resultModel;
        }

        #endregion

        #region Cover 

        public SP_Return CoverSettings(string pm_AccountID_source, string pm_AccountID_target)
        {
            SP_Return resultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };

            //double check
            resultModel = shareCommon.checkAuthorized(15);
            if (resultModel.r == 0)
                return resultModel;

            if (string.IsNullOrEmpty(pm_AccountID_source) || string.IsNullOrEmpty(pm_AccountID_target))
            {
                resultModel.r = 0;
                resultModel.msg = "Please select both source and target account id.";
                return resultModel;
            }
            if (pm_AccountID_source == pm_AccountID_target)
            {
                resultModel.r = 0;
                resultModel.msg = "Source and target account id cannot be the same.";
                return resultModel;
            }

            string json_parm = "{ 'pm_AccountID_source': '" + pm_AccountID_source + "', 'pm_AccountID_target': '" + pm_AccountID_target + "', 'pm_z_usr': '" + GlobalVariables.MySession.Account + "' }";
            json_parm = json_parm.Replace("'", "\"");

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.NVarChar, Value = json_parm}
            };
            resultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "z.prIntranet_CoverAuthority", wkParm);
            if (resultModel.r == 1 && !string.IsNullOrWhiteSpace(resultModel.JsonData))
                resultModel = JsonConvert.DeserializeObject<SP_Return>(resultModel.JsonData);

            return resultModel;
        }

        #endregion

        #region Query 

        // Moved to Home\LoginScripts\AdminQuery_Scripts

        #endregion

    }
}
