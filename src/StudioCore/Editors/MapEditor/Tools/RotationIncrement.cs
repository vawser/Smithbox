using StudioCore.Configuration;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Tools;

public static class RotationIncrement
{
    public static void Shortcuts()
    {
        // Only apply in Map Editor screen
        if (Smithbox.EditorHandler.FocusedEditor is MapEditorScreen)
        {
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_SwitchDegreeIncrementType))
            {
                CycleIncrementType();
            }
        }
    }

    public static void CycleIncrementType()
    {
        CFG.Current.Toolbar_Rotate_IncrementType += 1;
        if (CFG.Current.Toolbar_Rotate_IncrementType > 4)
        {
            CFG.Current.Toolbar_Rotate_IncrementType = 0;
        }
    }

    public static void DisplayCurrentRotateIncrement()
    {
        switch (CFG.Current.Toolbar_Rotate_IncrementType)
        {
            case 0:
                ImguiUtils.WrappedTextColored(CFG.Current.ImGui_AliasName_Text, $"Degree Increment [0]: {CFG.Current.Toolbar_Rotate_Increment_0}°");
                break;
            case 1:
                ImguiUtils.WrappedTextColored(CFG.Current.ImGui_AliasName_Text, $"Degree Increment [1]: {CFG.Current.Toolbar_Rotate_Increment_1}°");
                break;
            case 2:
                ImguiUtils.WrappedTextColored(CFG.Current.ImGui_AliasName_Text, $"Degree Increment [2]: {CFG.Current.Toolbar_Rotate_Increment_2}°");
                break;
            case 3:
                ImguiUtils.WrappedTextColored(CFG.Current.ImGui_AliasName_Text, $"Degree Increment [3]: {CFG.Current.Toolbar_Rotate_Increment_3}°");
                break;
            case 4:
                ImguiUtils.WrappedTextColored(CFG.Current.ImGui_AliasName_Text, $"Degree Increment [4]: {CFG.Current.Toolbar_Rotate_Increment_4}°");
                break;
        }
        ImguiUtils.ShowHoverTooltip($"Press {KeyBindings.Current.MAP_SwitchDegreeIncrementType.HintText} to cycle the degree increment used by Rotate Selection on X/Y Axis.");
    }

    public static void DisplayViewportRotateIncrement()
    {
        switch (CFG.Current.Toolbar_Rotate_IncrementType)
        {
            case 0:
                ImguiUtils.WrappedTextColored(CFG.Current.ImGui_AliasName_Text, $"Degree Increment: {CFG.Current.Toolbar_Rotate_Increment_0}°");
                break;
            case 1:
                ImguiUtils.WrappedTextColored(CFG.Current.ImGui_AliasName_Text, $"Degree Increment: {CFG.Current.Toolbar_Rotate_Increment_1}°");
                break;
            case 2:
                ImguiUtils.WrappedTextColored(CFG.Current.ImGui_AliasName_Text, $"Degree Increment: {CFG.Current.Toolbar_Rotate_Increment_2}°");
                break;
            case 3:
                ImguiUtils.WrappedTextColored(CFG.Current.ImGui_AliasName_Text, $"Degree Increment: {CFG.Current.Toolbar_Rotate_Increment_3}°");
                break;
            case 4:
                ImguiUtils.WrappedTextColored(CFG.Current.ImGui_AliasName_Text, $"Degree Increment: {CFG.Current.Toolbar_Rotate_Increment_4}°");
                break;
        }
        ImguiUtils.ShowHoverTooltip($"Press {KeyBindings.Current.MAP_SwitchDegreeIncrementType.HintText} to cycle the degree increment used by Rotate Selection on X/Y Axis.");
    }

    public static float GetRadianRotateAmount()
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
