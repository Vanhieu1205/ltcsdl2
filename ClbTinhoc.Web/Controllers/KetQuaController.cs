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

            var ketQua = await _context.KetQua
                .Include(k => k.SinhVien)
                .Include(k => k.KhoaHoc)
                .FirstOrDefaultAsync(m => m.MaKetQua == id);

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
        public async Task<IActionResult> Edit(int id, [Bind("MaKetQua,MaSinhVien,MaKhoaHoc,DiemCuoiKy")] KetQua ketQua)
        {
            // Check if user is admin
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "admin")
            {
                TempData["ErrorMessage"] = "Bạn không có quyền sửa điểm!";
                return RedirectToAction(nameof(Index));
            }

            if (id != ketQua.MaKetQua)
            {
                return NotFound();
            }

            // Remove validation for MaSinhVien and MaKhoaHoc as they are disabled and fetched from DB
            ModelState.Remove("MaSinhVien");
            ModelState.Remove("MaKhoaHoc");

            // Manually validate DiemCuoiKy
            if (ketQua.DiemCuoiKy < 0 || ketQua.DiemCuoiKy > 10)
            {
                ModelState.AddModelError("DiemCuoiKy", "Điểm cuối kỳ phải từ 0 đến 10.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingKetQua = await _context.KetQua.FindAsync(id);
                    if (existingKetQua == null)
                    {
                        TempData["ErrorMessage"] = "Không tìm thấy kết quả học tập!";
                        return RedirectToAction(nameof(Index));
                    }

                    // Update only the DiemCuoiKy and NgayCapNhat properties
                    existingKetQua.DiemCuoiKy = ketQua.DiemCuoiKy;
                    existingKetQua.NgayCapNhat = DateTime.Now;

                    _context.Update(existingKetQua);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật kết quả thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KetQuaExists(ketQua.MaKetQua))
                    {
                        TempData["ErrorMessage"] = "Không tìm thấy kết quả học tập!";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật kết quả!";
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
                    return RedirectToAction(nameof(Index));
                }
            }

            // If we got this far, something failed, redisplay form
            var modelForView = await _context.KetQua
                .Include(k => k.SinhVien)
                .Include(k => k.KhoaHoc)
                .FirstOrDefaultAsync(m => m.MaKetQua == id);

            if (modelForView == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy kết quả học tập!";
                return RedirectToAction(nameof(Index));
            }

            // Update the DiemCuoiKy on the model sent to the view
            modelForView.DiemCuoiKy = ketQua.DiemCuoiKy;

            return View(modelForView);
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