using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MySample;

public class FooSagaClassMap : SagaClassMap<FooSaga>
{
    protected override void Configure(EntityTypeBuilder<FooSaga> entity, ModelBuilder modelBuilder)
    {
        entity.Property(x => x.CurrentState).HasMaxLength(64);;
    }
}



public class FooSagaDbContext : SagaDbContext
{

    public FooSagaDbContext(DbContextOptions<FooSagaDbContext> options)
        : base(options)
    { }

    // Implement the Configurations property
    protected override IEnumerable<ISagaClassMap> Configurations
    {
        get
        {
            // Return the saga class maps for the sagas this context manages
            yield return new FooSagaClassMap();
        }
        
    }
}

public class FooSagaDbContextFactory : IDesignTimeDbContextFactory<FooSagaDbContext>
{
    public FooSagaDbContext CreateDbContext(string[] args)
    {

        var optionsBuilder = new DbContextOptionsBuilder<FooSagaDbContext>();

        // Use SQL Server provider
        optionsBuilder.UseSqlServer("Server=localhost,1435;Database=Foo;User Id=sa;password=myPassw0rd;MultipleActiveResultSets=true;TrustServerCertificate=True");

        return new FooSagaDbContext(optionsBuilder.Options);
    }
}
