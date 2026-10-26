using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Application.DTOs.Auth;

public class LoginDto
{
    [Required(ErrorMessage = "O e-mail e obrigatorio.")]
    [EmailAddress(ErrorMessage = "E-mail invalido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "A senha e obrigatoria.")]
    [MinLength(6, ErrorMessage = "A senha deve ter ao menos 6 caracteres.")]
    public string Senha { get; set; } = string.Empty;
}
