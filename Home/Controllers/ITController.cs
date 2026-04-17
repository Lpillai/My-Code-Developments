using IT;
using RuntimeVariables;
using SharedScrpits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web;
using System.Web.Mvc;


namespace Home.Controllers
{
    [SessionExpireFilter]
    public class ITController : Controller
    {
        Common_Share shareCommon = new Common_Share();
        Get_Share shareGet = new Get_Share();
        Convertor_Share shareConvertor = new Convertor_Share();
        IT_Program pgmIT = new IT_Program();
        Code_Program pgmCode = new Code_Program();
        Inventory_Program pgmInventory = new Inventory_Program();


        public ActionResult IntranetSessionList()
        {
            ViewBag.PageInfo = shareCommon.GetPageInfo(34);
            ViewBag.Dev = pgmIT.devMode;

            List<string> CurrentSession = new List<string>();
            foreach (var crntSession in Session)
                CurrentSession.Add(string.Concat(crntSession, "=", Session[crntSession.ToString()]));
            //ViewData["CurrentSession"] = CurrentSession.ToList();
            ViewData["CurrentSession"] = CurrentSession.ToArray().ToString();

            MemoryCache cache = MemoryCache.Default;
            List<string> cacheKeys = cache.Select(kvp => "KEY=" + kvp.Key + ", VALUE=" + kvp.Value.ToString()).ToList();
            ViewData["CurrentCache"] = cacheKeys.ToList();

            return View();
        }


        public ActionResult P21SessionList()
        {
            ViewBag.PageInfo = shareCommon.GetPageInfo(36);
            ViewBag.Dev = pgmIT.devMode;
            ViewBag.authorized = shareCommon.isAuthorized(36);
            P21SessionData theData = new P21SessionData();
            theData.sessionList = pgmIT.FetchP21SessionList();

            return View(theData);
        }


        [ValidateAntiForgeryToken]
        public ActionResult GoDeleteSessions(List<P21Session> sessionList)
        {
            SP_Return theResult = pgmIT.DeleteSessions(sessionList);
            TempData["message"] = theResult.msg;

            return RedirectToAction("P21SessionList");
        }

        #region Code

        public PartialViewResult Show_ClusterDetails(string pm_cluster)
        {
            ViewBag.authorized = shareCommon.isAuthorized(58);
            ViewData["pm_cluster"] = pm_cluster;
            return PartialView("_Code_ClusterDetails", pgmCode.Code_FetchClusterDetails(pm_cluster));
        }


        public PartialViewResult Show_CodeDetails(string pm_CRUD, string pm_cluster, int pm_code)
        {
            ViewBag.authorized = shareCommon.isAuthorized(58);
            //ViewData["pm_cluster"] = pm_cluster;
            ViewBag.pmCRUD = pm_CRUD;

            CodeStore theCode = new CodeStore();
            if (pm_CRUD != "C")
                theCode = GlobalVariables.MySession.List_CodeStore.First(f => f.Cluster == pm_cluster && f.Code == pm_code);
            return PartialView("_Code_CodeDetails", theCode);
        }


        public ActionResult Code_ClusterList(string pm_cluster = "")
        {
            ViewBag.PageInfo = shareCommon.GetPageInfo(58);
            ViewBag.Dev = pgmCode.devMode;
            ViewData["pm_cluster"] = pm_cluster;
            return View(pgmCode.Code_FetchClusterList());
        }


        [ValidateAntiForgeryToken]
        public ActionResult GoSaveCode(CodeStore pm_code)
        {
            SP_Return theResult = pgmCode.Code_SetCode(pm_code);
            TempData["message"] = theResult.msg;

            //return RedirectToAction("Code_ClusterList");
            return RedirectToAction("Code_ClusterList", new { pm_cluster = pm_code.Cluster });
        }

        #endregion

        #region Inventory

