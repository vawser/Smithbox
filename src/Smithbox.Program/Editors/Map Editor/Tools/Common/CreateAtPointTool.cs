using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;
using StudioCore.Keybinds;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using Veldrid;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The Create at Point tab of the Create tool: creates a part or region with properties set up beforehand, at a point
/// clicked in the viewport. The first click places a marker on the surface under the cursor (found by the
/// click-picking pass, as the Ruler finds its points), and a second click on the marker creates the object there.
/// Clicking elsewhere moves the marker, so a quick double-click places and creates in one go.
/// </summary>
public class CreateAtPointTool
{
    private readonly MapEditorView View;
    private readonly ProjectEntry Project;
    private readonly CreateAction Owner;

    // Follows the active map unless another is picked
    private (string, ObjectContainer) TargetMap = MapTargetPicker.ActiveMap;

    private CreateAtPointType BaseType = CreateAtPointType.Part;
    private Type PartType;
    private Type RegionType;

    // The object whose properties are set up in the tab: each created object is a copy of it
    private object Template;

    private bool ShowProperties = true;
    private string PropertyFilter = "";
    private bool ExactPropertyFilter = false;

    private bool AutoName = true;
    private bool KeepPlacing = true;
    private float HeightOffset = 0.0f;

    // Random rotation, in degrees, added to the template's rotation for each object created
    private bool ShowRandomRotation = true;
    private bool RandomRotation = false;
    private bool RandomRotateX = false;
    private bool RandomRotateY = true;
    private bool RandomRotateZ = false;
    private Vector2 RandomRangeX = new(0, 360);
    private Vector2 RandomRangeY = new(0, 360);
    private Vector2 RandomRangeZ = new(0, 360);

    // Placement
    private bool Placing;
    private Vector3? Point;

    // A click waiting for its picking pass: where it was and in which viewport
    private bool AwaitingPick;
    private Vector2 AwaitingScreen;
    private VulkanViewport AwaitingViewport;

    // A second click made on the first one's spot before its pick came back: a double-click
    private bool CreateWhenPicked;

    // Clicks taken by the tool are held until the button is released, so the release doesn't select
    private bool SwallowUntilRelease;

    private string Status = "";
    private int LastShownFrame = -10;

    // How near the marker, in pixels, a click must be to create the object
    private static float MarkerRadius => 10.0f * DPI.UIScale();

    public CreateAtPointTool(MapEditorView view, ProjectEntry project, CreateAction owner)
    {
        View = view;
        Project = project;
        Owner = owner;
    }

    /// <summary>Whether the tab was shown this frame or the last (the viewport may update before it is drawn).</summary>
    private bool IsShown => ImGui.GetFrameCount() - LastShownFrame <= 1;

    private Type SelectedType => BaseType == CreateAtPointType.Part ? PartType : RegionType;

    private MsbEntityType SelectedEntityType => BaseType == CreateAtPointType.Part ? MsbEntityType.Part : MsbEntityType.Region;

    #region Display
    public void Display()
    {
        LastShownFrame = ImGui.GetFrameCount();

        GUI.WrappedText(LOC.Get("MAP_CreateAtPoint_Hint"));

        // Target Map
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CreateAtPoint_Target_Map_Header"), LOC.Get("MAP_CreateAtPoint_Target_Map_Header_TT"));

        MapTargetPicker.Display(View, "createAtPointTargetMapSelect", ref TargetMap);

        var map = View.Selection.ResolveTargetMap(TargetMap);

        if (map == null)
        {
            GUI.WrappedText(LOC.Get("MAP_CreateAtPoint_No_Map"));
            return;
        }

        DisplayTypeSelection();

        if (SelectedType == null)
        {
            GUI.WrappedText(LOC.Get("MAP_CreateAtPoint_No_Type"));
            return;
        }

        EnsureTemplate();

        if (Template == null)
        {
            GUI.WrappedText(LOC.Get("MAP_CreateAtPoint_Template_Failed", SelectedType.Name));
            return;
        }

        DisplayPlacementOptions();
        DisplayRandomRotation();
        DisplayActions();
        DisplayPoint();
        DisplayProperties();
    }

