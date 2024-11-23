using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudioCore.Configuration.Interface;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata,
    IncludeFields = true,
    ReadCommentHandling = JsonCommentHandling.Skip)
]
[JsonSerializable(typeof(InterfaceThemeConfig))]
public partial class InterfaceThemeSerializationContext
    : JsonSerializerContext
{ }

public class InterfaceThemeConfig
{
    public int id { get; set; }

    public string name { get; set; }

    public List<float> ImGui_MainBg { get; set; }
    public List<float> ImGui_ChildBg { get; set; }
    public List<float> ImGui_PopupBg { get; set; }
    public List<float> ImGui_Border { get; set; }
    public List<float> ImGui_TitleBarBg { get; set; }
    public List<float> ImGui_TitleBarBg_Active { get; set; }
    public List<float> ImGui_MenuBarBg { get; set; }
    public List<float> Imgui_Moveable_MainBg { get; set; }
    public List<float> Imgui_Moveable_ChildBg { get; set; }
    public List<float> Imgui_Moveable_ChildBgSecondary { get; set; }
    public List<float> Imgui_Moveable_TitleBg { get; set; }
    public List<float> Imgui_Moveable_TitleBg_Active { get; set; }
    public List<float> Imgui_Moveable_Header { get; set; }
    public List<float> ImGui_ScrollbarBg { get; set; }
    public List<float> ImGui_ScrollbarGrab { get; set; }
    public List<float> ImGui_ScrollbarGrab_Hover { get; set; }
    public List<float> ImGui_ScrollbarGrab_Active { get; set; }
    public List<float> ImGui_SliderGrab { get; set; }
    public List<float> ImGui_SliderGrab_Active { get; set; }
    public List<float> ImGui_Tab { get; set; }
    public List<float> ImGui_Tab_Hover { get; set; }
    public List<float> ImGui_Tab_Active { get; set; }
    public List<float> ImGui_UnfocusedTab { get; set; }
    public List<float> ImGui_UnfocusedTab_Active { get; set; }
    public List<float> ImGui_Button { get; set; }
    public List<float> ImGui_Button_Hovered { get; set; }
    public List<float> ImGui_ButtonActive { get; set; }
    public List<float> ImGui_Selection { get; set; }
    public List<float> ImGui_Selection_Hover { get; set; }
    public List<float> ImGui_Selection_Active { get; set; }
    public List<float> ImGui_Input_Background { get; set; }
    public List<float> ImGui_Input_Background_Hover { get; set; }
    public List<float> ImGui_Input_Background_Active { get; set; }
    public List<float> ImGui_Input_CheckMark { get; set; }
    public List<float> ImGui_Input_Conflict_Background { get; set; }
    public List<float> ImGui_Input_Vanilla_Background { get; set; }
    public List<float> ImGui_Input_Default_Background { get; set; }
    public List<float> ImGui_Input_AuxVanilla_Background { get; set; }
    public List<float> ImGui_Input_DiffCompare_Background { get; set; }
    public List<float> ImGui_MultipleInput_Background { get; set; }
    public List<float> ImGui_ErrorInput_Background { get; set; }
    public List<float> ImGui_Default_Text_Color { get; set; }
    public List<float> ImGui_Warning_Text_Color { get; set; }
    public List<float> ImGui_Benefit_Text_Color { get; set; }
    public List<float> ImGui_Invalid_Text_Color { get; set; }

    public List<float> ImGui_TimeAct_InfoText_1_Color { get; set; }
    public List<float> ImGui_TimeAct_InfoText_2_Color { get; set; }
    public List<float> ImGui_TimeAct_InfoText_3_Color { get; set; }
    public List<float> ImGui_TimeAct_InfoText_4_Color { get; set; }
    public List<float> ImGui_Conditional_Text_Color5 { get; set; }

    public List<float> ImGui_ParamRef_Text { get; set; }
    public List<float> ImGui_ParamRefMissing_Text { get; set; }
    public List<float> ImGui_ParamRefInactive_Text { get; set; }
    public List<float> ImGui_EnumName_Text { get; set; }
    public List<float> ImGui_EnumValue_Text { get; set; }
    public List<float> ImGui_FmgLink_Text { get; set; }
    public List<float> ImGui_FmgRef_Text { get; set; }
    public List<float> ImGui_FmgRefInactive_Text { get; set; }
    public List<float> ImGui_IsRef_Text { get; set; }
    public List<float> ImGui_VirtualRef_Text { get; set; }
    public List<float> ImGui_Ref_Text { get; set; }
    public List<float> ImGui_AuxConflict_Text { get; set; }
    public List<float> ImGui_AuxAdded_Text { get; set; }
    public List<float> ImGui_PrimaryChanged_Text { get; set; }
    public List<float> ImGui_ParamRow_Text { get; set; }
    public List<float> ImGui_AliasName_Text { get; set; }
    public List<float> ImGui_TextEditor_ModifiedRow_Text { get; set; }
    public List<float> ImGui_TextEditor_UniqueRow_Text { get; set; }

    public List<float> ImGui_Logger_Information_Color { get; set; }
    public List<float> ImGui_Logger_Warning_Color { get; set; }
    public List<float> ImGui_Logger_Error_Color { get; set; }

    public List<float> DisplayGroupEditor_Border_Highlight { get; set; }
    public List<float> DisplayGroupEditor_DisplayActive_Frame { get; set; }
    public List<float> DisplayGroupEditor_DisplayActive_Checkbox { get; set; }
    public List<float> DisplayGroupEditor_DrawActive_Frame { get; set; }
    public List<float> DisplayGroupEditor_DrawActive_Checkbox { get; set; }
    public List<float> DisplayGroupEditor_CombinedActive_Frame { get; set; }
    public List<float> DisplayGroupEditor_CombinedActive_Checkbox { get; set; }
}
