using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClbTinhoc.Web.Models
{
    public class KetQua
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaKetQua { get; set; }


        public string MaSinhVien { get; set; }

        public int MaKhoaHoc { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập điểm cuối kỳ")]
        [Range(0, 10, ErrorMessage = "Điểm phải từ 0 đến 10")]
        [Display(Name = "Điểm cuối kỳ")]
        public double DiemCuoiKy { get; set; }

        [Display(Name = "Ngày cập nhật")]
        public DateTime NgayCapNhat { get; set; }

        // Navigation properties
        [ForeignKey("MaSinhVien")]
        public virtual SinhVien SinhVien { get; set; }

        [ForeignKey("MaKhoaHoc")]
        public virtual KhoaHoc KhoaHoc { get; set; }
    }
}