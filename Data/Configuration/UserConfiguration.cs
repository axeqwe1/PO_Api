using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PO_Api.Models;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {

        builder
        .HasMany(p => p.Access)
        .WithOne(r => r.User)
        .HasForeignKey(r => r.UserId)
        .HasPrincipalKey(n => n.UserId);
    }
}