        public PartialViewResult ShowBinDetails(string pm_dpt, int pm_uid)
        {
            Inventory_Bin theBin = new Inventory_Bin();
            bool isNew = true;
            theBin.dpt = pm_dpt;

            if (pm_uid > 0)
            {
                isNew = false;
                theBin = pgmInventory.Inventory_GetOneBin(pm_dpt, pm_uid);
            }

            ViewBag.authorized = shareCommon.isAuthorized(112);
            ViewData["isNew"] = isNew;
            return PartialView("_Inventory_BinDetails", theBin);
        }


        public ActionResult Inventory_BinManagement(string dpt)
        {
            switch (dpt)
            {
                case "IT":
                    ViewBag.PageInfo = shareCommon.GetPageInfo(112);
                    break;
                case "TW":
                    ViewBag.PageInfo = shareCommon.GetPageInfo(118);
                    break;
                default:
                    break;
            }

            ViewBag.Dev = pgmInventory.devMode;
            ViewData["dpt"] = dpt;
            return View(pgmInventory.Inventory_FetchBinData(dpt));
        }


        [ValidateAntiForgeryToken]
        public ActionResult Inventory_GoSaveBin(Inventory_Bin pm_bin)
        {
            SP_Return theResult = pgmInventory.Inventory_UpdateBin(pm_bin);
            TempData["message"] = theResult.msg;
            return RedirectToAction("Inventory_BinManagement", new { dpt = pm_bin.dpt });
        }


        public PartialViewResult ShowItemActions(string pm_dpt, string pm_bin_cd, string pm_action, int pm_item_uid, string pm_item_id, string pm_storage_code = "")
        {
            string thePartial = "";
            Inventory_Item theItem = GlobalVariables.MySession.List_InventoryItem.Find(f => f.item_uid == pm_item_uid);

            ViewData["pm_dpt"] = pm_dpt;
            ViewData["pm_bin_cd"] = pm_bin_cd;
            ViewData["pm_item_uid"] = pm_item_uid;
            ViewData["pm_item_id"] = pm_item_id;
            ViewData["pm_storage_code"] = pm_storage_code;

            if (pm_action != "Purchase")
                ViewData["isConsumable"] = theItem.isConsumable;

            List<Inventory_Bin> BinList = new List<Inventory_Bin>();
            switch (pm_action)
            {
                case "Move":
                    BinList = pgmInventory.Inventory_GetAvailableBinList(pm_dpt);
                    break;
                case "Carry":
                    BinList = pgmInventory.Inventory_Locations(pm_dpt).FindAll(f => f.bin_cd != pm_bin_cd);
                    break;
                default:
                    BinList = pgmInventory.Inventory_GetAvailableBinList(pm_dpt).FindAll(f => f.storage_code.Substring(0, 2) == pm_bin_cd);
                    break;
            }
            ViewData["AvailableBins"] = new SelectList(BinList, "storage_code", "bin_hierarchy");
            ViewData["AvailableLocs"] = new SelectList(BinList, "bin_cd", "bin_hierarchy");

            switch (pm_action)
            {
                case "Purchase":
                    var selection = new List<SelectListItem>();
                    selection.AddRange(shareConvertor.ToDropDownOptions(shareGet.FetchCodeList(pm_dpt + "_Category", 1).FindAll(f => f.Code != "0")));
                    ViewData["CategoryList"] = selection;
                    thePartial = "_Inventory_ActionPurchase";
                    break;
                case "Update":
                    thePartial = "_Inventory_ActionUpdate";
                    ViewData["pm_item_desc"] = theItem.item_desc;
                    break;
                case "Move":
                    thePartial = "_Inventory_ActionMove";
                    break;
                case "Carry":
                    thePartial = "_Inventory_ActionCarry";
                    break;
                case "Requisition":
                    thePartial = "_Inventory_ActionRequisition";
                    break;
                case "Return":
                    thePartial = "_Inventory_ActionReturn";
                    break;
                case "Scrap":
                    thePartial = "_Inventory_ActionScrap";
                    break;
                case "Maintain":
                    thePartial = "_Inventory_ActionMaintain";
                    break;
                default:
                    ViewData["msg_title"] = "Warning";
                    ViewData["msg_text"] = "No setting for this action.";
                    thePartial = "../Home/_Message";
                    break;
            }

            return PartialView(thePartial);
        }


