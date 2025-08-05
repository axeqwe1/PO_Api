using System;
using Microsoft.EntityFrameworkCore;
using PO_Api.Models;

namespace PO_Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<PO_Details>()
            //    .HasNoKey() // ถ้าไม่มีจริง ๆ
            //                //.HasKey(e => new { e.PONo, e.MatrCode, e.Color }); // ถ้า unique จริง ให้ใช้
            //    .ToView("PO_Details"); // ระบุว่าเป็น View
            modelBuilder.Entity<PO_Main>()
            .HasMany(p => p.Details)
            .WithOne(d => d.PO_Main)
            .HasForeignKey(d => d.PONo)
            .HasPrincipalKey(p => p.PONo);

            modelBuilder.Entity<PO_Main>()
            .HasMany(p => p.FileAttachment)
            .WithOne() // ถ้าไม่มี navigation back จาก PO_FileAttachment → PO_Main
            .HasForeignKey(f => f.PONo)
            .HasPrincipalKey(p => p.PONo);

            modelBuilder.Entity<PO_FileAttachment>(entity =>
            {
                entity.ToTable("PO_FileAttachment");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Filename).HasColumnName("filename");
                entity.Property(e => e.Type).HasColumnName("type");
                entity.Property(e => e.UploadDate).HasColumnName("uploadDate");
                entity.Property(e => e.Url).HasColumnName("url");
                entity.Property(e => e.PONo).HasColumnName("PONo");
                entity.Property(e => e.OriginalName).HasColumnName("originalName");
                entity.Property(e => e.UploadByType).HasColumnName("UploadByType");
                entity.Property(e => e.FileSize).HasColumnName("fileSize");
            });
                entity.Property(e => e.Price).HasConversion<double>();
            });
        public DbSet<Role> Roles { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<PO_Main> PO_Mains { get; set; }
        public DbSet<PO_SuppRcv> PO_SuppRcvs { get; set; }
        public DbSet<PO_Details> PO_Details { get; set; }
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<PO_FileAttachment> PO_FileAttachments { get; set; }
        public DbSet<PO_FileAttachment> PO_FileAttachments { get; set; }
        public DbSet<PO_PasswordResetToken> PO_PasswordResetTokens { get; set; }
        public DbSet<PO_Notifications> PO_Notifications { get; set; }
        public DbSet<PO_NotificationReceiver> PO_NotificationReceivers { get; set; }
        public DbSet<UserEmail> UserEmails { get; set; }

        public DbSet<UserCenter> UserCenters { get; set; }
        public DbSet<SupMonitoring> SupMonitorings { get; set; }
        public DbSet<WorkMinuteModel> WorkMinutes { get; set; }
        public DbSet<MasterLine> masterLines { get; set; }
        public DbSet<MonitorLineStatus> MonitorLineStatus { get; set; }
    }
}