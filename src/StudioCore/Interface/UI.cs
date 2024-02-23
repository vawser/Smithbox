using StudioCore.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace StudioCore.Interface
{
    public static class UI
    {
        public static string[] GetThemes()
        {
            string[] Themes = ["Dark", "Blue"];
            return Themes;
        }

        public static void SetTheme(int idx)
        {
            CFG.Current.SelectedTheme = idx;

            // Light
            if (idx == 1)
            {
                SetBlueTheme();
            }
            // Dark: default
            else
            {
                ResetInterface();
            }
        }

        public static void ResetInterface()
        {
            CFG.Current.ImGui_FrameBorderSize = CFG.Default.ImGui_FrameBorderSize;
            CFG.Current.ImGui_TabRounding = CFG.Default.ImGui_TabRounding;
            CFG.Current.ImGui_ScrollbarRounding = CFG.Default.ImGui_ScrollbarRounding;

            // Base
            CFG.Current.ImGui_WindowBg = CFG.Default.ImGui_WindowBg;
            CFG.Current.ImGui_ChildBg = CFG.Default.ImGui_ChildBg;
            CFG.Current.ImGui_PopupBg = CFG.Default.ImGui_PopupBg;
            CFG.Current.ImGui_Border = CFG.Default.ImGui_Border;
            CFG.Current.ImGui_FrameBg = CFG.Default.ImGui_FrameBg;
            CFG.Current.ImGui_FrameBgHovered = CFG.Default.ImGui_FrameBgHovered;
            CFG.Current.ImGui_FrameBgActive = CFG.Default.ImGui_FrameBgActive;
            CFG.Current.ImGui_TitleBg = CFG.Default.ImGui_TitleBg;
            CFG.Current.ImGui_TitleBgActive = CFG.Default.ImGui_TitleBgActive;
            CFG.Current.ImGui_MenuBarBg = CFG.Default.ImGui_MenuBarBg;
            CFG.Current.ImGui_ScrollbarBg = CFG.Default.ImGui_ScrollbarBg;
            CFG.Current.ImGui_ScrollbarGrab = CFG.Default.ImGui_ScrollbarGrab;
            CFG.Current.ImGui_ScrollbarGrabHovered = CFG.Default.ImGui_ScrollbarGrabHovered;
            CFG.Current.ImGui_ScrollbarGrabActive = CFG.Default.ImGui_ScrollbarGrabActive;
            CFG.Current.ImGui_CheckMark = CFG.Default.ImGui_CheckMark;
            CFG.Current.ImGui_SliderGrab = CFG.Default.ImGui_SliderGrab;
            CFG.Current.ImGui_SliderGrabActive = CFG.Default.ImGui_SliderGrabActive;
            CFG.Current.ImGui_Button = CFG.Default.ImGui_Button;
            CFG.Current.ImGui_ButtonHovered = CFG.Default.ImGui_ButtonHovered;
            CFG.Current.ImGui_ButtonActive = CFG.Default.ImGui_ButtonActive;
            CFG.Current.ImGui_Selection = CFG.Default.ImGui_Selection;
            CFG.Current.ImGui_SelectionHovered = CFG.Default.ImGui_SelectionHovered;
            CFG.Current.ImGui_SelectionActive = CFG.Default.ImGui_SelectionActive;
            CFG.Current.ImGui_Tab = CFG.Default.ImGui_Tab;
            CFG.Current.ImGui_TabHovered = CFG.Default.ImGui_TabHovered;
            CFG.Current.ImGui_TabActive = CFG.Default.ImGui_TabActive;
            CFG.Current.ImGui_TabUnfocused = CFG.Default.ImGui_TabUnfocused;
            CFG.Current.ImGui_TabUnfocusedActive = CFG.Default.ImGui_TabUnfocusedActive;

            // Settings
            CFG.Current.Floating_WindowBg_Color = CFG.Default.Floating_WindowBg_Color;
            CFG.Current.Floating_TitleBg_Color = CFG.Default.Floating_TitleBg_Color;
            CFG.Current.Floating_TitleBgActive_Color = CFG.Default.Floating_TitleBgActive_Color;
            CFG.Current.Floating_ChildBg_Color = CFG.Default.Floating_ChildBg_Color;
            CFG.Current.Floating_Header_Color = CFG.Default.Floating_Header_Color;

            // Logger
            CFG.Current.Logger_WindowBg_Color = CFG.Default.Logger_WindowBg_Color;
            CFG.Current.Logger_TitleBg_Color = CFG.Default.Logger_TitleBg_Color;
            CFG.Current.Logger_TitleBgActive_Color = CFG.Default.Logger_TitleBgActive_Color;
            CFG.Current.Logger_ChildBg_Color = CFG.Default.Logger_ChildBg_Color;
            CFG.Current.Logger_Text_Warning_Color = CFG.Default.Logger_Text_Warning_Color;

            // Param Editor
            CFG.Current.ParamEditor_RowField_Color = CFG.Default.ParamEditor_RowField_Color;
            CFG.Current.ParamEditor_Row_Frame_Conflict_Color = CFG.Default.ParamEditor_Row_Frame_Conflict_Color;
            CFG.Current.ParamEditor_Row_Frame_Vanilla_Color = CFG.Default.ParamEditor_Row_Frame_Vanilla_Color;
            CFG.Current.ParamEditor_Row_Text_IsRef_Color = CFG.Default.ParamEditor_Row_Text_IsRef_Color;
            CFG.Current.ParamEditor_Row_Text_Match = CFG.Default.ParamEditor_Row_Text_Match;
            CFG.Current.ParamEditor_Row_Frame_Default_Color = CFG.Default.ParamEditor_Row_Frame_Default_Color;
            CFG.Current.ParamEditor_Row_Text_Default = CFG.Default.ParamEditor_Row_Text_Default;
            CFG.Current.ParamEditor_Row_Frame_AuxVanilla_Color = CFG.Default.ParamEditor_Row_Frame_AuxVanilla_Color;
            CFG.Current.ParamEditor_Row_Frame_DiffCompare_Color = CFG.Default.ParamEditor_Row_Frame_DiffCompare_Color;
            CFG.Current.ParamEditor_Row_Text_VirtualRef_Color = CFG.Default.ParamEditor_Row_Text_VirtualRef_Color;
            CFG.Current.ParamEditor_Row_Text_Ref_Color = CFG.Default.ParamEditor_Row_Text_Ref_Color;

            CFG.Current.ParamEditor_Row_View_AuxConflict_Color = CFG.Default.ParamEditor_Row_View_AuxConflict_Color;
            CFG.Current.ParamEditor_Row_View_AuxAdded_Color = CFG.Default.ParamEditor_Row_View_AuxAdded_Color;
            CFG.Current.ParamEditor_Row_View_PrimaryChanged_Color = CFG.Default.ParamEditor_Row_View_PrimaryChanged_Color;
            CFG.Current.ParamEditor_Row_View_AllVanilla_Color = CFG.Default.ParamEditor_Row_View_AllVanilla_Color;

            CFG.Current.ParamEditor_Row_View_FmgLink_Color = CFG.Default.ParamEditor_Row_View_FmgLink_Color;
        }

        public static void SetLightMode()
        {
            
        }

        public static void SetBlueTheme()
        {
            CFG.Current.ImGui_WindowBg = new Vector4(0.2248783f, 0.2248784f, 0.35813951f, 1f);
            CFG.Current.ImGui_ChildBg = new Vector4(0.21016766f, 0.21016769f, 0.31162792f, 1f);
            CFG.Current.ImGui_PopupBg = new Vector4(0.25882354f, 0.25882354f, 0.42352942f, 1f);
            CFG.Current.ImGui_Border = new Vector4(0f, 0f, 0f, 1f);
            CFG.Current.ImGui_FrameBg = new Vector4(0.097609505f, 0.097609535f, 0.21860462f, 1f);
            CFG.Current.ImGui_FrameBgHovered = new Vector4(0.08307193f, 0.083071984f, 0.27906978f, 1f);
            CFG.Current.ImGui_FrameBgActive = new Vector4(0.1282423f, 0.12824236f, 0.3534884f, 1f);
            CFG.Current.ImGui_TitleBg = new Vector4(0.19837748f, 0.1983775f, 0.32558137f, 1f);
            CFG.Current.ImGui_TitleBgActive = new Vector4(0.2283613f, 0.22836134f, 0.4232558f, 1f);
            CFG.Current.ImGui_MenuBarBg = new Vector4(0.19232015f, 0.19232018f, 0.32558137f, 1f);
            CFG.Current.ImGui_ScrollbarBg = new Vector4(0.09561925f, 0.095619276f, 0.24186045f, 1f);
            CFG.Current.ImGui_ScrollbarGrab = new Vector4(0.21767443f, 0.28524825f, 0.6f, 1f);
            CFG.Current.ImGui_ScrollbarGrabHovered = new Vector4(0.35425746f, 0.34818822f, 0.67441857f, 1f);
            CFG.Current.ImGui_ScrollbarGrabActive = new Vector4(0.31411573f, 0.35037524f, 0.6139535f, 1f);
            CFG.Current.ImGui_CheckMark = new Vector4(0.28225985f, 0.27852896f, 0.47906977f, 1f);
            CFG.Current.ImGui_SliderGrab = new Vector4(0.29062128f, 0.286079f, 0.53023255f, 1f);
            CFG.Current.ImGui_SliderGrabActive = new Vector4(0.31117365f, 0.3136441f, 0.5767442f, 1f);
            CFG.Current.ImGui_Button = new Vector4(0.17663603f, 0.17663607f, 0.33023256f, 1f);
            CFG.Current.ImGui_ButtonHovered = new Vector4(0.2641428f, 0.26414287f, 0.5162791f, 1f);
            CFG.Current.ImGui_ButtonActive = new Vector4(0.18256353f, 0.21504536f, 0.45116282f, 1f);
            CFG.Current.ImGui_Selection = new Vector4(0.15723093f, 0.15918645f, 0.36744183f, 1f);
            CFG.Current.ImGui_SelectionHovered = new Vector4(0.30827475f, 0.30827478f, 0.53023255f, 1f);
            CFG.Current.ImGui_SelectionActive = new Vector4(0.20742022f, 0.34091946f, 0.47441858f, 1f);
            CFG.Current.ImGui_Tab = new Vector4(0.18345052f, 0.18345056f, 0.37209302f, 1f);
            CFG.Current.ImGui_TabHovered = new Vector4(0.20655489f, 0.28231457f, 0.5767442f, 1f);
            CFG.Current.ImGui_TabActive = new Vector4(0.21538128f, 0.32897708f, 0.6093023f, 1f);
            CFG.Current.ImGui_TabUnfocused = new Vector4(0.20417523f, 0.20417525f, 0.3627907f, 1f);
            CFG.Current.ImGui_TabUnfocusedActive = new Vector4(0.19729586f, 0.19729589f, 0.3534884f, 1f);
            CFG.Current.Floating_WindowBg_Color = new Vector4(0.18808003f, 0.18808004f, 0.29302323f, 1f);
            CFG.Current.Floating_TitleBg_Color = new Vector4(0.21176471f, 0.21176471f, 0.30980393f, 1f);
            CFG.Current.Floating_TitleBgActive_Color = new Vector4(0.22745098f, 0.22745098f, 0.42352942f, 1f);
            CFG.Current.Floating_ChildBg_Color = new Vector4(0.21176471f, 0.21176471f, 0.30980393f, 1f);
            CFG.Current.Floating_Header_Color = new Vector4(0.3f, 0.3f, 0.6f, 0.4f);
            CFG.Current.Logger_WindowBg_Color = new Vector4(0.1882353f, 0.1882353f, 0.29411766f, 0.98039216f);
            CFG.Current.Logger_TitleBg_Color = new Vector4(0.21176471f, 0.21176471f, 0.30980393f, 1f);
            CFG.Current.Logger_TitleBgActive_Color = new Vector4(0.22745098f, 0.22745098f, 0.42352942f, 1f);
            CFG.Current.Logger_TitleBgActive_Color = new Vector4(0.22745098f, 0.22745098f, 0.42352942f, 1f);
            CFG.Current.Logger_ChildBg_Color = new Vector4(0.17016765f, 0.17016767f, 0.26511627f, 0.98039216f);
            CFG.Current.Logger_Text_Warning_Color = new Vector4(1f, 0f, 0f, 1f);
            CFG.Current.ParamEditor_RowField_Color = new Vector4(0.8f, 0.8f, 1f, 1f);
            CFG.Current.ParamEditor_Row_Frame_Conflict_Color = new Vector4(0.24186045f, 0.098994054f, 0.098994054f, 1f);
            CFG.Current.ParamEditor_Row_Frame_Vanilla_Color = new Vector4(0.14680369f, 0.16167375f, 0.26976746f, 1f);
            CFG.Current.ParamEditor_Row_Text_IsRef_Color = new Vector4(1f, 0.5f, 1f, 1f);
            CFG.Current.ParamEditor_Row_Text_Match = new Vector4(0.75f, 0.75f, 0.75f, 1f);
            CFG.Current.ParamEditor_Row_Frame_Default_Color = new Vector4(0.09676581f, 0.09676586f, 0.29302323f, 1f);
            CFG.Current.ParamEditor_Row_Text_Default = new Vector4(0.9f, 0.9f, 0.9f, 1f);
            CFG.Current.ParamEditor_Row_Frame_AuxVanilla_Color = new Vector4(0.2f, 0.2f, 0.35f, 1f);
            CFG.Current.ParamEditor_Row_Frame_DiffCompare_Color = new Vector4(0.2f, 0.2f, 0.35f, 1f);
            CFG.Current.ParamEditor_Row_Text_VirtualRef_Color = new Vector4(1f, 0.75f, 1f, 1f);
            CFG.Current.ParamEditor_Row_Text_Ref_Color = new Vector4(1f, 0.75f, 0.75f, 1f);
            CFG.Current.ParamEditor_Row_View_AuxConflict_Color = new Vector4(1f, 0.7f, 0.7f, 1f);
            CFG.Current.ParamEditor_Row_View_AuxAdded_Color = new Vector4(0.7f, 0.7f, 1f, 1f);
            CFG.Current.ParamEditor_Row_View_PrimaryChanged_Color = new Vector4(0.7f, 1f, 0.7f, 1f);
            CFG.Current.ParamEditor_Row_View_AllVanilla_Color = new Vector4(0.9f, 0.9f, 0.9f, 1f);
            CFG.Current.ParamEditor_Row_View_FmgLink_Color = new Vector4(1f, 1f, 0f, 1f);
            CFG.Current.ModelEditor_SceneTree_ChildBg = new Vector4(0.12941177f, 0.12941177f, 0.23529412f, 1f);
            CFG.Current.ModelEditor_Properties_ChildBg = new Vector4(0.12941177f, 0.12941177f, 0.23529412f, 1f);
            CFG.Current.ModelEditor_MultipleEdit_Frame = new Vector4(0f, 0.5f, 0f, 0.1f);
            CFG.Current.MapEditor_SceneTree_ChildBg = new Vector4(0.12941177f, 0.12941177f, 0.23529412f, 1f);
            CFG.Current.MapEditor_Properties_ChildBg = new Vector4(0.12941177f, 0.12941177f, 0.23529412f, 1f);
            CFG.Current.MapEditor_Error_Frame = new Vector4(0.8f, 0.2f, 0.2f, 1f);
            CFG.Current.MapEditor_MultipleEdit_Frame = new Vector4(0f, 0.5f, 0f, 0.1f);
            CFG.Current.MapEditor_ParamRow_Text = new Vector4(0.8f, 0.8f, 0.8f, 1f);
            CFG.Current.DisplayGroupEditor_Border_Highlight = new Vector4(1f, 0.2f, 0.2f, 1f);
            CFG.Current.DisplayGroupEditor_DisplayActive_Frame = new Vector4(0.4f, 0.06f, 0.06f, 1f);
            CFG.Current.DisplayGroupEditor_DisplayActive_Checkbox = new Vector4(1f, 0.2f, 0.2f, 1f);
            CFG.Current.DisplayGroupEditor_DrawActive_Frame = new Vector4(0.02f, 0.3f, 0.02f, 1f);
            CFG.Current.DisplayGroupEditor_DrawActive_Checkbox = new Vector4(0.2f, 1f, 0.2f, 1f);
            CFG.Current.DisplayGroupEditor_CombinedActive_Frame = new Vector4(0.4f, 0.4f, 0.06f, 1f);
            CFG.Current.DisplayGroupEditor_CombinedActive_Checkbox = new Vector4(1f, 1f, 0.02f, 1f);
            CFG.Current.ProgramUpdate_Available_Color = new Vector4(0f, 1f, 0f, 1f);
            CFG.Current.UpgradeParam_Text_Available_Color = new Vector4(1f, 0.3f, 0.3f, 1f);
            CFG.Current.UpgradeParam_Text_OutOfDate_Color = new Vector4(0f, 1f, 0f, 1f);
            CFG.Current.UpgradeParam_Text_Invalid_Color = new Vector4(1f, 0.3f, 0.3f, 1f);
        }

        // Dev function for producing the Theme settings
        public static void CopyUIStatetoClipboard()
        {
            var output = "";
            output = output + $"CFG.Current.ImGui_WindowBg = new Vector4({CFG.Current.ImGui_WindowBg.X}f, {CFG.Current.ImGui_WindowBg.Y}f, {CFG.Current.ImGui_WindowBg.Z}f, {CFG.Current.ImGui_WindowBg.W}f);\n";
            output = output + $"CFG.Current.ImGui_ChildBg = new Vector4({CFG.Current.ImGui_ChildBg.X}f, {CFG.Current.ImGui_ChildBg.Y}f, {CFG.Current.ImGui_ChildBg.Z}f, {CFG.Current.ImGui_ChildBg.W}f);\n";
            output = output + $"CFG.Current.ImGui_PopupBg = new Vector4({CFG.Current.ImGui_PopupBg.X}f, {CFG.Current.ImGui_PopupBg.Y}f, {CFG.Current.ImGui_PopupBg.Z}f, {CFG.Current.ImGui_PopupBg.W}f);\n";
            output = output + $"CFG.Current.ImGui_Border = new Vector4({CFG.Current.ImGui_Border.X}f, {CFG.Current.ImGui_Border.Y}f, {CFG.Current.ImGui_Border.Z}f, {CFG.Current.ImGui_Border.W}f);\n";
            output = output + $"CFG.Current.ImGui_FrameBg = new Vector4({CFG.Current.ImGui_FrameBg.X}f, {CFG.Current.ImGui_FrameBg.Y}f, {CFG.Current.ImGui_FrameBg.Z}f, {CFG.Current.ImGui_FrameBg.W}f);\n";
            output = output + $"CFG.Current.ImGui_FrameBgHovered = new Vector4({CFG.Current.ImGui_FrameBgHovered.X}f, {CFG.Current.ImGui_FrameBgHovered.Y}f, {CFG.Current.ImGui_FrameBgHovered.Z}f, {CFG.Current.ImGui_FrameBgHovered.W}f);\n";
            output = output + $"CFG.Current.ImGui_FrameBgActive = new Vector4({CFG.Current.ImGui_FrameBgActive.X}f, {CFG.Current.ImGui_FrameBgActive.Y}f, {CFG.Current.ImGui_FrameBgActive.Z}f, {CFG.Current.ImGui_FrameBgActive.W}f);\n";
            output = output + $"CFG.Current.ImGui_TitleBg = new Vector4({CFG.Current.ImGui_TitleBg.X}f, {CFG.Current.ImGui_TitleBg.Y}f, {CFG.Current.ImGui_TitleBg.Z}f, {CFG.Current.ImGui_TitleBg.W}f);\n";
            output = output + $"CFG.Current.ImGui_TitleBgActive = new Vector4({CFG.Current.ImGui_TitleBgActive.X}f, {CFG.Current.ImGui_TitleBgActive.Y}f, {CFG.Current.ImGui_TitleBgActive.Z}f, {CFG.Current.ImGui_TitleBgActive.W}f);\n";
            output = output + $"CFG.Current.ImGui_MenuBarBg = new Vector4({CFG.Current.ImGui_MenuBarBg.X}f, {CFG.Current.ImGui_MenuBarBg.Y}f, {CFG.Current.ImGui_MenuBarBg.Z}f, {CFG.Current.ImGui_MenuBarBg.W}f);\n";
            output = output + $"CFG.Current.ImGui_ScrollbarBg = new Vector4({CFG.Current.ImGui_ScrollbarBg.X}f, {CFG.Current.ImGui_ScrollbarBg.Y}f, {CFG.Current.ImGui_ScrollbarBg.Z}f, {CFG.Current.ImGui_ScrollbarBg.W}f);\n";
            output = output + $"CFG.Current.ImGui_ScrollbarGrab = new Vector4({CFG.Current.ImGui_ScrollbarGrab.X}f, {CFG.Current.ImGui_ScrollbarGrab.Y}f, {CFG.Current.ImGui_ScrollbarGrab.Z}f, {CFG.Current.ImGui_ScrollbarGrab.W}f);\n";
            output = output + $"CFG.Current.ImGui_ScrollbarGrabHovered = new Vector4({CFG.Current.ImGui_ScrollbarGrabHovered.X}f, {CFG.Current.ImGui_ScrollbarGrabHovered.Y}f, {CFG.Current.ImGui_ScrollbarGrabHovered.Z}f, {CFG.Current.ImGui_ScrollbarGrabHovered.W}f);\n";
            output = output + $"CFG.Current.ImGui_ScrollbarGrabActive = new Vector4({CFG.Current.ImGui_ScrollbarGrabActive.X}f, {CFG.Current.ImGui_ScrollbarGrabActive.Y}f, {CFG.Current.ImGui_ScrollbarGrabActive.Z}f, {CFG.Current.ImGui_ScrollbarGrabActive.W}f);\n";
            output = output + $"CFG.Current.ImGui_CheckMark = new Vector4({CFG.Current.ImGui_CheckMark.X}f, {CFG.Current.ImGui_CheckMark.Y}f, {CFG.Current.ImGui_CheckMark.Z}f, {CFG.Current.ImGui_CheckMark.W}f);\n";
            output = output + $"CFG.Current.ImGui_SliderGrab = new Vector4({CFG.Current.ImGui_SliderGrab.X}f, {CFG.Current.ImGui_SliderGrab.Y}f, {CFG.Current.ImGui_SliderGrab.Z}f, {CFG.Current.ImGui_SliderGrab.W}f);\n";
            output = output + $"CFG.Current.ImGui_SliderGrabActive = new Vector4({CFG.Current.ImGui_SliderGrabActive.X}f, {CFG.Current.ImGui_SliderGrabActive.Y}f, {CFG.Current.ImGui_SliderGrabActive.Z}f, {CFG.Current.ImGui_SliderGrabActive.W}f);\n";
            output = output + $"CFG.Current.ImGui_Button = new Vector4({CFG.Current.ImGui_Button.X}f, {CFG.Current.ImGui_Button.Y}f, {CFG.Current.ImGui_Button.Z}f, {CFG.Current.ImGui_Button.W}f);\n";
            output = output + $"CFG.Current.ImGui_ButtonHovered = new Vector4({CFG.Current.ImGui_ButtonHovered.X}f, {CFG.Current.ImGui_ButtonHovered.Y}f, {CFG.Current.ImGui_ButtonHovered.Z}f, {CFG.Current.ImGui_ButtonHovered.W}f);\n";
            output = output + $"CFG.Current.ImGui_ButtonActive = new Vector4({CFG.Current.ImGui_ButtonActive.X}f, {CFG.Current.ImGui_ButtonActive.Y}f, {CFG.Current.ImGui_ButtonActive.Z}f, {CFG.Current.ImGui_ButtonActive.W}f);\n";
            output = output + $"CFG.Current.ImGui_Selection = new Vector4({CFG.Current.ImGui_Selection.X}f, {CFG.Current.ImGui_Selection.Y}f, {CFG.Current.ImGui_Selection.Z}f, {CFG.Current.ImGui_Selection.W}f);\n";
            output = output + $"CFG.Current.ImGui_SelectionHovered = new Vector4({CFG.Current.ImGui_SelectionHovered.X}f, {CFG.Current.ImGui_SelectionHovered.Y}f, {CFG.Current.ImGui_SelectionHovered.Z}f, {CFG.Current.ImGui_SelectionHovered.W}f);\n";
            output = output + $"CFG.Current.ImGui_SelectionActive = new Vector4({CFG.Current.ImGui_SelectionActive.X}f, {CFG.Current.ImGui_SelectionActive.Y}f, {CFG.Current.ImGui_SelectionActive.Z}f, {CFG.Current.ImGui_SelectionActive.W}f);\n";
            output = output + $"CFG.Current.ImGui_Tab = new Vector4({CFG.Current.ImGui_Tab.X}f, {CFG.Current.ImGui_Tab.Y}f, {CFG.Current.ImGui_Tab.Z}f, {CFG.Current.ImGui_Tab.W}f);\n";
            output = output + $"CFG.Current.ImGui_TabHovered = new Vector4({CFG.Current.ImGui_TabHovered.X}f, {CFG.Current.ImGui_TabHovered.Y}f, {CFG.Current.ImGui_TabHovered.Z}f, {CFG.Current.ImGui_TabHovered.W}f);\n";
            output = output + $"CFG.Current.ImGui_TabActive = new Vector4({CFG.Current.ImGui_TabActive.X}f, {CFG.Current.ImGui_TabActive.Y}f, {CFG.Current.ImGui_TabActive.Z}f, {CFG.Current.ImGui_TabActive.W}f);\n";
            output = output + $"CFG.Current.ImGui_TabUnfocused = new Vector4({CFG.Current.ImGui_TabUnfocused.X}f, {CFG.Current.ImGui_TabUnfocused.Y}f, {CFG.Current.ImGui_TabUnfocused.Z}f, {CFG.Current.ImGui_TabUnfocused.W}f);\n";
            output = output + $"CFG.Current.ImGui_TabUnfocusedActive = new Vector4({CFG.Current.ImGui_TabUnfocusedActive.X}f, {CFG.Current.ImGui_TabUnfocusedActive.Y}f, {CFG.Current.ImGui_TabUnfocusedActive.Z}f, {CFG.Current.ImGui_TabUnfocusedActive.W}f);\n";
            output = output + $"CFG.Current.Floating_WindowBg_Color = new Vector4({CFG.Current.Floating_WindowBg_Color.X}f, {CFG.Current.Floating_WindowBg_Color.Y}f, {CFG.Current.Floating_WindowBg_Color.Z}f, {CFG.Current.Floating_WindowBg_Color.W}f);\n";
            output = output + $"CFG.Current.Floating_TitleBg_Color = new Vector4({CFG.Current.Floating_TitleBg_Color.X}f, {CFG.Current.Floating_TitleBg_Color.Y}f, {CFG.Current.Floating_TitleBg_Color.Z}f, {CFG.Current.Floating_TitleBg_Color.W}f);\n";
            output = output + $"CFG.Current.Floating_TitleBgActive_Color = new Vector4({CFG.Current.Floating_TitleBgActive_Color.X}f, {CFG.Current.Floating_TitleBgActive_Color.Y}f, {CFG.Current.Floating_TitleBgActive_Color.Z}f, {CFG.Current.Floating_TitleBgActive_Color.W}f);\n";
            output = output + $"CFG.Current.Floating_ChildBg_Color = new Vector4({CFG.Current.Floating_ChildBg_Color.X}f, {CFG.Current.Floating_ChildBg_Color.Y}f, {CFG.Current.Floating_ChildBg_Color.Z}f, {CFG.Current.Floating_ChildBg_Color.W}f);\n";
            output = output + $"CFG.Current.Floating_Header_Color = new Vector4({CFG.Current.Floating_Header_Color.X}f, {CFG.Current.Floating_Header_Color.Y}f, {CFG.Current.Floating_Header_Color.Z}f, {CFG.Current.Floating_Header_Color.W}f);\n";
            output = output + $"CFG.Current.Logger_WindowBg_Color = new Vector4({CFG.Current.Logger_WindowBg_Color.X}f, {CFG.Current.Logger_WindowBg_Color.Y}f, {CFG.Current.Logger_WindowBg_Color.Z}f, {CFG.Current.Logger_WindowBg_Color.W}f);\n";
            output = output + $"CFG.Current.Logger_TitleBg_Color = new Vector4({CFG.Current.Logger_TitleBg_Color.X}f, {CFG.Current.Logger_TitleBg_Color.Y}f, {CFG.Current.Logger_TitleBg_Color.Z}f, {CFG.Current.Logger_TitleBg_Color.W}f);\n";
            output = output + $"CFG.Current.Logger_TitleBgActive_Color = new Vector4({CFG.Current.Logger_TitleBgActive_Color.X}f, {CFG.Current.Logger_TitleBgActive_Color.Y}f, {CFG.Current.Logger_TitleBgActive_Color.Z}f, {CFG.Current.Logger_TitleBgActive_Color.W}f);\n";
            output = output + $"CFG.Current.Logger_TitleBgActive_Color = new Vector4({CFG.Current.Logger_TitleBgActive_Color.X}f, {CFG.Current.Logger_TitleBgActive_Color.Y}f, {CFG.Current.Logger_TitleBgActive_Color.Z}f, {CFG.Current.Logger_TitleBgActive_Color.W}f);\n";
            output = output + $"CFG.Current.Logger_ChildBg_Color = new Vector4({CFG.Current.Logger_ChildBg_Color.X}f, {CFG.Current.Logger_ChildBg_Color.Y}f, {CFG.Current.Logger_ChildBg_Color.Z}f, {CFG.Current.Logger_ChildBg_Color.W}f);\n";
            output = output + $"CFG.Current.Logger_Text_Warning_Color = new Vector4({CFG.Current.Logger_Text_Warning_Color.X}f, {CFG.Current.Logger_Text_Warning_Color.Y}f, {CFG.Current.Logger_Text_Warning_Color.Z}f, {CFG.Current.Logger_Text_Warning_Color.W}f);\n";
            output = output + $"CFG.Current.ParamEditor_RowField_Color = new Vector4({CFG.Current.ParamEditor_RowField_Color.X}f, {CFG.Current.ParamEditor_RowField_Color.Y}f, {CFG.Current.ParamEditor_RowField_Color.Z}f, {CFG.Current.ParamEditor_RowField_Color.W}f);\n";
            output = output + $"CFG.Current.ParamEditor_Row_Frame_Conflict_Color = new Vector4({CFG.Current.ParamEditor_Row_Frame_Conflict_Color.X}f, {CFG.Current.ParamEditor_Row_Frame_Conflict_Color.Y}f, {CFG.Current.ParamEditor_Row_Frame_Conflict_Color.Z}f, {CFG.Current.ParamEditor_Row_Frame_Conflict_Color.W}f);\n";
            output = output + $"CFG.Current.ParamEditor_Row_Frame_Vanilla_Color = new Vector4({CFG.Current.ParamEditor_Row_Frame_Vanilla_Color.X}f, {CFG.Current.ParamEditor_Row_Frame_Vanilla_Color.Y}f, {CFG.Current.ParamEditor_Row_Frame_Vanilla_Color.Z}f, {CFG.Current.ParamEditor_Row_Frame_Vanilla_Color.W}f);\n";
            output = output + $"CFG.Current.ParamEditor_Row_Text_IsRef_Color = new Vector4({CFG.Current.ParamEditor_Row_Text_IsRef_Color.X}f, {CFG.Current.ParamEditor_Row_Text_IsRef_Color.Y}f, {CFG.Current.ParamEditor_Row_Text_IsRef_Color.Z}f, {CFG.Current.ParamEditor_Row_Text_IsRef_Color.W}f);\n";
            output = output + $"CFG.Current.ParamEditor_Row_Text_Match = new Vector4({CFG.Current.ParamEditor_Row_Text_Match.X}f, {CFG.Current.ParamEditor_Row_Text_Match.Y}f, {CFG.Current.ParamEditor_Row_Text_Match.Z}f, {CFG.Current.ParamEditor_Row_Text_Match.W}f);\n";
            output = output + $"CFG.Current.ParamEditor_Row_Frame_Default_Color = new Vector4({CFG.Current.ParamEditor_Row_Frame_Default_Color.X}f, {CFG.Current.ParamEditor_Row_Frame_Default_Color.Y}f, {CFG.Current.ParamEditor_Row_Frame_Default_Color.Z}f, {CFG.Current.ParamEditor_Row_Frame_Default_Color.W}f);\n";
            output = output + $"CFG.Current.ParamEditor_Row_Text_Default = new Vector4({CFG.Current.ParamEditor_Row_Text_Default.X}f, {CFG.Current.ParamEditor_Row_Text_Default.Y}f, {CFG.Current.ParamEditor_Row_Text_Default.Z}f, {CFG.Current.ParamEditor_Row_Text_Default.W}f);\n";
            output = output + $"CFG.Current.ParamEditor_Row_Frame_AuxVanilla_Color = new Vector4({CFG.Current.ParamEditor_Row_Frame_AuxVanilla_Color.X}f, {CFG.Current.ParamEditor_Row_Frame_AuxVanilla_Color.Y}f, {CFG.Current.ParamEditor_Row_Frame_AuxVanilla_Color.Z}f, {CFG.Current.ParamEditor_Row_Frame_AuxVanilla_Color.W}f);\n";
            output = output + $"CFG.Current.ParamEditor_Row_Frame_DiffCompare_Color = new Vector4({CFG.Current.ParamEditor_Row_Frame_DiffCompare_Color.X}f, {CFG.Current.ParamEditor_Row_Frame_DiffCompare_Color.Y}f, {CFG.Current.ParamEditor_Row_Frame_DiffCompare_Color.Z}f, {CFG.Current.ParamEditor_Row_Frame_DiffCompare_Color.W}f);\n";
            output = output + $"CFG.Current.ParamEditor_Row_Text_VirtualRef_Color = new Vector4({CFG.Current.ParamEditor_Row_Text_VirtualRef_Color.X}f, {CFG.Current.ParamEditor_Row_Text_VirtualRef_Color.Y}f, {CFG.Current.ParamEditor_Row_Text_VirtualRef_Color.Z}f, {CFG.Current.ParamEditor_Row_Text_VirtualRef_Color.W}f);\n";
            output = output + $"CFG.Current.ParamEditor_Row_Text_Ref_Color = new Vector4({CFG.Current.ParamEditor_Row_Text_Ref_Color.X}f, {CFG.Current.ParamEditor_Row_Text_Ref_Color.Y}f, {CFG.Current.ParamEditor_Row_Text_Ref_Color.Z}f, {CFG.Current.ParamEditor_Row_Text_Ref_Color.W}f);\n";
            output = output + $"CFG.Current.ParamEditor_Row_View_AuxConflict_Color = new Vector4({CFG.Current.ParamEditor_Row_View_AuxConflict_Color.X}f, {CFG.Current.ParamEditor_Row_View_AuxConflict_Color.Y}f, {CFG.Current.ParamEditor_Row_View_AuxConflict_Color.Z}f, {CFG.Current.ParamEditor_Row_View_AuxConflict_Color.W}f);\n";
            output = output + $"CFG.Current.ParamEditor_Row_View_AuxAdded_Color = new Vector4({CFG.Current.ParamEditor_Row_View_AuxAdded_Color.X}f, {CFG.Current.ParamEditor_Row_View_AuxAdded_Color.Y}f, {CFG.Current.ParamEditor_Row_View_AuxAdded_Color.Z}f, {CFG.Current.ParamEditor_Row_View_AuxAdded_Color.W}f);\n";
            output = output + $"CFG.Current.ParamEditor_Row_View_PrimaryChanged_Color = new Vector4({CFG.Current.ParamEditor_Row_View_PrimaryChanged_Color.X}f, {CFG.Current.ParamEditor_Row_View_PrimaryChanged_Color.Y}f, {CFG.Current.ParamEditor_Row_View_PrimaryChanged_Color.Z}f, {CFG.Current.ParamEditor_Row_View_PrimaryChanged_Color.W}f);\n";
            output = output + $"CFG.Current.ParamEditor_Row_View_AllVanilla_Color = new Vector4({CFG.Current.ParamEditor_Row_View_AllVanilla_Color.X}f, {CFG.Current.ParamEditor_Row_View_AllVanilla_Color.Y}f, {CFG.Current.ParamEditor_Row_View_AllVanilla_Color.Z}f, {CFG.Current.ParamEditor_Row_View_AllVanilla_Color.W}f);\n";
            output = output + $"CFG.Current.ParamEditor_Row_View_FmgLink_Color = new Vector4({CFG.Current.ParamEditor_Row_View_FmgLink_Color.X}f, {CFG.Current.ParamEditor_Row_View_FmgLink_Color.Y}f, {CFG.Current.ParamEditor_Row_View_FmgLink_Color.Z}f, {CFG.Current.ParamEditor_Row_View_FmgLink_Color.W}f);\n";
            output = output + $"CFG.Current.ModelEditor_SceneTree_ChildBg = new Vector4({CFG.Current.ModelEditor_SceneTree_ChildBg.X}f, {CFG.Current.ModelEditor_SceneTree_ChildBg.Y}f, {CFG.Current.ModelEditor_SceneTree_ChildBg.Z}f, {CFG.Current.ModelEditor_SceneTree_ChildBg.W}f);\n";
            output = output + $"CFG.Current.ModelEditor_Properties_ChildBg = new Vector4({CFG.Current.ModelEditor_Properties_ChildBg.X}f, {CFG.Current.ModelEditor_Properties_ChildBg.Y}f, {CFG.Current.ModelEditor_Properties_ChildBg.Z}f, {CFG.Current.ModelEditor_Properties_ChildBg.W}f);\n";
            output = output + $"CFG.Current.ModelEditor_MultipleEdit_Frame = new Vector4({CFG.Current.ModelEditor_MultipleEdit_Frame.X}f, {CFG.Current.ModelEditor_MultipleEdit_Frame.Y}f, {CFG.Current.ModelEditor_MultipleEdit_Frame.Z}f, {CFG.Current.ModelEditor_MultipleEdit_Frame.W}f);\n";
            output = output + $"CFG.Current.MapEditor_SceneTree_ChildBg = new Vector4({CFG.Current.MapEditor_SceneTree_ChildBg.X}f, {CFG.Current.MapEditor_SceneTree_ChildBg.Y}f, {CFG.Current.MapEditor_SceneTree_ChildBg.Z}f, {CFG.Current.MapEditor_SceneTree_ChildBg.W}f);\n";
            output = output + $"CFG.Current.MapEditor_Properties_ChildBg = new Vector4({CFG.Current.MapEditor_Properties_ChildBg.X}f, {CFG.Current.MapEditor_Properties_ChildBg.Y}f, {CFG.Current.MapEditor_Properties_ChildBg.Z}f, {CFG.Current.MapEditor_Properties_ChildBg.W}f);\n";
            output = output + $"CFG.Current.MapEditor_Error_Frame = new Vector4({CFG.Current.MapEditor_Error_Frame.X}f, {CFG.Current.MapEditor_Error_Frame.Y}f, {CFG.Current.MapEditor_Error_Frame.Z}f, {CFG.Current.MapEditor_Error_Frame.W}f);\n";
            output = output + $"CFG.Current.MapEditor_MultipleEdit_Frame = new Vector4({CFG.Current.MapEditor_MultipleEdit_Frame.X}f, {CFG.Current.MapEditor_MultipleEdit_Frame.Y}f, {CFG.Current.MapEditor_MultipleEdit_Frame.Z}f, {CFG.Current.MapEditor_MultipleEdit_Frame.W}f);\n";
            output = output + $"CFG.Current.MapEditor_ParamRow_Text = new Vector4({CFG.Current.MapEditor_ParamRow_Text.X}f, {CFG.Current.MapEditor_ParamRow_Text.Y}f, {CFG.Current.MapEditor_ParamRow_Text.Z}f, {CFG.Current.MapEditor_ParamRow_Text.W}f);\n";
            output = output + $"CFG.Current.DisplayGroupEditor_Border_Highlight = new Vector4({CFG.Current.DisplayGroupEditor_Border_Highlight.X}f, {CFG.Current.DisplayGroupEditor_Border_Highlight.Y}f, {CFG.Current.DisplayGroupEditor_Border_Highlight.Z}f, {CFG.Current.DisplayGroupEditor_Border_Highlight.W}f);\n";
            output = output + $"CFG.Current.DisplayGroupEditor_DisplayActive_Frame = new Vector4({CFG.Current.DisplayGroupEditor_DisplayActive_Frame.X}f, {CFG.Current.DisplayGroupEditor_DisplayActive_Frame.Y}f, {CFG.Current.DisplayGroupEditor_DisplayActive_Frame.Z}f, {CFG.Current.DisplayGroupEditor_DisplayActive_Frame.W}f);\n";
            output = output + $"CFG.Current.DisplayGroupEditor_DisplayActive_Checkbox = new Vector4({CFG.Current.DisplayGroupEditor_DisplayActive_Checkbox.X}f, {CFG.Current.DisplayGroupEditor_DisplayActive_Checkbox.Y}f, {CFG.Current.DisplayGroupEditor_DisplayActive_Checkbox.Z}f, {CFG.Current.DisplayGroupEditor_DisplayActive_Checkbox.W}f);\n";
            output = output + $"CFG.Current.DisplayGroupEditor_DrawActive_Frame = new Vector4({CFG.Current.DisplayGroupEditor_DrawActive_Frame.X}f, {CFG.Current.DisplayGroupEditor_DrawActive_Frame.Y}f, {CFG.Current.DisplayGroupEditor_DrawActive_Frame.Z}f, {CFG.Current.DisplayGroupEditor_DrawActive_Frame.W}f);\n";
            output = output + $"CFG.Current.DisplayGroupEditor_DrawActive_Checkbox = new Vector4({CFG.Current.DisplayGroupEditor_DrawActive_Checkbox.X}f, {CFG.Current.DisplayGroupEditor_DrawActive_Checkbox.Y}f, {CFG.Current.DisplayGroupEditor_DrawActive_Checkbox.Z}f, {CFG.Current.DisplayGroupEditor_DrawActive_Checkbox.W}f);\n";
            output = output + $"CFG.Current.DisplayGroupEditor_CombinedActive_Frame = new Vector4({CFG.Current.DisplayGroupEditor_CombinedActive_Frame.X}f, {CFG.Current.DisplayGroupEditor_CombinedActive_Frame.Y}f, {CFG.Current.DisplayGroupEditor_CombinedActive_Frame.Z}f, {CFG.Current.DisplayGroupEditor_CombinedActive_Frame.W}f);\n";
            output = output + $"CFG.Current.DisplayGroupEditor_CombinedActive_Checkbox = new Vector4({CFG.Current.DisplayGroupEditor_CombinedActive_Checkbox.X}f, {CFG.Current.DisplayGroupEditor_CombinedActive_Checkbox.Y}f, {CFG.Current.DisplayGroupEditor_CombinedActive_Checkbox.Z}f, {CFG.Current.DisplayGroupEditor_CombinedActive_Checkbox.W}f);\n";
            output = output + $"CFG.Current.ProgramUpdate_Available_Color = new Vector4({CFG.Current.ProgramUpdate_Available_Color.X}f, {CFG.Current.ProgramUpdate_Available_Color.Y}f, {CFG.Current.ProgramUpdate_Available_Color.Z}f, {CFG.Current.ProgramUpdate_Available_Color.W}f);\n";
            output = output + $"CFG.Current.UpgradeParam_Text_Available_Color = new Vector4({CFG.Current.UpgradeParam_Text_Available_Color.X}f, {CFG.Current.UpgradeParam_Text_Available_Color.Y}f, {CFG.Current.UpgradeParam_Text_Available_Color.Z}f, {CFG.Current.UpgradeParam_Text_Available_Color.W}f);\n";
            output = output + $"CFG.Current.UpgradeParam_Text_OutOfDate_Color = new Vector4({CFG.Current.UpgradeParam_Text_OutOfDate_Color.X}f, {CFG.Current.UpgradeParam_Text_OutOfDate_Color.Y}f, {CFG.Current.UpgradeParam_Text_OutOfDate_Color.Z}f, {CFG.Current.UpgradeParam_Text_OutOfDate_Color.W}f);\n";
            output = output + $"CFG.Current.UpgradeParam_Text_Invalid_Color = new Vector4({CFG.Current.UpgradeParam_Text_Invalid_Color.X}f, {CFG.Current.UpgradeParam_Text_Invalid_Color.Y}f, {CFG.Current.UpgradeParam_Text_Invalid_Color.Z}f, {CFG.Current.UpgradeParam_Text_Invalid_Color.W}f);\n";

            PlatformUtils.Instance.SetClipboardText(output);
        }
    }
}
