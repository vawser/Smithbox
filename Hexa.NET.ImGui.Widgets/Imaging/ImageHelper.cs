namespace Hexa.NET.ImGui.Widgets.Imaging
{
    using System;
    using System.Numerics;

    public enum PixelFormat
    {
        BGRA8UNorm,
        BGR8UNorm,

        RGBA32Float,
        RGBA32UNorm,
        RGBA16Float,
        RGBA16UNorm,
        RGBA8UNorm,

        RGB32Float,
        RGB32UNorm,
        RGB16Float,
        RGB16UNorm,
        RGB8UNorm,

        RG32Float,
        RG32UNorm,
        RG16Float,
        RG16UNorm,
        RG8UNorm,

        R32Float,
        R32UNorm,
        R16Float,
        R16UNorm,
        R8UNorm,
    }

    public unsafe struct HexaImage
    {
        public int Width;
        public int Height;
        public PixelFormat Format;
        public byte* Pixels;

        public HexaImage(int width, int height, PixelFormat format)
        {
            Width = width;
            Height = height;
            Format = format;
            Pixels = AllocT<byte>(width * height * ImageHelper.GetBytesPerPixel(format));
        }

        public void Resize(int width, int height)
        {
            byte* resized = ImageHelper.Resize(Pixels, Width, Height, Format, width, height);
            Free(Pixels);
            Pixels = resized;
            Width = width;
            Height = height;
        }

        public void ConvertFormat(PixelFormat format)
        {
            byte* converted = AllocT<byte>(Width * Height * ImageHelper.GetBytesPerPixel(format));
            ImageHelper.ConvertFormat(Pixels, Format, converted, format, Width, Height);
            Free(Pixels);
            Pixels = converted;
            Format = format;
        }

        public void Dispose()
        {
            if (Pixels != null)
            {
                Free(Pixels);
                this = default;
            }
        }
    }

    public unsafe class ImageHelper
    {
        public static int GetBytesPerPixel(PixelFormat format)
        {
            int bitsPerChannel = GetBitsPerChannel(format);
            int channels = GetChannelCount(format);
            return (bitsPerChannel * channels + 7) / 8;
        }

        public static unsafe void ConvertFormat(byte* src, PixelFormat srcFormat, byte* dst, PixelFormat dstFormat, int width, int height)
        {
            if (srcFormat == dstFormat)
            {
                int bytesPerPixel = GetBytesPerPixel(srcFormat);
                int size = width * height * bytesPerPixel;
                Memcpy(src, dst, size, size);
                return;
            }
            bool hasSrcAlpha = GetChannelCount(srcFormat) == 4;
            bool hasDstAlpha = GetChannelCount(dstFormat) == 4;
            int totalPixels = width * height;

            for (int i = 0; i < totalPixels; i++)
            {
                Vector4 pixelValue = GetPixel(src, srcFormat, i);
                if (hasSrcAlpha && !hasDstAlpha)
                {
                    pixelValue *= pixelValue.W; // premultiply alpha.
                    pixelValue.W = 0;
                }
                else if (!hasSrcAlpha && hasDstAlpha)
                {
                    float sum = pixelValue.X + pixelValue.Y + pixelValue.Z;
                    float a = sum / 3.0f;
                    if (a > 0.0f)
                        pixelValue /= a; // unpremultiply alpha.
                    pixelValue.W = a;
                }
                SetPixel(dst, dstFormat, i, pixelValue);
            }
        }

        public static int GetChannelCount(PixelFormat format)
        {
            return format switch
            {
                PixelFormat.BGRA8UNorm => 4,
                PixelFormat.BGR8UNorm => 3,
                PixelFormat.RGBA32Float => 4,
                PixelFormat.RGBA32UNorm => 4,
                PixelFormat.RGBA16Float => 4,
                PixelFormat.RGBA16UNorm => 4,
                PixelFormat.RGBA8UNorm => 4,
                PixelFormat.RGB32Float => 3,
                PixelFormat.RGB32UNorm => 3,
                PixelFormat.RGB16Float => 3,
                PixelFormat.RGB16UNorm => 3,
                PixelFormat.RGB8UNorm => 3,
                PixelFormat.RG32Float => 2,
                PixelFormat.RG32UNorm => 2,
                PixelFormat.RG16Float => 2,
                PixelFormat.RG16UNorm => 2,
                PixelFormat.RG8UNorm => 2,
                PixelFormat.R32Float => 1,
                PixelFormat.R32UNorm => 1,
                PixelFormat.R16Float => 1,
                PixelFormat.R16UNorm => 1,
                PixelFormat.R8UNorm => 1,
                _ => throw new ArgumentOutOfRangeException(nameof(format)),
            };
        }

        public static int GetBitsPerChannel(PixelFormat format)
        {
            return format switch
            {
                PixelFormat.BGRA8UNorm => 8,
                PixelFormat.BGR8UNorm => 8,
                PixelFormat.RGBA32Float => 32,
                PixelFormat.RGBA32UNorm => 32,
                PixelFormat.RGBA16Float => 16,
                PixelFormat.RGBA16UNorm => 16,
                PixelFormat.RGBA8UNorm => 8,
                PixelFormat.RGB32Float => 32,
                PixelFormat.RGB32UNorm => 32,
                PixelFormat.RGB16Float => 16,
                PixelFormat.RGB16UNorm => 16,
                PixelFormat.RGB8UNorm => 8,
                PixelFormat.RG32Float => 32,
                PixelFormat.RG32UNorm => 32,
                PixelFormat.RG16Float => 16,
                PixelFormat.RG16UNorm => 16,
                PixelFormat.RG8UNorm => 8,
                PixelFormat.R32Float => 32,
                PixelFormat.R32UNorm => 32,
                PixelFormat.R16Float => 16,
                PixelFormat.R16UNorm => 16,
                PixelFormat.R8UNorm => 8,
                _ => throw new ArgumentOutOfRangeException(nameof(format)),
            };
        }

        public static bool IsUNorm(PixelFormat format)
        {
            return format switch
            {
                PixelFormat.BGRA8UNorm => true,
                PixelFormat.BGR8UNorm => true,
                PixelFormat.RGBA32Float => false,
                PixelFormat.RGBA32UNorm => true,
                PixelFormat.RGBA16Float => false,
                PixelFormat.RGBA16UNorm => true,
                PixelFormat.RGBA8UNorm => true,
                PixelFormat.RGB32Float => false,
                PixelFormat.RGB32UNorm => true,
                PixelFormat.RGB16Float => false,
                PixelFormat.RGB16UNorm => true,
                PixelFormat.RGB8UNorm => true,
                PixelFormat.RG32Float => false,
                PixelFormat.RG32UNorm => true,
                PixelFormat.RG16Float => false,
                PixelFormat.RG16UNorm => true,
                PixelFormat.RG8UNorm => true,
                PixelFormat.R32Float => false,
                PixelFormat.R32UNorm => true,
                PixelFormat.R16Float => false,
                PixelFormat.R16UNorm => true,
                PixelFormat.R8UNorm => true,
                _ => throw new ArgumentOutOfRangeException(nameof(format)),
            };
        }

        public static bool IsFloat(PixelFormat format)
        {
            return format switch
            {
                PixelFormat.BGRA8UNorm => false,
                PixelFormat.BGR8UNorm => false,
                PixelFormat.RGBA32Float => true,
                PixelFormat.RGBA32UNorm => false,
                PixelFormat.RGBA16Float => true,
                PixelFormat.RGBA16UNorm => false,
                PixelFormat.RGBA8UNorm => false,
                PixelFormat.RGB32Float => true,
                PixelFormat.RGB32UNorm => false,
                PixelFormat.RGB16Float => true,
                PixelFormat.RGB16UNorm => false,
                PixelFormat.RGB8UNorm => false,
                PixelFormat.RG32Float => true,
                PixelFormat.RG32UNorm => false,
                PixelFormat.RG16Float => true,
                PixelFormat.RG16UNorm => false,
                PixelFormat.RG8UNorm => false,
                PixelFormat.R32Float => true,
                PixelFormat.R32UNorm => false,
                PixelFormat.R16Float => true,
                PixelFormat.R16UNorm => false,
                PixelFormat.R8UNorm => false,
                _ => throw new ArgumentOutOfRangeException(nameof(format)),
            };
        }

        public static Vector4 GetPixel(byte* pixels, PixelFormat format, int index)
        {
            int channels = GetChannelCount(format);
            int bitsPerChannel = GetBitsPerChannel(format);
            int bytesPerChannel = (bitsPerChannel + 7) / 8;
            int pixelIndex = index * channels * bytesPerChannel;

            if (channels > 4)
            {
                throw new Exception("Too many channels");
            }

            Vector4 result = default;

            if (IsFloat(format))
            {
                float* pResult = (float*)&result;

                switch (bitsPerChannel)
                {
                    case 16:
                        Half* pHalf = (Half*)(pixels + pixelIndex * bytesPerChannel);
                        for (int i = 0; i < channels; i++)
                        {
                            pResult[i] = (float)pHalf[i];
                        }
                        break;

                    case 32:
                        for (int i = 0; i < channels; i++)
                        {
                            pResult[i] = *(float*)(pixels + pixelIndex * bytesPerChannel);
                        }
                        break;
                }

                return result;
            }

            if (IsUNorm(format))
            {
                float* pResult = (float*)&result;

                switch (bitsPerChannel)
                {
                    case 8:
                        for (int i = 0; i < channels; i++)
                        {
                            pResult[i] = *(pixels + pixelIndex + i) / (float)byte.MaxValue;
                        }
                        break;

                    case 16:
                        for (int i = 0; i < channels; i++)
                        {
                            pResult[i] = *(ushort*)(pixels + pixelIndex + i * bytesPerChannel) / (float)ushort.MaxValue;
                        }
                        break;

                    case 32:
                        for (int i = 0; i < channels; i++)
                        {
                            pResult[i] = *(uint*)(pixels + pixelIndex + i * bytesPerChannel) / (float)uint.MaxValue;
                        }
                        break;
                }

                if (format == PixelFormat.BGRA8UNorm || format == PixelFormat.BGR8UNorm)
                {
                    return BGRAToRGBA(result);
                }

                return result;
            }

            throw new NotSupportedException();
        }

        public static Vector4 BGRAToRGBA(Vector4 bgra)
        {
            return new Vector4(bgra.Z, bgra.Y, bgra.X, bgra.W);
        }

        public static unsafe void SetPixel(byte* pixels, PixelFormat format, int index, Vector4 value)
        {
            int channels = GetChannelCount(format);
            int bitsPerChannel = GetBitsPerChannel(format);
            int bytesPerChannel = (bitsPerChannel + 7) / 8;
            int pixelIndex = index * channels * bytesPerChannel;

            if (channels > 4)
            {
                throw new Exception("Too many channels");
            }

            if (IsFloat(format))
            {
                float* pValue = (float*)&value;

                switch (bitsPerChannel)
                {
                    case 16:
                        Half* pHalf = (Half*)(pixels + pixelIndex);
                        for (int i = 0; i < channels; i++)
                        {
                            pHalf[i] = (Half)pValue[i];
                        }
                        break;

                    case 32:
                        float* pFloat = (float*)(pixels + pixelIndex);
                        for (int i = 0; i < channels; i++)
                        {
                            pFloat[i] = pValue[i];
                        }
                        break;
                }

                return;
            }

            if (IsUNorm(format))
            {
                float* pValue = (float*)&value;

                switch (bitsPerChannel)
                {
                    case 8:
                        for (int i = 0; i < channels; i++)
                        {
                            *(pixels + pixelIndex + i) = (byte)(Clamp(pValue[i]) * byte.MaxValue);
                        }
                        break;

                    case 16:
                        for (int i = 0; i < channels; i++)
                        {
                            *(ushort*)(pixels + pixelIndex + i * bytesPerChannel) = (ushort)(Clamp(pValue[i]) * 65535.0f);
                        }
                        break;

                    case 32:
                        for (int i = 0; i < channels; i++)
                        {
                            *(uint*)(pixels + pixelIndex + i * bytesPerChannel) = (uint)(Clamp(pValue[i]) * uint.MaxValue);
                        }
                        break;
                }

                return;
            }

            throw new NotSupportedException();
        }

        private static float Clamp(float value)
        {
            if (value < 0.0f) return 0.0f;
            if (value > 1.0f) return 1.0f;
            return value;
        }

        public static byte* Resize(byte* pixels, int srcWidth, int srcHeight, PixelFormat pixelFormat, int dstWidth, int dstHeight)
        {
            int bytesPerPixel = GetBytesPerPixel(pixelFormat); // Calculate bytes per pixel, assume that is padded.
            float xScale = (float)srcWidth / dstWidth;
            float yScale = (float)srcHeight / dstHeight;
            byte* resized = AllocT<byte>(dstWidth * dstHeight * bytesPerPixel);

            for (int y = 0; y < dstHeight; y++)
            {
                for (int x = 0; x < dstWidth; x++)
                {
                    int srcX = (int)(x * xScale);
                    int srcY = (int)(y * yScale);

                    byte* srcPixel = pixels + (srcY * srcWidth + srcX) * bytesPerPixel;
                    byte* dstPixel = resized + (y * dstWidth + x) * bytesPerPixel;

                    for (int b = 0; b < bytesPerPixel; b++)
                    {
                        dstPixel[b] = srcPixel[b];
                    }
                }
            }

            return resized;
        }
    }
}