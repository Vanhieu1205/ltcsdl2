using ClbTinhoc.Web.Data;
using ClbTinhoc.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ClbTinhoc.Web.Controllers
{
    public class SupportController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SupportController> _logger;

        public SupportController(ApplicationDbContext context, ILogger<SupportController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Support
        public async Task<IActionResult> Index()
        {
            var supports = await _context.Support.ToListAsync();
            return View(supports);
        }

        // GET: Support/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Support/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaSupport,HoTen,Email,LopSinhHoat,SoDienThoai")] Support support, IFormFile HinhAnh)
        {
            try
            {
                // ✅ Xóa validation tự động cho HinhAnh (vì không có binding từ form cho string HinhAnh)
                ModelState.Remove("HinhAnh");

                if (ModelState.IsValid)
                {
                    if (HinhAnh != null && HinhAnh.Length > 0)
                    {
                        string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        string fileName = Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + Path.GetExtension(HinhAnh.FileName);
                        string filePath = Path.Combine(uploadsFolder, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await HinhAnh.CopyToAsync(stream);
                        }

                        support.HinhAnh = fileName;
                    }
                    else
                    {
                        // ❗️Nếu bạn muốn bắt buộc phải có ảnh, thêm dòng này
                        ModelState.AddModelError("HinhAnh", "Vui lòng chọn hình ảnh.");
                        return View(support);
                    }

                    _context.Add(support);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }

                // Ghi log lỗi nếu có
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    _logger.LogError($"Validation error: {error.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating support");
                ModelState.AddModelError("", "Có lỗi xảy ra khi tạo mới. Vui lòng thử lại.");
            }

            return View(support);
        }


        // GET: Support/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var support = await _context.Support.FindAsync(id);
            if (support == null)
            {
                return NotFound();
            }
            return View(support);
        }

        // POST: Support/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("MaSupport,HoTen,Email,LopSinhHoat,SoDienThoai,HinhAnh")] Support support)
        {
            if (id != support.MaSupport)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(support);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SupportExists(support.MaSupport))
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
            return View(support);
        }

        // GET: Support/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var support = await _context.Support
                .FirstOrDefaultAsync(m => m.MaSupport == id);
            if (support == null)
            {
                return NotFound();
            }

            return View(support);
        }

        // POST: Support/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var support = await _context.Support.FindAsync(id);
            _context.Support.Remove(support);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SupportExists(string id)
        {
            return _context.Support.Any(e => e.MaSupport == id);
        }
    }
}
