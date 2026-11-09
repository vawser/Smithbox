using Hexa.NET.DirectXTex;
using System;

// Checks how the Hexa DirectXTex binding takes a ScratchImage: decodes a 4x4 BC1 DDS to RGBA8.
unsafe
{
    var dds = new byte[4 + 124 + 8];
    void W(int o, uint v) => BitConverter.GetBytes(v).CopyTo(dds, o);
    W(0, 0x20534444);            // "DDS "
    W(4, 124);                   // header size
    W(8, 0x1 | 0x2 | 0x4 | 0x1000 | 0x80000); // caps|height|width|pixelformat|linearsize
    W(12, 4); W(16, 4);          // height, width
    W(20, 8);                    // linear size
    W(4 + 72, 32);               // pixel format size
    W(4 + 76, 0x4);              // DDPF_FOURCC
    W(4 + 80, 0x31545844);       // "DXT1"
    W(4 + 104, 0x1000);          // caps texture
    // One BC1 block: colour0 = red (0xF800), colour1 = red, all indices 0
    dds[128] = 0x00; dds[129] = 0xF8; dds[130] = 0x00; dds[131] = 0xF8;

    ScratchImage img = DirectXTex.CreateScratchImage();
    TexMetadata meta;
    HexaGen.Runtime.HResult hr;
    fixed (byte* p = dds)
        hr = DirectXTex.LoadFromDDSMemory(p, (nuint)dds.Length, DDSFlags.None, &meta, &img);
    Console.WriteLine($"Load: 0x{hr.Value:X8} fmt={meta.Format} {meta.Width}x{meta.Height} handle=0x{img.Handle:X}");

    Image* src = img.GetImage(0, 0, 0);
    Console.WriteLine($"Image: {(nint)src:X} fmt={src->Format} {src->Width}x{src->Height} compressed={DirectXTex.IsCompressed(src->Format)}");

    ScratchImage dst = DirectXTex.CreateScratchImage();
    hr = DirectXTex.Decompress(src, 28, &dst);
    Console.WriteLine($"Decompress: 0x{hr.Value:X8}");
    Image* d = dst.GetImage(0, 0, 0);
    Console.WriteLine($"Out: fmt={d->Format} {d->Width}x{d->Height} pitch={d->RowPitch} first=({d->Pixels[0]},{d->Pixels[1]},{d->Pixels[2]},{d->Pixels[3]})");

    img.Release();
    dst.Release();
}
