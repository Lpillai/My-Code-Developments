using Home.Models;
using Newtonsoft.Json.Linq;
using Reports;
using RuntimeVariables;
using SharedScrpits;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Taiwan;
using Transfer;


namespace Home.Controllers
{
    [SessionExpireFilter]
    public class ReportsController : Controller
    {
        Common_Share shareCommon = new Common_Share();
        Get_Share shareGet = new Get_Share();
        Convertor_Share shareConvertor = new Convertor_Share();
        FileActions_Share shareFileActions = new FileActions_Share();
        HomeScripts scriptsHome = new HomeScripts();
        Reports_Program pgmReports = new Reports_Program();
        RPT_Transfer_Program pgmRPT_Transfer = new RPT_Transfer_Program();
        RPT_Warehouse_Program pgmRPT_Warehouse = new RPT_Warehouse_Program();
        RPT_Procurement_Program pgmRPT_Procurement = new RPT_Procurement_Program();
        RPT_QC_Program pgmRPT_QC = new RPT_QC_Program();
        CommercialInvoice_Program pgmCI = new CommercialInvoice_Program();
        PackingList_Program pgmPackingList = new PackingList_Program();
        Attendance_Program pgmAttendance = new Attendance_Program();


        public ActionResult ReportsList(string cluster)
        {
            List<LoginReport> theReportList = pgmReports.FetchReportList(cluster);
            ViewData["cluster"] = cluster;
            ViewData["cluster_name"] = theReportList.FirstOrDefault().cluster_name;
            ViewBag.Dev = pgmReports.devMode;

            return View("ReportsList", theReportList);
        }


        public PartialViewResult Show_EmbeddedSSRS(string pm_path, string pm_parm)
        {
            ViewData["ReportViewer"] = pgmReports.EmbedSSRS(pm_path, pm_parm);
            return PartialView("_ReportViewer");
        }


        [HttpGet]
        public FileResult Show_NewTab(string pm_path, string pm_parm)
        {
            MemoryStream example = pgmReports.NewTab(pm_path, pm_parm);
            example.Seek(0, 0);
            //Response.Headers.Add("Content-Disposition", "inline");
            return File(example, "application/pdf");
        }


