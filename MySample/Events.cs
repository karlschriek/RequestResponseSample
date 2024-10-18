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

public class FazRequested : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
}

public class FazCompleted : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }

    public string SomeValue;
}