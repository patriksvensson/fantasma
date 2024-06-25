namespace Fantasma.Internal;

internal sealed class SleepPreference
{
    public TimeSpan Time { get; }

    public SleepPreference(TimeSpan time)
    {
        Time = time;
    }
}