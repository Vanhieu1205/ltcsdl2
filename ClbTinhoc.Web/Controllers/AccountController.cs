using ClbTinhoc.Web.Data;
using ClbTinhoc.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using ClbTinhoc.Web.Attributes;
using Microsoft.AspNetCore.Authorization;

namespace ClbTinhoc.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Account/Login
        [AllowAnonymous] // Cho phép truy cập mà không cần đăng nhập
        public IActionResult Login()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous] // Cho phép truy cập mà không cần đăng nhập
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin đăng nhập";
                return View();
            }

            var user = await _context.UserLogin
                .FirstOrDefaultAsync(u => u.studentId == username && u.pass == password);

            if (user == null)
            {
                ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không đúng";
                return View();
            }

            // Validate role
            if (!UserRoles.IsValidRole(user.Role))
            {
                user.Role = UserRoles.User; // Set default role if invalid
                await _context.SaveChangesAsync();
            }

            // Lưu thông tin đăng nhập vào session
            HttpContext.Session.SetString("UserId", user.Id.ToString());
            HttpContext.Session.SetString("Username", user.studentId);
            HttpContext.Session.SetString("UserRole", user.Role);

            return RedirectToAction("Index", "Home");
        }

        // GET: Account/Register
        [AllowAnonymous] // Cho phép truy cập mà không cần đăng nhập
        public IActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous] // Cho phép truy cập mà không cần đăng nhập
        public async Task<IActionResult> Register(UserLogin model)
        {
            if (ModelState.IsValid)
            {
                // Ensure you check for an existing username or email
                var existingUser = await _context.UserLogin
                    .FirstOrDefaultAsync(u => u.username == model.username || u.studentId == model.studentId);

                if (existingUser != null)
                {
                    // Ghi log lỗi ra console/terminal
                    Console.WriteLine($"[ĐĂNG KÝ] Tên đăng nhập hoặc email đã tồn tại: {model.username} / {model.email}");
                    ViewBag.Error = "Tên đăng nhập hoặc email đã tồn tại";
                    return View(model);
                }

                // Gán quyền mặc định là user
                model.Role = UserRoles.User;

                try
                {
                    // Add the new user to the database
                    _context.Add(model);
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    // Ghi log lỗi ra console/terminal
                    Console.WriteLine($"[ĐĂNG KÝ] Lỗi khi lưu tài khoản mới: {ex.Message}");
                    ModelState.AddModelError("", "Có lỗi xảy ra khi đăng ký tài khoản. Vui lòng thử lại hoặc liên hệ quản trị viên.");
                    return View(model);
                }
                return RedirectToAction(nameof(Login));
            }
            else
            {
                // Ghi log lỗi ra console/terminal
                foreach (var entry in ModelState)
                {
                    foreach (var error in entry.Value.Errors)
                    {
                        Console.WriteLine($"[ĐĂNG KÝ] Lỗi trường {entry.Key}: {error.ErrorMessage}");
                    }
                }
            }
            return View(model);
        }

        // GET: Account/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        // GET: Account/AccessDenied
        [AllowAnonymous] // Cho phép truy cập mà không cần đăng nhập
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}