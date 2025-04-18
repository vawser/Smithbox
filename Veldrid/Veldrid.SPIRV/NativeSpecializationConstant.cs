using System.Runtime.InteropServices;

namespace Veldrid.SPIRV
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct NativeSpecializationConstant
    {
        public uint ID;
        public ulong Constant;
    }
}
