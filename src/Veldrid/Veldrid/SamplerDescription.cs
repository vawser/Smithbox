using System;
using Vortice.Vulkan;

namespace Veldrid
{
    /// <summary>
    /// Describes a <see cref="Sampler"/>, for creation using a <see cref="ResourceFactory"/>.
    /// </summary>
    public struct SamplerDescription : IEquatable<SamplerDescription>
    {
        /// <summary>
        /// The <see cref="SamplerAddressMode"/> mode to use for the U (or S) coordinate.
        /// </summary>
        public VkSamplerAddressMode AddressModeU;
        /// <summary>
        /// The <see cref="SamplerAddressMode"/> mode to use for the V (or T) coordinate.
        /// </summary>
        public VkSamplerAddressMode AddressModeV;
        /// <summary>
        /// The <see cref="SamplerAddressMode"/> mode to use for the W (or R) coordinate.
        /// </summary>
        public VkSamplerAddressMode AddressModeW;
        /// <summary>
        /// The minification filter
        /// </summary>
        public VkFilter MinFilter;
        /// <summary>
        /// The magnification filter
        /// </summary>
        public VkFilter MagFilter;
        /// <summary>
        /// The mip map filtering mode
        /// </summary>
        public VkSamplerMipmapMode MipmapMode;
        /// <summary>
        /// The reduction mode
        /// </summary>
        public VkSamplerReductionMode ReductionMode;
        /// <summary>
        /// An optional value controlling the kind of comparison to use when sampling. If null, comparison sampling is not used.
        /// </summary>
        public VkCompareOp? ComparisonKind;
        /// <summary>
        /// The maximum anisotropy of the filter, when <see cref="SamplerFilter.Anisotropic"/> is used, or otherwise ignored.
        /// </summary>
        public uint MaximumAnisotropy;
        /// <summary>
        /// The minimum level of detail.
        /// </summary>
        public uint MinimumLod;
        /// <summary>
        /// The maximum level of detail.
        /// </summary>
        public uint MaximumLod;
        /// <summary>
        /// The level of detail bias.
        /// </summary>
        public int LodBias;
        /// <summary>
        /// The constant color that is sampled when <see cref="SamplerAddressMode.Border"/> is used, or otherwise ignored.
        /// </summary>
        public VkBorderColor BorderColor;

        /// <summary>
        /// Constructs a new SamplerDescription.
        /// </summary>
        /// <param name="addressModeU">The <see cref="SamplerAddressMode"/> mode to use for the U (or R) coordinate.</param>
        /// <param name="addressModeV">The <see cref="SamplerAddressMode"/> mode to use for the V (or S) coordinate.</param>
        /// <param name="addressModeW">The <see cref="SamplerAddressMode"/> mode to use for the W (or T) coordinate.</param>
        /// <param name="filter">The filter used when sampling.</param>
        /// <param name="comparisonKind">An optional value controlling the kind of comparison to use when sampling. If null,
        /// comparison sampling is not used.</param>
        /// <param name="maximumAnisotropy">The maximum anisotropy of the filter, when <see cref="SamplerFilter.Anisotropic"/> is
        /// used, or otherwise ignored.</param>
        /// <param name="minimumLod">The minimum level of detail.</param>
        /// <param name="maximumLod">The maximum level of detail.</param>
        /// <param name="lodBias">The level of detail bias.</param>
        /// <param name="borderColor">The constant color that is sampled when <see cref="SamplerAddressMode.Border"/> is used, or
        /// otherwise ignored.</param>
        public SamplerDescription(
            VkSamplerAddressMode addressModeU,
            VkSamplerAddressMode addressModeV,
            VkSamplerAddressMode addressModeW,
            VkFilter minFilter,
            VkFilter magFilter,
            VkSamplerMipmapMode mipFilter, 
            VkCompareOp? comparisonKind,
            uint maximumAnisotropy,
            uint minimumLod,
            uint maximumLod,
            int lodBias,
            VkBorderColor borderColor)
        {
            AddressModeU = addressModeU;
            AddressModeV = addressModeV;
            AddressModeW = addressModeW;
            MinFilter = minFilter;
            MagFilter = magFilter;
            MipmapMode = mipFilter;
            ReductionMode = VkSamplerReductionMode.WeightedAverage;
            ComparisonKind = comparisonKind;
            MaximumAnisotropy = maximumAnisotropy;
            MinimumLod = minimumLod;
            MaximumLod = maximumLod;
            LodBias = lodBias;
            BorderColor = borderColor;
        }

