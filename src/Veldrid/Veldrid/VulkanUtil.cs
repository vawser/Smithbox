using System;
using System.Diagnostics;
using Vortice.Vulkan;
using static Vortice.Vulkan.Vulkan;

namespace Veldrid
{
    internal unsafe static class VulkanUtil
    {
        private static Lazy<bool> s_isVulkanLoaded = new Lazy<bool>(TryLoadVulkan);
        private static readonly Lazy<string[]> s_instanceExtensions = new Lazy<string[]>(EnumerateInstanceExtensions);

        [Conditional("DEBUG")]
        public static void CheckResult(VkResult result)
        {
            if (result != VkResult.Success)
            {
                throw new VeldridException("Unsuccessful VkResult: " + result);
            }
        }

        public static uint FindMemoryType(VkPhysicalDeviceMemoryProperties memProperties, uint typeFilter, VkMemoryPropertyFlags properties)
        {
            for (int i = 0; i < memProperties.memoryTypeCount; i++)
            {
                if (((typeFilter & (1 << i)) != 0)
                    && (memProperties.GetMemoryType((uint)i).propertyFlags & properties) == properties)
                {
                    return (uint)i;
                }
            }

            throw new VeldridException("No suitable memory type.");
        }

        public static string[] EnumerateInstanceLayers()
        {
            uint propCount = 0;
            VkResult result = vkEnumerateInstanceLayerProperties(&propCount, null);
            CheckResult(result);
            if (propCount == 0)
            {
                return Array.Empty<string>();
            }

            VkLayerProperties[] props = new VkLayerProperties[propCount];
            fixed (VkLayerProperties* ptr = props)
                vkEnumerateInstanceLayerProperties(&propCount, ptr);

            string[] ret = new string[propCount];
            for (int i = 0; i < propCount; i++)
            {
                fixed (sbyte* layerNamePtr = props[i].layerName)
                {
                    ret[i] = Util.GetString(layerNamePtr);
                }
            }

            return ret;
        }

        public static string[] GetInstanceExtensions() => s_instanceExtensions.Value;

        private static string[] EnumerateInstanceExtensions()
        {
            if (!IsVulkanLoaded())
            {
                return Array.Empty<string>();
            }

            uint propCount = 0;
            VkResult result = vkEnumerateInstanceExtensionProperties(null, &propCount, null);
            if (result != VkResult.Success)
            {
                return Array.Empty<string>();
            }

            if (propCount == 0)
            {
                return Array.Empty<string>();
            }

            VkExtensionProperties[] props = new VkExtensionProperties[propCount];

            fixed (VkExtensionProperties* ptr = props)
                vkEnumerateInstanceExtensionProperties(null, &propCount, ptr);

            string[] ret = new string[propCount];
            for (int i = 0; i < propCount; i++)
            {
                fixed (sbyte* extensionNamePtr = props[i].extensionName)
                {
                    ret[i] = Util.GetString(extensionNamePtr);
                }
            }

            return ret;
        }

        public static bool IsVulkanLoaded() => s_isVulkanLoaded.Value;
        private static bool TryLoadVulkan()
        {
            try
            {
                uint propCount;
                vkEnumerateInstanceExtensionProperties(null, &propCount, null);
                return true;
            }
            catch { return false; }
        }

