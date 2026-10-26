using Ecommerce.Application.UseCases;
using Ecommerce.Application.UseCases.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IProdutoUseCase, ProdutoUseCase>();
        services.AddScoped<IPedidoUseCase, PedidoUseCase>();
        services.AddScoped<IAuthUseCase, AuthUseCase>();

        return services;
    }
}
