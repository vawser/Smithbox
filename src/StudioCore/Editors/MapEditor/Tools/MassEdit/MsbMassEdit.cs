using ImGuiNET;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Tools;

public static class MsbMassEdit
{
    public static void Display()
    {
        var width = ImGui.GetWindowWidth();
        var buttonSize = new Vector2(width, 24);

        ConfigureMapTarget();

        ImGui.Separator();

        ConfigureSelection();

        ImGui.Separator();

        ConfigureEdit();

        ImGui.Separator();

        if(ImGui.Button("Apply", buttonSize))
        {
            ApplyMassEdit();
        }
    }

    public static void ConfigureMapTarget()
    {

    }

    public static void ConfigureSelection()
    {

    }

    public static void ConfigureEdit()
    {

    }

    public static void ApplyMassEdit()
    {

    }
}

public enum MapTargetType
{
    [Display(Name ="Loaded")]
    Loaded,
    [Display(Name = "All")]
    All
}

