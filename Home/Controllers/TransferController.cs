using Home.Models;
using RuntimeVariables;
using SharedScrpits;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Transfer;


namespace Home.Controllers
{
    [SessionExpireFilter]
    public class TransferController : Controller
    {
        Common_Share shareCommon = new Common_Share();
        Get_Share shareGet = new Get_Share();
        Convertor_Share shareConvertor = new Convertor_Share();
        HomeScripts scriptsHome = new HomeScripts();
        Logistics_Program pgmLogistics = new Logistics_Program();
        ShippingCalendar_Program pgmShippingCalendar = new ShippingCalendar_Program();
        TransitTime_Program pgmTransitTime = new TransitTime_Program();


        public PartialViewResult show_Preloads(int pmP21URL, int pmFrom, string pmTo)
        {
            return PartialView("_Preloads", pgmLogistics.FetchOpenPreloads(pmP21URL, pmFrom, pmTo));
        }


        public ActionResult PreloadList()
        {
            ViewBag.PageInfo = shareCommon.GetPageInfo(54);
            ViewBag.Dev = pgmLogistics.devMode;
            ViewData["pmP21URL"] = (!scriptsHome.devStatus() ? 1 : 2);
            ViewData["List_Location"] = new SelectList(shareGet.GetList_Location(), "location_id", "id_name");

            return View();
        }


        public PartialViewResult show_InternationalShipments(string pmPickTickets)
        {
            return PartialView("_InternationalShipments", pgmLogistics.FetchCustomsClearanceInformation(pmPickTickets));
        }


        public ActionResult InternationalShipmentData()
        {
            ViewBag.PageInfo = shareCommon.GetPageInfo(55);
            ViewBag.Dev = pgmLogistics.devMode;

            return View();
        }


        #region Open Transfers

