using Admin;
using Home.Models;
using RuntimeVariables;
using SharedScrpits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace Home.Controllers
{
    [SessionExpireFilter]
    public class AdminController : Controller
    {
        Common_Share shareCommon = new Common_Share();
        Get_Share shareGet = new Get_Share();
        Convertor_Share shareConvertor = new Convertor_Share();
        HomeScripts scriptsHome = new HomeScripts();
        LoginScripts scriptsLogin = new LoginScripts();
        AdminQuery_Scripts scriptsQuery = new AdminQuery_Scripts();
        Account_Program pgmAccount = new Account_Program();
        Menu_Program pgmMenu = new Menu_Program();
        Report_Program pgmReport = new Report_Program();
        Authority_Program pgmAuthority = new Authority_Program();


        #region Account

        public ActionResult AccountManagement(string pm_id = "")
        {
            ViewBag.PageInfo = shareCommon.GetPageInfo(13);
            ViewData["pm_id"] = pm_id;
            return View(pgmAccount.FetchAccountList());
        }


        [HttpPost]
        [MultipleButton(Name = "AdminAccountAction", Argument = "Welcome")]
        public ActionResult AccountWelcome(AccountItem pm_Account)
        {
            SP_Return result = pgmAccount.SendWelcome(pm_Account.id);
            TempData["message"] = result.msg;

            return RedirectToAction("AccountManagement");
        }


        [HttpPost]
        [MultipleButton(Name = "AdminAccountAction", Argument = "ResetPW")]
        public ActionResult AccountResetPW(AccountItem pm_Account)
        {
            SP_Return result = pgmAccount.ResetPassword(pm_Account.id);
            TempData["message"] = result.msg;

            return RedirectToAction("AccountManagement");
        }


        [HttpPost]
        [MultipleButton(Name = "AdminAccountAction", Argument = "Save")]
        public ActionResult AccountSave(AccountItem pm_Account, bool pm_isNew)
        {
            if (pm_isNew)
            {
                if (GlobalVariables.MySession.List_Account.Where(a => a.id == pm_Account.id).ToList().Count > 0)
                {
                    TempData["message"] = "The account has been used already.";
                    return RedirectToAction("AccountManagement");
                }
            }

            SP_Return result = pgmAccount.ManageAccount(pm_Account);

            if (result.r == 0)
                TempData["message"] = "Account '" + Request.Form["id"] + "' is saved.";
            else
                TempData["message"] = result.msg;

            return RedirectToAction("AccountManagement");
        }


        public PartialViewResult ShowAccountDetails(string pm_id, bool pm_isNew = false)
        {
            ViewBag.authorized = shareCommon.isAuthorized(13);
            ViewData["isNew"] = pm_isNew;
            AccountItem theAccount = new AccountItem();

            if (!String.IsNullOrEmpty(pm_id))
                theAccount = GlobalVariables.MySession.List_Account.Where(a => a.id == pm_id).SingleOrDefault();

            var timeZone = shareConvertor.ToDropDownOptions(shareGet.FetchCodeList("Time_Zone"));
            SelectList theSelect = new SelectList(timeZone, "Value", "Text", theAccount.area);
            ViewData["area_name"] = theSelect;

            return PartialView("_AccountDetails", theAccount);
        }


        public ActionResult WhichGroupIsIn(string pm_AccountID)
        {
            ViewData["Account"] = pm_AccountID;
            List<GroupItem> List_Group = pgmAccount.WhichGroupIsIn(pm_AccountID);
            return View("WhichGroupIsIn", List_Group);
        }

        #endregion


        #region Menu

        public ActionResult MenuManagement()
        {
            ViewBag.PageInfo = shareCommon.GetPageInfo(14);
            return View(pgmMenu.FetchMenuList());
        }


        public PartialViewResult ShowMenuDetails(int pm_id, bool pm_isNew)
        {
            MenuItem theMenu = new MenuItem();

            if (!pm_isNew)
                theMenu = GlobalVariables.MySession.List_Menu.Where(a => a.menu_id == pm_id).SingleOrDefault();

            ViewData["isNew"] = pm_isNew;

            return PartialView("_MenuDetails", theMenu);
        }


        public ActionResult MenuSave(MenuItem pm_Menu)
        {
            SP_Return result = pgmMenu.UpdateMenu(pm_Menu);

            if (result.r != 0)
                TempData["message"] = result.msg;

            return RedirectToAction("MenuManagement");
        }


        public ActionResult WhoHasPermission(string pm_MenuID, string pm_menu_name)
        {
            ViewData["Menu"] = pm_menu_name;
            List<PermissionItem> List_Permission = pgmMenu.WhoHasPermission(pm_MenuID);
            return View("WhoHasPermission", List_Permission);
        }

        #endregion


        #region Report

        public ActionResult ReportManagement()
        {
            ViewBag.PageInfo = shareCommon.GetPageInfo(49);
            ViewBag.authorized = shareCommon.isAuthorized(49);
            return View(pgmReport.FetchReportList());
        }


        public PartialViewResult ShowReportDetails(int pm_ReportID, bool pm_isNew)
        {
            ViewBag.authorized = shareCommon.isAuthorized(49);
            ReportItem theReport = new ReportItem();

            if (!pm_isNew)
                theReport = GlobalVariables.MySession.List_Report.Where(a => a.report_id == pm_ReportID).SingleOrDefault();

            ViewData["isNew"] = pm_isNew;
            ViewBag.pmCRUD = pm_isNew ? "C" : "U";
            ViewData["cluster"] = new SelectList(shareGet.Fetch_ReportClusterList(), "Value", "Text", theReport.cluster);

            return PartialView("_ReportDetails", theReport);
        }


        public ActionResult GoReportSave(ReportItem pm_Report, string pmCRUD)
        {
            SP_Return result = pgmReport.UpdateReport(pm_Report, pmCRUD);
            TempData["message"] = result.msg;

            return RedirectToAction("ReportManagement");
        }


        public ActionResult GoReportDelete(int pm_ReportID)
        {
            SP_Return result = pgmReport.DeleteReport(pm_ReportID);
            TempData["message"] = result.msg;

            return RedirectToAction("ReportManagement");
        }


        public PartialViewResult ShowReportCopy(int pm_ReportID)
        {
            ViewBag.authorized = shareCommon.isAuthorized(49);

            ReportItem theReport = new ReportItem();
            theReport = GlobalVariables.MySession.List_Report.Where(a => a.report_id == pm_ReportID).SingleOrDefault();

            ViewData["pm_Report"] = "[" + theReport.cluster + "] " + theReport.report_name;
            ViewData["cluster"] = new SelectList(shareGet.Fetch_ReportClusterList(), "Value", "Text", theReport.cluster);

            return PartialView("_ReportCopy", theReport);
        }


        public ActionResult GoReportCopy(ReportItem pm_Report)
        {
            SP_Return result = pgmReport.CopyReport(pm_Report);
            TempData["message"] = result.msg;

            return RedirectToAction("ReportManagement");
        }


        public ActionResult WhoHasReportPermission(string pm_ReportID, string pm_report_name)
        {
            ViewData["Report"] = pm_report_name;
            List<ReportPermissionItem> List_Permission = pgmReport.WhoHasReportPermission(pm_ReportID);
            return View("WhoHasReportPermission", List_Permission);
        }


        public ActionResult GetSSRSList()
        {
            var jsonData = shareGet.Fetch_SSRSList();
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }

        #endregion


        #region Authority

        public ActionResult AuthorityManagement()
        {
            ViewBag.PageInfo = shareCommon.GetPageInfo(15);
            ViewBag.authorized = shareCommon.isAuthorized(15);
            List<AccountItem> AccountList = pgmAccount.GetAccountList();
            ViewData["AccountQueryList"] = AccountList;
            ViewData["AccountSelectList"] = new SelectList(AccountList, "id", "full_name");
            return View(pgmAuthority.FetchAuthorityData());
        }


        public PartialViewResult ShowPermissionDetails(string pm_kind, int pm_group_id, string pm_account_id, bool pm_isNew)
        {
            ViewBag.authorized = shareCommon.isAuthorized(15);
            ViewBag.pm_isNew = pm_isNew;
            ViewData["pm_kind"] = pm_kind;
            ViewData["pm_group_id"] = pm_group_id;
            ViewData["pm_account_id"] = pm_account_id;
            PermissionData thePermission = pgmAuthority.GetPermissionData(pm_group_id, pm_account_id, pm_isNew);

            if (pm_kind == "I")
            {
                var find = from data in GlobalVariables.MySession.List_Permission.FindAll(p => p.kind == "I").Select(s => s.account_id).Distinct()
                           select new AccountItem()
                           {
                               id = data
                           };
                List<AccountItem> individualList = find.ToList();
                List<AccountItem> AccountList = pgmAccount.GetAccountList().Where(x => individualList.All(y => y.id != x.id)).ToList();
                ViewData["AccountList"] = new SelectList(AccountList, "id", "full_name");
            }

            if (pm_kind == "G")
            {
                if (thePermission.permissionList.Count() > 0)
                    ViewData["group_name"] = thePermission.permissionList[0].group_name;
                else
                    ViewData["group_name"] = GlobalVariables.MySession.List_Group.FirstOrDefault(g => g.group_id == pm_group_id).group_name ?? "";
            }

            return PartialView("_PermissionDetails", thePermission);
        }


        public ActionResult RemovePermission(int pm_group_id, string pm_account_id)
        {
            SP_Return result = pgmAuthority.RemovePermission(pm_group_id, pm_account_id);

            if (result.r == 0)
                TempData["message"] = result.msg;

            return RedirectToAction("AuthorityManagement");
        }


        public ActionResult SavePermission(List<MenuItem> menuList, List<ReportItem> reportList, int pm_group_id = -1, string pm_account_id = "")
        {
            SP_Return result = pgmAuthority.UpdatePermission(pm_group_id, pm_account_id, menuList, reportList);
            TempData["message"] = result.msg;
            return RedirectToAction("AuthorityManagement");
        }


        public PartialViewResult ShowGroupDetails(int pm_group_id, bool pm_isNew)
        {
            ViewBag.authorized = shareCommon.isAuthorized(15);
            ViewData["isNew"] = pm_isNew;
            GroupData theData = new GroupData();
            theData.accountList = pgmAccount.GetAccountList();

            foreach (var a in theData.accountList)
                a.account_chk = false;

            if (pm_isNew)
            {
                theData.group = new GroupItem();
                theData.memberList = new List<GroupItem>();
            }
            else
            {
                theData.group = GlobalVariables.MySession.List_Group.Where(a => a.group_id == pm_group_id).FirstOrDefault();
                theData.memberList = GlobalVariables.MySession.List_Group.Where(a => a.group_id == pm_group_id).ToList();

                foreach (var m in theData.memberList)
                {
                    var member = theData.accountList.FirstOrDefault(a => a.id == m.account_id);
                    if (member != null)
                        member.account_chk = true;
                }
            }

            return PartialView("_GroupDetails", theData);
        }


        public ActionResult RemoveGroup(int pm_group_id)
        {
            SP_Return result = pgmAuthority.RemoveGroup(pm_group_id);

            if (result.r == 0)
                TempData["message"] = result.msg;

            return RedirectToAction("AuthorityManagement");
        }


        public ActionResult SaveGroup(List<AccountItem> pmAccountList, string pm_group_name, int pm_group_id = -1)
        {/*   //for test
            List<Account> testList = new List<Account>();
            testList = pmAccountList.FindAll(a => a.chk == true).ToList();
            string theseBuys = string.Join(",", testList.Select(a => a.id).ToArray());
            TempData["message"] = pm_group_id.ToString() + "\r\n" + pm_group_name + "\r\n" + theseBuys; 
            */
            SP_Return result = pgmAuthority.UpdateGroup(pm_group_id, pm_group_name, pmAccountList);

            if (result.r == 0)
                TempData["message"] = result.msg;

            return RedirectToAction("AuthorityManagement");
        }


        public ActionResult GoCoverSettings(string pm_AccountID_source, string pm_AccountID_target)
        {
            SP_Return result = pgmAuthority.CoverSettings(pm_AccountID_source, pm_AccountID_target);

            if (result.r == 0)
                TempData["message"] = result.msg;

            return RedirectToAction("AuthorityManagement");
        }


        public PartialViewResult ShowQueryTree(string pm_account_id)
        {
            ViewData["pm_account_id"] = pm_account_id;
            ViewData["full_name"] = GlobalVariables.MySession.List_Account.Find(f => f.id == pm_account_id).full_name;
            ViewData["Distinct_Report_Cluster"] = scriptsLogin.FetchReportClusterForAccount(pm_account_id);

            return PartialView("_QueryDetails", scriptsQuery.FetchQueryAuthority(pm_account_id, (List<ReportItem>)ViewData["Distinct_Report_Cluster"]));
        }

        #endregion

    }
}