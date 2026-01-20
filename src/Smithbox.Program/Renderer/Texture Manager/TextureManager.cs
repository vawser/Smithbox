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
    private readonly GraphicsDevice _gd;
    private readonly IImguiRenderer _imgui;

    private readonly VulkanImGuiRenderer imGuiRenderer;

    public IconManager IconManager;

    public TextureManager(GraphicsDevice gd, IImguiRenderer imgui)
    {
        _gd = gd;
        _imgui = imgui;

        if(!Smithbox.LowRequirementsMode)
        {
            imGuiRenderer = (VulkanImGuiRenderer)_imgui;
        }

        IconManager = new(_gd, _imgui);
    }
}
