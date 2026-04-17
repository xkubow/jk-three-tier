namespace JK.Playground.Services;

public static class CounterHelper
{
    private static int _counter;

    public static int Increment()
    {
        Interlocked.Increment(ref _counter);
        return _counter;
    }

    public static int Decrement()
    {
        Interlocked.Decrement(ref _counter);
        return _counter;
    }

    public static int Get()
    {
        return Volatile.Read(ref _counter);
    }
}