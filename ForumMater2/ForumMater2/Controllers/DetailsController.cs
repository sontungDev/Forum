using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ForumMater2.Models;
using PagedList;

namespace ForumMater2.Controllers
{
    public class DetailsController : XController
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
        public ActionResult Club(string id)
        {
            ViewBag.Url = UrlContext();
            Club club = db.Clubs.Find(id);
            return View(club);
        }

        public ActionResult AdminClub(string id, int page = 1,int size  = 10)
        {
            Club club = db.Clubs.Find(id);

            IEnumerable<Post> posts = db.Posts.Where(m => m.ClubID == id && m.Approval != "AID0")
                .OrderByDescending(m => m.DateTimeCreated).ToPagedList(page, size);

            ViewBag.Url = UrlContext();
            ViewBag.CID = id;
            return View(posts);
        }
        public ActionResult AdminPost(string id)
        {
            ViewBag.Url = UrlContext();
            Post post = db.Posts.Find(id);
            return View(post);
        }
        public ActionResult AdminUser(string id)
        {
            User user = db.Users.Find(id);
            return View(user);
        }
    }
}