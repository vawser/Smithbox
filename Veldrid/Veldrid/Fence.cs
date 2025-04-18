using System;
using Vortice.Vulkan;
using static Vortice.Vulkan.Vulkan;

namespace Veldrid
{
    // A GPU-CPU sync point
    /// <summary>
    /// A synchronization primitive which allows the GPU to communicate when submitted work items have finished executing.
    /// </summary>
    public unsafe class Fence : DeviceResource, IDisposable
    {
        private readonly GraphicsDevice _gd;
        private VkFence _fence;
        private string _name;
        private bool _destroyed;
        
        internal VkFence DeviceFence => _fence;
        
        internal Fence(GraphicsDevice gd, bool signaled)
        {
            _gd = gd;
            var fenceCI = new VkFenceCreateInfo
            {
                flags = signaled ? VkFenceCreateFlags.Signaled : VkFenceCreateFlags.None
            };
            VkResult result = vkCreateFence(_gd.Device, &fenceCI, null, out _fence);
            VulkanUtil.CheckResult(result);
        }
        
        /// <summary>
        /// Gets a value indicating whether the Fence is currently signaled. A Fence is signaled after a CommandList finishes
        /// execution after it was submitted with a Fence instance.
        /// </summary>
        public bool Signaled => vkGetFenceStatus(_gd.Device, _fence) == VkResult.Success;

        /// <summary>
        /// Sets this instance to the unsignaled state.
        /// </summary>
        public void Reset()
        {
            _gd.ResetFence(this);
        }

        /// <summary>
        /// A string identifying this instance. Can be used to differentiate between objects in graphics debuggers and other
        /// tools.
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                _name = value; _gd.SetResourceName(this, value);
            }
        }

        /// <summary>
        /// Frees unmanaged device resources controlled by this instance.
        /// </summary>
        public void Dispose()
        {
            if (!_destroyed)
            {
                vkDestroyFence(_gd.Device, _fence, null);
                _destroyed = true;
            }
        }
    }
}
