namespace MySample;

using MassTransit;

public class FooStateMachine : MassTransitStateMachine<FooSaga>
{
   
    public FooStateMachine()
    {
        InstanceState(x => x.CurrentState);
        
        
        Request(() => FazRequested, o => { });

        Initially(
            When(FooRequested)
                .Then(async context =>
                {
                    context.Saga.CorrelationId = context.Message.CorrelationId;
                })
                .Request(FazRequested, 
                    context => new Uri($"queue:endpoint-1"),
                    context => new FazRequested {}
                    )
                .TransitionTo(FazRequested.Pending)
        );
        
        During(FazRequested.Pending,
            When(FazRequested.Completed)
                .Then(context =>
                {
                    Console.WriteLine(context.Message);
                })
                .TransitionTo(ItWorked)
        );
        
    }

    public State ItWorked { get; }

    public Request<FooSaga, FazRequested, FazCompleted> FazRequested { get; private set; } = null!;

    public Event<FooRequested> FooRequested { get; private set; } = null!;

}


public class FooSaga : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    

    // // If using Optimistic concurrency, this property is required
    // public byte[] RowVersion { get; set; }
    
    public string CurrentState { get; set; }
    
}
