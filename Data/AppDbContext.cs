using Microsoft.EntityFrameworkCore;
using PO_Api.Models;
using YourProject.Models;

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

            modelBuilder.Entity<PO_Notifications>()
            .HasMany(p => p.Receivers)
            .WithOne(r => r.Notification)
            .HasForeignKey(r => r.noti_id)
            .HasPrincipalKey(n => n.noti_id);

            modelBuilder.Entity<User>()
            .HasMany(p => p.Emails)
            .WithOne(t => t.User) // ถ้าไม่มี navigation back จาก PO_FileAttachment → PO_Main
            .HasForeignKey(f => f.userId)
            .HasPrincipalKey(p => p.userId);

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

            modelBuilder.Entity<PO_Details>(entity =>
            {
                entity.Property(e => e.Qty).HasConversion<double>();
                entity.Property(e => e.ChargeValue).HasConversion<decimal>();
                entity.Property(e => e.ChargeAmt).HasConversion<double>();
                entity.Property(e => e.TotalAmount).HasConversion<double>();
                entity.Property(e => e.Price).HasConversion<double>();
            });


        }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<PO_Main> PO_Mains { get; set; }
        public DbSet<PO_SuppRcv> PO_SuppRcvs { get; set; }
        public DbSet<PO_Details> PO_Details { get; set; }
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<PO_FileAttachment> PO_FileAttachments { get; set; }
        public DbSet<PO_PasswordResetToken> PO_PasswordResetTokens { get; set; }
        public DbSet<PO_Notifications> PO_Notifications { get; set; }
        public DbSet<PO_NotificationReceiver> PO_NotificationReceivers { get; set; }
        public DbSet<UserEmail> UserEmails { get; set; }

        public DbSet<UserCenter> UserCenters { get; set; }
    }
}