using ClbTinhoc.Web.Data;
using ClbTinhoc.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace ClbTinhoc.Web.Controllers
{
    public class DiemThiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DiemThiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Hiển thị danh sách điểm thi
        public async Task<IActionResult> Index()
        {
            var list = await _context.DiemThi
                .Include(d => d.SinhVien)
                .Include(d => d.KhoaHoc)
                .OrderBy(d => d.MaKhoaHoc).ThenBy(d => d.MaSinhVien).ThenBy(d => d.LanThi)
                .ToListAsync();
            return View(list);
        }

        // GET: DiemThi/Edit?maSinhVien=...&maKhoaHoc=...&lanThi=...
        public async Task<IActionResult> Edit(string maSinhVien, int maKhoaHoc, int? lanThi)
        {
            var list = await _context.DiemThi
                .Where(d => d.MaSinhVien == maSinhVien && d.MaKhoaHoc == maKhoaHoc)
                .OrderBy(d => d.LanThi)
                .ToListAsync();

            DiemThi model = null;
            if (lanThi.HasValue)
            {
                model = list.FirstOrDefault(d => d.LanThi == lanThi.Value);
            }
            if (model == null)
            {
                model = new DiemThi { MaSinhVien = maSinhVien, MaKhoaHoc = maKhoaHoc, LanThi = 1 };
            }
            ViewBag.TenSinhVien = (await _context.SinhVien.FindAsync(maSinhVien))?.HoTen;
            ViewBag.TenKhoaHoc = (await _context.KhoaHoc.FindAsync(maKhoaHoc))?.TenKhoaHoc;
            ViewBag.LanThiList = list;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(DiemThi model)
        {
            if (ModelState.IsValid)
            {
                var diemThi = await _context.DiemThi
                    .FirstOrDefaultAsync(d => d.MaSinhVien == model.MaSinhVien && d.MaKhoaHoc == model.MaKhoaHoc && d.LanThi == model.LanThi);
                if (diemThi == null)
                {
                    _context.DiemThi.Add(model);
                }
                else
                {
                    diemThi.Diem = model.Diem;
                }
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            // Nếu lỗi, load lại danh sách lần thi
            var list = await _context.DiemThi
                .Where(d => d.MaSinhVien == model.MaSinhVien && d.MaKhoaHoc == model.MaKhoaHoc)
                .OrderBy(d => d.LanThi)
                .ToListAsync();
            ViewBag.TenSinhVien = (await _context.SinhVien.FindAsync(model.MaSinhVien))?.HoTen;
            ViewBag.TenKhoaHoc = (await _context.KhoaHoc.FindAsync(model.MaKhoaHoc))?.TenKhoaHoc;
            ViewBag.LanThiList = list;
            return View(model);
        }

        // Xóa điểm thi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string maSinhVien, int maKhoaHoc, int lanThi)
        {
            var diemThi = await _context.DiemThi
                .FirstOrDefaultAsync(d => d.MaSinhVien == maSinhVien && d.MaKhoaHoc == maKhoaHoc && d.LanThi == lanThi);
            if (diemThi != null)
            {
                _context.DiemThi.Remove(diemThi);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}