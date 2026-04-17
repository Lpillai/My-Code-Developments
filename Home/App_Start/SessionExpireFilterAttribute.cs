using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Home
{
    //[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class SessionExpireFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            bool isLoginPage = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName == "Login" && filterContext.ActionDescriptor.ActionName == "Index";

            if (!isLoginPage && (HttpContext.Current.Session["Validity"] == null || !(bool)HttpContext.Current.Session["Validity"]))
            {
                //filterContext.Result = new RedirectResult("~/Login/Index");
                //return;
                reLogon(filterContext);
            }

            base.OnActionExecuting(filterContext);
        }


        private void reLogon(ActionExecutingContext filterContext)
        {
            RouteValueDictionary dictionary = new RouteValueDictionary ( new {
                    controller = "Login",
                    action = "StartLogOut",
                    returnUrl = filterContext.HttpContext.Request.RawUrl
                });

            filterContext.Result = new RedirectToRouteResult(dictionary);
        }
    }
    
    /*
    public class SessionExpireFilterAttribute : ActionFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            //You may fetch data from database here 
            filterContext.Controller.ViewBag.GreetMesssage = "Hello SBS";
            base.OnResultExecuting(filterContext);
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var controllerName = filterContext.RouteData.Values["controller"];
            var actionName = filterContext.RouteData.Values["action"];
            var message = String.Format("{0} controller:{1} action:{2}", "onactionexecuting", controllerName, actionName);
            Debug.WriteLine(message, "Action Filter Log");
            base.OnActionExecuting(filterContext);
        }
    }
    */
}
