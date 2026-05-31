using System.Threading;

namespace HoverText.Core.Platform;

public sealed class SingleInstanceLock : IDisposable
{
    private readonly Mutex mutex;

    private SingleInstanceLock(Mutex mutex, bool isPrimaryInstance)
    {
        this.mutex = mutex;
        IsPrimaryInstance = isPrimaryInstance;
    }

    public bool IsPrimaryInstance { get; }

    public static SingleInstanceLock TryAcquire(string name)
    {
        var mutex = new Mutex(initiallyOwned: true, name, out bool createdNew);
        if (createdNew)
        {
            return new SingleInstanceLock(mutex, true);
        }

        bool acquired = false;
        try
        {
            acquired = mutex.WaitOne(0);
        }
        catch (AbandonedMutexException)
        {
            acquired = true;
        }

        return new SingleInstanceLock(mutex, acquired);
    }

    public void Dispose()
    {
        if (IsPrimaryInstance)
        {
            mutex.ReleaseMutex();
        }

        mutex.Dispose();
    }
}
