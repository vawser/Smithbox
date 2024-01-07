using System;
using System.Runtime.InteropServices;

namespace Veldrid.SPIRV
{
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct CompilationResult
    {
        public Bool32 Succeeded;
        public InteropArray DataBuffers;
        public ReflectionInfo ReflectionInfo;

        public uint GetLength(uint index)
        {
            if (index >= DataBuffers.Count) { throw new ArgumentOutOfRangeException(nameof(index)); }
            return DataBuffers.Ref<InteropArray>(index).Count;
        }

        public void* GetData(uint index)
        {
            if (index >= DataBuffers.Count) { throw new ArgumentOutOfRangeException(nameof(index)); }
            return DataBuffers.Ref<InteropArray>(index).Data;
        }
    }
}
