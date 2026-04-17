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
    public class Menu_Program
    {
        //private SqlConnection SBScon = GlobalConfig.MyConfig.Connection_SBS;
        Common_Share shareCommon = new Common_Share();


        public List<MenuTreeItem> FetchMenuItem(int parent_id)
        {
            List<MenuItem> wkList = GlobalVariables.MySession.List_Menu.Where(e => e.parent_menu == parent_id).ToList();
            List<MenuTreeItem> wkItem = new List<MenuTreeItem>();

            var find = from data in wkList
                       select new MenuTreeItem()
                       {
                           menu_id = data.menu_id,
                           menu_name = data.menu_name,
                           child_menu = null,
                           menu_sort = data.menu_sort,
                           z_status = data.z_status,
                           chk_read = false
                       };
            wkItem = find.ToList();

            foreach (var item in wkItem)
                item.child_menu = FetchMenuItem(item.menu_id);

            return wkItem;
        }


        public List<MenuItem> GetMenuItem()
        {
            if (GlobalVariables.MySession.List_Menu == null)
                FetchMenuList();

            return GlobalVariables.MySession.List_Menu;
        }


        public MenuData FetchMenuList()
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            int i;

            MenuData theData = new MenuData();
            List<MenuItem> theEntireList = new List<MenuItem>();
            List<MenuTreeItem> theItemList = new List<MenuTreeItem>();

            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "z.prIntranet_GetMenuList");
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
            {
                theEntireList = JsonConvert.DeserializeObject<List<MenuItem>>(ResultModel.JsonData);

                for (i = 0; i < theEntireList.Count(); i++)
                {
                    theEntireList[i].no = i + 1;

                    theEntireList[i].isFolder = false;
                    if (theEntireList.Where(e => e.parent_menu == theEntireList[i].menu_id).ToList().Count() > 0)
                        theEntireList[i].isFolder = true;
                }

                GlobalVariables.MySession.List_Menu = null;
                GlobalVariables.MySession.List_Menu = theEntireList;
                theData.theList = theEntireList;

                theItemList = FetchMenuItem(0);
                theData.theItem = theItemList;
            }

            return theData;
        }


        public SP_Return UpdateMenu(MenuItem pm_Menu)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };

            //double check
            ResultModel = shareCommon.checkAuthorized(14);
            if (ResultModel.r == 0)
                return ResultModel;

            pm_Menu.z_usr = GlobalVariables.MySession.Account;

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.NVarChar, Value = JsonConvert.SerializeObject(pm_Menu)}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "z.prIntranet_ManageMenu", wkParm);
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
                ResultModel = JsonConvert.DeserializeObject<SP_Return>(ResultModel.JsonData);

            return ResultModel;
        }


        public List<PermissionItem> WhoHasPermission(string pm_MenuID)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            List<PermissionItem> theList = new List<PermissionItem>();
            string json_parm = "{\"pm_kind\": 3, \"pm_account_id\": \"\", \"pm_group_id\": 0, \"pm_menu_id\": " + pm_MenuID + "}";

            List<SqlParameter> wkParm = new List<SqlParameter>()
            {
                new SqlParameter() {ParameterName = "@pmJSON", SqlDbType = SqlDbType.NVarChar, Value = json_parm}
            };
            ResultModel = shareCommon.ExecSP(GlobalConfig.MyConfig.Connection_SBS, "z.prIntranet_GetPermissionData", wkParm);
            if (ResultModel.r == 1 && !string.IsNullOrWhiteSpace(ResultModel.JsonData))
                theList = JsonConvert.DeserializeObject<List<PermissionItem>>(ResultModel.JsonData);

            return theList;
        }

    }
}
