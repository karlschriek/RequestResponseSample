using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MySample;

public class FazConsumer : IConsumer<FazRequested>
{
    public FazConsumer(ILogger<FazConsumer> logger)
    {
    }

    public async Task Consume(ConsumeContext<FazRequested> context)
    {
        await context.RespondAsync(new FazCompleted()
        {
            SomeValue = "faznez",  // Attach this consumer's ID
        });

    }
}


