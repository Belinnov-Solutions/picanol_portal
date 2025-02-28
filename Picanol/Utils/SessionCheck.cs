using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Picanol.Utils
{
    public class SessionCheck:ActionFilterAttribute
    {
        public override void OnActionExecuting(System.Web.Mvc.ActionExecutingContext filterContext)
        {
            if (filterContext != null)
            {
                HttpSessionStateBase objHttpSessionStateBase = filterContext.HttpContext.Session;
                var userSession = objHttpSessionStateBase["UserInfo"];
                if (((userSession == null) && (!objHttpSessionStateBase.IsNewSession)) || (objHttpSessionStateBase.IsNewSession))
                {
                    objHttpSessionStateBase.RemoveAll();
                    objHttpSessionStateBase.Clear();
                    objHttpSessionStateBase.Abandon();
                    if (filterContext.HttpContext.Request.IsAjaxRequest())
                    {
                        filterContext.HttpContext.Response.StatusCode = 403;
                        filterContext.Result = new JsonResult { Data = "LogOut" };
                    }
                    else
                    {
                        filterContext.Result = new RedirectResult("~/Home/Index");
                    }

                }


            }
        }
    }
}