using System;
using System.Diagnostics;
using Vortice.Vulkan;

namespace Veldrid
{
    public static class FormatHelpers
    {
        public static uint GetSizeInBytes(VkFormat format)
        {
            switch (format)
            {
                case VkFormat.R8Unorm:
                case VkFormat.R8Snorm:
                case VkFormat.R8Uint:
                case VkFormat.R8Sint:
                    return 1;

                case VkFormat.B5G5R5A1UnormPack16:
                case VkFormat.R16Unorm:
                case VkFormat.R16Snorm:
                case VkFormat.R16Uint:
                case VkFormat.R16Sint:
                case VkFormat.R16Sfloat:
                case VkFormat.D16Unorm:
                case VkFormat.R8G8Unorm:
                case VkFormat.R8G8Snorm:
                case VkFormat.R8G8Uint:
                case VkFormat.R8G8Sint:
                    return 2;

                case VkFormat.R32Uint:
                case VkFormat.R32Sint:
                case VkFormat.R32Sfloat:
                case VkFormat.D32Sfloat:
                case VkFormat.R16G16Unorm:
                case VkFormat.R16G16Snorm:
                case VkFormat.R16G16Uint:
                case VkFormat.R16G16Sint:
                case VkFormat.R16G16Sfloat:
                case VkFormat.R8G8B8A8Unorm:
                case VkFormat.R8G8B8A8Srgb:
                case VkFormat.R8G8B8A8Snorm:
                case VkFormat.R8G8B8A8Uint:
                case VkFormat.R8G8B8A8Sint:
                case VkFormat.B8G8R8A8Unorm:
                case VkFormat.B8G8R8A8Srgb:
                case VkFormat.A2R10G10B10UnormPack32:
                case VkFormat.A2R10G10B10UintPack32:
                case VkFormat.B10G11R11UfloatPack32:
                case VkFormat.D24UnormS8Uint:
                    return 4;

                case VkFormat.D32SfloatS8Uint:
                    return 5;

                case VkFormat.R16G16B16A16Unorm:
                case VkFormat.R16G16B16A16Snorm:
                case VkFormat.R16G16B16A16Uint:
                case VkFormat.R16G16B16A16Sint:
                case VkFormat.R16G16B16A16Sfloat:
                case VkFormat.R32G32Uint:
                case VkFormat.R32G32Sint:
                case VkFormat.R32G32Sfloat:
                    return 8;
                
                case VkFormat.R32G32B32Sfloat:
                case VkFormat.R32G32B32Sint:
                case VkFormat.R32G32B32Uint:
                    return 12;

                case VkFormat.R32G32B32A32Sfloat:
                case VkFormat.R32G32B32A32Uint:
                case VkFormat.R32G32B32A32Sint:
                    return 16;

                case VkFormat.Bc1RgbUnormBlock:
                case VkFormat.Bc1RgbSrgbBlock:
                case VkFormat.Bc1RgbaUnormBlock:
                case VkFormat.Bc1RgbaSrgbBlock:
                case VkFormat.Bc2UnormBlock:
                case VkFormat.Bc2SrgbBlock:
                case VkFormat.Bc3UnormBlock:
                case VkFormat.Bc3SrgbBlock:
                case VkFormat.Bc4UnormBlock:
                case VkFormat.Bc4SnormBlock:
                case VkFormat.Bc5UnormBlock:
                case VkFormat.Bc5SnormBlock:
                case VkFormat.Bc7UnormBlock:
                case VkFormat.Bc7SrgbBlock:
                case VkFormat.Etc2R8G8B8UnormBlock:
                case VkFormat.Etc2R8G8B8A1UnormBlock:
                case VkFormat.Etc2R8G8B8A8UnormBlock:
                    Debug.Fail("GetSizeInBytes should not be used on a compressed format.");
                    throw Illegal.Value<VkFormat>();
                default:
                    Debug.Fail("Unimplemented VKFormat.");
                    throw Illegal.Value<VkFormat>();
            }
        }

