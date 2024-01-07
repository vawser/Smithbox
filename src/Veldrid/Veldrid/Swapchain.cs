using System;
using System.Linq;
using Vortice.Vulkan;
using static Vortice.Vulkan.Vulkan;
using static Veldrid.VulkanUtil;

namespace Veldrid
{
    /// <summary>
    /// A device resource providing the ability to present rendered images to a visible surface.
    /// See <see cref="SwapchainDescription"/>.
    /// </summary>
    public unsafe class Swapchain : DeviceResource, IDisposable
    {
        private readonly GraphicsDevice _gd;
        private readonly VkSurfaceKHR _surface;
        private VkSwapchainKHR _deviceSwapchain;
        private readonly VkSwapchainFramebuffer _framebuffer;
        private VkFence _imageAvailableFence;
        private bool _syncToVBlank;
        private readonly SwapchainSource _swapchainSource;
        private readonly bool _colorSrgb;
        private bool? _newSyncToVBlank;
        private uint _currentImageIndex;
        private string _name;
        
        internal VkSwapchainKHR DeviceSwapchain => _deviceSwapchain;
        internal uint ImageIndex => _currentImageIndex;
        internal VkFence ImageAvailableFence => _imageAvailableFence;
        internal VkSurfaceKHR Surface => _surface;
        internal ResourceRefCount RefCount { get; }
        
        /// <summary>
        /// Gets a <see cref="Framebuffer"/> representing the render targets of this instance.
        /// </summary>
        public  Framebuffer Framebuffer => _framebuffer;
        
        /// <summary>
        /// Resizes the renderable Textures managed by this instance to the given dimensions.
        /// </summary>
        /// <param name="width">The new width of the Swapchain.</param>
        /// <param name="height">The new height of the Swapchain.</param>
        public void Resize(uint width, uint height)
        {
            RecreateAndReacquire(width, height);
        }
        
        /// <summary>
        /// Gets or sets whether presentation of this Swapchain will be synchronized to the window system's vertical refresh
        /// rate.
        /// </summary>
        public bool SyncToVerticalBlank
        {
            get => _newSyncToVBlank ?? _syncToVBlank;
            set
            {
                if (_syncToVBlank != value)
                {
                    _newSyncToVBlank = value;
                }
            }
        }
        
        /// <summary>
        /// A string identifying this instance. Can be used to differentiate between objects in graphics debuggers and other
        /// tools.
        /// </summary>
        public string Name { get => _name; set { _name = value; _gd.SetResourceName(this, value); } }
        
        public Swapchain(GraphicsDevice gd, ref SwapchainDescription description) : this(gd, ref description, VkSurfaceKHR.Null) { }

        public Swapchain(GraphicsDevice gd, ref SwapchainDescription description, VkSurfaceKHR existingSurface)
        {
            _gd = gd;
            _syncToVBlank = description.SyncToVerticalBlank;
            _swapchainSource = description.Source;
            _colorSrgb = description.ColorSrgb;

            if (existingSurface == VkSurfaceKHR.Null)
            {
                _surface = VkSurfaceUtil.CreateSurface(gd, gd.Instance, _swapchainSource);
            }
            else
            {
                _surface = existingSurface;
            }

            _framebuffer = new VkSwapchainFramebuffer(gd, this, _surface, description.Width, description.Height, description.DepthFormat);

            CreateSwapchain(description.Width, description.Height);

            var fenceCI = new VkFenceCreateInfo
            {
                flags = VkFenceCreateFlags.None
            };
            vkCreateFence(_gd.Device, &fenceCI, null, out _imageAvailableFence);

            AcquireNextImage(_gd.Device, VkSemaphore.Null, _imageAvailableFence);
            fixed (VkFence* p = &_imageAvailableFence)
            {
                vkWaitForFences(_gd.Device, 1, p, true, ulong.MaxValue);
                vkResetFences(_gd.Device, 1, p);
            }

            RefCount = new ResourceRefCount(DisposeCore);
        }

