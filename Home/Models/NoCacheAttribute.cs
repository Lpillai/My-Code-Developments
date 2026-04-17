using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace Home.Models
{
    public class NoCacheAttribute : ActionFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            var response = filterContext.HttpContext.Response;
            response.Cache.SetCacheability(HttpCacheability.NoCache);
            response.Cache.SetNoStore();
            response.Cache.SetExpires(DateTime.UtcNow.AddSeconds(-1));
            response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
            base.OnResultExecuting(filterContext);
        }
    }
}