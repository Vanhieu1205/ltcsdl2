using ClbTinhoc.Web.Data;
using ClbTinhoc.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClbTinhoc.Web.Attributes;

namespace ClbTinhoc.Web.Controllers
{
    [RequireLogin]
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
        public async Task<IActionResult> Edit(string id, [Bind("MaSupport,HoTen,Email,LopSinhHoat,SoDienThoai,HinhAnh")] Support support, IFormFile imageFile)
        {
            // Check if user is admin
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "admin")
            {
                TempData["ErrorMessage"] = "Bạn không có quyền sửa thông tin Support!";
                return RedirectToAction(nameof(Index));
            }

            if (id != support.MaSupport)
            {
                TempData["ErrorMessage"] = "Không tìm thấy Support cần sửa.";
                return RedirectToAction(nameof(Index));
            }

            // Retain existing image path if no new file is uploaded
            if (imageFile == null)
            {
                // Need to fetch the existing support to get the current HinhAnh value
                var existingSupport = await _context.Support.AsNoTracking().FirstOrDefaultAsync(s => s.MaSupport == id);
                if (existingSupport != null)
                {
                    support.HinhAnh = existingSupport.HinhAnh;
                }
            }
            // Remove validation for HinhAnh if a new file is uploaded, as it's handled manually
            else
            {
                ModelState.Remove("HinhAnh");
            }


            if (ModelState.IsValid)
            {
                try
                {
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                        var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
                        if (!allowedExtensions.Contains(extension))
                        {
                            ModelState.AddModelError("imageFile", "Chỉ chấp nhận file .jpg, .jpeg, .png.");
                            return View(support);
                        }

                        if (imageFile.Length > 5 * 1024 * 1024)
                        {
                            ModelState.AddModelError("imageFile", "Kích thước file không được vượt quá 5MB.");
                            return View(support);
                        }

                        string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        // Sanitize file name based on Support ID or generate a new one
                        string fileName = Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + extension;
                        string filePath = Path.Combine(uploadsFolder, fileName);

                        // Ensure unique filename
                        int counter = 1;
                        while (System.IO.File.Exists(filePath))
                        {
                            fileName = $"{Path.GetFileNameWithoutExtension(imageFile.FileName)}_{counter}{extension}";
                            filePath = Path.Combine(uploadsFolder, fileName);
                            counter++;
                        }

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }

                        support.HinhAnh = fileName;
                    }

                    _context.Update(support);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật thông tin Support thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SupportExists(support.MaSupport))
                    {
                        TempData["ErrorMessage"] = "Không tìm thấy Support khi cập nhật.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Lỗi đồng thời khi cập nhật Support.";
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating Support");
                    TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật thông tin Support: " + ex.Message;
                    return RedirectToAction(nameof(Index));
                }
            }

            // If we got this far, something failed, redisplay form
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
