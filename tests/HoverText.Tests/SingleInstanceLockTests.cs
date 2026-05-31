using HoverText.Core.Platform;

namespace HoverText.Tests;

[TestClass]
public sealed class SingleInstanceLockTests
{
    [TestMethod]
    public void TryAcquire_allows_only_one_owner_for_same_name()
    {
        string name = $"HoverText.Tests.{Guid.NewGuid():N}";

        using SingleInstanceLock first = SingleInstanceLock.TryAcquire(name);
        bool secondIsPrimary = TryAcquireOnWorkerThread(name);

        Assert.IsTrue(first.IsPrimaryInstance);
        Assert.IsFalse(secondIsPrimary);
    }

    [TestMethod]
    public void TryAcquire_releases_name_after_dispose()
    {
        string name = $"HoverText.Tests.{Guid.NewGuid():N}";

        using (SingleInstanceLock first = SingleInstanceLock.TryAcquire(name))
        {
            Assert.IsTrue(first.IsPrimaryInstance);
        }

        using SingleInstanceLock second = SingleInstanceLock.TryAcquire(name);

        Assert.IsTrue(second.IsPrimaryInstance);
    }

    private static bool TryAcquireOnWorkerThread(string name)
    {
        bool result = true;
        var thread = new Thread(() =>
        {
            using SingleInstanceLock second = SingleInstanceLock.TryAcquire(name);
            result = second.IsPrimaryInstance;
        });
        thread.Start();
        thread.Join();
        return result;
    }
}