        public static uint GetSampleCountUInt32(VkSampleCountFlags sampleCount)
        {
            switch (sampleCount)
            {
                case VkSampleCountFlags.Count1:
                    return 1;
                case VkSampleCountFlags.Count2:
                    return 2;
                case VkSampleCountFlags.Count4:
                    return 4;
                case VkSampleCountFlags.Count8:
                    return 8;
                case VkSampleCountFlags.Count16:
                    return 16;
                case VkSampleCountFlags.Count32:
                    return 32;
                default:
                    throw Illegal.Value<VkSampleCountFlags>();
            }
        }

        public static bool IsStencilFormat(VkFormat format)
        {
            return format == VkFormat.D24UnormS8Uint || format == VkFormat.D32SfloatS8Uint;
        }

        public static bool IsDepthStencilFormat(VkFormat format)
        {
            return format == VkFormat.D32SfloatS8Uint
                || format == VkFormat.D24UnormS8Uint
                || format == VkFormat.R16Unorm
                || format == VkFormat.R32Sfloat;
        }

        public static bool IsCompressedFormat(VkFormat format)
        {
            return format == VkFormat.Bc1RgbUnormBlock
                || format == VkFormat.Bc1RgbSrgbBlock
                || format == VkFormat.Bc1RgbaUnormBlock
                || format == VkFormat.Bc1RgbaSrgbBlock
                || format == VkFormat.Bc2UnormBlock
                || format == VkFormat.Bc2SrgbBlock
                || format == VkFormat.Bc3UnormBlock
                || format == VkFormat.Bc3SrgbBlock
                || format == VkFormat.Bc4UnormBlock
                || format == VkFormat.Bc4SnormBlock
                || format == VkFormat.Bc5UnormBlock
                || format == VkFormat.Bc5SnormBlock
                || format == VkFormat.Bc6hUfloatBlock
                || format == VkFormat.Bc6hSfloatBlock
                || format == VkFormat.Bc7UnormBlock
                || format == VkFormat.Bc7SrgbBlock
                || format == VkFormat.Etc2R8G8B8UnormBlock
                || format == VkFormat.Etc2R8G8B8A1UnormBlock
                || format == VkFormat.Etc2R8G8B8A8UnormBlock;
        }

        public static uint GetRowPitch(uint width, VkFormat format)
        {
            switch (format)
            {
                case VkFormat.Bc1RgbUnormBlock:
                case VkFormat.Bc1RgbSrgbBlock:
                case VkFormat.Bc1RgbaUnormBlock:
                case VkFormat.Bc1RgbaSrgbBlock:
                case VkFormat.Bc2UnormBlock:
                case VkFormat.Bc2SrgbBlock:
                case VkFormat.Bc3UnormBlock:
                case VkFormat.Bc3SrgbBlock:
                case VkFormat.Bc4UnormBlock:
                case VkFormat.Bc4SnormBlock:
                case VkFormat.Bc5UnormBlock:
                case VkFormat.Bc5SnormBlock:
                case VkFormat.Bc6hSfloatBlock:
                case VkFormat.Bc6hUfloatBlock:
                case VkFormat.Bc7UnormBlock:
                case VkFormat.Bc7SrgbBlock:
                case VkFormat.Etc2R8G8B8UnormBlock:
                case VkFormat.Etc2R8G8B8A1UnormBlock:
                case VkFormat.Etc2R8G8B8A8UnormBlock:
                    var blocksPerRow = (width + 3) / 4;
                    var blockSizeInBytes = GetBlockSizeInBytes(format);
                    return blocksPerRow * blockSizeInBytes;

                default:
                    return width * GetSizeInBytes(format);
            }
        }

        public static uint GetBlockSizeInBytes(VkFormat format)
        {
            switch (format)
            {
                case VkFormat.Bc1RgbUnormBlock:
                case VkFormat.Bc1RgbSrgbBlock:
                case VkFormat.Bc1RgbaUnormBlock:
                case VkFormat.Bc1RgbaSrgbBlock:
                case VkFormat.Bc4UnormBlock:
                case VkFormat.Bc4SnormBlock:
                case VkFormat.Etc2R8G8B8UnormBlock:
                case VkFormat.Etc2R8G8B8A1UnormBlock:
                    return 8;
                case VkFormat.Bc2UnormBlock:
                case VkFormat.Bc2SrgbBlock:
                case VkFormat.Bc3UnormBlock:
                case VkFormat.Bc3SrgbBlock:
                case VkFormat.Bc5UnormBlock:
                case VkFormat.Bc5SnormBlock:
                case VkFormat.Bc6hSfloatBlock:
                case VkFormat.Bc6hUfloatBlock:
                case VkFormat.Bc7UnormBlock:
                case VkFormat.Bc7SrgbBlock:
                case VkFormat.Etc2R8G8B8A8UnormBlock:
                    return 16;
                default:
                    throw Illegal.Value<VkFormat>();
            }
        }
        