        public bool AcquireNextImage(VkDevice device, VkSemaphore semaphore, VkFence fence)
        {
            if (_newSyncToVBlank != null)
            {
                _syncToVBlank = _newSyncToVBlank.Value;
                _newSyncToVBlank = null;
                RecreateAndReacquire(_framebuffer.Width, _framebuffer.Height);
                return false;
            }

            VkResult result = vkAcquireNextImageKHR(
                device,
                _deviceSwapchain,
                ulong.MaxValue,
                semaphore,
                fence,
                out _currentImageIndex);
            _framebuffer.SetImageIndex(_currentImageIndex);
            if (result == VkResult.ErrorOutOfDateKHR || result == VkResult.SuboptimalKHR)
            {
                CreateSwapchain(_framebuffer.Width, _framebuffer.Height);
                return false;
            }
            else if (result != VkResult.Success)
            {
                throw new VeldridException("Could not acquire next image from the Vulkan swapchain.");
            }

            return true;
        }

        private void RecreateAndReacquire(uint width, uint height)
        {
            if (CreateSwapchain(width, height))
            {
                if (AcquireNextImage(_gd.Device, VkSemaphore.Null, _imageAvailableFence))
                {
                    fixed (VkFence* p = &_imageAvailableFence)
                    {
                        vkWaitForFences(_gd.Device, 1, p, true, ulong.MaxValue);
                        vkResetFences(_gd.Device, 1, p);
                    }
                }
            }
        }

