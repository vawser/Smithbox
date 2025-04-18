using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Vortice.Vulkan;
using static Veldrid.VulkanUtil;
using static Vortice.Vulkan.Vulkan;

namespace Veldrid
{
    internal sealed unsafe class CommandBufferPool : IDisposable
    {
        private readonly GraphicsDevice _device;
        private readonly VkCommandPool _pool;
        private readonly List<VkCommandBuffer> _commandBuffers = new();
        private int _index;

        public CommandBufferPool(GraphicsDevice device, uint queueFamilyIndex)
        {
            _device = device;
            var info = new VkCommandPoolCreateInfo
            {
                flags = VkCommandPoolCreateFlags.Transient,
                queueFamilyIndex = queueFamilyIndex,
            };
            var result = vkCreateCommandPool(_device.Device, &info, null, out _pool);
            CheckResult(result);
        }

        public VkCommandBuffer GetCommandBuffer()
        {
            if (_index < _commandBuffers.Count)
            {
                return _commandBuffers[_index++];
            }

            var info = new VkCommandBufferAllocateInfo
            {
                commandPool = _pool,
                level = VkCommandBufferLevel.Primary,
                commandBufferCount = 1,
            };

            VkCommandBuffer ret;
            vkAllocateCommandBuffer(_device.Device, &info, out ret);
            _commandBuffers.Add(ret);
            _index++;
            return ret;
        }

        public void Reset()
        {
            if (_index > 0)
                vkResetCommandPool(_device.Device, _pool, VkCommandPoolResetFlags.None);
            _index = 0;
        }

        private void ReleaseUnmanagedResources()
        {
            foreach (var b in _commandBuffers)
            {
                vkFreeCommandBuffers(_device.Device, _pool, b);
            }
            vkDestroyCommandPool(_device.Device, _pool);
        }
        
        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        ~CommandBufferPool()
        {
            ReleaseUnmanagedResources();
        }
    }
}