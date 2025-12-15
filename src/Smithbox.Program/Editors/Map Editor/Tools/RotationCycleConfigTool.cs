using Hexa.NET.ImGui;
using StudioCore.Application;
using System;

namespace StudioCore.Editors.MapEditor;

public class RotationCycleConfigTool
{
    public MapEditorScreen Editor;
    public ProjectEntry Project;

    public RotationCycleConfigTool(MapEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    /// <summary>
    /// Shortcuts
    /// </summary>
    public void OnShortcut()
    {
        // Only apply in Map Editor screen
        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_SwitchRotationDegreeIncrementType))
        {
            CycleIncrementType();
        }
        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_SwitchRotationDegreeIncrementTypeBackward))
        {
            CycleIncrementType(true);
        }
    }

    /// <summary>
    /// Tool Window
    /// </summary>
    public void OnToolWindow()
    {
        var windowWidth = ImGui.GetWindowWidth();

        if (ImGui.CollapsingHeader("Rotation Increments"))
        {
            UIHelper.SimpleHeader("Current Rotation Increment", "Current Rotation Increment", $"Shortcut: {KeyBindings.Current.MAP_SwitchRotationDegreeIncrementType.HintText}", UI.Current.ImGui_Default_Text_Color);

            Editor.RotationCycleConfigTool.DisplayCurrentRotateIncrement();

            ImGui.Checkbox("Display rotation increment in viewport", ref CFG.Current.Viewport_DisplayRotationIncrement);
            UIHelper.Tooltip("Display the current degree increment type you are using in the information panel.");

            if (ImGui.Button("Cycle Increment", DPI.WholeWidthButton(windowWidth, 24)))
            {
                Editor.RotationCycleConfigTool.CycleIncrementType();
            }
            UIHelper.Tooltip($"Press {KeyBindings.Current.MAP_SwitchRotationDegreeIncrementType.HintText} to cycle the degree increment used by Rotate Selection on X/Y Axis.");

            UIHelper.SimpleHeader("Degree Increment [0]", "Degree Increment [0]", "", UI.Current.ImGui_Default_Text_Color);
            DPI.ApplyInputWidth(windowWidth);

            var rot0 = CFG.Current.Toolbar_Rotate_Increment_0;
            if (ImGui.SliderFloat("##degreeIncrement0", ref rot0, -360.0f, 360.0f))
            {
                CFG.Current.Toolbar_Rotate_Increment_0 = Math.Clamp(rot0, -360.0f, 360.0f);
            }
            UIHelper.Tooltip("Press Ctrl+Left Click to input directly.\nSet the angle increment amount used by the rotation.");

            UIHelper.SimpleHeader("Degree Increment [1]", "Degree Increment [1]", "", UI.Current.ImGui_Default_Text_Color);
            DPI.ApplyInputWidth(windowWidth);

            var rot1 = CFG.Current.Toolbar_Rotate_Increment_1;
            if (ImGui.SliderFloat("##degreeIncrement1", ref rot1, -360.0f, 360.0f))
            {
                CFG.Current.Toolbar_Rotate_Increment_1 = Math.Clamp(rot1, -360.0f, 360.0f);
            }
            UIHelper.Tooltip("Press Ctrl+Left Click to input directly.\nSet the angle increment amount used by the rotation.");

            UIHelper.SimpleHeader("Degree Increment [2]", "Degree Increment [2]", "", UI.Current.ImGui_Default_Text_Color);
            DPI.ApplyInputWidth(windowWidth);

            var rot2 = CFG.Current.Toolbar_Rotate_Increment_2;
            if (ImGui.SliderFloat("##degreeIncrement2", ref rot2, -360.0f, 360.0f))
            {
                CFG.Current.Toolbar_Rotate_Increment_2 = Math.Clamp(rot2, -360.0f, 360.0f);
            }
            UIHelper.Tooltip("Press Ctrl+Left Click to input directly.\nSet the angle increment amount used by the rotation.");

            UIHelper.SimpleHeader("Degree Increment [3]", "Degree Increment [3]", "", UI.Current.ImGui_Default_Text_Color);
            DPI.ApplyInputWidth(windowWidth);

            var rot3 = CFG.Current.Toolbar_Rotate_Increment_3;
            if (ImGui.SliderFloat("##degreeIncrement3", ref rot3, -360.0f, 360.0f))
            {
                CFG.Current.Toolbar_Rotate_Increment_3 = Math.Clamp(rot3, -360.0f, 360.0f);
            }
            UIHelper.Tooltip("Press Ctrl+Left Click to input directly.\nSet the angle increment amount used by the rotation.");

            UIHelper.SimpleHeader("Degree Increment [4]", "Degree Increment [4]", "", UI.Current.ImGui_Default_Text_Color);
            DPI.ApplyInputWidth(windowWidth);

            var rot4 = CFG.Current.Toolbar_Rotate_Increment_4;
            if (ImGui.SliderFloat("##degreeIncrement4", ref rot4, -360.0f, 360.0f))
            {
                CFG.Current.Toolbar_Rotate_Increment_4 = Math.Clamp(rot4, -360.0f, 360.0f);
            }
            UIHelper.Tooltip("Press Ctrl+Left Click to input directly.\nSet the angle increment amount used by the rotation.");
        }
    }

    public void CycleIncrementType(bool decrement = false)
    {
        if (decrement)
        {
            CFG.Current.Toolbar_Rotate_IncrementType -= 1;
            if (CFG.Current.Toolbar_Rotate_IncrementType < 0)
            {
                CFG.Current.Toolbar_Rotate_IncrementType = 4;
            }
        }
        else
        {
            CFG.Current.Toolbar_Rotate_IncrementType += 1;
            if (CFG.Current.Toolbar_Rotate_IncrementType > 4)
            {
                CFG.Current.Toolbar_Rotate_IncrementType = 0;
            }
        }
    }

    public void DisplayCurrentRotateIncrement()
    {
        switch (CFG.Current.Toolbar_Rotate_IncrementType)
        {
            case 0:
                UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, $"Degree Increment [0]: {CFG.Current.Toolbar_Rotate_Increment_0}°");
                break;
            case 1:
                UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, $"Degree Increment [1]: {CFG.Current.Toolbar_Rotate_Increment_1}°");
                break;
            case 2:
                UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, $"Degree Increment [2]: {CFG.Current.Toolbar_Rotate_Increment_2}°");
                break;
            case 3:
                UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, $"Degree Increment [3]: {CFG.Current.Toolbar_Rotate_Increment_3}°");
                break;
            case 4:
                UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, $"Degree Increment [4]: {CFG.Current.Toolbar_Rotate_Increment_4}°");
                break;
        }
        UIHelper.Tooltip($"Press {KeyBindings.Current.MAP_SwitchRotationDegreeIncrementType.HintText} to cycle the degree increment used by Rotate Selection on X/Y Axis.");
    }

    public void DisplayViewportRotateIncrement()
    {
        switch (CFG.Current.Toolbar_Rotate_IncrementType)
        {
            case 0:
                UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, $"Degree Increment: {CFG.Current.Toolbar_Rotate_Increment_0}°");
                break;
            case 1:
                UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, $"Degree Increment: {CFG.Current.Toolbar_Rotate_Increment_1}°");
                break;
            case 2:
                UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, $"Degree Increment: {CFG.Current.Toolbar_Rotate_Increment_2}°");
                break;
            case 3:
                UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, $"Degree Increment: {CFG.Current.Toolbar_Rotate_Increment_3}°");
                break;
            case 4:
                UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, $"Degree Increment: {CFG.Current.Toolbar_Rotate_Increment_4}°");
                break;
        }
        UIHelper.Tooltip($"Press {KeyBindings.Current.MAP_SwitchRotationDegreeIncrementType.HintText} to cycle the degree increment used by Rotate Selection on X/Y Axis.");
    }

    public float GetRadianRotateAmount()
    {
        var amount = (float)Math.PI / 180 * CFG.Current.Toolbar_Rotate_Increment_0;

        switch (CFG.Current.Toolbar_Rotate_IncrementType)
        {
            case 0:
                amount = (float)Math.PI / 180 * CFG.Current.Toolbar_Rotate_Increment_0;
                break;
            case 1:
                amount = (float)Math.PI / 180 * CFG.Current.Toolbar_Rotate_Increment_1;
                break;
            case 2:
                amount = (float)Math.PI / 180 * CFG.Current.Toolbar_Rotate_Increment_2;
                break;
            case 3:
                amount = (float)Math.PI / 180 * CFG.Current.Toolbar_Rotate_Increment_3;
                break;
            case 4:
                amount = (float)Math.PI / 180 * CFG.Current.Toolbar_Rotate_Increment_4;
                break;
        }

        return amount;
    }
}
