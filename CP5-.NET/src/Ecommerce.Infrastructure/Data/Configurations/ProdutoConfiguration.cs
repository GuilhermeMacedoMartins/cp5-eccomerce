using Ecommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.Infrastructure.Data.Configurations;

public class ProdutoConfiguration : IEntityTypeConfiguration<ProdutoEntity>
{
    public void Configure(EntityTypeBuilder<ProdutoEntity> builder)
    {
        builder.ToTable("Produtos");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Nome).IsRequired().HasMaxLength(150);
        builder.Property(p => p.Descricao).HasMaxLength(500);
        builder.Property(p => p.Preco).HasColumnType("decimal(10,2)").IsRequired();
        builder.Property(p => p.Categoria).IsRequired().HasMaxLength(80);

        builder.HasIndex(p => p.Categoria).HasDatabaseName("IX_Produtos_Categoria");
        builder.HasIndex(p => p.Nome).HasDatabaseName("IX_Produtos_Nome");

        builder.HasIndex(p => new { p.Ativo, p.Categoria })
               .HasDatabaseName("IX_Produtos_Ativo_Categoria")
               .IncludeProperties(p => new { p.Nome, p.Preco, p.QuantidadeEstoque });
    }
}
