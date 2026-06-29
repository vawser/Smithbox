using Hexa.NET.ImGui;
using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Preferences;

public class MapEditorPrefs
{
    public static Type GetPrefType()
    {
        return typeof(MapEditorPrefs);
    }

    #region General
    public static PreferenceItem MapEditor_SkipHavokLoad()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.MapEditor,
            Spacer = true,
            DisplayRestrictions =
            {
                ProjectType.ER,
                ProjectType.NR
            },

            Section = SectionCategory.MapEditor_General,

            Title = "PREF_MapEditor_SkipHavokLoad",
            Description = "PREF_MapEditor_SkipHavokLoad_TT",

            Draw = () =>
            {
                ImGui.Checkbox("##inputValue", ref CFG.Current.MapEditor_SkipHavokLoad);
            }
        };
    }

    #endregion

    #region Map List
    public static PreferenceItem MapEditor_Map_List_Enable_Load_on_Double_Click()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.MapEditor,
            Spacer = true,

            Section = SectionCategory.MapEditor_Map_List,

            Title = "PREF_MapEditor_Map_List_Enable_Load_on_Double_Click",
            Description = "PREF_MapEditor_Map_List_Enable_Load_on_Double_Click_TT",

            Draw = () =>
            {
                ImGui.Checkbox("##inputValue", ref CFG.Current.MapEditor_Map_List_Enable_Load_on_Double_Click);
            }
        };
    }
    public static PreferenceItem MapEditor_Map_List_Display_Community_Names()
    {
        return new PreferenceItem
        {
            OrderID = 1,
            Category = PreferenceCategory.MapEditor,
            Spacer = true,

            Section = SectionCategory.MapEditor_Map_List,

            Title = "PREF_MapEditor_Map_List_Display_Map_Aliases",
            Description = "PREF_MapEditor_Map_List_Display_Map_Aliases_TT",

            Draw = () =>
            {
                ImGui.Checkbox("##inputValue", ref CFG.Current.MapEditor_Map_List_Display_Map_Aliases);
            }
        };
    }

    #endregion

    #region Map Contents
    public static PreferenceItem MapEditor_Map_Contents_Display_Character_Aliases()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.MapEditor,
            Spacer = true,

            Section = SectionCategory.MapEditor_Map_Contents,

            Title = "PREF_MapEditor_Map_Contents_Display_Character_Aliases",
            Description = "PREF_MapEditor_Map_Contents_Display_Character_Aliases_TT",

            Draw = () =>
            {
                ImGui.Checkbox("##inputValue", ref CFG.Current.MapEditor_Map_Contents_Display_Character_Aliases);
            }
        };
    }
    public static PreferenceItem MapEditor_Map_Contents_Display_Asset_Aliases()
    {
        return new PreferenceItem
        {
            OrderID = 1,
            Category = PreferenceCategory.MapEditor,
            Spacer = true,

            Section = SectionCategory.MapEditor_Map_Contents,

            Title = "PREF_MapEditor_Map_Contents_Display_Asset_Aliases",
            Description = "PREF_MapEditor_Map_Contents_Display_Asset_Aliases_TT",

            Draw = () =>
            {
                ImGui.Checkbox("##inputValue", ref CFG.Current.MapEditor_Map_Contents_Display_Asset_Aliases);
            }
        };
    }
    public static PreferenceItem MapEditor_Map_Contents_Display_Map_Piece_Aliases()
    {
        return new PreferenceItem
        {
            OrderID = 2,
            Category = PreferenceCategory.MapEditor,
            Spacer = true,

            Section = SectionCategory.MapEditor_Map_Contents,

            Title = "PREF_MapEditor_Map_Contents_Display_Map_Piece_Aliases",
            Description = "PREF_MapEditor_Map_Contents_Display_Map_Piece_Aliases_TT",

            Draw = () =>
            {
                ImGui.Checkbox("##inputValue", ref CFG.Current.MapEditor_Map_Contents_Display_Map_Piece_Aliases);
            }
        };
    }
    public static PreferenceItem MapEditor_Map_Contents_Display_Treasure_Aliases()
    {
        return new PreferenceItem
        {
            OrderID = 3,
            Category = PreferenceCategory.MapEditor,
            Spacer = true,

            Section = SectionCategory.MapEditor_Map_Contents,

            Title = "PREF_MapEditor_Map_Contents_Display_Treasure_Aliases",
            Description = "PREF_MapEditor_Map_Contents_Display_Treasure_Aliases_TT",

            Draw = () =>
            {
                ImGui.Checkbox("##inputValue", ref CFG.Current.MapEditor_Map_Contents_Display_Treasure_Aliases);
            }
        };
    }

    #endregion

    #region Map Object Properties
    public static PreferenceItem MapEditor_Properties_Enable_Commmunity_Names()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.MapEditor,
            Spacer = true,

            Section = SectionCategory.MapEditor_Map_Object_Properties,

            Title = "PREF_MapEditor_Properties_Enable_Commmunity_Names",
            Description = "PREF_MapEditor_Properties_Enable_Commmunity_Names_TT",

            Draw = () =>
            {
                ImGui.Checkbox("##inputValue", ref CFG.Current.MapEditor_Properties_Enable_Commmunity_Names);
            }
        };
    }

    public static PreferenceItem MapEditor_Properties_Display_Unknown_Properties()
    {
        return new PreferenceItem
        {
            OrderID = 1,
            Category = PreferenceCategory.MapEditor,
            Spacer = true,

            Section = SectionCategory.MapEditor_Map_Object_Properties,

            Title = "PREF_MapEditor_Properties_Display_Unknown_Properties",
            Description = "PREF_MapEditor_Properties_Display_Unknown_Properties_TT",

            Draw = () =>
            {
                ImGui.Checkbox("##inputValue", ref CFG.Current.MapEditor_Properties_Display_Unknown_Properties);
            }
        };
    }

    public static PreferenceItem MapEditor_Properties_Enable_Referenced_Rename()
    {
        return new PreferenceItem
        {
            OrderID = 2,
            Category = PreferenceCategory.MapEditor,
            Spacer = true,

            Section = SectionCategory.MapEditor_Map_Object_Properties,

            Title = "PREF_MapEditor_Properties_Enable_Referenced_Rename",
            Description = "PREF_MapEditor_Properties_Enable_Referenced_Rename_TT",

            Draw = () =>
            {
                ImGui.Checkbox("##inputValue", ref CFG.Current.MapEditor_Properties_Enable_Referenced_Rename);
            }
        };
    }
    #endregion


    public static PreferenceItem MapEditor_Properties_Display_Property_Info()
    {
        return new PreferenceItem
        {
            OrderID = 3,
            Category = PreferenceCategory.MapEditor,
            Spacer = true,

            Section = SectionCategory.MapEditor_Map_Object_Properties,

            Title = "PREF_MapEditor_Properties_Display_Property_Attributes",
            Description = "PREF_MapEditor_Properties_Display_Property_Attributes_TT",

            Draw = () =>
            {
                ImGui.Checkbox("##inputValue", ref CFG.Current.MapEditor_Properties_Display_Property_Attributes);
            }
        };
    }

    #region Additional Property Information
    public static PreferenceItem MapEditor_Properties_Display_Additional_Information_at_Top()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.MapEditor,
            Spacer = true,

            Section = SectionCategory.MapEditor_Additional_Property_Information,

            Title = "PREF_MapEditor_Properties_Display_Additional_Information_at_Top",
            Description = "PREF_MapEditor_Properties_Display_Additional_Information_at_Top_TT",

            Draw = () =>
            {
                ImGui.Checkbox("##inputValue", ref CFG.Current.MapEditor_Properties_Display_Additional_Information_at_Top);
            }
        };
    }

    public static PreferenceItem MapEditor_Properties_Display_Class_Information()
    {
        return new PreferenceItem
        {
            OrderID = 1,
            Category = PreferenceCategory.MapEditor,
            Spacer = true,

            Section = SectionCategory.MapEditor_Additional_Property_Information,

            Title = "PREF_MapEditor_Properties_Display_Behavior_Information",
            Description = "PREF_MapEditor_Properties_Display_Behavior_Information_TT",

            Draw = () =>
            {
                ImGui.Checkbox("##inputValue", ref CFG.Current.MapEditor_Properties_Display_Behavior_Information);
            }
        };
    }

    public static PreferenceItem MapEditor_Properties_Display_Reference_Information()
    {
        return new PreferenceItem
        {
            OrderID = 2,
            Category = PreferenceCategory.MapEditor,
            Spacer = true,

            Section = SectionCategory.MapEditor_Additional_Property_Information,

            Title = "PREF_MapEditor_Properties_Display_Reference_Information",
            Description = "PREF_MapEditor_Properties_Display_Reference_Information_TT",

            Draw = () =>
            {
                ImGui.Checkbox("##inputValue", ref CFG.Current.MapEditor_Properties_Display_Reference_Information);
            }
        };
    }

    public static PreferenceItem MapEditor_Properties_Display_Reference_Name()
    {
        return new PreferenceItem
        {
            OrderID = 3,
            Category = PreferenceCategory.MapEditor,
            Spacer = true,

            Section = SectionCategory.MapEditor_Additional_Property_Information,

            Title = "PREF_MapEditor_Properties_Display_Reference_Name",
            Description = "PREF_MapEditor_Properties_Display_Reference_Name_TT",

            Draw = () =>
            {
                ImGui.Checkbox("##inputValue", ref CFG.Current.MapEditor_Properties_Display_Reference_Name);
            }
        };
    }
    public static PreferenceItem MapEditor_Properties_Display_Reference_Entity_ID()
    {
        return new PreferenceItem
        {
            OrderID = 4,
            Category = PreferenceCategory.MapEditor,
            Spacer = true,

            Section = SectionCategory.MapEditor_Additional_Property_Information,

            Title = "PREF_MapEditor_Properties_Display_Reference_Entity_ID",
            Description = "PREF_MapEditor_Properties_Display_Reference_Entity_ID_TT",

            Draw = () =>
            {
                ImGui.Checkbox("##inputValue", ref CFG.Current.MapEditor_Properties_Display_Reference_Entity_ID);
            }
        };
    }
    public static PreferenceItem MapEditor_Properties_Display_Reference_Alias()
    {
        return new PreferenceItem
        {
            OrderID = 5,
            Category = PreferenceCategory.MapEditor,
            Spacer = true,

            Section = SectionCategory.MapEditor_Additional_Property_Information,

            Title = "PREF_MapEditor_Properties_Display_Reference_Alias",
            Description = "PREF_MapEditor_Properties_Display_Reference_Alias_TT",

            Draw = () =>
            {
                ImGui.Checkbox("##inputValue", ref CFG.Current.MapEditor_Properties_Display_Reference_Alias);
            }
        };
    }

    #endregion

    #region Character Substitution
    public static PreferenceItem MapEditor_Enable_Character_Substitution()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.MapEditor,
            Spacer = true,

            Section = SectionCategory.MapEditor_Character_Substitution,

            Title = "PREF_MapEditor_Enable_Character_Substitution",
            Description = "PREF_MapEditor_Enable_Character_Substitution_TT",

            Draw = () =>
            {
                ImGui.Checkbox("##inputValue", ref CFG.Current.MapEditor_Enable_Character_Substitution);
            }
        };
    }
    public static PreferenceItem MapEditor_Character_Substitution_ID()
    {
        return new PreferenceItem
        {
            OrderID = 1,
            Category = PreferenceCategory.MapEditor,
            Spacer = true,
            InlineName = false,

            Section = SectionCategory.MapEditor_Character_Substitution,

            Title = "PREF_MapEditor_Character_Substitution_ID",
            Description = "PREF_MapEditor_Character_Substitution_ID_TT",

            Draw = () =>
            {
                DPI.ApplyInputWidth();
                ImGui.InputText("##inputeValue", ref CFG.Current.MapEditor_Character_Substitution_ID, 255);
            }
        };
    }

    #endregion

    #region Model Selector
    public static PreferenceItem MapEditor_Model_Selector_Display_Aliases()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.MapEditor,
            Spacer = true,

            Section = SectionCategory.MapEditor_Model_Selector,

            Title = "PREF_MapEditor_Model_Selector_Display_Aliases",
            Description = "PREF_MapEditor_Model_Selector_Display_Aliases_TT",

            Draw = () =>
            {
                ImGui.Checkbox("##inputValue", ref CFG.Current.MapEditor_Model_Selector_Display_Aliases);
            }
        };
    }
    public static PreferenceItem MapEditor_Model_Selector_Display_Tags()
    {
        return new PreferenceItem
        {
            OrderID = 1,
            Category = PreferenceCategory.MapEditor,
            Spacer = true,

            Section = SectionCategory.MapEditor_Model_Selector,

            Title = "PREF_MapEditor_Model_Selector_Display_Tags",
            Description = "PREF_MapEditor_Model_Selector_Display_Tags_TT",

            Draw = () =>
            {
                ImGui.Checkbox("##inputValue", ref CFG.Current.MapEditor_Model_Selector_Display_Tags);
            }
        };
    }
    public static PreferenceItem MapEditor_Model_Selector_Display_Low_Detail_Entries()
    {
        return new PreferenceItem
        {
            OrderID = 2,
            Category = PreferenceCategory.MapEditor,
            Spacer = true,

            Section = SectionCategory.MapEditor_Model_Selector,

            Title = "PREF_MapEditor_Model_Selector_Display_Low_Detail_Entries",
            Description = "PREF_MapEditor_Model_Selector_Display_Low_Detail_Entries_TT",

            Draw = () =>
            {
                ImGui.Checkbox("##inputValue", ref CFG.Current.MapEditor_Model_Selector_Display_Low_Detail_Entries);
            }
        };
    }

    #endregion

    #region Selection Groups
    public static PreferenceItem MapEditor_Selection_Group_Enable_Shortcuts()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.MapEditor,
            Spacer = true,

            Section = SectionCategory.MapEditor_Selection_Groups,

            Title = "PREF_MapEditor_Selection_Group_Enable_Shortcuts",
            Description = "PREF_MapEditor_Selection_Group_Enable_Shortcuts_TT",

            Draw = () =>
            {
                ImGui.Checkbox("##inputValue", ref CFG.Current.MapEditor_Selection_Group_Enable_Shortcuts);
            }
        };
    }
    public static PreferenceItem MapEditor_Selection_Group_Frame_Selection_On_Use()
    {
        return new PreferenceItem
        {
            OrderID = 1,
            Category = PreferenceCategory.MapEditor,
            Spacer = true,

            Section = SectionCategory.MapEditor_Selection_Groups,

            Title = "PREF_MapEditor_Selection_Group_Frame_Selection_On_Use",
            Description = "PREF_MapEditor_Selection_Group_Frame_Selection_On_Use_TT",

            Draw = () =>
            {
                ImGui.Checkbox("##inputValue", ref CFG.Current.MapEditor_Selection_Group_Frame_Selection_On_Use);
            }
        };
    }
    public static PreferenceItem MapEditor_Selection_Group_Enable_Quick_Creation()
    {
        return new PreferenceItem
        {
            OrderID = 2,
            Category = PreferenceCategory.MapEditor,
            Spacer = true,

            Section = SectionCategory.MapEditor_Selection_Groups,

            Title = "PREF_MapEditor_Selection_Group_Enable_Quick_Creation",
            Description = "PREF_MapEditor_Selection_Group_Enable_Quick_Creation_TT",

            Draw = () =>
            {
                ImGui.Checkbox("##inputValue", ref CFG.Current.MapEditor_Selection_Group_Enable_Quick_Creation);
            }
        };
    }
    public static PreferenceItem MapEditor_Selection_Group_Confirm_Delete()
    {
        return new PreferenceItem
        {
            OrderID = 3,
            Category = PreferenceCategory.MapEditor,
            Spacer = true,

            Section = SectionCategory.MapEditor_Selection_Groups,

            Title = "PREF_MapEditor_Selection_Group_Confirm_Delete",
            Description = "PREF_MapEditor_Selection_Group_Confirm_Delete_TT",

            Draw = () =>
            {
                ImGui.Checkbox("##inputValue", ref CFG.Current.MapEditor_Selection_Group_Confirm_Delete);
            }
        };
    }
    public static PreferenceItem MapEditor_Selection_Group_Show_Keybind()
    {
        return new PreferenceItem
        {
            OrderID = 4,
            Category = PreferenceCategory.MapEditor,
            Spacer = true,

            Section = SectionCategory.MapEditor_Selection_Groups,

            Title = "PREF_MapEditor_Selection_Group_Show_Keybind",
            Description = "PREF_MapEditor_Selection_Group_Show_Keybind_TT",

            Draw = () =>
            {
                ImGui.Checkbox("##inputValue", ref CFG.Current.MapEditor_Selection_Group_Show_Keybind);
            }
        };
    }
    public static PreferenceItem MapEditor_Selection_Group_Show_Tags()
    {
        return new PreferenceItem
        {
            OrderID = 5,
            Category = PreferenceCategory.MapEditor,
            Spacer = true,

            Section = SectionCategory.MapEditor_Selection_Groups,

            Title = "PREF_MapEditor_Selection_Group_Show_Tags",
            Description = "PREF_MapEditor_Selection_Group_Show_Tags_TT",

            Draw = () =>
            {
                ImGui.Checkbox("##inputValue", ref CFG.Current.MapEditor_Selection_Group_Show_Tags);
            }
        };
    }

    #endregion
}