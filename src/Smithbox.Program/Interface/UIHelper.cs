using Google.Protobuf.WellKnownTypes;
using Hexa.NET.DirectXTex;
using Hexa.NET.ImGui;
using StudioCore.Utilities;
using System;
using System.IO;
using System.Numerics;
using System.Text;
using static HKLib.hk2018.hkSerialize.CompatTypeParentInfo;

namespace StudioCore.Application;
public static class UIHelper
{
    public static uint EDITOR_DOCK_GROUP = 1000;
    public static uint PROJECT_EDITOR_DOCK_GROUP = 1001;
    public static uint ANIM_EDITOR_DOCK_GROUP = 1002;
    public static uint FILE_BROWSER_DOCK_GROUP = 1003;
    public static uint GPARAM_EDITOR_DOCK_GROUP = 1004;
    public static uint MAP_DATA_EDITOR_DOCK_GROUP = 1005;
    public static uint MAP_EDITOR_DOCK_GROUP = 1006;
    public static uint MATERIAL_EDITOR_DOCK_GROUP = 1007;
    public static uint MODEL_EDITOR_DOCK_GROUP = 1008;
    public static uint PARAM_EDITOR_DOCK_GROUP = 1009;
    public static uint TEXT_EDITOR_DOCK_GROUP = 1010;
    public static uint TEXTURE_VIEWER_DOCK_GROUP = 1011;
    public static uint PROJECT_METADATA_EDITOR_DOCK_GROUP = 1012;

    public static uint MAP_EDITOR_VIEW_DOCK_GROUP = 1100;
    public static uint MODEL_EDITOR_VIEW_DOCK_GROUP = 1101;
    public static uint ANIM_EDITOR_VIEW_DOCK_GROUP = 1102;
    public static uint FILE_BROWSER_VIEW_DOCK_GROUP = 1103;
    public static uint GPARAM_EDITOR_VIEW_DOCK_GROUP = 1104;
    public static uint MAP_DATA_EDITOR_VIEW_DOCK_GROUP = 1105;
    public static uint MATERIAL_EDITOR_VIEW_DOCK_GROUP = 1106;
    public static uint PARAM_EDITOR_VIEW_DOCK_GROUP = 1107;
    public static uint TEXT_EDITOR_VIEW_DOCK_GROUP = 1108;
    public static uint TEXTURE_VIEWER_VIEW_DOCK_GROUP = 1109;
    public static uint PROJECT_EDITOR_VIEW_DOCK_GROUP = 1110;
    public static uint PROJECT_METADATA_EDITOR_VIEW_DOCK_GROUP = 1111;

    public static ImGuiWindowClass DockGroup_EditorView = new(
        classId: EDITOR_DOCK_GROUP, 
        dockingAllowUnclassed: false);

    public static ImGuiWindowClass DockGroup_ProjectEditor = new(
        classId: PROJECT_EDITOR_DOCK_GROUP, 
        dockingAllowUnclassed: false);

    public static ImGuiWindowClass DockGroup_ProjectMetadataEditor = new(
        classId: PROJECT_METADATA_EDITOR_DOCK_GROUP,
        dockingAllowUnclassed: false);

    public static ImGuiWindowClass DockGroup_AnimEditor = new(
        classId: ANIM_EDITOR_DOCK_GROUP,
        dockingAllowUnclassed: false);

    public static ImGuiWindowClass DockGroup_FileBrowser = new(
        classId: FILE_BROWSER_DOCK_GROUP,
        dockingAllowUnclassed: false);

    public static ImGuiWindowClass DockGroup_GparamEditor = new(
        classId: GPARAM_EDITOR_DOCK_GROUP,
        dockingAllowUnclassed: false);

    public static ImGuiWindowClass DockGroup_MapDataEditor = new(
        classId: MAP_DATA_EDITOR_DOCK_GROUP,
        dockingAllowUnclassed: false);

    public static ImGuiWindowClass DockGroup_MapEditor = new(
        classId: MAP_EDITOR_DOCK_GROUP,
        dockingAllowUnclassed: false);

    public static ImGuiWindowClass DockGroup_MaterialEditor = new(
        classId: MATERIAL_EDITOR_DOCK_GROUP,
        dockingAllowUnclassed: false);

    public static ImGuiWindowClass DockGroup_ModelEditor = new(
        classId: MODEL_EDITOR_DOCK_GROUP,
        dockingAllowUnclassed: false);

    public static ImGuiWindowClass DockGroup_ParamEditor = new(
        classId: PARAM_EDITOR_DOCK_GROUP, 
        dockingAllowUnclassed: false);

    public static ImGuiWindowClass DockGroup_TextEditor = new(
        classId: TEXT_EDITOR_DOCK_GROUP,
        dockingAllowUnclassed: false);

    public static ImGuiWindowClass DockGroup_TextureViewer = new(
        classId: TEXTURE_VIEWER_DOCK_GROUP,
        dockingAllowUnclassed: false);

    public static ImGuiWindowClass DockGroup_MapEditorView = new(
        classId: MAP_EDITOR_VIEW_DOCK_GROUP,
        dockingAllowUnclassed: false);

    public static ImGuiWindowClass DockGroup_ModelEditorView = new(
        classId: MODEL_EDITOR_VIEW_DOCK_GROUP,
        dockingAllowUnclassed: false);

    public static ImGuiWindowClass DockGroup_AnimEditorView = new(
        classId: ANIM_EDITOR_VIEW_DOCK_GROUP,
        dockingAllowUnclassed: false);

