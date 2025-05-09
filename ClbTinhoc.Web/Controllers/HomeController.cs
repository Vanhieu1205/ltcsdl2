using ClbTinhoc.Web.Data;
using ClbTinhoc.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using ClbTinhoc.Web.Attributes;
using Microsoft.AspNetCore.Authorization;

namespace ClbTinhoc.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var khoaHocs = await _context.KhoaHoc
                .OrderByDescending(k => k.NgayBatDau)
                .Take(5)
                .ToListAsync();
            return View(khoaHocs);
        }

        [RequireLogin]
        [RequireAdmin]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}