        [DeleteFileAttribute]
        public ActionResult Download_GetRenderedFile(string pm_cluster, string pm_file_name)
        {
            string fullPath = Path.Combine(Server.MapPath("~/UploadedFiles"), pm_file_name);
            FileInfo fi = new FileInfo(pm_file_name);

            if (fi.Extension == ".pdf" || fi.Extension == ".PDF")
                return File(fullPath, "application/pdf", pm_file_name);
            else if (fi.Extension == ".xlsx" || fi.Extension == ".XLSX")
                return File(fullPath, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", pm_file_name);
            else
            {
                TempData["message"] = "Download error! No available file type.";
                return RedirectToAction("ReportsList", new { cluster = pm_cluster });
            }
        }

        #region Test

        public ActionResult TestDownload_1(string pm_cluster, string pm_path, string pm_parm)
        {
            string fileName = "ShippingPkgBulk_1_" + DateTime.Now.ToString("yyMMddHHmmss") + ".pdf";
            string fullPath = Path.Combine(Server.MapPath("~/UploadedFiles"), fileName);

            using (FileStream file = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
                pgmReports.DownloadFile(pm_path, pm_parm, 0).WriteTo(file);

            return RedirectToAction("Download_GetRenderedFile", new { pm_cluster = pm_cluster, pm_file_name = fileName });
        }


        public FileResult TestDownload_2()
        {
            MemoryStream example = pgmRPT_Warehouse.Get_ShipByOrderNo("/Label/Shipping_PkgBulk", "7033882", "1955007", "1");
            example.Seek(0, 0);

            Response.Headers.Add("Content-Disposition", "attachment;filename=ShippingPkgBulk_2.pdf");
            return File(example, "application/pdf");
        }

        #endregion

        #region AccountManager

        public ActionResult Show_PackingList_Sales(string pm_cluster, string pm_path, string pm_parm)
        {
            string fileName = "PackingList_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx";
            string fullPath = Path.Combine(Server.MapPath("~/UploadedFiles"), fileName);

            using (FileStream file = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
                pgmReports.DownloadFile(pm_path, pm_parm, 1).WriteTo(file);

            return RedirectToAction("Download_GetRenderedFile", new { pm_cluster = pm_cluster, pm_file_name = fileName });
        }

        #endregion

        #region OSBuy

        public PartialViewResult Show_CartonPO(string rpt_name)
        {
            ViewBag.ReportName = rpt_name;
            return PartialView("_OSBuy_CartonPO");
        }


        public PartialViewResult Get_CartonPO(string pm_PO)
        {
            ViewBag.authorized = shareCommon.isReportAuthorized(1);
            ViewData["PO"] = pm_PO;
            return PartialView("_OSBuy_CartonLabel", pgmRPT_Procurement.Lot_GetPOData(pm_PO));
        }


        [HttpPost]
        [MultipleButton(Name = "ReportOSBuyAction", Argument = "Carton")]
        public FileResult Open_Carton(string pm_PO, List<Lot_PO> pm_Lots)
        {
            MemoryStream example = pgmRPT_Procurement.Lot_OpenLabel_Carton("/Label/Label_Carton", pm_PO, pm_Lots);
            example.Seek(0, 0); //back to the start of the stream before returning it
            return File(example, "application/pdf");
        }


        [HttpPost]
        [MultipleButton(Name = "ReportOSBuyAction", Argument = "Bulk")]
        public FileResult Open_Bulk(string pm_PO, List<Lot_PO> pm_Lots)
        {
            MemoryStream example = pgmRPT_Procurement.Lot_OpenLabel_Carton("/Label/Label_Bulk", pm_PO, pm_Lots);
            example.Seek(0, 0);
            return File(example, "application/pdf");
        }


        [HttpPost]
        [MultipleButton(Name = "ReportOSBuyAction", Argument = "Bag")]
        public FileResult Open_Bag(string pm_PO, List<Lot_PO> pm_Lots)
        {
            MemoryStream example = pgmRPT_Procurement.Lot_OpenLabel_Carton("/Label/Label_Bag", pm_PO, pm_Lots);
            example.Seek(0, 0);
            return File(example, "application/pdf");
        }


        public ActionResult Show_OpenOrder(string pm_cluster, string pm_path, string pm_parm)
        {
            string fileName = "OpenOrder_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx";
            string fullPath = Path.Combine(shareFileActions.GetUploadFolder(), fileName);

            using (FileStream file = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
                pgmReports.DownloadFile(pm_path, pm_parm, 1).WriteTo(file);

            return RedirectToAction("Download_GetRenderedFile", new { pm_cluster = pm_cluster, pm_file_name = fileName });
        }


        public PartialViewResult Show_Audit_item_maintenance(string cluster, string rpt_name)
        {
            ViewData["cluster"] = cluster;
            ViewBag.ReportName = rpt_name;
            return PartialView("_OSBuy_audit_item_maintenance");
        }


        public ActionResult Run_Audit_item_maintenance(string cluster)
        {
            SP_Return ResultModel = pgmRPT_Procurement.Go_Audit_item_maintenance();

            if (ResultModel.r == 1)
            {
                TempData["message"] = "Command Sent.";
                return RedirectToAction("ReportsList", new { cluster = cluster });
            }
            else
            {
                TempData["message"] = ResultModel.msg;
                return RedirectToAction("Show_Audit_item_maintenance", new { cluster = cluster });
            }
        }


        public PartialViewResult Show_BuyHistory(string rpt_name)
        {
            ViewBag.ReportName = rpt_name;
            return PartialView("_OSBuy_BuyHistory");
        }


        public FileStreamResult Run_OSBuyHistoryReport(DateTime pm_date_start, DateTime pm_date_end)
        {
            string theFileName = "BuyHistory_" + DateTime.Now.ToString("yyyyMMdd", DateTimeFormatInfo.InvariantInfo) + ".xlsx";
            MemoryStream example = pgmRPT_Procurement.OSBuyHistory_ProduceSheet(pm_date_start, pm_date_end);
            return File(example, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", theFileName);
        }

        #endregion

        #region Procurement

        public PartialViewResult Show_PurchaseOrder(string rpt_name)
        {
            ViewBag.ReportName = rpt_name;
            return PartialView("_PurchaseOrder");
        }


        public FileStreamResult Open_PurchaseOrder(int pm_po_no)
        {
            string theFileName = pm_po_no.ToString() + "_" + DateTime.Now.ToString("yyyyMMdd", DateTimeFormatInfo.InvariantInfo) + ".pdf";
            MemoryStream example = pgmRPT_Procurement.PurchaseOrder("/Intranet/PurchaseOrder", pm_po_no);
            return File(example, "application/pdf", theFileName);
        }


        public PartialViewResult Show_SupplierScoreCard_Summary(string cluster, string rpt_name)
        {
            ViewData["cluster"] = cluster;
            ViewBag.ReportName = rpt_name;
            return PartialView("_Procurement_SupplierScoreCard_Summary");
        }


        public ActionResult Run_SupplierScoreCard_Summary(string cluster)
        {
            SP_Return ResultModel = pgmRPT_Procurement.Go_SupplierScoreCard_Summary((!scriptsHome.devStatus() ? 1 : 0).ToString());

            if (ResultModel.r == 1)
            {
                TempData["message"] = "Command Sent.";
                return RedirectToAction("ReportsList", new { cluster = cluster });
            }
            else
            {
                TempData["message"] = ResultModel.msg;
                return RedirectToAction("Show_SupplierScoreCard_Summary", new { cluster = cluster });
            }
        }


        public FileStreamResult Run_AggregateInspectionReport()
        {
            string theFileName = "AggregateInspectionReport_" + DateTime.Now.ToString("yyyyMMdd", DateTimeFormatInfo.InvariantInfo) + ".xlsx";
            MemoryStream example = pgmRPT_Procurement.AggregateInspection_ProduceSheet();
            return File(example, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", theFileName);
        }

        #endregion

        #region Transfer

        public PartialViewResult Show_Audit_preload(string cluster, string rpt_name)
        {
            ViewData["cluster"] = cluster;
            ViewBag.ReportName = rpt_name;
            return PartialView("_Transfer_audit_preload");
        }


        public ActionResult Run_Audit_preload(string cluster)
        {
            SP_Return ResultModel = pgmRPT_Transfer.Go_Audit_preload();

            if (ResultModel.r == 1)
            {
                TempData["message"] = "Command Sent.";
                return RedirectToAction("ReportsList", new { cluster = cluster });
            }
            else
            {
                TempData["message"] = ResultModel.msg;
                return RedirectToAction("Show_Audit_preload", new { cluster = cluster });
            }
        }


        public PartialViewResult Show_PackingDetails(string pm_report, string pmPickTickets)
        {
            ViewData["pm_report"] = pm_report;
            ViewData["pmPickTickets"] = pmPickTickets;
            ViewData["PickTickets_label"] = "<span>" + pmPickTickets.Replace(",", "</span><span>") + "</span>";

            TempData["message"] = TempData["Controller_Message"];
            return PartialView("_PackingDetails", pgmPackingList.GetPackingList(pmPickTickets));
        }


        public PartialViewResult Show_PackingList(string rpt_name, string pm_parm, string pmPickTickets)
        {
            ViewBag.ReportName = rpt_name;

            JObject obj = JObject.Parse(pm_parm);
            string pm_report = (string)obj["pm_report"];
            ViewData["pm_report"] = pm_report.Replace("/", "%2F").Replace(" ", "%20");

            ViewData["pmPickTickets"] = pmPickTickets;
            return PartialView("_PackingList");
        }


        public ActionResult GoUpdate_Packing_LoadDefault(string pmPickTickets)
        {
            SP_Return ResultModel = pgmPackingList.LoadDefaultPackingList();
            TempData["Controller_Message"] = ResultModel.msg;
            return RedirectToAction("Show_PackingDetails", new { pmPickTickets = pmPickTickets });
        }


        [ValidateAntiForgeryToken]
        public ActionResult GoUpdate_Packing_UnitWeight(string pmPickTickets, List<Transfer_PackingList_line> pm_PackingList_line)
        {
            pgmPackingList.RecalculatePackingList(pm_PackingList_line);
            return RedirectToAction("Show_PackingDetails", new { pmPickTickets = pmPickTickets });
        }


        public FileStreamResult GoDownload_PackingSpreadsheet(string pmPickTickets)
        {
            string theFileName = "PackingSpreadsheet_" + DateTime.Now.ToString("yyMMddHHmmss") + ".xlsx";
            MemoryStream example = pgmRPT_Transfer.Get_PackingList_lines();
            example.Seek(0, 0); //back to the start of the stream before returning it
            return File(example, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", theFileName);
        }


        [ValidateAntiForgeryToken]
        public FileStreamResult GoPrint_PackingList(string pm_report, string pmPickTickets, Transfer_PackingList_hdr pm_packing_list, int plt)
        {
            pgmPackingList.SetPackingListShip(pm_packing_list, plt);
            SP_Return ResultModel = pgmPackingList.SavePackingList();

            //MemoryStream example = pgmRPT_Transfer.RPT_PackingList("/Intranet/Packing List");
            MemoryStream example = pgmRPT_Transfer.RPT_PackingList(pm_report);
            example.Seek(0, 0);

            string file_name = "PackingList_" + pmPickTickets + ".pdf";
            return File(example, "application/pdf", file_name);
        }


        [ValidateAntiForgeryToken]
        public FileStreamResult GoPrint_PalletsMark(string pm_report, string pmPickTickets, List<Transfer_Pallet> pm_Pallets)
        {
            SP_Return ResultModel = pgmPackingList.SavePalletList(pmPickTickets, pm_Pallets);

            //MemoryStream example = pgmRPT_Transfer.RPT_PalletsMark("/Intranet/Pallets Mark", pmPickTickets);
            MemoryStream example = pgmRPT_Transfer.RPT_PalletsMark(pm_report, pmPickTickets);
            example.Seek(0, 0);

            string file_name = "PalletsMark_" + pmPickTickets + ".pdf";
            return File(example, "application/pdf", file_name);
        }


        public PartialViewResult Show_PalletsMarkDetails(string pm_report, string pmPickTickets)
        {
            ViewData["pm_report"] = pm_report;
            ViewData["pmPickTickets"] = pmPickTickets;
            ViewData["PickTickets_label"] = "<span>" + pmPickTickets.Replace(",", "</span><span>") + "</span>";

            TempData["message"] = TempData["Controller_Message"];
            return PartialView("_PalletsMarkDetails", pgmPackingList.GetPalletList(pmPickTickets));
        }


        public PartialViewResult Show_PalletsMark(string rpt_name, string pm_parm, string pmPickTickets)
        {
            ViewBag.ReportName = rpt_name;

            JObject obj = JObject.Parse(pm_parm);
            string pm_report = (string)obj["pm_report"];
            ViewData["pm_report"] = pm_report.Replace("/", "%2F").Replace(" ", "%20");

            ViewData["pmPickTickets"] = pmPickTickets;
            return PartialView("_PalletsMark");
        }


        public PartialViewResult Show_CI(string cluster, string rpt_name)
        {
            ViewBag.cluster = cluster;
            ViewBag.ReportName = rpt_name;
            return PartialView("_CI");
        }


        public ActionResult GoDownLoad_CI(string pm_cluster, string pmPickTickets, bool isKits, bool isUOM100)
        {
            SP_Return ResultModel = pgmRPT_Transfer.RPT_CommercialInvoice(pmPickTickets, isKits, isUOM100);

            if (ResultModel.r == 1)
                return RedirectToAction("Download_GetRenderedFile", new { cluster = pm_cluster, pm_file_name = ResultModel.JsonData });
            else
            {
                TempData["message"] = ResultModel.msg;
                return RedirectToAction("AlertingMessage", "Home");
            }
        }

        #endregion

        #region Warehouse Regular

        public PartialViewResult Show_StringToBarcode(string rpt_name)
        //public PartialViewResult Show_StringToBarcode(string rpt_name, string pm_parm)
        {
            ViewBag.ReportName = rpt_name;
            /*
            JObject obj = JObject.Parse(pm_parm);
            string pm_label = (string)obj["pm_label"];
            string pm_kind = (string)obj["pm_kind"];
            ViewData["label"] = pm_label;
            ViewData["kind"] = pm_kind;
            */
            ViewData["KindList"] = shareConvertor.ToDropDownOptions(shareGet.FetchCodeList("Label_BarcodeKind", 2));

            return PartialView("_StringToBarcode");
        }


        public FileResult Open_StringToBarcode(string pm_kind, string pm_string, bool isText)
        {
            MemoryStream example = pgmRPT_Warehouse.Get_StringToBarcode(pm_kind, pm_string, isText);
            example.Seek(0, 0);
            Response.Headers.Add("Content-Disposition", "attachment;filename=StringTo" + pm_kind + ".pdf");
            return File(example, "application/pdf");
        }


        public PartialViewResult Show_CleanedPart2(string rpt_name, string pm_parm)
        {
            ViewBag.ReportName = rpt_name;

            JObject obj = JObject.Parse(pm_parm);
            string pm_loc_grp = (string)obj["pm_loc_grp"];
            string pm_customer = (string)obj["pm_customer"];
            string pm_label = (string)obj["pm_label"];
            ViewData["pm_loc_grp"] = pm_loc_grp;
            ViewData["pm_customer"] = pm_customer;
            ViewData["label"] = pm_label;

            return PartialView("_CleanedPart2");
        }
        public PartialViewResult Show_CleanedPart3(string rpt_name, string pm_parm)
        {
            ViewBag.ReportName = rpt_name;

            JObject obj = JObject.Parse(pm_parm);
            string pm_label = (string)obj["pm_label"];
            ViewData["label"] = pm_label;

            return PartialView("_CleanedPart3");
        }

        public FileResult Open_CleanedPart2(string pm_label, string pm_customer, string pm_lot, string pm_loc_grp, string pm_qty)
        {
            MemoryStream example = pgmRPT_Warehouse.Get_CleanedPart2(pm_label, pm_customer, pm_lot, pm_loc_grp, pm_qty);
            example.Seek(0, 0);
            Response.Headers.Add("Content-Disposition", "attachment;filename=" + pm_lot + ".pdf");
            return File(example, "application/pdf");
        }
        public FileResult Open_CleanedPart3(string pm_label, string pm_process_x_transaction_uid, string pm_lot, string pm_qty)
        {
            MemoryStream example = pgmRPT_Warehouse.Get_CleanedPart3(pm_label, pm_process_x_transaction_uid, pm_lot, pm_qty);
            example.Seek(0, 0);
            Response.Headers.Add("Content-Disposition", "attachment;filename=" + pm_process_x_transaction_uid + "-" + pm_lot + ".pdf");
            return File(example, "application/pdf");
        }


        public PartialViewResult Show_LocationBins(string rpt_name, string pm_parm)
        {
            ViewBag.ReportName = rpt_name;

            JObject obj = JObject.Parse(pm_parm);
            string pm_loc_grp = (string)obj["pm_loc_grp"];
            string pm_label = (string)obj["pm_label"];
            ViewData["pm_loc_grp"] = pm_loc_grp;
            ViewData["label"] = pm_label;

            List<SelectListItem> selectList = new List<SelectListItem>();
            selectList.Add(new SelectListItem { Value = "20", Text = "20" });
            selectList.Add(new SelectListItem { Value = "22", Text = "22" });
            selectList.Add(new SelectListItem { Value = "24", Text = "24" });
            selectList.Add(new SelectListItem { Value = "28", Text = "28" });
            selectList.Add(new SelectListItem { Value = "32", Text = "32" });
            selectList.Add(new SelectListItem { Value = "36", Text = "36" });
            selectList.Add(new SelectListItem { Value = "48", Text = "48" });
            selectList.Add(new SelectListItem { Value = "56", Text = "56" });
            selectList.Add(new SelectListItem { Value = "64", Text = "64" });
            selectList.Add(new SelectListItem { Value = "72", Text = "72" });
            ViewData["FontSize"] = selectList;

            return PartialView("_LocationBins");
        }


        public FileResult Open_LocationBins(string pm_label, string pm_loc_grp, string bin_id_start, string bin_id_end, string font_size)
        {
            MemoryStream example = pgmRPT_Warehouse.Get_LocationBins(pm_label, pm_loc_grp, bin_id_start, bin_id_end, font_size);
            example.Seek(0, 0);
            Response.Headers.Add("Content-Disposition", "attachment;filename=LocationBins" + pm_loc_grp + ".pdf");
            return File(example, "application/pdf");
        }


        public PartialViewResult Show_RecPkgBulk2(string rpt_name, string pm_parm)
        {
            ViewBag.ReportName = rpt_name;

            JObject obj = JObject.Parse(pm_parm);
            string pm_label = (string)obj["pm_label"];
            string pm_loc = (string)obj["pm_loc"];
            ViewData["label"] = pm_label;
            ViewData["loc"] = pm_loc;

            return PartialView("_RecPkgBulk");
        }


        public FileResult Open_RecPkgBulk2(string pm_label, string pm_location_id, string pm_lot_cd, string pm_qty)
        {
            MemoryStream example = pgmRPT_Warehouse.Get_RecPkgBulk(pm_label, pm_location_id, pm_lot_cd, pm_qty);
            example.Seek(0, 0);
            Response.Headers.Add("Content-Disposition", "attachment;filename=" + pm_lot_cd + ".pdf");
            return File(example, "application/pdf");
        }


        public PartialViewResult Show_ShipByPickTicket(string rpt_name, string pm_parm)
        {
            ViewBag.ReportName = rpt_name;

            JObject obj = JObject.Parse(pm_parm);
            string pm_label = (string)obj["pm_label"];
            ViewData["label"] = pm_label;

            return PartialView("_ShipByPickTicket");
        }


        public FileResult Open_ShipByPickTicket(string pm_label, string pm_pick_ticket_no)
        {
            MemoryStream example = pgmRPT_Warehouse.Get_ShipByPickTicket(pm_label, pm_pick_ticket_no);
            example.Seek(0, 0);
            Response.Headers.Add("Content-Disposition", "attachment;filename=" + pm_pick_ticket_no + ".pdf");
            return File(example, "application/pdf");
        }


        public PartialViewResult Show_ShipByOrderNo(string rpt_name, string pm_parm)
        {
            ViewBag.ReportName = rpt_name;

            JObject obj = JObject.Parse(pm_parm);
            string pm_label = (string)obj["pm_label"];
            ViewData["label"] = pm_label;

            return PartialView("_ShipByOrderNo");
        }


        public FileResult Open_ShipByOrderNo(string pm_label, string pm_lot_cd, string pm_order_no, string pm_qty)
        {
            MemoryStream example = pgmRPT_Warehouse.Get_ShipByOrderNo(pm_label, pm_lot_cd, pm_order_no, pm_qty);
            example.Seek(0, 0);
            Response.Headers.Add("Content-Disposition", "attachment;filename=" + pm_order_no + "-" + pm_lot_cd + ".pdf");
            return File(example, "application/pdf");
        }


        public PartialViewResult Show_ShippingAddress(string rpt_name, string pm_parm)
        {
            ViewBag.ReportName = rpt_name;

            JObject obj = JObject.Parse(pm_parm);
            string pm_label = (string)obj["pm_label"];
            ViewData["label"] = pm_label;

            return PartialView("_ShippingAddress");
        }


        public FileResult Open_ShippingAddress(string pm_label, string pm_id)
        {
            MemoryStream example = pgmRPT_Warehouse.Get_CustomerAddress(pm_label, pm_id);
            example.Seek(0, 0);
            Response.Headers.Add("Content-Disposition", "attachment;filename=" + pm_id + ".pdf");
            return File(example, "application/pdf");
        }


        public PartialViewResult Show_KitParts2(string rpt_name, string pm_parm)
        {
            ViewBag.ReportName = rpt_name;

            JObject obj = JObject.Parse(pm_parm);
            string pm_label = (string)obj["pm_label"];
            string pm_customer = (string)obj["pm_customer"];
            ViewData["label"] = pm_label;
            ViewData["pm_customer"] = pm_customer;

            return PartialView("_KitParts2");
        }


        public FileResult Open_KitParts2(string pm_label, string pm_customer, string pm_item_id)
        {
            MemoryStream example = pgmRPT_Warehouse.Get_KitParts2(pm_label, pm_customer, pm_item_id);
            example.Seek(0, 0);
            Response.Headers.Add("Content-Disposition", "attachment;filename=Kits.pdf");
            return File(example, "application/pdf");
        }


        public PartialViewResult Show_Components(string rpt_name, string pm_parm)
        {
            ViewBag.ReportName = rpt_name;

            JObject obj = JObject.Parse(pm_parm);
            string pm_label = (string)obj["pm_label"];
            //string pm_customer = (string)obj["pm_customer"];
            ViewData["label"] = pm_label;
            //ViewData["pm_customer"] = pm_customer;

            return PartialView("_Components");
        }


        public FileResult Open_Components(string pm_label, string pm_item_id)
        {
            MemoryStream example = pgmRPT_Warehouse.Get_Components(pm_label, pm_item_id);
            example.Seek(0, 0);
            Response.Headers.Add("Content-Disposition", "attachment;filename=Components.pdf");
            return File(example, "application/pdf");
        }


        public PartialViewResult Show_Textlabel(string rpt_name, string pm_parm)
        {
            ViewBag.ReportName = rpt_name;

            JObject obj = JObject.Parse(pm_parm);
            string pm_label = (string)obj["pm_label"];
            ViewData["label"] = pm_label;

            return PartialView("_textlabel");
        }


        public FileResult Open_Textlabel(string pm_label, string pm_text)
        {
            MemoryStream example = pgmRPT_Warehouse.Get_Textlabel(pm_label, pm_text);
            example.Seek(0, 0);
            Response.Headers.Add("Content-Disposition", "attachment;filename=TextLabel.pdf");
            return File(example, "application/pdf");
        }


        public PartialViewResult Show_BRPProduction(string rpt_name, string pm_parm)
        {
            ViewBag.ReportName = rpt_name;

            JObject obj = JObject.Parse(pm_parm);
            string pm_label = (string)obj["pm_label"];
            string pm_job_no = (string)obj["pm_job_no"];
            ViewData["label"] = pm_label;
            ViewData["job_no"] = pm_job_no;

            return PartialView("_BRPProduction");
        }


        public FileResult Open_BRPProduction(string pm_label, string pm_job_no, string pm_lot_no, string pm_qty)
        {
            MemoryStream example = pgmRPT_Warehouse.Get_BRPProduction(pm_label, pm_job_no, pm_lot_no, pm_qty);
            example.Seek(0, 0);
            //Response.Headers.Add("Content-Disposition", "attachment;filename=BRPProduction.pdf");
            Response.Headers.Add("Content-Disposition", "attachment;filename=BRPProduction_" + pm_lot_no + ".pdf");
            return File(example, "application/pdf");
        }



        public PartialViewResult Show_BoxLabel(string rpt_name, string pm_parm)
        {
            ViewBag.ReportName = rpt_name;

            JObject obj = JObject.Parse(pm_parm);
            ViewData["label"] = (string)obj["pm_label"];

            return PartialView("_BoxLabel");
        }


        public FileResult Open_BoxLabel(string pm_label, string pm_order_no, string pm_label_count)
        {
            MemoryStream example = pgmRPT_Warehouse.Get_BoxLabel(pm_label, pm_order_no, pm_label_count);
            example.Seek(0, 0);
            Response.Headers.Add("Content-Disposition", "attachment;filename=BoxLabel.pdf");
            return File(example, "application/pdf");
        }


        public PartialViewResult Show_PartBin(string rpt_name, string pm_parm)
        {
            ViewBag.ReportName = rpt_name;

            JObject obj = JObject.Parse(pm_parm);
            ViewData["label"] = (string)obj["pm_label"];

            return PartialView("_PartBin");
        }


        public FileResult Open_PartBin(string pm_label, string pm_part_no)
        {
            MemoryStream example = pgmRPT_Warehouse.Get_PartBin(pm_label, pm_part_no);
            example.Seek(0, 0);
            Response.Headers.Add("Content-Disposition", "attachment;filename=PartBin.pdf");
            return File(example, "application/pdf");
        }


        public FileResult Open_LotAttribute(string pm_label, string pm_lot_no, string pm_location_id)
        {
            MemoryStream example = pgmRPT_Warehouse.Get_LotAttribute(pm_label, pm_lot_no, pm_location_id);
            example.Seek(0, 0);
            Response.Headers.Add("Content-Disposition", "attachment;filename=LotAttribute.pdf");
            return File(example, "application/pdf");
        }


        public PartialViewResult Show_LotAttribute(string rpt_name, string pm_parm)
        {
            ViewBag.ReportName = rpt_name;

            JObject obj = JObject.Parse(pm_parm);
            string pm_label = (string)obj["pm_label"];
            string pm_location_id = (string)obj["pm_location_id"];
            ViewData["label"] = pm_label;
            ViewData["location_id"] = pm_location_id;

            return PartialView("_LotAttribute");
        }

        /*
        public PartialViewResult Show_MissingPhotoLabels(string rpt_name, string pm_parm)
        {
            ViewBag.ReportName = rpt_name;

            JObject obj = JObject.Parse(pm_parm);
            string pm_label = (string)obj["pm_label"];
            string pm_loc = (string)obj["pm_loc"];
            ViewData["label"] = pm_label;
            ViewData["loc"] = pm_loc;

            return PartialView("_MissingPhotoLabels");
        }


        public FileResult Open_MissingPhotoLabels(string pm_label, string pm_lot_cd, string pm_location_id, string pm_qty)
        {
            MemoryStream example = pgmRPT_Warehouse.Get_RecPkgBulk(pm_label, pm_location_id, pm_lot_cd, pm_qty);
            example.Seek(0, 0);
            Response.Headers.Add("Content-Disposition", "attachment;filename=" + pm_lot_cd + ".pdf");
            return File(example, "application/pdf");
        }
        */
        #endregion

        #region Label Contract Bin

        public PartialViewResult Show_ContractItem_ASML_LK(string rpt_name, string pm_parm)
        {
            ViewBag.ReportName = rpt_name;

            JObject obj = JObject.Parse(pm_parm);
            string pm_label = (string)obj["pm_label"];
            string pm_job_no = (string)obj["pm_job_no"];
            string pm_ship_to_id = (string)obj["pm_ship_to_id"];
            ViewData["label"] = pm_label;
            ViewData["pm_job_no"] = pm_job_no;
            ViewData["pm_ship_to_id"] = pm_ship_to_id;

            return PartialView("_ContractItem_ASML_LK");
        }


        public FileResult Open_ContractItem_ASML_LK(string pm_label, string pm_job_no, string pm_ship_to_id, string pm_contract_bin_id)
        {
            string pmJson = "{'job_no':'" + pm_job_no + "', 'ship_to_id':" + pm_ship_to_id + ", 'contract_bin_id':'" + pm_contract_bin_id + "'}";
            MemoryStream example = pgmRPT_Warehouse.Get_ContractItem2(pm_label, pmJson.Replace("'", "\""));
            example.Seek(0, 0);
            Response.Headers.Add("Content-Disposition", "attachment;filename=" + (string.IsNullOrWhiteSpace(pm_contract_bin_id) ? "ASML_all" : pm_contract_bin_id) + ".pdf");
            return File(example, "application/pdf");
        }

        #endregion

        #region Finland Warehouse

        public PartialViewResult Show_FinlandDCLabel(string rpt_name, string pm_parm)
        {
            ViewBag.ReportName = rpt_name;

            JObject obj = JObject.Parse(pm_parm);
            string pm_label = (string)obj["pm_label"];
            ViewData["label"] = pm_label;

            return PartialView("_FinlandDCLabel");
        }


        public FileResult Open_FinlandDCLabel(string pm_label, string pm_lot_cd, string pm_order_no, string pm_qty)
        {
            MemoryStream example = pgmRPT_Warehouse.Get_ShipByOrderNo(pm_label, pm_lot_cd, pm_order_no, pm_qty);
            example.Seek(0, 0);
            Response.Headers.Add("Content-Disposition", "attachment;filename=" + pm_order_no + "-" + pm_lot_cd + ".pdf");
            return File(example, "application/pdf");
        }

        /*
        public FileResult Open_FinlandProduction(string pm_label, string pm_job_no, string pm_item_id, string pm_lot, string pm_qty)
        {
            //MemoryStream example = pgmRPT_Warehouse.Get_FinlandProduction(pm_label, pm_job_no, pm_item_id, pm_lot, pm_qty);
            MemoryStream example = pgmRPT_Warehouse.Get_BRPProduction(pm_label, pm_job_no, pm_item_id, pm_lot, pm_qty);
            example.Seek(0, 0);

            Response.Headers.Add("Content-Disposition", "attachment;filename=FinlandProduction.pdf");
            return File(example, "application/pdf");
        }

        public PartialViewResult Show_FinlandProduction(string rpt_name, string pm_parm)
        {
            ViewBag.ReportName = rpt_name;

            JObject obj = JObject.Parse(pm_parm);
            string pm_label = (string)obj["pm_label"];
            ViewData["label"] = pm_label;

            return PartialView("_FinalProduction");
        }
        */
        #endregion

        #region KR Warehouse

        public PartialViewResult Show_ContractItem_KR_Brooks(string rpt_name, string pm_parm)
        {
            ViewBag.ReportName = rpt_name;

            JObject obj = JObject.Parse(pm_parm);
            string pm_label = (string)obj["pm_label"];
            string pm_building = (string)obj["pm_building"];
            ViewData["label"] = pm_label;
            ViewData["pm_building"] = pm_building;

            return PartialView("_ContractItem_KR_Brooks");
        }


        public FileResult Open_ContractItem_KR_Brooks(string pm_label, string pm_contract_bin_id, string pm_building)
        {
            MemoryStream example = pgmRPT_Warehouse.Get_ContractItem_Building(pm_label, pm_building, pm_contract_bin_id);
            example.Seek(0, 0);
            Response.Headers.Add("Content-Disposition", "attachment;filename=" + (string.IsNullOrWhiteSpace(pm_contract_bin_id) ? "KR_Brooks_" + pm_building : pm_building + "_" + pm_contract_bin_id) + ".pdf");
            return File(example, "application/pdf");
        }

        #endregion

        #region MX Warehouse

        public PartialViewResult Get_Bosal_Invoice(string pm_invoice_no)
        {
            List<Bosal_Invoice> theLineList = pgmRPT_Warehouse.Bosal_FetchInvoiceLine(pm_invoice_no);
            return PartialView("_Shipping_Bosal_Box", theLineList);
        }


        public PartialViewResult Show_Shipping_Bosal_Invoice(string rpt_name)
        {
            ViewBag.ReportName = rpt_name;
            return PartialView("_Shipping_Bosal_Invoice");
        }


        [ValidateAntiForgeryToken]
        public FileResult Open_Shipping_Bosal_Box(List<Bosal_Invoice> pmLineList)
        {
            MemoryStream example = pgmRPT_Warehouse.Get_Shipping_Bosal_Box("/Label/Shipping_BOSAL_box", pmLineList);
            example.Seek(0, 0);

            int invoice_no = Int32.Parse(pmLineList.FirstOrDefault().invoice_no);
            Response.Headers["Content-Disposition"] = $"attachment; filename=Bosal_{invoice_no}.pdf";

            return File(example, "application/pdf");
        }

        #endregion

        #region TW Warehouse

        public PartialViewResult Show_ContractItem_ASML(string rpt_name)
        {
            ViewBag.ReportName = rpt_name;
            return PartialView("_ContractItem_ASML");
        }


        public FileResult Open_ContractItem_ASML(string pm_contract_bin_id)
        {
            MemoryStream example = pgmRPT_Warehouse.Get_ContractItem("/Label/TW_ASML_Bin", pm_contract_bin_id);
            example.Seek(0, 0);
            Response.Headers.Add("Content-Disposition", "attachment;filename=" + (string.IsNullOrWhiteSpace(pm_contract_bin_id) ? "ASML_all" : pm_contract_bin_id) + ".pdf");
            return File(example, "application/pdf");
        }


        public PartialViewResult Show_ContractItem_ASML_TNF(string rpt_name)
        {
            ViewBag.ReportName = rpt_name;
            return PartialView("_ContractItem_ASML_TNF");
        }


        public FileResult Open_ContractItem_ASML_TNF(string pm_contract_bin_id)
        {
            MemoryStream example = pgmRPT_Warehouse.Get_ContractItem("/Label/TW_ASML_TNF_Bin", pm_contract_bin_id);
            example.Seek(0, 0);
            Response.Headers.Add("Content-Disposition", "attachment;filename=" + (string.IsNullOrWhiteSpace(pm_contract_bin_id) ? "ASML_TNF_all" : pm_contract_bin_id) + ".pdf");
            return File(example, "application/pdf");
        }


        public PartialViewResult Show_ContractItem_Edwards(string rpt_name)
        {
            ViewBag.ReportName = rpt_name;
            return PartialView("_ContractItem_Edwards");
        }


        public FileResult Open_ContractItem_Edwards(string pm_contract_bin_id)
        {
            MemoryStream example = pgmRPT_Warehouse.Get_ContractItem("/Label/TW_Edwards_Bin", pm_contract_bin_id);
            example.Seek(0, 0);
            Response.Headers.Add("Content-Disposition", "attachment;filename=" + (string.IsNullOrWhiteSpace(pm_contract_bin_id) ? "Edwards_all" : pm_contract_bin_id) + ".pdf");
            return File(example, "application/pdf");
        }


        public PartialViewResult Show_ContractItem_TW_Brooks(string rpt_name)
        {
            ViewBag.ReportName = rpt_name;
            return PartialView("_ContractItem_TW_Brooks");
        }


        public FileResult Open_ContractItem_TW_Brooks(string pm_contract_bin_id)
        {
            MemoryStream example = pgmRPT_Warehouse.Get_ContractItem("/Label/TW_Brooks_Bin", pm_contract_bin_id);
            example.Seek(0, 0);
            Response.Headers.Add("Content-Disposition", "attachment;filename=" + (string.IsNullOrWhiteSpace(pm_contract_bin_id) ? "TW_Brooks_all" : pm_contract_bin_id) + ".pdf");
            return File(example, "application/pdf");
        }

        #endregion

        #region US Warehouse

        public PartialViewResult Get_SPORTECH_PickTicket(string pm_pick_ticket_no, string pmLabelType)
        {
            List<SPORTECH_PickTicket> theLineList = pgmRPT_Warehouse.SPORTECH_FetchPickTicketLine(pm_pick_ticket_no);
            ViewData["pick_ticket_no"] = pm_pick_ticket_no;
            ViewData["pmLabelType"] = pmLabelType;
            return PartialView("_Shipping_SPORTECH_Box", theLineList);
        }


        public PartialViewResult Show_SPORTECH_PickTicket(string rpt_name, string pm_parm)
        {
            ViewBag.ReportName = rpt_name;

            JObject obj = JObject.Parse(pm_parm);
            string pmLabelType = (string)obj["pmLabelType"];
            ViewData["pmLabelType"] = pmLabelType;

            return PartialView("_Shipping_SPORTECH_PickTicket");
        }


        [ValidateAntiForgeryToken]
        public FileResult Open_Shipping_SPORTECH_Box(string pick_ticket_no, string label_type, List<SPORTECH_PickTicket> pmLineList)
        {
            MemoryStream example = new MemoryStream();
            //MemoryStream example = pgmRPT_Warehouse.Get_Shipping_SPORTECH_Box("/Label/Shipping_SPORTECH_box", pick_ticket_no, pmLineList);
            if (label_type == "Primary")
            {
                example = pgmRPT_Warehouse.Get_Shipping_SPORTECH_Box("/Label/Shipping_SPORTECH_Primary", pick_ticket_no, pmLineList);
                Response.Headers["Content-Disposition"] = $"attachment; filename=SPORTECH_Primary_{pick_ticket_no}.pdf";
            }
            else
            {
                example = pgmRPT_Warehouse.Get_Shipping_SPORTECH_Box("/Label/Shipping_SPORTECH_Master", pick_ticket_no, pmLineList);
                Response.Headers["Content-Disposition"] = $"attachment; filename=SPORTECH_Master_{pick_ticket_no}.pdf";
            }
            example.Seek(0, 0);

            return File(example, "application/pdf");
        }


        public ActionResult Show_Prop65(string pm_cluster, string pm_path, string pm_parm)
        {
            string fileName = "Prop65.pdf";
            string fullPath = Path.Combine(Server.MapPath("~/UploadedFiles"), fileName);

            using (FileStream file = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
                pgmReports.DownloadFile(pm_path, pm_parm, 0).WriteTo(file);

            return RedirectToAction("Download_GetRenderedFile", new { pm_cluster = pm_cluster, pm_file_name = fileName });
        }


        public FileResult Open_Varian(string pm_label, string pm_customer_id, string pm_item_id)
        {
            MemoryStream example = pgmRPT_Warehouse.Get_Varian(pm_label, pm_customer_id, pm_item_id);
            example.Seek(0, 0);
            Response.Headers.Add("Content-Disposition", "attachment;filename=Varian.pdf");
            return File(example, "application/pdf");
        }


        public PartialViewResult Show_Varian(string rpt_name, string pm_parm)
        {
            ViewBag.ReportName = rpt_name;

            JObject obj = JObject.Parse(pm_parm);
            string pm_label = (string)obj["pm_label"];
            string pm_customer_id = (string)obj["pm_customer_id"];
            ViewData["label"] = pm_label;
            ViewData["customer_id"] = pm_customer_id;

            return PartialView("_Varian");
        }

        #endregion

        #region VN Warehouse

        public PartialViewResult Show_GoodsNote_Receiving(string cluster, string rpt_name)
        {
            ViewBag.cluster = cluster;
            ViewBag.ReportName = rpt_name;
            return PartialView("_GoodsNote_Receiving");
        }


        public PartialViewResult Show_GoodsNote_Shipping(string cluster, string rpt_name)
        {
            ViewBag.cluster = cluster;
            ViewBag.ReportName = rpt_name;

            List<SelectListItem> TypeList = new List<SelectListItem>();
            TypeList.Add(new SelectListItem { Text = "Goods Note", Value = "GoodsNote" });
            TypeList.Add(new SelectListItem { Text = "Delivery Note", Value = "DeliveryNote" });
            ViewData["List_Type"] = TypeList;

            return PartialView("_GoodsNote_Shipping");
        }


        public ActionResult GoDownLoad_GoodsNote_Receiving(string pm_cluster, string receipt_date)
        {
            SP_Return ResultModel = pgmRPT_Warehouse.RPT_GoodsNote_Receiving(receipt_date);

            if (ResultModel.r == 1)
                return RedirectToAction("Download_GetRenderedFile", new { cluster = pm_cluster, pm_file_name = ResultModel.JsonData });
            else
            {
                TempData["message"] = ResultModel.msg;
                return RedirectToAction("AlertingMessage", "Home");
            }
        }


        public ActionResult GoDownLoad_GoodsNote_Shipping(string pm_cluster, string pmInvoices, string pmType)
        {
            SP_Return ResultModel = pgmRPT_Warehouse.RPT_GoodsNote_Shipping(pmInvoices, pmType);

            if (ResultModel.r == 1)
                return RedirectToAction("Download_GetRenderedFile", new { cluster = pm_cluster, pm_file_name = ResultModel.JsonData });
            else
            {
                TempData["message"] = ResultModel.msg;
                return RedirectToAction("AlertingMessage", "Home");
            }
        }

        #endregion

        #region TW QC

        public PartialViewResult Show_QC_Sample(string rpt_name, string pm_parm)
        {
            ViewBag.ReportName = rpt_name;

            JObject obj = JObject.Parse(pm_parm);
            string pm_label = (string)obj["pm_label"];
            ViewData["label"] = pm_label;

            return PartialView("_QC_Sample");
        }


        public FileResult Open_QC_Sample(string pm_label, string po_no, string line_no, string unit_wgt, string vendor_lot)
        {
            MemoryStream example = pgmRPT_QC.Get_QC_Sample(pm_label, po_no, line_no, unit_wgt, vendor_lot);
            example.Seek(0, 0);
            Response.Headers.Add("Content-Disposition", "attachment;filename=Sample_" + po_no + "#" + line_no + ".pdf");
            return File(example, "application/pdf");
        }


        public PartialViewResult Show_QC_SampleProcess(string rpt_name, string pm_parm)
        {
            ViewBag.ReportName = rpt_name;

            JObject obj = JObject.Parse(pm_parm);
            string pm_label = (string)obj["pm_label"];
            ViewData["label"] = pm_label;

            return PartialView("_QC_Sample_Process");
        }


        public FileResult Open_QC_SampleProcess(string pm_label, string po_no, string line_no)
        {
            MemoryStream example = pgmRPT_QC.Get_QC_SampleProcess(pm_label, po_no, line_no);
            example.Seek(0, 0);
            Response.Headers.Add("Content-Disposition", "attachment;filename=SampleProcess_" + po_no + "#" + line_no + ".pdf");
            return File(example, "application/pdf");
        }

        #endregion

        #region TW HR

        public FileStreamResult Show_DayOffList()
        {
            string fileName = "每月請假明細_" + DateTime.Now.ToString("yyyyMMdd") + ".xlsx";
            MemoryStream theSheet = pgmAttendance.GetDayOffList();
            return File(theSheet, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        #endregion

    }
}