    private void DisplayTypeSelection()
    {
        // Base Type
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CreateAtPoint_Base_Type_Header"), LOC.Get("MAP_CreateAtPoint_Base_Type_Header_TT"));

        if (ImGui.RadioButton($"{LOC.Get("MAP_CreateAtPoint_Base_Type_Part")}##createAtPointBasePart", BaseType == CreateAtPointType.Part))
        {
            BaseType = CreateAtPointType.Part;
        }
        GUI.Tooltip(LOC.Get("MAP_CreateAtPoint_Base_Type_Part_TT"));

        ImGui.SameLine();

        if (ImGui.RadioButton($"{LOC.Get("MAP_CreateAtPoint_Base_Type_Region")}##createAtPointBaseRegion", BaseType == CreateAtPointType.Region))
        {
            BaseType = CreateAtPointType.Region;
        }
        GUI.Tooltip(LOC.Get("MAP_CreateAtPoint_Base_Type_Region_TT"));

        // Sub Type
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CreateAtPoint_Sub_Type_Header"), LOC.Get("MAP_CreateAtPoint_Sub_Type_Header_TT"));

        var classes = BaseType == CreateAtPointType.Part ? Owner.PartsClasses : Owner.RegionClasses;
        var current = SelectedType;

        ImGui.SetNextItemWidth(-1);
        if (ImGui.BeginCombo("##createAtPointSubType", current?.Name ?? LOC.Get("MAP_CreateAtPoint_Sub_Type_None")))
        {
            foreach (var (_, type) in classes)
            {
                if (ImGui.Selectable($"{type.Name}##createAtPointSubType_{type.FullName}", type == current))
                {
                    if (BaseType == CreateAtPointType.Part)
                        PartType = type;
                    else
                        RegionType = type;
                }
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("MAP_CreateAtPoint_Sub_Type_TT"));
    }

    private void DisplayPlacementOptions()
    {
        // Placement
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CreateAtPoint_Placement_Header"), LOC.Get("MAP_CreateAtPoint_Placement_Header_TT"));

        ImGui.SetNextItemWidth(-1);
        ImGui.DragFloat("##createAtPointHeightOffset", ref HeightOffset, 0.05f, 0, 0, LOC.Get("MAP_CreateAtPoint_Height_Offset_Format"));
        GUI.Tooltip(LOC.Get("MAP_CreateAtPoint_Height_Offset_TT"));

        ImGui.Checkbox($"{LOC.Get("MAP_CreateAtPoint_Keep_Placing")}##createAtPointKeepPlacing", ref KeepPlacing);
        GUI.Tooltip(LOC.Get("MAP_CreateAtPoint_Keep_Placing_TT"));

        ImGui.Checkbox($"{LOC.Get("MAP_CreateAtPoint_Auto_Name")}##createAtPointAutoName", ref AutoName);
        GUI.Tooltip(LOC.Get("MAP_CreateAtPoint_Auto_Name_TT"));

        if (!AutoName)
        {
            var nameProp = Template.GetType().GetProperty("Name");

            if (nameProp != null && nameProp.CanWrite && nameProp.PropertyType == typeof(string))
            {
                var name = (string)nameProp.GetValue(Template) ?? "";

                ImGui.SetNextItemWidth(-1);
                if (ImGui.InputText("##createAtPointName", ref name, 255))
                {
                    nameProp.SetValue(Template, name);
                }
                GUI.Tooltip(LOC.Get("MAP_CreateAtPoint_Name_TT"));
            }
        }
    }

    private void DisplayRandomRotation()
    {
        // Random Rotation
        GUI.Spacer();
        GUI.ConditionalHeader(LOC.Get("MAP_CreateAtPoint_Random_Rotation_Header"),
            LOC.Get("MAP_CreateAtPoint_Random_Rotation_Header_TT"), ref ShowRandomRotation);

        if (!ShowRandomRotation)
            return;

        ImGui.Checkbox($"{LOC.Get("MAP_CreateAtPoint_Random_Rotation_Enable")}##createAtPointRandomRotation", ref RandomRotation);
        GUI.Tooltip(LOC.Get("MAP_CreateAtPoint_Random_Rotation_Enable_TT"));

        if (!RandomRotation)
            return;

        DisplayRandomAxis("X", ref RandomRotateX, ref RandomRangeX);
        DisplayRandomAxis("Y", ref RandomRotateY, ref RandomRangeY);
        DisplayRandomAxis("Z", ref RandomRotateZ, ref RandomRangeZ);
    }

    private static void DisplayRandomAxis(string axis, ref bool enabled, ref Vector2 range)
    {
        ImGui.Checkbox($"{LOC.Get($"MAP_CreateAtPoint_Random_Rotation_Axis_{axis}")}##createAtPointRandomAxis{axis}", ref enabled);
        GUI.Tooltip(LOC.Get("MAP_CreateAtPoint_Random_Rotation_Axis_TT", axis));

        if (!enabled)
            return;

        ImGui.SetNextItemWidth(-1);
        if (ImGui.DragFloat2($"##createAtPointRandomRange{axis}", ref range, 1.0f, -360.0f, 360.0f,
            LOC.Get("MAP_CreateAtPoint_Random_Rotation_Range_Format")))
        {
            // Keep min at or below max, whichever end was dragged past the other
            if (range.X > range.Y)
                range = new Vector2(range.Y, range.X);
        }
        GUI.Tooltip(LOC.Get("MAP_CreateAtPoint_Random_Rotation_Range_TT", axis));
    }

    /// <summary>
    /// A random rotation within each enabled axis's range, in degrees, added to the template's rotation. The result is
    /// wrapped into -180..180, the range the MSBs store.
    /// </summary>
    private void ApplyRandomRotation(object entry)
    {
        if (!RandomRotation || (!RandomRotateX && !RandomRotateY && !RandomRotateZ))
            return;

        var prop = entry.GetType().GetProperty("Rotation");

        if (prop == null || prop.PropertyType != typeof(Vector3) || !prop.CanWrite)
            return;

        var rotation = (Vector3)prop.GetValue(entry);

        if (RandomRotateX) rotation.X = Wrap(rotation.X + Roll(RandomRangeX));
        if (RandomRotateY) rotation.Y = Wrap(rotation.Y + Roll(RandomRangeY));
        if (RandomRotateZ) rotation.Z = Wrap(rotation.Z + Roll(RandomRangeZ));

        prop.SetValue(entry, rotation);
    }

    private static float Roll(Vector2 range)
        => range.X + (float)Random.Shared.NextDouble() * (range.Y - range.X);

    private static float Wrap(float degrees)
    {
        var wrapped = (degrees + 180.0f) % 360.0f;
        if (wrapped < 0)
            wrapped += 360.0f;

        return wrapped - 180.0f;
    }

    private void DisplayActions()
    {
        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CreateAtPoint_Actions_Header"), LOC.Get("MAP_CreateAtPoint_Actions_Header_TT"));

        if (Placing)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color,
                LOC.Get(Point == null ? "MAP_CreateAtPoint_Status_Place" : "MAP_CreateAtPoint_Status_Confirm"));
        }
        else if (!string.IsNullOrEmpty(Status))
        {
            GUI.WrappedText(Status);
        }

