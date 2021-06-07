using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ForumMater2.Models;
using LIB;
using PagedList;
using PagedList.Mvc;

namespace ForumMater2.Controllers
{
    public class UserController : XController
    {
        private ClubForumEntities db = new ClubForumEntities();

        // Trang chủ
        #region
        public ActionResult Home(int page = 1, int size = 1)
        {
            if(Session["user"] != null)
            {
                string user_id = Session["user"].ToString();
                User user = DBHelper.Instance.GetUserById(user_id);

                // lấy user đang sử dụng
                ViewBag.User = user;

                ViewBag.Url = UrlContext();
                ViewBag.UrlAva = UrlContext() + "/assets/images/avatars";
                IEnumerable<Post> list_post = db.Posts.OrderByDescending(m => m.DateTimeCreated).ToPagedList(page, size);

                return View(list_post);
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

        // trang cá nhân
        public ActionResult MyProfile()
        {
            if(Session["user"] != null)
            {
                ViewBag.Url = UrlContext();
                ViewBag.UrlAva = UrlContext() + "/assets/images/avatars";
                ViewBag.UrlCover = UrlContext() + "assets/images/covers";

                return View();
            }
            return Redirect("/Log/Login");
        }
        [HttpPost]
        // dùng để cập nhập ảnh đại diện người dùng
        public JsonResult MyProfile1(FormCollection form_data)
        {
            string imgbase64 = form_data["avatar"];
            byte[] bytes = Convert.FromBase64String(imgbase64.Split(',')[1]);

            // lấy tên ảnh
            string name_file = Guid.NewGuid() + ".jpeg";

            // ghi file vào máy chủ
            FileStream stream = new FileStream(Server.MapPath("~/assets/images/avatars/" + name_file), FileMode.Create);
            stream.Write(bytes, 0, bytes.Length);
            stream.Flush();

            // lấy user đang sử dụng
            string id = Session["user"].ToString();

            // cập nhật ảnh đại diện
            User user = DBHelper.Instance.GetUserById(id);
            user.Avatar = name_file;
            
            db.Entry(user).State = EntityState.Modified;
            
            db.SaveChanges();

            return Json( form_data["avatar"] , JsonRequestBehavior.AllowGet) ;
        }

        // làm gì đó tiếp đi
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}