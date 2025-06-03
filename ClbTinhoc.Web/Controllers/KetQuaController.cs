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
            ViewData["MaKhoaHoc"] = new SelectList(_context.KhoaHoc, "MaKhoaHoc", "TenKhoaHoc", ketQua.MaKhoaHoc);
            return View(ketQua);
        }

        // POST: KetQua/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, KetQua ketQuaInput)
        {
            if (id != ketQuaInput.MaKetQua)
            {
                return Json(new { success = false, message = "Không tìm thấy kết quả" });
            }

            var existingKetQua = await _context.KetQua.FindAsync(id);
            if (existingKetQua == null)
            {
                return Json(new { success = false, message = "Không tìm thấy kết quả" });
            }

            // Safely parse DiemCuoiKy from form data and assign to model
            if (double.TryParse(Request.Form["DiemCuoiKy"], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double diemCuoiKy))
            {
                ketQuaInput.DiemCuoiKy = diemCuoiKy;
            }
            else
            {
                ModelState.AddModelError("DiemCuoiKy", "Điểm cuối kỳ không hợp lệ.");
            }

            // Update existing entity with values from the input model
            if (ModelState.IsValid)
            {
                try
                {
                    existingKetQua.MaSinhVien = ketQuaInput.MaSinhVien;
                    existingKetQua.MaKhoaHoc = ketQuaInput.MaKhoaHoc;
                    existingKetQua.DiemCuoiKy = ketQuaInput.DiemCuoiKy; // Use the parsed value
                    existingKetQua.NgayCapNhat = DateTime.Now;

                    _context.Update(existingKetQua);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KetQuaExists(ketQuaInput.MaKetQua))
                    {
                        return Json(new { success = false, message = "Không tìm thấy kết quả" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật" });
                    }
                }
            }

            var errors = ModelState.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToList()
            );
            return Json(new { success = false, message = "Dữ liệu không hợp lệ", errors = errors });
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NhapDiem(string MaSinhVien, int MaKhoaHoc, double DiemCuoiKy)
        {
            try
            {
                var ketQua = await _context.KetQua
                    .FirstOrDefaultAsync(k => k.MaSinhVien == MaSinhVien && k.MaKhoaHoc == MaKhoaHoc);

                if (ketQua == null)
                {
                    ketQua = new KetQua
                    {
                        MaSinhVien = MaSinhVien,
                        MaKhoaHoc = MaKhoaHoc,
                        DiemCuoiKy = DiemCuoiKy,
                        NgayCapNhat = DateTime.Now
                    };
                    _context.KetQua.Add(ketQua);
                }
                else
                {
                    ketQua.DiemCuoiKy = DiemCuoiKy;
                    ketQua.NgayCapNhat = DateTime.Now;
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetDiem(string maSinhVien, int maKhoaHoc)
        {
            var ketQua = await _context.KetQua
                .FirstOrDefaultAsync(k => k.MaSinhVien == maSinhVien && k.MaKhoaHoc == maKhoaHoc);

            if (ketQua != null)
            {
                return Json(new { diemCuoiKy = ketQua.DiemCuoiKy });
            }

            return Json(new { diemCuoiKy = 0 });
        }
    }
}