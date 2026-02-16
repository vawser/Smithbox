// Tracy profiler interop for C#
// Requires TracyClient.dll to be present in current directory
// You can build it by cloning https://github.com/wolfpld/tracy
// and building it with BUILD_SHARED_LIBS=ON, TRACY_FIBERS=ON and TRACY_MANUAL_LIFETIME=ON
//
// cmake -S . -B build -DBUILD_SHARED_LIBS=ON -DTRACY_FIBERS=ON -DTRACY_MANUAL_LIFETIME=ON -DTRACY_DELAYED_INIT=ON
// cmake --build ./build --config Release --target TracyClient

using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Tracy;

public unsafe class Profiler
{
#if DEBUG
    public static bool EnableTracy = true;
#else
    public static bool EnableTracy =
        !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SMITHBOX_PROFILER")) &&
        Environment.GetEnvironmentVariable("SMITHBOX_PROFILER") != "0";
#endif

    [DllImport("TracyClient.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern void ___tracy_startup_profiler();

    [DllImport("TracyClient.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern void ___tracy_shutdown_profiler();

    public static void Startup()
    {
        if (!EnableTracy) return;

        ___tracy_startup_profiler();
    }

    public static void Shutdown()
    {
        if (!EnableTracy) return;

        ___tracy_shutdown_profiler();
    }

    [DllImport("TracyClient.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern ___tracy_c_zone_context ___tracy_emit_zone_begin(___tracy_source_location_data* srcloc,
        int active);

    [DllImport("TracyClient.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern ___tracy_c_zone_context ___tracy_emit_zone_begin_callstack(
        ___tracy_source_location_data* srcloc, int depth, int active);

    [DllImport("TracyClient.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern ___tracy_c_zone_context ___tracy_emit_zone_begin_alloc(ulong srcloc, int active);

    [DllImport("TracyClient.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern ___tracy_c_zone_context ___tracy_emit_zone_begin_alloc_callstack(ulong srcloc, int depth,
        int active);

    [DllImport("TracyClient.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern void ___tracy_emit_zone_end(___tracy_c_zone_context ctx);

    [DllImport("TracyClient.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern void ___tracy_emit_zone_text(___tracy_c_zone_context ctx, string txt, ulong size);

    [DllImport("TracyClient.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern void ___tracy_emit_zone_name(___tracy_c_zone_context ctx, string txt, ulong size);

    [DllImport("TracyClient.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern void ___tracy_emit_zone_color(___tracy_c_zone_context ctx, uint color);

    [DllImport("TracyClient.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern void ___tracy_emit_zone_value(___tracy_c_zone_context ctx, ulong value);

    [DllImport("TracyClient.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern ulong ___tracy_alloc_srcloc(uint line, string source, ulong sourceSz, string function,
        ulong functionSz);

    [DllImport("TracyClient.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern ulong ___tracy_alloc_srcloc_name(uint line, string source, ulong sourceSz,
        string function, ulong functionSz, string name, ulong nameSz);

    public sealed class TracyZone : IDisposable
    {
        private ___tracy_c_zone_context _ctx;
        private bool _disposed;

        public TracyZone(___tracy_c_zone_context ctx) => _ctx = ctx;

        public void Dispose()
        {
            if (!_disposed)
            {
                TracyCZoneEnd(_ctx);
                _disposed = true;
            }
        }
    }

    public static TracyZone TracyZoneAuto(string? name = null, uint color = 0, int active = 1,
            [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
    {
        ___tracy_c_zone_context ctx;
        if (!string.IsNullOrEmpty(name))
        {
            ctx = TracyCZoneN(active, name!, memberName, sourceFilePath, sourceLineNumber);
            if (color != 0)
            {
                ___tracy_c_zone_context cctx = TracyCZoneNC(active, name!, color, memberName, sourceFilePath, sourceLineNumber);
                ctx = cctx;
            }
        }
        else if (color != 0)
        {
            ctx = Profiler.TracyCZoneC(active, color, memberName, sourceFilePath, sourceLineNumber);
        }
        else
        {
            ctx = Profiler.TracyCZone(active, memberName, sourceFilePath, sourceLineNumber);
        }

        return new TracyZone(ctx);
    }

    public static ___tracy_c_zone_context TracyCZone(int active,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        ___tracy_c_zone_context result = new();
        if (EnableTracy)
        {
            var id = ___tracy_alloc_srcloc((uint)sourceLineNumber, sourceFilePath, (ulong)sourceFilePath.Length,
                memberName, (ulong)memberName.Length);
            result = ___tracy_emit_zone_begin_alloc(id, active);
        }

        return result;
    }

    public static ___tracy_c_zone_context TracyCZoneN(int active,
        string name,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        ___tracy_c_zone_context result = new();
        if (EnableTracy)
        {
            var id = ___tracy_alloc_srcloc_name((uint)sourceLineNumber, sourceFilePath,
                (ulong)sourceFilePath.Length, memberName, (ulong)memberName.Length, name, (ulong)name.Length);
            result = ___tracy_emit_zone_begin_alloc(id, active);
        }

        return result;
    }

    public static ___tracy_c_zone_context TracyCZoneC(int active,
        uint color,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        ___tracy_c_zone_context result = new();
        if (EnableTracy)
        {
            var id = ___tracy_alloc_srcloc((uint)sourceLineNumber, sourceFilePath, (ulong)sourceFilePath.Length,
                memberName, (ulong)memberName.Length);
            result = ___tracy_emit_zone_begin_alloc(id, active);
        }

        ___tracy_emit_zone_color(result, color);
        return result;
    }

    public static ___tracy_c_zone_context TracyCZoneNC(int active,
        string name,
        uint color,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        ___tracy_c_zone_context result = new();
        if (EnableTracy)
        {
            var id = ___tracy_alloc_srcloc_name((uint)sourceLineNumber, sourceFilePath,
                (ulong)sourceFilePath.Length, memberName, (ulong)memberName.Length, name, (ulong)name.Length);
            result = ___tracy_emit_zone_begin_alloc(id, active);
            ___tracy_emit_zone_color(result, color);
        }

        return result;
    }

    public static void TracyCZoneEnd(___tracy_c_zone_context ctx)
    {
        if (!EnableTracy) return;

        ___tracy_emit_zone_end(ctx);
    }

    [DllImport("TracyClient.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern void ___tracy_fiber_enter(IntPtr namePtr);

    [DllImport("TracyClient.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern void ___tracy_fiber_leave();

    private static readonly ConcurrentDictionary<string, IntPtr> s_fiberNamePtrs = new();
    private static readonly object s_fiberNameLock = new();

    private static IntPtr GetOrCreateFiberNamePtr(string name)
    {
        lock (s_fiberNameLock)
        {
            if (s_fiberNamePtrs.TryGetValue(name, out var ptr)) return ptr;
            var p = Marshal.StringToHGlobalAnsi(name);
            s_fiberNamePtrs[name] = p;
            return p;
        }
    }

    public static void TracyFiberEnter(string name)
    {
        if (!EnableTracy) return;

        var p = GetOrCreateFiberNamePtr(name);
        try { ___tracy_fiber_enter(p); } catch { }
    }

    public static void TracyFiberLeave()
    {
        if (!EnableTracy) return;

        try { ___tracy_fiber_leave(); } catch { }
    }

    [DllImport("TracyClient.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern void ___tracy_emit_frame_mark(string? name);

    [DllImport("TracyClient.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern void ___tracy_emit_frame_mark_start(string name);

    [DllImport("TracyClient.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern void ___tracy_emit_frame_mark_end(string name);

    public static void TracyCFrameMark()
    {
        if (!EnableTracy) return;

        ___tracy_emit_frame_mark(null);
    }

    public static void TracyCFrameMarkNamed(string name)
    {
        if (!EnableTracy) return;

        ___tracy_emit_frame_mark(name);
    }

    public static void TracyCFrameMarkStart(string name)
    {
        if (!EnableTracy) return;

        ___tracy_emit_frame_mark_start(name);
    }

    public static void TracyCFrameMarkEnd(string name)
    {
        if (!EnableTracy) return;

        ___tracy_emit_frame_mark_end(name);
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    private struct ___tracy_source_location_data
    {
        public char* name;
        public char* function;
        public char* file;
        public uint line;
        public uint color;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct ___tracy_c_zone_context
    {
        public uint id;
        public int active;

        public ___tracy_c_zone_context()
        {
            id = 0;
            active = 0;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    private struct ___tracy_gpu_time_data
    {
        public long gpuTime;
        public ushort queryId;
        public byte context;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    private struct ___tracy_gpu_zone_begin_data
    {
        public ulong srcloc;
        public ushort queryId;
        public byte context;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    private struct ___tracy_gpu_zone_end_data
    {
        public ushort queryId;
        public byte context;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    private struct ___tracy_gpu_new_context_data
    {
        public long gpuTime;
        public float period;
        public byte context;
        public byte flags;
        public byte type;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    private struct ___tracy_gpu_context_name_data
    {
        public byte context;
        [MarshalAs(UnmanagedType.LPStr)] public string name;
        public ushort len;
    }

    public class TracyCZoneCtx
    {
        internal ___tracy_c_zone_context _ctx;

        public TracyCZoneCtx(___tracy_c_zone_context val)
        {
            _ctx = val;
        }
    }
}
