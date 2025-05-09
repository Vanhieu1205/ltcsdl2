using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClbTinhoc.Web.Models
{
    public class UserLogin
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string studentId { get; set; }
        public string username { get; set; }
        public string email { get; set; }
        public string SoDienThoai { get; set; }
        public DateTime NgaySinh { get; set; }
        public string pass { get; set; }
        [StringLength(50)]
        public string Role { get; set; }
    }
}