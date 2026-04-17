using Home.Models;
using P21;
using RuntimeVariables;
using SharedScrpits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Warehouse;


namespace Home.Controllers
{
    [SessionExpireFilter]
    public class WarehouseController : Controller
    {
        Common_Share shareCommon = new Common_Share();
        HomeScripts scriptsHome = new HomeScripts();
        Packing_Program pgmPacking = new Packing_Program();
        Receiving_Program pgmReceiving = new Receiving_Program();
        Integration_Program pgmIntegration = new Integration_Program();


        #region Receiving

        public PartialViewResult ShowItemNetWeight(string pm_item_id)
        {
            ViewBag.authorized = shareCommon.isAuthorized(75);
            return PartialView("_ItemNetWeight", pgmIntegration.Fetch_item_net_weight(pm_item_id));
        }


        public ActionResult ItemNetWeight(string pm_item_id = null)
        {
            ViewBag.PageInfo = shareCommon.GetPageInfo(75);
            ViewBag.Dev = pgmReceiving.devMode;
            ViewData["item_id"] = pm_item_id;

            return View();
        }


        [ValidateAntiForgeryToken]
        public async Task<ActionResult> GoSaveItemNetWeightAsync(item_net_weight pmItem)
        {
            SP_Return ResultModel = await pgmIntegration.SaveItemNetWeightAsync(pmItem);
            TempData["message"] = ResultModel.msg;
            return RedirectToAction("ItemNetWeight", new { pm_item_id = pmItem.item_id });
        }

        #endregion

        #region Packing

        public PartialViewResult ShowPalletizingRecord2(int pm_pick_ticket_no)
        {
            ViewBag.authorized = shareCommon.isAuthorized(88);
            ViewData["pick_ticket_no"] = pm_pick_ticket_no;

            List<palletizing_record2_line> wkPT = new List<palletizing_record2_line>();
            if (TempData["message"] != null)
                wkPT = GlobalVariables.MySession.List_PalletizingRecord2_Line;
            else
            {
                TempData["message"] = TempData["UpdateMSG"];
                wkPT = pgmPacking.Fetch_PalletizingRecord2(pm_pick_ticket_no);
            }

            return PartialView("_PalletizingRecord2", wkPT);
        }


        public ActionResult PalletizingRecord2()
        {
            ViewBag.PageInfo = shareCommon.GetPageInfo(88);
            ViewBag.Dev = pgmPacking.devMode;
            ViewData["pick_ticket_no"] = TempData["pm_pick_ticket_no"];

            if (string.IsNullOrWhiteSpace(GlobalVariables.MySession.packing_person))
                pgmPacking.SetSession_packing_person(GlobalVariables.MySession.Account);

            ViewData["packing_person"] = GlobalVariables.MySession.packing_person;

            return View();
        }


        [ValidateAntiForgeryToken]
        public ActionResult GoUpdate_PackingPerson(string pm_packing_person)
        {
            pgmPacking.SetSession_packing_person(pm_packing_person);
            return RedirectToAction("PalletizingRecord2");
        }


        [ValidateAntiForgeryToken]
        public ActionResult GoUpdate_Palletizing(int pm_pick_ticket_no, List<palletizing_record2> pm_lines)
        {
            SP_Return ResultModel = pgmPacking.Set_PalletizingRecord2(pm_pick_ticket_no, pm_lines);
            if (ResultModel.r == 0)
                TempData["message"] = ResultModel.msg;
            else
                TempData["UpdateMSG"] = ResultModel.msg;
            TempData["pm_pick_ticket_no"] = pm_pick_ticket_no;
            return RedirectToAction("PalletizingRecord2");
        }


        public PartialViewResult ShowPalletizingData(int pm_pick_ticket_no)
        {
            ViewBag.authorized = shareCommon.isAuthorized(77);
            return PartialView("_PalletizingData", pgmPacking.Fetch_PalletizingData(pm_pick_ticket_no));
        }


        public ActionResult PalletizingChecking()
        {
            ViewBag.PageInfo = shareCommon.GetPageInfo(77);
            ViewBag.Dev = pgmPacking.devMode;
            ViewBag.authorized = shareCommon.isAuthorized(77);
            return View();
        }


        [ValidateAntiForgeryToken]
        public ActionResult GoSavePalletizingChecking(int pm_pick_ticket_no)
        {
            SP_Return ResultModel = pgmPacking.SavePalletizingChecking(pm_pick_ticket_no);
            TempData["message"] = ResultModel.msg;
            return RedirectToAction("PalletizingChecking");
        }

        #endregion

    }
}