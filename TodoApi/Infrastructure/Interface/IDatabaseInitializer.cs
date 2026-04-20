namespace TodoApi.Infrastructure.Interface;

public interface IDatabaseInitializer
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
}