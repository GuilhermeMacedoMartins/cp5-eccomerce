using Ecommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.Infrastructure.Data.Configurations;

public class PedidoConfiguration : IEntityTypeConfiguration<PedidoEntity>
{
    public void Configure(EntityTypeBuilder<PedidoEntity> builder)
    {
        builder.ToTable("Pedidos");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.ClienteNome).IsRequired().HasMaxLength(150);
        builder.Property(p => p.ClienteEmail).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(p => p.ValorTotal).HasColumnType("decimal(10,2)");

        builder.Property(p => p.Cep).HasMaxLength(9);
        builder.Property(p => p.Logradouro).HasMaxLength(200);
        builder.Property(p => p.Bairro).HasMaxLength(120);
        builder.Property(p => p.Cidade).HasMaxLength(120);
        builder.Property(p => p.Uf).HasMaxLength(2);

        builder.HasIndex(p => p.Status).HasDatabaseName("IX_Pedidos_Status");
        builder.HasIndex(p => p.DataPedido).HasDatabaseName("IX_Pedidos_DataPedido");
        builder.HasIndex(p => p.UsuarioId).HasDatabaseName("IX_Pedidos_UsuarioId");

        builder.HasOne(p => p.Usuario)
               .WithMany(u => u.Pedidos)
               .HasForeignKey(p => p.UsuarioId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Itens)
               .WithOne(i => i.Pedido)
               .HasForeignKey(i => i.PedidoId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
