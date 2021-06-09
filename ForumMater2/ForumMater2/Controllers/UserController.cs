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
                User user = db.Users.Find(user_id);

                // lấy user đang sử dụng
                ViewBag.User = user;

                ViewBag.Url = UrlContext();
                ViewBag.UrlAva = UrlContext() + "/assets/images/users/avatars";
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

        // chi tiết bài đăng
        public ActionResult PostDetail(string id)
        {
            if(Session["user"] != null)
            {
                ViewBag.Url = UrlContext();
                Post post = db.Posts.Find(id);
                return View(post);
            }
            return Redirect("/Log/Login");
        }
        #endregion


        // Bình luận
        #region
        // viết binh luận nhưng không chờ duyệt
        [HttpPost]
        public JsonResult PostCommnent(string post_id, string content)
        {
            string current_id = db.Comments.Select(m => m.ID).Max();
            string id = Assitant.Instance.GetAutoID(current_id, "MID");

            string user_id = Session["user"].ToString();
            string approval = "AID0000000000";
            string json = "";

            Comment comment = new Comment()
            {
                ID = id,
                Approval = approval,
                Content = content,
                DateTimeCreated = DateTime.Now,
                PostID = post_id,
                UserID = user_id             
            };

            db.Comments.Add(comment);
            int res = db.SaveChanges();

            if (res > 0)
                json = "Thành công";
            else
                json = "Thất bại";

            return Json(json, JsonRequestBehavior.AllowGet);
        }
        #endregion

        // trang cá nhân
        #region
        public ActionResult MyProfile()
        {
            if(Session["user"] != null)
            {
                ViewBag.Url = UrlContext();
                ViewBag.UrlAva = UrlContext() + "/assets/images/users/avatars";
                ViewBag.UrlCover = UrlContext() + "assets/images/covers";

                string id = Session["user"].ToString();

                User user = db.Users.Find(id);

                return View(user);
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
            User user = db.Users.Find(id);
            user.Avatar = name_file;
            
            db.Entry(user).State = EntityState.Modified;
            
            db.SaveChanges();

            return Json( form_data["avatar"] , JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult MyProfile2(FormCollection form_data)
        {
            string first_name = form_data["first-name"];
            string last_name = form_data["last-name"];
            bool gender = bool.Parse(form_data["gender"]);
            DateTime dob = DateTime.Parse(form_data["dob"]);
            string phone = form_data["phone"];
            string address = form_data["address"];
            string workplace = form_data["workplace"];

            string id = Session["user"].ToString();

            User user = db.Users.Find(id);

            user.FirstName = first_name;
            user.LastName = last_name;
            user.Gender = gender;
            user.DateOfBirth = dob;
            user.Phone = phone;
            user.Address = address;
            user.Workplace = workplace;

            db.Entry(user).State = EntityState.Modified;

            db.SaveChanges();

            return RedirectToAction("MyProfile","User", user);
        }
        #endregion

        // thêm câu lạc bộ
        #region
        public ActionResult CreateClub()
        {
            if(Session["user"] != null)
            {
                ViewBag.Url = UrlContext();
                return View();
            }
            return Redirect("/Log/Login");
        }
        [HttpPost]
        public JsonResult CreateClub(FormCollection form_data)
        {
            string imgbase64 = form_data["cover"];
            string name = form_data["long_name"];
            string short_name = form_data["short_name"];
            string describe = form_data["describe"];
            string type_club = form_data["type_club"];


            byte[] bytes = Convert.FromBase64String(imgbase64.Split(',')[1]);

            // lấy tên ảnh
            string name_file = Guid.NewGuid() + ".jpeg";

            // ghi file vào máy chủ
            FileStream stream = new FileStream(Server.MapPath("~/assets/images/clubs/covers/" + name_file), FileMode.Create);
            stream.Write(bytes, 0, bytes.Length);
            stream.Flush();

            // lấy user đang sử dụng
            string user_id = Session["user"].ToString();
            string current_id = db.Clubs.Select(m => m.ID).Max();

            string id = Assitant.Instance.GetAutoID(current_id, "CID");

            Club club = new Club()
            {
                ID = id,
                Approval = "AID0000000000",
                CoverPhoto = name_file,
                DateCreated = DateTime.Now.Date,
                Description = describe,
                Name = name,
                ShortName = short_name,
                UserCreated = user_id,
                Type = type_club               
            };

            db.Clubs.Add(club);

            db.SaveChanges();

            return Json("Thành công", JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult IsExistShortNameClub(string val)
        {
            Club club = null;
            string result = "";
            club = db.Clubs.Where(m => m.ShortName == val).FirstOrDefault();
            if (club is null)
                result = "not exist";
            else
                result = "exist";
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult IsExistLongNameClub(string val)
        {
            Club club = null;
            string result = "";
            club = db.Clubs.Where(m => m.Name == val).FirstOrDefault();
            if (club is null)
                result = "not exist";
            else
                result = "exist";
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        // trang chi tiết câu lạc bộ
        #region
        // trang cho quản trưởng clb
        public ActionResult ClubDetail(string id)
        {
            if(Session["user"] != null)
            {
                string user_id = Session["user"].ToString();
                ViewBag.Url = UrlContext();
                Club club = db.Clubs.Find(id);
                int roles = db.UserClubRoles.Where(m => m.ClubID == id && m.UserID == user_id).Select(m => m.Role).FirstOrDefault();
                if (roles == 3)
                    return View(club);
                else if (roles == 2)
                    return RedirectToAction("ClubDetailMem/" + id);
                else
                    return RedirectToAction("ClubDetailCus/" + id);
            }
            return Redirect("/Log/Login");
        }
        // trang cho thành viên
        public ActionResult ClubDetailMem(string id)
        {
            if (Session["user"] != null)
            {
                ViewBag.Url = UrlContext();
                Club club = db.Clubs.Find(id);
                return View("ClubDetail",club);
            }
            return Redirect("/Log/Login");
        }
        // dành cho người không phải thành viên
        public ActionResult ClubDetailCus(string id)
        {
            if (Session["user"] != null)
            {
                ViewBag.Url = UrlContext();
                Club club = db.Clubs.Find(id);
                return View("ClubDetail", club);
            }
            return Redirect("/Log/Login");
        }

        #endregion

        // xử lý rời , gia nhập câu lạc bộ
        #region
        // xử lý rời câu lạc bộ
        [HttpPost]
        public JsonResult LeaveClub(string user_id, string club_id)
        {
            UserClubRole userClubRole = db.UserClubRoles.Where(m => m.ClubID == club_id && m.UserID == user_id).FirstOrDefault();

            userClubRole.Role = 0;

            int res = db.SaveChanges();
            if(res > 0)
            {
                return Json("Thành công", JsonRequestBehavior.AllowGet);
            }
            return Json("Thất bại", JsonRequestBehavior.AllowGet);

        }
        // xử lý gia nhập câu lạc bộ
        [HttpPost]
        public JsonResult JoinClub(string user_id, string club_id)
        {

            UserClubRole userClubRole = db.UserClubRoles.Where(m => m.ClubID == club_id && m.UserID == user_id).FirstOrDefault();

            userClubRole.Role = 1;

            int res = db.SaveChanges();
            if (res > 0)
            {
                return Json("Thành công", JsonRequestBehavior.AllowGet);
            }
            return Json("Thất bại", JsonRequestBehavior.AllowGet);
        }

        // xử lý hủy yêu cầu câu lạc bộ
        [HttpPost]
        public JsonResult CancelRequest(string user_id, string club_id)
        {
            UserClubRole userClubRole = db.UserClubRoles.Where(m => m.ClubID == club_id && m.UserID == user_id).FirstOrDefault();

            userClubRole.Role = 0;

            int res = db.SaveChanges();
            if (res > 0)
            {
                return Json("Thành công", JsonRequestBehavior.AllowGet);
            }
            return Json("Thất bại", JsonRequestBehavior.AllowGet);
        }

        #endregion
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