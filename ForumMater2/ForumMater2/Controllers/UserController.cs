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
    }
}