using Hexa.NET.ImGui;
using StudioCore.Application;
using System;

namespace StudioCore.Preferences;

public class TimeActEditorPrefs
{
    public static Type GetPrefType()
    {
        return typeof(TimeActEditorPrefs);
    }

    #region Event Graph
    public static PreferenceItem TimeActEditor_Graph_Frame_Width()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.TimeActEditor,
            Spacer = true,

            Section = SectionCategory.TimeActEditor_Event_Graph,

            Title = "PREF_TimeActEditor_Graph_Frame_Width",
            Description = "PREF_TimeActEditor_Graph_Frame_Width_TT",

            Draw = () => {
                ImGui.DragFloat("##inputValue", ref CFG.Current.TimeActEditor_Graph_Frame_Width, 0.5f, 1f, 100f);
            }
        };
    }
    #endregion

    #region Event Display
    public static PreferenceItem TimeActEditor_Event_Marker_Size()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.TimeActEditor,
            Spacer = true,

            Section = SectionCategory.TimeActEditor_Event_Display,

            Title = "PREF_TimeActEditor_Event_Marker_Size",
            Description = "PREF_TimeActEditor_Event_Marker_Size_TT",

            Draw = () => {
                ImGui.DragFloat("##inputValue", ref CFG.Current.TimeActEditor_Event_Marker_Size, 0.05f, 0.1f, 10f);
            }
        };
    }
    #endregion
}
