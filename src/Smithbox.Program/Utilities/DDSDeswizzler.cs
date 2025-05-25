namespace StudioCore.Utilities;

public class DDSDeswizzler
{
    public readonly int BlockSize;
    public readonly byte Format;
    public int DDSWidth;
    public byte[] InputBytes;
    public byte[] OutputBytes;
    public int WriteOffset;

    public DDSDeswizzler(byte tpfFormat, byte[] input, int blockSize)
    {
        Format = tpfFormat;
        BlockSize = blockSize;
        InputBytes = input;
        OutputBytes = null;
    }

    public void CreateOutput()
    {
        OutputBytes = new byte[InputBytes.Length];
    }

    public void DeswizzleDDSBytesPS3(int width, int height, int offset, int offsetFactor)
    {
        if (width * height > 4)
        {
            DeswizzleDDSBytesPS3(width / 2, height / 2, offset, offsetFactor * 2);
            DeswizzleDDSBytesPS3(width / 2, height / 2, offset + width / 2, offsetFactor * 2);
            DeswizzleDDSBytesPS3(width / 2, height / 2, offset + width / 2 * (height / 2) * offsetFactor,
                offsetFactor * 2);
            DeswizzleDDSBytesPS3(width / 2, height / 2,
                offset + width / 2 * (height / 2) * offsetFactor + width / 2, offsetFactor * 2);
        }
        else
        {
            OutputBytes[offset * 3] = InputBytes[WriteOffset + 3];
            OutputBytes[offset * 3 + 1] = InputBytes[WriteOffset + 2];
            OutputBytes[offset * 3 + 2] = InputBytes[WriteOffset + 1];

            OutputBytes[offset * 3 + 3] = InputBytes[WriteOffset + 7];
            OutputBytes[offset * 3 + 4] = InputBytes[WriteOffset + 6];
            OutputBytes[offset * 3 + 5] = InputBytes[WriteOffset + 5];

            OutputBytes[offset * 3 + DDSWidth * 3] = InputBytes[WriteOffset + 11];
            OutputBytes[offset * 3 + DDSWidth * 3 + 1] = InputBytes[WriteOffset + 10];
            OutputBytes[offset * 3 + DDSWidth * 3 + 2] = InputBytes[WriteOffset + 9];

            OutputBytes[offset * 3 + DDSWidth * 3 + 3] = InputBytes[WriteOffset + 15];
            OutputBytes[offset * 3 + DDSWidth * 3 + 4] = InputBytes[WriteOffset + 14];
            OutputBytes[offset * 3 + DDSWidth * 3 + 5] = InputBytes[WriteOffset + 13];

            WriteOffset += 16;
        }
    }

    public void DeswizzleDDSBytesPS4(int width, int height, int offset, int offsetFactor)
    {
        if (width * height > 16)
        {
            DeswizzleDDSBytesPS4(width / 2, height / 2, offset, offsetFactor * 2);
            DeswizzleDDSBytesPS4(width / 2, height / 2, offset + width / 8 * BlockSize, offsetFactor * 2);
            DeswizzleDDSBytesPS4(width / 2, height / 2, offset + DDSWidth / 8 * (height / 4) * BlockSize,
                offsetFactor * 2);
            DeswizzleDDSBytesPS4(width / 2, height / 2,
                offset + DDSWidth / 8 * (height / 4) * BlockSize + width / 8 * BlockSize, offsetFactor * 2);
        }
        else
            for (var i = 0; i < BlockSize; i++)
            {
                OutputBytes[offset + i] = InputBytes[WriteOffset];
                WriteOffset += 1;
            }
    }

