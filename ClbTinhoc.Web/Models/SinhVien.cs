using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClbTinhoc.Web.Models
{
    [Table("sinhvien")]
    public class SinhVien
    {
        public SinhVien()
        {
            KhoaHoc_SinhVien = new HashSet<KhoaHoc_SinhVien>();
            KetQuas = new HashSet<KetQua>();
        }

        [Key]
        [Column("MaSinhVien")]
        [StringLength(20)]
        [Display(Name = "Mã sinh viên")]
        public string MaSinhVien { get; set; }

        [Required]
        [Column("HoTen")]
        [StringLength(50)]
        [Display(Name = "Họ tên")]
        public string HoTen { get; set; }

        [Required]
        [Column("LopSinhHoat")]
        [StringLength(50)]
        [Display(Name = "Lớp sinh hoạt")]
        public string LopSinhHoat { get; set; }

        [Required]
        [Column("Email")]
        [StringLength(50)]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [Column("SoDienThoai")]
        [StringLength(15)]
        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get; set; }

        [Required]
        [Column("NgayThamGia")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày tham gia")]
        public DateTime NgayThamGia { get; set; }

        // Navigation properties
        public virtual ICollection<KhoaHoc_SinhVien> KhoaHoc_SinhVien { get; set; }
        public virtual ICollection<KetQua> KetQuas { get; set; }
    }
}