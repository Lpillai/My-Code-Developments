using Home.Models;
using RuntimeVariables;
using Sales;
using SharedScrpits;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace Home.Controllers
{
    [SessionExpireFilter]
    public class SalesController : Controller
    {
        Common_Share shareCommon = new Common_Share();
        Get_Share shareGet = new Get_Share();
        Convertor_Share shareConvertor = new Convertor_Share();
        HomeScripts scriptsHome = new HomeScripts();
        Sales_Fileter filterSales = new Sales_Fileter();
        AccountManager_Program pgmAccountManager = new AccountManager_Program();
        BOM_Program pgmBOM = new BOM_Program();


        #region Account Manager

        public ActionResult OpenSalesOrder(string pm_taker_group, string pm_taker)
        {
            int wkMenuID;
            switch (pm_taker_group)
            {
                case "KR":
                    wkMenuID = 133;
                    break;
                case "TW":
                    wkMenuID = 53;
                    break;
                case "VN":
                    wkMenuID = 56;
                    break;
                default:
                    wkMenuID = -1;
                    break;
            }
            ViewBag.PageInfo = shareCommon.GetPageInfo(wkMenuID);

            ViewBag.Dev = pgmAccountManager.devMode;
            ViewData["pmP21URL"] = (!scriptsHome.devStatus() ? 1 : 2);
            ViewData["pm_taker_group"] = pm_taker_group;
            ViewData["pm_taker"] = pm_taker;

            IEnumerable<SelectListItem> List_taker = shareConvertor.ToDropDownOptions(shareGet.FetchCodeList("Taker_" + pm_taker_group));
            List_taker.First().Value = "All";
            List_taker.First().Text = "All";
            ViewData["List_Takers"] = new SelectList(List_taker, "Text", "Text", (string.IsNullOrWhiteSpace(pm_taker) ? GlobalVariables.MySession.Account.ToUpper() : pm_taker));

            GlobalVariables.MySession.List_SO_OpenOrder = null;
            return View();
        }


        public PartialViewResult GetTakerOrders(string pm_taker_group, string pm_taker)
        {
            //ViewBag.authorized = shareCommon.isAuthorized(53);
            ViewData["pmP21URL"] = (!scriptsHome.devStatus() ? 1 : 2);

            if (GlobalVariables.MySession.List_SO_OpenOrder == null)
                pgmAccountManager.SetOpenSalesOrder(pm_taker_group);

            return PartialView("_TakerOrders", pgmAccountManager.GetTakerOrder(pm_taker));
        }


        public PartialViewResult GetItemInquiry(int pmP21URL, string pm_item_id, int pm_loc_id)
        {
            ViewData["pm_loc_id"] = pm_loc_id;
            return PartialView("_ItemInquiry", pgmAccountManager.FetchItemInquiry(pmP21URL, pm_item_id, pm_loc_id));
        }


        public FileStreamResult SheetToExcel_OpenSalesOrder(string pm_taker)
        {
            string pm_fileName = "Open SO " + DateTime.Now.ToString("MMddHHmmss", DateTimeFormatInfo.InvariantInfo) + ".xlsx";
            MemoryStream theSheet = pgmAccountManager.SheetToExcel_OpenSalesOrder(pm_taker);
            return File(theSheet, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", pm_fileName);
        }

        #endregion

        #region BOM

        public PartialViewResult GetPOInquiry(int i, int pm_week)
        {
            ViewData["ModalTitle"] = GlobalVariables.MySession.List_BOM_forecast.Find(f => f.i == i).customer_part_no + " (Week" + pm_week.ToString("00") + ")";
            return PartialView("_POInquiry", pgmBOM.GetBOM_forecast_incomes(i, pm_week));
        }


        public PartialViewResult GetProcureInfoInquiry(int i, int pm_ShortageWeek)
        {
            ViewBag.authorized = shareCommon.isAuthorized(78);
            return PartialView("_ProcureInfoInquiry", pgmBOM.GetBOM_Procure_Info(i, pm_ShortageWeek));
        }


        public ActionResult BOM_ASMLTW()
        {
            ViewBag.PageInfo = shareCommon.GetPageInfo(78);
            ViewBag.Dev = pgmBOM.devMode;

            SP_Return ResultModel = pgmBOM.PrepareBOM_ASMLTW();
            if (ResultModel.r != 1)
                TempData["message"] = ResultModel.msg;
            ViewData["WeekStart"] = DateTime.Parse(GlobalVariables.MySession.List_BOM_forecast.First().week01.period.Substring(0, 10));

            return View(GlobalVariables.MySession.List_BOM_forecast);
        }


        public FileStreamResult SheetToExcel_BOM_ASMLTW()
        {
            string pm_fileName = "BOM_ASMLTW " + DateTime.Now.ToString("MMddHHmmss", DateTimeFormatInfo.InvariantInfo) + ".xlsx";
            MemoryStream theSheet = pgmBOM.SheetToExcel_BOM_ASMLTW();
            return File(theSheet, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", pm_fileName);
        }


        [ValidateAntiForgeryToken]
        public ActionResult Go_UpdateUsage_ASMLTW(List<BOM_forecast> pm_BOM, DateTime pm_WeekStart)
        {
            pgmBOM.ReCalculateBOM(pm_BOM, pm_WeekStart);
            return RedirectToAction("BOM_ASMLTW");
        }


        [ValidateAntiForgeryToken]
        public ActionResult GoPostNewBuy_BOM_ASMLTW(BOM_ProcureInfo pm_Info)
        {
            SP_Return ResultModel = pgmBOM.PostNewBuy_BOM_ASMLTW(pm_Info);
            TempData["message"] = ResultModel.msg;
            return RedirectToAction("BOM_ASMLTW");
        }

        #endregion

        #region Filter

        public PartialViewResult FilterSO(string pm_taker_group, string pm_taker)
        {
            List<string> theParameters = new List<string>();
            theParameters.Add(pm_taker_group);
            theParameters.Add(pm_taker);

            ViewData["pmParameters"] = String.Join(",", theParameters.ToArray());
            ViewData["goController"] = "Sales";
            ViewData["goAction"] = "actFilter";
            return PartialView("../Home/_Filters", filterSales.getFilters());
        }

        /*
        [HttpPost]
        [MultipleButton(Name = "FilterAction", Argument = "Reset")]
        public ActionResult goReset(string pmParameters)
        {
            List<string> theParameters = pmParameters.Split(',').ToList();
            filterSales.PostFilter(null);
            return RedirectToAction("OpenSalesOrder", new { pm_taker_group = theParameters[0], pm_taker = theParameters[1] });
        }


        [HttpPost]
        [MultipleButton(Name = "FilterAction", Argument = "Filter")]
        public ActionResult goFilter(List<Filters> pmFilterList, string pmParameters)
        {
            List<string> theParameters = pmParameters.Split(',').ToList();
            filterSales.PostFilter(pmFilterList);
            return RedirectToAction("OpenSalesOrder", new { pm_taker_group = theParameters[0], pm_taker = theParameters[1] });
        }
        */

        public ActionResult actFilter(List<Filters> pmFilterList, string pmParameters, string filter_action)
        {
            List<string> theParameters = pmParameters.Split(',').ToList();

            if (filter_action == "Filter")
                filterSales.PostFilter(pmFilterList);
            else
                filterSales.PostFilter(null);

            return RedirectToAction("OpenSalesOrder", new { pm_taker_group = theParameters[0], pm_taker = theParameters[1] });
        }

        #endregion

    }
}