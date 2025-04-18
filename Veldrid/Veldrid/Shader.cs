using System;
using Vortice.Vulkan;
using static Vortice.Vulkan.Vulkan;
using static Veldrid.VulkanUtil;

namespace Veldrid
{
    /// <summary>
    /// A device resource encapsulating a single shader module.
    /// See <see cref="ShaderDescription"/>.
    /// </summary>
    public unsafe class Shader : DeviceResource, IDisposable
    {
        private readonly GraphicsDevice _gd;
        private readonly VkShaderModule _shaderModule;
        private bool _disposed;
        private string _name;
        
        internal VkShaderModule ShaderModule => _shaderModule;
        
        internal Shader(GraphicsDevice gd, ref ShaderDescription description)
            : this(description.Stage, description.EntryPoint)
        {
            _gd = gd;
            
            fixed (byte* codePtr = description.ShaderBytes)
            {
                var shaderModuleCI = new VkShaderModuleCreateInfo
                {
                    codeSize = (UIntPtr)description.ShaderBytes.Length,
                    pCode = (uint*)codePtr
                };
                VkResult result = vkCreateShaderModule(gd.Device, &shaderModuleCI, null, out _shaderModule);
                CheckResult(result);
            }
        }
        
        internal Shader(VkShaderStageFlags stage, string entryPoint)
        {
            Stage = stage;
            EntryPoint = entryPoint;
        }

        /// <summary>
        /// The shader stage this instance can be used in.
        /// </summary>
        public VkShaderStageFlags Stage { get; }

        /// <summary>
        /// The name of the entry point function.
        /// </summary>
        public string EntryPoint { get; }

        /// <summary>
        /// A string identifying this instance. Can be used to differentiate between objects in graphics debuggers and other
        /// tools.
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                _gd.SetResourceName(this, value);
            }
        }

        /// <summary>
        /// Frees unmanaged device resources controlled by this instance.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                vkDestroyShaderModule(_gd.Device, ShaderModule, null);
            }
        }
    }
}
