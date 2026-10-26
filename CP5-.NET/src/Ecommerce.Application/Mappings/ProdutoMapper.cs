using Ecommerce.Application.DTOs.Produto;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Mappings;

public static class ProdutoMapper
{
    public static ProdutoDto ToDto(this ProdutoEntity e) => new()
    {
        Id = e.Id,
        Nome = e.Nome,
        Descricao = e.Descricao,
        Preco = e.Preco,
        QuantidadeEstoque = e.QuantidadeEstoque,
        Categoria = e.Categoria,
        Ativo = e.Ativo,
        DataCriacao = e.DataCriacao
    };

    public static List<ProdutoDto> ToDtoList(this IEnumerable<ProdutoEntity> entities) =>
        entities.Select(ToDto).ToList();

    public static ProdutoEntity ToEntity(this CriarProdutoDto dto) => new()
    {
        Nome = dto.Nome,
        Descricao = dto.Descricao,
        Preco = dto.Preco,
        QuantidadeEstoque = dto.QuantidadeEstoque,
        Categoria = dto.Categoria,
        Ativo = true,
        DataCriacao = DateTime.UtcNow
    };

    public static void ApplyTo(this AtualizarProdutoDto dto, ProdutoEntity e)
    {
        e.Nome = dto.Nome;
        e.Descricao = dto.Descricao;
        e.Preco = dto.Preco;
        e.QuantidadeEstoque = dto.QuantidadeEstoque;
        e.Categoria = dto.Categoria;
        e.Ativo = dto.Ativo;
    }
}
