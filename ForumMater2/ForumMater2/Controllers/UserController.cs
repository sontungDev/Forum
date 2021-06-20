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
        public ActionResult Home(int page = 1, int size = 5)
        {
            if(Session["user"] != null)
            {
                string user_id = Session["user"].ToString();
                User user = db.Users.Find(user_id);

                // lấy user đang sử dụng
                ViewBag.User = user;

                ViewBag.Url = UrlContext();
                ViewBag.UrlAva = UrlContext() + "/assets/images/users/avatars";
                //IEnumerable<Post> list_post = db.Posts.Where(m => m.Approval != "AID0" && m.cl).OrderByDescending(m => m.DateTimeCreated).ToPagedList(page, size);
                List<Club> clubs_considered = db.UserClubRoles.Where(m => m.Role >= 2 && m.UserID == user_id).Select(m => m.Club).ToList();
                List<Post> posts = new List<Post>();
                foreach(var item in clubs_considered)
                {
                    foreach(var child_item in item.Posts)
                    {
                        posts.Add(child_item);
                    }
                }
                IEnumerable<Post> result = posts.Where(m => m.Approval != "AID0").OrderByDescending(m => m.DateTimeCreated).ToPagedList(page, size);
                return View(result);
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
                List<Club> clubs_considered = db.UserClubRoles.Where(m => m.UserID == user_id && m.Role >= 2).Select(m => m.Club).ToList();
                ViewBag.Url = UrlContext();
                return View(clubs_considered);
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
                Approval = "AID0"
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
            string approval = "AID0";
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

        public ActionResult SeeProfile(string id)
        {
            if(Session["user"] != null)
            {
                User user = db.Users.Find(id);
                ViewBag.Url = UrlContext();
                return View(user);
            }
            return Redirect("/Log/Login");
        }    
        [HttpPost]
        // dùng để cập nhập ảnh đại diện người dùng
        public JsonResult MyProfileA(FormCollection form_data)
        {
            string imgbase64 = form_data["avatar"];
            byte[] bytes = Convert.FromBase64String(imgbase64.Split(',')[1]);

            // lấy tên ảnh
            string name_file = Guid.NewGuid() + ".png";

            // ghi file vào máy chủ
            FileStream stream = new FileStream(Server.MapPath("~/assets/images/users/avatars/" + name_file), FileMode.Create);
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
            string name_file = Guid.NewGuid() + ".png";

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
                Approval = "AID0",
                CoverPhoto = name_file,
                DateCreated = DateTime.Now.Date,
                Description = describe,
                Name = name,
                ShortName = short_name,
                UserCreated = user_id,
                Type = type_club
            };

            db.Clubs.Add(club);

            int res = db.SaveChanges();
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

        public ActionResult NotJoinedClubs()
        {
            if(Session["user"] != null)
            {
                ViewBag.Url = UrlContext();
                return View();
            }    
            return Redirect("/Log/Login");
        }
        #endregion

        // trang chi tiết câu lạc bộ
        #region
        // trang cho quản trưởng clb
        public ActionResult ClubDetail(string id, int page = 1, int size = 10)
        {
            if(Session["user"] != null)
            {
                string user_id = Session["user"].ToString();
                ViewBag.Url = UrlContext();
                Club club = db.Clubs.Find(id);
                int roles = db.UserClubRoles.Where(m => m.ClubID == id && m.UserID == user_id).Select(m => m.Role).FirstOrDefault();
                IEnumerable<Post> posts = db.Posts.Where(m => m.ClubID == id && m.Approval != "AID0")
                    .OrderByDescending(m => m.DateTimeCreated).ToPagedList(page, size);
                ViewBag.ClubID = id;
                return View(posts);
            }
            return Redirect("/Log/Login");
        }
        // hiển thị toàn bộ CLB
        public ActionResult AllClubs()
        {
            if(Session["user"] != null)
            {
                ViewBag.Url = UrlContext();
                List<Club> clubs = db.Clubs.Where(m => m.Approval != "AID0").ToList();
                return View(clubs);
            }
            return Redirect("/Log/Login");
        }

        // trang edit clb
        public ActionResult EditClub(string id) // id của club
        {
            if(Session["user"] != null)
            {
                string user_id = Session["user"].ToString();
                UserClubRole userClubRole = db.UserClubRoles.Where(m => m.ClubID == id && m.UserID == user_id && m.Role >= 3).FirstOrDefault();
                if (userClubRole != null)
                {
                    Club club = userClubRole.Club;
                    return View(club);
                }
                else
                {
                    return RedirectToAction("Home", "User");
                }
            }
            else
            {
                return Redirect("/Log/Login");
            }
             
        }
        [HttpPost]
        public JsonResult EditClub(FormCollection form_data) // id của club
        {
            string name = form_data["long_name"];
            string short_name = form_data["short_name"];
            string describe = form_data["describe"];
            string type_club = form_data["type_club"];
            string club_id = form_data["id"];
            Club club = db.Clubs.Find(club_id);

            club.Name = name;
            club.ShortName = short_name;
            club.Description = describe;
            club.Type = type_club;


            if (form_data["cover"] != null)
            {
                string imgbase64 = form_data["cover"];


                byte[] bytes = Convert.FromBase64String(imgbase64.Split(',')[1]);

                // lấy tên ảnh
                string name_file = Guid.NewGuid() + ".png";

                // ghi file vào máy chủ
                FileStream stream = new FileStream(Server.MapPath("~/assets/images/clubs/covers/" + name_file), FileMode.Create);
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush();

                club.CoverPhoto = name_file;
            }

            db.SaveChanges();
            return Json("xong", JsonRequestBehavior.AllowGet);
        }

        #endregion

        // xử lý rời , gia nhập câu lạc bộ
        #region
        // xử lý rời câu lạc bộ
        [HttpPost]
        public JsonResult LeaveClub(string user_id, string club_id)
        {
            UserClubRole userClubRole = db.UserClubRoles.Where(m => m.ClubID == club_id && m.UserID == user_id).FirstOrDefault();

            db.UserClubRoles.Remove(userClubRole);

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

            if(userClubRole == null)
            {
                userClubRole = new UserClubRole()
                {
                    ClubID = club_id,
                    UserID = user_id,
                    DateTimeJoined = DateTime.Now,
                    Role = 1
                };
                db.UserClubRoles.Add(userClubRole);
            }
            else
            {
                userClubRole.Role = 1;
            }
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

            db.UserClubRoles.Remove(userClubRole);

            int res = db.SaveChanges();
            if (res > 0)
            {
                return Json("Thành công", JsonRequestBehavior.AllowGet);
            }
            return Json("Thất bại", JsonRequestBehavior.AllowGet);
        }

        #endregion

        // thành viên
        #region
        // duyệt thành viên
        public ActionResult VerifyMembers(string id)
        {
            if(Session["user"] != null)
            {
                Club club = db.Clubs.Find(id);

                ViewBag.Url = UrlContext();
                return View(club);
            }
            return Redirect("/Log/Login");
        }
        [HttpPost]
        public JsonResult VerifyMembers(string user_id, string club_id, bool result)
        {
            UserClubRole userClubRole = db.UserClubRoles.Where(m => m.UserID == user_id && m.ClubID == club_id).FirstOrDefault();

            if (result)
            {
                userClubRole.Role = 2;
            }
            else
            {
                db.UserClubRoles.Remove(userClubRole);
            }
            db.SaveChanges();

            return Json("Thành công", JsonRequestBehavior.AllowGet);
        }

        // xóa thành viên
        [HttpPost]
        public JsonResult RemoveRemember(string cid, string uid)
        {
            UserClubRole clubRole = db.UserClubRoles.Where(m => m.UserID == uid && m.ClubID == cid).FirstOrDefault();

            db.UserClubRoles.Remove(clubRole);

            int res = db.SaveChanges();

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        #endregion

        // kế hoạch
        #region
        public ActionResult CreatePlan(string id)
        {
            if(Session["user"] != null)
            {
                ViewBag.ID = id;
                return View();
            }
            return Redirect("/Log/Login");
        }
        [HttpPost]
        public ActionResult CreatePlan(FormCollection form_data)
        {
            string current_id = db.Plans.Select(m => m.ID).Max();
            string id = Assitant.Instance.GetAutoID(current_id, "KID");

            string title = form_data["title"];
            string content = form_data["content"];
            DateTime start = DateTime.Parse(form_data["start-datetime"]);
            string club_id = form_data["club-id"];

            Plan plan = new Plan()
            {
                ID = id,
                ClubID = club_id,
                Content = content,
                Title = title,
                DateTimeExpected = start
            };

            db.Plans.Add(plan);
            db.SaveChanges();
            
            return Redirect("/User/ClubDetail/" + club_id);
        }
        [HttpGet]
        public ActionResult RemovePlan(string id)
        {
            Plan plan = db.Plans.Find(id);
            string club_id = plan.ClubID;

            db.Plans.Remove(plan);

            db.SaveChanges();

            return Redirect("/User/ClubDetail/" + club_id);
        }
        public ActionResult EditPlan(string id)
        {
            if(Session["user"] != null)
            {
                Plan plan = db.Plans.Find(id);

                return View(plan);
            }
            return Redirect("/Log/Login");
        }
        [HttpPost]
        public ActionResult EditPlan(FormCollection form_data)
        {
            string id = form_data["plan-id"];

            Plan plan = db.Plans.Find(id);

            string title = form_data["title"];
            string content = form_data["content"];
            DateTime date_expected = DateTime.Parse(form_data["start-datetime"]);

            plan.Title = title;
            plan.Content = content;
            plan.DateTimeExpected = date_expected;

            db.SaveChanges();

            return Redirect("/User/ClubDetail/" + plan.ClubID);
        }
        #endregion

        // tìm kiếm
        #region
        [HttpGet]
        public ActionResult Search(string key_word)
        {
            ViewBag.key_word = key_word;
            List<User> users = db.Users.Where(m => m.FirstName.Contains(key_word) || m.LastName.Contains(key_word)).ToList();
            ViewBag.Url = UrlContext();
            if (users.Count == 0)
                ViewBag.message = "Không có dữ liệu phù hợp";
            return View(users);
        }
        [HttpGet]

        public ActionResult SearchClub(string key_word)
        {
            ViewBag.key_word = key_word;
            List<Club> clubs = db.Clubs.Where(m => m.Name.Contains(key_word) || m.ShortName.Contains(key_word)).ToList();
            ViewBag.Url = UrlContext();
            if (clubs.Count == 0)
                ViewBag.message = "Không có dữ liệu phù hợp";
            return View(clubs);
        }
        [HttpGet]
        public ActionResult SearchPost(string key_word)
        {
            ViewBag.key_word = key_word;
            List<Post> posts = db.Posts.Where(m => m.Title.Contains(key_word) || m.Hashtag.Contains(key_word)).ToList();
            ViewBag.Url = UrlContext();
            if (posts.Count == 0)
                ViewBag.message = "Không có dữ liệu phù hợp";
            return View(posts);
        }

        #endregion

        // thay đổi password
        [HttpPost]
        public JsonResult ChangePassword(FormCollection form_data)
        {
            bool json = false;
            if (Session["user"] != null)
            {
                string user_id = Session["user"].ToString();
                User user = db.Users.Find(user_id);

                string db_pass = Assitant.Instance.DecodeF64(user.Password);
                string current_pass = form_data["current-pass"];
                string new_pass = form_data["new-pass"];


                if (db_pass.Equals(current_pass))
                {
                    json = true;

                    user.Password = Assitant.Instance.EncodeF64(new_pass);

                    db.SaveChanges();

                }           
            }
            return Json(json, JsonRequestBehavior.AllowGet);
        }


        public ActionResult ForgetPassWord()
        {
            return View();
        }
        // gửi mail
        [HttpPost]
        public JsonResult SendPassToEmail(string user_name)
        {
            User user = db.Users.Where(m => m.UserName == user_name).FirstOrDefault();
            bool response = false;
            if (user != null)
            {

                string email_form = "tung.ns.60cntt@ntu.edu.vn";
                string password = "U3QxMTAyMjAxMEF1QWdDdUZl";
                System.Net.Mail.MailMessage mail = new System.Net.Mail.MailMessage();
                mail.From = new System.Net.Mail.MailAddress(email_form);
                mail.To.Add(user.Email);
                mail.Subject = "Diễn đàn CLB - Đại học Nha Trang";
                mail.Body = "Mật khẩu hiện tại của bạn : " + Assitant.Instance.DecodeF64(user.Password);
                mail.IsBodyHtml = true;
                System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient("smtp.gmail.com", 587);
                smtp.Credentials = new System.Net.NetworkCredential(email_form, Assitant.Instance.DecodeF64(password));
                smtp.EnableSsl = true;
                smtp.Send(mail);

                response = true;
            }
            return Json(response, JsonRequestBehavior.AllowGet);
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