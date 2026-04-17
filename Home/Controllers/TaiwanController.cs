using RuntimeVariables;
using SharedScrpits;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Taiwan;


namespace Home.Controllers
{
    [SessionExpireFilter]
    public class TaiwanController : Controller
    {
        Common_Share shareCommon = new Common_Share();
        Get_Share shareGet = new Get_Share();
        Convertor_Share shareConvertor = new Convertor_Share();
        FileActions_Share shareFileActions = new FileActions_Share();
        Employee_Program pgmEmployee = new Employee_Program();
        Attendance_Program pgmAttendance = new Attendance_Program();


        #region Emplyee

        public PartialViewResult ShowEmployeeDetails(string pm_account_id, bool pm_isNew = false)
        {
            ViewBag.authorized = shareCommon.isAuthorized(68);
            ViewData["isNew"] = pm_isNew;
            ViewData["isHR"] = pgmEmployee.isHR();
            tw_employee theEmployee = new tw_employee();

            if (pm_isNew)
                theEmployee.z_status = "N";
            else
                theEmployee = pgmEmployee.Get_TW_employee(pm_account_id);

            var twShift = shareConvertor.ToDropDownOptions(shareGet.FetchCodeList("TW_Shift", 4));
            //SelectList theShift = new SelectList(twShift, "Value", "Text", theEmployee.shift);
            SelectList theShift = new SelectList(twShift, "Value", "Text");
            ViewData["shift"] = theShift;

            return PartialView("_EmployeeDetails", theEmployee);
        }


        public ActionResult EmployeeManagement(string pm_account_id = "")
        {
            ViewBag.PageInfo = shareCommon.GetPageInfo(68);
            ViewBag.Dev = pgmEmployee.devMode;
            ViewData["pm_account_id"] = pm_account_id;
            return View(pgmEmployee.FetchList_TW_employee());
        }


        [ValidateAntiForgeryToken]
        public ActionResult Go_SaveEmployeeDetails(tw_employee pm_emp)
        {
            if (pm_emp.z_status == "N")
                pm_emp.z_status = "";
            SP_Return result = pgmEmployee.Update_TW_employee(pm_emp);
            TempData["message"] = result.msg;

            if (pm_emp.z_status == "D")
                return RedirectToAction("EmployeeManagement");
            else
                return RedirectToAction("EmployeeManagement", new { pm_account_id = pm_emp.account_id });
        }

        #endregion

        #region Day off

