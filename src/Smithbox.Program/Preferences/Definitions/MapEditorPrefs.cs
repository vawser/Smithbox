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

    #region Map List
    public static PreferenceItem MapEditor_Map_List_Enable_Load_on_Double_Click()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.MapEditor,
            Spacer = true,

            Section = SectionCategory.MapEditor_Map_List,

            Title = "Enable Load on Double-click",
            Description = "If enabled, double-clicking a map in the map list will load it.",

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

            Title = "Display Map Aliases",
            Description = "If enabled, the map aliases for maps will be displayed.",

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

            Title = "Display Character Aliases",
            Description = "If enabled, character aliases will be displayed where possible.",

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

            Title = "Display Asset Aliases",
            Description = "If enabled, asset (object) aliases will be displayed where possible.",

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

            Title = "Display Map Piece Aliases",
            Description = "If enabled, map piece aliases will be displayed where possible.",

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

            Title = "Display Treasure Aliases",
            Description = "If enabled, itemlot references will be displayed as aliases for treasure events.",

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

            Title = "Enable Community Names",
            Description = "If enabled, community names are used for each map object property name.",

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

            Title = "Display Unknown Properties",
            Description = "If enabled, unknown properties will be displayed (i.e. any property starting with Unk).",

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

            Title = "Propagate Name Changes to References",
            Description = "If enabled, renaming a viewport object will propagate the name change to any references it is found in.",

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

            Title = "Display Property Attributes",
            Description = "If enabled, data type attributes are displayed in the tooltip for each property.",

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

            Title = "Display at the Top",
            Description = "If enabled, the additional property information will be displayed at the top of the properties window. By default they will appear at the bottom.",

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

            Title = "Display Map Object Behavior Information",
            Description = "If enabled, information about what the map object does is displayed.",

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

            Title = "Display Map Object References",
            Description = "If enabled, references information for the map object is displayed..",

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

            Title = "Display Reference Name",
            Description = "If enabled, map object references will display the name of the map object.",

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

            Title = "Display Reference Entity ID",
            Description = "If enabled, map object references will display the Entity ID of the map object.",

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

            Title = "Display Reference Alias",
            Description = "If enabled, map object references will display the alias of the map object.",

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

            Title = "Enable Substitution",
            Description = "If enabled, the character marker for 'c0000' map objects will be substituted for the character model assigned here.",

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

            Title = "Character Model ID",
            Description = "The character ID of the model you want to use for the substitution.",

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

            Title = "Display Aliases",
            Description = "If enabled, aliases for each model entry are displayed where possible.",

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

            Title = "Display Tags",
            Description = "If enabled, the tags associated with an model are displayed.",

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

            Title = "Include Low Detail Entries",
            Description = "If enabled, the low detail part entries are displayed.",

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

            Title = "Enable Shortcuts for Groups",
            Description = "If enabled, the shortcuts for Selection Groups are enabled.",

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

            Title = "Frame Selection on Use",
            Description = "If enabled, when a selection group is triggered, the contents is framed in the viewport.",

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

            Title = "Enable Quick Creation",
            Description = "If enabled, the 'Create Selection Group' shortcut will automatically create the group, bypassing the creation prompt.",

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

            Title = "Confirm before Deletion",
            Description = "If enabled, a confirmation dialog occurs before deletion of a group.",

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

            Title = "Display Shortcut",
            Description = "If enabled, the shortcut to select a group is displayed next to its name.",

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

            Title = "Display Tags",
            Description = "If enabled, the tags associated with a group are displayed next to its name.",

            Draw = () =>
            {
                ImGui.Checkbox("##inputValue", ref CFG.Current.MapEditor_Selection_Group_Show_Tags);
            }
        };
    }

    #endregion
}