        GUI.ConditionalMultiButtonInput("createAtPointActions",
            "createAtPointPlace", LOC.Get("MAP_CreateAtPoint_Place"), LOC.Get("MAP_CreateAtPoint_Place_TT"),
            BeginPlacing, !Placing,

            "createAtPointCreate", LOC.Get("MAP_CreateAtPoint_Create"), LOC.Get("MAP_CreateAtPoint_Create_TT"),
            () => CreateAtPoint(), Point != null,

            "createAtPointCancel", LOC.Get("MAP_CreateAtPoint_Cancel"), LOC.Get("MAP_CreateAtPoint_Cancel_TT"),
            CancelPlacing, Placing || Point != null);
    }

    private void DisplayPoint()
    {
        // Point
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CreateAtPoint_Point_Header"), LOC.Get("MAP_CreateAtPoint_Point_Header_TT"));

        if (Point is Vector3 p)
        {
            ImGui.SetNextItemWidth(-1);
            if (ImGui.DragFloat3("##createAtPointPosition", ref p, 0.05f, 0, 0, "%.3f"))
            {
                Point = p;
            }
            GUI.Tooltip(LOC.Get("MAP_CreateAtPoint_Position_TT"));
        }
        else
        {
            GUI.WrappedText(LOC.Get("MAP_CreateAtPoint_Point_Not_Set"));
        }
    }

    private void DisplayProperties()
    {
        // Properties
        GUI.Spacer();
        GUI.ConditionalHeader(LOC.Get("MAP_CreateAtPoint_Properties_Header", SelectedType.Name),
            LOC.Get("MAP_CreateAtPoint_Properties_Header_TT"), ref ShowProperties);

        if (!ShowProperties)
            return;

        EditorFilters.DisplayFramedListFilter("createAtPointProperties", ref PropertyFilter, ref ExactPropertyFilter);

        GUI.ConditionalMultiButtonInput("createAtPointPropertyActions",
            "createAtPointReset", LOC.Get("MAP_CreateAtPoint_Reset"), LOC.Get("MAP_CreateAtPoint_Reset_TT"),
            ResetTemplate, true,

            "createAtPointFromSelection", LOC.Get("MAP_CreateAtPoint_From_Selection"), LOC.Get("MAP_CreateAtPoint_From_Selection_TT"),
            CopyFromSelection, GetSelectionSource() != null);

        ImGui.BeginChild("##createAtPointPropertyList", new Vector2(0, 0), ImGuiChildFlags.Borders);

        if (ImGui.BeginTable("##createAtPointPropertyTable", 2, ImGuiTableFlags.Resizable | ImGuiTableFlags.RowBg))
        {
            ImGui.TableSetupColumn("##createAtPointPropertyName", ImGuiTableColumnFlags.WidthStretch, 0.4f);
            ImGui.TableSetupColumn("##createAtPointPropertyValue", ImGuiTableColumnFlags.WidthStretch, 0.6f);

            var id = 0;
            DisplayObject(Template, 0, ref id);

            ImGui.EndTable();
        }

        ImGui.EndChild();
    }
    #endregion

    #region Template
    /// <summary>Makes the template when there is none, or when the type picked is not the one it was made as.</summary>
    private void EnsureTemplate()
    {
        if (Template != null && Template.GetType() == SelectedType)
            return;

        ResetTemplate();
    }

    private void ResetTemplate()
    {
        Template = null;

        if (SelectedType == null)
            return;

        try
        {
            Template = SelectedType.GetConstructor(Type.EmptyTypes)?.Invoke(null);
        }
        catch (Exception e)
        {
            Smithbox.LogError<CreateAtPointTool>($"Failed to create a {SelectedType.Name}.", e);
        }
    }

    /// <summary>The selected object of the template's type, to take the properties of.</summary>
    private object GetSelectionSource()
    {
        if (SelectedType == null)
            return null;

        return View.ViewportSelection.GetFilteredSelection<MsbEntity>(e => e.WrappedObject?.GetType() == SelectedType)
            .Select(e => e.WrappedObject)
            .FirstOrDefault();
    }

    private void CopyFromSelection()
    {
        var source = GetSelectionSource();

        var copy = Copy(source);

        if (copy != null)
        {
            Template = copy;
        }
    }

    /// <summary>A copy of the template, or of another MSB entry, that shares nothing with it.</summary>
    private static object Copy(object entry) => entry switch
    {
        IMsbPart part => part.DeepCopy(),
        IMsbRegion region => region.DeepCopy(),
        _ => null
    };
    #endregion

    #region Property Editing
    /// <summary>
    /// The rows for an object's properties, as name and value. Classes nested within it open as trees of their own.
    /// </summary>
    private void DisplayObject(object obj, int depth, ref int id)
    {
        if (obj == null || depth > 8)
            return;

        var type = obj.GetType();

        foreach (var prop in View.MapPropertyCache.GetCachedProperties(type))
        {
            if (prop.GetIndexParameters().Length > 0)
                continue;

            var meta = Project.Handler.MapData.Meta.GetFieldMeta(prop.Name, type);

            if (!ShowProperty(prop, type, meta, depth))
                continue;

            var name = prop.Name;
            if (CFG.Current.MapEditor_Properties_Enable_Commmunity_Names && meta != null && !meta.IsEmpty && !string.IsNullOrEmpty(meta.AltName))
            {
                name = meta.AltName;
            }

            var tooltip = meta != null && !meta.IsEmpty ? meta.Wiki : "";

            ImGui.PushID(id++);

            DisplayProperty(obj, prop, name, tooltip, meta, depth, ref id);

            ImGui.PopID();
        }
    }

    private bool ShowProperty(PropertyInfo prop, Type owner, MapEntityPropertyFieldMeta meta, int depth)
    {
        var propType = prop.PropertyType;

        // Read-only properties can still hold arrays and classes that are edited in place
        if (!prop.CanWrite && !propType.IsArray && !IsNestedClass(propType))
            return false;

        if (depth == 0 && typeof(IMsbEntry).IsAssignableFrom(owner))
        {
            // Set by the click, and by the naming options
            if (prop.Name is "Position" or "Name")
                return false;
        }

        if (meta != null && meta.IndexProperty)
            return false;

        if (!CFG.Current.MapEditor_Properties_Display_Unknown_Properties && prop.Name.StartsWith("unk", StringComparison.OrdinalIgnoreCase))
            return false;

        // The filter applies to the template's own fields, so a nested class shows whole once it matches
        if (depth == 0 && !EditorFilters.IsMatch(PropertyFilter, prop.Name, ExactPropertyFilter, meta?.AltName ?? ""))
            return false;

        return true;
    }

    private static bool IsNestedClass(Type type)
        => type.IsClass && type != typeof(string) && !type.IsArray && !typeof(IList).IsAssignableFrom(type);

    private void DisplayProperty(object obj, PropertyInfo prop, string name, string tooltip, MapEntityPropertyFieldMeta meta, int depth, ref int id)
    {
        var propType = prop.PropertyType;
        var value = prop.GetValue(obj);

        ImGui.TableNextRow();
        ImGui.TableSetColumnIndex(0);
        ImGui.AlignTextToFramePadding();

        // Region shapes: the shape type is picked here, then its fields are set below
        if (propType == typeof(MSB.Shape) && prop.CanWrite)
        {
            var open = ImGui.TreeNodeEx(name, ImGuiTreeNodeFlags.DefaultOpen);
            ShowTooltip(tooltip);

            ImGui.TableSetColumnIndex(1);
            DisplayShapeCombo(obj, prop, value);

            if (open)
            {
                DisplayObject(value, depth + 1, ref id);
                ImGui.TreePop();
            }

            return;
        }

        if (value is IList list && (propType.IsArray || typeof(IList).IsAssignableFrom(propType)))
        {
            var open = ImGui.TreeNodeEx(name, ImGuiTreeNodeFlags.None);
            ShowTooltip(tooltip);

            ImGui.TableSetColumnIndex(1);
            ImGui.TextDisabled($"[{list.Count}]");

            if (open)
            {
                DisplayList(list, name, depth, ref id);
                ImGui.TreePop();
            }

            return;
        }

        if (IsNestedClass(propType))
        {
            var open = ImGui.TreeNodeEx(name, ImGuiTreeNodeFlags.None);
            ShowTooltip(tooltip);

            ImGui.TableSetColumnIndex(1);
            ImGui.TextDisabled(value?.GetType().Name ?? "null");

            if (open)
            {
                DisplayObject(value, depth + 1, ref id);
                ImGui.TreePop();
            }

            return;
        }

        ImGui.Text(name);
        ShowTooltip(tooltip);

        ImGui.TableSetColumnIndex(1);

        if (!prop.CanWrite)
        {
            ImGui.TextDisabled($"{value}");
            return;
        }

        if (EditValue(propType, value, meta != null && meta.IsBool, out var newValue))
        {
            prop.SetValue(obj, newValue);
        }
    }

    private void DisplayList(IList list, string name, int depth, ref int id)
    {
        var elementType = list.GetType().IsArray
            ? list.GetType().GetElementType()
            : list.GetType().GetGenericArguments().FirstOrDefault() ?? typeof(object);

        // Long lists of values (draw groups) would otherwise flood the table
        var count = Math.Min(list.Count, 512);

        for (var i = 0; i < count; i++)
        {
            ImGui.PushID(id++);

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            ImGui.AlignTextToFramePadding();

            var item = list[i];

            if (item != null && IsNestedClass(item.GetType()))
            {
                var open = ImGui.TreeNodeEx($"{name}[{i}]", ImGuiTreeNodeFlags.None);

                ImGui.TableSetColumnIndex(1);
                ImGui.TextDisabled(item.GetType().Name);

                if (open)
                {
                    DisplayObject(item, depth + 1, ref id);
                    ImGui.TreePop();
                }
            }
            else
            {
                ImGui.Text($"{name}[{i}]");

                ImGui.TableSetColumnIndex(1);

                if (!list.IsReadOnly && EditValue(elementType, item, false, out var newValue))
                {
                    list[i] = newValue;
                }
            }

            ImGui.PopID();
        }
    }

    private static void DisplayShapeCombo(object obj, PropertyInfo prop, object value)
    {
        var shapeTypes = GetShapeTypes();

        ImGui.SetNextItemWidth(-1);
        if (ImGui.BeginCombo("##shape", value?.GetType().Name ?? "null"))
        {
            foreach (var shapeType in shapeTypes)
            {
                if (ImGui.Selectable(shapeType.Name, value?.GetType() == shapeType))
                {
                    try
                    {
                        prop.SetValue(obj, Activator.CreateInstance(shapeType));
                    }
                    catch (Exception e)
                    {
                        Smithbox.LogError<CreateAtPointTool>($"Failed to create a {shapeType.Name} shape.", e);
                    }
                }
            }

            ImGui.EndCombo();
        }
    }

    private static List<Type> ShapeTypes;

    private static List<Type> GetShapeTypes()
    {
        ShapeTypes ??= typeof(MSB.Shape).GetNestedTypes()
            .Where(t => t.IsSubclassOf(typeof(MSB.Shape)) && !t.IsAbstract && t.GetConstructor(Type.EmptyTypes) != null)
            .ToList();

        return ShapeTypes;
    }

    private static void ShowTooltip(string tooltip)
    {
        if (!string.IsNullOrEmpty(tooltip))
        {
            GUI.Tooltip(tooltip);
        }
    }

    /// <summary>The input for a single value. Returns true with the new value when it was changed.</summary>
    private static unsafe bool EditValue(Type type, object value, bool isBool, out object newValue)
    {
        newValue = null;

        ImGui.SetNextItemWidth(-1);

        if (type.IsEnum)
        {
            var names = Enum.GetNames(type);
            var current = value != null ? Enum.GetName(type, value) ?? $"{value}" : "";

            if (ImGui.BeginCombo("##value", current))
            {
                foreach (var enumName in names)
                {
                    if (ImGui.Selectable(enumName, enumName == current))
                    {
                        newValue = Enum.Parse(type, enumName);
                    }
                }

                ImGui.EndCombo();
            }

            return newValue != null;
        }

        if (type == typeof(bool))
        {
            var v = (bool)value;
            if (ImGui.Checkbox("##value", ref v))
            {
                newValue = v;
                return true;
            }
            return false;
        }

        // Integer fields the meta marks as flags are shown as checkboxes
        if (isBool && (type == typeof(byte) || type == typeof(sbyte) || type == typeof(short) || type == typeof(ushort)
            || type == typeof(int) || type == typeof(uint)))
        {
            var v = Convert.ToInt64(value) != 0;
            if (ImGui.Checkbox("##value", ref v))
            {
                newValue = Convert.ChangeType(v ? 1 : 0, type);
                return true;
            }
            return false;
        }

        if (type == typeof(string))
        {
            var v = (string)value ?? "";
            if (ImGui.InputText("##value", ref v, 255))
            {
                newValue = v;
                return true;
            }
            return false;
        }

        if (type == typeof(float))
        {
            var v = (float)value;
            if (ImGui.DragFloat("##value", ref v, 0.01f, 0, 0, "%.3f"))
            {
                newValue = v;
                return true;
            }
            return false;
        }

        if (type == typeof(double))
        {
            var v = (double)value;
            if (ImGui.InputScalar("##value", ImGuiDataType.Double, &v))
            {
                newValue = v;
                return true;
            }
            return false;
        }

        if (type == typeof(Vector2))
        {
            var v = (Vector2)value;
            if (ImGui.DragFloat2("##value", ref v, 0.01f, 0, 0, "%.3f"))
            {
                newValue = v;
                return true;
            }
            return false;
        }

        if (type == typeof(Vector3))
        {
            var v = (Vector3)value;
            if (ImGui.DragFloat3("##value", ref v, 0.01f, 0, 0, "%.3f"))
            {
                newValue = v;
                return true;
            }
            return false;
        }

        if (type == typeof(Vector4))
        {
            var v = (Vector4)value;
            if (ImGui.DragFloat4("##value", ref v, 0.01f, 0, 0, "%.3f"))
            {
                newValue = v;
                return true;
            }
            return false;
        }

        if (type == typeof(byte)) return EditScalar<byte>(ImGuiDataType.U8, value, out newValue);
        if (type == typeof(sbyte)) return EditScalar<sbyte>(ImGuiDataType.S8, value, out newValue);
        if (type == typeof(short)) return EditScalar<short>(ImGuiDataType.S16, value, out newValue);
        if (type == typeof(ushort)) return EditScalar<ushort>(ImGuiDataType.U16, value, out newValue);
        if (type == typeof(int)) return EditScalar<int>(ImGuiDataType.S32, value, out newValue);
        if (type == typeof(uint)) return EditScalar<uint>(ImGuiDataType.U32, value, out newValue);
        if (type == typeof(long)) return EditScalar<long>(ImGuiDataType.S64, value, out newValue);
        if (type == typeof(ulong)) return EditScalar<ulong>(ImGuiDataType.U64, value, out newValue);

        ImGui.TextDisabled($"{value}");
        return false;
    }

    private static unsafe bool EditScalar<T>(ImGuiDataType dataType, object value, out object newValue) where T : unmanaged
    {
        var v = (T)value;
        newValue = null;

        if (ImGui.InputScalar("##value", dataType, &v))
        {
            newValue = v;
            return true;
        }

        return false;
    }
    #endregion

    #region Viewport
    private void BeginPlacing()
    {
        Placing = true;
        Point = null;
        Status = "";
    }

    private void CancelPlacing()
    {
        Placing = false;
        Point = null;
        AwaitingPick = false;
        CreateWhenPicked = false;
    }

    /// <summary>
    /// Places the marker and creates the object from clicks in the viewport. Called each frame by the active viewport.
    /// Returns true while the viewport's clicks are taken, so they don't select.
    /// </summary>
    public bool HandleViewportInput(VulkanViewport viewport, bool canPick)
    {
        if (!IsShown)
        {
            if (Placing || AwaitingPick)
            {
                CancelPlacing();
            }

            SwallowUntilRelease = false;
            return false;
        }

        // A placing click's result, from the viewport it was made in
        if (AwaitingPick)
        {
            var pipeline = AwaitingViewport.ViewPipeline;

            if (pipeline.PickingResultsReady)
            {
                pipeline.PickingResultsReady = false;
                AwaitingPick = false;
                FinishPlacing(AwaitingViewport, pipeline.PickingDepth);
            }
            else if (!pipeline.IsPickingPending)
            {
                // The request was taken by something else
                AwaitingPick = false;
                CreateWhenPicked = false;
            }
        }

        var taken = false;

        if (SwallowUntilRelease)
        {
            if (!InputManager.IsMouseDown(MousebindID.Viewport_Picking_Action) || InputManager.IsMouseReleased(MousebindID.Viewport_Picking_Action))
                SwallowUntilRelease = false;

            taken = true;
        }

        if (!Placing)
            return taken;

        if (InputManager.IsKeyPressed(Key.Escape))
        {
            CancelPlacing();
            return true;
        }

        var pressed = canPick && InputManager.IsMousePressed(MousebindID.Viewport_Picking_Action);

        if (!pressed)
            return true;

        SwallowUntilRelease = true;

        var mouse = InputManager.MousePosition;
        var screen = new Vector2(MathF.Floor(mouse.X - viewport.X) + 0.5f, MathF.Floor(mouse.Y - viewport.Y) + 0.5f);

        // The second click of a double-click, made before the first one's pick came back
        if (AwaitingPick)
        {
            if (Vector2.Distance(screen, AwaitingScreen) <= MarkerRadius)
            {
                CreateWhenPicked = true;
            }

            return true;
        }

        // A click on the marker creates the object
        if (Point is Vector3 point && IsOnMarker(viewport, point, screen))
        {
            CreateAtPoint();
            return true;
        }

        // Anywhere else places (or moves) the marker
        if (!viewport.ViewPipeline.IsPickingPending)
        {
            AwaitingScreen = screen;
            AwaitingViewport = viewport;
            AwaitingPick = true;
            CreateWhenPicked = false;

            viewport.ViewPipeline.PickingResultsReady = false;
            viewport.ViewPipeline.CreateAsyncPickingRequest();
        }

        return true;
    }

    private static bool IsOnMarker(VulkanViewport viewport, Vector3 point, Vector2 screen)
    {
        var marker = viewport.WorldToScreen(point, out var inFront);

        return inFront && Vector2.Distance(marker, screen) <= MarkerRadius;
    }

    /// <summary>Turns the picked depth into the point under the click.</summary>
    private void FinishPlacing(VulkanViewport viewport, float depth)
    {
        var createNow = CreateWhenPicked;
        CreateWhenPicked = false;

        if (depth <= 0 || !Unproject(viewport, AwaitingScreen, depth, out var point))
        {
            Status = LOC.Get("MAP_CreateAtPoint_Status_Nothing_Hit");
            Smithbox.Log(this, Status, LogLevel.Warning);
            return;
        }

        // Diagnostic for verification: remove once placement is confirmed
        Smithbox.Log(this, $"[Create at Point] point placed: screen {AwaitingScreen}, depth {depth:0.########}, point {point}, double-click {createNow}",
            LogLevel.Debug);

        Point = point;
        Status = "";

        if (createNow)
        {
            CreateAtPoint();
        }
    }

    /// <summary>
    /// The world position at a viewport position (pixels from its corner) and a picking depth, through the inverse of
    /// the view projection the scene was drawn with, as the Ruler finds its points.
    /// </summary>
    private static bool Unproject(VulkanViewport viewport, Vector2 screen, float depth, out Vector3 point)
    {
        point = default;

        if (viewport.Width <= 0 || viewport.Height <= 0)
            return false;

        var x = 2.0f * screen.X / viewport.Width - 1.0f;
        var y = 1.0f - 2.0f * screen.Y / viewport.Height;
        var z = viewport.Device.IsDepthRangeZeroToOne ? depth : depth * 2.0f - 1.0f;

        var viewProjection = viewport.ViewportCamera.CameraTransform.CameraViewMatrixLH * viewport.ViewportCamera.ProjectionMatrix;

        if (!Matrix4x4.Invert(viewProjection, out var inverse))
            return false;

        var world = Vector4.Transform(new Vector4(x, y, z, 1.0f), inverse);

        if (MathF.Abs(world.W) < 1e-12f)
            return false;

        point = new Vector3(world.X, world.Y, world.Z) / world.W;

        return float.IsFinite(point.X) && float.IsFinite(point.Y) && float.IsFinite(point.Z);
    }

    /// <summary>
    /// The marker at the placed point, and a hint by the cursor while placing. Called by the viewport's overlay.
    /// </summary>
    public void DrawOverlay(VulkanViewport viewport)
    {
        if (!IsShown || viewport.Owner is not MapUniverse)
            return;

        if (!Placing && Point == null)
            return;

        var drawList = ImGui.GetWindowDrawList();
        var origin = new Vector2(viewport.X, viewport.Y);
        var shadow = ImGui.GetColorU32(new Vector4(0, 0, 0, 1));
        var color = ImGui.GetColorU32(UI.Current.ImGui_Benefit_Text_Color);
        var scale = DPI.UIScale();

        if (Point is Vector3 point)
        {
            var screen = viewport.WorldToScreen(point, out var inFront);

            if (inFront && screen.X >= 0 && screen.Y >= 0 && screen.X <= viewport.Width && screen.Y <= viewport.Height)
            {
                var at = origin + screen;
                var r = MarkerRadius;

                drawList.AddCircle(at, r + 1, shadow, 24, 3.0f * scale);
                drawList.AddCircle(at, r, color, 24, 2.0f * scale);
                drawList.AddLine(at - new Vector2(r * 0.5f, 0), at + new Vector2(r * 0.5f, 0), color, 1.5f * scale);
                drawList.AddLine(at - new Vector2(0, r * 0.5f), at + new Vector2(0, r * 0.5f), color, 1.5f * scale);

                if (Placing)
                {
                    var text = LOC.Get("MAP_CreateAtPoint_Overlay_Confirm", SelectedType?.Name ?? "");
                    var textAt = at + new Vector2(r + 4, -r - 4) * new Vector2(1, 1);
                    drawList.AddText(textAt + Vector2.One, shadow, text);
                    drawList.AddText(textAt, color, text);
                }
            }
        }

        if (Placing && Point == null && !AwaitingPick)
        {
            var mouse = InputManager.MousePosition;
            var text = LOC.Get("MAP_CreateAtPoint_Overlay_Place");
            var textAt = mouse + new Vector2(16, 8) * scale;
            drawList.AddText(textAt + Vector2.One, shadow, text);
            drawList.AddText(textAt, color, text);
        }
    }
    #endregion

    #region Creation
    private void CreateAtPoint()
    {
        if (Point is not Vector3 point)
            return;

        var map = View.Selection.ResolveTargetMap(TargetMap);

        if (map == null)
        {
            Status = LOC.Get("MAP_CreateAtPoint_No_Map");
            Smithbox.LogError<CreateAtPointTool>(Status);
            return;
        }

        EnsureTemplate();

        var entry = Copy(Template);

        if (entry == null)
        {
            Status = LOC.Get("MAP_CreateAtPoint_Template_Failed", SelectedType?.Name ?? "");
            Smithbox.LogError<CreateAtPointTool>(Status);
            return;
        }

        var parent = map.RootObject;

        SetPosition(entry, parent, point + new Vector3(0, HeightOffset, 0));
        ApplyRandomRotation(entry);

        if (AutoName)
        {
            AssignName(entry, map);
        }

        var entity = new MsbEntity(View.Universe, map, entry, SelectedEntityType);

        var action = new EntAddAction(View, map, new List<MsbEntity> { entity }, parent);
        View.ViewportActionManager.ExecuteAction(action);

        var name = (entry as IMsbEntry)?.Name ?? SelectedType.Name;
        Status = LOC.Get("MAP_CreateAtPoint_Status_Created", name, map.Name);
        Smithbox.Log(this, Status);

        View.DelayPicking();

        Point = null;

        if (!KeepPlacing)
        {
            Placing = false;
        }
    }

    /// <summary>
    /// Sets the entry's position to a world point, made local to the parent chain (which, for tiled maps, holds the
    /// map's offset).
    /// </summary>
    private static void SetPosition(object entry, Entity parent, Vector3 world)
    {
        var prop = entry.GetType().GetProperty("Position");

        if (prop == null || prop.PropertyType != typeof(Vector3) || !prop.CanWrite)
            return;

        var root = Transform.Default;
        for (var p = parent; p != null; p = p.Parent)
        {
            root += p.GetLocalTransform();
        }

        var local = Vector3.Transform(world - root.Position, Quaternion.Inverse(root.Rotation));

        prop.SetValue(entry, local);
    }

    /// <summary>
    /// A name the map doesn't use yet. Parts take the game's model_suffix form (with a unique instance ID for ER and
    /// NR), as Mass Create names them; regions take their type's name with a number.
    /// </summary>
    private void AssignName(object entry, MapContainer map)
    {
        var names = new HashSet<string>(map.Objects.Select(o => o.Name).Where(n => n != null), StringComparer.OrdinalIgnoreCase);

        if (entry is IMsbPart part)
        {
            var model = string.IsNullOrEmpty(part.ModelName) ? SelectedType.Name : part.ModelName;

            var ids = new MassCreateTool.InstanceIdTracker(Project.Descriptor.ProjectType, map);
            ids.Assign(entry, model, names);
            return;
        }

        if (entry is IMsbEntry msbEntry)
        {
            var baseName = string.IsNullOrEmpty(msbEntry.Name) ? SelectedType.Name : msbEntry.Name;

            var suffix = 0;
            while (names.Contains($"{baseName}_{suffix:D4}"))
            {
                suffix++;
            }

            msbEntry.Name = $"{baseName}_{suffix:D4}";
        }
    }
    #endregion
}

public enum CreateAtPointType
{
    Part,
    Region
}
