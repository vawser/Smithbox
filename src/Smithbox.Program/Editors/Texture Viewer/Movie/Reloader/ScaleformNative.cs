using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace StudioCore.Editors.TextureViewer;

/// <summary>
/// Access to the game process for the Scaleform Reloader: memory reads and writes, an allocation near the exe, and
/// code patches with every game thread suspended (none may be running in the patched bytes).
/// </summary>
public sealed class ScaleformProcess : IDisposable
{
    public int ProcessId { get; }
    public IntPtr Handle { get; private set; }

    private const int PROCESS_ALL = 0x0008 | 0x0010 | 0x0020 | 0x0400 | 0x0002; // VM_OPERATION, VM_READ, VM_WRITE, QUERY_INFORMATION, CREATE_THREAD
    private const uint MEM_COMMIT = 0x1000, MEM_RESERVE = 0x2000, MEM_FREE = 0x10000;
    private const uint PAGE_EXECUTE_READWRITE = 0x40;

    public ScaleformProcess(int processId)
    {
        ProcessId = processId;
        Handle = OpenProcess(PROCESS_ALL, false, processId);
        if (Handle == IntPtr.Zero)
            throw new InvalidOperationException($"OpenProcess failed ({Marshal.GetLastWin32Error()})");
    }

    public void Dispose()
    {
        if (Handle != IntPtr.Zero)
        {
            CloseHandle(Handle);
            Handle = IntPtr.Zero;
        }
    }

    #region Memory
    public byte[] Read(ulong address, int count)
    {
        var buffer = new byte[count];
        if (!ReadProcessMemory(Handle, (IntPtr)(long)address, buffer, (UIntPtr)(uint)count, out var read) || (int)read != count)
            throw new InvalidOperationException($"Read of {count} bytes at {address:X} failed");
        return buffer;
    }

    public bool TryRead(ulong address, int count, out byte[] buffer)
    {
        buffer = new byte[count];
        return address != 0 && ReadProcessMemory(Handle, (IntPtr)(long)address, buffer, (UIntPtr)(uint)count, out var read) && (int)read == count;
    }

    public ulong U64(ulong address) => BitConverter.ToUInt64(Read(address, 8));
    public uint U32(ulong address) => BitConverter.ToUInt32(Read(address, 4));
    public byte U8(ulong address) => Read(address, 1)[0];

    public ulong TryU64(ulong address) => TryRead(address, 8, out var b) ? BitConverter.ToUInt64(b) : 0;

    /// <summary>A null-terminated UTF-16 string, at most maxChars.</summary>
    public string WideString(ulong address, int maxChars = 128)
    {
        if (address == 0 || !TryRead(address, maxChars * 2, out var b))
            return "";
        var s = Encoding.Unicode.GetString(b);
        var end = s.IndexOf('\0');
        return end >= 0 ? s[..end] : s;
    }

    /// <summary>A UTF-16 string of a known length.</summary>
    public string WideString(ulong address, int length, bool exact)
    {
        if (address == 0 || length <= 0 || length > 512 || !TryRead(address, length * 2, out var b))
            return "";
        return Encoding.Unicode.GetString(b);
    }

    public void Write(ulong address, byte[] data)
    {
        if (!WriteProcessMemory(Handle, (IntPtr)(long)address, data, (UIntPtr)(uint)data.Length, out var written) || (int)written != data.Length)
            throw new InvalidOperationException($"Write of {data.Length} bytes at {address:X} failed");
    }

    public void WriteU64(ulong address, ulong value) => Write(address, BitConverter.GetBytes(value));
    public void WriteU32(ulong address, uint value) => Write(address, BitConverter.GetBytes(value));
    #endregion

