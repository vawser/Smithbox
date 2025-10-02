using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.MapEditor.Framework;
using StudioCore.Interface;
using StudioCore.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Tools;

public class ScrambleAction
{
    public MapEditorScreen Editor;
    public ProjectEntry Project;

    public ScrambleAction(MapEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    /// <summary>
    /// Shortcut
    /// </summary>
    public void OnShortcut()
    {
        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_ScrambleSelection) && Editor.ViewportSelection.IsSelection())
        {
            ApplyScramble();
        }
    }

    /// <summary>
    /// Context Menu
    /// </summary>
    public void OnContext(Entity ent)
    {
        if (ent.WrappedObject is IMsbPart or IMsbRegion or BTL.Light)
        {
            if (ImGui.Selectable("Scramble"))
            {
                ApplyScramble();
            }
            UIHelper.Tooltip($"Apply the scramble configuration to the currently selected map objects.\n\nShortcut: {KeyBindings.Current.MAP_ScrambleSelection.HintText}");
        }
    }

    /// <summary>
    /// Edit Menu
    /// </summary>
    public void OnMenu()
    {
        if (ImGui.MenuItem("Scramble", KeyBindings.Current.MAP_ScrambleSelection.HintText))
        {
            ApplyScramble();
        }
        UIHelper.Tooltip($"Apply the scramble configuration to the currently selected map objects.");
    }

    /// <summary>
    /// Tool Window
    /// </summary>
    public void OnToolWindow()
    {
        var windowWidth = ImGui.GetWindowWidth();

        if (ImGui.CollapsingHeader("Scramble"))
        {
            var randomOffsetMin_Pos_X = CFG.Current.Scrambler_OffsetMin_Position_X;
            var randomOffsetMin_Pos_Y = CFG.Current.Scrambler_OffsetMin_Position_Y;
            var randomOffsetMin_Pos_Z = CFG.Current.Scrambler_OffsetMin_Position_Z;

            var randomOffsetMax_Pos_X = CFG.Current.Scrambler_OffsetMax_Position_X;
            var randomOffsetMax_Pos_Y = CFG.Current.Scrambler_OffsetMax_Position_Y;
            var randomOffsetMax_Pos_Z = CFG.Current.Scrambler_OffsetMax_Position_Z;

            var randomOffsetMin_Rot_X = CFG.Current.Scrambler_OffsetMin_Rotation_X;
            var randomOffsetMin_Rot_Y = CFG.Current.Scrambler_OffsetMin_Rotation_Y;
            var randomOffsetMin_Rot_Z = CFG.Current.Scrambler_OffsetMin_Rotation_Z;

            var randomOffsetMax_Rot_X = CFG.Current.Scrambler_OffsetMax_Rotation_X;
            var randomOffsetMax_Rot_Y = CFG.Current.Scrambler_OffsetMax_Rotation_Y;
            var randomOffsetMax_Rot_Z = CFG.Current.Scrambler_OffsetMax_Rotation_Z;

            var randomOffsetMin_Scale_X = CFG.Current.Scrambler_OffsetMin_Scale_X;
            var randomOffsetMin_Scale_Y = CFG.Current.Scrambler_OffsetMin_Scale_Y;
            var randomOffsetMin_Scale_Z = CFG.Current.Scrambler_OffsetMin_Scale_Z;

            var randomOffsetMax_Scale_X = CFG.Current.Scrambler_OffsetMax_Scale_X;
            var randomOffsetMax_Scale_Y = CFG.Current.Scrambler_OffsetMax_Scale_Y;
            var randomOffsetMax_Scale_Z = CFG.Current.Scrambler_OffsetMax_Scale_Z;

            // Position
            UIHelper.SimpleHeader("Position", "Position", "", UI.Current.ImGui_Default_Text_Color);

            ImGui.Checkbox("X##scramblePosX", ref CFG.Current.Scrambler_RandomisePosition_X);
            UIHelper.Tooltip("Include the X co-ordinate of the selection's Position in the scramble.");

            ImGui.SameLine();
            DPI.ApplyInputWidth(100f);
            ImGui.InputFloat("##offsetMinPosX", ref randomOffsetMin_Pos_X);
            UIHelper.Tooltip("Minimum amount to add to the position X co-ordinate.");

            ImGui.SameLine();

            DPI.ApplyInputWidth(100f);
            ImGui.InputFloat("##offsetMaxPosX", ref randomOffsetMax_Pos_X);
            UIHelper.Tooltip("Maximum amount to add to the position X co-ordinate.");

            ImGui.Checkbox("Y##scramblePosY", ref CFG.Current.Scrambler_RandomisePosition_Y);
            UIHelper.Tooltip("Include the Y co-ordinate of the selection's Position in the scramble.");

            ImGui.SameLine();
            DPI.ApplyInputWidth(100f);
            ImGui.InputFloat("##offsetMinPosY", ref randomOffsetMin_Pos_Y);
            UIHelper.Tooltip("Minimum amount to add to the position Y co-ordinate.");

            ImGui.SameLine();
            DPI.ApplyInputWidth(100f);
            ImGui.InputFloat("##offsetMaxPosY", ref randomOffsetMax_Pos_Y);
            UIHelper.Tooltip("Maximum amount to add to the position Y co-ordinate.");

            ImGui.Checkbox("Z##scramblePosZ", ref CFG.Current.Scrambler_RandomisePosition_Z);
            UIHelper.Tooltip("Include the Z co-ordinate of the selection's Position in the scramble.");

            ImGui.SameLine();
            DPI.ApplyInputWidth(100f);
            ImGui.InputFloat("##offsetMinPosZ", ref randomOffsetMin_Pos_Z);
            UIHelper.Tooltip("Minimum amount to add to the position Z co-ordinate.");

            ImGui.SameLine();
            DPI.ApplyInputWidth(100f);
            ImGui.InputFloat("##offsetMaxPosZ", ref randomOffsetMax_Pos_Z);
            UIHelper.Tooltip("Maximum amount to add to the position Z co-ordinate.");
            ImGui.Text("");

            // Rotation
            UIHelper.SimpleHeader("Rotation", "Rotation", "", UI.Current.ImGui_Default_Text_Color);

            ImGui.Checkbox("X##scrambleRotX", ref CFG.Current.Scrambler_RandomiseRotation_X);
            UIHelper.Tooltip("Include the X co-ordinate of the selection's Rotation in the scramble.");

            ImGui.SameLine();
            DPI.ApplyInputWidth(100f);
            ImGui.InputFloat("##offsetMinRotX", ref randomOffsetMin_Rot_X);
            UIHelper.Tooltip("Minimum amount to add to the rotation X co-ordinate.");

            ImGui.SameLine();
            DPI.ApplyInputWidth(100f);
            ImGui.InputFloat("##offsetMaxRotX", ref randomOffsetMax_Rot_X);
            UIHelper.Tooltip("Maximum amount to add to the rotation X co-ordinate.");

            ImGui.Checkbox("Y##scrambleRotY", ref CFG.Current.Scrambler_RandomiseRotation_Y);
            UIHelper.Tooltip("Include the Y co-ordinate of the selection's Rotation in the scramble.");

            ImGui.SameLine();
            DPI.ApplyInputWidth(100f);
            ImGui.InputFloat("##offsetMinRotY", ref randomOffsetMin_Rot_Y);
            UIHelper.Tooltip("Minimum amount to add to the rotation Y co-ordinate.");

            ImGui.SameLine();
            DPI.ApplyInputWidth(100f);
            ImGui.InputFloat("##offsetMaxRotY", ref randomOffsetMax_Rot_Y);
            UIHelper.Tooltip("Maximum amount to add to the rotation Y co-ordinate.");

            ImGui.Checkbox("Z##scrambleRotZ", ref CFG.Current.Scrambler_RandomiseRotation_Z);
            UIHelper.Tooltip("Include the Z co-ordinate of the selection's Rotation in the scramble.");

            ImGui.SameLine();
            DPI.ApplyInputWidth(100f);
            ImGui.InputFloat("##offsetMinRotZ", ref randomOffsetMin_Rot_Z);
            UIHelper.Tooltip("Minimum amount to add to the rotation Z co-ordinate.");

            ImGui.SameLine();
            DPI.ApplyInputWidth(100f);
            ImGui.InputFloat("##offsetMaxRotZ", ref randomOffsetMax_Rot_Z);
            UIHelper.Tooltip("Maximum amount to add to the rotation Z co-ordinate.");
            ImGui.Text("");

            // Scale
            UIHelper.SimpleHeader("Scale", "Scale", "", UI.Current.ImGui_Default_Text_Color);

            ImGui.Checkbox("X##scrambleScaleX", ref CFG.Current.Scrambler_RandomiseScale_X);
            UIHelper.Tooltip("Include the X co-ordinate of the selection's Scale in the scramble.");

            ImGui.SameLine();
            DPI.ApplyInputWidth(100f);
            ImGui.InputFloat("##offsetMinScaleX", ref randomOffsetMin_Scale_X);
            UIHelper.Tooltip("Minimum amount to add to the scale X co-ordinate.");

            ImGui.SameLine();
            DPI.ApplyInputWidth(100f);
            ImGui.InputFloat("##offsetMaxScaleX", ref randomOffsetMax_Scale_X);
            UIHelper.Tooltip("Maximum amount to add to the scale X co-ordinate.");

            ImGui.Checkbox("Y##scrambleScaleY", ref CFG.Current.Scrambler_RandomiseScale_Y);
            UIHelper.Tooltip("Include the Y co-ordinate of the selection's Scale in the scramble.");

            ImGui.SameLine();
            DPI.ApplyInputWidth(100f);
            ImGui.InputFloat("##offsetMinScaleY", ref randomOffsetMin_Scale_Y);
            UIHelper.Tooltip("Minimum amount to add to the scale Y co-ordinate.");

            ImGui.SameLine();
            DPI.ApplyInputWidth(100f);
            ImGui.InputFloat("##offsetMaxScaleY", ref randomOffsetMax_Scale_Y);
            UIHelper.Tooltip("Maximum amount to add to the scale Y co-ordinate.");

            ImGui.Checkbox("Z##scrambleScaleZ", ref CFG.Current.Scrambler_RandomiseScale_Z);
            UIHelper.Tooltip("Include the Z co-ordinate of the selection's Scale in the scramble.");

            ImGui.SameLine();
            DPI.ApplyInputWidth(100f);
            ImGui.InputFloat("##offsetMinScaleZ", ref randomOffsetMin_Scale_Z);
            UIHelper.Tooltip("Minimum amount to add to the scale Z co-ordinate.");

            ImGui.SameLine();
            DPI.ApplyInputWidth(100f);
            ImGui.InputFloat("##offsetMaxScaleZ", ref randomOffsetMax_Scale_Y);
            UIHelper.Tooltip("Maximum amount to add to the scale Z co-ordinate.");
            UIHelper.WrappedText("");

            ImGui.Checkbox("Scale Proportionally##scrambleSharedScale", ref CFG.Current.Scrambler_RandomiseScale_SharedScale);
            UIHelper.Tooltip("When scrambling the scale, the Y and Z values will follow the X value, making the scaling proportional.");
            UIHelper.WrappedText("");

            // Clamp floats
            randomOffsetMin_Pos_X = Math.Clamp(randomOffsetMin_Pos_X, -10000f, 10000f);
            randomOffsetMin_Pos_Y = Math.Clamp(randomOffsetMin_Pos_Y, -10000f, 10000f);
            randomOffsetMin_Pos_Z = Math.Clamp(randomOffsetMin_Pos_Z, -10000f, 10000f);

            randomOffsetMax_Pos_X = Math.Clamp(randomOffsetMax_Pos_X, -10000f, 10000f);
            randomOffsetMax_Pos_Y = Math.Clamp(randomOffsetMax_Pos_Y, -10000f, 10000f);
            randomOffsetMax_Pos_Z = Math.Clamp(randomOffsetMax_Pos_Z, -10000f, 10000f);

            randomOffsetMin_Rot_X = Math.Clamp(randomOffsetMin_Rot_X, 0.0f, 360f);
            randomOffsetMin_Rot_Y = Math.Clamp(randomOffsetMin_Rot_Y, 0.0f, 360f);
            randomOffsetMin_Rot_Z = Math.Clamp(randomOffsetMin_Rot_Z, 0.0f, 360f);

            randomOffsetMax_Rot_X = Math.Clamp(randomOffsetMax_Rot_X, 0.0f, 360f);
            randomOffsetMax_Rot_Y = Math.Clamp(randomOffsetMax_Rot_Y, 0.0f, 360f);
            randomOffsetMax_Rot_Z = Math.Clamp(randomOffsetMax_Rot_Z, 0.0f, 360f);

            randomOffsetMin_Scale_X = Math.Clamp(randomOffsetMin_Scale_X, 0.0f, 100f);
            randomOffsetMin_Scale_Y = Math.Clamp(randomOffsetMin_Scale_Y, 0.0f, 100f);
            randomOffsetMin_Scale_Z = Math.Clamp(randomOffsetMin_Scale_Z, 0.0f, 100f);

            randomOffsetMax_Scale_X = Math.Clamp(randomOffsetMax_Scale_X, 0.0f, 100f);
            randomOffsetMax_Scale_Y = Math.Clamp(randomOffsetMax_Scale_Y, 0.0f, 100f);
            randomOffsetMax_Scale_Z = Math.Clamp(randomOffsetMax_Scale_Z, 0.0f, 100f);

            CFG.Current.Scrambler_OffsetMin_Position_X = randomOffsetMin_Pos_X;
            CFG.Current.Scrambler_OffsetMin_Position_Y = randomOffsetMin_Pos_Y;
            CFG.Current.Scrambler_OffsetMin_Position_Z = randomOffsetMin_Pos_Z;

            CFG.Current.Scrambler_OffsetMax_Position_X = randomOffsetMax_Pos_X;
            CFG.Current.Scrambler_OffsetMax_Position_Y = randomOffsetMax_Pos_Y;
            CFG.Current.Scrambler_OffsetMax_Position_Z = randomOffsetMax_Pos_Z;

            CFG.Current.Scrambler_OffsetMin_Rotation_X = randomOffsetMin_Rot_X;
            CFG.Current.Scrambler_OffsetMin_Rotation_Y = randomOffsetMin_Rot_Y;
            CFG.Current.Scrambler_OffsetMin_Rotation_Z = randomOffsetMin_Rot_Z;

            CFG.Current.Scrambler_OffsetMax_Rotation_X = randomOffsetMax_Rot_X;
            CFG.Current.Scrambler_OffsetMax_Rotation_Y = randomOffsetMax_Rot_Y;
            CFG.Current.Scrambler_OffsetMax_Rotation_Z = randomOffsetMax_Rot_Z;

            CFG.Current.Scrambler_OffsetMin_Scale_X = randomOffsetMin_Scale_X;
            CFG.Current.Scrambler_OffsetMin_Scale_Y = randomOffsetMin_Scale_Y;
            CFG.Current.Scrambler_OffsetMin_Scale_Z = randomOffsetMin_Scale_Z;

            CFG.Current.Scrambler_OffsetMax_Scale_X = randomOffsetMax_Scale_X;
            CFG.Current.Scrambler_OffsetMax_Scale_Y = randomOffsetMax_Scale_Y;
            CFG.Current.Scrambler_OffsetMax_Scale_Z = randomOffsetMax_Scale_Z;

            if (ImGui.Button("Scramble Selection", DPI.WholeWidthButton(windowWidth, 24)))
            {
                ApplyScramble();
            }
        }
    }

    /// <summary>
    /// Effect
    /// </summary>
    public void ApplyScramble()
    {
        if (Editor.ViewportSelection.IsSelection())
        {
            List<ViewportAction> actlist = new();
            foreach (Entity sel in Editor.ViewportSelection.GetFilteredSelection<Entity>(o => o.HasTransform))
            {
                sel.ClearTemporaryTransform(Editor, false);
                actlist.Add(sel.GetUpdateTransformAction(GetScrambledTransform(sel), true));
            }

            Actions.Viewport.CompoundAction action = new(actlist);
            Editor.EditorActionManager.ExecuteAction(action);
        }
        else
        {
            PlatformUtils.Instance.MessageBox("No object selected.", "Smithbox", MessageBoxButtons.OK);
        }
    }


    public Transform GetScrambledTransform(Entity sel)
    {
        float posOffset_X = (float)GetRandomNumber(CFG.Current.Scrambler_OffsetMin_Position_X, CFG.Current.Scrambler_OffsetMax_Position_X);
        float posOffset_Y = (float)GetRandomNumber(CFG.Current.Scrambler_OffsetMin_Position_Y, CFG.Current.Scrambler_OffsetMax_Position_Y);
        float posOffset_Z = (float)GetRandomNumber(CFG.Current.Scrambler_OffsetMin_Position_Z, CFG.Current.Scrambler_OffsetMax_Position_Z);

        float rotOffset_X = (float)GetRandomNumber(CFG.Current.Scrambler_OffsetMin_Rotation_X, CFG.Current.Scrambler_OffsetMax_Rotation_X);
        float rotOffset_Y = (float)GetRandomNumber(CFG.Current.Scrambler_OffsetMin_Rotation_Y, CFG.Current.Scrambler_OffsetMax_Rotation_Y);
        float rotOffset_Z = (float)GetRandomNumber(CFG.Current.Scrambler_OffsetMin_Rotation_Z, CFG.Current.Scrambler_OffsetMax_Rotation_Z);

        float scaleOffset_X = (float)GetRandomNumber(CFG.Current.Scrambler_OffsetMin_Scale_X, CFG.Current.Scrambler_OffsetMax_Scale_X);
        float scaleOffset_Y = (float)GetRandomNumber(CFG.Current.Scrambler_OffsetMin_Scale_Y, CFG.Current.Scrambler_OffsetMax_Scale_Y);
        float scaleOffset_Z = (float)GetRandomNumber(CFG.Current.Scrambler_OffsetMin_Scale_Z, CFG.Current.Scrambler_OffsetMax_Scale_Z);

        Transform objT = sel.GetLocalTransform();

        var newTransform = Transform.Default;

        var radianRotateAmount = 0.0f;
        var rot_x = objT.EulerRotation.X;
        var rot_y = objT.EulerRotation.Y;
        var rot_z = objT.EulerRotation.Z;

        var newPos = objT.Position;
        var newRot = objT.Rotation;
        var newScale = objT.Scale;

        if (CFG.Current.Scrambler_RandomisePosition_X)
        {
            newPos = new Vector3(newPos[0] + posOffset_X, newPos[1], newPos[2]);
        }
        if (CFG.Current.Scrambler_RandomisePosition_Y)
        {
            newPos = new Vector3(newPos[0], newPos[1] + posOffset_Y, newPos[2]);
        }
        if (CFG.Current.Scrambler_RandomisePosition_Z)
        {
            newPos = new Vector3(newPos[0], newPos[1], newPos[2] + posOffset_Z);
        }

        newTransform.Position = newPos;

        if (CFG.Current.Scrambler_RandomiseRotation_X)
        {
            radianRotateAmount = (float)Math.PI / 180 * rotOffset_X;
            rot_x = objT.EulerRotation.X + radianRotateAmount;
        }
        if (CFG.Current.Scrambler_RandomiseRotation_Y)
        {
            radianRotateAmount = (float)Math.PI / 180 * rotOffset_Y;
            rot_y = objT.EulerRotation.Y + radianRotateAmount;
        }
        if (CFG.Current.Scrambler_RandomiseRotation_Z)
        {
            radianRotateAmount = (float)Math.PI / 180 * rotOffset_Z;
            rot_z = objT.EulerRotation.Z + radianRotateAmount;
        }

        if (CFG.Current.Scrambler_RandomiseRotation_X || CFG.Current.Scrambler_RandomiseRotation_Y || CFG.Current.Scrambler_RandomiseRotation_Z)
        {
            newTransform.EulerRotation = new Vector3(rot_x, rot_y, rot_z);
        }
        else
        {
            newTransform.Rotation = newRot;
        }

        // If shared scale, the scale randomisation will be the same for X, Y, Z
        if (CFG.Current.Scrambler_RandomiseScale_SharedScale)
        {
            scaleOffset_Y = scaleOffset_X;
            scaleOffset_Z = scaleOffset_X;
        }

        if (CFG.Current.Scrambler_RandomiseScale_X)
        {
            newScale = new Vector3(scaleOffset_X, newScale[1], newScale[2]);
        }
        if (CFG.Current.Scrambler_RandomiseScale_Y)
        {
            newScale = new Vector3(newScale[0], scaleOffset_Y, newScale[2]);
        }
        if (CFG.Current.Scrambler_RandomiseScale_Z)
        {
            newScale = new Vector3(newScale[0], newScale[1], scaleOffset_Z);
        }

        newTransform.Scale = newScale;

        return newTransform;
    }
    public double GetRandomNumber(double minimum, double maximum)
    {
        Random random = new Random();
        return random.NextDouble() * (maximum - minimum) + minimum;
    }
}
