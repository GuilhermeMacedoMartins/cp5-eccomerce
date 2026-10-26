using System.Net.Http.Headers;
using System.Net.Http.Json;
using Ecommerce.API.Common;
using Ecommerce.Application.DTOs.Auth;

namespace Ecommerce.Tests.Functional;

public static class AuthHelper
{
    public const string EmailAdmin = "admin@ecommerce.com";
    public const string SenhaAdmin = "Admin@123";
    public const string EmailCliente = "cliente@ecommerce.com";
    public const string SenhaCliente = "Cliente@123";

    public static async Task<HttpClient> ClienteAutenticadoAsync(
        CustomWebApplicationFactory factory, string email, string senha)
    {
        var client = factory.CreateClient();

        var resposta = await client.PostAsJsonAsync("/api/auth/login", new LoginDto { Email = email, Senha = senha });
        resposta.EnsureSuccessStatusCode();

        var corpo = await resposta.Content.ReadFromJsonAsync<ApiResponse<TokenDto>>();
        var token = corpo?.Dados?.AccessToken ?? throw new InvalidOperationException("Token nao retornado no login.");

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public static Task<HttpClient> ComoAdminAsync(CustomWebApplicationFactory f) =>
        ClienteAutenticadoAsync(f, EmailAdmin, SenhaAdmin);

    public static Task<HttpClient> ComoClienteAsync(CustomWebApplicationFactory f) =>
        ClienteAutenticadoAsync(f, EmailCliente, SenhaCliente);
}
