using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ForumMater2.Models;

namespace ForumMater2.Controllers
{
    public class LogController : XController
    {
        // Đăng nhập
        #region
        public ActionResult Login()
        {
            ViewBag.Url = UrlContext();
            return View();
        }
        [HttpPost]
        public ActionResult Login(FormCollection form_data)
        {
            // lấy dữ liệu từ form sau khi submit
            string user_name = form_data["user-name"];
            string password = form_data["password"];
            string remember = form_data["remember-user"];

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
                return RedirectToAction("Home", "User");
            }
            else
                return Content("Fail");
        }
        #endregion

        // Đăng ký
        #region
        public ActionResult Register()
        {
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
    }
}