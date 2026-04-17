using RuntimeVariables;
using RuntimeConfig;
using SharedScrpits;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using Newtonsoft.Json;


namespace Home.Models
{
    public class LoginScripts
    {
        HomeScripts scriptsHome = new HomeScripts();
        Common_Share shareCommon = new Common_Share();
        //private SqlConnection SBScon = GlobalConfig.MyConfig.Connection_SBS;


        public List<Login> FetchMenuForAccount(string pm_id)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<Login> theList = new List<Login>();
            string pmJSON = "{'pm_id': '" + pm_id + "'}";
            pmJSON = pmJSON.Replace("'", "\"");

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.NVarChar, Value = pmJSON}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "z.prIntranet_GetLoginMenu", wkParm);
            if (ResultModel.r == 1)
            {
                theList = JsonConvert.DeserializeObject<List<Login>>(ResultModel.JsonData);
                if (scriptsHome.debugStatus())
                {
                    foreach (Login item in theList)
                        item.menu_path += " [Debug]";
                }
            }

            return theList;
        }

        
        public List<ReportItem> FetchReportClusterForAccount(string pm_id)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<ReportItem> theClusters = new List<ReportItem>();
            string pmJSON = "{\"pm_id\": \"" + pm_id + "\", \"pm_cluster\":\"\"}";

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.NVarChar, Value = pmJSON}
            };

            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "z.prIntranet_GetLoginReport", wkParm);
            if (ResultModel.r == 1)
                theClusters = JsonConvert.DeserializeObject<List<ReportItem>>(ResultModel.JsonData);

            return theClusters;
        }


        public List<ReportItem> FetchReportForAccount(string pm_id, List<ReportItem> pm_cluster_list)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<ReportItem> theReports = new List<ReportItem>();
            List<SqlParameter> wkParm = new List<SqlParameter>();

            if (pm_cluster_list != null)
            {
                string pmCluster = string.Join(",", pm_cluster_list.Select(s => s.cluster));
                string pmJSON = "{\"pm_id\": \"" + pm_id + "\", \"pm_cluster\":\"" + pmCluster + "\"}";
                wkParm = null;
                wkParm = new List<SqlParameter>()
                    {
                        new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.NVarChar, Value = pmJSON}
                    };
                ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "z.prIntranet_GetLoginReport", wkParm);
                if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
                    theReports = JsonConvert.DeserializeObject<List<ReportItem>>(ResultModel.JsonData);
                /*
                foreach (var wkCluster in pm_cluster_list)
                {
                    string pmJSON = "{\"pm_id\": \"" + pm_id + "\", \"pm_cluster\":\"" + wkCluster.cluster + "\"}";
                    wkParm = null;
                    wkParm = new List<SqlParameter>()
                    {
                        new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.NVarChar, Value = pmJSON}
                    };
                    ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "z.prIntranet_GetLoginReport", wkParm);
                    if (ResultModel.r == 1)
                        theReports.AddRange(JsonConvert.DeserializeObject<List<ReportItem>>(ResultModel.JsonData));
                }*/
            }

            return theReports;
        }
        

        public List<LoginTreeItem> ToLoginTreeItem(int parent_id)
        {
            List<Login> wkList = GlobalVariables.MySession.LoginMenuList.Where(e => e.parent_menu == parent_id).ToList();
            List<LoginTreeItem> wkItem = new List<LoginTreeItem>();

            foreach (var item in wkList)
            {
                wkItem.Add(new LoginTreeItem()
                {
                    parent_id = item.parent_menu,
                    menu_id = item.menu_id,
                    menu_name = item.menu_name,
                    folder = item.folder,
                    run = item.run,
                    param = item.param,
                    kind = item.kind,
                    child_menu = ToLoginTreeItem(item.menu_id)
                });
            }

            return wkItem;
        }


        public string LoginTreeItemToString(List<LoginTreeItem> pmMenuList)
        {
            string menuString = "";
            string project_name = (scriptsHome.debugStatus() ? "" : scriptsHome.getProjectName() + "/");
            //string project_name = "";
            //string project_name = (scriptsHome.debugStatus() ? "" : GlobalConfig.MyConfig.ProjectName + "/");
            //string project_name = (scriptsHome.debugStatus() ? "" : ConfigurationManager.AppSettings["RootFolder"].ToString() + "/");

            foreach (var thisRow in pmMenuList)
            {
                if (thisRow.child_menu.Count() > 0)
                {
                    if (thisRow.parent_id == 0)
                    {
                        menuString += "<li class=\"dropdown\">";
                        menuString += "<a class=\"dropdown-toggle\" data-toggle=\"dropdown\" href=\"#\">" + thisRow.menu_name + "</span></a>";
                    }
                    else
                    {
                        menuString += "<li class=\"dropdown-submenu\">";
                        menuString += "<a class=\"submenu\" data-toggle=\"dropdown\" href=\"#\">" + thisRow.menu_name + "&nbsp;&nbsp;<span class=\"caret\"></span></a>";
                    }
                    menuString += "<ul class=\"dropdown-menu\">";
                    menuString += LoginTreeItemToString(thisRow.child_menu);
                    menuString += "</ul>";
                    menuString += "</li>";
                }
                else
                {
                    menuString += "<li>";
                    if (thisRow.kind == "C")
                        menuString += "<a href=\"/" + project_name + "Home/ComingSoon\">" + thisRow.menu_name + "</a>";
                    else
                        menuString += "<a href=\"/" + project_name + thisRow.folder + "/" + thisRow.run + thisRow.param + "\">" + thisRow.menu_name + "</a>";
                    menuString += "</li>";
                }
            }

            return menuString;
        }


        public SP_Return LoginCheck(string pm_id, string pm_pw)
        {
            /*
            try
            {
                GlobalVariables.MySession.Connection_SBS = new SqlConnection(ConfigurationManager.ConnectionStrings["sbsdevConnection"].ConnectionString);
                //GlobalVariables.MySession.Connection_SBS = new SqlConnection(ConfigurationManager.ConnectionStrings["sbsConnection"].ConnectionString);
                GlobalVariables.MySession.Connection_OS_BuySheet = new SqlConnection(ConfigurationManager.ConnectionStrings["osbuydevConnection"].ConnectionString);
                //GlobalVariables.MySession.Connection_OS_BuySheet = new SqlConnection(ConfigurationManager.ConnectionStrings["osbuyConnection"].ConnectionString);
                GlobalVariables.MySession.ProjectName = ConfigurationManager.AppSettings["ProjectName"].ToString();
                GlobalVariables.MySession.SSRSReportUrl = ConfigurationManager.AppSettings["SSRSReportUrl"].ToString();
                GlobalVariables.MySession.SSRSdomain = ConfigurationManager.AppSettings["SSRSdomain"].ToString();
                GlobalVariables.MySession.SSRSuser = ConfigurationManager.AppSettings["SSRSuser"].ToString();
                GlobalVariables.MySession.SSRSpassword = ConfigurationManager.AppSettings["SSRSpassword"].ToString();
                GlobalVariables.MySession.Validity = true;
            }
            catch (Exception ex)
            {
                throw new Exception("GlobalVariables.MySession", ex);
            }
            try
            {
                //if (!ConfigsModel.MyConfig.Validity)
                //{
                    ConfigsModel.MyConfig.Connection_SBS = new SqlConnection(ConfigurationManager.ConnectionStrings["sbsdevConnection"].ConnectionString);
                    //ConfigsModel.MyConfig.Connection_SBS = new SqlConnection(ConfigurationManager.ConnectionStrings["sbsConnection"].ConnectionString);
                    ConfigsModel.MyConfig.Connection_OS_BuySheet = new SqlConnection(ConfigurationManager.ConnectionStrings["osbuydevConnection"].ConnectionString);
                    //ConfigsModel.MyConfig.Connection_OS_BuySheet = new SqlConnection(ConfigurationManager.ConnectionStrings["osbuyConnection"].ConnectionString);
                    ConfigsModel.MyConfig.ProjectName = ConfigurationManager.AppSettings["ProjectName"].ToString();
                    ConfigsModel.MyConfig.SSRSReportUrl = ConfigurationManager.AppSettings["SSRSReportUrl"].ToString();
                    ConfigsModel.MyConfig.SSRSdomain = ConfigurationManager.AppSettings["SSRSdomain"].ToString();
                    ConfigsModel.MyConfig.SSRSuser = ConfigurationManager.AppSettings["SSRSuser"].ToString();
                    ConfigsModel.MyConfig.SSRSpassword = ConfigurationManager.AppSettings["SSRSpassword"].ToString();
                    ConfigsModel.MyConfig.Validity = true;
                //}
            }
            catch (Exception ex)
            {
                throw new Exception("ConfigsModel.MyConfig", ex);
            }
            */
            SP_Return LoginModel = new SP_Return();
            Profile ProfileModel = new Profile();
            pm_id = pm_id.ToLower().Replace("@specialtybolt.com", "");
            string json_parm = "{\"pm_id\": \"" + pm_id + "\", \"pm_pw\": \"" + pm_pw + "\"}";

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.NVarChar, Value = json_parm}
            };
            LoginModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "z.prIntranet_Login", wkParm);
            if (LoginModel.r == 1)
            {
                LoginModel = JsonConvert.DeserializeObject<SP_Return>(LoginModel.JsonData);

                if (LoginModel.r != 1 && LoginModel.r != 2)
                {
                    ProfileModel = JsonConvert.DeserializeObject<Profile>(LoginModel.JsonData);
                    GlobalVariables.MySession.Account = pm_id;
                    GlobalVariables.MySession.FirstName = ProfileModel.first_name;
                    GlobalVariables.MySession.LastName = ProfileModel.last_name;
                    GlobalVariables.MySession.Email = ProfileModel.email;
                    GlobalVariables.MySession.LoginMenuList = FetchMenuForAccount(pm_id);
                    GlobalVariables.MySession.LoginTreeList = ToLoginTreeItem(0);
                    GlobalVariables.MySession.LoginMenuString = LoginTreeItemToString(GlobalVariables.MySession.LoginTreeList);
                    GlobalVariables.MySession.LoginReportList = FetchReportForAccount(pm_id, FetchReportClusterForAccount(pm_id));
                    HttpContext.Current.Session["Validity"] = true;
                }
            }

            return LoginModel;
        }


        public SP_Return ChangePassword(string pm_Password, string pm_NewPassword, string pm_NewPassword2)
        {
            SP_Return PasswordModel = new SP_Return();
            string json_parm = "";
            char wkLetter;
            string regexPattern = @"^[^0-9A-Za-z]"; //the ^ character in the brackets actually means 'NOT'
            int i, upperCnt, lowerCnt, numCnt, symbolCnt;

            PasswordModel.msg = "";
            if (string.IsNullOrWhiteSpace(pm_Password) || string.IsNullOrWhiteSpace(pm_NewPassword) || string.IsNullOrWhiteSpace(pm_NewPassword2))
                PasswordModel.msg = "Please insert information into all columns.";
            else
            if (pm_NewPassword != pm_NewPassword2)
                PasswordModel.msg = "What you input for two new password columns are not matched, please confirm your new password again.";
            else
            if (pm_Password == pm_NewPassword)
                PasswordModel.msg = "New password cannot be the same to original one, please try another.";
            else
            {
                upperCnt = lowerCnt = numCnt = symbolCnt = 0;
                for (i = 0; i < pm_NewPassword.Length; i++)
                {
                    wkLetter = pm_NewPassword[i];
                    if (Char.IsUpper(wkLetter))
                        upperCnt++;
                    if (Char.IsLower(wkLetter))
                        lowerCnt++;
                    if (Char.IsNumber(wkLetter))
                        numCnt++;
                    if (Regex.IsMatch(wkLetter.ToString(), regexPattern))
                        symbolCnt++;
                }

                if (upperCnt <= 0 || 
                    lowerCnt <= 0 || 
                    numCnt <= 0 || 
                    symbolCnt <= 0 ||
                    pm_NewPassword.ToLower().Contains(GlobalVariables.MySession.Account) ||
                    pm_NewPassword.ToLower().Contains(GlobalVariables.MySession.FirstName.ToLower()) ||
                    pm_NewPassword.ToLower().Contains(GlobalVariables.MySession.LastName.ToLower()) ||
                    pm_NewPassword.ToLower().Contains(GlobalVariables.MySession.Email.ToLower())
                )
                    PasswordModel.msg = "New password does not comply with the rules.";
            }

            if (PasswordModel.msg != "")
            {
                PasswordModel.r = -1;
                return PasswordModel;
            }

            json_parm = "{\"pm_id\": \"" + GlobalVariables.MySession.Account + "\", \"pm_pw\": \"" + pm_NewPassword + "\", \"pm_usr\": \"" + GlobalVariables.MySession.Account + "\"}";

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.NVarChar, Value = json_parm}
            };
            PasswordModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "z.prIntranet_ChangePassword", wkParm);
            if (PasswordModel.r == 1)
                PasswordModel = JsonConvert.DeserializeObject<SP_Return>(PasswordModel.JsonData);

            return PasswordModel;
        }


        public SP_Return ForgetPassword(string pm_email)
        {
            SP_Return ForgetModel = new SP_Return();
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(GlobalConfig.MyConfig.Connection_SBS.ConnectionString);
            string json_parm = "{\"pm_email\": \"" + pm_email + "\", \"pm_z_usr\": \"" + builder.UserID + "\"}";

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.NVarChar, Value = json_parm}
            };
            ForgetModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "z.prIntranet_ForgetPassword", wkParm);
            if (ForgetModel.r == 1)
                ForgetModel = JsonConvert.DeserializeObject<SP_Return>(ForgetModel.JsonData);

            builder.Clear();
            return ForgetModel;
        }

    }


    public class AdminQuery_Scripts
    {
        LoginScripts scriptsLogin = new LoginScripts();
        List<Login> wkQueryLoginMenuTree = new List<Login>();

        public QueryData FetchQueryAuthority(string pm_account_id, List<ReportItem> pm_cluster_list)
        {
            QueryData wkQuery = new QueryData();
            wkQuery.theMenus = FetchAccountMenuTree(pm_account_id);
            wkQuery.theReports = scriptsLogin.FetchReportForAccount(pm_account_id, pm_cluster_list);
            return wkQuery;
        }


        public List<MenuTreeItem> FetchAccountMenuTree(string pm_account_id)
        {
            wkQueryLoginMenuTree = null;
            wkQueryLoginMenuTree = scriptsLogin.FetchMenuForAccount(pm_account_id);

            return ConvertMenuItem(0);
        }


        public List<MenuTreeItem> ConvertMenuItem(int parent_id)
        {
            List<Login> wkList = wkQueryLoginMenuTree.Where(e => e.parent_menu == parent_id).ToList();
            List<MenuTreeItem> wkItem = new List<MenuTreeItem>();

            var find = from data in wkList
                       select new MenuTreeItem()
                       {
                           menu_id = data.menu_id,
                           menu_name = data.menu_name,
                           child_menu = null,
                           menu_sort = data.menu_sort,
                           z_status = "",
                           chk_read = (data.kind == "R" ? true : false)
                       };
            wkItem = find.ToList();

            foreach (var item in wkItem)
                item.child_menu = ConvertMenuItem(item.menu_id);

            return wkItem;
        }

    }

}