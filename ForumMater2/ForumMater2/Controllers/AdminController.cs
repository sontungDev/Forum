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

            var str_user = HttpContext.Request.Cookies.Get("remember_admin_name");
            var str_pass = HttpContext.Request.Cookies.Get("remember_admin_password");

            if (str_user != null && str_pass != null)
            {
                ViewBag.username = str_user.Value;
                ViewBag.pass = Assitant.Instance.DecodeF64(str_pass.Value);
                ViewBag.check = "checked";
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
                    HttpCookie user_name_cookie = new HttpCookie("remember_admin_name");
                    user_name_cookie.Value = user_name;
                    user_name_cookie.Expires.AddMonths(1);

                    HttpCookie password_cookie = new HttpCookie("remember_admin_password");
                    password_cookie.Value = Assitant.Instance.EncodeF64(password);
                    password_cookie.Expires.AddMonths(1);

                    Response.Cookies.Add(user_name_cookie);
                    Response.Cookies.Add(password_cookie);
                }

                // nếu ô nhớ mật khẩu không được tick, thì xóa đi cookie đã lưu nếu có
                else
                {
                    //lấy cookie đang có
                    HttpCookie user_name_cookie = ControllerContext.HttpContext.Request.Cookies["remember_admin_name"];
                    HttpCookie password_cookie = ControllerContext.HttpContext.Request.Cookies["remember_admin_password"];

                    if (user_name_cookie != null && password_cookie != null)
                    {
                        // thiết lập lại giờ lưu cookie
                        user_name_cookie.Expires = DateTime.Now.AddMonths(-1);
                        password_cookie.Expires = DateTime.Now.AddMonths(-1);

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
                return Content("Bạn không thể truy cập vào mục này");
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

        // tìm kiếm clb
        [HttpPost]
        public ActionResult Clubs(FormCollection form_data)
        {
            string clb_id = form_data["ma-clb"];
            string qtv_id = form_data["ma-qtv"];

            ViewBag.clb = clb_id;
            ViewBag.qtv = qtv_id;
            List<Club> clubs;
            if(clb_id.Length == 0 && qtv_id.Length == 0)
            {
                clubs = db.Clubs.ToList();
                // xuất cái danh sách ra
            }
            else
            {
                clubs = db.Clubs.Where(m => m.ID.Contains(clb_id) && m.Approval.Contains(qtv_id)).ToList();
            }

            if (clubs.Count == 0)
                ViewBag.nodata = "Không có dữ liệu phù hợp";

            return View(clubs);
        }

        // tìm kiếm bài viết
        [HttpPost]
        public ActionResult Posts(FormCollection form_data)
        {
            string id = form_data["id"];
            string clb = form_data["clb"];

            ViewBag.id = id;
            ViewBag.clb = clb;

            List<Post> posts;
            if (id.Length == 0 && clb.Length == 0)
            {
                posts = db.Posts.ToList();
                // xuất cái danh sách ra
            }
            else
            {
                posts = db.Posts.Where(m => m.ID.Contains(id) && m.ClubID.Contains(clb)).ToList();
            }

            if (posts.Count == 0)
                ViewBag.nodata = "Không có dữ liệu phù hợp";
            return View(posts);
        }
        // tìm kiếm tài khoản
        [HttpPost]
        public ActionResult Users(FormCollection form_data)
        {
            string id = form_data["id"];
            string name = form_data["name"];

            ViewBag.id = id;
            ViewBag.name = name;

            List<User> users;
            if (id.Length == 0 && name.Length == 0)
            {
                users = db.Users.ToList();
                // xuất cái danh sách ra
            }
            else
            {
                users = db.Users.Where(m => m.ID.Contains(id) && m.LastName.Contains(name)).ToList();
            }

            if (users.Count == 0)
                ViewBag.nodata = "Không có dữ liệu phù hợp";
            return View(users);
        }
        
        // tìm kiếm trong quản lý admin
        [HttpPost]
        public ActionResult ManageAdmin(FormCollection form_data)
        {
            string id = form_data["id"];
            string level = form_data["level"];

            ViewBag.id = id;
            ViewBag.level = level;

            List<Administrator> admins;
            if (id.Length == 0 && level.Length == 0)
            {
                admins = db.Administrators.ToList();
                // xuất cái danh sách ra
            }
            else
            {
                int level_short = int.Parse(level);
                admins = db.Administrators.Where(m => m.ID.Contains(id) && m.Level == level_short).ToList();
            }

            if (admins.Count == 0)
                ViewBag.nodata = "Không có dữ liệu phù hợp";

            return View(admins);
        }

        // thêm quản trị viên
        [HttpPost]
        public JsonResult CreateAdmin(FormCollection form_data)
        {
            string admin_name = form_data["admin-name"];
            string password = form_data["password"];
            string full_name = form_data["full-name"];
            string email = form_data["email"];
            string phone = form_data["phone"];
            string level = form_data["level"];

            bool result = false;

            string current_id = db.Administrators.Select(m => m.ID).Max();

            string id = Assitant.Instance.GetAutoID(current_id, "AID");

            Administrator administrator = new Administrator()
            {
                ID = id,
                AdministratorName = admin_name,
                Password = Assitant.Instance.EncodeF64(password),
                Email = email,
                FullName = full_name,
                DateCreated = DateTime.Now.Date,
                Level = short.Parse(level),
                Phone = phone
            };

            db.Administrators.Add(administrator);

            int res = db.SaveChanges();


            if (res > 0)
                result = true;
           
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult RemoveUser(string id)
        {
            User user = db.Users.Find(id);

            db.Users.Remove(user);

            db.SaveChanges();
            return RedirectToAction("Users","Admin");
        }
        public ActionResult RemovePost(string id)
        {
            Post post = db.Posts.Find(id);

            db.Posts.Remove(post);

            db.SaveChanges();
            return RedirectToAction("Posts", "Admin");
        }
        public ActionResult RemoveClub(string id)
        {
            Club club = db.Clubs.Find(id);

            db.Clubs.Remove(club);

            db.SaveChanges();
            return RedirectToAction("Clubs", "Admin");
        }
        public ActionResult RemoveAdmin(string id)
        {
            Administrator administrator = db.Administrators.Find(id);

            db.Administrators.Remove(administrator);

            db.SaveChanges();
            return RedirectToAction("ManageAdmin", "Admin");
        }
    }
}