        public static void GetBlockDimensions(VkFormat format, out int width, out int height)
        {
            switch (format)
            {
                case VkFormat.Bc1RgbUnormBlock:
                case VkFormat.Bc1RgbSrgbBlock:
                case VkFormat.Bc1RgbaUnormBlock:
                case VkFormat.Bc1RgbaSrgbBlock:
                case VkFormat.Bc2UnormBlock:
                case VkFormat.Bc2SrgbBlock:
                case VkFormat.Bc3UnormBlock:
                case VkFormat.Bc3SrgbBlock:
                case VkFormat.Bc4UnormBlock:
                case VkFormat.Bc4SnormBlock:
                case VkFormat.Bc5UnormBlock:
                case VkFormat.Bc5SnormBlock:
                case VkFormat.Bc6hSfloatBlock:
                case VkFormat.Bc6hUfloatBlock:
                case VkFormat.Bc7UnormBlock:
                case VkFormat.Bc7SrgbBlock:
                    width = 4;
                    height = 4;
                    break;
                default:
                    width = 1;
                    height = 1;
                    break;
            }
        }

        internal static bool IsFormatViewCompatible(VkFormat viewFormat, VkFormat realFormat)
        {
            if (IsCompressedFormat(realFormat))
            {
                return IsSrgbCounterpart(viewFormat, realFormat);
            }
            else
            {
                return GetViewFamilyFormat(viewFormat) == GetViewFamilyFormat(realFormat);
            }
        }

        private static bool IsSrgbCounterpart(VkFormat viewFormat, VkFormat realFormat)
        {
            throw new NotImplementedException();
        }

        public static uint GetNumRows(uint height, VkFormat format)
        {
            switch (format)
            {
                case VkFormat.Bc1RgbUnormBlock:
                case VkFormat.Bc1RgbSrgbBlock:
                case VkFormat.Bc1RgbaUnormBlock:
                case VkFormat.Bc1RgbaSrgbBlock:
                case VkFormat.Bc2UnormBlock:
                case VkFormat.Bc2SrgbBlock:
                case VkFormat.Bc3UnormBlock:
                case VkFormat.Bc3SrgbBlock:
                case VkFormat.Bc4UnormBlock:
                case VkFormat.Bc4SnormBlock:
                case VkFormat.Bc5UnormBlock:
                case VkFormat.Bc5SnormBlock:
                case VkFormat.Bc6hSfloatBlock:
                case VkFormat.Bc6hUfloatBlock:
                case VkFormat.Bc7UnormBlock:
                case VkFormat.Bc7SrgbBlock:
                case VkFormat.Etc2R8G8B8UnormBlock:
                case VkFormat.Etc2R8G8B8A1UnormBlock:
                case VkFormat.Etc2R8G8B8A8UnormBlock:
                    return (height + 3) / 4;

                default:
                    return height;
            }
        }

        public static uint GetDepthPitch(uint rowPitch, uint height, VkFormat format)
        {
            return rowPitch * GetNumRows(height, format);
        }

        public static uint GetRegionSize(uint width, uint height, uint depth, VkFormat format)
        {
            uint blockSizeInBytes;
            if (IsCompressedFormat(format))
            {
                Debug.Assert((width % 4 == 0 || width < 4) && (height % 4 == 0 || height < 4));
                blockSizeInBytes = GetBlockSizeInBytes(format);
                width /= 4;
                height /= 4;
            }
            else
            {
                blockSizeInBytes = GetSizeInBytes(format);
            }

            return width * height * depth * blockSizeInBytes;
        }

