using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ForumMater2.Models;

namespace ForumMater2.Controllers
{
    public class DetailsController : Controller
    {
        ClubForumEntities db = new ClubForumEntities();
        // GET: Details
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Post(string id)
        {
            Post post = db.Posts.Find(id);
            return View(post);
        }
    }
}