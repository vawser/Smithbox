using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Veldrid.SPIRV
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal unsafe struct InteropArray
    {
        public uint Count;
        public void* Data;

        public InteropArray(uint count, void* data)
        {
            Count = count;
            Data = data;
        }

        public ref T Ref<T>(int index)
        {
            if (index >= Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return ref Unsafe.AsRef<T>((byte*)Data + (index * Unsafe.SizeOf<T>()));
        }

        public ref T Ref<T>(uint index)
        {
            if (index >= Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return ref Unsafe.AsRef<T>((byte*)Data + (index * Unsafe.SizeOf<T>()));
        }
    }
}
