using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ForumMater2.Models;
using LIB;

namespace ForumMater2.Controllers
{
    public class UserController : XController
    {
        private ClubForumEntities db = new ClubForumEntities();

        // Trang chủ
        #region
        public ActionResult Home()
        {
            if(Session["user"] != null)
            {
                string user_id = Session["user"].ToString();
                User user = DBHelper.Instance.GetUserById(user_id);

                // lấy user đang sử dụng
                ViewBag.User = user;
                ViewBag.Url = UrlContext();
                return View();
            }
            return Redirect("/Log/Login"); 
        }
        #endregion

        // Đăng bài
        #region
        public ActionResult CreatePost()
        {
            if(Session["user"] != null)
            {
                string user_id = Session["user"].ToString();
                List<Club> list_club_parti = db.UserClubRoles.Where(m => m.UserID == user_id).Select(m => m.Club).ToList();
                ViewBag.Url = UrlContext();
                return View(list_club_parti);
            }
            return Redirect("/Log/Login");
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult CreatePost(FormCollection form_data)
        {
            string user_id = Session["user"].ToString();
            string current_post_id = db.Posts.Select(m => m.ID).Max();

            string id_post = Assitant.Instance.GetAutoID(current_post_id, "PID");

            Post post = new Post()
            {
                ID = id_post,
                Content = form_data["content"],
                DateTimeCreated = DateTime.Now,
                UserID = user_id,
                ClubID = form_data["club_id"],
                Title = form_data["title"],
                Hashtag = form_data["hashtag"],
                Approval = "AID0000000000"
            };
            db.Posts.Add(post);
            db.SaveChanges();

            return RedirectToAction("Home","User");
        }
        #endregion

        // làm gì đó tiếp đi
    }
}