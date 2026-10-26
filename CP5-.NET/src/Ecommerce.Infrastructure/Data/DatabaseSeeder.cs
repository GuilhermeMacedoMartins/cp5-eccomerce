using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Infrastructure.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationContext context, IPasswordHasher hasher, ILogger logger)
    {
        if (!await context.Usuarios.AnyAsync())
        {
            var (adminHash, adminSalt) = hasher.GerarHash("Admin@123");
            var (clienteHash, clienteSalt) = hasher.GerarHash("Cliente@123");

            context.Usuarios.AddRange(
                new UsuarioEntity
                {
                    Nome = "Administrador",
                    Email = "admin@ecommerce.com",
                    SenhaHash = adminHash,
                    SenhaSalt = adminSalt,
                    Perfil = PerfilUsuario.Administrador
                },
                new UsuarioEntity
                {
                    Nome = "Cliente Demo",
                    Email = "cliente@ecommerce.com",
                    SenhaHash = clienteHash,
                    SenhaSalt = clienteSalt,
                    Perfil = PerfilUsuario.Cliente
                });

            await context.SaveChangesAsync();
            logger.LogInformation("Usuarios padrao criados");
        }

        if (!await context.Produtos.AnyAsync())
        {
            context.Produtos.AddRange(
                new ProdutoEntity { Nome = "Mouse Gamer RGB", Descricao = "Mouse optico 7 botoes", Preco = 189.90m, QuantidadeEstoque = 50, Categoria = "Perifericos" },
                new ProdutoEntity { Nome = "Teclado Mecanico", Descricao = "Switch blue ABNT2", Preco = 349.90m, QuantidadeEstoque = 30, Categoria = "Perifericos" },
                new ProdutoEntity { Nome = "Monitor 24 polegadas", Descricao = "IPS Full HD 75Hz", Preco = 1299.00m, QuantidadeEstoque = 8, Categoria = "Telas" },
                new ProdutoEntity { Nome = "Headset 7.1", Descricao = "Som surround com microfone", Preco = 299.90m, QuantidadeEstoque = 15, Categoria = "Audio" },
                new ProdutoEntity { Nome = "Webcam Full HD", Descricao = "1080p com autofoco", Preco = 259.00m, QuantidadeEstoque = 12, Categoria = "Video" });

            await context.SaveChangesAsync();
            logger.LogInformation("Produtos de demonstracao criados");
        }
    }
}