        public FileStreamResult Get_attachment(int pm_attach_uid)
        {
            MemoryStream workStream = null;
            tw_LeaveAttachment theAttach = pgmAttendance.Get_TW_Attachment(pm_attach_uid);
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


        public ActionResult Go_DeleteAttachment(int pm_attach_uid)
        {
            SP_Return result = pgmAttendance.Delete_TW_LeaveTaking_Attachment(pm_attach_uid);
            TempData["controller_msg"] = result.msg;
            return RedirectToAction("LeaveTakingManagement");
        }


        [ValidateAntiForgeryToken]
        public ActionResult Go_UploadAttach(HttpPostedFileBase pmFile, int pm_apply_uid)
        {
            string thePath = "";

            if (pmFile != null && pmFile.ContentLength > 0)
            {
                //if (!Directory.Exists(Server.MapPath("~/UploadedFiles")))
                if (!Directory.Exists(shareFileActions.GetUploadFolder()))
                    TempData["message"] = "Can't find target folder for uploading. Please contact IT team to fix the issue.";
                else
                {
                    //thePath = Path.Combine(Server.MapPath("~/UploadedFiles"), Path.GetFileName(pmFile.FileName));
                    thePath = Path.Combine(shareFileActions.GetUploadFolder(), Path.GetFileName(pmFile.FileName));
                    pmFile.SaveAs(thePath);
                }
            }

            TempData["controller_msg"] = pgmAttendance.Upload_TW_LeaveTaking_Attachment(thePath, pm_apply_uid);

            return RedirectToAction("LeaveTakingManagement");
        }


        public JsonResult GetWorkingHours(string datetime_s, string  datetime_e) //It will be fired from Jquery ajax call
        {
            double returnHours = shareGet.GetWorkingHourDifference(DateTime.Parse(datetime_s), DateTime.Parse(datetime_e));
            return Json(returnHours, JsonRequestBehavior.AllowGet);
        }


        public PartialViewResult ShowLeaveTakingDetails(string pm_source, int pm_apply_uid, bool pm_isNew = false)
        {
            ViewBag.authorized = shareCommon.isAuthorized(70);
            ViewData["source"] = pm_source;
            ViewData["isNew"] = pm_isNew;
            tw_day_apply theApply = new tw_day_apply();

            if (!pm_isNew)
                theApply = pgmAttendance.Get_TW_OneLeave(pm_apply_uid);
            else
            {
                theApply.day_start = DateTime.UtcNow.AddHours(8);
                if (theApply.day_start.Minute > 0 && theApply.day_start.Minute < 30)
                    theApply.day_start = theApply.day_start.AddMinutes(30 - theApply.day_start.Minute);
                if (theApply.day_start.Minute > 30)
                    theApply.day_start = theApply.day_start.AddMinutes(60 - theApply.day_start.Minute);
                theApply.day_end = theApply.day_start;
            }

            var hourOptions = shareGet.GetList_CardSystemHours();
            ViewData["TimeList_start"] = new SelectList(hourOptions, "Text", "Value", theApply.day_start.Hour.ToString("00") + ":" + theApply.day_start.Minute.ToString("00"));
            ViewData["TimeList_end"] = new SelectList(hourOptions, "Text", "Value", theApply.day_end.Hour.ToString("00") + ":" + theApply.day_end.Minute.ToString("00"));

            var twLeaves = pgmAttendance.FetchList_TW_LeaveSettings();
            SelectList theLeave = new SelectList(twLeaves, "day_id", "day_name", theApply.day_id);
            ViewData["twLeaves"] = theLeave;

            return PartialView("_LeaveTakingDetails", theApply);
        }


        public ActionResult LeaveTakingManagement(string pm_account_id = "")
        {
            ViewBag.PageInfo = shareCommon.GetPageInfo(70);
            ViewBag.Dev = pgmAttendance.devMode;
            ViewBag.authorized = shareCommon.isAuthorized(70);

            if (pm_account_id == "")
                pm_account_id = GlobalVariables.MySession.Account;
            ViewData["pm_account_id"] = pm_account_id;
            ViewData["session_account"] = GlobalVariables.MySession.Account;
            TempData["message"] = TempData["controller_msg"];

            return View(pgmAttendance.FetchList_TW_LeaveTaking(pm_account_id));
        }


        [ValidateAntiForgeryToken]
        //public ActionResult Go_SaveLeaveTaking(string pm_source, tw_day_apply pm_apply, string day_start_hour, string day_end_hour)
        public ActionResult Go_SaveLeaveTaking(string pm_source, tw_day_apply pm_apply)
        {
            string msg = "";
            string thePath = "";

            //pm_apply.day_start = pm_apply.day_start.Add(TimeSpan.Parse(day_start_hour));
            //pm_apply.day_end = pm_apply.day_end.Add(TimeSpan.Parse(day_end_hour));
            SP_Return result = pgmAttendance.Update_TW_LeaveTaking(pm_apply);
            msg = result.msg;

            if (pm_apply.attachedFile != null && pm_apply.attachedFile.ContentLength > 0)
            {
                if (!Directory.Exists(shareFileActions.GetUploadFolder()))
                    msg += "Can't find target folder for uploading. Please contact IT team to fix the issue.";
                else
                {
                    thePath = Path.Combine(shareFileActions.GetUploadFolder(), Path.GetFileName(pm_apply.attachedFile.FileName));
                    pm_apply.attachedFile.SaveAs(thePath);
                }

                if (result.r == 1)
                    msg += "\r\n" + pgmAttendance.Upload_TW_LeaveTaking_Attachment(thePath, Int16.Parse(result.JsonData));
            }

            TempData["controller_msg"] = msg;
            if (pm_source == "_HR_ManagingPageDetails")
                return RedirectToAction("HR_ManagingPage", new { pm_account_id = pm_apply.account_id });
            else
                return RedirectToAction("LeaveTakingManagement");
        }


        [ValidateAntiForgeryToken]
        public ActionResult Go_AddCardStamp(DateTime pmStamp)
        {
            SP_Return result = pgmAttendance.AddCardStamp(pmStamp);
            TempData["controller_msg"] = result.msg;
            return RedirectToAction("LeaveTakingManagement");
        }


        public PartialViewResult ShowAvailableDetails(string pm_account_id, int pm_day_year, int pm_day_id, bool pm_isNew = false)
        {
            ViewBag.authorized = shareCommon.isAuthorized(73);
            ViewData["isNew"] = pm_isNew;
            tw_day_available theAvailable = new tw_day_available();

            if (!pm_isNew)
                theAvailable = pgmAttendance.Get_TW_OneAvailable(pm_account_id, pm_day_year, pm_day_id);
            else
            {
                theAvailable.account_id = pm_account_id;
                theAvailable.available_start = DateTime.Now;
                theAvailable.available_end = DateTime.Now;
            }

            var twLeaves = pgmAttendance.FetchList_TW_LeaveSettings();
            SelectList theLeave = new SelectList(twLeaves, "day_id", "day_name", theAvailable.day_id);
            ViewData["twLeaves"] = theLeave;

            return PartialView("_AvailableDetails", theAvailable);
        }


        public PartialViewResult Show_HR_ManagingPageDetails(string pm_account_id)
        {
            ViewBag.authorized = shareCommon.isAuthorized(73);
            ViewData["pm_account_id"] = pm_account_id;
            return PartialView("_HR_ManagingPageDetails", pgmAttendance.FetchList_TW_LeaveTaking(pm_account_id));
        }


        public ActionResult HR_ManagingPage(string pm_account_id = "")
        {
            ViewBag.PageInfo = shareCommon.GetPageInfo(73);
            ViewBag.Dev = pgmAttendance.devMode;
            TempData["message"] = TempData["controller_msg"];

            if (pm_account_id == "")
                pm_account_id = GlobalVariables.MySession.Account;
            var twEmps = pgmEmployee.FetchList_TW_employee();
            SelectList theEmps = new SelectList(twEmps, "account_id", "full_name", pm_account_id);
            ViewData["List_TW_Employee"] = theEmps;

            return View();
        }


        [ValidateAntiForgeryToken]
        public ActionResult Go_SaveAvailable(tw_day_available pm_available)
        {
            SP_Return result = pgmAttendance.Update_TW_Available(pm_available);
            TempData["controller_msg"] = result.msg;

            return RedirectToAction("HR_ManagingPage", new { pm_account_id = pm_available.account_id });
        }


        public ActionResult HR_LeaveApplicationPage()
        {
            ViewBag.PageInfo = shareCommon.GetPageInfo(95);
            ViewBag.Dev = pgmAttendance.devMode;
            return View(pgmAttendance.FetchList_TW_HR_LeaveApply());
        }

        #endregion

        #region Calendar

        public PartialViewResult ShowCalendarDetails()
        {
            ViewBag.authorized = shareCommon.isAuthorized(92);
            tw_Calendar theCalendar = new tw_Calendar();

            return PartialView("_CalendarDetails", theCalendar);
        }


        public ActionResult Calendar()
        {
            ViewBag.PageInfo = shareCommon.GetPageInfo(92);
            ViewBag.Dev = pgmAttendance.devMode;
            ViewBag.authorized = shareCommon.isAuthorized(92);
            ViewData["AssignedDate"] = pgmAttendance.Get_TW_AssignedDate();
            return View(pgmAttendance.FetchList_TW_Calendar());
        }


        public ActionResult goDeleteCalendar(string theHoliday)
        {
            tw_Calendar wkHoliday = new tw_Calendar() { holiday = DateTime.Parse(theHoliday), z_status = "D", z_usr = GlobalVariables.MySession.Account };
            SP_Return ResultModel = pgmAttendance.Update_TW_Calendar(wkHoliday);
            TempData["message"] = ResultModel.msg;
            return RedirectToAction("Calendar");
        }


        [ValidateAntiForgeryToken]
        public ActionResult goSaveCalendar(tw_Calendar pmHoliday)
        {
            SP_Return ResultModel = pgmAttendance.Update_TW_Calendar(pmHoliday);
            TempData["message"] = ResultModel.msg;
            return RedirectToAction("Calendar");
        }

        #endregion

        #region Card Stamp

        public PartialViewResult ShowCardStampDetails(string pm_card_cd, DateTime pm_card_stamp)
        {
            ViewBag.authorized = shareCommon.isAuthorized(72);

            var twEmps = pgmEmployee.FetchList_TW_employee();
            SelectList theEmps = new SelectList(twEmps, "account_id", "full_name");
            ViewData["List_TW_Employee"] = theEmps;

            return PartialView("_CardStampDetails", pgmAttendance.Get_TW_OneCardStamp(pm_card_cd, pm_card_stamp));
        }


        public PartialViewResult ShowCardStampList(DateTime pm_theDate)
        {
            ViewBag.authorized = shareCommon.isAuthorized(72);
            ViewData["AssignedDate"] = pgmAttendance.Get_TW_AssignedDate();
            return PartialView("_CardStampList", pgmAttendance.FetchList_TW_CardStamp(pm_theDate));
        }


        public ActionResult CardStamp(DateTime? pm_theDate = null)
        {
            ViewBag.PageInfo = shareCommon.GetPageInfo(72);
            ViewBag.Dev = pgmAttendance.devMode;
            ViewData["pm_theDate"] = (pm_theDate == null ? pgmAttendance.Get_TW_AssignedDate() : String.Format("{0:yyyy-MM-dd}", pm_theDate));
            return View();
        }


        [ValidateAntiForgeryToken]
        public ActionResult Go_SaveCardStampDetails(tw_CardStamp pm_Stamp)
        {
            SP_Return result = pgmAttendance.Update_TW_CardStamp(pm_Stamp);
            TempData["message"] = result.msg;
            return RedirectToAction("CardStamp", new { pm_theDate = (pm_Stamp.override_stamp == null ? pm_Stamp.card_stamp : pm_Stamp.override_stamp) });
        }


        public ActionResult goImportCardStamp(List<HttpPostedFileBase> pm_file, string assigned_date)
        {
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            string thePath = "";
            List<string> savedPath = new List<string>();

            foreach (HttpPostedFileBase pFile in pm_file)
            {
                thePath = Path.Combine(shareFileActions.GetUploadFolder(), Path.GetFileName(pFile.FileName));
                pFile.SaveAs(thePath);
                savedPath.Add(thePath);
            }

            if (savedPath.Count() > 0)
                ResultModel = pgmAttendance.ImportCardStamp(savedPath, assigned_date);
            else
                ResultModel.msg = "No files uploaded.";

            TempData["message"] = ResultModel.msg;
            return RedirectToAction("CardStamp");
        }

        #endregion

        #region Approval

        public PartialViewResult ShowLeaveTakingApprovalSpecial(int pm_apply_uid)
        {
            ViewBag.authorized = shareCommon.isAuthorized(71);
            tw_LeaveApproval theApply = GlobalVariables.MySession.List_TW_DayAproval.Where(w => w.apply_uid == pm_apply_uid).First();
            tw_day_available theAvailable = pgmAttendance.LoadApprovalToAvaiable(theApply);
            ViewData["emp_name"] = theApply.emp_name;
            ViewData["account_id"] = theApply.account_id;
            ViewData["apply_uid"] = theApply.apply_uid;

            return PartialView("_LeaveTakingApproval_Special", theAvailable);
        }


        public ActionResult LeaveTakingApproval()
        {
            ViewBag.PageInfo = shareCommon.GetPageInfo(71);
            ViewBag.Dev = pgmAttendance.devMode;
            ViewBag.authorized = shareCommon.isAuthorized(71);
            ViewData["isHR"] = pgmEmployee.isHR();
            TempData["message"] = TempData["controller_msg"];
            List<tw_LeaveApproval> theApprovals = pgmAttendance.FetchList_TW_LeaveApproval();
            return View(theApprovals);
        }


        //[HttpPost]
        //[MultipleButton(Name = "LeaveApprovalAction", Argument = "Approve")]
        [ValidateAntiForgeryToken]
        public ActionResult Go_ApproveApply(string apply_uid, string pm_reason = "")
        {
            SP_Return result = pgmAttendance.Update_LeaveApproval(apply_uid, 1, pm_reason);
            TempData["controller_msg"] = result.msg;
            return RedirectToAction("LeaveTakingApproval");
        }


        //[HttpPost]
        //[MultipleButton(Name = "LeaveApprovalAction", Argument = "Reject")]
        [ValidateAntiForgeryToken]
        public ActionResult Go_RejectApply(string apply_uid, string pm_reason)
        {
            SP_Return result = pgmAttendance.Update_LeaveApproval(apply_uid, -1, pm_reason);
            TempData["controller_msg"] = result.msg;
            return RedirectToAction("LeaveTakingApproval");
        }


        [ValidateAntiForgeryToken]
        public ActionResult Go_ApproveApplySpecial(tw_day_available pm_available, string apply_uid, string pm_reason = "")
        {
            pm_available.available_hours *= 8;  //days to hours
            SP_Return result = pgmAttendance.Update_TW_Available(pm_available);
            if (result.r == 1)
                result = pgmAttendance.Update_LeaveApproval(apply_uid, 1, pm_reason);
            TempData["controller_msg"] = result.msg;
            return RedirectToAction("LeaveTakingApproval");
        }

        #endregion

        #region Calculation and Inform

        public PartialViewResult ShowCalculationAndInform()
        {
            ViewBag.authorized = shareCommon.isAuthorized(93);
            return PartialView("_CalculationAndInform", pgmAttendance.FetchList_TW_CardStamp_Matching());
        }


        public ActionResult HR_CalculationAndInform()
        {
            ViewBag.PageInfo = shareCommon.GetPageInfo(93);
            ViewBag.Dev = pgmAttendance.devMode;
            return View();
        }


        [ValidateAntiForgeryToken]
        public ActionResult Go_Inform(List<tw_CardStamp_Matching> pm_Stamps)
        {
            SP_Return result = pgmAttendance.Inform_TW_CardStamp_Matching(pm_Stamps);
            TempData["message"] = result.msg;
            return RedirectToAction("HR_CalculationAndInform");
        }

        #endregion

    }
}