    public static ImGuiWindowClass DockGroup_FileBrowserView = new(
        classId: FILE_BROWSER_VIEW_DOCK_GROUP,
        dockingAllowUnclassed: false);

    public static ImGuiWindowClass DockGroup_GparamEditorView = new(
        classId: GPARAM_EDITOR_VIEW_DOCK_GROUP,
        dockingAllowUnclassed: false);

    public static ImGuiWindowClass DockGroup_MapDataEditorView = new(
        classId: MAP_DATA_EDITOR_VIEW_DOCK_GROUP,
        dockingAllowUnclassed: false);

    public static ImGuiWindowClass DockGroup_MaterialEditorView = new(
        classId: MATERIAL_EDITOR_VIEW_DOCK_GROUP,
        dockingAllowUnclassed: false);

    public static ImGuiWindowClass DockGroup_ParamEditorView = new(
        classId: PARAM_EDITOR_VIEW_DOCK_GROUP,
        dockingAllowUnclassed: false);

    public static ImGuiWindowClass DockGroup_TextEditorView = new(
        classId: TEXT_EDITOR_VIEW_DOCK_GROUP,
        dockingAllowUnclassed: false);

    public static ImGuiWindowClass DockGroup_TextureViewerView = new(
        classId: TEXTURE_VIEWER_VIEW_DOCK_GROUP,
        dockingAllowUnclassed: false);

    public static ImGuiWindowClass DockGroup_ProjectEditorView = new(
        classId: PROJECT_EDITOR_VIEW_DOCK_GROUP,
        dockingAllowUnclassed: false);

    public static ImGuiWindowClass DockGroup_ProjectMetadataEditorView = new(
        classId: PROJECT_METADATA_EDITOR_VIEW_DOCK_GROUP,
        dockingAllowUnclassed: false);

