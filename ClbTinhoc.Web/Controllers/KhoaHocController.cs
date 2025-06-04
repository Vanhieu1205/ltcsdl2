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

namespace ClbTinhoc.Web.Controllers
{
    [RequireLogin]
    public class KhoaHocController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<KhoaHocController> _logger;

        public KhoaHocController(ApplicationDbContext context, ILogger<KhoaHocController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: KhoaHoc
        public async Task<IActionResult> Index()
        {
            return View(await _context.KhoaHoc.ToListAsync());
        }

        // GET: KhoaHoc/Search
        public async Task<IActionResult> Search(string searchString)
        {
            var khoaHoc = from k in _context.KhoaHoc
                          select k;

            if (!String.IsNullOrEmpty(searchString))
            {
                khoaHoc = khoaHoc.Where(k => k.TenKhoaHoc.Contains(searchString) ||
                                            k.MoTa.Contains(searchString));
            }

            return View("Index", await khoaHoc.ToListAsync());
        }

        // GET: KhoaHoc/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: KhoaHoc/Create
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

        // GET: KhoaHoc/Details/5
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

            _logger.LogInformation($"Found {availableStudents.Count} available students and {availableSupports.Count} available supports for KhoaHoc ID {id}.");
            ViewBag.AvailableStudents = availableStudents;
            ViewBag.AvailableSupports = availableSupports;

            return View(khoaHoc);
        }

        // POST: KhoaHoc/RegisterCourse
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterCourse(int MaKhoaHoc, string MaSinhVien)
        {
            try
            {
                _logger.LogInformation($"RegisterCourse called with MaKhoaHoc: {MaKhoaHoc}, MaSinhVien: {MaSinhVien}");

                if (string.IsNullOrEmpty(MaSinhVien))
                {
                    _logger.LogWarning("MaSinhVien is empty.");
                    TempData["Error"] = "Vui lòng chọn sinh viên.";
                    return RedirectToAction("Details", new { id = MaKhoaHoc });
                }

                var khoaHoc = await _context.KhoaHoc.FindAsync(MaKhoaHoc);
                if (khoaHoc == null)
                {
                    _logger.LogWarning($"KhoaHoc with ID {MaKhoaHoc} not found.");
                    TempData["Error"] = "Khóa học không tồn tại.";
                    return RedirectToAction("Details", new { id = MaKhoaHoc });
                }

                var sinhVien = await _context.SinhVien.FindAsync(MaSinhVien);
                if (sinhVien == null)
                {
                    _logger.LogWarning($"SinhVien with MaSinhVien {MaSinhVien} not found.");
                    TempData["Error"] = "Sinh viên không tồn tại.";
                    return RedirectToAction("Details", new { id = MaKhoaHoc });
                }

                var existing = await _context.KhoaHoc_SinhViens
                    .AnyAsync(ks => ks.MaKhoaHoc == MaKhoaHoc && ks.MaSinhVien == MaSinhVien);

                if (existing)
                {
                    _logger.LogWarning($"SinhVien {MaSinhVien} already enrolled in KhoaHoc {MaKhoaHoc}.");
                    TempData["Error"] = "Sinh viên đã đăng ký hoặc đang chờ duyệt khóa học này.";
                    return RedirectToAction("Details", new { id = MaKhoaHoc });
                }

                var userRole = HttpContext.Session.GetString("UserRole");
                var khoaHocSinhVien = new KhoaHoc_SinhVien
                {
                    MaKhoaHoc = MaKhoaHoc,
                    MaSinhVien = MaSinhVien,
                    TrangThai = userRole == "admin" ? "DaDuyet" : "ChoDuyet"
                };

                _logger.LogInformation($"Adding KhoaHoc_SinhVien: MaKhoaHoc={MaKhoaHoc}, MaSinhVien={MaSinhVien}, TrangThai={khoaHocSinhVien.TrangThai}");
                _context.KhoaHoc_SinhViens.Add(khoaHocSinhVien);
                await _context.SaveChangesAsync();

                _logger.LogInformation("KhoaHoc_SinhVien saved successfully.");
                if (userRole == "admin")
                    TempData["Success"] = "Đăng ký thành công!";
                else
                    TempData["Success"] = "Đăng ký thành công, vui lòng chờ admin duyệt.";
                return RedirectToAction("Details", new { id = MaKhoaHoc });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error registering course: {ex.Message}\nStackTrace: {ex.StackTrace}");
                TempData["Error"] = "Có lỗi xảy ra khi đăng ký khóa học. Vui lòng thử lại.";
                return RedirectToAction("Details", new { id = MaKhoaHoc });
            }
        }

