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

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
            modelBuilder.Entity<Monitoring>().HasNoKey();
            modelBuilder.Entity<SupMonitoring>().HasNoKey();
            

            //modelBuilder.Entity<User>()
            //.HasMany(p => p.Emails)
            //.WithOne(t => t.User) // ถ้าไม่มี navigation back จาก PO_FileAttachment → PO_Main
            //.HasForeignKey(f => f.userId)
            //.HasPrincipalKey(p => p.userId);

            //modelBuilder.Entity<PO_FileAttachment>(entity =>
            //{
            //    entity.ToTable("PO_FileAttachment");
            //    entity.HasKey(e => e.Id);
            //    entity.Property(e => e.Id).HasColumnName("id");
            //    entity.Property(e => e.Filename).HasColumnName("filename");
            //    entity.Property(e => e.Type).HasColumnName("type");
            //    entity.Property(e => e.UploadDate).HasColumnName("uploadDate");
            //    entity.Property(e => e.Url).HasColumnName("url");
            //    entity.Property(e => e.PONo).HasColumnName("PONo");
            //    entity.Property(e => e.OriginalName).HasColumnName("originalName");
            //    entity.Property(e => e.UploadByType).HasColumnName("UploadByType");
            //    entity.Property(e => e.FileSize).HasColumnName("fileSize");
            //});

            //modelBuilder.Entity<PO_Details>(entity =>
            //{
            //    entity.Property(e => e.Qty).HasConversion<double>();
            //    entity.Property(e => e.ChargeValue).HasConversion<decimal>();
            //    entity.Property(e => e.ChargeAmt).HasConversion<double>();
            //    entity.Property(e => e.TotalAmount).HasConversion<double>();
            //    entity.Property(e => e.Price).HasConversion<double>();
            //});

        }
        public DbSet<User> Users { get; set; }
        public DbSet<Roles> Roles { get; set; }
        public DbSet<Programes> Programs { get; set; }
        public DbSet<UserAccess> Accesses { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public DbSet<Monitoring> Monitoring { get; set; }
        public DbSet<SupMonitoring> SupMonitorings { get; set; }
        public DbSet<WorkMinuteModel> WorkMinutes { get; set; }
        public DbSet<MasterLine> masterLines { get; set; }
        public DbSet<MonitorLineStatus> MonitorLineStatus { get; set; }
    }
}