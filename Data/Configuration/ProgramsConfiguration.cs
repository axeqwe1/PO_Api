using System;
using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PO_Api.Models;


public class ProgramsConfiguration : IEntityTypeConfiguration<Programes>
{
    public void Configure(EntityTypeBuilder<Programes> builder)
    {
        builder
        .HasMany(p => p.Access)
        .WithOne(r => r.Programs)
        .HasForeignKey(r => r.ProgramId)
        .HasPrincipalKey(n => n.ProgramId);
    }
}
