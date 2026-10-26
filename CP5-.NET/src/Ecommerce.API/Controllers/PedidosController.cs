using Ecommerce.API.Common;
using Ecommerce.API.Hateoas;
using Ecommerce.Application.DTOs.Common;
using Ecommerce.Application.DTOs.Pedido;
using Ecommerce.Application.UseCases.Interfaces;
using Ecommerce.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Ecommerce.API.Controllers;

[Route("api/[controller]")]
[Authorize]
[SwaggerTag("Pedidos de compra. Exige autenticacao; o cliente so enxerga os proprios pedidos.")]
public class PedidosController : ApiControllerBase
{
    private readonly IPedidoUseCase _useCase;
    private readonly ILinkService _linkService;
    private readonly ILogger<PedidosController> _logger;

    public PedidosController(IPedidoUseCase useCase, ILinkService linkService, ILogger<PedidosController> logger)
    {
        _useCase = useCase;
        _linkService = linkService;
        _logger = logger;
    }

    [HttpGet]
    [SwaggerOperation(
        Summary = "Lista pedidos paginados",
        Description = "PageResult com links HATEOAS. Administrador enxerga todos os pedidos; " +
                      "Cliente enxerga somente os proprios.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Pagina retornada", typeof(ApiResponse<PageResult<PedidoDto>>))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Token ausente ou invalido")]
    public async Task<IActionResult> ObterPaginado([FromQuery] PageQueryDto query, CancellationToken ct)
    {
        // Cliente ve apenas os proprios pedidos
        int? filtroUsuario = EhAdministrador ? null : UsuarioId;

        var resultado = await _useCase.ObterPaginadoAsync(query, filtroUsuario, ct);

        if (resultado.IsFailure)
            return TratarFalha(resultado.Error);

        var pagina = resultado.Value;

        foreach (var pedido in pagina.Data)
            _linkService.AdicionarLinks(pedido);

        _linkService.AdicionarLinksDaColecao(pagina, "/api/pedidos", query);

        return Ok(ApiResponse<PageResult<PedidoDto>>.Ok(pagina));
    }

    [HttpGet("{id:int}", Name = "ObterPedidoPorId")]
    [SwaggerOperation(Summary = "Obtem um pedido por ID")]
    [SwaggerResponse(StatusCodes.Status200OK, "Pedido encontrado", typeof(ApiResponse<PedidoDto>))]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "O pedido pertence a outro usuario", typeof(ApiResponse<object>))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Pedido nao encontrado", typeof(ApiResponse<object>))]
    public async Task<IActionResult> ObterPorId(int id, CancellationToken ct)
    {
        var resultado = await _useCase.ObterPorIdAsync(id, ct);

        if (resultado.IsFailure)
            return TratarFalha(resultado.Error);

        // Cliente nao acessa pedido de terceiro
        if (!EhAdministrador && resultado.Value.UsuarioId != UsuarioId)
            return TratarFalha(DomainErrors.Pedido.NaoPertenceAoUsuario(id));

        var dto = _linkService.AdicionarLinks(resultado.Value);
        return Ok(ApiResponse<PedidoDto>.Ok(dto));
    }

    [HttpPost]
    [SwaggerOperation(
        Summary = "Cria um pedido",
        Description = "Consulta o CEP no servico externo ViaCEP para completar o endereco de entrega, " +
                      "valida o estoque pelas regras de dominio e congela o preco no momento da compra.")]
    [SwaggerResponse(StatusCodes.Status201Created, "Pedido criado", typeof(ApiResponse<PedidoDto>))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "CEP invalido ou itens duplicados", typeof(ApiResponse<object>))]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Estoque insuficiente", typeof(ApiResponse<object>))]
    [SwaggerResponse(StatusCodes.Status503ServiceUnavailable, "Servico de CEP indisponivel", typeof(ApiResponse<object>))]
    public async Task<IActionResult> Criar([FromBody] CriarPedidoDto dto, CancellationToken ct)
    {
        _logger.LogInformation("Usuario {UsuarioId} criando pedido com {Qtd} item(ns)", UsuarioId, dto.Itens.Count);

        var resultado = await _useCase.CriarAsync(dto, UsuarioId, ct);

        if (resultado.IsFailure)
            return TratarFalha(resultado.Error);

        var criado = _linkService.AdicionarLinks(resultado.Value);

        return CreatedAtRoute("ObterPedidoPorId", new { id = criado.Id },
            ApiResponse<PedidoDto>.Ok(criado));
    }

    [HttpPut("{id:int}/status")]
    [SwaggerOperation(
        Summary = "Atualiza o status do pedido",
        Description = "Pedidos Entregue ou Cancelado nao podem mais ser alterados. " +
                      "Cancelar um pedido repoe o estoque dos produtos.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Status atualizado", typeof(ApiResponse<PedidoDto>))]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Transicao nao permitida", typeof(ApiResponse<object>))]
    public async Task<IActionResult> AtualizarStatus(int id, [FromBody] AtualizarStatusPedidoDto dto, CancellationToken ct)
    {
        var atual = await _useCase.ObterPorIdAsync(id, ct);

        if (atual.IsFailure)
            return TratarFalha(atual.Error);

        if (!EhAdministrador && atual.Value.UsuarioId != UsuarioId)
            return TratarFalha(DomainErrors.Pedido.NaoPertenceAoUsuario(id));

        var resultado = await _useCase.AtualizarStatusAsync(id, dto, ct);

        if (resultado.IsFailure)
            return TratarFalha(resultado.Error);

        var atualizado = _linkService.AdicionarLinks(resultado.Value);
        return Ok(ApiResponse<PedidoDto>.Ok(atualizado));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "SomenteAdministrador")]
    [SwaggerOperation(Summary = "Remove um pedido (Administrador)")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Pedido removido")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Pedido nao encontrado", typeof(ApiResponse<object>))]
    public async Task<IActionResult> Remover(int id, CancellationToken ct)
    {
        var resultado = await _useCase.RemoverAsync(id, ct);
        return resultado.IsFailure ? TratarFalha(resultado.Error) : NoContent();
    }
}