    #region Allocation
    /// <summary>
    /// Commits read/write/execute memory within reach of a 5-byte jmp from the exe image (±2 GB of it), by walking the
    /// free regions below, then above, the image.
    /// </summary>
    public ulong AllocateNear(ulong imageBase, ulong imageSize, int size)
    {
        const ulong Granularity = 0x10000;
        const ulong Reach = 0x7FF00000;

        ulong low = imageBase + imageSize > Reach ? imageBase + imageSize - Reach : Granularity;
        ulong high = imageBase + Reach;

        foreach (var downward in new[] { true, false })
        {
            ulong address = downward ? (imageBase - Granularity) & ~(Granularity - 1) : (imageBase + imageSize + Granularity) & ~(Granularity - 1);
            for (int tries = 0; tries < 4096 && address >= low && address < high; tries++)
            {
                if (VirtualQueryEx(Handle, (IntPtr)(long)address, out var info, (uint)Marshal.SizeOf<MEMORY_BASIC_INFORMATION>()) == 0)
                    break;

                var regionBase = (ulong)(long)info.BaseAddress;
                var regionSize = (ulong)(long)info.RegionSize;

                if (info.State == MEM_FREE && regionSize >= (ulong)size)
                {
                    var candidate = downward ? (regionBase + regionSize - (ulong)size) & ~(Granularity - 1) : (regionBase + Granularity - 1) & ~(Granularity - 1);
                    if (candidate >= regionBase && candidate + (ulong)size <= regionBase + regionSize)
                    {
                        var got = VirtualAllocEx(Handle, (IntPtr)(long)candidate, (UIntPtr)(uint)size, MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);
                        if (got != IntPtr.Zero)
                            return (ulong)(long)got;
                    }
                }

                address = downward ? (regionBase - Granularity) & ~(Granularity - 1) : regionBase + regionSize;
                if (downward && regionBase < Granularity)
                    break;
            }
        }

        throw new InvalidOperationException("No free memory within 2 GB of the game's exe");
    }
    #endregion

    #region Patching
    /// <summary>
    /// Writes code patches with every thread of the game suspended. A patch is only written while no thread's
    /// instruction pointer is inside any patched range (retried a few times otherwise).
    /// </summary>
    public void PatchWithThreadsSuspended(List<(ulong Address, byte[] Bytes)> patches)
    {
        for (int attempt = 0; attempt < 20; attempt++)
        {
            var threads = SuspendAll();
            try
            {
                if (!AnyThreadInside(threads, patches))
                {
                    foreach (var (address, bytes) in patches)
                    {
                        VirtualProtectEx(Handle, (IntPtr)(long)address, (UIntPtr)(uint)bytes.Length, PAGE_EXECUTE_READWRITE, out var old);
                        Write(address, bytes);
                        VirtualProtectEx(Handle, (IntPtr)(long)address, (UIntPtr)(uint)bytes.Length, old, out _);
                        FlushInstructionCache(Handle, (IntPtr)(long)address, (UIntPtr)(uint)bytes.Length);
                    }
                    return;
                }
            }
            finally
            {
                ResumeAll(threads);
            }

            System.Threading.Thread.Sleep(5);
        }

        throw new InvalidOperationException("A game thread kept running in the code to patch");
    }

    private List<IntPtr> SuspendAll()
    {
        var threads = new List<IntPtr>();
        var snapshot = CreateToolhelp32Snapshot(0x4, 0); // TH32CS_SNAPTHREAD
        if (snapshot == (IntPtr)(-1))
            throw new InvalidOperationException("Thread snapshot failed");

        try
        {
            var entry = new THREADENTRY32 { dwSize = (uint)Marshal.SizeOf<THREADENTRY32>() };
            if (Thread32First(snapshot, ref entry))
            {
                do
                {
                    if (entry.th32OwnerProcessID != ProcessId)
                        continue;

                    var thread = OpenThread(0x0002 | 0x0008, false, entry.th32ThreadID); // SUSPEND_RESUME | GET_CONTEXT
                    if (thread == IntPtr.Zero)
                        continue;

                    if (SuspendThread(thread) == uint.MaxValue)
                    {
                        CloseHandle(thread);
                        continue;
                    }
                    threads.Add(thread);
                }
                while (Thread32Next(snapshot, ref entry));
            }
        }
        finally
        {
            CloseHandle(snapshot);
        }

        return threads;
    }

