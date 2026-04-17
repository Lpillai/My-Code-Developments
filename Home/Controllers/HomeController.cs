using Home.Models;
using RuntimeVariables;
using SharedScrpits;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;


namespace Home.Controllers
{
    [SessionExpireFilter]
    public class HomeController : Controller
    {
        HomeScripts scriptsHome = new HomeScripts();
        Common_Share shareCommon = new Common_Share();
        FileActions_Share shareFileActions = new FileActions_Share();


        [HttpPost]
        public JsonResult KeepSessionAlive()
        {
            return new JsonResult { Data = "Success" };
        }


        public ActionResult AlertingMessage()
        {
            //TempData["message"] = theMessage;
            return View();
        }


        public ActionResult GetTempHomeIamge()
        {
            string path = Path.Combine(shareFileActions.GetImageFolder(), "SBS Intranet.png");
            byte[] imageByteData = System.IO.File.ReadAllBytes(path);
            return File(imageByteData, "image/png");
        }


        public ActionResult GetSiteIconIamge()
        {
            string path = Path.Combine(shareFileActions.GetImageFolder(), "specialtybolt_favicon.png");
            byte[] imageByteData = System.IO.File.ReadAllBytes(path);
            return File(imageByteData, "image/png");
        }


        public ActionResult GetHomeLink()
        {
            //string project_name = (!this.scriptsHome.debugStatus() ? GlobalConfig.MyConfig.ProjectName + "/" : "");
            string project_name = (!this.scriptsHome.debugStatus() ? scriptsHome.getProjectName() + "/" : "");
            return Content("<a class=\"navbar-brand\" href=\"/" + project_name + "Home/Index\">" + (this.scriptsHome.debugStatus() ? "[Debug] " : "") + "SBS Intranet" + (this.scriptsHome.devStatus() ? " DEV" : "") + "</a>");
        }


        public ActionResult Index()
        {
            shareFileActions.ClearOldTempFile();
            ViewBag.dev = scriptsHome.devStatus();
            ViewBag.debug = scriptsHome.debugStatus();
            ViewBag.Title = (scriptsHome.debugStatus() ? "[Debug] " : "") + "SBS Intranet" + (scriptsHome.devStatus() ? " DEV" : "");
            return View();
        }


        private static DateTime GetLinkerTime(Assembly assembly)
        {
            var filePath = assembly.Location;
            const int c_PeHeaderOffset = 60;
            const int c_LinkerTimestampOffset = 8;

            var buffer = new byte[2048];

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                stream.Read(buffer, 0, 2048);

            var offset = BitConverter.ToInt32(buffer, c_PeHeaderOffset);
            var secondsSince1970 = BitConverter.ToInt32(buffer, offset + c_LinkerTimestampOffset);
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var linkTimeUtc = epoch.AddSeconds(secondsSince1970);

            return linkTimeUtc;
        }


        public ActionResult About()
        {
            ViewBag.Message = "Assembly";
            ViewData["isDebug"] = scriptsHome.debugStatus().ToString();
            ViewData["isDev"] = scriptsHome.devStatus().ToString();
            ViewData["AssemblyDate"] = GetLinkerTime(Assembly.GetExecutingAssembly());
            //ViewData["MySession"] = GlobalVariables.MySession;
            /*
            ViewData["folderPath1"] = System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).Replace("bin", ""));
            ViewData["folderPath2"] = System.IO.Path.GetDirectoryName(ViewData["folderPath1"].ToString().Substring(0, ViewData["folderPath1"].ToString().Length - 1)).Replace("\\Home", "");
            if (scriptsHome.debugStatus())
            {
                ViewData["folderPath3"] = ViewData["folderPath2"].ToString().Replace(System.IO.Path.GetDirectoryName(ViewData["folderPath2"].ToString()), "").Replace("\\", "");
            }
            else
            {
                ViewData["folderPath3"] = ViewData["folderPath1"].ToString().Replace(System.IO.Path.GetDirectoryName(ViewData["folderPath1"].ToString()), "").Replace("\\", "");
            }
            ViewData["RootFolder"] = scriptsHome.getProjectName();
            */
            return View();
        }


        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }


        public ActionResult GetImage()
        {
            string path = Path.Combine(shareFileActions.GetImageFolder(), "ComingSoon2.jpg");
            byte[] imageByteData = System.IO.File.ReadAllBytes(path);
            return File(imageByteData, "image/png");
        }


        public ActionResult ComingSoon()
        {
            ViewBag.MetaRefresh = "<meta http-equiv='refresh' content='3; url=" + Url.Action("Index", "Home") + "' />";
            return View();
        }

    }
}