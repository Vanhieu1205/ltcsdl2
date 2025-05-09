using ClbTinhoc.Web.Data;
using ClbTinhoc.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ClbTinhoc.Web.Controllers
{
    public class SinhVienController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SinhVienController> _logger;

        public SinhVienController(ApplicationDbContext context, ILogger<SinhVienController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: SinhVien
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Đang tải danh sách sinh viên");
                var sinhViens = await _context.SinhVien.ToListAsync();
                _logger.LogInformation($"Đã tải {sinhViens.Count} sinh viên");
                return View(sinhViens);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải danh sách sinh viên");
                return View(new List<SinhVien>());
            }
        }

        // GET: SinhVien/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: SinhVien/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaSinhVien,HoTen,LopSinhHoat,Email,SoDienThoai,NgayThamGia")] SinhVien sinhVien)
        {
            try
            {
                _logger.LogInformation($"Đang thêm sinh viên: {sinhVien.MaSinhVien} - {sinhVien.HoTen}");

                if (ModelState.IsValid)
                {
                    _logger.LogInformation("ModelState hợp lệ, đang thêm vào database");
                    _context.Add(sinhVien);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Đã thêm sinh viên thành công");
                    return RedirectToAction(nameof(Index));
                }

                _logger.LogWarning("ModelState không hợp lệ");
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        _logger.LogWarning($"Lỗi: {state.Key} - {error.ErrorMessage}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm sinh viên");
                ModelState.AddModelError("", $"Lỗi: {ex.Message}");
            }

            return View(sinhVien);
        }

        // GET: SinhVien/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sinhVien = await _context.SinhVien
                .FirstOrDefaultAsync(m => m.MaSinhVien == id);
            if (sinhVien == null)
            {
                return NotFound();
            }

            return View(sinhVien);
        }

        // GET: SinhVien/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sinhVien = await _context.SinhVien.FindAsync(id);
            if (sinhVien == null)
            {
                return NotFound();
            }
            return View(sinhVien);
        }

        // POST: SinhVien/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("MaSinhVien,HoTen,LopSinhHoat,Email,SoDienThoai,NgayThamGia")] SinhVien sinhVien)
        {
            if (id != sinhVien.MaSinhVien)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sinhVien);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SinhVienExists(sinhVien.MaSinhVien))
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
            return View(sinhVien);
        }

        // GET: SinhVien/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sinhVien = await _context.SinhVien
                .FirstOrDefaultAsync(m => m.MaSinhVien == id);
            if (sinhVien == null)
            {
                return NotFound();
            }

            return View(sinhVien);
        }

        // POST: SinhVien/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var sinhVien = await _context.SinhVien.FindAsync(id);
            if (sinhVien != null)
            {
                _context.SinhVien.Remove(sinhVien);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SinhVienExists(string id)
        {
            return _context.SinhVien.Any(e => e.MaSinhVien == id);
        }
    }
}