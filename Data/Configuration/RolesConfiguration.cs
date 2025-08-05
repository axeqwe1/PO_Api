using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PO_Api.Models;

public class RolesConfiguration : IEntityTypeConfiguration<Roles>
{
    public void Configure(EntityTypeBuilder<Roles> builder)
    {
        builder
        .HasMany(p => p.Access)
        .WithOne(r => r.Roles)
        .HasForeignKey(r => r.RoleId)
        .HasPrincipalKey(n => n.RoleId);
    }
}
