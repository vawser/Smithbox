using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System;
using System.Numerics;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
///     Rotates the loaded model in the viewport only. The rotation is held by the model's container, never by the
///     FLVER, so it can't be saved. It stays set when another model is loaded, so models can be compared at the
///     same angle.
/// </summary>
public class ModelDisplayRotationTool
{
    public ModelEditorView View;
    public ProjectEntry Project;

    // Degrees. Applied as roll (Z), then pitch (X), then yaw (Y), so the turntable spins about the vertical axis.
    private Vector3 _rotation = Vector3.Zero;

    private bool _spin = false;
    private float _spinSpeed = 45.0f;

    public ModelDisplayRotationTool(ModelEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public Quaternion Rotation => Quaternion.CreateFromYawPitchRoll(
        Utils.DegToRadians(_rotation.Y),
        Utils.DegToRadians(_rotation.X),
        Utils.DegToRadians(_rotation.Z));

    /// <summary>
    ///     Advances the turntable and applies the rotation to the loaded model. Runs every frame, so it keeps
    ///     going while the tool isn't shown.
    /// </summary>
    public void Update()
    {
        if (_spin)
        {
            _rotation.Y = WrapAngle(_rotation.Y + _spinSpeed * ImGui.GetIO().DeltaTime);
        }

        View.Selection.SelectedModelWrapper?.Container?.SetDisplayRotation(Rotation);
    }

    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("MODEL_DisplayRotation_Header")}##modelDisplayRotationHeader"))
            return;

        ImGui.BeginChild("ModelDisplayRotationToolSection", ImGuiChildFlags.Borders);

        Display();

        ImGui.EndChild();
    }

    private void Display()
    {
        GUI.WrappedText(LOC.Get("MODEL_DisplayRotation_Hint"));

        if (View.Selection.SelectedModelWrapper?.Container == null)
        {
            GUI.Spacer();
            GUI.WrappedText(LOC.Get("MODEL_DisplayRotation_No_Model"));
            return;
        }

        // Rotation
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("MODEL_DisplayRotation_Rotation_Header"),
            LOC.Get("MODEL_DisplayRotation_Rotation_Header_TT"));

        var rotation = _rotation;

        ImGui.SetNextItemWidth(-1);
        if (ImGui.DragFloat3("##modelDisplayRotationInput", ref rotation, 0.5f, -180.0f, 180.0f, "%.1f°"))
        {
            _rotation = new Vector3(WrapAngle(rotation.X), WrapAngle(rotation.Y), WrapAngle(rotation.Z));
        }
        GUI.Tooltip(LOC.Get("MODEL_DisplayRotation_Rotation_Input_TT"));

        // Quick Rotate
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("MODEL_DisplayRotation_Quick_Header"),
            LOC.Get("MODEL_DisplayRotation_Quick_Header_TT"));

        GUI.MultiButtonInput("modelDisplayRotationQuick",
            "modelDisplayRotationQuickX", LOC.Get("MODEL_DisplayRotation_Quick_X"),
            LOC.Get("MODEL_DisplayRotation_Quick_X_TT"), () => _rotation.X = WrapAngle(_rotation.X + 90.0f),

            "modelDisplayRotationQuickY", LOC.Get("MODEL_DisplayRotation_Quick_Y"),
            LOC.Get("MODEL_DisplayRotation_Quick_Y_TT"), () => _rotation.Y = WrapAngle(_rotation.Y + 90.0f),

            "modelDisplayRotationQuickZ", LOC.Get("MODEL_DisplayRotation_Quick_Z"),
            LOC.Get("MODEL_DisplayRotation_Quick_Z_TT"), () => _rotation.Z = WrapAngle(_rotation.Z + 90.0f));

        // Turntable
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("MODEL_DisplayRotation_Turntable_Header"),
            LOC.Get("MODEL_DisplayRotation_Turntable_Header_TT"));

        ImGui.Checkbox($"{LOC.Get("MODEL_DisplayRotation_Spin")}##modelDisplayRotationSpin", ref _spin);
        GUI.Tooltip(LOC.Get("MODEL_DisplayRotation_Spin_TT"));

        ImGui.SetNextItemWidth(-1);
        ImGui.SliderFloat("##modelDisplayRotationSpinSpeed", ref _spinSpeed, -360.0f, 360.0f, "%.0f°/s");
        GUI.Tooltip(LOC.Get("MODEL_DisplayRotation_Spin_Speed_TT"));

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("MODEL_DisplayRotation_Actions_Header"),
            LOC.Get("MODEL_DisplayRotation_Actions_Header_TT"));

        GUI.MultiButtonInput("modelDisplayRotationActions",
            "modelDisplayRotationReset", LOC.Get("MODEL_DisplayRotation_Reset"),
            LOC.Get("MODEL_DisplayRotation_Reset_TT"), Reset);
    }

    private void Reset()
    {
        _rotation = Vector3.Zero;
        _spin = false;
    }

    // Keeps an angle in [-180, 180)
    private static float WrapAngle(float degrees)
    {
        var wrapped = (degrees + 180.0f) % 360.0f;

        if (wrapped < 0)
        {
            wrapped += 360.0f;
        }

        return wrapped - 180.0f;
    }
}
