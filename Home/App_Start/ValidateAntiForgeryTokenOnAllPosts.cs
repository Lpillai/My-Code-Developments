using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace Home
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ValidateAntiForgeryTokenOnAllPosts : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            var request = filterContext.HttpContext.Request;

            //  Only validate POSTs
            if (request.HttpMethod == WebRequestMethods.Http.Post)
            {
                bool skipCheck = filterContext.ActionDescriptor.IsDefined(typeof(DontCheckForAntiForgeryTokenAttribute), true)
                    || filterContext.ActionDescriptor.ControllerDescriptor.IsDefined(typeof(DontCheckForAntiForgeryTokenAttribute), true);

                if (skipCheck)
                    return;

                //  Ajax POSTs and normal form posts have to be treated differently when it comes
                //  to validating the AntiForgeryToken
                if (request.IsAjaxRequest())
                {
                    var antiForgeryCookie = request.Cookies[AntiForgeryConfig.CookieName];

                    var cookieValue = antiForgeryCookie != null
                        ? antiForgeryCookie.Value
                        : null;

                    //AntiForgery.Validate(cookieValue, request.Headers["__RequestVerificationToken"]);
                }
                else
                {
                    new ValidateAntiForgeryTokenAttribute()
                        .OnAuthorization(filterContext);
                }
            }
        }
    }


    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class DontCheckForAntiForgeryTokenAttribute : Attribute { }

}