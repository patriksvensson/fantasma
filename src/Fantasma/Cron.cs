namespace Fantasma;

[PublicAPI]
public sealed class Cron
{
    public string Expression { get; }

    public Cron(string expression)
    {
        Expression = expression ?? throw new ArgumentNullException(nameof(expression));
    }
}