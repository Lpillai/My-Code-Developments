using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using RuntimeVariables;
using SharedScrpits;
using Procurement;
using Transfer;
using Home.Models;
using System.Configuration;
using Ganss.Xss;

namespace Home.Controllers
{
    [SessionExpireFilter]
    public class ProcurementController : Controller
    {
        HomeScripts scriptsHome = new HomeScripts();
        Common_Share shareCommon = new Common_Share();
        Get_Share shareGet = new Get_Share();
        Convertor_Share shareConvertor = new Convertor_Share();
        FileActions_Share shareFileActions = new FileActions_Share();
        Buy_Fileter filterBuy = new Buy_Fileter();
        BuySheet_Program pgmBuySheet = new BuySheet_Program();
        BuyNote_Program pgmBuyNote = new BuyNote_Program();
        BuySplits_Program pgmBuySplits = new BuySplits_Program();
        BuyPO_Program pgmBuyPO = new BuyPO_Program();
        BuyArchive_Program pgmBuyArchive = new BuyArchive_Program();
        BuyOthers_Program pgmBuyOthers = new BuyOthers_Program();
        SupplierScorecard_Program pgmSupplierScorecard = new SupplierScorecard_Program();
        TransitTime_Program pgmTransitTime = new TransitTime_Program();


        public ContentResult GetHeaderLink(string cluster, string fieldName)
        {
            ContentResult result = new ContentResult();
            result.Content = pgmBuySheet.GetHeaderLink(cluster, fieldName);
            result.ContentType = "text/html";
            return result;
        }


        public ContentResult GetHeaderLink_FTB(string fieldName)
        {
            ContentResult result = new ContentResult();
            result.Content = pgmBuySheet.GetHeaderLink_FTB(fieldName);
            result.ContentType = "text/html";
            return result;
        }


        public PartialViewResult ShowQuotes(string cluster, int pm_viewID)
        {
            ViewData["viewID"] = pm_viewID;

            List<BuyQuote> theQuotes = new List<BuyQuote>();
            switch (cluster)
            {
                case "OS":
                    theQuotes = GlobalVariables.MySession.List_Buy_OSSheet.FirstOrDefault(w => w.viewID == pm_viewID).theQuotes;
                    break;
                case "Domestic":
                    theQuotes = GlobalVariables.MySession.List_Buy_DomesticSheet.FirstOrDefault(w => w.viewID == pm_viewID).theQuotes;
                    break;
                case "FTB":
                    theQuotes = GlobalVariables.MySession.List_Buy_FTBSheet.FirstOrDefault(w => w.viewID == pm_viewID).theQuotes;
                    break;
                default:
                    break;
            }

            return PartialView("BuyQuotes", theQuotes);
        }


        public List<BuyNote> getNotesList(Buy pm_newBuy)
        {
            List<BuyNote> theNotes = new List<BuyNote>();
            theNotes = pgmBuyNote.GetNotesForOneBuy(pm_newBuy.viewID, pm_newBuy.entryID);
            return theNotes;
        }


        [HttpPost]
        [MultipleButton(Name = "CreateBuyAction", Argument = "PartInfo")]
        public ActionResult GetPartInfo(Buy pm_newBuy)
        {
            TempData["message"] = null;
            TempData["EnableRequired"] = true;

            Buy buyModel = pgmBuySheet.GetPartInfo(pm_newBuy);
            TempData["createdBuy"] = buyModel;

            if (string.IsNullOrWhiteSpace(buyModel.purchase_class) && string.IsNullOrWhiteSpace(buyModel.TruePrimarySupplier))
            {
                TempData["message"] = "No data found.";
                TempData["EnableRequired"] = false;
            }

            if (string.IsNullOrWhiteSpace(buyModel.supplier_standard_cost) || Decimal.Parse(buyModel.supplier_standard_cost) <= 0)
            {
                TempData["message"] = "Please set Cost on the primary supplier by buy to loc in P21 for this part.";
                TempData["EnableRequired"] = false;
            }
            else
            {
                TempData["message"] = "Please ensure purchasing cost corresponds to sourcing cost on POD and supplier standard cost built on P21.";
            }

            return RedirectToAction("CreateBuy", new { cluster = pm_newBuy.cluster });
        }


        public ActionResult BuySheet(string cluster)
        {
            ViewBag.Dev = pgmBuySheet.devMode;
            ViewBag.cluster = cluster;
            ViewBag.account_id = GlobalVariables.MySession.Account;

            switch (cluster)
            {
                case "OS":
                    ViewBag.PageInfo = shareCommon.GetPageInfo(19);
                    ViewBag.authorized = shareCommon.isAuthorized(19);
                    break;
                case "Domestic":
                    ViewBag.PageInfo = shareCommon.GetPageInfo(31);
                    ViewBag.authorized = shareCommon.isAuthorized(31);
                    break;
                case "FTB":
                    ViewBag.PageInfo = shareCommon.GetPageInfo(107);
                    ViewBag.authorized = shareCommon.isAuthorized(107);
                    break;
                default:
                    TempData["message"] = "Unavailable Cluster.";
                    break;
            }

            var url = Url.Action("BuySheet", "Procurement", new { cluster = cluster });   // Outputcache root
            HttpResponse.RemoveOutputCacheItem(url);    // Clean output cache by root

            switch (cluster)
            {
                case "OS":
                    GlobalVariables.MySession.List_Buy_OSSheet = null;
                    if (GlobalVariables.MySession.List_Buy_OSFilters != null)
                        ViewBag.Filters = GlobalVariables.MySession.List_Buy_OSFilters;
                    if (GlobalVariables.MySession.List_Buy_OSOrder != null)
                        ViewBag.Sorts = GlobalVariables.MySession.List_Buy_OSOrder;
                    break;
                case "Domestic":
                    GlobalVariables.MySession.List_Buy_DomesticSheet = null;
                    if (GlobalVariables.MySession.List_Buy_DomesticFilters != null)
                        ViewBag.Filters = GlobalVariables.MySession.List_Buy_DomesticFilters;
                    if (GlobalVariables.MySession.List_Buy_DomesticOrder != null)
                        ViewBag.Sorts = GlobalVariables.MySession.List_Buy_DomesticOrder;
                    break;
                case "FTB":
                    GlobalVariables.MySession.List_Buy_FTBSheet = null;
                    if (GlobalVariables.MySession.List_Buy_FTBFilters != null)
                        ViewBag.Filters = GlobalVariables.MySession.List_Buy_FTBFilters;
                    if (GlobalVariables.MySession.List_Buy_FTBOrder != null)
                        ViewBag.Sorts = GlobalVariables.MySession.List_Buy_FTBOrder;
                    break;
                default:
                    break;
            }

            List<Buy> theSheet = pgmBuySheet.GetBuySheet(cluster);
            return View("BuySheet", theSheet);
        }


