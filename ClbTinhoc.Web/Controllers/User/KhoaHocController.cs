using ClbTinhoc.Web.Data;
using ClbTinhoc.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using ClbTinhoc.Web.Attributes;

namespace ClbTinhoc.Web.Controllers.User
{
    [RequireLogin]
    [Route("user/[controller]")]
    public class KhoaHocController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<KhoaHocController> _logger;

        public KhoaHocController(ApplicationDbContext context, ILogger<KhoaHocController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: user/KhoaHoc
        public async Task<IActionResult> Index()
        {
            var khoaHocs = await _context.KhoaHoc
                .Include(k => k.KhoaHoc_SinhVien)
                .ThenInclude(ks => ks.SinhVien)
                .Include(k => k.SupportKhoaHoc)
                .ThenInclude(ks => ks.Support)
                .ToListAsync();
            return View(khoaHocs);
        }

        // GET: user/KhoaHoc/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var khoaHoc = await _context.KhoaHoc
                .Include(k => k.KhoaHoc_SinhVien)
                .ThenInclude(ks => ks.SinhVien)
                .Include(k => k.SupportKhoaHoc)
                .ThenInclude(ks => ks.Support)
                .FirstOrDefaultAsync(m => m.MaKhoaHoc == id);

            if (khoaHoc == null)
            {
                return NotFound();
            }

            return View(khoaHoc);
        }
    }
}