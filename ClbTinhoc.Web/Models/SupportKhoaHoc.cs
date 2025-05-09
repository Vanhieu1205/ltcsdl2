using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
namespace ClbTinhoc.Web.Models
{
    public class SupportKhoaHoc
    {
        [Key]
        [Column(Order = 0)]
        public string MaSupport { get; set; }

        [Key]
        [Column(Order = 1)]
        public int MaKhoaHoc { get; set; }

        [ForeignKey("MaSupport")]
        public virtual Support Support { get; set; }

        [ForeignKey("MaKhoaHoc")]
        public virtual KhoaHoc KhoaHoc { get; set; }
    }
}