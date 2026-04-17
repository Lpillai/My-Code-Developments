using Home.Models;
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
    public class LoginController : Controller
    {
        LoginScripts scriptsLogin = new LoginScripts();
        Common_Share shareCommon = new Common_Share();
        FileActions_Share shareFileActions = new FileActions_Share();


        public ActionResult GetImage()
        {
            string path = Path.Combine(shareFileActions.GetImageFolder(), "logo_1.png");
            byte[] imageByteData = System.IO.File.ReadAllBytes(path);
            return File(imageByteData, "image/png");
        }


        public ActionResult GetLoginName()
        {
            return Content(GlobalVariables.MySession.FirstName);
        }


        public ActionResult GetLoginMenu()
        {
            return Content(GlobalVariables.MySession.LoginMenuString);
        }


        [NoCache]
        public ActionResult Index()
        {
            HomeScripts scriptsHome = new HomeScripts();
            scriptsHome.FetchEnvironment();
            /*
            //if (GlobalConfig.MyConfig == null)
            if (GlobalConfig.MyConfig == null || !GlobalConfig.MyConfig.Validity)
            {
                scriptsHome.FetchConfig();
                scriptsHome.FetchEnvironment();
            }
            */
            ViewBag.dev = scriptsHome.devStatus();
            ViewBag.debug = scriptsHome.debugStatus();
            ViewBag.Title = (scriptsHome.debugStatus() ? "[Debug] " : "") + "SBS Intranet" + (scriptsHome.devStatus() ? " DEV" : "");

            return View();
        }


        public ActionResult StartLogin(FormCollection collection)
        {
            string pm_Account = collection["pm_Account"].Replace(" ", "");
            string pm_Password = collection["pm_Password"];

            if (string.IsNullOrWhiteSpace(pm_Account) || string.IsNullOrWhiteSpace(pm_Password))
            {
                TempData["message"] = "Please insert information into both two columns.";
                return RedirectToAction("Index");
            }
            else
            {
                SP_Return result = scriptsLogin.LoginCheck(pm_Account, pm_Password);

                switch (result.r)
                {
                    case 1: //The parameter is invalid, please contact IT group for this error.
                    case 2: //This account or the password is incorrect.
                    case 10: //Run time error.
                        TempData["message"] = result.msg;
                        return RedirectToAction("Index");
                    default:
                        if (result.r == 0)  //good to go
                        {
                            return RedirectToAction("Index", "Home");
                        }
                        else
                        {
                            //case 3: //Your password is expired, please change it immediately.
                            TempData["message"] = result.msg;
                            return RedirectToAction("ChangePassword", "Login");
                        }
                }
            }
        }


        public ActionResult ForgetPassword(FormCollection collection)
        {
            string pm_email = collection["pm_email"];

            if (string.IsNullOrWhiteSpace(pm_email))
                TempData["message"] = "Please insert information in the email column.";
            else
            {
                SP_Return ForgetModel = scriptsLogin.ForgetPassword(pm_email);
                TempData["message"] = ForgetModel.msg;
            }

            return RedirectToAction("Index");
        }


        [SessionExpireFilter]
        public ActionResult ChangePassword()
        {
            ViewBag.Message = "Account: " + GlobalVariables.MySession.Account;
            return View();
        }


        [SessionExpireFilter]
        public ActionResult StartChange(string Password, string NewPassword, string NewPassword2)
        {
            ViewBag.Message = "Account: " + GlobalVariables.MySession.Account;
            SP_Return result = scriptsLogin.ChangePassword(Password, NewPassword, NewPassword2);

            if (result.r == 0)
            {
                TempData["message"] = "Password was changed successfully.";
                return RedirectToAction("Index", "Home");
            }
            else
            {
                TempData["message"] = result.msg;
                return RedirectToAction("ChangePassword");
            }
        }


        public ActionResult StartLogOut()
        {
            shareCommon.AppendLog(new ActionLog() { keys = "Log out" });

            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetExpires(DateTime.UtcNow.AddHours(-1));
            Response.Cache.SetNoStore();
            Session.Clear();
            Session.RemoveAll();
            Session.Abandon();
            Response.Cookies.Clear();

            GlobalVariables.MySession.Account = null;
            ModelState.Clear();

            return RedirectToAction("Index");
        }

    }
}