        public ActionResult GetSupplierList(string item_id, string location_id) //It will be fired from Jquery ajax call
        {
            var jsonData = new List<Vendor>();
            if (!string.IsNullOrWhiteSpace(item_id) && location_id != "0")
                jsonData = pgmBuySheet.GetList_Supplier(item_id, location_id).ToList();
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetVendorList(string supplier_id, string company_id) //It will be fired from Jquery ajax call
        {
            var jsonData = new List<Vendor>();

            if (!string.IsNullOrWhiteSpace(supplier_id) && supplier_id != "0" && !string.IsNullOrWhiteSpace(company_id))
                jsonData = pgmBuySheet.GetList_Vendor(supplier_id, company_id);

            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }


        public ActionResult GetSupplierInfo(string location_id, string item_id, string supplier_id) //It will be fired from Jquery ajax call
        {
            var jsonData = new BuySheet_SupplierInfo();
            if (!string.IsNullOrWhiteSpace(location_id) && !string.IsNullOrWhiteSpace(supplier_id) && supplier_id != "0")
                jsonData = pgmBuySheet.Get_SupplierInfo(location_id, item_id, supplier_id);
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }


        #region Filter

        public PartialViewResult FilterBuy(string cluster, string pm_Source)
        {
            List<string> theParameters = new List<string>();
            theParameters.Add(cluster);
            theParameters.Add(pm_Source);

            ViewData["pmParameters"] = String.Join(",", theParameters.ToArray());
            ViewData["goController"] = "Procurement";
            ViewData["goAction"] = "actFilter";
            return PartialView("../Home/_Filters", filterBuy.getFilters(cluster, pm_Source));
        }

        /*
        [HttpPost]
        [MultipleButton(Name = "FilterAction", Argument = "Reset")]
        public ActionResult goReset(string pmParameters)
        {
            List<string> theParameters = pmParameters.Split(',').ToList();
            filterBuy.PostFilter(null, theParameters[0], theParameters[1]);
            return RedirectToAction(theParameters[1], new { cluster = theParameters[0] });
        }


        [HttpPost]
        [MultipleButton(Name = "FilterAction", Argument = "Filter")]
        public ActionResult goFilter(List<Filters> pmFilterList, string pmParameters)
        {
            List<string> theParameters = pmParameters.Split(',').ToList();
            filterBuy.PostFilter(pmFilterList, theParameters[0], theParameters[1]);
            return RedirectToAction(theParameters[1], new { cluster = theParameters[0] });
        }
        */

        public ActionResult actFilter(List<Filters> pmFilterList, string pmParameters, string filter_action)
        {
            List<string> theParameters = pmParameters.Split(',').ToList();

            if (filter_action == "Filter")
                filterBuy.PostFilter(pmFilterList, theParameters[0], theParameters[1]);
            else
                filterBuy.PostFilter(null, theParameters[0], theParameters[1]);

            return RedirectToAction(theParameters[1], new { cluster = theParameters[0] });
        }


        public PartialViewResult FilterToolingCharge(string pm_Source)
        {
            List<string> theParameters = new List<string>();
            theParameters.Add(pm_Source);
            ViewData["pmParameters"] = String.Join(",", theParameters.ToArray());

            ViewData["goController"] = "Procurement";
            ViewData["goAction"] = "actToolingChargeFilter";
            return PartialView("../Home/_Filters", pgmBuyOthers.getToolingChargeFilters());
        }


        public ActionResult actToolingChargeFilter(List<Filters> pmFilterList, string pmParameters, string filter_action)
        {
            List<string> theParameters = pmParameters.Split(',').ToList();

            if (filter_action == "Filter")
                pgmBuyOthers.PostToolingChargeFilter(pmFilterList);
            else
                pgmBuyOthers.PostToolingChargeFilter(null);

            return RedirectToAction(theParameters[0]);
        }

        #endregion

        #region Notes

        public PartialViewResult AppendNote()
        {
            string cluster = Request.Form["pm_newNote_cluster"];
            List<BuyNote> theNotes = new List<BuyNote>();
            BuyNote noteModel = new BuyNote();
            noteModel.entryID = Request.Form["pm_newNote_entryID"];
            noteModel.notes = Request.Form["pm_newNote_Text"];
            noteModel.viewID = Int32.Parse(Request.Form["pm_newNote_viewID"]);
            TempData["message"] = null;

            ViewData["entryID"] = noteModel.entryID;
            ViewData["viewID"] = noteModel.viewID;
            ViewBag.cluster = cluster;
            ViewBag.Source = "BuySheet";
            switch (cluster)
            {
                case "OS":
                    ViewBag.authorized = shareCommon.isAuthorized(19);
                    break;
                case "Domestic":
                    ViewBag.authorized = shareCommon.isAuthorized(31);
                    break;
                case "FTB":
                    ViewBag.authorized = shareCommon.isAuthorized(107);
                    break;
                default:
                    TempData["message"] = "Unavailable Cluster.";
                    break;
            }

            if (string.IsNullOrWhiteSpace(noteModel.entryID) || string.IsNullOrWhiteSpace(noteModel.notes) || string.IsNullOrWhiteSpace(noteModel.viewID.ToString()))
            {
                TempData["msgType"] = "Info";
                TempData["message"] = "Please fill up the new note column.";
            }
            else
            {
                SP_Return theResult = pgmBuyNote.CreateNewNote(cluster, noteModel, "BuySheet");
                if (theResult.r != 1)
                {
                    TempData["msgType"] = "Error";
                    TempData["message"] = theResult.msg;
                }
                else
                {
                    TempData["msgType"] = "Success";
                    TempData["message"] = "New note appened.";
                }
            }

            switch (cluster)
            {
                case "OS":
                    theNotes = GlobalVariables.MySession.List_Buy_OSNotes.FindAll(x => x.viewID.Equals(noteModel.viewID));
                    break;
                case "Domestic":
                    theNotes = GlobalVariables.MySession.List_Buy_DomesticNotes.FindAll(x => x.viewID.Equals(noteModel.viewID));
                    break;
                case "FTB":
                    theNotes = GlobalVariables.MySession.List_Buy_FTBNotes.FindAll(x => x.viewID.Equals(noteModel.viewID));
                    break;
                default:
                    break;
            }

            return PartialView("BuyNotes", theNotes);
        }


        public PartialViewResult DeleteNote(string cluster, string pm_entryID, int pm_viewID, int pm_notesID)
        {
            BuyNote noteModel = new BuyNote();
            noteModel.entryID = pm_entryID;
            noteModel.viewID = pm_viewID;
            noteModel.notesID = pm_notesID;
            ViewData["viewID"] = noteModel.viewID;
            ViewData["entryID"] = pm_entryID;
            ViewBag.cluster = cluster;
            ViewBag.authorized = cluster == "Domestic" ? shareCommon.isAuthorized(31) : shareCommon.isAuthorized(19);
            ViewBag.Source = "BuySheet";
            List<BuyNote> theNotes = new List<BuyNote>();
            TempData["message"] = null;

            SP_Return theResult = pgmBuyNote.DeleteOneNote(cluster, noteModel);
            if (theResult.r != 1)
            {
                TempData["msgType"] = "Error";
                TempData["message"] = theResult.msg;
            }
            else
            {
                TempData["msgType"] = "Success";
                TempData["message"] = "Note deleted.";
            }

            switch (cluster)
            {
                case "OS":
                    theNotes = GlobalVariables.MySession.List_Buy_OSNotes.FindAll(x => x.viewID.Equals(noteModel.viewID));
                    break;
                case "Domestic":
                    theNotes = GlobalVariables.MySession.List_Buy_DomesticNotes.FindAll(x => x.viewID.Equals(noteModel.viewID));
                    break;
                case "FTB":
                    theNotes = GlobalVariables.MySession.List_Buy_FTBNotes.FindAll(x => x.viewID.Equals(noteModel.viewID));
                    break;
                default:
                    break;
            }

            return PartialView("BuyNotes", theNotes);
        }


        public PartialViewResult EditNote()
        {
            string cluster = Request.Form["pm_Note_cluster"];
            ViewBag.cluster = cluster;
            ViewBag.Source = "BuySheet";
            switch (cluster)
            {
                case "OS":
                    ViewBag.authorized = shareCommon.isAuthorized(19);
                    break;
                case "Domestic":
                    ViewBag.authorized = shareCommon.isAuthorized(31);
                    break;
                case "FTB":
                    ViewBag.authorized = shareCommon.isAuthorized(107);
                    break;
                default:
                    TempData["message"] = "Unavailable Cluster.";
                    break;
            }

            BuyNote noteModel = new BuyNote();
            noteModel.entryID = Request.Form["pm_Note_entryID"];
            noteModel.viewID = Int16.Parse(Request.Form["pm_Note_viewID"]);
            noteModel.notesID = Int16.Parse(Request.Form["pm_Note_notesID"]);
            noteModel.notes = Request.Form["pm_Upd_Notes"];
            noteModel.user = (Request.Form["pm_Upd_user"]).ToLower();
            ViewData["viewID"] = noteModel.viewID;
            List<BuyNote> theNotes = new List<BuyNote>();
            TempData["message"] = null;

            if (string.IsNullOrWhiteSpace(noteModel.entryID) || string.IsNullOrWhiteSpace(noteModel.notes) || string.IsNullOrWhiteSpace(noteModel.viewID.ToString()))
            {
                TempData["msgType"] = "Info";
                TempData["message"] = "Please fill up the note column.";
            }
            else if (noteModel.user != GlobalVariables.MySession.FirstName.ToLower())
            {
                TempData["msgType"] = "Warning";
                TempData["message"] = "You don't have authority to modify the others' note.";
            }
            else
            {
                SP_Return theResult = pgmBuyNote.UpdateOneNote(cluster, noteModel);
                if (theResult.r != 1)
                {
                    TempData["msgType"] = "Error";
                    TempData["message"] = theResult.msg;
                }
                else
                {
                    TempData["msgType"] = "Success";
                    TempData["message"] = "Note updated.";
                }
            }

            switch (cluster)
            {
                case "OS":
                    theNotes = GlobalVariables.MySession.List_Buy_OSNotes.FindAll(x => x.viewID.Equals(noteModel.viewID));
                    break;
                case "Domestic":
                    theNotes = GlobalVariables.MySession.List_Buy_DomesticNotes.FindAll(x => x.viewID.Equals(noteModel.viewID));
                    break;
                case "FTB":
                    theNotes = GlobalVariables.MySession.List_Buy_FTBNotes.FindAll(x => x.viewID.Equals(noteModel.viewID));
                    break;
                default:
                    break;
            }

            return PartialView("BuyNotes", theNotes);
        }


        public PartialViewResult ShowNotes(string cluster, string pm_Source, string pm_entryID, int pm_viewID)
        {
            ViewData["viewID"] = pm_viewID;
            ViewData["entryID"] = pm_entryID;
            ViewBag.cluster = cluster;
            ViewBag.Source = pm_Source;
            ViewBag.newCount = 0;
            switch (cluster)
            {
                case "OS":
                    ViewBag.authorized = shareCommon.isAuthorized(19);
                    break;
                case "Domestic":
                    ViewBag.authorized = shareCommon.isAuthorized(31);
                    break;
                case "FTB":
                    ViewBag.authorized = shareCommon.isAuthorized(107);
                    break;
                default:
                    TempData["message"] = "Unavailable Cluster.";
                    break;
            }

            List<Buy> BuyList = new List<Buy>();
            List<BuyNote> NotesList = new List<BuyNote>();
            TempData["message"] = null;

            switch (pm_Source)
            {
                case "BuySheet":
                case "EditBuy":
                    switch (cluster)
                    {
                        case "OS":
                            NotesList = GlobalVariables.MySession.List_Buy_OSNotes;
                            break;
                        case "Domestic":
                            NotesList = GlobalVariables.MySession.List_Buy_DomesticNotes;
                            break;
                        case "FTB":
                            NotesList = GlobalVariables.MySession.List_Buy_FTBNotes;
                            break;
                        default:
                            break;
                    }
                    break;
                case "BuyArchive":
                    switch (cluster)
                    {
                        case "OS":
                            NotesList = GlobalVariables.MySession.List_ArchiveBuy_OSNotes;
                            break;
                        case "Domestic":
                            NotesList = GlobalVariables.MySession.List_ArchiveBuy_DomesticNotes;
                            break;
                        case "FTB":
                            NotesList = GlobalVariables.MySession.List_ArchiveBuy_FTBNotes;
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    NotesList = pgmBuyNote.GetNotesForOneBuy(pm_viewID, pm_entryID);
                    break;
            }

            List<BuyNote> theNotes = NotesList.FindAll(x => x.viewID.Equals(pm_viewID));

            if (pm_Source == "BuySheet")
            {
                switch (cluster)
                {
                    case "OS":
                        BuyList = GlobalVariables.MySession.List_Buy_OSSheet;
                        break;
                    case "Domestic":
                        BuyList = GlobalVariables.MySession.List_Buy_DomesticSheet;
                        break;
                    case "FTB":
                        BuyList = GlobalVariables.MySession.List_Buy_FTBSheet;
                        break;
                    default:
                        break;
                }

                if (BuyList.Where(b => b.viewID == pm_viewID).FirstOrDefault().planner.ToLower() == GlobalVariables.MySession.Account)
                    ViewBag.newCount = theNotes.FindAll(x => x.read != "y").Count();
            }

            return PartialView("BuyNotes", theNotes);
        }


        public PartialViewResult AsReadNote(string cluster, string pm_entryID, int pm_viewID)
        {
            ViewData["viewID"] = pm_viewID;
            ViewData["entryID"] = pm_entryID;
            ViewBag.cluster = cluster;
            ViewBag.Source = "BuySheet";
            ViewBag.newCount = 0;
            switch (cluster)
            {
                case "OS":
                    ViewBag.authorized = shareCommon.isAuthorized(19);
                    break;
                case "Domestic":
                    ViewBag.authorized = shareCommon.isAuthorized(31);
                    break;
                case "FTB":
                    ViewBag.authorized = shareCommon.isAuthorized(107);
                    break;
                default:
                    TempData["message"] = "Unavailable Cluster.";
                    break;
            }
            List<BuyNote> theNotes = new List<BuyNote>();
            TempData["message"] = null;

            switch (cluster)
            {
                case "OS":
                    theNotes = GlobalVariables.MySession.List_Buy_OSNotes.FindAll(x => x.viewID.Equals(pm_viewID));
                    break;
                case "Domestic":
                    theNotes = GlobalVariables.MySession.List_Buy_DomesticNotes.FindAll(x => x.viewID.Equals(pm_viewID));
                    break;
                case "FTB":
                    theNotes = GlobalVariables.MySession.List_Buy_FTBNotes.FindAll(x => x.viewID.Equals(pm_viewID));
                    break;
                default:
                    break;
            }

            SP_Return theResult = pgmBuyNote.MarkAsReadNotesForOneBuy(cluster, pm_entryID, pm_viewID);
            if (theResult.r != 1)
            {
                TempData["msgType"] = "Warning";
                TempData["message"] = theResult.msg;
            }

            return PartialView("BuyNotes", theNotes);
        }

        #endregion

        #region Splits

        public ActionResult ShowSplits(string cluster, string pm_Source, string pm_entryID, int pm_viewID)
        {
            ViewData["entryID"] = pm_entryID;
            ViewData["viewID"] = pm_viewID;
            ViewBag.cluster = cluster;
            ViewBag.Source = pm_Source;
            List<BuySplit> SplitsList = new List<BuySplit>();
            Buy thisBuy = new Buy();

            switch (pm_Source)
            {
                case "BuySheet":
                    switch (cluster)
                    {
                        case "OS":
                            SplitsList = GlobalVariables.MySession.List_Buy_OSSplits;
                            thisBuy = GlobalVariables.MySession.List_Buy_OSSheet.Find(f => f.viewID == pm_viewID);
                            ViewData["isEditable"] = (string.IsNullOrWhiteSpace(thisBuy.PO_no) ? true : false);
                            break;
                        case "Domestic":
                            SplitsList = GlobalVariables.MySession.List_Buy_DomesticSplits;
                            thisBuy = GlobalVariables.MySession.List_Buy_DomesticSheet.Find(f => f.viewID == pm_viewID);
                            ViewData["isEditable"] = (string.IsNullOrWhiteSpace(thisBuy.PO_no) ? true : false);
                            break;
                        case "FTB":
                            SplitsList = GlobalVariables.MySession.List_Buy_FTBSplits;
                            thisBuy = GlobalVariables.MySession.List_Buy_FTBSheet.Find(f => f.viewID == pm_viewID);
                            ViewData["isEditable"] = (string.IsNullOrWhiteSpace(thisBuy.PO_no) && (thisBuy.creator == GlobalVariables.MySession.Account) ? true : false);
                            break;
                        default:
                            break;
                    }
                    break;
                case "BuyArchive":
                    switch (cluster)
                    {
                        case "OS":
                            SplitsList = GlobalVariables.MySession.List_ArchiveBuy_OSSplits;
                            break;
                        case "Domestic":
                            SplitsList = GlobalVariables.MySession.List_ArchiveBuy_DomesticSplits;
                            break;
                        case "FTB":
                            SplitsList = GlobalVariables.MySession.List_ArchiveBuy_FTBSplits;
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }

            if (pm_Source == "ArchiveItem")
                //return PartialView("BuySplits", pgmBuySplits.GetSplitsForOneBuy(pm_entryID, pm_viewID));
                return PartialView("BuySplits", pgmBuySplits.FetchAllSplits(pm_viewID.ToString()));

            List<BuySplit> theSplits = new List<BuySplit>();
            if (SplitsList != null && !string.IsNullOrEmpty(pm_Source))
                theSplits = SplitsList.FindAll(x => x.viewID.Equals(pm_viewID));

            return PartialView("BuySplits", theSplits);
        }


        public ActionResult EditSplits(string cluster, string pm_split_entryID = "", int pm_split_viewID = 0)
        {
            ViewBag.Dev = pgmBuySheet.devMode;
            ViewBag.cluster = cluster;
            switch (cluster)
            {
                case "OS":
                    ViewBag.authorized = shareCommon.isAuthorized(19);
                    break;
                case "Domestic":
                    ViewBag.authorized = shareCommon.isAuthorized(31);
                    break;
                case "FTB":
                    ViewBag.authorized = shareCommon.isAuthorized(107);
                    break;
                default:
                    TempData["message"] = "Unavailable Cluster.";
                    break;
            }
            int maxiumCnt = 10;
            int? new_quantity = 0;
            int new_po_quantity = 0;

            Buy wkBuy = new Buy(); ;
            ViewData["price_usd"] = 0;
            switch (cluster)
            {
                case "OS":
                    wkBuy = GlobalVariables.MySession.List_Buy_OSSheet.FirstOrDefault(x => x.viewID == pm_split_viewID);
                    break;
                case "Domestic":
                    wkBuy = GlobalVariables.MySession.List_Buy_DomesticSheet.FirstOrDefault(x => x.viewID == pm_split_viewID);
                    break;
                case "FTB":
                    wkBuy = GlobalVariables.MySession.List_Buy_FTBSheet.FirstOrDefault(x => x.viewID == pm_split_viewID);
                    break;
                default:
                    wkBuy = null;
                    break;
            }
            if (wkBuy.theQuotes != null && wkBuy.theQuotes.FindAll(f => f.z_status == "A").Count() == 1)
                ViewData["price_usd"] = wkBuy.theQuotes.Find(f => f.z_status == "A").price_usd;

            List<BuySplit> wkList = new List<BuySplit>();
            if (TempData["splitsList"] != null)
                wkList = TempData["splitsList"] as List<BuySplit>;
            else
            {
                switch (cluster)
                {
                    case "OS":
                        wkList = GlobalVariables.MySession.List_Buy_OSSplits.FindAll(x => x.viewID == pm_split_viewID);
                        break;
                    case "Domestic":
                        wkList = GlobalVariables.MySession.List_Buy_DomesticSplits.FindAll(x => x.viewID == pm_split_viewID);
                        break;
                    case "FTB":
                        wkList = GlobalVariables.MySession.List_Buy_FTBSplits.FindAll(x => x.viewID == pm_split_viewID);
                        break;
                    default:
                        break;
                }
            }

            if (wkBuy == null)
            {
                TempData["message"] = "Empty model of Splits.";
                return RedirectToAction("BuySheet", new { cluster = cluster });
            }
            else
            {
                ViewData["entryID"] = wkBuy.entryID;
                ViewData["viewID"] = wkBuy.viewID;

                if (string.IsNullOrWhiteSpace(wkBuy.preload_loc) || int.Parse(wkBuy.preload_loc) <= 0)
                    ViewData["Preload"] = false;
                else
                {
                    ViewData["Preload"] = true;
                    for (int i = 0; i < wkList.Count(); i++)
                    {
                        List<BuySheet_Preload> PreloadList = pgmBuyOthers.GetList_PreloadCalendar(wkBuy.po_to_loc.ToString(), wkBuy.preload_loc, (DateTime)wkList[i].release_date);
                        if (PreloadList == null || PreloadList.Count() <= 0)
                            TempData["message"] = "None of preload belongs to this location id and release date.";
                        else
                        {
                            PreloadList.Insert(0, new BuySheet_Preload() { order_no = null, preload_po_so = "" });

                            if (string.IsNullOrWhiteSpace(wkList[i].preload_so))
                                ViewData["PreloadList_" + i.ToString()] = new SelectList(PreloadList, "order_no", "preload_po_so", PreloadList[0].order_no);
                            else
                                ViewData["PreloadList_" + i.ToString()] = new SelectList(PreloadList, "order_no", "preload_po_so", wkList[i].preload_so);
                        }
                    }
                }

                new_quantity = wkBuy.new_quantity;
                new_po_quantity = Convert.ToInt32(wkBuy.PO_order_quantity_1);
                foreach (var item in wkList)
                {
                    item.sumQuant = new_quantity;
                    item.po_sumQuant = new_po_quantity;
                    if ((item.release_date.ToString() == "") || (((DateTime)item.release_date).ToString("yyyy-MM-dd") == "1900-01-01"))
                        item.EDT_release_date = "";
                    else
                        item.EDT_release_date = ((DateTime)item.release_date).ToString("yyyy-MM-dd");
                    if ((item.expected_date.ToString() == "") || (((DateTime)item.expected_date).ToString("yyyy-MM-dd") == "1900-01-01"))
                        item.EDT_expected_date = "";
                    else
                        item.EDT_expected_date = ((DateTime)item.expected_date).ToString("yyyy-MM-dd");
                }

                while (wkList.Count < maxiumCnt)
                {
                    wkList.Add(
                        new BuySplit()
                        {
                            selected = false,
                            viewID = pm_split_viewID,
                            splitID = "",
                            ship_method = "Ocean",
                            entryID = pm_split_entryID,
                            release_date = DateTime.Today,
                            EDT_release_date = DateTime.Today.ToString("d"),
                            expected_date = DateTime.Today,
                            EDT_expected_date = DateTime.Today.ToString("d"),
                            //quantity = 0,
                            //po_quantity = 0,
                            sumQuant = new_quantity,
                            po_sumQuant = new_po_quantity
                        }
                    );
                }

                List<SelectListItem> ShipMethod = new List<SelectListItem>();
                ShipMethod.Add(new SelectListItem { Text = "Ocean", Value = "Ocean" });
                ShipMethod.Add(new SelectListItem { Text = "Air", Value = "Air" });
                ViewData["ShipMethod"] = ShipMethod;

                return View(wkList);
            }
        }


        public ActionResult goUpdateSplits(string cluster, List<BuySplit> pm_Splits)
        {
            string pm_entryID = "";
            int pm_viewID = 0;
            string msg = "";
            TempData["message"] = null;

            if (pm_Splits.Count() > 0)
            {
                pm_entryID = pm_Splits[0].entryID;
                pm_viewID = pm_Splits[0].viewID;
            }

            ViewData["entryID"] = pm_entryID;
            ViewData["viewID"] = pm_viewID;
            SP_Return theResult = pgmBuySplits.ManageSplits(cluster, pm_Splits);
            if (theResult.r == 1)
            {
                msg = "Splits of Buy #" + pm_viewID + " are updated.";

                switch (cluster)
                {
                    case "OS":
                        GlobalVariables.MySession.List_Buy_OSSheet = null;
                        break;
                    case "Domestic":
                        GlobalVariables.MySession.List_Buy_DomesticSheet = null;
                        break;
                    case "FTB":
                        GlobalVariables.MySession.List_Buy_FTBSheet = null;
                        if (GlobalVariables.MySession.List_Buy_FTBSplits.FindAll(f => f.entryID == pm_entryID && f.ship_method == "Air").Count() > 0)
                            msg += "\r\nPlease well describe Air info in this buy note.";
                        break;
                    default:
                        break;
                }

                TempData["message"] = msg;
                return RedirectToAction("BuySheet", new { cluster = cluster });
            }
            else
            {
                TempData["message"] = theResult.msg;
                TempData["splitsList"] = pm_Splits.ToList();
                return RedirectToAction("EditSplits", new { cluster = cluster, pm_split_entryID = pm_entryID, pm_split_viewID = pm_viewID });
            }
        }

        #endregion

        #region Edit

        [HttpPost]
        [MultipleButton(Name = "CreateBuyAction", Argument = "Create")]
        [ValidateAntiForgeryToken]
        public ActionResult CreateOne(Buy pm_newBuy)
        {
            TempData["message"] = null;
            List<Location> LocationList = shareGet.GetList_Location();
            ViewData["LocationList"] = new SelectList(LocationList, "location_id", "id_name");
            if (pm_newBuy is null)
            {
                TempData["message"] = "Null buy";
                return View("CreateBuy", pm_newBuy);
            }

            if (pm_newBuy.theQuotes.Exists(e => e.z_status == "A" && e.vendor_id == 0))
            {
                TempData["message"] = "Invalid vendor for Approved quote.";
                TempData["createdBuy"] = pm_newBuy;
                return RedirectToAction("CreateBuy", new { cluster = pm_newBuy.cluster });
            }

            if (pm_newBuy.cluster == "FTB")
                pm_newBuy.grand_total_value = pm_newBuy.grand_total_value.Replace(",", "").Replace(" ", "");

            if (string.IsNullOrWhiteSpace(pm_newBuy.total_value) || pm_newBuy.total_value == "NaN")
                pm_newBuy.total_value = "0";
            if (string.IsNullOrWhiteSpace(pm_newBuy.grand_total_value) || pm_newBuy.grand_total_value == "NaN")
                pm_newBuy.grand_total_value = "0";

            SP_Return theResult = pgmBuySheet.CreateOneBuy2(pm_newBuy);
            if (theResult.r == 1)
            {
                switch (pm_newBuy.cluster)
                {
                    case "OS":
                        GlobalVariables.MySession.List_Buy_OSSheet = null;
                        if (Decimal.Parse(pm_newBuy.total_value) > 5000)
                            theResult.msg += System.Environment.NewLine + "Total value of this buy is greater than $5,000. Please consider split the shipment.";
                        break;
                    case "Domestic":
                        GlobalVariables.MySession.List_Buy_DomesticSheet = null;
                        if (Decimal.Parse(pm_newBuy.total_value) > 5000)
                            theResult.msg += System.Environment.NewLine + "Total value of this buy is greater than $5,000. Please consider split the shipment.";
                        break;
                    case "FTB":
                        GlobalVariables.MySession.List_Buy_FTBSheet = null;
                        if (Decimal.Parse(pm_newBuy.grand_total_value) > 5000)
                        //if (pm_newBuy.grand_total_value > 5000)
                            theResult.msg += System.Environment.NewLine + "Total value of this buy is greater than $5,000. Please consider split the shipment.";
                        break;
                    default:
                        theResult.msg += System.Environment.NewLine + "Can't find any sheet.";
                        break;
                }

                TempData["message"] = theResult.msg;
                return RedirectToAction("BuySheet", new { cluster = pm_newBuy.cluster });
            }
            else
            {
                //TempData["message"] = "Failed to create a new buy.\r\n" + theResult.msg;
                TempData["message"] = theResult.msg;
                TempData["createdBuy"] = pm_newBuy;
                return RedirectToAction("CreateBuy", new { cluster = pm_newBuy.cluster });
            }
        }


        public FileStreamResult ExampleSheet()
        {
            MemoryStream example = pgmBuySheet.GetExampleSheet();
            return File(example, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "example.xlsx");
        }


        [DeleteFileAttribute]
        //public async Task<ActionResult> DownloadDocument(string pm_folder, string pm_file_name)
        public async Task<ActionResult> DownloadDocument()
        {
            var encFolder = Request.Unvalidated.QueryString["pm_folder"];
            var encFile = Request.Unvalidated.QueryString["pm_file_name"];
            var pm_folder = HttpUtility.UrlDecode(encFolder ?? "");
            var pm_file_name = HttpUtility.UrlDecode(encFile ?? "");
            //var sanitizer = new HtmlSanitizer();
            //pm_folder = sanitizer.Sanitize(pm_folder);
            //pm_file_name = sanitizer.Sanitize(pm_file_name);

            string theParms = "P21URL=" + (!pgmBuySheet.isDev ? "1" : "2") + "&pmFolderPath=" + pm_folder + "&pmFileName=" + pm_file_name;
            string thisPath = shareFileActions.GetUploadFolder() + pm_file_name;
            //string fullPath = thisPath.Replace(GlobalConfig.MyConfig.ProjectName, "SBS_API");
            string fullPath = thisPath.Replace(scriptsHome.getProjectName(), "SBS_API").Replace("\\Home", "");
            HttpResponseMessage response = await shareCommon.callSBS_API_Get("Epicor/GetDocument", theParms);

            if (!response.IsSuccessStatusCode)
                return Content("StatusCode = " + ((int)response.StatusCode).ToString() + ", " + response.ReasonPhrase);
            else
            {
                if (!System.IO.File.Exists(fullPath))
                    return Content("File does not exist.");
                else
                {
                    FileInfo fi = new FileInfo(fullPath);
                    fi.CopyTo(thisPath, true); // existing file will be overwritten
                    shareFileActions.deleteWhenUnlock(fi);
                    return File(thisPath, System.Net.Mime.MediaTypeNames.Application.Octet, pm_file_name);
                }
            }
        }


        [HttpPost]
        [MultipleButton(Name = "BuySheetAction", Argument = "Export")]
        public ActionResult SheetToExcel(string cluster)
        {
            string thePath = Path.Combine(Server.MapPath("~/UploadedFiles"), GlobalVariables.MySession.Account + ".xlsx");
            string theFileName = cluster + "BuySheet_" + DateTime.Now.ToString("MMddyyyy", DateTimeFormatInfo.InvariantInfo) + ".xlsx";
            SP_Return ExportModel = pgmBuySheet.SheetToExcel(cluster, thePath, pgmBuySheet.ExportSheet(cluster, Request.Form["collection_viewID"]));

            TempData["message"] = null;
            if (ExportModel.r == 0)
                TempData["message"] = ExportModel.msg;
            else
            {
                System.IO.FileInfo wkFile = new System.IO.FileInfo(thePath);
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                using (ExcelPackage pck = new ExcelPackage(wkFile))
                {
                    //Write it back to the client
                    Response.ContentType = "application/vnd.ms-excel";
                    Response.AddHeader("content-disposition", "attachment;  filename=" + theFileName);
                    Response.BinaryWrite(pck.GetAsByteArray());
                    Response.Flush();
                    Response.End();
                };
                wkFile.Delete();
            }

            return RedirectToAction("BuySheet", new { cluster = cluster });
        }


        public ActionResult UploadFile(HttpPostedFileBase pm_file, string cluster)
        {
            TempData["message"] = null;
            ViewData["LocationList"] = shareGet.GetList_Location();
            SP_Return theResult = new SP_Return();
            string thePath = "";

            if (!Directory.Exists(shareFileActions.GetUploadFolder()))
            {
                TempData["message"] = "Can't find target folder for uploading. Please contact IT team to fix the issue.";
                return RedirectToAction("CreateBuy", new { cluster = cluster });
            }

            if (pm_file.ContentLength > 0)
            {
                thePath = Path.Combine(shareFileActions.GetUploadFolder(), Path.GetFileName(pm_file.FileName));
                pm_file.SaveAs(thePath);
                theResult = pgmBuySheet.ImportData(thePath, cluster);
                TempData["message"] = theResult.msg;
                if (theResult.r != 1)
                    return RedirectToAction("CreateBuy", new { cluster = cluster });

                return RedirectToAction("BuySheet", new { cluster = cluster });
            }
            else
            {
                TempData["message"] = "No content found in the file.";
                return RedirectToAction("CreateBuy", new { cluster = cluster });
            }
        }


        public ActionResult CreateBuy(string cluster)
        {
            ViewBag.Dev = pgmBuySheet.devMode;
            ViewBag.cluster = cluster;
            switch (cluster)
            {
                case "OS":
                    ViewBag.PageInfo = shareCommon.GetPageInfo(18);
                    ViewBag.authorized = shareCommon.isAuthorized(18);
                    break;
                case "Domestic":
                    ViewBag.PageInfo = shareCommon.GetPageInfo(30);
                    ViewBag.authorized = shareCommon.isAuthorized(30);
                    break;
                case "FTB":
                    ViewBag.PageInfo = shareCommon.GetPageInfo(106);
                    ViewBag.authorized = shareCommon.isAuthorized(106);
                    break;
                default:
                    TempData["message"] = "Unavailable Cluster.";
                    break;
            }

            bool EnableRequired = false;
            Buy newBuy = new Buy();
            if (TempData["createdBuy"] == null)
                newBuy.cluster = cluster;
            else
            {
                newBuy = (Buy)TempData["createdBuy"];
                EnableRequired = true;
            }
            ViewData["EnableRequired"] = EnableRequired;

            if (newBuy.theQuotes == null)
            {
                newBuy.theQuotes = new List<BuyQuote>();
                newBuy.theQuotes.Add(new BuyQuote() { quote_seq = 0, lead = "0" });
            }

            if (cluster == "FTB")
            {
                if (newBuy.theFTB == null)
                {
                    newBuy.theFTB = new BuyFTB();
                    newBuy.theFTB.theDocuments = new List<BuyFTB_document>();
                }

                var selection = new List<SelectListItem>();
                selection.Add(new SelectListItem { Value = "", Text = "" });
                selection.AddRange(shareConvertor.ToDropDownOptions(shareGet.FetchCodeList("FTB_Sourcing", 2)));
                ViewData["SoucingList"] = selection;
                selection = null;
                selection = new List<SelectListItem>();
                selection.Add(new SelectListItem { Value = "", Text = "" });
                selection.AddRange(shareConvertor.ToDropDownOptions(shareGet.FetchCodeList("FTB_Buyer", 2).Where(w => w.Value != "QC").ToList()));
                ViewData["BuyerList"] = selection;

                List<SelectListItem> ShipMethodList = new List<SelectListItem>();
                ShipMethodList.Add(new SelectListItem { Text = "Ocean", Value = "Ocean" });
                ShipMethodList.Add(new SelectListItem { Text = "Air", Value = "Air" });
                ViewData["List_ShipMethod"] = ShipMethodList;

                List<SelectListItem> PPAPList = new List<SelectListItem>();
                PPAPList.Add(new SelectListItem { Text = "", Value = "" });
                PPAPList.Add(new SelectListItem { Text = "PPAP1", Value = "PPAP1" });
                PPAPList.Add(new SelectListItem { Text = "PPAP2", Value = "PPAP2" });
                PPAPList.Add(new SelectListItem { Text = "PPAP3", Value = "PPAP3" });
                PPAPList.Add(new SelectListItem { Text = "PPAP4", Value = "PPAP4" });
                ViewData["List_PPAP"] = PPAPList;

                List<SelectListItem> SampleList = new List<SelectListItem>();
                SampleList.Add(new SelectListItem { Text = "", Value = "" });
                SampleList.Add(new SelectListItem { Text = "Yes", Value = "True" });
                SampleList.Add(new SelectListItem { Text = "No", Value = "False" });
                ViewData["List_Sample"] = SampleList;
            }

            List<Location> LocationList = shareGet.GetList_Location();
            ViewData["LocationList"] = new SelectList(LocationList, "location_id", "id_name");
            ViewData["List_preload_loc"] = new SelectList(LocationList, "location_id", "id_name");

            if (string.IsNullOrWhiteSpace(newBuy.ItemID))
                ViewData["List_po_to_loc"] = new SelectList(LocationList, "location_id", "id_name");
            else
                ViewData["List_po_to_loc"] = new SelectList(shareGet.FetchList_inv_loc(newBuy.ItemID), "location_id", "id_name");

            if (!string.IsNullOrWhiteSpace(newBuy.preload_loc) && int.Parse(newBuy.preload_loc) > 0)
            {
                List<BuySheet_Preload> PreloadList = pgmBuyOthers.GetList_PreloadCalendar(newBuy.po_to_loc.ToString(), newBuy.preload_loc, (DateTime)newBuy.required_date);
                if (PreloadList == null || PreloadList.Count() <= 0)
                    TempData["message"] = "None of preload belongs to this location id and required date.";
                else
                {
                    PreloadList.Insert(0, new BuySheet_Preload());
                    ViewData["PreloadList_SO"] = new SelectList(PreloadList, "order_no", "preload_po_so", (string.IsNullOrWhiteSpace(newBuy.preload_so) ? PreloadList[1].order_no.ToString() : newBuy.preload_so));
                }
            }

            if (!string.IsNullOrWhiteSpace(newBuy.ItemID) && Convert.ToInt16(newBuy.po_to_loc) > 0)
            {
                List<Vendor> SupplierList = pgmBuySheet.GetList_Supplier(newBuy.ItemID, newBuy.po_to_loc.ToString());
                if (SupplierList == null)
                    TempData["message"] = "No supplier belongs to this part number and location id.";
                else
                {
                    SupplierList.Insert(0, new Vendor { vendor_id = 0, vendor_name = "" });
                    ViewData["SupplierList"] = new SelectList(SupplierList, "vendor_id", "vendor_name");
                }
            }

            List<SelectListItem> StatusList = new List<SelectListItem>();
            StatusList.Add(new SelectListItem { Text = "Candidate", Value = "" });
            StatusList.Add(new SelectListItem { Text = "Approve", Value = "A" });
            StatusList.Add(new SelectListItem { Text = "Text", Value = "T" });
            ViewData["StatusList"] = StatusList;

            ViewData["CurrencyList"] = shareConvertor.ToDropDownOptions(shareGet.FetchCodeList("Currency"));
            ViewData["LatestExchangeRate"] = shareGet.Fetch_LatestExchangeRate();

            return View(newBuy);
        }


        public ActionResult DeleteBuy(string cluster, string pm_entryId, string pm_viewID)
        {
            TempData["message"] = null;
            SP_Return theResult = pgmBuySheet.DeleteOneBuy(cluster, pm_entryId);

            if (theResult.r == 1)
            {
                TempData["message"] = "Buy #" + pm_viewID.ToString() + " deleted.";

                switch (cluster)
                {
                    case "OS":
                        GlobalVariables.MySession.List_Buy_OSSheet = null;
                        break;
                    case "Domestic":
                        GlobalVariables.MySession.List_Buy_DomesticSheet = null;
                        break;
                    case "FTB":
                        GlobalVariables.MySession.List_Buy_FTBSheet = null;
                        break;
                    default:
                        break;
                }

                return RedirectToAction("BuySheet", new { cluster = cluster });
            }
            else
            {
                TempData["message"] = theResult.msg;
                return RedirectToAction("BuySheet", new { cluster = cluster });
            }
        }


        public ActionResult EditBuy(string cluster, string pm_entryId = "")
        {
            TempData["message"] = "";
            ViewBag.Dev = pgmBuySheet.devMode;
            ViewBag.cluster = cluster;
            ViewBag.Account = GlobalVariables.MySession.Account;
            //ViewBag.FirstName = GlobalVariables.MySession.FirstName;
            switch (cluster)
            {
                case "OS":
                    ViewBag.PageInfo = shareCommon.GetPageInfo(19);
                    ViewBag.authorized = shareCommon.isAuthorized(19);
                    break;
                case "Domestic":
                    ViewBag.PageInfo = shareCommon.GetPageInfo(31);
                    ViewBag.authorized = shareCommon.isAuthorized(31);
                    break;
                case "FTB":
                    ViewBag.PageInfo = shareCommon.GetPageInfo(107);
                    ViewBag.authorized = shareCommon.isAuthorized(107);
                    break;
                default:
                    TempData["message"] = "Unavailable Cluster.";
                    break;
            }

            Buy oneBuy = new Buy();

            if (TempData["pm_updatedBuy"] != null)
            {
                TempData["message"] += "\r\n" + TempData["pm_message"];
                oneBuy = (Buy)TempData["pm_updatedBuy"];
            }
            else if (!string.IsNullOrWhiteSpace(pm_entryId))
                oneBuy = pgmBuySheet.getOneBuy(cluster, pm_entryId);
            else
            {
                TempData["message"] += "\r\n" + "Empty model of Buy.";
                return RedirectToAction("BuySheet", new { cluster = cluster });
            }

            if (oneBuy.theQuotes == null)
                oneBuy.theQuotes = new List<BuyQuote>();

            if (oneBuy.theQuotes.Count() == 0)
                oneBuy.theQuotes.Add(new BuyQuote() { quote_seq = 0, viewID = oneBuy.viewID, lead = "0" });
            else
                oneBuy.theQuotes.Insert(0, new BuyQuote() { quote_seq = 0, viewID = oneBuy.viewID, lead = "0" });

            ViewData["List_preload_loc"] = new SelectList(shareGet.GetList_Location(), "location_id", "id_name", oneBuy.preload_loc);
            ViewData["List_po_to_loc"] = new SelectList(shareGet.FetchList_inv_loc(oneBuy.ItemID), "location_id", "id_name", oneBuy.po_to_loc.ToString());

            if (!string.IsNullOrWhiteSpace(oneBuy.preload_loc) && int.Parse(oneBuy.preload_loc) > 0)
            {
                List<BuySheet_Preload> PreloadList = pgmBuyOthers.GetList_PreloadCalendar(oneBuy.po_to_loc.ToString(), oneBuy.preload_loc, (DateTime)oneBuy.required_date);
                if (PreloadList == null || PreloadList.Count() <= 0)
                    TempData["message"] += "\r\n" + "None of preload belongs to this location id and required date.";
                else
                {
                    PreloadList.Insert(0, new BuySheet_Preload());
                    ViewData["PreloadList_SO"] = new SelectList(PreloadList, "order_no", "preload_po_so", (string.IsNullOrWhiteSpace(oneBuy.preload_so) ? PreloadList[1].order_no.ToString() : oneBuy.preload_so));
                }
            }

            List<Vendor> SupplierList = pgmBuySheet.GetList_Supplier(oneBuy.ItemID, oneBuy.po_to_loc.ToString());
            if (SupplierList == null)
                TempData["message"] += "\r\n" + "No supplier belongs to this part number and location id.";
            else
            {
                SupplierList.Insert(0, new Vendor { vendor_id = 0, vendor_name = "" });
                ViewData["SupplierList"] = new SelectList(SupplierList, "vendor_id", "vendor_name");
            }

            List<SelectListItem> StatusList = new List<SelectListItem>();
            StatusList.Add(new SelectListItem { Text = "Candidate", Value = "" });
            StatusList.Add(new SelectListItem { Text = "Approve", Value = "A" });
            StatusList.Add(new SelectListItem { Text = "Text", Value = "T" });
            ViewData["StatusList"] = StatusList;

            ViewData["CurrencyList"] = shareConvertor.ToDropDownOptions(shareGet.FetchCodeList("Currency"));
            ViewData["LatestExchangeRate"] = shareGet.Fetch_LatestExchangeRate();

            if (GlobalVariables.MySession.isDebug)
                ViewBag.notesPath = "/Procurement/ShowNotes?cluster=" + cluster + "&pm_Source=EditBuy" + "&pm_entryID=" + pm_entryId + "&pm_viewID=" + oneBuy.viewID.ToString();
            else
                //ViewBag.notesPath = "/" + GlobalConfig.MyConfig.ProjectName + "/Procurement/ShowNotes?cluster=" + cluster + "&pm_Source=EditBuy" + "&pm_entryID=" + pm_entryId + "&pm_viewID=" + oneBuy.viewID.ToString();
                ViewBag.notesPath = "/" + scriptsHome.getProjectName() + "/Procurement/ShowNotes?cluster=" + cluster + "&pm_Source=EditBuy" + "&pm_entryID=" + pm_entryId + "&pm_viewID=" + oneBuy.viewID.ToString();

            //if (cluster == "FTB" && string.IsNullOrWhiteSpace(oneBuy.PO_no.Trim()))
            if (cluster == "FTB")
            {
                var selection = new List<SelectListItem>();
                selection.Add(new SelectListItem { Value = "", Text = "" });
                selection.AddRange(shareConvertor.ToDropDownOptions(shareGet.FetchCodeList("FTB_Sourcing", 2)));
                ViewData["SoucingList"] = selection;
                selection = null;
                selection = new List<SelectListItem>();
                selection.Add(new SelectListItem { Value = "", Text = "" });
                selection.AddRange(shareConvertor.ToDropDownOptions(shareGet.FetchCodeList("FTB_Buyer", 2)));
                ViewData["BuyerList"] = selection;

                List<SelectListItem> PPAPList = new List<SelectListItem>();
                PPAPList.Add(new SelectListItem { Text = "", Value = "", Selected = (oneBuy.theFTB.ppap_level == "" ? true : false) });
                PPAPList.Add(new SelectListItem { Text = "PPAP1", Value = "PPAP1", Selected = (oneBuy.theFTB.ppap_level == "PPAP1" ? true : false) });
                PPAPList.Add(new SelectListItem { Text = "PPAP2", Value = "PPAP2", Selected = (oneBuy.theFTB.ppap_level == "PPAP2" ? true : false) });
                PPAPList.Add(new SelectListItem { Text = "PPAP3", Value = "PPAP3", Selected = (oneBuy.theFTB.ppap_level == "PPAP3" ? true : false) });
                PPAPList.Add(new SelectListItem { Text = "PPAP4", Value = "PPAP4", Selected = (oneBuy.theFTB.ppap_level == "PPAP4" ? true : false) });
                ViewData["List_PPAP"] = PPAPList;

                List<SelectListItem> SampleList = new List<SelectListItem>();
                //SampleList.Add(new SelectListItem { Text = "", Value = "" });
                SampleList.Add(new SelectListItem { Text = "Yes", Value = "True" });
                SampleList.Add(new SelectListItem { Text = "No", Value = "False" });
                ViewData["List_Sample"] = SampleList;

                ViewData["isAccountManager"] = shareGet.FetchCodeList("FTB_AccountManager").Exists(e => e.Value == GlobalVariables.MySession.Account);
                ViewData["isSourcing"] = shareGet.FetchCodeList("FTB_Sourcing").Exists(e => e.Value == GlobalVariables.MySession.Account);
                ViewData["isBuyer"] = shareGet.FetchCodeList("FTB_Buyer").Exists(e => e.Value == GlobalVariables.MySession.Account);
                if (oneBuy.theFTB.step == 2 && (bool)ViewData["isBuyer"])
                {
                    oneBuy.theFTB.step = 3;
                    SP_Return ResultModel = pgmBuySheet.FTB_GoUpdate(oneBuy.theFTB);
                    if (ResultModel.r == 1)
                    {
                        oneBuy.theFTB.theStepLogs.Clear();
                        oneBuy.theFTB.theStepLogs.AddRange(pgmBuyOthers.FetchAllFTB(oneBuy.viewID).First().theStepLogs);
                        GlobalVariables.MySession.List_Buy_FTBSheet.Find(w => w.viewID == oneBuy.theFTB.viewID).theFTB.step = oneBuy.theFTB.step;
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(TempData["message"].ToString()))
                TempData["message"] = null;
            return View(oneBuy);
        }


        [HttpPost]
        [MultipleButton(Name = "EditBuyAction", Argument = "Update")]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateBuy(Buy pm_theBuy, string cluster, string rollback_note, int isRollBackSourcing = 0, bool isSourcingReview = false)
        {
            TempData["message"] = null;

            if (pm_theBuy.theQuotes.Exists(e => e.z_status == "A" && e.vendor_id == 0))
            {
                TempData["pm_message"] = "Invalid vendor for Approved quote.";
                TempData["pm_updatedBuy"] = pm_theBuy;
                return RedirectToAction("EditBuy", new { cluster = cluster });
            }

            if (isSourcingReview)
                pm_theBuy.theFTB.sourcing_review = GlobalVariables.MySession.Account;
            if (cluster == "FTB" && !string.IsNullOrWhiteSpace(rollback_note))
                pm_theBuy.theFTB.step = -1;
            if (isRollBackSourcing == 1)
                pm_theBuy.theFTB.step = -2;
            if (cluster == "FTB")
                pm_theBuy.grand_total_value = pm_theBuy.grand_total_value.Replace(",", "").Replace(" ", "");
            SP_Return theResult = pgmBuySheet.UpdateOneBuy(pm_theBuy);

            if (theResult.r == 1)
            {
                if (cluster == "FTB" && !string.IsNullOrWhiteSpace(rollback_note))
                    pgmBuyNote.CreateNewNote(cluster, new BuyNote() { entryID = pm_theBuy.entryID, notes = rollback_note, user = GlobalVariables.MySession.Account, createDate = DateTime.Now }, "BuySheet");

                TempData["message"] = "Buy #" + pm_theBuy.viewID.ToString() + " updated.";

                switch (cluster)
                {
                    case "OS":
                        GlobalVariables.MySession.List_Buy_OSSheet = null;
                        if (Decimal.Parse(pm_theBuy.total_value) > 5000)
                            TempData["message"] = "Total value of this buy is greater than $5,000. Please consider split the shipment.";
                        break;
                    case "Domestic":
                        GlobalVariables.MySession.List_Buy_DomesticSheet = null;
                        if (Decimal.Parse(pm_theBuy.total_value) > 5000)
                            TempData["message"] = "Total value of this buy is greater than $5,000. Please consider split the shipment.";
                        break;
                    case "FTB":
                        GlobalVariables.MySession.List_Buy_FTBSheet = null;
                        if (Decimal.Parse(pm_theBuy.grand_total_value) > 5000)
                        //if (pm_theBuy.grand_total_value > 5000)
                            TempData["message"] = "Total value of this buy is greater than $5,000. Please consider split the shipment.";
                        break;
                    default:
                        break;
                }

                return RedirectToAction("BuySheet", new { cluster = cluster });
            }
            else
            {
                TempData["pm_message"] = theResult.msg;
                TempData["pm_updatedBuy"] = pm_theBuy;
                //return RedirectToAction("EditBuy", new { cluster = cluster, pm_entryId = pm_theBuy.entryID });
                return RedirectToAction("EditBuy", new { cluster = cluster });
            }
        }

        /*
        [HttpPost]
        [MultipleButton(Name = "CreateBuyAction", Argument = "locChg_Create")]
        public ActionResult goUpdatePOLoc_Create(Buy pm_theBuy)
        {
            TempData["createdBuy"] = pgmBuySheet.UpdatePOLoc(pm_theBuy);
            return RedirectToAction("CreateBuy", new { cluster = pm_theBuy.cluster });
        }
        */
        [HttpPost]
        [MultipleButton(Name = "EditBuyAction", Argument = "locChg_Edit")]
        public ActionResult goUpdatePOLoc_Edit(Buy pm_theBuy, string cluster)
        {
            TempData["pm_updatedBuy"] = pgmBuySheet.UpdatePOLoc(pm_theBuy);
            return RedirectToAction("EditBuy", "Procurement", new { cluster = cluster, pm_entryId = pm_theBuy.entryID });
        }

        /*
        public ActionResult goBuyCreateAction(Buy pm_theBuy, string pm_action)
        {
            return RedirectToAction(pm_action, new { cluster = pm_theBuy.cluster });
        }
        */

        #endregion

        #region Archive

        public ActionResult BuyArchive(string cluster)
        {
            switch (cluster)
            {
                case "OS":
                    ViewBag.PageInfo = shareCommon.GetPageInfo(23);
                    break;
                case "Domestic":
                    ViewBag.PageInfo = shareCommon.GetPageInfo(32);
                    break;
                case "FTB":
                    ViewBag.PageInfo = shareCommon.GetPageInfo(108);
                    break;
                default:
                    TempData["message"] = "Unavailable Cluster.";
                    break;
            }
            ViewBag.Dev = pgmBuySheet.devMode;
            ViewBag.cluster = cluster;
            ViewBag.takeSession = true;

            var url = Url.Action("BuyArchive", "Procurement", new { cluster = cluster });   // Outputcache root
            HttpResponse.RemoveOutputCacheItem(url);    // Clean output cache by root

            return View("BuyArchive", pgmBuyArchive.FetchArchiveBuys(null, cluster));
        }


        public PartialViewResult ShowArchive_Quotes(int pm_viewID)
        {
            ViewData["viewID"] = pm_viewID;
            return PartialView("BuyQuotes", pgmBuyArchive.FetchQuotesOfOneBuy(pm_viewID));
        }


        public ActionResult RunArchive(string cluster, string pm_ItemID)
        {
            switch (cluster)
            {
                case "OS":
                    ViewBag.PageInfo = shareCommon.GetPageInfo(23);
                    break;
                case "Domestic":
                    ViewBag.PageInfo = shareCommon.GetPageInfo(32);
                    break;
                case "FTB":
                    ViewBag.PageInfo = shareCommon.GetPageInfo(108);
                    break;
                default:
                    TempData["message"] = "Unavailable Cluster.";
                    break;
            }
            ViewBag.Dev = pgmBuySheet.devMode;
            ViewBag.cluster = cluster;
            ViewBag.takeSession = false;
            List<Filters> pm_FilterList = filterBuy.SetFilterForItemID(cluster, pm_ItemID);

            var url = Url.Action("BuyArchive", "Procurement", new { cluster = cluster, pm_ItemID = pm_ItemID });   // Outputcache root
            HttpResponse.RemoveOutputCacheItem(url);    // Clean output cache by root

            return View("BuyArchive", pgmBuyArchive.FetchArchiveBuys(pm_FilterList, cluster));
        }


        public ActionResult BuyArchiveItem(string item_id)
        {
            ViewBag.Dev = pgmBuySheet.devMode;
            //ViewBag.cluster = "OS";
            List<Buy_ArchiveItem> archiveList = pgmBuyArchive.FetchArchiveItem(item_id);
            ViewData["archiveList"] = JsonConvert.SerializeObject(archiveList);

            return View("BuyArchiveItem", archiveList);
        }


        public FileStreamResult DownloadArchiveItemSheet(string pm_archive)
        {
            return File(pgmBuyArchive.GetArchiveItemSheet(pm_archive), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ExportItem.xlsx");
        }

        #endregion

        #region PO

        [HttpPost]
        [MultipleButton(Name = "BuySheetAction", Argument = "NotBuy")]
        public ActionResult NotBuy(string cluster)
        {
            TempData["message"] = null;
            ViewBag.Dev = pgmBuySheet.devMode;

            SP_Return theResult = pgmBuyPO.SetNotBuy(cluster, Request.Form["collection_viewID"]);

            if (theResult.r == 1)
            {
                switch (cluster)
                {
                    case "OS":
                        GlobalVariables.MySession.List_Buy_OSSheet = null;
                        break;
                    case "Domestic":
                        GlobalVariables.MySession.List_Buy_DomesticSheet = null;
                        break;
                    case "FTB":
                        GlobalVariables.MySession.List_Buy_FTBSheet = null;
                        break;
                    default:
                        break;
                }
            }
            else
            {
                TempData["message"] = theResult.msg;
            }

            return RedirectToAction("BuySheet", new { cluster = cluster });
        }


        public async Task<ActionResult> ApplyPOAsync(string cluster)
        {
            TempData["message"] = null;
            SP_Return theResult = await pgmBuyPO.GoApplyPOAsync(cluster, Request.Form["collection_entryID"], Request.Form["pmNewPO"]);

            if (theResult.r == 1)
            {
                TempData["message"] = theResult.msg;

                switch (cluster)
                {
                    case "OS":
                        GlobalVariables.MySession.List_Buy_OSSheet = null;
                        break;
                    case "Domestic":
                        GlobalVariables.MySession.List_Buy_DomesticSheet = null;
                        break;
                    case "FTB":
                        GlobalVariables.MySession.List_Buy_FTBSheet = null;
                        break;
                    default:
                        break;
                }

                return RedirectToAction("BuySheet", new { cluster = cluster });
            }
            else
            {
                TempData["message"] = theResult.msg;
                return RedirectToAction("BuySheet", new { cluster = cluster });
            }
        }


        [HttpPost]
        [MultipleButton(Name = "BuySheetAction", Argument = "CutPO")]
        public ActionResult CutPO(string cluster)
        {
            TempData["message"] = null;
            ViewBag.Dev = pgmBuySheet.devMode;

            SP_Return theResult = pgmBuyPO.GoCutPO(cluster, Request.Form["collection_viewID"]);

            if (theResult.r == 1)
            {
                switch (cluster)
                {
                    case "OS":
                        GlobalVariables.MySession.List_Buy_OSSheet = null;
                        break;
                    case "Domestic":
                        GlobalVariables.MySession.List_Buy_DomesticSheet = null;
                        break;
                    case "FTB":
                        GlobalVariables.MySession.List_Buy_FTBSheet = null;
                        break;
                    default:
                        break;
                }
            }
            else
            {
                TempData["message"] = theResult.msg;
            }

            return RedirectToAction("BuySheet", new { cluster = cluster });
        }


        [HttpPost]
        [MultipleButton(Name = "BuySheetAction", Argument = "Review")]
        public ActionResult BuyCreatePO(string cluster)
        {
            ViewBag.cluster = cluster;
            ViewBag.Dev = pgmBuySheet.devMode;
            switch (cluster)
            {
                case "OS":
                    ViewBag.authorized = shareCommon.isAuthorized(19);
                    break;
                case "Domestic":
                    ViewBag.authorized = shareCommon.isAuthorized(31);
                    break;
                case "FTB":
                    ViewBag.authorized = shareCommon.isAuthorized(107);
                    break;
                default:
                    TempData["message"] = "Unavailable Cluster.";
                    break;
            }
            List<BuySheet_CreatePO> POList = pgmBuyPO.GetList_PrepareCreatingPO(cluster, Request.Form["collection_viewID"]);
            ViewBag.createCount = POList.Where(p => p.go == "True").Count();

            return View("BuyCreatePO", POList);
        }


        [HttpPost]
        public async Task<ActionResult> GoCreatePOAsync(List<BuySheet_BuyID> pm_Sheet, string cluster)
        {
            TempData["message"] = null;
            cmd_Response theResult = await pgmBuyPO.GoCreatePOAsync(cluster, pm_Sheet.Where(s => s.GroupNo > 0).OrderBy(o => o.GroupNo).ToList());

            if (theResult.ResultModel.r != 1)
            {
                TempData["message"] = theResult.ResultModel.JsonData;
                return RedirectToAction("BuySheet", new { cluster = cluster });
            }

            TempData["ResultList"] = theResult.ResultList;
            return RedirectToAction("BuyCreatePOResult", new { cluster = cluster });
        }


        public ActionResult BuyCreatePOResult(string cluster)
        {
            ViewBag.cluster = cluster;
            ViewBag.Dev = pgmBuySheet.devMode;
            ViewData["ResultList"] = TempData["ResultList"];

            List<BuySheet_CreatePO> POList = pgmBuyPO.GetList_CreatingPOResult(cluster, ViewData["ResultList"] as List<cmd_Result>);
            switch (cluster)
            {
                case "OS":
                    GlobalVariables.MySession.List_Buy_OSSheet = null;
                    break;
                case "Domestic":
                    GlobalVariables.MySession.List_Buy_DomesticSheet = null;
                    break;
                case "FTB":
                    GlobalVariables.MySession.List_Buy_FTBSheet = null;
                    break;
                default:
                    break;
            }

            return View("BuyCreatePOResult", POList);
        }


        public ActionResult FTB_OpenPO()
        {
            ViewBag.PageInfo = shareCommon.GetPageInfo(41);
            ViewBag.Dev = pgmBuySheet.devMode;
            return View(pgmBuyPO.FTB_FetchOpenPO());
        }

        #endregion

        #region Sorting

        public ActionResult SortSheet(string cluster, string sort_field)
        {
            pgmBuySheet.SetOrder(cluster, sort_field);

            return RedirectToAction("BuySheet", new { cluster = cluster });
        }


        public ActionResult ClearSort(string cluster)
        {
            pgmBuySheet.ClearSort(cluster);
            return RedirectToAction("BuySheet", new { cluster = cluster });
        }

        #endregion

        #region Rates

        public PartialViewResult ShowRates_MetalDetails(int pm_RRN)
        {
            ViewBag.authorized = shareCommon.isAuthorized(42);
            BuySheet_Rates_Metals theRates = new BuySheet_Rates_Metals();
            if (pm_RRN == 0)
            {
                theRates.i = pm_RRN;
                ViewBag.pmCRUD = "C";
                ViewData["MetalUnit"] = "";
            }
            else
            {
                theRates = GlobalVariables.MySession.List_Rates_Metal.Find(t => t.i == pm_RRN);
                ViewBag.pmCRUD = "U";
                ViewData["MetalUnit"] = pgmBuyOthers.GetRates_MetalsUnit(theRates.metal_id);
            }

            ViewData["List_Metals"] = new SelectList(shareConvertor.ToDropDownOptions(shareGet.FetchCodeList("Metals")), "Value", "Text", theRates.metal_id);

            return PartialView("_RatesMetalDetails", theRates);
        }


        public ActionResult GetMetalChart(string pm_metal_name, string pm_metal_unit)
        {
            return File(pgmBuyOthers.GetRates_MetalsChart(pm_metal_name, pm_metal_unit), "image/bytes");
        }


        public ActionResult GetCurrencyChart(string pm_currency_name)
        {
            return File(shareGet.GetCurrencyChart(pm_currency_name), "image/bytes");
        }


        public PartialViewResult ShowRates_MetalCharts(string pm_metal_name, string pm_metal_unit)
        {
            ViewData["MetalName"] = pm_metal_name;
            ViewData["Metalunit"] = pm_metal_unit;
            return PartialView("_RatesMetalCharts");
        }


        public PartialViewResult ShowRates_CurrencyCharts(string pm_currency_name)
        {
            ViewData["CurrencyName"] = pm_currency_name;
            return PartialView("_RatesCurrencyCharts");
        }


        public ActionResult RatesManagement_Metals()
        {
            ViewBag.PageInfo = shareCommon.GetPageInfo(42);
            ViewBag.Dev = pgmBuySheet.devMode;
            ViewBag.authorized = shareCommon.isAuthorized(42);

            List<BuySheet_Rates_Metals> RateList = pgmBuyOthers.FetchAllRates_Metals();
            List<int> MetalList = RateList.Select(x => x.metal_id).Distinct().ToList();
            List<string> units = new List<string>();
            for (int j = 0; j < MetalList.Count(); j++)
                units.Add(pgmBuyOthers.GetRates_MetalsUnit(MetalList[j]));
            ViewData["List_Uints"] = units;

            return View("RatesManagement_Metals", RateList);
        }


        public ActionResult Rates_Currency()
        {
            ViewBag.PageInfo = shareCommon.GetPageInfo(44);
            ViewBag.Dev = pgmBuySheet.devMode;
            return View("Rates_Currency", shareGet.FetchAllRates_Currency());
        }


        [ValidateAntiForgeryToken]
        public ActionResult GoSaveRates_Metals(string pmCRUD, BuySheet_Rates_Metals pmRate)
        {
            SP_Return resultModel = pgmBuyOthers.GoSaveRates_Metal(pmCRUD, pmRate);
            TempData["message"] = resultModel.msg;

            return RedirectToAction("RatesManagement_Metals");
        }


        public ActionResult GoDeleteRate_Metals(int pm_RRN)
        {
            SP_Return resultModel = pgmBuyOthers.GoSaveRates_Metal("D", GlobalVariables.MySession.List_Rates_Metal.Find(t => t.i == pm_RRN));
            TempData["message"] = resultModel.msg;
            return RedirectToAction("RatesManagement_Metals");
        }


        #endregion

        #region Lot

        public PartialViewResult Show_VendorGroup(int group_id)
        {
            ViewBag.authorized = shareCommon.isAuthorized(45);
            int maxiumCnt = 0;
            List<Lot_VendorGroup> VendorList = new List<Lot_VendorGroup>();

            if (group_id == 0)
                ViewData["CURD"] = "C";
            else
            {
                ViewData["CURD"] = "U";
                VendorList = pgmBuyOthers.Fetch_GroupVendor(group_id);
            }

            maxiumCnt = VendorList.Count() + 3;
            for (int i = VendorList.Count(); i < maxiumCnt; i++)
            {
                VendorList.Add(
                    new Lot_VendorGroup()
                    {
                        selected = false,
                        i = i,
                        group_id = 0,
                        group_name = "",
                        vendor_id = null,
                        vendor_name = ""
                    }
                );
            }

            return PartialView("_LotVendorGroup", VendorList);
        }


        public ActionResult GetVendorGroupList(string item_id, string location_id) //It will be fired from Jquery ajax call
        {
            var jsonData = new List<Vendor>();
            if (!string.IsNullOrWhiteSpace(item_id) && location_id != "0")
                jsonData = pgmBuySheet.GetList_Supplier(item_id, location_id).ToList();
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }


        public ActionResult LotManagement()
        {
            ViewBag.PageInfo = shareCommon.GetPageInfo(45);
            ViewBag.Dev = pgmBuySheet.devMode;
            ViewBag.authorized = shareCommon.isAuthorized(45);

            List<Lot_Number> theSheet = pgmBuyOthers.FetchAllLot_AssignedNumber();
            return View(theSheet);
        }


        public ActionResult GoIncreaseLotNumber(int group_id, int amount)
        {
            SP_Return resultModel = pgmBuyOthers.AddLotNumber(group_id, amount);
            TempData["message"] = resultModel.msg;

            return RedirectToAction("LotManagement");
        }


        public ActionResult GoUpdateVendorGroup(List<Lot_VendorGroup> pm_Vendors)
        {
            SP_Return resultModel = pgmBuyOthers.UpdateVendorGroup(pm_Vendors);
            if (resultModel.r != 1)
                TempData["message"] = resultModel.msg;

            return RedirectToAction("LotManagement");
        }


        public ActionResult GoOccupyLotNumber(int pm_lot_uid, int pm_po_no, int pm_po_line, int pm_amount)
        {
            SP_Return resultModel = pgmBuyOthers.OccupyLotNumber(pm_lot_uid, pm_po_no, pm_po_line, pm_amount);
            TempData["message"] = resultModel.msg;

            return RedirectToAction("LotManagement");
        }

        #endregion

        #region Tooling Charge

        [ValidateAntiForgeryToken]
        public FileResult Download_ReceiptsSheet()
        {
            MemoryStream example = pgmBuyOthers.Get_ReceiptsSheet();
            example.Seek(0, 0);
            Response.Headers.Add("Content-Disposition", "attachment;filename=ReceiptsSheet.xlsx");
            return File(example, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        }


        public PartialViewResult Show_ToolingChargeReceipts(string pm_vendor_name, string pm_vendors, string pm_item_id, string pm_real_part_no, DateTime pm_refund_date)
        {
            ViewData["vendor_name"] = pm_vendor_name;
            ViewData["part_no"] = pm_real_part_no;
            return PartialView("_ToolingChargeReceipts", pgmBuyOthers.FetchToolingChargerReceipts(pm_vendors, pm_item_id, pm_real_part_no, pm_refund_date));
        }


        public ActionResult ToolingCharge_Archive()
        {
            ViewBag.PageInfo = shareCommon.GetPageInfo(69);
            ViewBag.Dev = pgmBuySheet.devMode;
            List<Charge_Tooling> theSheet = pgmBuyOthers.FetchToolingCharge_Archive();
            return View(theSheet);
        }


        public ActionResult ToolingCharge()
        {
            ViewBag.PageInfo = shareCommon.GetPageInfo(65);
            ViewBag.Dev = pgmBuySheet.devMode;
            ViewBag.authorized = shareCommon.isAuthorized(65);

            ViewBag.Source = "ToolingCharge";
            List<Charge_Tooling> theSheet = pgmBuyOthers.Get_ToolingCharge();
            if (GlobalVariables.MySession.List_Charge_ToolingFilters != null)
                ViewBag.Filters = GlobalVariables.MySession.List_Charge_ToolingFilters;

            return View(theSheet);
        }


        [ValidateAntiForgeryToken]
        //public ActionResult GoRefound_ToolingCharge(int pm_currency_id, List<Charge_Tooling> pm_tc)
        public async Task<ActionResult> GoRefound_ToolingCharge(List<Charge_Tooling> pm_tc)
        {
            //SP_Return resultModel = pgmBuyOthers.Go_RefundedToolingCharge(pm_tc);
            SP_Return resultModel = await pgmBuyOthers.Go_RefundedToolingCharge2(pm_tc);
            TempData["message"] = resultModel.msg;
            //return RedirectToAction("ToolingCharge", new { pm_currency_id = pm_currency_id });
            return RedirectToAction("ToolingCharge");
        }

        #endregion

        #region Asia Country

        public ActionResult AsiaCountryManagement()
        {
            ViewBag.PageInfo = shareCommon.GetPageInfo(79);
            ViewBag.Dev = pgmBuySheet.devMode;
            ViewBag.authorized = shareCommon.isAuthorized(79);
            List<Asia_Country> theCountries = pgmBuyOthers.Fetch_AsiaCountry();
            return View(theCountries);
        }


        [ValidateAntiForgeryToken]
        public ActionResult GoSave_AsiaCountries(List<Asia_Country> pm_countries)
        {
            SP_Return resultModel = pgmBuyOthers.Save_AsiaCountries(pm_countries);
            TempData["message"] = resultModel.msg;
            return RedirectToAction("AsiaCountryManagement");
        }

        #endregion

        #region Payment Terms

        public ActionResult VendorPaymentTerms()
        {
            ViewBag.PageInfo = shareCommon.GetPageInfo(89);
            ViewBag.Dev = pgmBuySheet.devMode;
            List<Vendor_PaymentTerms> thePaymentTerms = pgmBuyOthers.Fetch_VendorPaymentTerms();
            return View(thePaymentTerms);
        }

        #endregion

        #region Samples Inspection Free

        public ActionResult InspectionFreeManagement()
        {
            ViewBag.PageInfo = shareCommon.GetPageInfo(103);
            ViewBag.Dev = pgmBuySheet.devMode;
            ViewBag.authorized = shareCommon.isAuthorized(103);
            return View(pgmBuyOthers.Fetch_InspectionFree());
        }


        [ValidateAntiForgeryToken]
        public ActionResult GoSave_InspectionFree(int pm_suppler_id)
        {
            SP_Return resultModel = pgmBuyOthers.Update_InspectionFree("New", pm_suppler_id);
            TempData["message"] = resultModel.msg;
            return RedirectToAction("InspectionFreeManagement");
        }


        public ActionResult GoDelete_InspectionFree(int pm_suppler_id)
        {
            SP_Return resultModel = pgmBuyOthers.Update_InspectionFree("Delete", pm_suppler_id);
            TempData["message"] = resultModel.msg;
            return RedirectToAction("InspectionFreeManagement");
        }

        #endregion

        #region Supplier Scorecard

        public PartialViewResult Show_SupplierScorecard(int pm_supplier_id, int pm_target)
        {
            SupplierScorecard_finished theSheet = pgmSupplierScorecard.SupplierScorecard_Calculation(pm_supplier_id, pm_target);
            if (theSheet.DataFlag.r == 0)
                TempData["message"] = theSheet.DataFlag.msg;
            else
                ViewData["supplier_name"] = "<h5>" + theSheet.DataFlag.msg.Replace("\r\n", "</h5><h5>") + "</h5>";
            return PartialView("_Scorecard", theSheet);
        }


        public ActionResult SupplierScorecard()
        {
            ViewBag.PageInfo = shareCommon.GetPageInfo(86);
            ViewBag.Dev = pgmSupplierScorecard.devMode;
            return View();
        }

        #endregion

    }
}