using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Tools;

public class RotationIncrement
{
    public MapEditorScreen Editor;
    public ProjectEntry Project;

    public RotationIncrement(MapEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Shortcuts()
    {
        // Only apply in Map Editor screen
        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_SwitchDegreeIncrementType))
        {
            CycleIncrementType();
        }
        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_SwitchDegreeIncrementTypeBackward))
        {
            CycleIncrementType(true);
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
        UIHelper.Tooltip($"Press {KeyBindings.Current.MAP_SwitchDegreeIncrementType.HintText} to cycle the degree increment used by Rotate Selection on X/Y Axis.");
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
        UIHelper.Tooltip($"Press {KeyBindings.Current.MAP_SwitchDegreeIncrementType.HintText} to cycle the degree increment used by Rotate Selection on X/Y Axis.");
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
