
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;


namespace MySample;


public class FooTests
{
    public FooTests(ITestOutputHelper output)
    {
    }

    [Fact]
    public async Task FooTest()
    {
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
            {
                
                x.AddConsumer<FazConsumer>();
                x.AddSagaStateMachine<FooStateMachine, FooSaga>()
                    .InMemoryRepository();
                

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

        var harness = provider.GetRequiredService<ITestHarness>();

        await harness.Start();

        var correlationId = new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        
        await harness.Bus.Publish(new FooRequested()
        {
            CorrelationId = correlationId,
        });
        
        var sagaHarness = harness.GetSagaStateMachineHarness<FooStateMachine, FooSaga>();
        
        await sagaHarness.Exists(correlationId, sm => sm.GetState("FazRequested.Pending"));
        await sagaHarness.Exists(correlationId, sm => sm.GetState("ItWorked"));

    }

}