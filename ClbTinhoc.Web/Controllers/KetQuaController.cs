using ClbTinhoc.Web.Data;
using ClbTinhoc.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using ClbTinhoc.Web.Attributes;

namespace ClbTinhoc.Web.Controllers
{
    [RequireLogin]
    public class KetQuaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public KetQuaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: KetQua
        public async Task<IActionResult> Index()
        {
            var ketQuas = await _context.KetQua
                .Include(k => k.SinhVien)
                .Include(k => k.KhoaHoc)
                .ToListAsync();
            return View(ketQuas);
        }

        // GET: KetQua/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ketQua = await _context.KetQua
                .Include(k => k.SinhVien)
                .Include(k => k.KhoaHoc)
                .FirstOrDefaultAsync(m => m.MaKetQua == id);

            if (ketQua == null)
            {
                return NotFound();
            }

            return View(ketQua);
        }

        // GET: KetQua/Create
        public IActionResult Create()
        {
            ViewData["MaSinhVien"] = new SelectList(_context.SinhVien, "MaSinhVien", "HoTen");
            ViewData["MaKhoaHoc"] = new SelectList(_context.KhoaHoc, "MaKhoaHoc", "TenKhoaHoc");
            return View();
        }

        // POST: KetQua/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(KetQua ketQua)
        {
            if (ModelState.IsValid)
            {
                ketQua.NgayCapNhat = DateTime.Now;
                _context.Add(ketQua);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaSinhVien"] = new SelectList(_context.SinhVien, "MaSinhVien", "HoTen", ketQua.MaSinhVien);
            ViewData["MaKhoaHoc"] = new SelectList(_context.KhoaHoc, "MaKhoaHoc", "TenKhoaHoc", ketQua.MaKhoaHoc);
            return View(ketQua);
        }

        // GET: KetQua/Edit?maSinhVien=...&maKhoaHoc=...
        public async Task<IActionResult> Edit(string maSinhVien, int maKhoaHoc)
        {
            var diemThi = await _context.DiemThi
                .FirstOrDefaultAsync(d => d.MaSinhVien == maSinhVien && d.MaKhoaHoc == maKhoaHoc);
            if (diemThi == null)
            {
                diemThi = new DiemThi
                {
                    MaSinhVien = maSinhVien,
                    MaKhoaHoc = maKhoaHoc,
                    LanThi = 1
                };
            }
            ViewBag.TenSinhVien = (await _context.SinhVien.FindAsync(maSinhVien))?.HoTen;
            ViewBag.TenKhoaHoc = (await _context.KhoaHoc.FindAsync(maKhoaHoc))?.TenKhoaHoc;
            return View(diemThi);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(DiemThi model)
        {
            if (ModelState.IsValid)
            {
                var diemThi = await _context.DiemThi
                    .FirstOrDefaultAsync(d => d.MaSinhVien == model.MaSinhVien && d.MaKhoaHoc == model.MaKhoaHoc);
                if (diemThi == null)
                {
                    _context.DiemThi.Add(model);
                }
                else
                {
                    diemThi.Diem = model.Diem;
                    diemThi.LanThi = model.LanThi;
                }
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "KetQua");
            }
            ViewBag.TenSinhVien = (await _context.SinhVien.FindAsync(model.MaSinhVien))?.HoTen;
            ViewBag.TenKhoaHoc = (await _context.KhoaHoc.FindAsync(model.MaKhoaHoc))?.TenKhoaHoc;
            return View(model);
        }

        // GET: KetQua/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ketQua = await _context.KetQua
                .Include(k => k.SinhVien)
                .Include(k => k.KhoaHoc)
                .FirstOrDefaultAsync(m => m.MaKetQua == id);
            if (ketQua == null)
            {
                return NotFound();
            }

            return View(ketQua);
        }

        // POST: KetQua/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ketQua = await _context.KetQua.FindAsync(id);
            if (ketQua == null)
            {
                return NotFound();
            }
            _context.KetQua.Remove(ketQua);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool KetQuaExists(int id)
        {
            return _context.KetQua.Any(e => e.MaKetQua == id);
        }
    }
}