        [HttpGet]
        public ActionResult OpenTransfers()
        {
            ViewBag.PageInfo = shareCommon.GetPageInfo(25);
            ViewBag.authorized = shareCommon.isAuthorized(25);
            ViewBag.Dev = pgmLogistics.devMode;
            return View(pgmLogistics.FetchOpenTransfersList());
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdatePlannedRecptDate(List<Transfer_PlannedRecptDate_Override> pmShippingTime)
        {
            SP_Return theResult = pgmLogistics.UpdatePlannedRecptDate(pmShippingTime);

            if (theResult.r == 1)
                TempData["message"] = "Planned Recpt Date update completed.";
            else
                TempData["message"] = theResult.msg;

            return RedirectToAction("OpenTransfers");
        }

        #endregion

        #region Shipping Calendar

        public ActionResult GetLocationGrpList(string pm_country_cd) //It will be fired from Jquery ajax call
        {
            var jsonData = pgmShippingCalendar.GetList_ShippingLocGrp_Location(pm_country_cd).ToList();
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }


        public ActionResult GetLocationIDs(string pm_country_cd, int pm_group_id) //It will be fired from Jquery ajax call
        {
            List<Transfer_ShippingLocation> theList = pgmShippingCalendar.GetList_ShippingLocGrp_Location(pm_country_cd).ToList();
            var jsonData = theList.Where(w => w.group_id == pm_group_id).ToList().FirstOrDefault().location_id;
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }


        public ActionResult ShippingCalendar_Management()
        {
            ViewBag.PageInfo = shareCommon.GetPageInfo(57);
            ViewBag.Dev = pgmShippingCalendar.devMode;
            string pm_country_cd = (TempData["wk_country_cd"] == null ? null : TempData["wk_country_cd"].ToString());
            int pm_location_cd = Int32.Parse((TempData["wk_location_cd"] == null ? "0" : TempData["wk_location_cd"].ToString()));

            List<Transfer_ShippingCountry> theCountrys = pgmShippingCalendar.GetList_ShippingLocGrp_Country();
            ViewData["List_fromCountry"] = new SelectList(theCountrys, "country_cd", "country_name", pm_country_cd);
            TempData["pm_country_cd"] = pm_country_cd;

            if (string.IsNullOrWhiteSpace(pm_country_cd))
                pm_country_cd = theCountrys.First().country_cd;
            List<Transfer_ShippingLocation> theLoactions = pgmShippingCalendar.GetList_ShippingLocGrp_Location(pm_country_cd);
            ViewData["List_toLocationGroup"] = new SelectList(theLoactions, "group_id", "loc_cd", pm_location_cd);
            TempData["pm_location_cd"] = (pm_location_cd == 0 ? null : pm_location_cd.ToString());
            if (theLoactions.Where(w => w.group_id == pm_location_cd).Count() <= 0)
                ViewData["loc_IDs"] = theLoactions.First().location_id;
            else
                ViewData["loc_IDs"] = theLoactions.Find(f => f.group_id == pm_location_cd).location_id;

            return View();
        }


        public PartialViewResult ShippingCalendar_Details(string pm_country_cd, int pm_location_cd)
        {
            ViewBag.authorized = shareCommon.isAuthorized(57);
            ViewData["pm_country_cd"] = pm_country_cd;
            ViewData["pm_location_cd"] = pm_location_cd;

            Transfer_ShippingLocGrp thisData = GlobalVariables.MySession.List_ShippingLocGrp.Where(w => w.country_cd == pm_country_cd && w.group_id == pm_location_cd).FirstOrDefault();
            ViewData["pm_new_label"] = thisData.country_name + " - " + thisData.loc_cd;

            return PartialView("_CalendarDetails", pgmShippingCalendar.FetchShippingCalendar2(pm_country_cd, pm_location_cd));
        }


        [ValidateAntiForgeryToken]
        public ActionResult GoCreateShippingCalendar(string pm_country_cd, int pm_location_cd, string pm_ship_method, DateTime pm_ship_date)
        {
            SP_Return resultModel = pgmShippingCalendar.GoCreateShippingCalendar(pm_country_cd, pm_location_cd, pm_ship_method, pm_ship_date);
            TempData["message"] = resultModel.msg;
            TempData["wk_country_cd"] = pm_country_cd;
            TempData["wk_location_cd"] = pm_location_cd;
            return RedirectToAction("ShippingCalendar_Management");
        }


        [ValidateAntiForgeryToken]
        public ActionResult GoSaveShippingCalendar(string pm_country_cd, int pm_location_cd, Transfer_ShippingCalendar_Method pmShipDates)
        {
            List<Transfer_ShippingCalendar> allMethod = new List<Transfer_ShippingCalendar>();
            if (pmShipDates.Ocean != null && pmShipDates.Ocean.Count() > 0)
                allMethod.AddRange(pmShipDates.Ocean.Select(s => new Transfer_ShippingCalendar { date_uid = s.date_uid, ship_method = "Ocean", ship_date = s.ship_date, z_status = "", i = 0 }).ToList());
            if (pmShipDates.Air != null && pmShipDates.Air.Count() > 0)
                allMethod.AddRange(pmShipDates.Air.Select(s => new Transfer_ShippingCalendar { date_uid = s.date_uid, ship_method = "Air", ship_date = s.ship_date, z_status = "", i = 0 }).ToList());

            SP_Return resultModel = pgmShippingCalendar.GoSaveShippingCalendar(pm_country_cd, pm_location_cd, allMethod);
            TempData["message"] = resultModel.msg;
            TempData["wk_country_cd"] = pm_country_cd;
            TempData["wk_location_cd"] = pm_location_cd;
            return RedirectToAction("ShippingCalendar_Management");
        }


        public ActionResult GoDeleteShippingCalendar(string pm_country_cd, int pm_location_cd, int pm_date_uid)
        {
            SP_Return resultModel = pgmShippingCalendar.GoDeleteShippingCalendar(pm_date_uid);
            TempData["message"] = resultModel.msg;
            TempData["wk_country_cd"] = pm_country_cd;
            TempData["wk_location_cd"] = pm_location_cd;
            return RedirectToAction("ShippingCalendar_Management");
        }

        #endregion

        #region TW Logistics

        public PartialViewResult Get_Orders(int pmP21URL, int pmFrom, string pmTo, DateTime pmDate)
        {
            ViewBag.authorized = shareCommon.isAuthorized(51);
            Shipping_Orders theOrders = pgmLogistics.FetchShippingOrders(pmP21URL, pmFrom, pmTo, pmDate);
            ViewData["pm_theDate"] = pmDate.ToString("yyyy-MM-dd");
            return PartialView("_ShippingOrders", theOrders);
        }


        public ActionResult ShippingOrder(int pmFrom)
        {
            ViewBag.PageInfo = shareCommon.GetPageInfo(51);
            ViewBag.Dev = pgmLogistics.devMode;
            ViewData["pmFrom"] = pmFrom.ToString();
            ViewData["pmP21URL"] = (!scriptsHome.devStatus() ? 1 : 2);

            if (pmFrom == 13)
                ViewData["List_LocGrp"] = new SelectList(shareConvertor.ToDropDownOptions(shareGet.FetchCodeList("Shipping_LocGrp_TW", 2)), "Text", "Value");

            return View();
        }


        public async Task<ActionResult> RunExpectedShipDateModifying(Shipping_Orders pm_orders)
        {
            SP_Return ResultModel = await pgmLogistics.GoExpectedShipDateModifying(pm_orders);
            TempData["message"] = ResultModel.msg;
            return RedirectToAction("ShippingOrder", new { pmFrom = pm_orders.from_loc });
        }

        #endregion

        #region Transit

        public ActionResult TransitManagement_Country()
        {
            ViewBag.PageInfo = shareCommon.GetPageInfo(38);
            ViewBag.Dev = pgmTransitTime.devMode;
            ViewBag.authorized = shareCommon.isAuthorized(38);
            shareGet.FetchList_Country();

            return View("TransitManagement_Country", pgmTransitTime.FetchAllTransitTime_Country());
        }


        public ActionResult TransitManagement_Location()
        {
            ViewBag.PageInfo = shareCommon.GetPageInfo(40);
            ViewBag.Dev = pgmTransitTime.devMode;
            ViewBag.authorized = shareCommon.isAuthorized(40);
            return View("TransitManagement_Location", pgmTransitTime.FetchAllTransitTime_Location());
        }


        public PartialViewResult ShowTransit_CountryDetails(int pm_RRN)
        {
            ViewBag.authorized = shareCommon.isAuthorized(38);
            BuySheet_TransitTime_Country theTransitTime = new BuySheet_TransitTime_Country();
            theTransitTime.i = 0;

            ViewBag.pmCRUD = "C";
            ViewData["List_fromCountry"] = new SelectList(shareGet.FetchList_Country(), "country_code", "country_name", theTransitTime.from_country_id);
            ViewData["List_toLocation"] = new SelectList(shareGet.GetList_Location(), "location_id", "id_name", theTransitTime.to_loc_id);

            return PartialView("_TransitCountryDetails", theTransitTime);
        }


        public PartialViewResult ShowTransit_LocationDetails(int pm_RRN)
        {
            ViewBag.authorized = shareCommon.isAuthorized(40);
            BuySheet_TransitTime_Location theTransitTime = new BuySheet_TransitTime_Location();
            if (pm_RRN == 0)
            {
                theTransitTime.i = 0;
                ViewBag.pmCRUD = "C";
            }
            else
            {
                theTransitTime = GlobalVariables.MySession.List_TransitTime_Location.Find(t => t.i == pm_RRN);
                ViewBag.pmCRUD = "U";
            }

            ViewData["List_fromLocation"] = new SelectList(shareGet.GetList_Location(), "location_id", "id_name", theTransitTime.from_loc_id);
            ViewData["List_toLocation"] = new SelectList(shareGet.GetList_Location(), "location_id", "id_name", theTransitTime.to_loc_id);

            return PartialView("_TransitLocationDetails", theTransitTime);
        }


        public PartialViewResult ShowTransit_CountryMirror()
        {
            ViewBag.authorized = shareCommon.isAuthorized(38);
            ViewData["List_SourceCountry"] = pgmTransitTime.getTransitTime_SourceCountryList();
            ViewData["List_TargetCountry"] = pgmTransitTime.getTransitTime_TargetCountryList();

            return PartialView("_TransitCountryMirror");
        }


        public PartialViewResult ShowTransit_LocationMirror()
        {
            ViewBag.authorized = shareCommon.isAuthorized(40);
            ViewData["List_SourceLocation"] = pgmTransitTime.getTransitTime_SourceLocationList();
            ViewData["List_TargetLocation"] = pgmTransitTime.getTransitTime_TargetLocationList();

            return PartialView("_TransitLocationMirror");
        }


        public PartialViewResult ShowTransit_CountryBulk(string pm_from_country_id, string pm_ship_method)
        {
            ViewBag.authorized = shareCommon.isAuthorized(38);
            List<BuySheet_TransitTime_Country> theTransitTime = pgmTransitTime.getTransitTime_CountryBulkList(pm_from_country_id, pm_ship_method);
            ViewData["fromCountry"] = theTransitTime.First().from_country_name + " - " + pm_ship_method;

            return PartialView("_TransitCountryBulk", theTransitTime);
        }


        public PartialViewResult ShowTransit_LocationBulk(decimal pm_from_loc_id)
        {
            ViewBag.authorized = shareCommon.isAuthorized(40);
            List<BuySheet_TransitTime_Location> theTransitTime = pgmTransitTime.getTransitTime_LocationBulkList(pm_from_loc_id);
            ViewData["fromLocation"] = theTransitTime[0].from_loc_id + " - " + theTransitTime[0].from_loc_name;

            return PartialView("_TransitLocationBulk", theTransitTime);
        }


        [ValidateAntiForgeryToken]
        public ActionResult GoMirrorTransitTime_Country(string source_country_id, string target_country_id)
        {
            SP_Return resultModel = pgmTransitTime.GoMirrorTransitTime_Country(source_country_id, target_country_id);
            TempData["message"] = resultModel.msg;

            return RedirectToAction("TransitManagement_Country");
        }


        [ValidateAntiForgeryToken]
        public ActionResult GoMirrorTransitTime_Location(decimal source_loc_id, decimal target_loc_id)
        {
            SP_Return resultModel = pgmTransitTime.GoMirrorTransitTime_Location(source_loc_id, target_loc_id);
            TempData["message"] = resultModel.msg;
            return RedirectToAction("TransitManagement_Location");
        }


        [ValidateAntiForgeryToken]
        public ActionResult GoSaveTransitTime_CountryBulk(List<BuySheet_TransitTime_Country> pmTransit)
        {
            SP_Return resultModel = pgmTransitTime.GoSaveTransitTime_CountryBulk(pmTransit);
            TempData["message"] = resultModel.msg;

            return RedirectToAction("TransitManagement_Country");
        }


        [ValidateAntiForgeryToken]
        public ActionResult GoSaveTransitTime_LocationBulk(List<BuySheet_TransitTime_Location> pmTransit)
        {
            SP_Return resultModel = pgmTransitTime.GoSaveTransitTime_LocationBulk(pmTransit);
            TempData["message"] = resultModel.msg;

            return RedirectToAction("TransitManagement_Location");
        }


        [ValidateAntiForgeryToken]
        public ActionResult GoSaveTransitTime_Country(string pmCRUD, BuySheet_TransitTime_Country pmTransit)
        {
            SP_Return resultModel = pgmTransitTime.GoSaveTransitTime_Country(pmCRUD, pmTransit);
            TempData["message"] = resultModel.msg;
            return RedirectToAction("TransitManagement_Country");
        }


        [ValidateAntiForgeryToken]
        public ActionResult GoSaveTransitTime_Location(string pmCRUD, BuySheet_TransitTime_Location pmTransit)
        {
            SP_Return resultModel = pgmTransitTime.GoSaveTransitTime_Location(pmCRUD, pmTransit);
            TempData["message"] = resultModel.msg;
            return RedirectToAction("TransitManagement_Location");
        }


        public ActionResult GoDeleteTransit_Country(int pm_RRN, string pm_ship_method)
        {
            SP_Return resultModel = new SP_Return();
            if (pm_ship_method == "Ocean")
                pgmTransitTime.GoSaveTransitTime_Country("D", GlobalVariables.MySession.TransitTime_Country_Method.Ocean.Find(t => t.i == pm_RRN));
            if (pm_ship_method == "Air")
                pgmTransitTime.GoSaveTransitTime_Country("D", GlobalVariables.MySession.TransitTime_Country_Method.Air.Find(t => t.i == pm_RRN));
            TempData["message"] = resultModel.msg;
            return RedirectToAction("TransitManagement_Country");
        }


        public ActionResult GoDeleteTransit_Location(int pm_RRN)
        {
            SP_Return resultModel = pgmTransitTime.GoSaveTransitTime_Location("D", GlobalVariables.MySession.List_TransitTime_Location.Find(t => t.i == pm_RRN));
            TempData["message"] = resultModel.msg;
            return RedirectToAction("TransitManagement_Location");
        }


        public ActionResult TransitManagement_DefaultAir()
        {
            ViewBag.PageInfo = shareCommon.GetPageInfo(91);
            ViewBag.Dev = pgmTransitTime.devMode;
            ViewBag.authorized = shareCommon.isAuthorized(91);
            ViewData["List_fromCountry"] = new SelectList(shareGet.FetchList_Country(), "country_code", "country_name");
            ViewData["List_toLocation"] = new SelectList(shareGet.GetList_Location(), "location_id", "id_name");

            return View("TransitManagement_DefaultAir", pgmTransitTime.FetchAllTransit_DefaultAir());
        }


        public FileStreamResult GoExportDefaultAirSheet()
        {
            string fileName = "DefaultAir_" + DateTime.Now.ToString("yyyyMMdd") + ".xlsx";
            MemoryStream example = pgmTransitTime.ExportDefaultAirSheet();
            return File(example, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }


        [ValidateAntiForgeryToken]
        public ActionResult GoSaveTransit_DefaultAir(BuySheet_TransitTime_DefaultAir pmTransit)
        {
            SP_Return resultModel = pgmTransitTime.GoSaveTransit_DefaultAir(pmTransit);
            TempData["message"] = resultModel.msg;
            return RedirectToAction("TransitManagement_DefaultAir");
        }


        public ActionResult GoDeleteTransit_DefaultAir(string pm_item_id, string pm_from_country_id, decimal pm_to_loc_id)
        {
            SP_Return resultModel = pgmTransitTime.DeleteTransit_DefaultAir(pm_item_id, pm_from_country_id, pm_to_loc_id);
            TempData["message"] = resultModel.msg;
            return RedirectToAction("TransitManagement_DefaultAir");
        }

        #endregion

    }
}