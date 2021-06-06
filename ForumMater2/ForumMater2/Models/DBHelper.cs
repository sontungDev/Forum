using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LIB;

namespace ForumMater2.Models
{
    public class DBHelper
    {
        private static ClubForumEntities db = new ClubForumEntities();
        private DBHelper() { }
        private static DBHelper instance;
        public static DBHelper Instance
        {
            get
            {
                if (instance == null)
                    instance = new DBHelper();
                return instance;
            }
        }

        // kiểm tra tính hợp lệ của tài khoản
        public bool Authentication(string user_name, string password)
        {
            string db_password = db.Users.Where(m => m.UserName == user_name).Select(m => m.Password).FirstOrDefault();

            if (!String.IsNullOrEmpty(db_password))
            {
                string password_decoded = Assitant.Instance.DecodeF64(db_password);

                if (password.Equals(password_decoded))
                    return true;
            }
            return false;
        }

        // lấy user theo id
        public User GetUserById(string id)
        {
            User user = db.Users.Find(id);
            return user;
        }
        public string GetUserId(string user_name)
        {
            string id = db.Users.Where(m => m.UserName == user_name).Select(m => m.ID).FirstOrDefault();
            return id == null ? "" : id;
        }
    }
}