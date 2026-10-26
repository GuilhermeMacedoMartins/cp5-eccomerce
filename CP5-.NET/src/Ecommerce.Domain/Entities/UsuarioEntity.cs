using Ecommerce.Domain.Enums;

namespace Ecommerce.Domain.Entities;

public class UsuarioEntity
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public string SenhaSalt { get; set; } = string.Empty;
    public PerfilUsuario Perfil { get; set; } = PerfilUsuario.Cliente;
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    public ICollection<PedidoEntity> Pedidos { get; set; } = new List<PedidoEntity>();
}
