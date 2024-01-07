using System;
using System.Collections.Generic;
using System.Text;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace SoulsFormats
{
    /// <summary>
    /// An object that caches certain large buffers created when reading a flver. The intent
    /// of this class is to reduce GC pressure when mass reading many flvers that are short
    /// lived because they will eventually be sent to GPU memory and released
    /// </summary>
    public class FlverCache
    {
        private const int MaxCached = 5;
        internal List<FLVER.Vertex[]> VerticesArrayCache = new List<FLVER.Vertex[]>(MaxCached);
        internal List<bool> VerticesArrayUsed = new List<bool>(MaxCached);

        internal ArrayPool<int> IndicesPool = ArrayPool<int>.Create();
        internal List<int[]> LoanedIndices = new List<int[]>();

        public unsafe long MemoryUsage
        {
            get
            {
                long usage = 0;
                foreach (var a in VerticesArrayCache)
                {
                    usage += a.Length * Unsafe.SizeOf<FLVER.Vertex>();
                }
                return usage;
            }
        }

        public FlverCache()
        {
        }

        internal FLVER.Vertex[] GetCachedVertexArray(int size)
        {
            for (int i = 0; i < VerticesArrayCache.Count; i++) 
            {
                if (VerticesArrayCache[i].Length >= size && !VerticesArrayUsed[i])
                {
                    VerticesArrayUsed[i] = true;
                    return VerticesArrayCache[i];
                }
            }
            return null;
        }

        internal void CacheVertexArray(FLVER.Vertex[] tocache)
        {
            int maxval = int.MaxValue;
            int replaceindex = -1;
            if (VerticesArrayCache.Count < MaxCached)
            {
                VerticesArrayCache.Add(tocache);
                VerticesArrayUsed.Add(true);
                return;
            }

            for (int i = 0; i < VerticesArrayCache.Count; i++)
            {
                if (VerticesArrayCache[i].Length < maxval)
                {
                    maxval = VerticesArrayCache[i].Length;
                    replaceindex = i;
                }
            }

            if (replaceindex != -1 && tocache.Length > VerticesArrayCache[replaceindex].Length)
            {
                VerticesArrayCache[replaceindex] = tocache;
                VerticesArrayUsed[replaceindex] = true;
            }
        }

        public void ResetUsage()
        {
            for (int i = 0; i < VerticesArrayUsed.Count; i++)
            {
                VerticesArrayUsed[i] = false;
            }

            foreach (var l in LoanedIndices)
            {
                IndicesPool.Return(l);
            }
        }
    }
}
