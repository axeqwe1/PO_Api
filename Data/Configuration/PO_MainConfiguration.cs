using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PO_Api.Models;

public class PO_MainConfiguration : IEntityTypeConfiguration<PO_Main>
{
    public void Configure(EntityTypeBuilder<PO_Main> builder)
    {
        builder.HasMany(p => p.Details)
               .WithOne(d => d.PO_Main)
               .HasForeignKey(d => d.PONo)
               .HasPrincipalKey(p => p.PONo);

        builder.HasMany(p => p.FileAttachment)
               .WithOne()
               .HasForeignKey(f => f.PONo)
               .HasPrincipalKey(p => p.PONo);

        builder.Property(e => e.AmountNoVat).HasConversion<double>();
        builder.Property(e => e.AmountTotal).HasConversion<double>();
        builder.Property(e => e.TotalVat).HasConversion<double>();
        builder.Property(e => e.TotalChange).HasConversion<double>();
    }
}
