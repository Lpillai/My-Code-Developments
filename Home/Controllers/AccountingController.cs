using Accounting;
using Newtonsoft.Json;
using RuntimeVariables;
using SharedScrpits;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace Home.Controllers
{
    [SessionExpireFilter]
    public class AccountingController : Controller
    {
        Common_Share shareCommon = new Common_Share();
        FileActions_Share shareFileActions = new FileActions_Share();
        Get_Share shareGet = new Get_Share();
        Convertor_Share shareConvertor = new Convertor_Share();
        Accounting_Fileter filterAccounting = new Accounting_Fileter();
        CashLockbox_Program pgmCashLockbox = new CashLockbox_Program();
        AR_Program pgmAR = new AR_Program();
        AP_Program pgmAP = new AP_Program();
        LocalInformation_Program pgmLocalInformation = new LocalInformation_Program();


        public FileStreamResult Get_attachment(int pm_attach_uid)
        {
            MemoryStream workStream = null;
            AccountingAttachment theAttach = GlobalVariables.MySession.List_AccountingAttachment.Find(f => f.attach_uid == pm_attach_uid);
            string theExt = "";

            if (theAttach != null)
            {
                theExt = theAttach.attach_ext.ToLower();
                workStream = new MemoryStream((byte[])theAttach.attach_file);
                workStream.Seek(0, 0);
                Response.Headers.Add("Content-Disposition", "attachment;filename=" + theAttach.attach_name + theAttach.attach_ext);
            }

            return File(workStream, "application/force-download");
        }


        public ActionResult zip_AccountingAttachments(string pm_accounting_type, int pm_accounting_uid)
        {
            string zipName = pm_accounting_type + "-" + pm_accounting_uid.ToString() + ".zip";
            List<AccountingAttachment> theAttachments = new List<AccountingAttachment>();

            if (GlobalVariables.MySession.List_AccountingAttachment == null || 
                GlobalVariables.MySession.List_AccountingAttachment.Where(w => w.accounting_type == pm_accounting_type && w.accounting_uid == pm_accounting_uid && w.attach_uid != 0).Count() == 0)
                theAttachments = pgmAR.Fetch_Accounting_Attachment(pm_accounting_type, pm_accounting_uid).FindAll(f => f.accounting_type == pm_accounting_type && f.accounting_uid == pm_accounting_uid && f.attach_uid != 0);
            else 
                theAttachments = GlobalVariables.MySession.List_AccountingAttachment.FindAll(f => f.accounting_type == pm_accounting_type && f.accounting_uid == pm_accounting_uid && f.attach_uid != 0);

            if (theAttachments == null || theAttachments.Count() == 0)
                return Content("<script type='text/javascript'>alert('No any attachment uploaded yet.');</script>");
            else
            {
                using (MemoryStream workStream = new MemoryStream())
                {
                    using (var zip = new ZipArchive(workStream, ZipArchiveMode.Create, true))
                    {
                        foreach (AccountingAttachment oneFile in theAttachments)
                        {
                            var entry = zip.CreateEntry(oneFile.attach_name + oneFile.attach_ext);
                            using (MemoryStream fs = new MemoryStream((byte[])oneFile.attach_file))
                            {
                                fs.Seek(0, 0);
                                using (var entryStream = entry.Open())
                                {
                                    fs.CopyTo(entryStream);
                                }
                            }
                        }
                    }
                    return File(workStream.ToArray(), "application/zip", zipName);
                }
            }
        }


        public PartialViewResult show_AccountingAttachments(string pm_source, int pm_location_id, string pm_accounting_type, int pm_accounting_uid)
        {
            int menu_uid = -1000;
            string attach_tag = "";

            switch (pm_source)
            {
                case "AR":
                    menu_uid = 62;
                    attach_tag = "AttachmentTag_AR";
                    break;
                case "AP":
                    menu_uid = 121;
                    attach_tag = "AttachmentTag_AP";
                    break;
                default:
                    break;
            }

            ViewBag.authorized = shareCommon.isAuthorized(menu_uid);
            ViewData["source"] = pm_source;
            ViewData["location_id"] = pm_location_id;
            ViewData["accounting_type"] = pm_accounting_type;
            ViewData["accounting_uid"] = pm_accounting_uid;
            ViewData["attach_tag"] = shareConvertor.ToDropDownOptions(shareGet.FetchCodeList(attach_tag, 2).Where(w => !string.IsNullOrWhiteSpace(w.Code)).ToList());

            return PartialView("_AccountingAttachments", pgmAR.Fetch_Accounting_Attachment(pm_accounting_type, pm_accounting_uid));
        }


        [ValidateAntiForgeryToken]
        public ActionResult GoUpdateAttachment(List<AccountingAttachment> pmAR, string pm_source, int pm_location_id, string pm_accounting_type, int pm_accounting_uid)
        {
            if (pmAR.Where(w => w.z_status == "A" && w.attachedFile != null && w.tags == null).Count() > 0)
                TempData["message"] = "Tag is required.";
            else
                TempData["message"] = pgmAR.Update_Accounting_Attachment(pmAR.Where(w => w.z_status == "A" && w.attachedFile != null).ToList(), pm_accounting_type, pm_accounting_uid);

            string action = "";
            switch (pm_source)
            {
                case "AR":
                    action = "AccountsReceivable";
                    break;
                case "AP":
                    action = "AccountsPayable";
                    break;
                default:
                    break;
            }

            return RedirectToAction(action, "Accounting", new { pm_location_id = pm_location_id, pm_accounting_uid = pm_accounting_uid });
        }


        public ActionResult GoDeleteAttachment(string pm_source, int pm_location_id, string pm_attach_file, int pm_attach_uid, int pm_accounting_uid)
        {
            List<AccountingAttachment> theAR = new List<AccountingAttachment>();
            theAR.Add(new AccountingAttachment { attach_uid = pm_attach_uid, attach_name = pm_attach_file, z_status = "D" });
            TempData["message"] = pgmAR.Update_Accounting_Attachment(theAR, "", 0);

            string action = "";
            switch (pm_source)
            {
                case "AR":
                    action = "AccountsReceivable";
                    break;
                case "AP":
                    action = "AccountsPayable";
                    break;
                default:
                    break;
            }
                
            return RedirectToAction(action, "Accounting", new { pm_location_id = pm_location_id, pm_accounting_uid = pm_accounting_uid });
        }

        #region Local Information

        public JsonResult GetP21AddressInformation(int address_id) //It will be fired from Jquery ajax call
        {
            Local_P21_ID theID = pgmLocalInformation.FetchP21AddressInformation(address_id);
            return Json(theID.address_name, JsonRequestBehavior.AllowGet);
        }


        public PartialViewResult showLocalInformation_Address(int pm_address_id, string pm_address_name, int pm_address_uid = 0)
        {
            ViewBag.authorized = shareCommon.isAuthorized(125);
            ViewData["address_id"] = pm_address_id;
            ViewData["address_name"] = pm_address_name;
            ViewData["pm_isNew"] = false;

            Local_Address theAddress = new Local_Address();
            if (pm_address_uid == 0)
            {
                ViewData["pm_isNew"] = true;
                theAddress = new Local_Address() { p21_id = pm_address_id };
            }
            else
            {
                GlobalVariables.MySession.new_P21_ID = null;
                theAddress = GlobalVariables.MySession.theLocalInformation.theAddress.Find(f => f.address_uid == pm_address_uid);
            }

            ViewData["Language"] = shareConvertor.ToDropDownOptions(shareGet.FetchCodeList("LocalInformationLan", 1).FindAll(f => f.Code != "0"));
            ViewData["AddressType"] = shareConvertor.ToDropDownOptions(shareGet.FetchCodeList("LocalInformationType", 1).FindAll(f => f.Code != "0" && Int16.Parse(f.Code) > 100));

            return PartialView("_LocalInformation_Address", theAddress);
        }


        public PartialViewResult showLocalInformation_Contact(int pm_address_id, string pm_address_name, int pm_contact_uid = 0)
        {
            ViewBag.authorized = shareCommon.isAuthorized(125);
            ViewData["address_id"] = pm_address_id;
            ViewData["address_name"] = pm_address_name;
            ViewData["pm_isNew"] = false;

            Local_Contact theContact = new Local_Contact();
            if (pm_contact_uid == 0)
            {
                ViewData["pm_isNew"] = true;
                theContact = new Local_Contact() { p21_id = pm_address_id };
            }
            else
            {
                GlobalVariables.MySession.new_P21_ID = null;
                theContact = GlobalVariables.MySession.theLocalInformation.theContact.Find(f => f.contact_uid == pm_contact_uid);
            }

            ViewData["Language"] = shareConvertor.ToDropDownOptions(shareGet.FetchCodeList("LocalInformationLan", 1).FindAll(f => f.Code != "0"));
            ViewData["ContactType"] = shareConvertor.ToDropDownOptions(shareGet.FetchCodeList("LocalInformationType", 1).FindAll(f => f.Code != "0" && Int16.Parse(f.Code) < 100));

            return PartialView("_LocalInformation_Contact", theContact);
        }


        public PartialViewResult showLocalInformation_Details(int pm_address_id, string pm_address_name = "", bool pm_isNew = false)
        {
            ViewBag.authorized = shareCommon.isAuthorized(125);
            ViewData["pm_isNew"] = pm_isNew;
            ViewData["address_id"] = pm_address_id;
            ViewData["address_name"] = pm_address_name;

            Local_Information theInfo = new Local_Information();
            if (pm_isNew)
            {
                theInfo.theAddress = new List<Local_Address>();
                theInfo.theContact = new List<Local_Contact>();
            }
            else
            {
                theInfo = pgmLocalInformation.GetLocalInformation(pm_address_id);
                theInfo.address_name = pm_address_name;
            }

            return PartialView("_LocalInformation_Detail", theInfo);
        }


        public ActionResult LocalInformation(int pm_address_id = 0, string pm_address_name = "")
        {
            //string loc_name = "";
            ViewBag.PageInfo = shareCommon.GetPageInfo(125);
            ViewBag.Dev = pgmLocalInformation.devMode;
            ViewData["address_id"] = pm_address_id;
            ViewData["address_name"] = pm_address_name;

            return View(pgmLocalInformation.FetchP21List());
        }


        [ValidateAntiForgeryToken]
        public ActionResult GoUpdateLocalInformation_Address(Local_Address pm_address, int pm_address_id, string pm_address_name = "")
        {
            if (pm_address.address_uid == 0)
            {
                pm_address.p21_id = (GlobalVariables.MySession.new_P21_ID == null ? pm_address_id : GlobalVariables.MySession.new_P21_ID.address_id);
                pm_address_id = pm_address.p21_id;
                pm_address_name = (GlobalVariables.MySession.new_P21_ID == null ? pm_address_name : GlobalVariables.MySession.new_P21_ID.address_name);
            }

            string pmJSON = "{'pm_usr':'" + GlobalVariables.MySession.Account + "', 'theInformationType':'Address', 'theAddress':" + JsonConvert.SerializeObject(pm_address) + "}";
            pmJSON = pmJSON.Replace("'", "\"");

            SP_Return theResult = pgmLocalInformation.UpdateLocalInformation(pmJSON);
            TempData["message"] = theResult.msg;
            return RedirectToAction("LocalInformation", "Accounting", new { pm_address_id = pm_address_id, pm_address_name = pm_address_name });
        }


        [ValidateAntiForgeryToken]
        public ActionResult GoUpdateLocalInformation_Contact(Local_Contact pm_contact, int pm_address_id, string pm_address_name = "")
        {
            if (pm_contact.contact_uid == 0)
            {
                pm_contact.p21_id = (GlobalVariables.MySession.new_P21_ID == null ? pm_address_id : GlobalVariables.MySession.new_P21_ID.address_id);
                pm_address_id = pm_contact.p21_id;
                pm_address_name = (GlobalVariables.MySession.new_P21_ID == null ? pm_address_name : GlobalVariables.MySession.new_P21_ID.address_name);
            }

            string pmJSON = "{'pm_usr':'" + GlobalVariables.MySession.Account + "', 'theInformationType':'Contact', 'theContact':" + JsonConvert.SerializeObject(pm_contact) + "}";
            pmJSON = pmJSON.Replace("'", "\"");

            SP_Return theResult = pgmLocalInformation.UpdateLocalInformation(pmJSON);
            TempData["message"] = theResult.msg;
            return RedirectToAction("LocalInformation", "Accounting", new { pm_address_id = pm_address_id, pm_address_name = pm_address_name });
        }

        #endregion

        #region A/R

        public PartialViewResult showAR_Details(string pm_invoice_no)
        {
            ViewData["invoice_no"] = pm_invoice_no;
            return PartialView("_AR_Detail", pgmAR.FetchAR_Details(pm_invoice_no));
        }


        //public ActionResult AccountsReceivable(int pm_location_id, string pm_taker_group)
        public ActionResult AccountsReceivable(int pm_location_id, int pm_accounting_uid = 0)
        {
            //string loc_name = "";
            ViewBag.PageInfo = shareCommon.GetPageInfo(62);
            ViewBag.Dev = pgmAR.devMode;
            ViewBag.authorized = shareCommon.isAuthorized(62);
            ViewData["pm_location_id"] = pm_location_id;
            //ViewData["pm_taker_group"] = pm_taker_group;
            ViewData["pm_accounting_uid"] = pm_accounting_uid;

            List<CodeValue> theProgress = shareGet.FetchCodeList("AR_Progress").FindAll(w => w.Code != "0");
            ViewData["progress"] = theProgress;
            ViewData["progress_count"] = Math.Round((double)(100 / theProgress.Count()), 1, MidpointRounding.AwayFromZero);
            /*
            switch (pm_location_id)
            {
                case 13:
                    loc_name = "TW";
                    break;
                default:
                    break;
            }
            */
            List<AR_Item> theAR = (List<AR_Item>)TempData["pm_AR"];
            if (theAR == null)
                //theAR = pgmAR.GetAccountsReceivableList(pm_location_id, pm_taker_group);
                theAR = pgmAR.GetAccountsReceivableList(pm_location_id);

            return View(theAR);
        }


        [ValidateAntiForgeryToken]
        //public ActionResult goImportData_ASMLTW(HttpPostedFileBase pm_file, int pm_location_id, string pm_taker_group)
        public ActionResult goImportData_ASMLTW(HttpPostedFileBase pm_file, int pm_location_id)
        {
            TempData["message"] = null;
            SP_Return theResult = new SP_Return();
            //string theFileName = "ASMLTW" + Path.GetExtension(pm_file.FileName);
            string thePath = Path.Combine(shareFileActions.GetUploadFolder(), Path.GetFileName(pm_file.FileName));

            theResult.r = 1;
            if (pm_file.ContentLength > 0)
            {
                try
                {
                    pm_file.SaveAs(thePath);
                    theResult = pgmAR.ImportData_ASMLTW(thePath);
                }
                catch (Exception ex)
                {
                    theResult.r = 0;
                    theResult.msg = ex.ToString();
                }

                TempData["message"] = theResult.msg;
            }
            else
            {
                TempData["message"] = "No content was found in the file.";
            }

            //return RedirectToAction("AccountsReceivable", "Accounting", new { pm_location_id = pm_location_id, pm_taker_group = pm_taker_group });
            return RedirectToAction("AccountsReceivable", "Accounting", new { pm_location_id = pm_location_id });
        }


        [ValidateAntiForgeryToken]
        //public ActionResult GoUpdateAR(List<AR_Item> pmAR, int pm_location_id, string pm_taker_group)
        public ActionResult GoUpdateAR(List<AR_Item> pmAR, int pm_location_id)
        {
            SP_Return theResult = pgmAR.UpdateAccountsReceivable(pmAR);

            TempData["message"] = theResult.msg;

            if (theResult.r != 1)
                TempData["pm_AR"] = pmAR;

            //return RedirectToAction("AccountsReceivable", "Accounting", new { pm_location_id = pm_location_id, pm_taker_group = pm_taker_group });
            return RedirectToAction("AccountsReceivable", "Accounting", new { pm_location_id = pm_location_id });
        }


        public PartialViewResult showAR_Archive(string pm_invoice_no = "", int? pm_customer_id = null, string pm_gui_no = "")
        {
            return PartialView("_AR_Archive", pgmAR.FetchAR_Archive(pm_invoice_no, pm_customer_id, pm_gui_no));
        }


        public ActionResult AR_Archive()
        {
            ViewBag.PageInfo = shareCommon.GetPageInfo(63);
            ViewBag.Dev = pgmAR.devMode;
            return View();
        }

        #endregion

        #region A/P

        public PartialViewResult showAP_Details(int pm_receipt_number)
        {
            ViewData["receipt_number"] = pm_receipt_number;
            return PartialView("_AP_Detail", pgmAP.FetchAP_Details(pm_receipt_number));
        }


        public ActionResult AccountsPayable(int pm_location_id)
        {
            ViewBag.PageInfo = shareCommon.GetPageInfo(121);
            ViewBag.Dev = pgmAP.devMode;
            ViewBag.authorized = shareCommon.isAuthorized(121);
            ViewData["pm_location_id"] = pm_location_id;

            List<AP_Item> theAP = (List<AP_Item>)TempData["pm_AP"];
            if (theAP == null)
                theAP = pgmAP.GetAccountsPayableList(pm_location_id);

            return View(theAP);
        }


        [ValidateAntiForgeryToken]
        public ActionResult GoUpdateAP(List<AP_Item> pmAP, int pm_location_id)
        {
            SP_Return theResult = pgmAP.UpdateAccountsPayable(pmAP);

            TempData["message"] = theResult.msg;

            if (theResult.r != 1)
                TempData["pm_AP"] = pmAP;

            return RedirectToAction("AccountsPayable", "Accounting", new { pm_location_id = pm_location_id });
        }

        #endregion

        #region Filter

        //public PartialViewResult FilterAR(int pm_location_id, string pm_taker_group)
        public PartialViewResult FilterAR(int pm_location_id)
        {
            List<string> theParameters = new List<string>();
            theParameters.Add(pm_location_id.ToString());
            //theParameters.Add(pm_taker_group);

            ViewData["pmParameters"] = String.Join(",", theParameters.ToArray());
            ViewData["goController"] = "Accounting";
            ViewData["goAction"] = "actFilterAR";
            return PartialView("../Home/_Filters", filterAccounting.AR_getFilters());
        }

        /*
        [HttpPost]
        [MultipleButton(Name = "FilterAction", Argument = "Reset")]
        public ActionResult goReset(string pmParameters)
        {
            List<string> theParameters = pmParameters.Split(',').ToList();
            filterAccounting.PostFilter(null);
            return RedirectToAction("AccountsReceivable", new { pm_location_id = theParameters[0], pm_taker_group = theParameters[1] });
        }


        [HttpPost]
        [MultipleButton(Name = "FilterAction", Argument = "Filter")]
        public ActionResult goFilter(List<Filters> pmFilterList, string pmParameters)
        {
            List<string> theParameters = pmParameters.Split(',').ToList();
            filterAccounting.PostFilter(pmFilterList);
            return RedirectToAction("AccountsReceivable", new { pm_location_id = theParameters[0], pm_taker_group = theParameters[1] });
        }
        */

        public ActionResult actFilterAR(List<Filters> pmFilterList, string pmParameters, string filter_action)
        {
            List<string> theParameters = pmParameters.Split(',').ToList();

            if (filter_action == "Filter")
                filterAccounting.AR_PostFilter(pmFilterList);
            else
                filterAccounting.AR_PostFilter(null);

            //return RedirectToAction("AccountsReceivable", new { pm_location_id = theParameters[0], pm_taker_group = theParameters[1] });
            return RedirectToAction("AccountsReceivable", new { pm_location_id = theParameters[0] });
        }


        public PartialViewResult FilterAP(int pm_location_id)
        {
            List<string> theParameters = new List<string>();
            theParameters.Add(pm_location_id.ToString());

            ViewData["pmParameters"] = String.Join(",", theParameters.ToArray());
            ViewData["goController"] = "Accounting";
            ViewData["goAction"] = "actFilterAP";
            return PartialView("../Home/_Filters", filterAccounting.AP_getFilters());
        }


        public ActionResult actFilterAP(List<Filters> pmFilterList, string pmParameters, string filter_action)
        {
            List<string> theParameters = pmParameters.Split(',').ToList();

            if (filter_action == "Filter")
                filterAccounting.AP_PostFilter(pmFilterList);
            else
                filterAccounting.AP_PostFilter(null);

            return RedirectToAction("AccountsPayable", new { pm_location_id = theParameters[0] });
        }

        #endregion

        #region Cash Lockbox

        public ActionResult CashLockboxImportProcess()
        {
            ViewBag.PageInfo = shareCommon.GetPageInfo(37);
            ViewBag.Dev = pgmCashLockbox.devMode;
            ViewBag.authorized = shareCommon.isAuthorized(37);
            return View();
        }


        public ActionResult goImportCashReceipts(HttpPostedFileBase pm_file, string assigned_date)
        {
            TempData["message"] = null;
            SP_Return theResult = new SP_Return();
            string theFileName = assigned_date.Replace("-", "") + Path.GetExtension(pm_file.FileName);
            string thePath = Path.Combine(shareFileActions.GetUploadFolder(), Path.GetFileName(pm_file.FileName));

            theResult.r = 1;
            if (pm_file.ContentLength > 0)
            {
                try
                {
                    pm_file.SaveAs(thePath);
                }
                catch (Exception ex)
                {
                    theResult.r = 0;
                    theResult.msg = ex.ToString();
                }

                if (theResult.r == 0)
                    theResult.msg = thePath + "\r\n\r\n" + theResult.msg;
                else
                    theResult = pgmCashLockbox.CashReceipts_ImportSheet(thePath);

                if (theResult.r == 0)
                    theResult.msg = thePath + "\r\n\r\n" + theResult.msg;
                else
                    theResult = pgmCashLockbox.CashReceipts_ProcessWrapper(assigned_date);

                if (theResult.r == 1)
                    TempData["message"] = "Files are been producing in background. You will receive an mail notification once the process was done.";
                else
                    TempData["message"] = theResult.msg;
            }
            else
            {
                TempData["message"] = "No content was found in the file.";
            }

            return RedirectToAction("CashLockboxImportProcess");
        }

        #endregion

    }
}