        public static VkSampleCountFlags GetSampleCount(uint samples)
        {
            switch (samples)
            {
                case 1: return VkSampleCountFlags.Count1;
                case 2: return VkSampleCountFlags.Count2;
                case 4: return VkSampleCountFlags.Count4;
                case 8: return VkSampleCountFlags.Count8;
                case 16: return VkSampleCountFlags.Count16;
                case 32: return VkSampleCountFlags.Count32;
                default: throw new VeldridException("Unsupported multisample count: " + samples);
            }
        }

        public static VkFormat GetViewFamilyFormat(VkFormat format)
        {
            switch (format)
            {
                case VkFormat.R32G32B32A32Sfloat:
                case VkFormat.R32G32B32A32Uint:
                case VkFormat.R32G32B32A32Sint:
                    return VkFormat.R32G32B32A32Sfloat;
                case VkFormat.R16G16B16A16Unorm:
                case VkFormat.R16G16B16A16Snorm:
                case VkFormat.R16G16B16A16Uint:
                case VkFormat.R16G16B16A16Sint:
                case VkFormat.R16G16B16A16Sfloat:
                    return VkFormat.R16G16B16A16Sfloat;
                case VkFormat.R32G32Uint:
                case VkFormat.R32G32Sint:
                case VkFormat.R32G32Sfloat:
                    return VkFormat.R32G32Sfloat;
                case VkFormat.A2R10G10B10UnormPack32:
                case VkFormat.A2R10G10B10UintPack32:
                    return VkFormat.A2R10G10B10UnormPack32;
                case VkFormat.R8G8B8A8Unorm:
                case VkFormat.R8G8B8A8Srgb:
                case VkFormat.R8G8B8A8Snorm:
                case VkFormat.R8G8B8A8Uint:
                case VkFormat.R8G8B8A8Sint:
                    return VkFormat.R8G8B8A8Unorm;
                case VkFormat.R16G16Unorm:
                case VkFormat.R16G16Snorm:
                case VkFormat.R16G16Uint:
                case VkFormat.R16G16Sint:
                case VkFormat.R16G16Sfloat:
                    return VkFormat.R16G16Sfloat;
                case VkFormat.R32Uint:
                case VkFormat.R32Sint:
                case VkFormat.R32Sfloat:
                    return VkFormat.R32Sfloat;
                case VkFormat.R8G8Unorm:
                case VkFormat.R8G8Snorm:
                case VkFormat.R8G8Uint:
                case VkFormat.R8G8Sint:
                    return VkFormat.R8G8Unorm;
                case VkFormat.R16Unorm:
                case VkFormat.R16Snorm:
                case VkFormat.R16Uint:
                case VkFormat.R16Sint:
                case VkFormat.R16Sfloat:
                    return VkFormat.R16Sfloat;
                case VkFormat.R8Unorm:
                case VkFormat.R8Snorm:
                case VkFormat.R8Uint:
                case VkFormat.R8Sint:
                    return VkFormat.R8Unorm;
                case VkFormat.Bc1RgbUnormBlock:
                case VkFormat.Bc1RgbSrgbBlock:
                case VkFormat.Bc1RgbaUnormBlock:
                case VkFormat.Bc1RgbaSrgbBlock:
                    return VkFormat.Bc1RgbaUnormBlock;
                case VkFormat.Bc2UnormBlock:
                case VkFormat.Bc2SrgbBlock:
                    return VkFormat.Bc2UnormBlock;
                case VkFormat.Bc3UnormBlock:
                case VkFormat.Bc3SrgbBlock:
                    return VkFormat.Bc3UnormBlock;
                case VkFormat.Bc4UnormBlock:
                case VkFormat.Bc4SnormBlock:
                    return VkFormat.Bc4UnormBlock;
                case VkFormat.Bc5UnormBlock:
                case VkFormat.Bc5SnormBlock:
                    return VkFormat.Bc5UnormBlock;
                case VkFormat.Bc6hUfloatBlock:
                case VkFormat.Bc6hSfloatBlock:
                    return VkFormat.Bc6hUfloatBlock;
                case VkFormat.B8G8R8A8Unorm:
                case VkFormat.B8G8R8A8Srgb:
                    return VkFormat.B8G8R8A8Unorm;
                case VkFormat.Bc7UnormBlock:
                case VkFormat.Bc7SrgbBlock:
                    return VkFormat.Bc7UnormBlock;
                default:
                    return format;
            }
        }
    }
}
