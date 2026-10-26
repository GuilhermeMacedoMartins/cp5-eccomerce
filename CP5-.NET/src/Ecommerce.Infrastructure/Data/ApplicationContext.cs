using System.Reflection;
using Ecommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Data;

public class ApplicationContext : DbContext
{
    public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options) { }

    public DbSet<ProdutoEntity> Produtos => Set<ProdutoEntity>();
    public DbSet<PedidoEntity> Pedidos => Set<PedidoEntity>();
    public DbSet<ItemPedidoEntity> ItensPedido => Set<ItemPedidoEntity>();
    public DbSet<UsuarioEntity> Usuarios => Set<UsuarioEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
