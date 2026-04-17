using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace Home.Models
{
    public class DeleteFileAttribute : ActionFilterAttribute
    {
        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            filterContext.HttpContext.Response.Flush();

            if ((filterContext.Result as FilePathResult) != null)
            {
                //convert the current filter context to file and get the file path
                string filePath = (filterContext.Result as FilePathResult).FileName;

                //delete the file after download
                System.IO.File.Delete(filePath);
            }
        }
    }
}