        /// <summary>
        /// Describes a common point-filter sampler, with wrapping address mode.
        /// Settings:
        ///     AddressModeU = SamplerAddressMode.Wrap
        ///     AddressModeV = SamplerAddressMode.Wrap
        ///     AddressModeW = SamplerAddressMode.Wrap
        ///     Filter = SamplerFilter.MinPoint_MagPoint_MipPoint
        ///     LodBias = 0
        ///     MinimumLod = 0
        ///     MaximumLod = uint.MaxValue
        ///     MaximumAnisotropy = 0
        /// </summary>
        public static readonly SamplerDescription Point = new SamplerDescription
        {
            AddressModeU = VkSamplerAddressMode.Repeat,
            AddressModeV = VkSamplerAddressMode.Repeat,
            AddressModeW = VkSamplerAddressMode.Repeat,
            MinFilter = VkFilter.Nearest,
            MagFilter = VkFilter.Nearest,
            MipmapMode = VkSamplerMipmapMode.Nearest,
            ReductionMode = VkSamplerReductionMode.WeightedAverage,
            LodBias = 0,
            MinimumLod = 0,
            MaximumLod = uint.MaxValue,
            MaximumAnisotropy = 0,
        };

        /// <summary>
        /// Describes a common linear-filter sampler, with wrapping address mode.
        /// Settings:
        ///     AddressModeU = SamplerAddressMode.Wrap
        ///     AddressModeV = SamplerAddressMode.Wrap
        ///     AddressModeW = SamplerAddressMode.Wrap
        ///     Filter = SamplerFilter.MinLinear_MagLinear_MipLinear
        ///     LodBias = 0
        ///     MinimumLod = 0
        ///     MaximumLod = uint.MaxValue
        ///     MaximumAnisotropy = 0
        /// </summary>
        public static readonly SamplerDescription Linear = new SamplerDescription
        {
            AddressModeU = VkSamplerAddressMode.Repeat,
            AddressModeV = VkSamplerAddressMode.Repeat,
            AddressModeW = VkSamplerAddressMode.Repeat,
            MinFilter = VkFilter.Linear,
            MagFilter = VkFilter.Linear,
            MipmapMode = VkSamplerMipmapMode.Linear,
            ReductionMode = VkSamplerReductionMode.WeightedAverage,
            LodBias = 0,
            MinimumLod = 0,
            MaximumLod = uint.MaxValue,
            MaximumAnisotropy = 0,
        };

        /// <summary>
        /// Describes a common 4x-anisotropic-filter sampler, with wrapping address mode.
        /// Settings:
        ///     AddressModeU = SamplerAddressMode.Wrap
        ///     AddressModeV = SamplerAddressMode.Wrap
        ///     AddressModeW = SamplerAddressMode.Wrap
        ///     Filter = SamplerFilter.Anisotropic
        ///     LodBias = 0
        ///     MinimumLod = 0
        ///     MaximumLod = uint.MaxValue
        ///     MaximumAnisotropy = 4
        /// </summary>
        public static readonly SamplerDescription Aniso4x = new SamplerDescription
        {
            AddressModeU = VkSamplerAddressMode.Repeat,
            AddressModeV = VkSamplerAddressMode.Repeat,
            AddressModeW = VkSamplerAddressMode.Repeat,
            MinFilter = VkFilter.Linear,
            MagFilter = VkFilter.Linear,
            MipmapMode = VkSamplerMipmapMode.Linear,
            ReductionMode = VkSamplerReductionMode.WeightedAverage,
            LodBias = 0,
            MinimumLod = 0,
            MaximumLod = uint.MaxValue,
            MaximumAnisotropy = 4,
        };

        /// <summary>
        /// Element-wise equality.
        /// </summary>
        /// <param name="other">The instance to compare to.</param>
        /// <returns>True if all elements are equal; false otherswise.</returns>
        public bool Equals(SamplerDescription other)
        {
            return AddressModeU == other.AddressModeU
                && AddressModeV == other.AddressModeV
                && AddressModeW == other.AddressModeW
                && MinFilter == other.MinFilter
                && MagFilter == other.MagFilter
                && MipmapMode == other.MipmapMode
                && ReductionMode == other.ReductionMode
                && ComparisonKind.GetValueOrDefault() == other.ComparisonKind.GetValueOrDefault()
                && MaximumAnisotropy == other.MaximumAnisotropy
                && MinimumLod == other.MinimumLod
                && MaximumLod == other.MaximumLod
                && LodBias == other.LodBias
                && BorderColor == other.BorderColor;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return HashHelper.Combine(
                (int)AddressModeU,
                (int)AddressModeV,
                (int)AddressModeW,
                (int)MinFilter,
                (int)MagFilter,
                (int)MipmapMode,
                (int)ReductionMode,
                ComparisonKind.GetHashCode(),
                MaximumAnisotropy.GetHashCode(),
                MinimumLod.GetHashCode(),
                MaximumLod.GetHashCode(),
                LodBias.GetHashCode(),
                (int)BorderColor);
        }
    }
}
