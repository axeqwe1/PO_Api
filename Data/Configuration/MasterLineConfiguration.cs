using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PO_Api.Models;

public class MasterLineConfiguration : IEntityTypeConfiguration<MasterLine>
{
    public void Configure(EntityTypeBuilder<MasterLine> builder)
    {

        builder
        .HasMany(p => p.WorkMinute)
        .WithOne(t => t.MasterLine)
        .HasForeignKey(r => r.line)
        .HasPrincipalKey(n => n.Line);

        builder
        .HasMany(p => p.monitorLineStatuses)
        .WithOne(t => t.MasterLine)
        .HasForeignKey(r => r.Line)
        .HasPrincipalKey(n => n.Line);

    }
}