        public static void TransitionImageLayout(
            VkCommandBuffer cb,
            VkImage image,
            uint baseMipLevel,
            uint levelCount,
            uint baseArrayLayer,
            uint layerCount,
            VkImageAspectFlags aspectMask,
            VkImageLayout oldLayout,
            VkImageLayout newLayout)
        {
            Debug.Assert(oldLayout != newLayout);
            var barrier = new VkImageMemoryBarrier2
            {
                oldLayout = oldLayout,
                newLayout = newLayout,
                srcQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED,
                dstQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED,
                image = image
            };
            barrier.subresourceRange.aspectMask = aspectMask;
            barrier.subresourceRange.baseMipLevel = baseMipLevel;
            barrier.subresourceRange.levelCount = levelCount;
            barrier.subresourceRange.baseArrayLayer = baseArrayLayer;
            barrier.subresourceRange.layerCount = layerCount;
            
            if ((oldLayout == VkImageLayout.Undefined || oldLayout == VkImageLayout.Preinitialized) && newLayout == VkImageLayout.TransferDstOptimal)
            {
                barrier.srcAccessMask = VkAccessFlags2.None;
                barrier.dstAccessMask = VkAccessFlags2.TransferWrite;
                barrier.srcStageMask = VkPipelineStageFlags2.None;
                barrier.dstStageMask = VkPipelineStageFlags2.Transfer;
            }
            else if (oldLayout == VkImageLayout.ShaderReadOnlyOptimal && newLayout == VkImageLayout.TransferSrcOptimal)
            {
                barrier.srcAccessMask = VkAccessFlags2.ShaderRead;
                barrier.dstAccessMask = VkAccessFlags2.TransferRead;
                barrier.srcStageMask = VkPipelineStageFlags2.FragmentShader;
                barrier.dstStageMask = VkPipelineStageFlags2.Transfer;
            }
            else if (oldLayout == VkImageLayout.ShaderReadOnlyOptimal && newLayout == VkImageLayout.TransferDstOptimal)
            {
                barrier.srcAccessMask = VkAccessFlags2.ShaderRead;
                barrier.dstAccessMask = VkAccessFlags2.TransferWrite;
                barrier.srcStageMask = VkPipelineStageFlags2.FragmentShader;
                barrier.dstStageMask = VkPipelineStageFlags2.Transfer;
            }
            else if (oldLayout == VkImageLayout.Preinitialized && newLayout == VkImageLayout.TransferSrcOptimal)
            {
                barrier.srcAccessMask = VkAccessFlags2.None;
                barrier.dstAccessMask = VkAccessFlags2.TransferRead;
                barrier.srcStageMask = VkPipelineStageFlags2.None;
                barrier.dstStageMask = VkPipelineStageFlags2.Transfer;
            }
            else if (oldLayout == VkImageLayout.Preinitialized && newLayout == VkImageLayout.General)
            {
                barrier.srcAccessMask = VkAccessFlags2.None;
                barrier.dstAccessMask = VkAccessFlags2.ShaderRead;
                barrier.srcStageMask = VkPipelineStageFlags2.None;
                barrier.dstStageMask = VkPipelineStageFlags2.ComputeShader;
            }
            else if (oldLayout == VkImageLayout.Preinitialized && newLayout == VkImageLayout.ShaderReadOnlyOptimal)
            {
                barrier.srcAccessMask = VkAccessFlags2.None;
                barrier.dstAccessMask = VkAccessFlags2.ShaderRead;
                barrier.srcStageMask = VkPipelineStageFlags2.None;
                barrier.dstStageMask = VkPipelineStageFlags2.FragmentShader;
            }
            else if (oldLayout == VkImageLayout.General && newLayout == VkImageLayout.ShaderReadOnlyOptimal)
            {
                barrier.srcAccessMask = VkAccessFlags2.TransferRead;
                barrier.dstAccessMask = VkAccessFlags2.ShaderRead;
                barrier.srcStageMask = VkPipelineStageFlags2.Transfer;
                barrier.dstStageMask = VkPipelineStageFlags2.FragmentShader;
            }
            else if (oldLayout == VkImageLayout.ShaderReadOnlyOptimal && newLayout == VkImageLayout.General)
            {
                barrier.srcAccessMask = VkAccessFlags2.ShaderRead;
                barrier.dstAccessMask = VkAccessFlags2.ShaderRead;
                barrier.srcStageMask = VkPipelineStageFlags2.FragmentShader;
                barrier.dstStageMask = VkPipelineStageFlags2.ComputeShader;
            }

            else if (oldLayout == VkImageLayout.TransferSrcOptimal && newLayout == VkImageLayout.ShaderReadOnlyOptimal)
            {
                barrier.srcAccessMask = VkAccessFlags2.TransferRead;
                barrier.dstAccessMask = VkAccessFlags2.ShaderRead;
                barrier.srcStageMask = VkPipelineStageFlags2.Transfer;
                barrier.dstStageMask = VkPipelineStageFlags2.FragmentShader;
            }
            else if (oldLayout == VkImageLayout.TransferDstOptimal && newLayout == VkImageLayout.ShaderReadOnlyOptimal)
            {
                barrier.srcAccessMask = VkAccessFlags2.TransferWrite;
                barrier.dstAccessMask = VkAccessFlags2.ShaderRead;
                barrier.srcStageMask = VkPipelineStageFlags2.Transfer;
                barrier.dstStageMask = VkPipelineStageFlags2.FragmentShader;
            }
            else if (oldLayout == VkImageLayout.TransferSrcOptimal && newLayout == VkImageLayout.TransferDstOptimal)
            {
                barrier.srcAccessMask = VkAccessFlags2.TransferRead;
                barrier.dstAccessMask = VkAccessFlags2.TransferWrite;
                barrier.srcStageMask = VkPipelineStageFlags2.Transfer;
                barrier.dstStageMask = VkPipelineStageFlags2.Transfer;
            }
            else if (oldLayout == VkImageLayout.TransferDstOptimal && newLayout == VkImageLayout.TransferSrcOptimal)
            {
                barrier.srcAccessMask = VkAccessFlags2.TransferWrite;
                barrier.dstAccessMask = VkAccessFlags2.TransferRead;
                barrier.srcStageMask = VkPipelineStageFlags2.Transfer;
                barrier.dstStageMask = VkPipelineStageFlags2.Transfer;
            }
            else if (oldLayout == VkImageLayout.ColorAttachmentOptimal && newLayout == VkImageLayout.TransferSrcOptimal)
            {
                barrier.srcAccessMask = VkAccessFlags2.ColorAttachmentWrite;
                barrier.dstAccessMask = VkAccessFlags2.TransferRead;
                barrier.srcStageMask = VkPipelineStageFlags2.ColorAttachmentOutput;
                barrier.dstStageMask = VkPipelineStageFlags2.Transfer;
            }
            else if (oldLayout == VkImageLayout.ColorAttachmentOptimal && newLayout == VkImageLayout.TransferDstOptimal)
            {
                barrier.srcAccessMask = VkAccessFlags2.ColorAttachmentWrite;
                barrier.dstAccessMask = VkAccessFlags2.TransferWrite;
                barrier.srcStageMask = VkPipelineStageFlags2.ColorAttachmentOutput;
                barrier.dstStageMask = VkPipelineStageFlags2.Transfer;
            }
            else if (oldLayout == VkImageLayout.ColorAttachmentOptimal && newLayout == VkImageLayout.ShaderReadOnlyOptimal)
            {
                barrier.srcAccessMask = VkAccessFlags2.ColorAttachmentWrite;
                barrier.dstAccessMask = VkAccessFlags2.ShaderRead;
                barrier.srcStageMask = VkPipelineStageFlags2.ColorAttachmentOutput;
                barrier.dstStageMask = VkPipelineStageFlags2.FragmentShader;
            }
            else if (oldLayout == VkImageLayout.DepthStencilAttachmentOptimal && newLayout == VkImageLayout.ShaderReadOnlyOptimal)
            {
                barrier.srcAccessMask = VkAccessFlags2.DepthStencilAttachmentWrite;
                barrier.dstAccessMask = VkAccessFlags2.ShaderRead;
                barrier.srcStageMask = VkPipelineStageFlags2.LateFragmentTests;
                barrier.dstStageMask = VkPipelineStageFlags2.FragmentShader;
            }
            else if (oldLayout == VkImageLayout.ColorAttachmentOptimal && newLayout == VkImageLayout.PresentSrcKHR)
            {
                barrier.srcAccessMask = VkAccessFlags2.ColorAttachmentWrite;
                barrier.dstAccessMask = VkAccessFlags2.MemoryRead;
                barrier.srcStageMask = VkPipelineStageFlags2.ColorAttachmentOutput;
                barrier.dstStageMask = VkPipelineStageFlags2.BottomOfPipe;
            }
            else if (oldLayout == VkImageLayout.TransferDstOptimal && newLayout == VkImageLayout.ColorAttachmentOptimal)
            {
                barrier.srcAccessMask = VkAccessFlags2.TransferWrite;
                barrier.dstAccessMask = VkAccessFlags2.ColorAttachmentWrite;
                barrier.srcStageMask = VkPipelineStageFlags2.Transfer;
                barrier.dstStageMask = VkPipelineStageFlags2.ColorAttachmentOutput;
            }
            else if (oldLayout == VkImageLayout.TransferDstOptimal && newLayout == VkImageLayout.DepthStencilAttachmentOptimal)
            {
                barrier.srcAccessMask = VkAccessFlags2.TransferWrite;
                barrier.dstAccessMask = VkAccessFlags2.DepthStencilAttachmentWrite;
                barrier.srcStageMask = VkPipelineStageFlags2.Transfer;
                barrier.dstStageMask = VkPipelineStageFlags2.LateFragmentTests;
            }
            else
            {
                Debug.Fail("Invalid image layout transition.");
            }

            var dependencyInfo = new VkDependencyInfo
            {
                dependencyFlags = VkDependencyFlags.None,
                imageMemoryBarrierCount = 1,
                pImageMemoryBarriers = &barrier
            };
            
            vkCmdPipelineBarrier2(cb, &dependencyInfo);
        }
    }

    internal unsafe static class VkPhysicalDeviceMemoryPropertiesEx
    {
        public static VkMemoryType GetMemoryType(this VkPhysicalDeviceMemoryProperties memoryProperties, uint index)
        {
            return (&memoryProperties.memoryTypes.e0)[index];
        }
    }
}