    public static void ApplyBaseStyle()
    {
        var scale = DPI.UIScale();
        ImGuiStylePtr style = ImGui.GetStyle();

        // Colors
        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.PushStyleColor(ImGuiCol.WindowBg, UI.Current.ImGui_MainBg);
        ImGui.PushStyleColor(ImGuiCol.ChildBg, UI.Current.ImGui_ChildBg);
        ImGui.PushStyleColor(ImGuiCol.PopupBg, UI.Current.ImGui_PopupBg);
        ImGui.PushStyleColor(ImGuiCol.Border, UI.Current.ImGui_Border);
        ImGui.PushStyleColor(ImGuiCol.FrameBg, UI.Current.ImGui_Input_Background);
        ImGui.PushStyleColor(ImGuiCol.FrameBgHovered, UI.Current.ImGui_Input_Background_Hover);
        ImGui.PushStyleColor(ImGuiCol.FrameBgActive, UI.Current.ImGui_Input_Background_Active);
        ImGui.PushStyleColor(ImGuiCol.TitleBg, UI.Current.ImGui_TitleBarBg);
        ImGui.PushStyleColor(ImGuiCol.TitleBgActive, UI.Current.ImGui_TitleBarBg_Active);
        ImGui.PushStyleColor(ImGuiCol.MenuBarBg, UI.Current.ImGui_MenuBarBg);
        ImGui.PushStyleColor(ImGuiCol.ScrollbarBg, UI.Current.ImGui_ScrollbarBg);
        ImGui.PushStyleColor(ImGuiCol.ScrollbarGrab, UI.Current.ImGui_ScrollbarGrab);
        ImGui.PushStyleColor(ImGuiCol.ScrollbarGrabHovered, UI.Current.ImGui_ScrollbarGrab_Hover);
        ImGui.PushStyleColor(ImGuiCol.ScrollbarGrabActive, UI.Current.ImGui_ScrollbarGrab_Active);
        ImGui.PushStyleColor(ImGuiCol.CheckMark, UI.Current.ImGui_Input_CheckMark);
        ImGui.PushStyleColor(ImGuiCol.SliderGrab, UI.Current.ImGui_SliderGrab);
        ImGui.PushStyleColor(ImGuiCol.SliderGrabActive, UI.Current.ImGui_SliderGrab_Active);
        ImGui.PushStyleColor(ImGuiCol.Button, UI.Current.ImGui_Button);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, UI.Current.ImGui_Button_Hovered);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, UI.Current.ImGui_ButtonActive);
        ImGui.PushStyleColor(ImGuiCol.Header, UI.Current.ImGui_Selection);
        ImGui.PushStyleColor(ImGuiCol.HeaderHovered, UI.Current.ImGui_Selection_Hover);
        ImGui.PushStyleColor(ImGuiCol.HeaderActive, UI.Current.ImGui_Selection_Active);
        ImGui.PushStyleColor(ImGuiCol.Tab, UI.Current.ImGui_Tab);
        ImGui.PushStyleColor(ImGuiCol.TabHovered, UI.Current.ImGui_Tab_Hover);
        ImGui.PushStyleColor(ImGuiCol.TabSelected, UI.Current.ImGui_Tab_Active);
        ImGui.PushStyleColor(ImGuiCol.TabDimmed, UI.Current.ImGui_UnfocusedTab);
        ImGui.PushStyleColor(ImGuiCol.TabDimmedSelected, UI.Current.ImGui_UnfocusedTab_Active);

        // Sizes
        ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 1.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.TabRounding, 0.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.ScrollbarRounding, 0.0f);

        ImGui.PushStyleVar(ImGuiStyleVar.ScrollbarSize, 16.0f * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowMinSize, new Vector2(100f, 100f) * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, style.FramePadding * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, style.CellPadding * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.IndentSpacing, style.IndentSpacing * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, style.ItemSpacing * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemInnerSpacing, style.ItemInnerSpacing * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 1);
        ImGui.PushStyleVar(ImGuiStyleVar.ChildBorderSize, 1);
        ImGui.PushStyleVar(ImGuiStyleVar.PopupBorderSize, 1);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 1);
    }

    public static void UnapplyBaseStyle()
    {
        ImGui.PopStyleColor(29);
        ImGui.PopStyleVar(14);
    }

    public static void RestoreImguiIfMissing()
    {
        var curImgui = Path.Join(AppContext.BaseDirectory, "imgui.ini");
        var defaultImgui = Path.Join(AppContext.BaseDirectory, "imgui.default");

        if (!File.Exists(curImgui) && File.Exists(defaultImgui))
        {
            var bytes = File.ReadAllBytes(defaultImgui);
            File.WriteAllBytes(curImgui, bytes);
        }
    }

    public static void ShowMenuIcon(string iconStr)
    {
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, ImGui.GetStyle().ItemSpacing.Y));
        ImGui.TextUnformatted(iconStr);
        ImGui.PopStyleVar(1);
        ImGui.SameLine();
    }

    public static void ShowActiveStatus(bool isActive)
    {
        if (isActive)
        {
            ImGui.SameLine();
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, ImGui.GetStyle().ItemSpacing.Y));
            ImGui.TextUnformatted($"{Icons.CheckSquare}");
            ImGui.PopStyleVar(1);
        }
        else
        {
            ImGui.SameLine();
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, ImGui.GetStyle().ItemSpacing.Y));
            ImGui.TextUnformatted($"{Icons.Square}");
            ImGui.PopStyleVar(1);
        }
    }

    public static void Tooltip(string desc)
    {
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.PushTextWrapPos(450.0f);
            ImGui.TextUnformatted(desc);
            ImGui.PopTextWrapPos();
            ImGui.EndTooltip();
        }
    }

    public static void Spacer()
    {
        var size = ImGui.GetWindowSize();

        ImGui.PushTextWrapPos(size.X);
        ImGui.TextUnformatted("");
        ImGui.PopTextWrapPos();
    }

    public static void WrappedText(string text)
    {
        var size = ImGui.GetWindowSize();

        ImGui.PushTextWrapPos(size.X);
        ImGui.TextUnformatted(text);
        ImGui.PopTextWrapPos();
    }

    public static void WrappedTextColored(Vector4 color, string text)
    {
        var size = ImGui.GetWindowSize();

        ImGui.PushTextWrapPos(size.X);
        ImGui.PushStyleColor(ImGuiCol.Text, color);
        ImGui.TextUnformatted(text);
        ImGui.PopStyleColor();
        ImGui.PopTextWrapPos();
    }

    public static void WideTooltip(string desc)
    {
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.PushTextWrapPos(800.0f);
            ImGui.TextUnformatted(desc);
            ImGui.PopTextWrapPos();
            ImGui.EndTooltip();
        }
    }

    public static string GetKeybindHint(string hint)
    {
        if (hint == "")
            return "None";
        else
            return hint;
    }

    public static void WrappedYellowText(string text)
    {
        var size = ImGui.GetWindowSize();

        ImGui.PushTextWrapPos(size.X);
        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_AliasName_Text);
        ImGui.TextUnformatted(text);
        ImGui.PopStyleColor();
        ImGui.PopTextWrapPos();
    }

    public static void DisplayAlias(string aliasName)
    {
        if (aliasName != "")
        {
            ImGui.SameLine();
            if (CFG.Current.Interface_Alias_Wordwrap_General)
            {
                ImGui.PushTextWrapPos();
                ImGui.TextColored(UI.Current.ImGui_AliasName_Text, @$"{aliasName}");
                ImGui.PopTextWrapPos();
            }
            else
            {
                ImGui.TextColored(UI.Current.ImGui_AliasName_Text, @$"{aliasName}");
            }
        }
    }

    public static void DisplayAlias(string aliasName, bool customWrapBool)
    {
        if (aliasName != "")
        {
            ImGui.SameLine();
            if (customWrapBool)
            {
                ImGui.PushTextWrapPos();
                ImGui.TextColored(UI.Current.ImGui_AliasName_Text, @$"{aliasName}");
                ImGui.PopTextWrapPos();
            }
            else
            {
                ImGui.TextColored(UI.Current.ImGui_AliasName_Text, @$"{aliasName}");
            }
        }
    }

    public static void DisplayColoredAlias(string aliasName, Vector4 color)
    {
        if (aliasName != "")
        {
            ImGui.SameLine();
            if (CFG.Current.Interface_Alias_Wordwrap_General)
            {
                ImGui.PushTextWrapPos();
                ImGui.TextColored(color, @$"{aliasName}");
                ImGui.PopTextWrapPos();
            }
            else
            {
                ImGui.TextColored(color, @$"{aliasName}");
            }
        }
    }

    public static void DisplayColoredAlias(string aliasName, Vector4 color, bool customWrapBool)
    {
        if (aliasName != "")
        {
            ImGui.SameLine();
            if (customWrapBool)
            {
                ImGui.PushTextWrapPos();
                ImGui.TextColored(color, @$"{aliasName}");
                ImGui.PopTextWrapPos();
            }
            else
            {
                ImGui.TextColored(color, @$"{aliasName}");
            }
        }
    }

    public static void CopyToClipboard(string text)
    {
        PlatformUtils.Instance.SetClipboardText(text);
    }

    public static void SimpleHeader(string title, string tooltip)
    {
        var tblFlags = ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders;

        if (ImGui.BeginTable($"{title.GetHashCode()}", 1, tblFlags))
        {
            ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthFixed);

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.AlignTextToFramePadding();
            ImGui.Text($"{title}");

            if (tooltip != "")
            {
                UIHelper.Tooltip(tooltip);
            }

            ImGui.EndTable();
        }
    }

    public static void SimpleHeader(string id, string title, string tooltip, Vector4 textColor)
    {
        var tblFlags = ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders;

        if (ImGui.BeginTable($"{id}", 1, tblFlags))
        {
            ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthFixed);

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.AlignTextToFramePadding();
            ImGui.TextColored(textColor, $"{title}");

            if (tooltip != "")
            {
                UIHelper.Tooltip(tooltip);
            }

            ImGui.EndTable();
        }
    }

    public static void ConditionalHeader(string id, string title, string tooltip, Vector4 textColor, ref bool visibilityToggle)
    {
        var tblFlags = ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders;

        if (ImGui.BeginTable($"{id}", 1, tblFlags))
        {
            ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthFixed);

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.AlignTextToFramePadding();
            ImGui.TextColored(textColor, $"{title}");
            ImGui.SameLine();

            var icon = visibilityToggle ? Icons.Eye : Icons.EyeSlash;

            ImGui.PushItemFlag(ImGuiItemFlags.NoNav, true);
            ImGui.PushStyleColor(ImGuiCol.Button, Vector4.Zero);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, Vector4.Zero);
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, Vector4.Zero);
            ImGui.PushStyleColor(ImGuiCol.Border, Vector4.Zero);
            if (ImGui.Button($"{icon}", DPI.InlineIconButtonSize))
            {
                visibilityToggle = !visibilityToggle;
            }
            ImGui.PopStyleColor(4);
            ImGui.PopItemFlag();

            UIHelper.Tooltip(tooltip);

            ImGui.EndTable();
        }
    }


    public static void ConditionalHeader(string title, string tooltip, ref bool visibilityToggle)
    {
        var tblFlags = ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders;

        if (ImGui.BeginTable($"{title.GetHashCode()}", 1, tblFlags))
        {
            ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthFixed);

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.AlignTextToFramePadding();
            ImGui.Text($"{title}");
            ImGui.SameLine();

            var icon = visibilityToggle ? Icons.Eye : Icons.EyeSlash;

            ImGui.PushItemFlag(ImGuiItemFlags.NoNav, true);
            ImGui.PushStyleColor(ImGuiCol.Button, Vector4.Zero);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, Vector4.Zero);
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, Vector4.Zero);
            ImGui.PushStyleColor(ImGuiCol.Border, Vector4.Zero);
            if (ImGui.Button($"{icon}", DPI.InlineIconButtonSize))
            {
                visibilityToggle = !visibilityToggle;
            }
            ImGui.PopStyleColor(4);
            ImGui.PopItemFlag();

            UIHelper.Tooltip(tooltip);

            ImGui.EndTable();
        }
    }

    public static nuint GetTextInputBuffer(string contents)
    {
        int byteCount = Encoding.UTF8.GetByteCount(contents) + 1;
        return (nuint)byteCount;
    }


    public static ImGuiWindowFlags GetMainWindowFlags()
    {
        var flags = ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;

        if (!CFG.Current.Interface_Allow_Window_Movement)
        {
            flags |= ImGuiWindowFlags.NoMove;
        }

        return flags;
    }
    public static ImGuiWindowFlags GetInnerWindowFlags()
    {
        var flags = ImGuiWindowFlags.None | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;

        if (!CFG.Current.Interface_Allow_Window_Movement)
        {
            flags |= ImGuiWindowFlags.NoMove;
        }

        return flags;
    }
    public static ImGuiWindowFlags GetTextureViewerFlags()
    {
        var flags = ImGuiWindowFlags.None | ImGuiWindowFlags.AlwaysHorizontalScrollbar | ImGuiWindowFlags.AlwaysVerticalScrollbar;

        if (!CFG.Current.Interface_Allow_Window_Movement)
        {
            flags |= ImGuiWindowFlags.NoMove;
        }

        return flags;
    }
    public static ImGuiWindowFlags GetDisplayViewWindowFlags()
    {
        var flags = ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoNav;

        if (!CFG.Current.Interface_Allow_Window_Movement)
        {
            flags |= ImGuiWindowFlags.NoMove;
        }

        return flags;
    }

    public static ImGuiWindowFlags GetPopupWindowFlags()
    {
        var flags = ImGuiWindowFlags.NoCollapse;

        return flags;
    }
    public static ImGuiWindowFlags GetEditorPopupWindowFlags()
    {
        var flags = ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.MenuBar;

        return flags;
    }

    public static ImGuiWindowFlags GetFloatingWindowFlags()
    {
        var flags = ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoDocking;

        return flags;
    }
    public static ImGuiWindowFlags GetLoggerWindowFlags()
    {
        var flags = ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDocking;

        return flags;
    }

    public static ImGuiWindowFlags GetToolPaletteWindowFlags()
    {
        var flags = ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoDocking;

        return flags;
    }

    public static float GetEnumListHeight(int count)
    {
        return 20 + (ImGui.GetTextLineHeightWithSpacing() * Math.Min(12, count) * 1f);
    }

    public static void SetupPopupWindow()
    {
        var viewport = ImGui.GetMainViewport();
        Vector2 center = viewport.Pos + viewport.Size / 2;

        ImGui.SetNextWindowPos(center, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));

        var width = ImGui.GetIO().DisplaySize.X;
        var height = ImGui.GetIO().DisplaySize.Y;
        var size = new Vector2(width * 0.5f, height * 0.5f);
        ImGui.SetNextWindowSize(size);
    }

    public static void ApplyDiffHeaderBackground()
    {
        Vector2 pMin = ImGui.GetCursorScreenPos();
        Vector2 pMax = new Vector2(
            pMin.X + ImGui.GetContentRegionAvail().X,
            pMin.Y + ImGui.GetTextLineHeightWithSpacing()
        );

        var color = new Vector4(0.255f, 0.412f, 0.125f, 1.0f);

        color = UI.Current.ParamRowDiffBackgroundColor;

        // Draw background
        ImGui.GetWindowDrawList().AddRectFilled(
            pMin,
            pMax,
            ImGui.ColorConvertFloat4ToU32(color)
        );
    }

    // Sizing
    public static void SetInputWidth()
    {
        ImGui.SetNextItemWidth((ImGui.GetWindowWidth() * 0.75f) * DPI.UIScale());
    }

    public static Vector2 GetSmallPopupSize()
    {
        var width = ImGui.GetWindowWidth();
        var height = ImGui.GetWindowHeight();

        return new Vector2(width * 0.25f, height * 0.3f);
    }

    public static Vector2 GetMediumPopupSize()
    {
        var width = ImGui.GetWindowWidth();
        var height = ImGui.GetWindowHeight();

        return new Vector2(width * 0.25f, height * 0.5f);
    }

    // Int Input
    public static void IntInput(string id, ref int input, string name = "")
    {
        var tblFlags = ImGuiTableFlags.SizingFixedFit;

        if (ImGui.BeginTable($"{id}", 1, tblFlags))
        {
            ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthFixed);

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            var width = ImGui.GetWindowWidth() * 0.5f;
            if (name == "")
                width = ImGui.GetWindowWidth();

            ImGui.PushItemWidth(width);
            ImGui.InputInt($"{name}##{id}_input", ref input);

            ImGui.EndTable();
        }
    }

    // Text Input
    public static void HintTextInput(string id, ref string input, string hint, string name = "")
    {
        var tblFlags = ImGuiTableFlags.SizingFixedFit;

        if (ImGui.BeginTable($"{id}", 1, tblFlags))
        {
            ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthFixed);

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            var width = ImGui.GetWindowWidth() * 0.5f;
            if (name == "")
                width = ImGui.GetWindowWidth();

            ImGui.PushItemWidth(width);
            ImGui.InputTextWithHint($"{name}##{id}_input", hint, ref input, 255);

            ImGui.EndTable();
        }
    }

    public static void SinglelineTextInput(string id, ref string input, string name = "")
    {
        var tblFlags = ImGuiTableFlags.SizingFixedFit;

        if (ImGui.BeginTable($"{id}", 1, tblFlags))
        {
            ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthFixed);

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            var width = ImGui.GetWindowWidth() * 0.5f;
            if (name == "")
                width = ImGui.GetWindowWidth();

            ImGui.PushItemWidth(width);
            ImGui.InputText($"{name}##{id}_input", ref input, 255);

            ImGui.EndTable();
        }
    }

    public static void MultilineTextInput(string id, ref string input)
    {
        var tblFlags = ImGuiTableFlags.SizingFixedFit;

        if (ImGui.BeginTable($"{id}", 1, tblFlags))
        {
            ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthFixed);

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            var inputSize = GetMultilineTextSize();
            ImGui.InputTextMultiline($"##{id}_input", ref input, 65536, inputSize);

            ImGui.EndTable();
        }
    }
    public static void SinglelineTextInputWithHint(string id, ref string input, string hint)
    {
        var tblFlags = ImGuiTableFlags.SizingFixedFit;

        if (ImGui.BeginTable($"{id}", 1, tblFlags))
        {
            ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthFixed);

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            var width = ImGui.GetWindowWidth() * 0.5f;

            ImGui.PushItemWidth(width);
            ImGui.InputTextWithHint($"##{id}_input", hint, ref input, 255);

            ImGui.EndTable();
        }
    }

    public static Vector2 GetMultilineTextSize(float height = 150f)
    {
        var width = ImGui.GetWindowWidth();

        return new Vector2(width, height);
    }

    // Button Input
    public static void ButtonInputEntry(string buttonId, string buttonTitle, string buttonTooltip, Action buttonFunc, bool allowButtonFunc = true)
    {
        ImGui.TableNextRow();
        ImGui.TableSetColumnIndex(0);

        ImGui.PushStyleVar(ImGuiStyleVar.ButtonTextAlign, new Vector2(0.01f, 0.5f));
        if (allowButtonFunc)
        {
            if (ImGui.Button($"{buttonTitle}##{buttonId}", GetButtonSize()))
            {
                buttonFunc.Invoke();
            }
        }
        else
        {
            ImGui.BeginDisabled();
            if (ImGui.Button($"{buttonTitle}##{buttonId}", GetButtonSize()))
            {
            }
            ImGui.EndDisabled();
        }
        ImGui.PopStyleVar();
        if(buttonTooltip != "")
        {
            Tooltip(buttonTooltip);
        }
    }

    public static Vector2 GetButtonSize(float height = 24f)
    {
        var width = ImGui.GetWindowWidth();

        return new Vector2(width, height);
    }

    public static void MultiButtonInput(string id,
        string id1, string title1, string tooltip1, Action func1)
    {
        var tblFlags = ImGuiTableFlags.SizingFixedFit;

        if (ImGui.BeginTable($"{id}", 1, tblFlags))
        {
            ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthFixed);

            ButtonInputEntry(id1, title1, tooltip1, func1);

            ImGui.EndTable();
        }
    }

    public static void ConditionalMultiButtonInput(string id,
        string id1, string title1, string tooltip1, Action func1, bool allowFunc1)
    {
        var tblFlags = ImGuiTableFlags.SizingFixedFit;

        if (ImGui.BeginTable($"{id}", 1, tblFlags))
        {
            ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthFixed);

            ButtonInputEntry(id1, title1, tooltip1, func1, allowFunc1);

            ImGui.EndTable();
        }
    }

    public static void MultiButtonInput(string id, 
        string id1, string title1, string tooltip1, Action func1,
        string id2, string title2, string tooltip2, Action func2)
    {
        var tblFlags = ImGuiTableFlags.SizingFixedFit;

        if (ImGui.BeginTable($"{id}", 1, tblFlags))
        {
            ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthFixed);

            ButtonInputEntry(id1, title1, tooltip1, func1);
            ButtonInputEntry(id2, title2, tooltip2, func2);

            ImGui.EndTable();
        }
    }

    public static void ConditionalMultiButtonInput(string id,
        string id1, string title1, string tooltip1, Action func1, bool allowFunc1,
        string id2, string title2, string tooltip2, Action func2, bool allowFunc2)
    {
        var tblFlags = ImGuiTableFlags.SizingFixedFit;

        if (ImGui.BeginTable($"{id}", 1, tblFlags))
        {
            ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthFixed);

            ButtonInputEntry(id1, title1, tooltip1, func1, allowFunc1);
            ButtonInputEntry(id2, title2, tooltip2, func2, allowFunc2);

            ImGui.EndTable();
        }
    }

    public static void MultiButtonInput(string id,
        string id1, string title1, string tooltip1, Action func1,
        string id2, string title2, string tooltip2, Action func2,
        string id3, string title3, string tooltip3, Action func3)
    {
        var tblFlags = ImGuiTableFlags.SizingFixedFit;

        if (ImGui.BeginTable($"{id}", 1, tblFlags))
        {
            ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthFixed);

            ButtonInputEntry(id1, title1, tooltip1, func1);
            ButtonInputEntry(id2, title2, tooltip2, func2);
            ButtonInputEntry(id3, title3, tooltip3, func3);

            ImGui.EndTable();
        }
    }

    public static void ConditionalMultiButtonInput(string id,
        string id1, string title1, string tooltip1, Action func1, bool allowFunc1,
        string id2, string title2, string tooltip2, Action func2, bool allowFunc2,
        string id3, string title3, string tooltip3, Action func3, bool allowFunc3)
    {
        var tblFlags = ImGuiTableFlags.SizingFixedFit;

        if (ImGui.BeginTable($"{id}", 1, tblFlags))
        {
            ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthFixed);

            ButtonInputEntry(id1, title1, tooltip1, func1, allowFunc1);
            ButtonInputEntry(id2, title2, tooltip2, func2, allowFunc2);
            ButtonInputEntry(id3, title3, tooltip3, func3, allowFunc3);

            ImGui.EndTable();
        }
    }

    public static void MultiButtonInput(string id,
        string id1, string title1, string tooltip1, Action func1,
        string id2, string title2, string tooltip2, Action func2,
        string id3, string title3, string tooltip3, Action func3,
        string id4, string title4, string tooltip4, Action func4)
    {
        var tblFlags = ImGuiTableFlags.SizingFixedFit;

        if (ImGui.BeginTable($"{id}", 1, tblFlags))
        {
            ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthFixed);

            ButtonInputEntry(id1, title1, tooltip1, func1);
            ButtonInputEntry(id2, title2, tooltip2, func2);
            ButtonInputEntry(id3, title3, tooltip3, func3);
            ButtonInputEntry(id4, title4, tooltip4, func4);

            ImGui.EndTable();
        }
    }

    public static void ConditionalMultiButtonInput(string id,
        string id1, string title1, string tooltip1, Action func1, bool allowFunc1,
        string id2, string title2, string tooltip2, Action func2, bool allowFunc2,
        string id3, string title3, string tooltip3, Action func3, bool allowFunc3,
        string id4, string title4, string tooltip4, Action func4, bool allowFunc4)
    {
        var tblFlags = ImGuiTableFlags.SizingFixedFit;

        if (ImGui.BeginTable($"{id}", 1, tblFlags))
        {
            ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthFixed);

            ButtonInputEntry(id1, title1, tooltip1, func1, allowFunc1);
            ButtonInputEntry(id2, title2, tooltip2, func2, allowFunc2);
            ButtonInputEntry(id3, title3, tooltip3, func3, allowFunc3);
            ButtonInputEntry(id4, title4, tooltip4, func4, allowFunc4);

            ImGui.EndTable();
        }
    }

    public static void MultiButtonInput(string id,
        string id1, string title1, string tooltip1, Action func1,
        string id2, string title2, string tooltip2, Action func2,
        string id3, string title3, string tooltip3, Action func3,
        string id4, string title4, string tooltip4, Action func4,
        string id5, string title5, string tooltip5, Action func5)
    {
        var tblFlags = ImGuiTableFlags.SizingFixedFit;

        if (ImGui.BeginTable($"{id}", 1, tblFlags))
        {
            ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthFixed);

            ButtonInputEntry(id1, title1, tooltip1, func1);
            ButtonInputEntry(id2, title2, tooltip2, func2);
            ButtonInputEntry(id3, title3, tooltip3, func3);
            ButtonInputEntry(id4, title4, tooltip4, func4);
            ButtonInputEntry(id5, title5, tooltip5, func5);

            ImGui.EndTable();
        }
    }

    public static void ConditionalMultiButtonInput(string id,
        string id1, string title1, string tooltip1, Action func1, bool allowFunc1,
        string id2, string title2, string tooltip2, Action func2, bool allowFunc2,
        string id3, string title3, string tooltip3, Action func3, bool allowFunc3,
        string id4, string title4, string tooltip4, Action func4, bool allowFunc4,
        string id5, string title5, string tooltip5, Action func5, bool allowFunc5)
    {
        var tblFlags = ImGuiTableFlags.SizingFixedFit;

        if (ImGui.BeginTable($"{id}", 1, tblFlags))
        {
            ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthFixed);

            ButtonInputEntry(id1, title1, tooltip1, func1, allowFunc1);
            ButtonInputEntry(id2, title2, tooltip2, func2, allowFunc2);
            ButtonInputEntry(id3, title3, tooltip3, func3, allowFunc3);
            ButtonInputEntry(id4, title4, tooltip4, func4, allowFunc4);
            ButtonInputEntry(id5, title5, tooltip5, func5, allowFunc5);

            ImGui.EndTable();
        }
    }

    public static void MultiButtonInput(string id,
        string id1, string title1, string tooltip1, Action func1,
        string id2, string title2, string tooltip2, Action func2,
        string id3, string title3, string tooltip3, Action func3,
        string id4, string title4, string tooltip4, Action func4,
        string id5, string title5, string tooltip5, Action func5,
        string id6, string title6, string tooltip6, Action func6)
    {
        var tblFlags = ImGuiTableFlags.SizingFixedFit;

        if (ImGui.BeginTable($"{id}", 1, tblFlags))
        {
            ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthFixed);

            ButtonInputEntry(id1, title1, tooltip1, func1);
            ButtonInputEntry(id2, title2, tooltip2, func2);
            ButtonInputEntry(id3, title3, tooltip3, func3);
            ButtonInputEntry(id4, title4, tooltip4, func4);
            ButtonInputEntry(id5, title5, tooltip5, func5);
            ButtonInputEntry(id6, title6, tooltip6, func6);

            ImGui.EndTable();
        }
    }

    public static void ConditionalMultiButtonInput(string id,
        string id1, string title1, string tooltip1, Action func1, bool allowFunc1,
        string id2, string title2, string tooltip2, Action func2, bool allowFunc2,
        string id3, string title3, string tooltip3, Action func3, bool allowFunc3,
        string id4, string title4, string tooltip4, Action func4, bool allowFunc4,
        string id5, string title5, string tooltip5, Action func5, bool allowFunc5,
        string id6, string title6, string tooltip6, Action func6, bool allowFunc6)
    {
        var tblFlags = ImGuiTableFlags.SizingFixedFit;

        if (ImGui.BeginTable($"{id}", 1, tblFlags))
        {
            ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthFixed);

            ButtonInputEntry(id1, title1, tooltip1, func1, allowFunc1);
            ButtonInputEntry(id2, title2, tooltip2, func2, allowFunc2);
            ButtonInputEntry(id3, title3, tooltip3, func3, allowFunc3);
            ButtonInputEntry(id4, title4, tooltip4, func4, allowFunc4);
            ButtonInputEntry(id5, title5, tooltip5, func5, allowFunc5);
            ButtonInputEntry(id6, title6, tooltip6, func6, allowFunc6);

            ImGui.EndTable();
        }
    }

    public static void MultiButtonInput(string id,
        string id1, string title1, string tooltip1, Action func1,
        string id2, string title2, string tooltip2, Action func2,
        string id3, string title3, string tooltip3, Action func3,
        string id4, string title4, string tooltip4, Action func4,
        string id5, string title5, string tooltip5, Action func5,
        string id6, string title6, string tooltip6, Action func6,
        string id7, string title7, string tooltip7, Action func7)
    {
        var tblFlags = ImGuiTableFlags.SizingFixedFit;

        if (ImGui.BeginTable($"{id}", 1, tblFlags))
        {
            ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthFixed);

            ButtonInputEntry(id1, title1, tooltip1, func1);
            ButtonInputEntry(id2, title2, tooltip2, func2);
            ButtonInputEntry(id3, title3, tooltip3, func3);
            ButtonInputEntry(id4, title4, tooltip4, func4);
            ButtonInputEntry(id5, title5, tooltip5, func5);
            ButtonInputEntry(id6, title6, tooltip6, func6);
            ButtonInputEntry(id7, title7, tooltip7, func7);

            ImGui.EndTable();
        }
    }

    public static void ConditionalMultiButtonInput(string id,
        string id1, string title1, string tooltip1, Action func1, bool allowFunc1,
        string id2, string title2, string tooltip2, Action func2, bool allowFunc2,
        string id3, string title3, string tooltip3, Action func3, bool allowFunc3,
        string id4, string title4, string tooltip4, Action func4, bool allowFunc4,
        string id5, string title5, string tooltip5, Action func5, bool allowFunc5,
        string id6, string title6, string tooltip6, Action func6, bool allowFunc6,
        string id7, string title7, string tooltip7, Action func7, bool allowFunc7)
    {
        var tblFlags = ImGuiTableFlags.SizingFixedFit;

        if (ImGui.BeginTable($"{id}", 1, tblFlags))
        {
            ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthFixed);

            ButtonInputEntry(id1, title1, tooltip1, func1, allowFunc1);
            ButtonInputEntry(id2, title2, tooltip2, func2, allowFunc2);
            ButtonInputEntry(id3, title3, tooltip3, func3, allowFunc3);
            ButtonInputEntry(id4, title4, tooltip4, func4, allowFunc4);
            ButtonInputEntry(id5, title5, tooltip5, func5, allowFunc5);
            ButtonInputEntry(id6, title6, tooltip6, func6, allowFunc6);
            ButtonInputEntry(id7, title7, tooltip7, func7, allowFunc7);

            ImGui.EndTable();
        }
    }

    public static void MultiButtonInput(string id,
        string id1, string title1, string tooltip1, Action func1,
        string id2, string title2, string tooltip2, Action func2,
        string id3, string title3, string tooltip3, Action func3,
        string id4, string title4, string tooltip4, Action func4,
        string id5, string title5, string tooltip5, Action func5,
        string id6, string title6, string tooltip6, Action func6,
        string id7, string title7, string tooltip7, Action func7,
        string id8, string title8, string tooltip8, Action func8)
    {
        var tblFlags = ImGuiTableFlags.SizingFixedFit;

        if (ImGui.BeginTable($"{id}", 1, tblFlags))
        {
            ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthFixed);

            ButtonInputEntry(id1, title1, tooltip1, func1);
            ButtonInputEntry(id2, title2, tooltip2, func2);
            ButtonInputEntry(id3, title3, tooltip3, func3);
            ButtonInputEntry(id4, title4, tooltip4, func4);
            ButtonInputEntry(id5, title5, tooltip5, func5);
            ButtonInputEntry(id6, title6, tooltip6, func6);
            ButtonInputEntry(id7, title7, tooltip7, func7);
            ButtonInputEntry(id8, title8, tooltip8, func8);

            ImGui.EndTable();
        }
    }

    public static void ConditionalMultiButtonInput(string id,
        string id1, string title1, string tooltip1, Action func1, bool allowFunc1,
        string id2, string title2, string tooltip2, Action func2, bool allowFunc2,
        string id3, string title3, string tooltip3, Action func3, bool allowFunc3,
        string id4, string title4, string tooltip4, Action func4, bool allowFunc4,
        string id5, string title5, string tooltip5, Action func5, bool allowFunc5,
        string id6, string title6, string tooltip6, Action func6, bool allowFunc6,
        string id7, string title7, string tooltip7, Action func7, bool allowFunc7,
        string id8, string title8, string tooltip8, Action func8, bool allowFunc8)
    {
        var tblFlags = ImGuiTableFlags.SizingFixedFit;

        if (ImGui.BeginTable($"{id}", 1, tblFlags))
        {
            ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthFixed);

            ButtonInputEntry(id1, title1, tooltip1, func1, allowFunc1);
            ButtonInputEntry(id2, title2, tooltip2, func2, allowFunc2);
            ButtonInputEntry(id3, title3, tooltip3, func3, allowFunc3);
            ButtonInputEntry(id4, title4, tooltip4, func4, allowFunc4);
            ButtonInputEntry(id5, title5, tooltip5, func5, allowFunc5);
            ButtonInputEntry(id6, title6, tooltip6, func6, allowFunc6);
            ButtonInputEntry(id7, title7, tooltip7, func7, allowFunc7);
            ButtonInputEntry(id8, title8, tooltip8, func8, allowFunc8);

            ImGui.EndTable();
        }
    }

}

