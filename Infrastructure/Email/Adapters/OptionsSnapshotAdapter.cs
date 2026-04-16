using Microsoft.Extensions.Options;
namespace Infrastructure.Email.Adapters;

public sealed class OptionsSnapshotAdapter<T>(IOptions<T> options) : IOptionsSnapshot<T>
    where T : class
{
    public T Value { get; } = options.Value;

    public T Get(string? name) => Value;
}