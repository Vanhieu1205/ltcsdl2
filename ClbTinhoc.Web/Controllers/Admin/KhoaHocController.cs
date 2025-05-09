using ClbTinhoc.Web.Data;
using ClbTinhoc.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ClbTinhoc.Web.Attributes;

namespace ClbTinhoc.Web.Controllers.Admin
{
    [RequireLogin]
    [RequireAdmin]
    [Route("admin/[controller]")]
    public class KhoaHocController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<KhoaHocController> _logger;

        public KhoaHocController(ApplicationDbContext context, ILogger<KhoaHocController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: admin/KhoaHoc
        public async Task<IActionResult> Index()
        {
            return View(await _context.KhoaHoc.ToListAsync());
        }

        // GET: admin/KhoaHoc/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: admin/KhoaHoc/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TenKhoaHoc,MoTa,NgayBatDau,NgayKetThuc")] KhoaHoc khoaHoc, IFormFile image)
        {
            try
            {
                ModelState.Remove("image");

                if (image == null || image.Length == 0)
                {
                    ModelState.AddModelError("image", "Hình ảnh là bắt buộc.");
                    _logger.LogWarning("No image file uploaded.");
                    return View(khoaHoc);
                }

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("image", "Chỉ chấp nhận file .jpg, .jpeg, .png.");
                    _logger.LogWarning($"Invalid file extension: {extension}");
                    return View(khoaHoc);
                }

                if (image.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("image", "Kích thước file không được vượt quá 5MB.");
                    _logger.LogWarning($"File size too large: {image.Length} bytes");
                    return View(khoaHoc);
                }

                string imagesFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                if (!Directory.Exists(imagesFolder))
                {
                    Directory.CreateDirectory(imagesFolder);
                }

                string sanitizedFileName = SanitizeFileName(khoaHoc.TenKhoaHoc);
                string fileName = sanitizedFileName + extension;
                string filePath = Path.Combine(imagesFolder, fileName);

                int counter = 1;
                while (System.IO.File.Exists(filePath))
                {
                    fileName = $"{sanitizedFileName}_{counter}{extension}";
                    filePath = Path.Combine(imagesFolder, fileName);
                    counter++;
                }

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                khoaHoc.image = fileName;

                if (!ModelState.IsValid)
                {
                    foreach (var entry in ModelState)
                    {
                        foreach (var error in entry.Value.Errors)
                        {
                            _logger.LogWarning($"ModelState Error - {entry.Key}: {error.ErrorMessage}");
                        }
                    }
                    return View(khoaHoc);
                }

                _logger.LogInformation("Saving KhoaHoc to database...");
                _context.Add(khoaHoc);
                await _context.SaveChangesAsync();

                _logger.LogInformation("KhoaHoc saved successfully.");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating KhoaHoc: {ex.Message}\nStackTrace: {ex.StackTrace}");
                ModelState.AddModelError("", "Có lỗi xảy ra khi thêm khóa học. Vui lòng thử lại.");
                return View(khoaHoc);
            }
        }

        private string SanitizeFileName(string fileName)
        {
            string invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);
            string sanitized = Regex.Replace(fileName, invalidRegStr, "_");
            sanitized = sanitized.Replace(" ", "_");
            if (sanitized.Length > 100) sanitized = sanitized.Substring(0, 100);
            return sanitized;
        }

        // GET: admin/KhoaHoc/Details/5
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
                _logger.LogWarning($"KhoaHoc with ID {id} not found.");
                return NotFound();
            }

            var enrolledStudentIds = khoaHoc.KhoaHoc_SinhVien?.Select(ks => ks.MaSinhVien).ToList() ?? new List<string>();
            var availableStudents = await _context.SinhVien
                .Where(s => !enrolledStudentIds.Contains(s.MaSinhVien))
                .OrderBy(s => s.HoTen)
                .Select(s => new { s.MaSinhVien, s.HoTen })
                .ToListAsync();

            var enrolledSupportIds = khoaHoc.SupportKhoaHoc?.Select(ks => ks.MaSupport).ToList() ?? new List<string>();
            var availableSupports = await _context.Support
                .Where(s => !enrolledSupportIds.Contains(s.MaSupport))
                .OrderBy(s => s.HoTen)
                .Select(s => new { s.MaSupport, s.HoTen })
                .ToListAsync();

            ViewBag.AvailableStudents = availableStudents;
            ViewBag.AvailableSupports = availableSupports;

            return View(khoaHoc);
        }

        // GET: admin/KhoaHoc/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var khoaHoc = await _context.KhoaHoc.FindAsync(id);
            if (khoaHoc == null)
            {
                return NotFound();
            }
            return View(khoaHoc);
        }

        // POST: admin/KhoaHoc/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaKhoaHoc,TenKhoaHoc,MoTa,NgayBatDau,NgayKetThuc,image")] KhoaHoc khoaHoc, IFormFile ImageFile)
        {
            if (id != khoaHoc.MaKhoaHoc)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                        var extension = Path.GetExtension(ImageFile.FileName).ToLowerInvariant();
                        if (!allowedExtensions.Contains(extension))
                        {
                            ModelState.AddModelError("ImageFile", "Chỉ chấp nhận file .jpg, .jpeg, .png.");
                            return View(khoaHoc);
                        }

                        if (ImageFile.Length > 5 * 1024 * 1024)
                        {
                            ModelState.AddModelError("ImageFile", "Kích thước file không được vượt quá 5MB.");
                            return View(khoaHoc);
                        }

                        string imagesFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                        if (!Directory.Exists(imagesFolder))
                        {
                            Directory.CreateDirectory(imagesFolder);
                        }

                        string sanitizedFileName = SanitizeFileName(khoaHoc.TenKhoaHoc);
                        string fileName = sanitizedFileName + extension;
                        string filePath = Path.Combine(imagesFolder, fileName);

                        int counter = 1;
                        while (System.IO.File.Exists(filePath))
                        {
                            fileName = $"{sanitizedFileName}_{counter}{extension}";
                            filePath = Path.Combine(imagesFolder, fileName);
                            counter++;
                        }

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await ImageFile.CopyToAsync(stream);
                        }

                        khoaHoc.image = fileName;
                    }

                    _context.Update(khoaHoc);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KhoaHocExists(khoaHoc.MaKhoaHoc))
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
            return View(khoaHoc);
        }

        // GET: admin/KhoaHoc/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var khoaHoc = await _context.KhoaHoc
                .FirstOrDefaultAsync(m => m.MaKhoaHoc == id);
            if (khoaHoc == null)
            {
                return NotFound();
            }

            return View(khoaHoc);
        }

        // POST: admin/KhoaHoc/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var khoaHoc = await _context.KhoaHoc.FindAsync(id);
            if (khoaHoc != null)
            {
                _context.KhoaHoc.Remove(khoaHoc);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool KhoaHocExists(int id)
        {
            return _context.KhoaHoc.Any(e => e.MaKhoaHoc == id);
        }
    }
}