using System.Runtime.InteropServices;
using Vortice.Vulkan;

namespace Veldrid.SPIRV
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct ReflectionInfo
    {
        public InteropArray VertexElements; // InteropArray<NativeVertexElementDescription>
        public InteropArray ResourceLayouts; // InteropArray<NativeResourceLayoutDescription>
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct NativeVertexElementDescription
    {
        public InteropArray Name; // InteropArray<byte>
        public VkFormat Format;
        public uint Offset;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct NativeResourceLayoutDescription
    {
        public InteropArray ResourceElements; // InteropArray<NativeResourceElementDescription>
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct NativeResourceElementDescription
    {
        public InteropArray Name; // InteropArray<byte>
        public VkDescriptorType Kind;
        public VkShaderStageFlags Stages;
        public VkDescriptorBindingFlags Options;
    }
}
