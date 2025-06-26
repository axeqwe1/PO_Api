using PO_Api.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

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


        }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<PO_Main> PO_Mains { get; set; }
        public DbSet<PO_SuppRcv> PO_SuppRcvs { get; set; }
        public DbSet<PO_Details> PO_Details { get; set; }
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    }
}