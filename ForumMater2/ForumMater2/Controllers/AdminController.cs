using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ForumMater2.Models;
using LIB;

namespace ForumMater2.Controllers
{
    public class AdminController : XController
    {
        ClubForumEntities db = new ClubForumEntities();
        // GET: Admin
        public ActionResult Index()
        {
            ViewBag.Url = UrlContext();
            if (Session["admin"] != null)
            {
                return RedirectToAction("Home", "Admin");
            }          
            return View();
        }
        [HttpPost]
        public ActionResult Index(FormCollection form_data)
        {
            // lấy dữ liệu từ form sau khi submit
            string user_name = form_data["user-name"];
            string password = form_data["password"];
            string remember = form_data["remember-user"];

            // kiểm tra tài khoản mật khẩu có hợp lệ hay không
            bool result = DBHelper.Instance.Authentication(user_name, password, true);

            if (result)
            {
                // thiết lập session khi đăng nhập
                Session["admin"] = DBHelper.Instance.GetAdminId(user_name);

                // thiết lập cookie nếu nhớ mật khẩu
                if (!String.IsNullOrEmpty(remember))
                {
                    // lưu lại tài khoản, mật khẩu, và thời gian cookie này tồn tại là 1 giờ
                    HttpCookie user_name_cookie = new HttpCookie("remember_user_name");
                    user_name_cookie.Value = user_name;
                    user_name_cookie.Expires.AddHours(1);

                    HttpCookie password_cookie = new HttpCookie("remember_password");
                    password_cookie.Value = password;
                    password_cookie.Expires.AddHours(1);

                    Response.Cookies.Add(user_name_cookie);
                    Response.Cookies.Add(password_cookie);
                }

                // nếu ô nhớ mật khẩu không được tick, thì xóa đi cookie đã lưu nếu có
                else
                {
                    //lấy cookie đang có
                    HttpCookie user_name_cookie = ControllerContext.HttpContext.Request.Cookies["remember_user_name"];
                    HttpCookie password_cookie = ControllerContext.HttpContext.Request.Cookies["remember_password"];

                    if (user_name_cookie != null && password_cookie != null)
                    {
                        // thiết lập lại giờ lưu cookie
                        user_name_cookie.Expires = DateTime.Now.AddHours(-1);
                        password_cookie.Expires = DateTime.Now.AddHours(-1);

                        ControllerContext.HttpContext.Response.Cookies.Add(user_name_cookie);
                        ControllerContext.HttpContext.Response.Cookies.Add(password_cookie);
                    }
                }
                return RedirectToAction("Home", "Admin");
            }
            else
            {
                ViewBag.username = user_name;
                ViewBag.pass = password;
                ViewBag.message = "Sai tài khoản hoặc mật khẩu";
                return View();
            }    
        }
        public ActionResult Clubs()
        {
            if(Session["admin"] != null)
            {
                ViewBag.Url = UrlContext();
                List<Club> clubs = db.Clubs.ToList();
                return View(clubs);
            }
            return Redirect("/Admin/Index");
        }
        public ActionResult Users()
        {
            if(Session["admin"] != null)
            {
                List<User> users = db.Users.ToList();
                return View(users);
            }
            return Redirect("/Admin/Index");

        }
        public ActionResult Posts()
        {if (Session["admin"] != null)
            {
                List<Post> posts = db.Posts.ToList();
                return View(posts);
            }
            return Redirect("/Admin/Index");
        }
        public ActionResult AdminProfile()
        {
            if (Session["admin"] != null)
            {
                string admin_id = Session["admin"].ToString();
                Administrator administrator = db.Administrators.Find(admin_id);
                return View(administrator);
            }
            return Redirect("/Admin/Index");
        }
        [HttpPost]
        public JsonResult ChangePassWord(string old_password, string new_password)
        {
            string admin_id = Session["admin"].ToString();
            Administrator administrator = db.Administrators.Find(admin_id);
            bool result = false;

            if (Assitant.Instance.DecodeF64(administrator.Password).Equals(old_password))
            {
                administrator.Password = Assitant.Instance.EncodeF64(new_password);

                db.SaveChanges();

                result = true;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult BrowserPosts()
        {
            if (Session["admin"] != null)
            {
                List<Post> posts = db.Posts.Where(m => m.Approval == "AID0").ToList();
                return View(posts);
            }
            return Redirect("/Admin/Index");
        }

        // duyệt bài
        [HttpPost]
        public JsonResult AcceptBrowserPosts(string id)
        {
            string admin_id = Session["admin"].ToString();
            Post post = db.Posts.Find(id);
            string json = "";
            post.Approval = admin_id;
            int res = db.SaveChanges();
            if (res > 0)
                json = "true";
            else
                json = "false";
            return Json(json, JsonRequestBehavior.AllowGet);            
        }

        [HttpPost]
        public JsonResult RefuseBrowserPosts(string id)
        {
            Post post = db.Posts.Find(id);            
            db.Posts.Remove(post);
            int res = db.SaveChanges();
            string json = "";
            if (res > 0)
                json = "true";
            else
                json = "false";
            return Json(json, JsonRequestBehavior.AllowGet);
        }
        // hết duyệt bài

        // duyệt clb
        public ActionResult BrowserClubs()
        {
            if (Session["admin"] != null)
            {
                List<Club> clubs = db.Clubs.Where(m => m.Approval == "AID0").ToList();
                return View(clubs);
            }
            return Redirect("/Admin/Index");
        }
        [HttpPost]
        public JsonResult AcceptClubs(string id)
        {
            string json = "";
            string admin_id = Session["admin"].ToString();

            Club club = db.Clubs.Find(id);

            UserClubRole userClubRole = new UserClubRole()
            {
                ClubID = club.ID,
                UserID = club.UserCreated,
                DateTimeJoined = DateTime.Now,
                Role = 3 // trưởng câu lạc bộ
            };

            db.UserClubRoles.Add(userClubRole);
            club.Approval = admin_id;

            int res = db.SaveChanges();
            if (res > 0)
                json = "true";
            else json = "false";
            return Json(json, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult RefuseClubs(string id)
        {
            Club club = db.Clubs.Find(id);
            db.Clubs.Remove(club);
            int res = db.SaveChanges();
            string json = "";
            if (res > 0)
                json = "true";
            else
                json = "false";
            return Json(json, JsonRequestBehavior.AllowGet);
        }
        // hết duyệt clb
        public ActionResult Home()
        {
            if (Session["admin"] != null)
            {
                return View();
            }
            return Redirect("/Admin/Index");
        }
        public ActionResult ManageAdmin()
        {
            if (Session["admin"] != null)
            {
                Administrator admin = db.Administrators.Find(Session["admin"].ToString());
                if(admin.Level == 0)
                {
                    List<Administrator> administrators = db.Administrators.ToList();
                    return View(administrators);
                }
                return Content("Bạn thể truy cập vào phần này");
            }
            return Redirect("/Admin/Index");        }

        // chi tiết , chức năng
        public ActionResult DetailClub(string id)
        {
            Club club = db.Clubs.Find(id);
            return View(club);
        }

        // đăng xuất
        public ActionResult SignOut()
        {
            if(Session["admin"] != null)
            {
                Session["admin"] = null;
                return Redirect("/Admin/Index");
            }
            return Redirect("/Admin/Index");
        }

        // cập nhật thông tin cá nhân
        public JsonResult UpdateProfile(FormCollection form_data)
        {

            string full_name = form_data["full-name"];
            string email = form_data["email"];
            string phone = form_data["phone"];

            string admin_id = Session["admin"].ToString();

            Administrator admin = db.Administrators.Find(admin_id);

            admin.FullName = full_name;
            admin.Email = email;
            admin.Phone = phone;

            db.SaveChanges();

            return Json("Cập nhật thành công", JsonRequestBehavior.AllowGet);
        }
    }
}