namespace Fantasma;

public interface IJobProvider
{
    TimeSpan Sleep { get; }
    IJobStorage GetStorage();
}