//==================================================================================================
// ServiceCollectionExtensions unit tests - Testing DI registration.
// Test suite for AddResultKitMediatR dependency injection configuration.
//==================================================================================================

using Microsoft.Extensions.DependencyInjection;

namespace Fox.ResultKit.MediatR.Tests;

//==================================================================================================
/// <summary>
/// ServiceCollectionExtensions unit tests.
/// </summary>
//==================================================================================================
public class ServiceCollectionExtensionsTests
{
    //==============================================================================================
    /// <summary>
    /// AddResultKitMediatR registers the ResultPipelineBehavior.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void AddResultKitMediatR_should_register_result_pipeline_behavior()
    {
        var services = new ServiceCollection();

        services.AddResultKitMediatR();

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IPipelineBehavior<,>));
        descriptor.Should().NotBeNull();
        descriptor!.ImplementationType.Should().Be(typeof(ResultPipelineBehavior<,>));
    }

    //==============================================================================================
    /// <summary>
    /// AddResultKitMediatR fluent API támogatást biztosít (visszaadja a services-t).
    /// </summary>
    //==============================================================================================
    [Fact]
    public void AddResultKitMediatR_should_return_services_for_fluent_api()
    {
        var services = new ServiceCollection();

        var result = services.AddResultKitMediatR();

        result.Should().BeSameAs(services);
    }

    //==============================================================================================
    /// <summary>
    /// AddResultKitMediatR ArgumentNullException-t dob null services esetén.
    /// </summary>
    //==============================================================================================
    [Fact]
    public void AddResultKitMediatR_should_throw_argument_null_exception_when_services_is_null()
    {
        IServiceCollection services = null!;

        var act = () => services.AddResultKitMediatR();

        act.Should().Throw<ArgumentNullException>();
    }

    //==============================================================================================
    /// <summary>
    /// AddResultKitMediatR többször is meghívható (nem dob hibát).
    /// </summary>
    //==============================================================================================
    [Fact]
    public void AddResultKitMediatR_should_allow_multiple_registrations()
    {
        var services = new ServiceCollection();

        services.AddResultKitMediatR();
        var act = () => services.AddResultKitMediatR();

        act.Should().NotThrow();
    }

    //==============================================================================================
    /// <summary>
    /// AddResultKitMediatR integrálódik a MediatR pipeline-nal.
    /// </summary>
    //==============================================================================================
    [Fact]
    public async Task AddResultKitMediatR_should_integrate_with_mediatr_pipeline()
    {
        var services = new ServiceCollection();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensionsTests).Assembly));
        services.AddResultKitMediatR();
        services.AddTransient<IRequestHandler<TestCommand, Result>, TestCommandHandler>();
        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        var result = await mediator.Send(new TestCommand(false));

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
    }

    //==============================================================================================
    /// <summary>
    /// Teszt command.
    /// </summary>
    //==============================================================================================
    private record TestCommand(bool ThrowException) : IRequest<Result>;

    //==============================================================================================
    /// <summary>
    /// Teszt command handler.
    /// </summary>
    //==============================================================================================
    private class TestCommandHandler : IRequestHandler<TestCommand, Result>
    {
        public Task<Result> Handle(TestCommand request, CancellationToken cancellationToken)
        {
            if (request.ThrowException)
            {
                throw new InvalidOperationException("Test exception");
            }

            return Task.FromResult(Result.Success());
        }
    }
}
