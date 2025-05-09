using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClbTinhoc.Web.Models
{
    public class DiemThi
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaDiem { get; set; }
        public string MaSinhVien { get; set; }
        public int MaKhoaHoc { get; set; }
        public float Diem { get; set; }
        public int LanThi { get; set; }

        // Navigation properties
        [ForeignKey("MaSinhVien")]
        public virtual SinhVien SinhVien { get; set; }

        [ForeignKey("MaKhoaHoc")]
        public virtual KhoaHoc KhoaHoc { get; set; }
    }
}