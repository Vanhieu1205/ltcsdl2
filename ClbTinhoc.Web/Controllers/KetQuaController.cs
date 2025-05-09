using ClbTinhoc.Web.Data;
using ClbTinhoc.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ClbTinhoc.Web.Controllers
{
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
            ViewData["MaLopHoc"] = new SelectList(_context.KhoaHoc, "MaLopHoc", "TenLopHoc");
            return View();
        }

        // POST: KetQua/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaKetQua,MaSinhVien,MaLopHoc,DiemCuoiKy,NgayCapNhat")] KetQua ketQua)
        {
            if (ModelState.IsValid)
            {
                ketQua.NgayCapNhat = DateTime.Now;
                _context.Add(ketQua);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaSinhVien"] = new SelectList(_context.SinhVien, "MaSinhVien", "HoTen", ketQua.MaSinhVien);
            ViewData["MaLopHoc"] = new SelectList(_context.KhoaHoc, "MaLopHoc", "TenLopHoc", ketQua.MaKhoaHoc);
            return View(ketQua);
        }

        // GET: KetQua/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ketQua = await _context.KetQua.FindAsync(id);
            if (ketQua == null)
            {
                return NotFound();
            }
            ViewData["MaSinhVien"] = new SelectList(_context.SinhVien, "MaSinhVien", "HoTen", ketQua.MaSinhVien);
            ViewData["MaLopHoc"] = new SelectList(_context.KhoaHoc, "MaLopHoc", "TenLopHoc", ketQua.MaKhoaHoc);
            return View(ketQua);
        }

        // POST: KetQua/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaKetQua,MaSinhVien,MaLopHoc,DiemCuoiKy,NgayCapNhat")] KetQua ketQua)
        {
            if (id != ketQua.MaKetQua)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    ketQua.NgayCapNhat = DateTime.Now;
                    _context.Update(ketQua);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KetQuaExists(ketQua.MaKetQua))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaSinhVien"] = new SelectList(_context.SinhVien, "MaSinhVien", "HoTen", ketQua.MaSinhVien);
            ViewData["MaLopHoc"] = new SelectList(_context.KhoaHoc, "MaLopHoc", "TenLopHoc", ketQua.MaKhoaHoc);
            return View(ketQua);
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