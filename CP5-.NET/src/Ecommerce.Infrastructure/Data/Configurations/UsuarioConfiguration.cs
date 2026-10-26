using Ecommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.Infrastructure.Data.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<UsuarioEntity>
{
    public void Configure(EntityTypeBuilder<UsuarioEntity> builder)
    {
        builder.ToTable("Usuarios");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Nome).IsRequired().HasMaxLength(150);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(200);
        builder.Property(u => u.SenhaHash).IsRequired().HasMaxLength(256);
        builder.Property(u => u.SenhaSalt).IsRequired().HasMaxLength(256);
        builder.Property(u => u.Perfil).HasConversion<string>().HasMaxLength(20).IsRequired();

        // E-mail unico
        builder.HasIndex(u => u.Email).IsUnique().HasDatabaseName("IX_Usuarios_Email");
    }
}
