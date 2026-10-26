using Ecommerce.API.Common;
using Ecommerce.API.Hateoas;
using Ecommerce.Application.DTOs.Common;
using Ecommerce.Application.DTOs.Produto;
using Ecommerce.Application.UseCases.Interfaces;
using Ecommerce.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Ecommerce.API.Controllers;

[Route("api/[controller]")]
[Authorize]
[SwaggerTag("Catalogo de produtos. Escrita restrita ao perfil Administrador.")]
public class ProdutosController : ApiControllerBase
{
    private readonly IProdutoUseCase _useCase;
    private readonly ILinkService _linkService;
    private readonly ILogger<ProdutosController> _logger;

    public ProdutosController(IProdutoUseCase useCase, ILinkService linkService, ILogger<ProdutosController> logger)
    {
        _useCase = useCase;
        _linkService = linkService;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    [SwaggerOperation(
        Summary = "Lista produtos paginados",
        Description = "Devolve um PageResult com metadados completos e links de navegacao. " +
                      "Endpoint publico: nao exige autenticacao.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Pagina retornada", typeof(ApiResponse<PageResult<ProdutoDto>>))]
    public async Task<IActionResult> ObterPaginado([FromQuery] PageQueryDto query, CancellationToken ct)
    {
        var resultado = await _useCase.ObterPaginadoAsync(query, ct);

        if (resultado.IsFailure)
            return TratarFalha(resultado.Error);

        var pagina = resultado.Value;

        foreach (var produto in pagina.Data)
            _linkService.AdicionarLinks(produto, EhAdministrador);

        _linkService.AdicionarLinksDaColecao(pagina, "/api/produtos", query);

        return Ok(ApiResponse<PageResult<ProdutoDto>>.Ok(pagina));
    }

    [HttpGet("{id:int}", Name = "ObterProdutoPorId")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Obtem um produto por ID")]
    [SwaggerResponse(StatusCodes.Status200OK, "Produto encontrado", typeof(ApiResponse<ProdutoDto>))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Produto nao encontrado", typeof(ApiResponse<object>))]
    public async Task<IActionResult> ObterPorId(int id, CancellationToken ct)
    {
        var resultado = await _useCase.ObterPorIdAsync(id, ct);

        if (resultado.IsFailure)
            return TratarFalha(resultado.Error);

        var dto = _linkService.AdicionarLinks(resultado.Value, EhAdministrador);
        return Ok(ApiResponse<ProdutoDto>.Ok(dto));
    }

    [HttpPost]
    [Authorize(Policy = "SomenteAdministrador")]
    [SwaggerOperation(
        Summary = "Cadastra um produto (Administrador)",
        Description = "Requer token JWT de um usuario com perfil Administrador. " +
                      "Um token de Cliente recebe 403 Forbidden.")]
    [SwaggerResponse(StatusCodes.Status201Created, "Produto criado", typeof(ApiResponse<ProdutoDto>))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Token ausente ou invalido")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Perfil sem permissao")]
    public async Task<IActionResult> Criar([FromBody] CriarProdutoDto dto, CancellationToken ct)
    {
        _logger.LogInformation("Administrador {UsuarioId} cadastrando o produto {Nome}", UsuarioId, dto.Nome);

        var resultado = await _useCase.CriarAsync(dto, ct);

        if (resultado.IsFailure)
            return TratarFalha(resultado.Error);

        var criado = _linkService.AdicionarLinks(resultado.Value, true);

        return CreatedAtRoute("ObterProdutoPorId", new { id = criado.Id },
            ApiResponse<ProdutoDto>.Ok(criado));
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "SomenteAdministrador")]
    [SwaggerOperation(Summary = "Atualiza um produto (Administrador)")]
    [SwaggerResponse(StatusCodes.Status200OK, "Produto atualizado", typeof(ApiResponse<ProdutoDto>))]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Perfil sem permissao")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Produto nao encontrado", typeof(ApiResponse<object>))]
    public async Task<IActionResult> Atualizar(int id, [FromBody] AtualizarProdutoDto dto, CancellationToken ct)
    {
        var resultado = await _useCase.AtualizarAsync(id, dto, ct);

        if (resultado.IsFailure)
            return TratarFalha(resultado.Error);

        var atualizado = _linkService.AdicionarLinks(resultado.Value, true);
        return Ok(ApiResponse<ProdutoDto>.Ok(atualizado));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "SomenteAdministrador")]
    [SwaggerOperation(Summary = "Remove um produto (Administrador)")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Produto removido")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Produto vinculado a pedidos", typeof(ApiResponse<object>))]
    public async Task<IActionResult> Remover(int id, CancellationToken ct)
    {
        var resultado = await _useCase.RemoverAsync(id, ct);
        return resultado.IsFailure ? TratarFalha(resultado.Error) : NoContent();
    }
}
