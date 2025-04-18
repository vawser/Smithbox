namespace Hexa.NET.ImGui.Widgets
{
    using Hexa.NET.ImGui;
    using Hexa.NET.Utilities;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    public static unsafe class ImGuiGC
    {
        private static Dictionary<uint, ImGuiGCData> trackingList = new();
        private static HashSet<uint> gcList = new();
        private static Queue<uint> gcFreeQueue = new();
        private static float gcInterval = 10.0f;
        private static long lastGCTick;
        private readonly static object gcLock = new();
        private static long lastAllocatedBytes;
        private static long allocatedBytes;
        private static long frequency;

        private struct ImGuiGCData
        {
            public uint Id;
            public void* Data;
            public int Size;
        }

        public static void Init()
        {
            ImGuiContextHook hook = new();
            hook.Callback = (void*)Marshal.GetFunctionPointerForDelegate<ImGuiContextHookCallback>(HookCallback);
            hook.Type = ImGuiContextHookType.EndFramePost;

            ImGuiP.AddContextHook(ImGui.GetCurrentContext(), &hook);
        }

        public static bool KeepAlive<T>(uint id, out T* ptr) where T : unmanaged
        {
            lock (gcLock)
            {
                if (trackingList.TryGetValue(id, out var gcData))
                {
                    gcList.Add(id);
                    ptr = (T*)gcData.Data;

                    return true;
                }
            }
            ptr = null;
            return false;
        }

        public static T* Alloc<T>(uint id) where T : unmanaged
        {
            return Alloc<T>(id, 1);
        }

        public static T* Alloc<T>(uint id, int count) where T : unmanaged
        {
            int size = count * sizeof(T);
            void* data = Utils.Alloc(size);
            Alloc(id, data, size);
            return (T*)data;
        }

        public static void Alloc(uint id, void* data, int size)
        {
            lock (gcLock)
            {
                ImGuiGCData gcData = new();
                gcData.Id = id;
                gcData.Data = data;
                gcData.Size = size;
                trackingList.Add(id, gcData);
                gcList.Add(id);
            }

            Interlocked.Add(ref allocatedBytes, size);
            Interlocked.Increment(ref frequency);
        }

        public static void Free(uint id)
        {
            lock (gcLock)
            {
                gcFreeQueue.Enqueue(id); // queue for deletion
            }
        }

        private static void FreeInternal(uint id)
        {
            var gcData = trackingList[id];
            Utils.Free(gcData.Data);
            Interlocked.Add(ref allocatedBytes, -gcData.Size);
            trackingList.Remove(id);
            gcList.Remove(id);
        }

        private static void HookCallback
#if NET5_0_OR_GREATER
            (ImGuiContext* ctx, ImGuiContextHook* hook)
#else
            (nint ctx, nint hook)
#endif
        {
            long memoryDelta = allocatedBytes - lastAllocatedBytes;
            if (frequency > 128 || memoryDelta > 4096)
            {
                Collect(); // force GC if too many objects are being created in a short time, or if memory usage is increasing too fast
                return;
            }

            // do a scheduled GC Collect.
            long now = Stopwatch.GetTimestamp();
            long next = lastGCTick + (long)(gcInterval * Stopwatch.Frequency);
            if (now >= next)
            {
                Collect();
            }
        }

        public static void Collect()
        {
            lock (gcLock)
            {
                lastGCTick = Stopwatch.GetTimestamp();
                foreach (var item in trackingList)
                {
                    if (!gcList.Contains(item.Key))
                    {
                        gcFreeQueue.Enqueue(item.Key);
                    }
                }
#if NETSTANDARD2_0
                while (gcFreeQueue.Count > 0)
                {
                    var id = gcFreeQueue.Dequeue();
                    FreeInternal(id);
                }
#else
                while (gcFreeQueue.TryDequeue(out uint id))
                {
                    FreeInternal(id);
                }
#endif
                gcList.Clear();
                frequency = 0;
                lastAllocatedBytes = allocatedBytes;
            }
        }

        public static void Shutdown()
        {
            lock (gcLock)
            {
                foreach (var item in trackingList)
                {
                    Utils.Free(item.Value.Data);
                }

                trackingList.Clear();
                gcList.Clear();
                gcFreeQueue.Clear();
            }
        }
    }
}