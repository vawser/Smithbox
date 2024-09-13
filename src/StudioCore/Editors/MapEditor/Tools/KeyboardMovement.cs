using Silk.NET.OpenGL;
using StudioCore.Configuration;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Tools;

public static class KeyboardMovement
{
    public static void CycleIncrementType()
    {
        CFG.Current.MapEditor_Selection_Movement_IncrementType += 1;
        if (CFG.Current.MapEditor_Selection_Movement_IncrementType > 4)
        {
            CFG.Current.MapEditor_Selection_Movement_IncrementType = 0;
        }
    }

    public static void Shortcuts()
    {
        if (InputTracker.GetKey(KeyBindings.Current.MAP_KeyboardMove_CycleIncrement))
        {
            // Only apply in Map Editor screen
            if (Smithbox.EditorHandler.FocusedEditor is MapEditorScreen)
            {
                if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_KeyboardMove_CycleIncrement))
                {
                    CycleIncrementType();
                }
            }
        }

        var x_increment = CFG.Current.MapEditor_Selection_Movement_Increment_0;
        var y_increment = CFG.Current.MapEditor_Selection_Movement_Increment_0;
        var z_increment = CFG.Current.MapEditor_Selection_Movement_Increment_0;

        switch(CFG.Current.MapEditor_Selection_Movement_IncrementType)
        {
            case 0:
                x_increment = CFG.Current.MapEditor_Selection_Movement_Increment_0;
                y_increment = CFG.Current.MapEditor_Selection_Movement_Increment_0;
                z_increment = CFG.Current.MapEditor_Selection_Movement_Increment_0;
                break;
            case 1:
                x_increment = CFG.Current.MapEditor_Selection_Movement_Increment_1;
                y_increment = CFG.Current.MapEditor_Selection_Movement_Increment_1;
                z_increment = CFG.Current.MapEditor_Selection_Movement_Increment_1;
                break;
            case 2:
                x_increment = CFG.Current.MapEditor_Selection_Movement_Increment_2;
                y_increment = CFG.Current.MapEditor_Selection_Movement_Increment_2;
                z_increment = CFG.Current.MapEditor_Selection_Movement_Increment_2;
                break;
            case 3:
                x_increment = CFG.Current.MapEditor_Selection_Movement_Increment_3;
                y_increment = CFG.Current.MapEditor_Selection_Movement_Increment_3;
                z_increment = CFG.Current.MapEditor_Selection_Movement_Increment_3;
                break;
            case 4:
                x_increment = CFG.Current.MapEditor_Selection_Movement_Increment_4;
                y_increment = CFG.Current.MapEditor_Selection_Movement_Increment_4;
                z_increment = CFG.Current.MapEditor_Selection_Movement_Increment_4;
                break;
        }

        List<ViewportAction> actlist = new();
        HashSet<Entity> sels = Smithbox.EditorHandler.MapEditor._selection.GetFilteredSelection<Entity>(o => o.HasTransform);

        foreach (Entity sel in sels)
        {
            bool xMovement_Positive = false;
            bool xMovement_Negative = false;
            bool yMovement_Positive = false;
            bool yMovement_Negative = false;
            bool zMovement_Positive = false;
            bool zMovement_Negative = false;

            // TODO: Determine 'direction' based on camera position, e.g. if to the side of element, switch x and y
            if (InputTracker.GetKey(KeyBindings.Current.MAP_KeyboardMove_PositiveX))
            {
                xMovement_Positive = true;
            }
            if (InputTracker.GetKey(KeyBindings.Current.MAP_KeyboardMove_NegativeX))
            {
                xMovement_Negative = true;
            }
            if (InputTracker.GetKey(KeyBindings.Current.MAP_KeyboardMove_PositiveY))
            {
                yMovement_Positive = true;
            }
            if (InputTracker.GetKey(KeyBindings.Current.MAP_KeyboardMove_NegativeY))
            {
                yMovement_Negative = true;
            }
            if (InputTracker.GetKey(KeyBindings.Current.MAP_KeyboardMove_PositiveZ))
            {
                zMovement_Positive = true;
            }
            if (InputTracker.GetKey(KeyBindings.Current.MAP_KeyboardMove_NegativeZ))
            {
                zMovement_Negative = true;
            }

            Transform localT = sel.GetLocalTransform();

            // X
            if (xMovement_Positive)
            {
                var position = (Vector3)sel.GetPropertyValue("Position");
                var newPosition = new Vector3(position.X + x_increment, position.Y, position.Z);
                Transform newT = new(newPosition, localT.EulerRotation);
                actlist.Add(sel.GetUpdateTransformAction(newT));
            }
            if (xMovement_Negative)
            {
                var position = (Vector3)sel.GetPropertyValue("Position");
                var newPosition = new Vector3(position.X - x_increment, position.Y, position.Z);
                Transform newT = new(newPosition, localT.EulerRotation);
                actlist.Add(sel.GetUpdateTransformAction(newT));
            }

            // Y
            if (yMovement_Positive)
            {
                var position = (Vector3)sel.GetPropertyValue("Position");
                var newPosition = new Vector3(position.X, position.Y + y_increment, position.Z);
                Transform newT = new(newPosition, localT.EulerRotation);
                actlist.Add(sel.GetUpdateTransformAction(newT));
            }
            if (yMovement_Negative)
            {
                var position = (Vector3)sel.GetPropertyValue("Position");
                var newPosition = new Vector3(position.X, position.Y - y_increment, position.Z);
                Transform newT = new(newPosition, localT.EulerRotation);
                actlist.Add(sel.GetUpdateTransformAction(newT));
            }

            // Z
            if (zMovement_Positive)
            {
                var position = (Vector3)sel.GetPropertyValue("Position");
                var newPosition = new Vector3(position.X, position.Y, position.Z + z_increment);
                Transform newT = new(newPosition, localT.EulerRotation);
                actlist.Add(sel.GetUpdateTransformAction(newT));
            }
            if (zMovement_Negative)
            {
                var position = (Vector3)sel.GetPropertyValue("Position");
                var newPosition = new Vector3(position.X, position.Y, position.Z - z_increment);
                Transform newT = new(newPosition, localT.EulerRotation);
                actlist.Add(sel.GetUpdateTransformAction(newT));
            }
        }

        if (actlist.Any())
        {
            CompoundAction action = new(actlist);
            Smithbox.EditorHandler.MapEditor.EditorActionManager.ExecuteAction(action);
        }
    }

    public static void DisplayViewportMovementIncrement()
    {
        switch (CFG.Current.MapEditor_Selection_Movement_IncrementType)
        {
            case 0:
                ImguiUtils.WrappedTextColored(CFG.Current.ImGui_AliasName_Text, $"Movement Increment: {CFG.Current.MapEditor_Selection_Movement_Increment_0}");
                break;
            case 1:
                ImguiUtils.WrappedTextColored(CFG.Current.ImGui_AliasName_Text, $"Movement Increment: {CFG.Current.MapEditor_Selection_Movement_Increment_1}");
                break;
            case 2:
                ImguiUtils.WrappedTextColored(CFG.Current.ImGui_AliasName_Text, $"Movement Increment: {CFG.Current.MapEditor_Selection_Movement_Increment_2}");
                break;
            case 3:
                ImguiUtils.WrappedTextColored(CFG.Current.ImGui_AliasName_Text, $"Movement Increment: {CFG.Current.MapEditor_Selection_Movement_Increment_3}");
                break;
            case 4:
                ImguiUtils.WrappedTextColored(CFG.Current.ImGui_AliasName_Text, $"Movement Increment: {CFG.Current.MapEditor_Selection_Movement_Increment_4}");
                break;
        }
        ImguiUtils.ShowHoverTooltip($"Press {KeyBindings.Current.MAP_KeyboardMove_CycleIncrement.HintText} to cycle the movement increment used when moving a selection via Keyboard Move.");
    }

    public static void DisplayCurrentMovementIncrement()
    {
        switch (CFG.Current.MapEditor_Selection_Movement_IncrementType)
        {
            case 0:
                ImguiUtils.WrappedTextColored(CFG.Current.ImGui_AliasName_Text, $"Movement Increment [0]: {CFG.Current.MapEditor_Selection_Movement_Increment_0}");
                break;
            case 1:
                ImguiUtils.WrappedTextColored(CFG.Current.ImGui_AliasName_Text, $"Movement Increment [1]: {CFG.Current.MapEditor_Selection_Movement_Increment_1}");
                break;
            case 2:
                ImguiUtils.WrappedTextColored(CFG.Current.ImGui_AliasName_Text, $"Movement Increment [2]: {CFG.Current.MapEditor_Selection_Movement_Increment_2}");
                break;
            case 3:
                ImguiUtils.WrappedTextColored(CFG.Current.ImGui_AliasName_Text, $"Movement Increment [3]: {CFG.Current.MapEditor_Selection_Movement_Increment_3}");
                break;
            case 4:
                ImguiUtils.WrappedTextColored(CFG.Current.ImGui_AliasName_Text, $"Movement Increment [4]: {CFG.Current.MapEditor_Selection_Movement_Increment_4}");
                break;
        }
        ImguiUtils.ShowHoverTooltip($"Press {KeyBindings.Current.MAP_KeyboardMove_CycleIncrement.HintText} to cycle the movement increment used when moving a selection via Keyboard Move.");
    }
}
