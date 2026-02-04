using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Renderer;

public interface ITextureHandle
{
    public uint Width { get; set; }
    public uint Height { get; set; }

    public void Load(TPF tpf, int index);
    public void Dispose();
}
