using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ForumMater2.Models;

namespace ForumMater2.Controllers
{
    public class AdminController : Controller
    {
        ClubForumEntities db = new ClubForumEntities();
        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Clubs()
        {
            List<Club> clubs = db.Clubs.ToList();
            return View(clubs);
        }
        public ActionResult Users()
        {
            List<User> users = db.Users.ToList();
            return View(users);
        }
        public ActionResult Posts()
        {
            List<Post> posts = db.Posts.ToList();
            return View(posts);
        }
        public ActionResult AdminProfile()
        {
            return View();
        }
        [HttpPost]
        public JsonResult ChangePassWord(string old_password, string new_pass)
        {
            return Json("OK", JsonRequestBehavior.AllowGet);
        }
    }
}