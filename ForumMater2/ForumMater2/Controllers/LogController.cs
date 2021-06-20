using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ForumMater2.Models;
using LIB;

namespace ForumMater2.Controllers
{
    public class LogController : XController
    {
        ClubForumEntities db = new ClubForumEntities();
        // Đăng nhập
        #region
        public ActionResult Login()
        {
            ViewBag.Url = UrlContext();
            if (Session["user"] != null)
            {
                return RedirectToAction("Home", "User");
            }

            var str_user = HttpContext.Request.Cookies.Get("remember_user_name");
            var str_pass = HttpContext.Request.Cookies.Get("remember_password");

            if(str_user != null && str_pass != null)
            {
                ViewBag.user = str_user.Value;
                ViewBag.pass = Assitant.Instance.DecodeF64(str_pass.Value);
                ViewBag.check = "checked";
            }    
            
            return View();
        }
        [HttpPost]
        public ActionResult Login(FormCollection form_data)
        {
            // lấy dữ liệu từ form sau khi submit
            string user_name = form_data["user-name"];
            string password = form_data["password"];
            string remember = form_data["remember-user"];

            ViewBag.user = user_name;
            ViewBag.pass = password;

            // kiểm tra tài khoản mật khẩu có hợp lệ hay không
            bool result = DBHelper.Instance.Authentication(user_name, password);

            if (result)
            {

                // thiết lập session khi đăng nhập
                Session["user"] = DBHelper.Instance.GetUserId(user_name);

                // thiết lập cookie nếu nhớ mật khẩu
                if (!String.IsNullOrEmpty(remember))
                {
                    // lưu lại tài khoản, mật khẩu, và thời gian cookie này tồn tại là 1 giờ
                    HttpCookie user_name_cookie = new HttpCookie("remember_user_name");
                    user_name_cookie.Value = user_name;
                    user_name_cookie.Expires.AddHours(1);

                    HttpCookie password_cookie = new HttpCookie("remember_password");
                    password_cookie.Value = Assitant.Instance.EncodeF64(password);
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
                return RedirectToAction("Home", "User");
            }
            else
            {
                ViewBag.message = "Sài tên đăng nhập hoặc mật khẩu";
                return View();
            }    
                
        }
        #endregion

        // Đăng ký
        #region
        public ActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Register(FormCollection form_data)
        {
            string current_id = db.Users.Select(m => m.ID).Max();
            string id = Assitant.Instance.GetAutoID(current_id, "UID");
            string user_name = form_data["user-name"];
            string pass = form_data["pass"];
            string first_name = form_data["first-name"];
            string last_name = form_data["last-name"];
            DateTime dob = DateTime.Parse(form_data["dob"]);
            string email = form_data["email"];
            string work_place = form_data["work-place"];
            string address = form_data["address"];
            string phone = form_data["phone"];
            bool gender = bool.Parse(form_data["gender"]);


            User user = new User()
            {
                ID = id,
                Address = address,
                Avatar = "av.png",
                DateCreated = DateTime.Now.Date,
                DateOfBirth = dob,
                Email = email,
                FirstName = first_name,
                LastName = last_name,
                Gender = gender,
                Password = Assitant.Instance.EncodeF64(pass),
                Phone = phone,
                Workplace = work_place,
                UserName = user_name             
            };

            db.Users.Add(user);

            int res = db.SaveChanges();
            if (res > 0)
                return RedirectToAction("Login", "Log");
            else
                return View();
        }
        #endregion

        #region
        public ActionResult SignOut()
        {
            Session["user"] = null;
            return Redirect("/Log/Login");
        }

        #endregion
        // kiểm tra trùng tên
        [HttpPost]
        public JsonResult IsExistUserName(FormCollection form_data)
        {
            string current_id = db.Users.Select(m => m.ID).Max();
            string id = Assitant.Instance.GetAutoID(current_id, "UID");

            string user_name = form_data["user-name"];
            string pass = form_data["pass"];
            string first_name = form_data["first-name"];
            string last_name = form_data["last-name"];
            bool gender = bool.Parse(form_data["gender"]);
            DateTime dob = DateTime.Parse(form_data["dob"]);
            string email = form_data["email"];
            string work_place = form_data["work-place"];
            string address = form_data["address"];
            string phone = form_data["phone"];

            User user = db.Users.Where(m => m.UserName == user_name).FirstOrDefault();

            bool json = false;
            if(user is null)
            {
                json = true;
                User new_user = new User()
                {
                    ID = id,
                    Address = address,
                    Avatar = "av.png",
                    DateCreated = DateTime.Now.Date,
                    DateOfBirth = dob,
                    Email = email,
                    FirstName = first_name,
                    LastName = last_name,
                    Gender = gender,
                    Password = Assitant.Instance.EncodeF64(pass),
                    Phone = phone,
                    Workplace = work_place,
                    UserName = user_name
                };
                db.Users.Add(new_user);
                db.SaveChanges();
            }             
            return Json(json, JsonRequestBehavior.AllowGet);
        }
    }
}