    public void DeswizzleDDSBytesPS4(int width, int height)
    {
        ////[Hork Comment]
        //if (Format != 22 && width <= 4 && height <= 4)
        //{
        //    for (int i = 0; i < BlockSize; i++)
        //    {
        //        OutputBytes[i] = InputBytes[i];
        //    }
        //    return;
        //}

        ////[Hork Comment]
        //if (Format != 22 && width <= 2 && height == 1)
        //{
        //    for (int i = 0; i < width * 8; i++)
        //    {
        //        OutputBytes[i] = InputBytes[i];
        //    }
        //    return;
        //}

        var swizzleBlockSize = 0;
        var blocksH = 0;
        var blocksV = 0;

        if (Format == 22)
        {
            blocksH = (width + 7) / 8;
            blocksV = (height + 7) / 8;
            swizzleBlockSize = 8;
        }
        else if (Format == 105)
        {
            blocksH = (width + 15) / 16;
            blocksV = (height + 15) / 16;
            swizzleBlockSize = 16;
        }
        else
        {
            blocksH = (width + 31) / 32;
            blocksV = (height + 31) / 32;
            swizzleBlockSize = 32;
        }

        if (Format == 22)
        {
            DeswizzleDDSBytesPS4RGBA(width, height, 0, 2);
            WriteOffset = 0;
            return;
        }
        ////[Hork Comment]
        //else if (Format == 105)
        //{
        //    DeswizzleDDSBytesPS4RGBA8(width, height, 0, 2);
        //    WriteOffset = 0;
        //    return;
        //}

        var h = 0;
        var v = 0;
        var offset = 0;

        for (var i = 0; i < blocksV; i++)
        {
            h = 0;
            for (var j = 0; j < blocksH; j++)
            {
                offset = h + v;

                if (Format == 105)
                    DeswizzleDDSBytesPS4RGBA8(16, 16, offset, 2);
                else
                    DeswizzleDDSBytesPS4(32, 32, offset, 2);

                h += swizzleBlockSize / 4 * BlockSize;
                ////[Hork Comment]
                // swizzleBlockSize = 32
            }

            if (Format == 105)
                v += swizzleBlockSize * swizzleBlockSize;
            else
                if (BlockSize == 8)
                    v += swizzleBlockSize * width / 2;
                else
                    v += swizzleBlockSize * width;
        }

        WriteOffset = 0;
    }

    public void DeswizzleDDSBytesSwitch(int width, int height)
    {
        var numStripes = width / 32;
        var numBlocksInStripe = height / 32;
        var ddsBlockSize = 8;
        var stripeWidth = ddsBlockSize * 8;
        var stripeBlockSize = stripeWidth * 8;
        var offset = 0;
        DDSWidth = 256;
        for (var i = 0; i < numStripes; i++)
            for (var j = 0; j < numBlocksInStripe; j++)
            {
                offset = i * stripeWidth + j * numStripes * stripeBlockSize;

                // Left
                DeswizzleDDSBytesPS4(16, 16, offset, 2);
                DeswizzleDDSBytesPS4(16, 16, offset + numStripes * stripeBlockSize / 2, 2);

                // Right
                DeswizzleDDSBytesPS4(16, 16, offset + stripeWidth / 2, 2);
                DeswizzleDDSBytesPS4(16, 16, offset + stripeWidth / 2 + numStripes * stripeBlockSize / 2, 2);
            }
    }

    public void DeswizzleDDSBytesPS4RGBA(int width, int height, int offset, int offsetFactor)
    {
        if (width * height > 4)
        {
            DeswizzleDDSBytesPS4RGBA(width / 2, height / 2, offset, offsetFactor * 2);
            DeswizzleDDSBytesPS4RGBA(width / 2, height / 2, offset + width / 2, offsetFactor * 2);
            DeswizzleDDSBytesPS4RGBA(width / 2, height / 2, offset + width / 2 * (height / 2) * offsetFactor,
                offsetFactor * 2);
            DeswizzleDDSBytesPS4RGBA(width / 2, height / 2,
                offset + width / 2 * (height / 2) * offsetFactor + width / 2, offsetFactor * 2);
        }
        else
        {
            for (var i = 0; i < 16; i++)
                OutputBytes[offset * 8 + i] = InputBytes[WriteOffset + i];

            WriteOffset += 16;

            for (var i = 0; i < 16; i++)
                OutputBytes[offset * 8 + DDSWidth * 8 + i] = InputBytes[WriteOffset + i];

            WriteOffset += 16;
        }
    }

    public void DeswizzleDDSBytesPS4RGBA8(int width, int height, int offset, int offsetFactor)
    {
        if (width * height > 4)
        {
            DeswizzleDDSBytesPS4RGBA8(width / 2, height / 2, offset, offsetFactor * 2);
            DeswizzleDDSBytesPS4RGBA8(width / 2, height / 2, offset + width / 2, offsetFactor * 2);
            DeswizzleDDSBytesPS4RGBA8(width / 2, height / 2, offset + width / 2 * (height / 2) * offsetFactor,
                offsetFactor * 2);
            DeswizzleDDSBytesPS4RGBA8(width / 2, height / 2,
                offset + width / 2 * (height / 2) * offsetFactor + width / 2, offsetFactor * 2);
        }
        else
        {
            for (var i = 0; i < 8; i++)
                OutputBytes[offset * 4 + i] = InputBytes[WriteOffset + i];

            WriteOffset += 8;

            for (var i = 0; i < 8; i++)
                OutputBytes[offset * 4 + DDSWidth * 4 + i] = InputBytes[WriteOffset + i];

            WriteOffset += 8;
        }
    }
}