        private bool CreateSwapchain(uint width, uint height)
        {
            // Obtain the surface capabilities first -- this will indicate whether the surface has been lost.
            VkResult result = vkGetPhysicalDeviceSurfaceCapabilitiesKHR(_gd.PhysicalDevice, _surface, out VkSurfaceCapabilitiesKHR surfaceCapabilities);
            if (result == VkResult.ErrorSurfaceLostKHR)
            {
                throw new VeldridException($"The Swapchain's underlying surface has been lost.");
            }

            if (surfaceCapabilities.minImageExtent.width == 0 && surfaceCapabilities.minImageExtent.height == 0
                && surfaceCapabilities.maxImageExtent.width == 0 && surfaceCapabilities.maxImageExtent.height == 0)
            {
                return false;
            }

            if (_deviceSwapchain != VkSwapchainKHR.Null)
            {
                _gd.WaitForIdle();
            }

            _currentImageIndex = 0;
            uint surfaceFormatCount = 0;
            result = vkGetPhysicalDeviceSurfaceFormatsKHR(_gd.PhysicalDevice, _surface, &surfaceFormatCount, null);
            CheckResult(result);
            VkSurfaceFormatKHR[] formats = new VkSurfaceFormatKHR[surfaceFormatCount];
            fixed (VkSurfaceFormatKHR* pFormats = formats)
                result = vkGetPhysicalDeviceSurfaceFormatsKHR(_gd.PhysicalDevice, _surface, &surfaceFormatCount, pFormats);
            CheckResult(result);

            VkFormat desiredFormat = _colorSrgb
                ? VkFormat.B8G8R8A8Srgb
                : VkFormat.B8G8R8A8Unorm;

            VkSurfaceFormatKHR surfaceFormat = new VkSurfaceFormatKHR();
            if (formats.Length == 1 && formats[0].format == VkFormat.Undefined)
            {
                surfaceFormat = new VkSurfaceFormatKHR { colorSpace = VkColorSpaceKHR.SrgbNonLinear, format = desiredFormat };
            }
            else
            {
                foreach (VkSurfaceFormatKHR format in formats)
                {
                    if (format.colorSpace == VkColorSpaceKHR.SrgbNonLinear && format.format == desiredFormat)
                    {
                        surfaceFormat = format;
                        break;
                    }
                }
                if (surfaceFormat.format == VkFormat.Undefined)
                {
                    if (_colorSrgb && surfaceFormat.format != VkFormat.R8G8B8A8Srgb)
                    {
                        throw new VeldridException($"Unable to create an sRGB Swapchain for this surface.");
                    }

                    surfaceFormat = formats[0];
                }
            }

            uint presentModeCount = 0;
            result = vkGetPhysicalDeviceSurfacePresentModesKHR(_gd.PhysicalDevice, _surface, &presentModeCount, null);
            CheckResult(result);
            VkPresentModeKHR[] presentModes = new VkPresentModeKHR[presentModeCount];
            fixed (VkPresentModeKHR *pPresentModes = presentModes)
                result = vkGetPhysicalDeviceSurfacePresentModesKHR(_gd.PhysicalDevice, _surface, &presentModeCount, pPresentModes);
            CheckResult(result);

            VkPresentModeKHR presentMode = VkPresentModeKHR.Fifo;

            if (_syncToVBlank)
            {
                if (presentModes.Contains(VkPresentModeKHR.FifoRelaxed))
                {
                    presentMode = VkPresentModeKHR.FifoRelaxed;
                }
            }
            else
            {
                if (presentModes.Contains(VkPresentModeKHR.Mailbox))
                {
                    presentMode = VkPresentModeKHR.Mailbox;
                }
                else if (presentModes.Contains(VkPresentModeKHR.Immediate))
                {
                    presentMode = VkPresentModeKHR.Immediate;
                }
            }

            uint maxImageCount = surfaceCapabilities.maxImageCount == 0 ? uint.MaxValue : surfaceCapabilities.maxImageCount;
            uint imageCount = Math.Min(maxImageCount, surfaceCapabilities.minImageCount + 1);

            VkSwapchainCreateInfoKHR swapchainCI = new VkSwapchainCreateInfoKHR();
            swapchainCI.surface = _surface;
            swapchainCI.presentMode = presentMode;
            swapchainCI.imageFormat = surfaceFormat.format;
            swapchainCI.imageColorSpace = surfaceFormat.colorSpace;
            uint clampedWidth = Util.Clamp(width, surfaceCapabilities.minImageExtent.width, surfaceCapabilities.maxImageExtent.width);
            uint clampedHeight = Util.Clamp(height, surfaceCapabilities.minImageExtent.height, surfaceCapabilities.maxImageExtent.height);
            swapchainCI.imageExtent = new VkExtent2D { width = clampedWidth, height = clampedHeight };
            swapchainCI.minImageCount = imageCount;
            swapchainCI.imageArrayLayers = 1;
            swapchainCI.imageUsage = VkImageUsageFlags.ColorAttachment | VkImageUsageFlags.TransferDst;

            swapchainCI.imageSharingMode = VkSharingMode.Exclusive;
            swapchainCI.queueFamilyIndexCount = 0;
            swapchainCI.preTransform = VkSurfaceTransformFlagsKHR.Identity;
            swapchainCI.compositeAlpha = VkCompositeAlphaFlagsKHR.Opaque;
            swapchainCI.clipped = true;

            VkSwapchainKHR oldSwapchain = _deviceSwapchain;
            swapchainCI.oldSwapchain = oldSwapchain;

            result = vkCreateSwapchainKHR(_gd.Device, &swapchainCI, null, out _deviceSwapchain);
            CheckResult(result);
            if (oldSwapchain != VkSwapchainKHR.Null)
            {
                vkDestroySwapchainKHR(_gd.Device, oldSwapchain, null);
            }

            _framebuffer.SetNewSwapchain(_deviceSwapchain, width, height, surfaceFormat, swapchainCI.imageExtent);
            return true;
        }

        /// <summary>
        /// Frees unmanaged device resources controlled by this instance.
        /// </summary>
        public void Dispose()
        {
            RefCount.Decrement();
        }

        private void DisposeCore()
        {
            vkDestroyFence(_gd.Device, _imageAvailableFence, null);
            _framebuffer.Dispose();
            vkDestroySwapchainKHR(_gd.Device, _deviceSwapchain, null);
            vkDestroySurfaceKHR(_gd.Instance, _surface);
        }
    }
}
