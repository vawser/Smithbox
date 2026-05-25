# Tracy profiler C# bindings

﻿Tracy profiler interop for C#.

Requires **TracyClient.dll** to be present in current directory. You can build it by cloning the [Tracy Profiler](https://github.com/wolfpld/tracy) and building it with `BUILD_SHARED_LIBS=ON, TRACY_FIBERS=ON and TRACY_MANUAL_LIFETIME=ON`.

```sh
git clone https://github.com/wolfpld/tracy; cd tracy
cmake -S . -B build -DBUILD_SHARED_LIBS=ON -DTRACY_FIBERS=ON -DTRACY_MANUAL_LIFETIME=ON -DTRACY_DELAYED_INIT=ON
cmake --build ./build --config Release --target TracyClient
```

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
