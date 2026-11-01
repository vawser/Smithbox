using StudioCore.Application;
using System;
using System.Numerics;

namespace StudioCore.Editors.Viewport;

public enum ViewportGridEditor
{
    Map,
    Model,
    TimeAct
}

public enum ViewportGridType
{
    Primary,
    Secondary,
    Tertiary
}

/// <summary>
/// The settings of one viewport grid. Each field refers directly to its value in <see cref="CFG" />.
/// </summary>
public ref struct ViewportGridConfig
{
    public ref bool Display;
    public ref int Size;
    public ref float SectionSize;
    public ref Vector3 Color;
    public ref float PositionX;
    public ref float PositionY;
    public ref float PositionZ;
    public ref float RotationX;
    public ref float RotationY;
    public ref float RotationZ;

    private ViewportGridConfig(
        ref bool display,
        ref int size,
        ref float sectionSize,
        ref Vector3 color,
        ref float positionX,
        ref float positionY,
        ref float positionZ,
        ref float rotationX,
        ref float rotationY,
        ref float rotationZ)
    {
        Display = ref display;
        Size = ref size;
        SectionSize = ref sectionSize;
        Color = ref color;
        PositionX = ref positionX;
        PositionY = ref positionY;
        PositionZ = ref positionZ;
        RotationX = ref rotationX;
        RotationY = ref rotationY;
        RotationZ = ref rotationZ;
    }

    public static ViewportGridConfig Get(ViewportGridEditor editor, ViewportGridType type)
    {
        var cfg = CFG.Current;

        switch (editor, type)
        {
            case (ViewportGridEditor.Map, ViewportGridType.Primary):
                return new(
                    ref cfg.MapEditor_DisplayPrimaryGrid,
                    ref cfg.MapEditor_PrimaryGrid_Size,
                    ref cfg.MapEditor_PrimaryGrid_SectionSize,
                    ref cfg.MapEditor_PrimaryGrid_Color,
                    ref cfg.MapEditor_PrimaryGrid_Position_X,
                    ref cfg.MapEditor_PrimaryGrid_Position_Y,
                    ref cfg.MapEditor_PrimaryGrid_Position_Z,
                    ref cfg.MapEditor_PrimaryGrid_Rotation_X,
                    ref cfg.MapEditor_PrimaryGrid_Rotation_Y,
                    ref cfg.MapEditor_PrimaryGrid_Rotation_Z);

            case (ViewportGridEditor.Map, ViewportGridType.Secondary):
                return new(
                    ref cfg.MapEditor_DisplaySecondaryGrid,
                    ref cfg.MapEditor_SecondaryGrid_Size,
                    ref cfg.MapEditor_SecondaryGrid_SectionSize,
                    ref cfg.MapEditor_SecondaryGrid_Color,
                    ref cfg.MapEditor_SecondaryGrid_Position_X,
                    ref cfg.MapEditor_SecondaryGrid_Position_Y,
                    ref cfg.MapEditor_SecondaryGrid_Position_Z,
                    ref cfg.MapEditor_SecondaryGrid_Rotation_X,
                    ref cfg.MapEditor_SecondaryGrid_Rotation_Y,
                    ref cfg.MapEditor_SecondaryGrid_Rotation_Z);

            case (ViewportGridEditor.Map, ViewportGridType.Tertiary):
                return new(
                    ref cfg.MapEditor_DisplayTertiaryGrid,
                    ref cfg.MapEditor_TertiaryGrid_Size,
                    ref cfg.MapEditor_TertiaryGrid_SectionSize,
                    ref cfg.MapEditor_TertiaryGrid_Color,
                    ref cfg.MapEditor_TertiaryGrid_Position_X,
                    ref cfg.MapEditor_TertiaryGrid_Position_Y,
                    ref cfg.MapEditor_TertiaryGrid_Position_Z,
                    ref cfg.MapEditor_TertiaryGrid_Rotation_X,
                    ref cfg.MapEditor_TertiaryGrid_Rotation_Y,
                    ref cfg.MapEditor_TertiaryGrid_Rotation_Z);

            case (ViewportGridEditor.Model, ViewportGridType.Primary):
                return new(
                    ref cfg.ModelEditor_DisplayPrimaryGrid,
                    ref cfg.ModelEditor_PrimaryGrid_Size,
                    ref cfg.ModelEditor_PrimaryGrid_SectionSize,
                    ref cfg.ModelEditor_PrimaryGrid_Color,
                    ref cfg.ModelEditor_PrimaryGrid_Position_X,
                    ref cfg.ModelEditor_PrimaryGrid_Position_Y,
                    ref cfg.ModelEditor_PrimaryGrid_Position_Z,
                    ref cfg.ModelEditor_PrimaryGrid_Rotation_X,
                    ref cfg.ModelEditor_PrimaryGrid_Rotation_Y,
                    ref cfg.ModelEditor_PrimaryGrid_Rotation_Z);

            case (ViewportGridEditor.Model, ViewportGridType.Secondary):
                return new(
                    ref cfg.ModelEditor_DisplaySecondaryGrid,
                    ref cfg.ModelEditor_SecondaryGrid_Size,
                    ref cfg.ModelEditor_SecondaryGrid_SectionSize,
                    ref cfg.ModelEditor_SecondaryGrid_Color,
                    ref cfg.ModelEditor_SecondaryGrid_Position_X,
                    ref cfg.ModelEditor_SecondaryGrid_Position_Y,
                    ref cfg.ModelEditor_SecondaryGrid_Position_Z,
                    ref cfg.ModelEditor_SecondaryGrid_Rotation_X,
                    ref cfg.ModelEditor_SecondaryGrid_Rotation_Y,
                    ref cfg.ModelEditor_SecondaryGrid_Rotation_Z);

            case (ViewportGridEditor.Model, ViewportGridType.Tertiary):
                return new(
                    ref cfg.ModelEditor_DisplayTertiaryGrid,
                    ref cfg.ModelEditor_TertiaryGrid_Size,
                    ref cfg.ModelEditor_TertiaryGrid_SectionSize,
                    ref cfg.ModelEditor_TertiaryGrid_Color,
                    ref cfg.ModelEditor_TertiaryGrid_Position_X,
                    ref cfg.ModelEditor_TertiaryGrid_Position_Y,
                    ref cfg.ModelEditor_TertiaryGrid_Position_Z,
                    ref cfg.ModelEditor_TertiaryGrid_Rotation_X,
                    ref cfg.ModelEditor_TertiaryGrid_Rotation_Y,
                    ref cfg.ModelEditor_TertiaryGrid_Rotation_Z);

            case (ViewportGridEditor.TimeAct, ViewportGridType.Primary):
                return new(
                    ref cfg.ModelEditor_DisplayPrimaryGrid,
                    ref cfg.ModelEditor_PrimaryGrid_Size,
                    ref cfg.ModelEditor_PrimaryGrid_SectionSize,
                    ref cfg.ModelEditor_PrimaryGrid_Color,
                    ref cfg.ModelEditor_PrimaryGrid_Position_X,
                    ref cfg.ModelEditor_PrimaryGrid_Position_Y,
                    ref cfg.ModelEditor_PrimaryGrid_Position_Z,
                    ref cfg.ModelEditor_PrimaryGrid_Rotation_X,
                    ref cfg.ModelEditor_PrimaryGrid_Rotation_Y,
                    ref cfg.ModelEditor_PrimaryGrid_Rotation_Z);

            case (ViewportGridEditor.TimeAct, ViewportGridType.Secondary):
                return new(
                    ref cfg.ModelEditor_DisplaySecondaryGrid,
                    ref cfg.ModelEditor_SecondaryGrid_Size,
                    ref cfg.ModelEditor_SecondaryGrid_SectionSize,
                    ref cfg.ModelEditor_SecondaryGrid_Color,
                    ref cfg.ModelEditor_SecondaryGrid_Position_X,
                    ref cfg.ModelEditor_SecondaryGrid_Position_Y,
                    ref cfg.ModelEditor_SecondaryGrid_Position_Z,
                    ref cfg.ModelEditor_SecondaryGrid_Rotation_X,
                    ref cfg.ModelEditor_SecondaryGrid_Rotation_Y,
                    ref cfg.ModelEditor_SecondaryGrid_Rotation_Z);

            case (ViewportGridEditor.TimeAct, ViewportGridType.Tertiary):
                return new(
                    ref cfg.ModelEditor_DisplayTertiaryGrid,
                    ref cfg.ModelEditor_TertiaryGrid_Size,
                    ref cfg.ModelEditor_TertiaryGrid_SectionSize,
                    ref cfg.ModelEditor_TertiaryGrid_Color,
                    ref cfg.ModelEditor_TertiaryGrid_Position_X,
                    ref cfg.ModelEditor_TertiaryGrid_Position_Y,
                    ref cfg.ModelEditor_TertiaryGrid_Position_Z,
                    ref cfg.ModelEditor_TertiaryGrid_Rotation_X,
                    ref cfg.ModelEditor_TertiaryGrid_Rotation_Y,
                    ref cfg.ModelEditor_TertiaryGrid_Rotation_Z);

            default:
                throw new ArgumentOutOfRangeException(nameof(type));
        }
    }
}