    private static void ResumeAll(List<IntPtr> threads)
    {
        foreach (var thread in threads)
        {
            ResumeThread(thread);
            CloseHandle(thread);
        }
    }

    private static unsafe bool AnyThreadInside(List<IntPtr> threads, List<(ulong Address, byte[] Bytes)> patches)
    {
        // CONTEXT is 1232 bytes, 16-byte aligned; Rip at 0xF8, ContextFlags at 0x30
        var context = (byte*)NativeMemory.AlignedAlloc(1232, 16);
        try
        {
            foreach (var thread in threads)
            {
                new Span<byte>(context, 1232).Clear();
                *(uint*)(context + 0x30) = 0x00100001; // CONTEXT_AMD64 | CONTEXT_CONTROL
                if (!GetThreadContext(thread, (IntPtr)context))
                    continue;

                var rip = *(ulong*)(context + 0xF8);
                foreach (var (address, bytes) in patches)
                {
                    if (rip >= address && rip < address + (ulong)bytes.Length)
                        return true;
                }
            }
        }
        finally
        {
            NativeMemory.AlignedFree(context);
        }
        return false;
    }
    #endregion

    #region Kernel32
    [StructLayout(LayoutKind.Sequential)]
    private struct MEMORY_BASIC_INFORMATION
    {
        public IntPtr BaseAddress;
        public IntPtr AllocationBase;
        public uint AllocationProtect;
        public ushort PartitionId;
        public IntPtr RegionSize;
        public uint State;
        public uint Protect;
        public uint Type;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct THREADENTRY32
    {
        public uint dwSize;
        public uint cntUsage;
        public uint th32ThreadID;
        public uint th32OwnerProcessID;
        public int tpBasePri;
        public int tpDeltaPri;
        public uint dwFlags;
    }

    [DllImport("kernel32.dll", SetLastError = true)] private static extern IntPtr OpenProcess(int access, bool inherit, int pid);
    [DllImport("kernel32.dll", SetLastError = true)] private static extern bool CloseHandle(IntPtr handle);
    [DllImport("kernel32.dll", SetLastError = true)] private static extern bool ReadProcessMemory(IntPtr process, IntPtr address, byte[] buffer, UIntPtr size, out UIntPtr read);
    [DllImport("kernel32.dll", SetLastError = true)] private static extern bool WriteProcessMemory(IntPtr process, IntPtr address, byte[] buffer, UIntPtr size, out UIntPtr written);
    [DllImport("kernel32.dll", SetLastError = true)] private static extern IntPtr VirtualAllocEx(IntPtr process, IntPtr address, UIntPtr size, uint type, uint protect);
    [DllImport("kernel32.dll", SetLastError = true)] private static extern UIntPtr VirtualQueryEx(IntPtr process, IntPtr address, out MEMORY_BASIC_INFORMATION info, uint length);
    [DllImport("kernel32.dll", SetLastError = true)] private static extern bool VirtualProtectEx(IntPtr process, IntPtr address, UIntPtr size, uint protect, out uint old);
    [DllImport("kernel32.dll", SetLastError = true)] private static extern bool FlushInstructionCache(IntPtr process, IntPtr address, UIntPtr size);
    [DllImport("kernel32.dll", SetLastError = true)] private static extern IntPtr CreateToolhelp32Snapshot(uint flags, uint pid);
    [DllImport("kernel32.dll", SetLastError = true)] private static extern bool Thread32First(IntPtr snapshot, ref THREADENTRY32 entry);
    [DllImport("kernel32.dll", SetLastError = true)] private static extern bool Thread32Next(IntPtr snapshot, ref THREADENTRY32 entry);
    [DllImport("kernel32.dll", SetLastError = true)] private static extern IntPtr OpenThread(uint access, bool inherit, uint tid);
    [DllImport("kernel32.dll", SetLastError = true)] private static extern uint SuspendThread(IntPtr thread);
    [DllImport("kernel32.dll", SetLastError = true)] private static extern uint ResumeThread(IntPtr thread);
    [DllImport("kernel32.dll", SetLastError = true)] private static extern bool GetThreadContext(IntPtr thread, IntPtr context);
    #endregion
}
