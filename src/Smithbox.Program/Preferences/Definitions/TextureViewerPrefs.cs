using Hexa.NET.ImGui;
using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Preferences;

public class TextureViewerPrefs
{
    public static Type GetPrefType()
    {
        return typeof(TextureViewerPrefs);
    }

    #region File List
    public static PreferenceItem TextureViewer_File_List_Display_Character_Aliases()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.TextureViewer,
            Spacer = true,

            Section = SectionCategory.TextureViewer_File_List,

            Title = "Display Character Aliases",
            Description = "If enabled, character aliases are displayed.",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.TextureViewer_File_List_Display_Character_Aliases);
            }
        };
    }
    public static PreferenceItem TextureViewer_File_List_Display_Asset_Aliases()
    {
        return new PreferenceItem
        {
            OrderID = 1,
            Category = PreferenceCategory.TextureViewer,
            Spacer = true,

            Section = SectionCategory.TextureViewer_File_List,

            Title = "Display Asset Aliases",
            Description = "If enabled, asset (object) aliases are displayed.",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.TextureViewer_File_List_Display_Asset_Aliases);
            }
        };
    }
    public static PreferenceItem TextureViewer_File_List_Display_Part_Aliases()
    {
        return new PreferenceItem
        {
            OrderID = 2,
            Category = PreferenceCategory.TextureViewer,
            Spacer = true,

            Section = SectionCategory.TextureViewer_File_List,

            Title = "Display Part Aliases",
            Description = "If enabled, part aliases are displayed.",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.TextureViewer_File_List_Display_Part_Aliases);
            }
        };
    }
    public static PreferenceItem TextureViewer_File_List_Display_Low_Detail_Entries()
    {
        return new PreferenceItem
        {
            OrderID = 3,
            Category = PreferenceCategory.TextureViewer,
            Spacer = true,

            Section = SectionCategory.TextureViewer_File_List,

            Title = "Display Low Detail Entries",
            Description = "If enabled, low detail entries are displayed.",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.TextureViewer_File_List_Display_Low_Detail_Entries);
            }
        };
    }
    #endregion

    #region Texture List
    public static PreferenceItem TextureViewer_File_List_Display_Particle_Aliases()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.TextureViewer,
            Spacer = true,

            Section = SectionCategory.TextureViewer_Texture_List,

            Title = "Display Particle Aliases",
            Description = "If enabled, particle aliases are displayed.",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.TextureViewer_File_List_Display_Particle_Aliases);
            }
        };
    }
    #endregion
}
