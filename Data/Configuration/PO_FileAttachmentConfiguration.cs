using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PO_Api.Models;
using YourProject.Models;

public class PO_FileAttachmentConfiguration : IEntityTypeConfiguration<PO_FileAttachment>
{
    public void Configure(EntityTypeBuilder<PO_FileAttachment> builder)
    {
        builder.ToTable("PO_FileAttachment");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.Filename).HasColumnName("filename");
        builder.Property(e => e.Type).HasColumnName("type");
        builder.Property(e => e.UploadDate).HasColumnName("uploadDate");
        builder.Property(e => e.Url).HasColumnName("url");
        builder.Property(e => e.PONo).HasColumnName("PONo");
        builder.Property(e => e.OriginalName).HasColumnName("originalName");
        builder.Property(e => e.UploadByType).HasColumnName("UploadByType");
        builder.Property(e => e.FileSize).HasColumnName("fileSize");
    }
}
