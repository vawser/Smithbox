# Tracy pfofiler C# bindings

## Usage

```c#
using Tracy;

static example() {
    using var __scope = Profiler.TracyZoneAuto();
    var foo = 1 ** 1024;
    // sub zone limited to specific function call
    using var __limitedScope = Profiler.TracyZoneAuto();
    someSlowFunction();
    // Dispose ends limited zone
    __limitedScope.Dispose();
    // full function zone will end automatically when disposed
};
```

### Async

Since spans are thread-scoped, and C# async allows for the thread migration, you need to use tracy fibers.

```c#
using Tracy;

static async Task example()
{
    Profiler.TracyFiberEnter("ExampleTask");
    using var __scope = Profiler.TracyZoneAuto();
    var foo = 1 ** 1024;
    // Before each await, use `TracyFiberLeave`
    Profiler.TracyFiberLeave();
    await Task.Delay(2000);
    // After that you can enter this fiber again
    Profiler.TracyFiberEnter("ExampleTask");
    var bar = 1 ** 1024;
};
```
