using ClbTinhoc.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace ClbTinhoc.Web.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<SinhVien> SinhVien { get; set; }
        public DbSet<Support> Support { get; set; }
        public DbSet<KhoaHoc> KhoaHoc { get; set; }
        public DbSet<KetQua> KetQua { get; set; }
        public DbSet<KhoaHoc_SinhVien> KhoaHoc_SinhViens { get; set; }
        public DbSet<SupportKhoaHoc> SupportKhoaHoc { get; set; }
        public DbSet<UserLogin> UserLogin { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Cấu hình composite key cho KhoaHoc_SinhVien
            modelBuilder.Entity<KhoaHoc_SinhVien>()
                .HasKey(ks => new { ks.MaSinhVien, ks.MaKhoaHoc });

            // Cấu hình mối quan hệ
            modelBuilder.Entity<KhoaHoc_SinhVien>()
                .HasOne(ks => ks.SinhVien)
                .WithMany(s => s.KhoaHoc_SinhVien)
                .HasForeignKey(ks => ks.MaSinhVien)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<KhoaHoc_SinhVien>()
                .HasOne(ks => ks.KhoaHoc)
                .WithMany(k => k.KhoaHoc_SinhVien)
                .HasForeignKey(ks => ks.MaKhoaHoc)
                .OnDelete(DeleteBehavior.Cascade);
            // Cấu hình khóa chính cho các bảng
            modelBuilder.Entity<SinhVien>().HasKey(s => s.MaSinhVien);
            modelBuilder.Entity<Support>().HasKey(s => s.MaSupport);
            //modelBuilder.Entity<KhoaHoc>()
            //            .HasOne<KhoaHoc>()
            //            .WithMany()
            //            .HasForeignKey(l => l.MaKhoaHoc);
            modelBuilder.Entity<KetQua>().HasKey(k => k.MaKetQua);
            modelBuilder.Entity<UserLogin>().HasKey(u => u.Id);

            // Cấu hình khóa chính cho các bảng liên kết
            modelBuilder.Entity<KhoaHoc_SinhVien>()
                .HasKey(sl => new { sl.MaSinhVien, sl.MaKhoaHoc });

            modelBuilder.Entity<SupportKhoaHoc>()
                .HasKey(sl => new { sl.MaSupport, sl.MaKhoaHoc });

            // Cấu hình tên bảng
            modelBuilder.Entity<SinhVien>().ToTable("sinhvien");
            modelBuilder.Entity<Support>().ToTable("support");
            modelBuilder.Entity<KhoaHoc>().ToTable("khoahoc");
            modelBuilder.Entity<KetQua>().ToTable("ketqua");
            modelBuilder.Entity<KhoaHoc_SinhVien>().ToTable("student_khoahoc");
            modelBuilder.Entity<SupportKhoaHoc>().ToTable("support_khoahoc");
            modelBuilder.Entity<UserLogin>().ToTable("user_login");


        }
    }
}