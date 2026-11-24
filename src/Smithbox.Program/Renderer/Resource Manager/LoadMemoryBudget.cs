using StudioCore.Application;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace StudioCore.Renderer;

/// <summary>
///     The memory a resource load may have in flight at once, and how many containers it reads at once: the trade
///     between how much memory a load peaks at and how quickly it finishes.
///     <para>
///         Bytes are counted from when an entry is read out of its container (decompressed) until it is done with:
///         a model, collision or navmesh once it is parsed, a texture once it is uploaded to the GPU. Reading waits
///         while the budget is used up, so a load can't read far ahead of what parsing and uploading can keep up
///         with. Without it, a large map reads everything in parallel and waits gigabytes of decompressed data on the
///         upload queue. See Claude/Documentation/Map Load Memory Analysis.md.
///     </para>
/// </summary>
public static class LoadMemoryBudget
{
    public enum Profile
    {
        Low = 0,
        Balanced = 1,
        High = 2,
        Unlimited = 3,
        Custom = 4
    }

    // A reader that has waited this long goes ahead anyway, so a load can never stall for good on memory that
    // something else is holding up (e.g. uploads that can't run while the main thread waits on the load)
    private const int MaxWaitMs = 30000;

    private static readonly object _lock = new();
    private static long _inFlight;
    private static long _peakInFlight;
    private static int _mainThreadId = -1;

    private static ConcurrentExclusiveSchedulerPair _readerSchedulers;
    private static int _readerSchedulerConcurrency;

    /// <summary>Bytes currently held by loads (read, not yet parsed or uploaded).</summary>
    public static long InFlight => Interlocked.Read(ref _inFlight);

    /// <summary>The most that has been held at once since <see cref="ResetPeak" />.</summary>
    public static long PeakInFlight => Interlocked.Read(ref _peakInFlight);

    public static void ResetPeak()
    {
        Interlocked.Exchange(ref _peakInFlight, InFlight);
    }

    /// <summary>
    ///     The budget in bytes, or 0 for none.
    /// </summary>
    public static long BudgetBytes
    {
        get
        {
            var mb = (Profile)RenderSettings.Viewport.Viewport_Load_Memory_Profile switch
            {
                Profile.Low => 512,
                // Between Low and High: on ER's m11_00_00_00, High (24 readers) never had more than 1.3 GB in flight,
                // so a larger budget here wouldn't limit anything
                Profile.Balanced => 1024,
                Profile.High => 8192,
                Profile.Unlimited => 0,
                _ => Math.Max(0, RenderSettings.Viewport.Viewport_Load_Memory_Budget_MB)
            };

            return mb * 1024L * 1024L;
        }
    }

    /// <summary>
    ///     How many containers are read at once, over every load job, or 0 for no limit.
    /// </summary>
    public static int ContainerReaders
    {
        get
        {
            var cores = Environment.ProcessorCount;

            return (Profile)RenderSettings.Viewport.Viewport_Load_Memory_Profile switch
            {
                Profile.Low => 2,
                Profile.Balanced => Math.Max(2, cores / 2),
                Profile.High => Math.Max(2, cores),
                Profile.Unlimited => 0,
                _ => Math.Max(0, RenderSettings.Viewport.Viewport_Load_Container_Readers)
            };
        }
    }

    /// <summary>
    ///     The scheduler that container reads run on. Shared by every job, so the limit holds however many jobs run at
    ///     once (a map load starts one for each of its objects' assets). Null for no limit.
    /// </summary>
    public static TaskScheduler ReaderScheduler
    {
        get
        {
            var readers = ContainerReaders;
            if (readers <= 0)
            {
                return null;
            }

            lock (_lock)
            {
                // A changed setting applies to jobs started after it; running ones keep the scheduler they have
                if (_readerSchedulers == null || _readerSchedulerConcurrency != readers)
                {
                    _readerSchedulers = new ConcurrentExclusiveSchedulerPair(TaskScheduler.Default, readers);
                    _readerSchedulerConcurrency = readers;
                }

                return _readerSchedulers.ConcurrentScheduler;
            }
        }
    }

    /// <summary>
    ///     Records the main thread, which must never wait: it runs the uploads that free the budget.
    /// </summary>
    public static void SetMainThread()
    {
        _mainThreadId = Environment.CurrentManagedThreadId;
    }

    /// <summary>
    ///     Waits until the budget has room, before reading more. Returns at once if there is no budget, nothing is in
    ///     flight (so an entry bigger than the whole budget still loads), or this is the main thread.
    /// </summary>
    public static void WaitForRoom()
    {
        var budget = BudgetBytes;
        if (budget <= 0 || Environment.CurrentManagedThreadId == _mainThreadId)
        {
            return;
        }

        var waited = 0;

        lock (_lock)
        {
            while (_inFlight > 0 && _inFlight >= budget && waited < MaxWaitMs)
            {
                // Woken by Release; the timeout also picks up a change of setting
                Monitor.Wait(_lock, 100);
                waited += 100;
                budget = BudgetBytes;

                if (budget <= 0)
                {
                    return;
                }
            }
        }
    }

    public static void Add(long bytes)
    {
        if (bytes <= 0)
        {
            return;
        }

        var now = Interlocked.Add(ref _inFlight, bytes);

        long peak;
        while (now > (peak = Interlocked.Read(ref _peakInFlight)) &&
               Interlocked.CompareExchange(ref _peakInFlight, now, peak) != peak)
        {
        }
    }

    public static void Release(long bytes)
    {
        if (bytes <= 0)
        {
            return;
        }

        Interlocked.Add(ref _inFlight, -bytes);

        lock (_lock)
        {
            Monitor.PulseAll(_lock);
        }
    }
}
