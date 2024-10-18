
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
            })
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();

        await harness.Start();

        var correlationId = Guid.NewGuid();
        
        await harness.Bus.Publish(new FooRequested()
        {
            CorrelationId = Guid.NewGuid(),
        });
        
        var sagaHarness = harness.GetSagaStateMachineHarness<FooStateMachine, FooSaga>();
        
        await sagaHarness.Exists(correlationId, sm => sm.GetState("FazRequested.Pending"));
        await sagaHarness.Exists(correlationId, sm => sm.GetState("ItWorked"));

    }

}