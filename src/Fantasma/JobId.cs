namespace Fantasma;

[PublicAPI]
public sealed class JobId
{
    public string Id { get; }

    public JobId(string id)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
    }
}