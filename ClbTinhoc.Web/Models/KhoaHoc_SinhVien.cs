using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClbTinhoc.Web.Models
{
    public class KhoaHoc_SinhVien
    {
        
        [Column(Order = 0)]
        public string MaSinhVien { get; set; }

        
        [Column(Order = 1)]
        public int MaKhoaHoc { get; set; }

        [ForeignKey("MaSinhVien")]
        public virtual SinhVien SinhVien { get; set; }

        [ForeignKey("MaKhoaHoc")]
        public virtual KhoaHoc KhoaHoc { get; set; }
    }
}