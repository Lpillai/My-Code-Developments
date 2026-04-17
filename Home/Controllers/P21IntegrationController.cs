using Home.Models;
using P21;
using RuntimeVariables;
using SharedScrpits;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace Home.Controllers
{
    [SessionExpireFilter]
    public class P21IntegrationController : Controller
    {
        Common_Share shareCommon = new Common_Share();
        FileActions_Share shareFileActions = new FileActions_Share();
        HomeScripts scriptsHome = new HomeScripts();
        Integration_Program pgmP21 = new Integration_Program();


        #region Item Maintenance

        [DeleteFileAttribute]
        public ActionResult ExampleSheet_PGID()
        {
            SP_Return ExportModel = pgmP21.GetExampleSheet_PGID();
            if (ExportModel.r == 1)
                return File(ExportModel.JsonData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            //return File(ExportModel.JsonData, System.Net.Mime.MediaTypeNames.Application.Octet);
            else
            {
                TempData["message"] = ExportModel.msg;
                return RedirectToAction("ProductGroupID");
            }
        }


        public ActionResult ProductGroupID()
        {
            ViewBag.PageInfo = shareCommon.GetPageInfo(84);
            if (scriptsHome.devStatus())
                ViewBag.Dev = pgmP21.devMode;
            ViewBag.authorized = shareCommon.isAuthorized(84);
            return View();
        }


        [ValidateAntiForgeryToken]
        public ActionResult goUpdateProductGroupID(HttpPostedFileBase pm_file)
        {
            TempData["message"] = null;
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            string thePath = Path.Combine(shareFileActions.GetUploadFolder(), Path.GetFileName(pm_file.FileName));

            ResultModel.r = 1;
            if (pm_file.ContentLength > 0)
            {
                try
                {
                    pm_file.SaveAs(thePath);
                    ResultModel = pgmP21.UpdateProductGroupID(thePath);
                }
                catch (Exception ex)
                {
                    ResultModel.r = 0;
                    ResultModel.msg = ex.ToString();
                }

                if (ResultModel.r == 1)
                    TempData["message"] = "Command Sent. The outcome will be sent to you in a few minutes.";
                else
                    TempData["message"] = ResultModel.msg;
            }
            else
            {
                TempData["message"] = "No content was found in the file.";
            }

            return RedirectToAction("ProductGroupID");
        }


        [DeleteFileAttribute]
        public ActionResult ExampleSheet_ABCClass()
        {
            SP_Return ExportModel = pgmP21.GetExampleSheet_ABCClass();
            if (ExportModel.r == 1)
                return File(ExportModel.JsonData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            else
            {
                TempData["message"] = ExportModel.msg;
                return RedirectToAction("ABCClass");
            }
        }


        public ActionResult ABCClass()
        {
            ViewBag.PageInfo = shareCommon.GetPageInfo(115);
            if (scriptsHome.devStatus())
                ViewBag.Dev = pgmP21.devMode;
            ViewBag.authorized = shareCommon.isAuthorized(115);
            return View();
        }


        [ValidateAntiForgeryToken]
        public ActionResult goUpdateABCClass(HttpPostedFileBase pm_file)
        {
            TempData["message"] = null;
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            string thePath = Path.Combine(shareFileActions.GetUploadFolder(), Path.GetFileName(pm_file.FileName));

            ResultModel.r = 1;
            if (pm_file.ContentLength > 0)
            {
                try
                {
                    pm_file.SaveAs(thePath);
                    ResultModel = pgmP21.UpdateABCClass(thePath);
                }
                catch (Exception ex)
                {
                    ResultModel.r = 0;
                    ResultModel.msg = ex.ToString();
                }

                if (ResultModel.r == 1)
                    TempData["message"] = "Command Sent. The outcome will be sent to you in a few minutes.";
                else
                    TempData["message"] = ResultModel.msg;
            }
            else
            {
                TempData["message"] = "No content was found in the file.";
            }

            return RedirectToAction("ABCClass");
        }


        [DeleteFileAttribute]
        public ActionResult ExampleSheet_LeadTime()
        {
            SP_Return ExportModel = pgmP21.GetExampleSheet_LeadTime();
            if (ExportModel.r == 1)
                return File(ExportModel.JsonData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            else
            {
                TempData["message"] = ExportModel.msg;
                return RedirectToAction("LeadTime");
            }
        }


        public ActionResult LeadTime()
        {
            ViewBag.PageInfo = shareCommon.GetPageInfo(116);
            if (scriptsHome.devStatus())
                ViewBag.Dev = pgmP21.devMode;
            ViewBag.authorized = shareCommon.isAuthorized(116);
            return View();
        }


        [ValidateAntiForgeryToken]
        public ActionResult goUpdateLeadTime(HttpPostedFileBase pm_file)
        {
            TempData["message"] = null;
            SP_Return ResultModel = new SP_Return() { r = 1, msg = "", JsonData = "" };
            string thePath = Path.Combine(shareFileActions.GetUploadFolder(), Path.GetFileName(pm_file.FileName));

            ResultModel.r = 1;
            if (pm_file.ContentLength > 0)
            {
                try
                {
                    pm_file.SaveAs(thePath);
                    ResultModel = pgmP21.UpdateLeadTime(thePath);
                }
                catch (Exception ex)
                {
                    ResultModel.r = 0;
                    ResultModel.msg = ex.ToString();
                }

                if (ResultModel.r == 1)
                    TempData["message"] = "Command Sent. The outcome will be sent to you in a few minutes.";
                else
                    TempData["message"] = ResultModel.msg;
            }
            else
            {
                TempData["message"] = "No content was found in the file.";
            }

            return RedirectToAction("LeadTime");
        }

        #endregion

    }
}