public class InputTextHandler
{
    private byte[] _buffer;

    public InputTextHandler(string initialValue, int size = 512)
    {
        _buffer = new byte[size];
        Update(initialValue);
    }

    public void Update(string value)
    {
        Array.Clear(_buffer, 0, _buffer.Length);
        Encoding.UTF8.GetBytes(value ?? "", 0, value?.Length ?? 0, _buffer, 0);
    }

    public bool Draw(string label, out string result)
    {
        bool changed = false;
        unsafe
        {
            fixed (byte* bufPtr = _buffer)
            {
                if (ImGui.InputText(label, bufPtr, (uint)_buffer.Length))
                {
                    int len = Array.IndexOf(_buffer, (byte)0);
                    result = Encoding.UTF8.GetString(_buffer, 0, len >= 0 ? len : _buffer.Length);
                    changed = true;
                }
                else
                {
                    result = null;
                }
            }
        }
        return changed;
    }

}

/// <summary>
/// Version that only applies change after deactivation of edit
/// </summary>
public class DelayedInputTextHandler
{
    private byte[] _buffer;

    public DelayedInputTextHandler(string initialValue, int size = 512)
    {
        _buffer = new byte[size];
        Update(initialValue);
    }

    public void Update(string value)
    {
        Array.Clear(_buffer, 0, _buffer.Length);
        Encoding.UTF8.GetBytes(value ?? "", 0, value?.Length ?? 0, _buffer, 0);
    }

    public bool Draw(string label, out string result)
    {
        bool changed = false;
        unsafe
        {
            fixed (byte* bufPtr = _buffer)
            {
                if (ImGui.InputText(label, bufPtr, (uint)_buffer.Length))
                {
                    int len = Array.IndexOf(_buffer, (byte)0);
                    result = Encoding.UTF8.GetString(_buffer, 0, len >= 0 ? len : _buffer.Length);
                }
                else
                {
                    result = null;
                }

                if(ImGui.IsItemDeactivatedAfterEdit())
                {
                    changed = true;
                }
            }
        }
        return changed;
    }

}