using Andre.Formats;
using DirectXTexNet;
using Hexa.NET.ImGui;
using HKLib.hk2018.hkHashMapDetail;
using Microsoft.AspNetCore.Components.Forms;
using Octokit;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.ParamEditor;
using StudioCore.Editors.TextureViewer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Veldrid;
using static SoulsFormats.ACB.Asset;
using Texture = Veldrid.Texture;

namespace StudioCore.Renderer;

public class TextureManager
{
    public IconManager IconManager;

    public TextureManager()
    {
        IconManager = new();
    }
}
