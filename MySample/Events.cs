using MassTransit;

namespace MySample;


public class FooRequested : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
}

public class FooCompleted : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
}

public class FazRequested {
    
}

public class FazCompleted 
{
    public string SomeValue;
}