        // POST: KhoaHoc/RegisterSupport
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterSupport(int MaKhoaHoc, string MaSupport)
        {
            try
            {
                _logger.LogInformation($"RegisterSupport called with MaKhoaHoc: {MaKhoaHoc}, MaSupport: {MaSupport}");

                if (string.IsNullOrEmpty(MaSupport))
                {
                    _logger.LogWarning("MaSupport is empty.");
                    TempData["Error"] = "Vui lòng chọn support.";
                    return RedirectToAction("Details", new { id = MaKhoaHoc });
                }

                var khoaHoc = await _context.KhoaHoc.FindAsync(MaKhoaHoc);
                if (khoaHoc == null)
                {
                    _logger.LogWarning($"KhoaHoc with ID {MaKhoaHoc} not found.");
                    return NotFound();
                }

                var support = await _context.Support.FindAsync(MaSupport);
                if (support == null)
                {
                    _logger.LogWarning($"Support with ID {MaSupport} not found.");
                    TempData["Error"] = "Support không tồn tại.";
                    return RedirectToAction("Details", new { id = MaKhoaHoc });
                }

                var existingRegistration = await _context.SupportKhoaHoc
                    .FirstOrDefaultAsync(ks => ks.MaKhoaHoc == MaKhoaHoc && ks.MaSupport == MaSupport);

                if (existingRegistration != null)
                {
                    _logger.LogWarning($"Support {MaSupport} is already registered for KhoaHoc {MaKhoaHoc}");
                    TempData["Error"] = "Support này đã được đăng ký cho khóa học.";
                    return RedirectToAction("Details", new { id = MaKhoaHoc });
                }

                var supportKhoaHoc = new SupportKhoaHoc
                {
                    MaKhoaHoc = MaKhoaHoc,
                    MaSupport = MaSupport
                };

                _context.SupportKhoaHoc.Add(supportKhoaHoc);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Successfully registered Support {MaSupport} for KhoaHoc {MaKhoaHoc}");
                TempData["Success"] = "Đăng ký support thành công.";
                return RedirectToAction("Details", new { id = MaKhoaHoc });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error registering support: {ex.Message}\nStackTrace: {ex.StackTrace}");
                TempData["Error"] = "Có lỗi xảy ra khi đăng ký support. Vui lòng thử lại.";
                return RedirectToAction("Details", new { id = MaKhoaHoc });
            }
        }

        // GET: KhoaHoc/Edit/5
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

        // POST: KhoaHoc/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaKhoaHoc,TenKhoaHoc,MoTa,NgayBatDau,NgayKetThuc,image")] KhoaHoc khoaHoc, IFormFile ImageFile)
        {
            // Check if user is admin
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "admin")
            {
                TempData["ErrorMessage"] = "Bạn không có quyền sửa khóa học!";
                return RedirectToAction(nameof(Index));
            }

            if (id != khoaHoc.MaKhoaHoc)
            {
                TempData["ErrorMessage"] = "Không tìm thấy khóa học cần sửa.";
                return RedirectToAction(nameof(Index));
            }

            // Retain existing image path if no new file is uploaded
            if (ImageFile == null)
            {
                var existingKhoaHoc = await _context.KhoaHoc.AsNoTracking().FirstOrDefaultAsync(k => k.MaKhoaHoc == id);
                if (existingKhoaHoc != null)
                {
                    khoaHoc.image = existingKhoaHoc.image;
                }
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
                            // Preserve other model state errors and return view
                            return View(khoaHoc);
                        }

                        if (ImageFile.Length > 5 * 1024 * 1024)
                        {
                            ModelState.AddModelError("ImageFile", "Kích thước file không được vượt quá 5MB.");
                            // Preserve other model state errors and return view
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
                    TempData["SuccessMessage"] = "Cập nhật khóa học thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KhoaHocExists(khoaHoc.MaKhoaHoc))
                    {
                        TempData["ErrorMessage"] = "Không tìm thấy khóa học khi cập nhật.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Lỗi đồng thời khi cập nhật khóa học.";
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating KhoaHoc");
                    TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật khóa học: " + ex.Message;
                    return RedirectToAction(nameof(Index));
                }
            }

            // If we got this far, something failed, redisplay form
            // (ModelState was not valid)
            return View(khoaHoc);
        }

        // GET: KhoaHoc/Delete/5
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

        // POST: KhoaHoc/Delete/5
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

        // XÓA SINH VIÊN KHỎI KHÓA HỌC
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveStudent(int MaKhoaHoc, string MaSinhVien)
        {
            var kh_sv = await _context.KhoaHoc_SinhViens
                .FirstOrDefaultAsync(x => x.MaKhoaHoc == MaKhoaHoc && x.MaSinhVien == MaSinhVien);
            if (kh_sv != null)
            {
                _context.KhoaHoc_SinhViens.Remove(kh_sv);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xóa sinh viên khỏi khóa học.";
            }
            else
            {
                TempData["Error"] = "Không tìm thấy sinh viên trong khóa học.";
            }
            return RedirectToAction("Details", new { id = MaKhoaHoc });
        }

        // XÓA SUPPORT KHỎI KHÓA HỌC
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveSupport(int MaKhoaHoc, string MaSupport)
        {
            var kh_sp = await _context.SupportKhoaHoc
                .FirstOrDefaultAsync(x => x.MaKhoaHoc == MaKhoaHoc && x.MaSupport == MaSupport);
            if (kh_sp != null)
            {
                _context.SupportKhoaHoc.Remove(kh_sp);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xóa support khỏi khóa học.";
            }
            else
            {
                TempData["Error"] = "Không tìm thấy support trong khóa học.";
            }
            return RedirectToAction("Details", new { id = MaKhoaHoc });
        }

        // DUYỆT SINH VIÊN
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveStudent(int MaKhoaHoc, string MaSinhVien)
        {
            var kh_sv = await _context.KhoaHoc_SinhViens
                .FirstOrDefaultAsync(x => x.MaKhoaHoc == MaKhoaHoc && x.MaSinhVien == MaSinhVien);
            if (kh_sv != null)
            {
                kh_sv.TrangThai = "DaDuyet";
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã duyệt sinh viên.";
            }
            return RedirectToAction("Details", new { id = MaKhoaHoc });
        }

        // TỪ CHỐI SINH VIÊN
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectStudent(int MaKhoaHoc, string MaSinhVien)
        {
            var kh_sv = await _context.KhoaHoc_SinhViens
                .FirstOrDefaultAsync(x => x.MaKhoaHoc == MaKhoaHoc && x.MaSinhVien == MaSinhVien);
            if (kh_sv != null)
            {
                kh_sv.TrangThai = "TuChoi";
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã từ chối sinh viên.";
            }
            return RedirectToAction("Details", new { id = MaKhoaHoc });
        }
    }
}