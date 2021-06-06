using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ForumMater2.Controllers
{
    public class XController : Controller
    {
        // GET: X
        protected string UrlContext()
        {
            string res = String.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content("~"));
            return res.Remove(res.Length - 1);
        }
    }
}