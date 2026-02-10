//==================================================================================================
// Dependency injection extension methods for configuring ResultKit.MediatR.
// AddResultKitMediatR method for automatic pipeline behavior registration.
//==================================================================================================
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Fox.ResultKit.MediatR;

//==================================================================================================
/// <summary>
/// Dependency injection extension methods for configuring ResultKit.MediatR.
/// </summary>
//==================================================================================================
public static class ServiceCollectionExtensions
{
    //==============================================================================================
    /// <summary>
    /// Adds the ResultKit MediatR pipeline behavior to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for fluent API support.</returns>
    //==============================================================================================
    public static IServiceCollection AddResultKitMediatR(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ResultPipelineBehavior<,>));

        return services;
    }
}
