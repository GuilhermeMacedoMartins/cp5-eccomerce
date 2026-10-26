using Ecommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.Infrastructure.Data.Configurations;

public class ItemPedidoConfiguration : IEntityTypeConfiguration<ItemPedidoEntity>
{
    public void Configure(EntityTypeBuilder<ItemPedidoEntity> builder)
    {
        builder.ToTable("ItensPedido");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Quantidade).IsRequired();
        builder.Property(i => i.PrecoUnitario).HasColumnType("decimal(10,2)").IsRequired();

        // Subtotal e calculado em runtime, nao e coluna
        builder.Ignore(i => i.Subtotal);

        builder.HasOne(i => i.Produto)
               .WithMany(p => p.ItensPedido)
               .HasForeignKey(i => i.ProdutoId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(i => i.PedidoId).HasDatabaseName("IX_ItensPedido_PedidoId");
        builder.HasIndex(i => i.ProdutoId).HasDatabaseName("IX_ItensPedido_ProdutoId");
    }
}
