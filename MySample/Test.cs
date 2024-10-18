
using MassTransit;
using MassTransit.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Xunit.Abstractions;


namespace MySample;


public class FooTests
{
    public FooTests(ITestOutputHelper output)
    {
        // Configure logging
        var loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Information() // Set minimum logging level
            .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Error)
            .MinimumLevel.Override("MassTransit", Serilog.Events.LogEventLevel.Error)
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss}] [{Level:u3}] {Message:lj}{NewLine}{Exception}");
        loggerConfig.WriteTo.TestOutput(output,
            outputTemplate: "[{Timestamp:HH:mm:ss}] [{Level:u3}] {Message:lj}{NewLine}{Exception}");

        Log.Logger = loggerConfig.CreateLogger();
    }

    [Fact]
    public async Task FooTest()
    {
        
        await using var provider = new ServiceCollection()
            .AddDbContext<FooSagaDbContext>(options =>
                options.UseSqlServer("Server=localhost,1435;Database=Foo;User Id=sa;password=myPassw0rd;MultipleActiveResultSets=true;TrustServerCertificate=True")) // 
            .AddMassTransitTestHarness(x =>
            {
                x.AddConsumer<FazConsumer>();
                x.AddSagaStateMachine<FooStateMachine, FooSaga>()
                    //.InMemoryRepository();
                    .EntityFrameworkRepository(r =>
                    {
                        r.ConcurrencyMode = ConcurrencyMode.Pessimistic;
                        r.ExistingDbContext<FooSagaDbContext>();
                    });

                x.UsingInMemory((context, cfg) =>
                {
                    cfg.UseInMemoryScheduler();

                    // Configure two endpoints for consumer
                    cfg.ReceiveEndpoint($"endpoint-1", e =>
                    {
                        e.ConfigureConsumer<FazConsumer>(context);
                    });
                    cfg.ReceiveEndpoint($"endpoint-2", e =>
                    {
                        e.ConfigureConsumer<FazConsumer>(context);
                    });

                    // Configure all endpoints automatically for other consumers that might be in the project:
                    cfg.ConfigureEndpoints(context);
                });
                
            })
            .BuildServiceProvider(true);

        using var scope = provider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<FooSagaDbContext>();
        await dbContext.Database.MigrateAsync();

        var harness = provider.GetRequiredService<ITestHarness>();

        await harness.Start();

        var correlationId = Guid.NewGuid();

        await harness.Bus.Publish(new FooRequested()
        {
            CorrelationId = correlationId,
        });

        var sagaHarness = harness.GetSagaStateMachineHarness<FooStateMachine, FooSaga>();

        await sagaHarness.Exists(correlationId, sm => sm.GetState("FazRequested.Pending"));
        await sagaHarness.Exists(correlationId, sm => sm.GetState("ItWorked"));
    }

}