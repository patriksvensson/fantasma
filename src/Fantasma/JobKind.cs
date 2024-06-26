namespace Fantasma;

[PublicAPI]
public enum JobKind
{
    Queued = 0,
    Delayed = 1,
    Recurring = 2,
}