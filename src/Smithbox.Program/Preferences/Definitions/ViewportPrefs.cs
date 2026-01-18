using Hexa.NET.ImGui;
using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Preferences;

public class ViewportPrefs
{
    public static Type GetPrefType()
    {
        return typeof(ViewportPrefs);
    }

    #region General
    public static PreferenceItem System_Frame_Rate()
    {
        return new PreferenceItem
        {
            Category = PreferenceCategory.Viewport,
            Spacer = true,
            InlineName = false,

            Section = "General",

            Title = "Frame Rate",
            Description = "Adjusts the frame rate of the viewport.",

            Draw = () => {
                if (ImGui.SliderFloat("##inputValue", ref CFG.Current.System_Frame_Rate, 20.0f, 240.0f))
                {
                    CFG.Current.System_Frame_Rate = (float)Math.Round(CFG.Current.System_Frame_Rate);
                }
            }
        };
    }


    #endregion

    #region Rendering
    public static PreferenceItem Viewport_Enable_Rendering()
    {
        return new PreferenceItem
        {
            Category = PreferenceCategory.Viewport,
            Spacer = true,

            Section = "Rendering",

            Title = "Enable Model Rendering",
            Description = "If enabled, model rendering will occur within the viewport.",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.Viewport_Enable_Rendering);
            }
        };
    }

    public static PreferenceItem Viewport_Enable_Texturing()
    {
        return new PreferenceItem
        {
            Category = PreferenceCategory.Viewport,
            Spacer = true,

            Section = "Rendering",

            Title = "Enable Texture Rendering",
            Description = "If enabled, texture rendering will occur within the viewport.",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.Viewport_Enable_Texturing);
            }
        };
    }

    public static PreferenceItem Viewport_Enable_Culling()
    {
        return new PreferenceItem
        {
            Category = PreferenceCategory.Viewport,
            Spacer = true,

            Section = "Rendering",

            Title = "Enable Frustum Culling",
            Description = "If enabled, entities outside of the camera frustum to be culled within the viewport.",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.Viewport_Enable_Culling);
            }
        };
    }

    #endregion

    #region Model Rendering

    #endregion

    #region Coloring

    #endregion

    #region Display Preset

    #endregion
}
