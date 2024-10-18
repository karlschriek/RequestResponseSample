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
                    context => new FazRequested
                    {
                    })
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
    
    public State CurrentState { get; set; }
    
}
