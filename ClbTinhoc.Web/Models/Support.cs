using System.ComponentModel.DataAnnotations;

namespace ClbTinhoc.Web.Models
{
    public class Support
    {
        
        [StringLength(50, ErrorMessage = "Mã Support không được vượt quá 50 ký tự.")]
        public string MaSupport { get; set; }

        [StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự.")]
        public string HoTen { get; set; }

        
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string Email { get; set; }

        [StringLength(50, ErrorMessage = "Lớp sinh hoạt không được vượt quá 50 ký tự.")]
        public string LopSinhHoat { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        public string SoDienThoai { get; set; }
            
        public string HinhAnh { get; set; }
        //public virtual ICollection<SupportLopHoc> SupportLopHocs { get; set; }
    }
}
