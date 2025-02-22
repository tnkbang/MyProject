﻿using Data.Models;
using Logic.IRepositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Website.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserServices userServices;
        private readonly ReadFileController readFile;

        public UserController(IUserServices userServices)
        {
            this.userServices = userServices;
            this.readFile = new ReadFileController();
        }

        #region Đăng nhập và đăng ký
        [HttpPost]
        public JsonResult CheckUsername(string uname)
        {
            bool check = userServices.HasUsername(uname);

            return Json(check);
        }

        [HttpPost]
        public async Task<JsonResult> Create(string uname, string pass)
        {
            if (string.IsNullOrEmpty(uname) || string.IsNullOrEmpty(pass))
                return Json(new { tt = false, mess = "Thiếu thông tin !" });

            if (userServices.HasUsername(uname))
                return Json(new { tt = false, mess = "Tên người dùng đã tồn tại !" });

            userServices.Create(uname, pass);

            User user = userServices.Details(uname, userServices.AsPassword(uname, pass));
            await SetLogin(user);
            user = userServices.HideSensitive(user);

            return Json(new { tt = true, user = user });
        }
        
        [HttpGet]
        public JsonResult GetFormLoginRegister()
        {
            string body = readFile.ReadHtml("/Data/User/LoginRegister.html");
            return Json(new { body });
        }

        //Xác thực đăng nhập
        private async Task<int> SetLogin(User user)
        {
            //Trạng thái của return:
            // 0 - Email hoặc mật khẩu không chính xác
            // 1 - Tài khoản bị khóa
            // 2 - Đăng nhập thành công
            if (user.Uid != null)
            {
                if (!user.Active)
                {
                    return 1;
                }

                //Tạo list lưu chủ thể đăng nhập
                List<Claim> claims = new List<Claim>() {
                        new Claim("Uid", user.Uid),
                        new Claim("Uname", user.Username)
                    };

                //Tạo Identity để xác thực và xác nhận
                ClaimsIdentity identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                ClaimsPrincipal principal = new ClaimsPrincipal(identity);

                //Gọi hàm đăng nhập bất đồng bộ của HttpContext
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties()
                {
                    //Nhớ tài khoản
                    IsPersistent = true
                });

                return 2;
            }

            return 0;
        }

        //Gọi đăng nhập
        [HttpPost]
        public async Task<IActionResult> GetLogin(string uname, string pass)
        {
            User user = userServices.Details(uname, userServices.AsPassword(uname, pass));
            int login = await SetLogin(user);

            if (login == 2)
            {
                user = userServices.HideSensitive(user);
                return Json(new { tt = true, user = user });
            }
            if (login == 1)
            {
                return Json(new { tt = false, mess = "Tài khoản của bạn bị khóa !" });
            }
            return Json(new { tt = false, mess = "Không tìm thấy tài khoản đăng nhập !" });
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            //Gọi hàm đăng xuất của HttpContext
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return Json(new { tt = true, mess = "Đăng xuất thành công !" });
        }
        #endregion

        #region Thay đổi thông tin

        [HttpPost]
        [Authorize]
        public JsonResult GetProfile()
        {
            User user = userServices.Details(User.Claims.First().Value);
            string body = readFile.ReadProfile("/Data/User/Profile.html", user);
            return Json(new { body });
        }

        [HttpGet]
        public JsonResult GetFormChangeAvt()
        {
            string body = readFile.ReadHtml("/Data/User/ChangeAvt.html");
            return Json(new { body });
        }

        [HttpPost]
        [Authorize]
        public async Task<JsonResult> ChangeAvt(IFormFile img)
        {
            User user = userServices.Details(User.Claims.First().Value);
            string? urlImg = await userServices.SetImages(user.Uid, img);

            user.Image = urlImg;
            userServices.Update(user);

            return Json(new { tt = true });
        }

        [HttpGet]
        [Authorize]
        public JsonResult GetFormChangeProfile()
        {
            User user = userServices.HideSensitive(userServices.Details(User.Claims.First().Value));
            string body = readFile.ChangeProfile("/Data/User/ChangeProfile.html", user);
            return Json(new { body, user.Sex });
        }

        [HttpPost]
        [Authorize]
        public JsonResult ChangeProfile(User uUpdate)
        {
            User user = userServices.Details(User.Claims.First().Value);

            user.FirstName = uUpdate.FirstName;
            user.LastName = uUpdate.LastName;
            user.Sex = uUpdate.Sex;
            user.Birthday = uUpdate.Birthday;
            user.Phone = uUpdate.Phone;
            user.Description = uUpdate.Description;

            userServices.Update(user);

            return Json(new { tt = true });
        }

        #endregion
    }
}