        public PartialViewResult ShowItemHistory(int pm_item_uid, string pm_item_id)
        {
            ViewData["item_id"] = pm_item_id;
            return PartialView("_Inventory_ItemHistory", pgmInventory.Inventory_GetOneItemHistory(pm_item_uid));
        }


        public PartialViewResult ShowItemInfo(int pm_item_uid)
        {
            ViewBag.authorized = shareCommon.isAuthorized(113);
            ViewData["item_uid"] = pm_item_uid;
            return PartialView("_Inventory_ItemInfo", GlobalVariables.MySession.List_InventoryItem.Find(f => f.item_uid == pm_item_uid));
        }


        public PartialViewResult ShowInventoryDetails(string pm_dpt, string pm_bin_cd)
        {
            System.Web.HttpBrowserCapabilitiesBase myBrowserCaps = Request.Browser;
            ViewBag.IsMobileDevice = myBrowserCaps.IsMobileDevice;
            //ViewBag.size = Request.Browser.ScreenCharactersWidth.ToString() + " x " + Request.Browser.ScreenCharactersHeight.ToString();
            ViewBag.size = (Request.Browser.ScreenPixelsWidth * 2 - 100).ToString() + " x " + (Request.Browser.ScreenPixelsHeight * 2 - 100).ToString();

            ViewBag.authorized = shareCommon.isAuthorized(113);
            ViewData["bin_cd"] = pm_bin_cd;
            ViewData["pm_dpt"] = pm_dpt;
            return PartialView("_Inventory_ItemDetails", pgmInventory.Inventory_GetItemList(pm_dpt, pm_bin_cd));
        }


        public ActionResult Inventory_ItemManagement(string dpt)
        {
            switch (dpt)
            {
                case "IT":
                    ViewBag.PageInfo = shareCommon.GetPageInfo(113);
                    break;
                case "TW":
                    ViewBag.PageInfo = shareCommon.GetPageInfo(119);
                    break;
                default:
                    break;
            }
            ViewBag.Dev = pgmInventory.devMode;
            ViewData["dpt"] = dpt;

            List<Inventory_Bin> LocationList = pgmInventory.Inventory_Locations(dpt);
            ViewData["List_Locations"] = new SelectList(LocationList, "bin_cd", "z_memo", (TempData["pm_bin_cd"] == null ? null : TempData["pm_bin_cd"].ToString()));

            //return View(pgmIT.Inventory_FetchItemList());
            return View();
        }


        [ValidateAntiForgeryToken]
        public ActionResult Inventory_GoSaveItem(string pm_bin_cd, Inventory_Item pm_item, string pm_storage_code)
        {
            TempData["pm_bin_cd"] = pm_bin_cd;
            SP_Return theResult = pgmInventory.Inventory_CreateItem(pm_item, pm_storage_code);
            TempData["message"] = theResult.msg;
            return RedirectToAction("Inventory_ItemManagement", new { dpt = pm_item.dpt });
        }


        [ValidateAntiForgeryToken]
        public ActionResult Inventory_GoItemAction(string pm_bin_cd, Inventory_History pm_history)
        {
            TempData["pm_bin_cd"] = pm_bin_cd;
            if (pm_history.behavior == "Carry")
                TempData["pm_bin_cd"] = pm_history.storage_code;
            SP_Return theResult = pgmInventory.Inventory_AppendHistory(pm_history, "");
            TempData["message"] = theResult.msg;
            return RedirectToAction("Inventory_ItemManagement", new { dpt = pm_history.dpt });
        }


        public ActionResult Inventory_ItemSummary()
        {
            ViewBag.PageInfo = shareCommon.GetPageInfo(114);
            ViewBag.Dev = pgmInventory.devMode;
            return View(pgmCode.Code_FetchClusterList());
        }

        #endregion

    }
}