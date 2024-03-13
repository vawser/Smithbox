using Andre.Formats;
using ImGuiNET;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Banks;
using StudioCore.Banks.FormatBank;
using StudioCore.BanksMain;
using StudioCore.Editor;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.MapEditor.Toolbar;
using StudioCore.Gui;
using StudioCore.Interface;
using StudioCore.Scene;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Veldrid.Utilities;
using static StudioCore.Editors.MapEditor.MapPropertyEditor;

namespace StudioCore.Editors.ModelEditor;

public class ModelPropertyEditor
{
    private readonly string[] _lightTypes = { "Spot", "Directional", "Point" };

    private readonly ModelPropertyCache _propCache;

    private readonly string[] _regionShapes =
    {
        "Point", "Sphere", "Cylinder", "Box", "Composite", "Rectangle", "Circle"
    };

    private object _changingObject;
    private object _changingPropery;
    private ViewportAction _lastUncommittedAction;

    public ViewportActionManager ContextActionManager;
    public PropertyInfo RequestedSearchProperty = null;

    private IViewport _viewport;

    private MapEditorToolbar _msbToolbar;

    public ModelPropertyEditor(ViewportActionManager manager, ModelPropertyCache propCache, IViewport viewport, MapEditorToolbar msbToolbar)
    {
        ContextActionManager = manager;
        _propCache = propCache;
        _viewport = viewport;
        _msbToolbar = msbToolbar;
    }

   
    private (bool, bool) PropertyRow(Type typ, object oldval, out object newval, PropertyInfo prop)
    {
        ImGui.SetNextItemWidth(-1);

        newval = null;
        var isChanged = false;
        if (typ == typeof(long))
        {
            var val = (long)oldval;
            var strval = $@"{val}";

            if (ImGui.InputText("##value", ref strval, 99))
            {
                var res = long.TryParse(strval, out val);
                if (res)
                {
                    newval = val;
                    isChanged = true;
                }
            }
        }
        else if (typ == typeof(int))
        {
            var val = (int)oldval;

            if (FlverFormatBank.Bank.IsBooleanProperty(prop.Name))
            {
                bool bVar = false;

                if (val > 0)
                    bVar = true;

                if (ImGui.Checkbox("##value", ref bVar))
                {
                    if (bVar == true)
                        val = 1;
                    else
                        val = 0;

                    newval = val;
                    isChanged = true;
                }
            }
            else
            {
                if (ImGui.InputInt("##value", ref val))
                {
                    newval = val;
                    isChanged = true;
                }
            }
        }
        else if (typ == typeof(uint))
        {
            var val = (uint)oldval;
            var strval = $@"{val}";

            if (FlverFormatBank.Bank.IsBooleanProperty(prop.Name))
            {
                bool bVar = false;

                if (val > 0)
                    bVar = true;

                if (ImGui.Checkbox("##value", ref bVar))
                {
                    if (bVar == true)
                        val = 1;
                    else
                        val = 0;

                    newval = val;
                    isChanged = true;
                }
            }
            else
            {
                if (ImGui.InputText("##value", ref strval, 16))
                {
                    var res = uint.TryParse(strval, out val);
                    if (res)
                    {
                        newval = val;
                        isChanged = true;
                    }
                }
            }
        }
        else if (typ == typeof(short))
        {
            int val = (short)oldval;

            if (FlverFormatBank.Bank.IsBooleanProperty(prop.Name))
            {
                bool bVar = false;

                if (val > 0)
                    bVar = true;

                if (ImGui.Checkbox("##value", ref bVar))
                {
                    if (bVar == true)
                        val = 1;
                    else
                        val = 0;

                    newval = val;
                    isChanged = true;
                }
            }
            else
            {
                if (ImGui.InputInt("##value", ref val))
                {
                    newval = (short)val;
                    isChanged = true;
                }
            }
        }
        else if (typ == typeof(ushort))
        {
            var val = (ushort)oldval;
            var strval = $@"{val}";

            if (FlverFormatBank.Bank.IsBooleanProperty(prop.Name))
            {
                bool bVar = false;

                if (val > 0)
                    bVar = true;

                if (ImGui.Checkbox("##value", ref bVar))
                {
                    if (bVar == true)
                        val = 1;
                    else
                        val = 0;

                    newval = val;
                    isChanged = true;
                }
            }
            else
            {
                if (ImGui.InputText("##value", ref strval, 5))
                {
                    var res = ushort.TryParse(strval, out val);
                    if (res)
                    {
                        newval = val;
                        isChanged = true;
                    }
                }
            }
        }
        else if (typ == typeof(sbyte))
        {
            int val = (sbyte)oldval;

            if (FlverFormatBank.Bank.IsBooleanProperty(prop.Name))
            {
                bool bVar = false;

                if (val > 0)
                    bVar = true;

                if (ImGui.Checkbox("##value", ref bVar))
                {
                    if (bVar == true)
                        val = 1;
                    else
                        val = 0;

                    newval = val;
                    isChanged = true;
                }
            }
            else
            {
                if (ImGui.InputInt("##value", ref val))
                {
                    newval = (sbyte)val;
                    isChanged = true;
                }
            }
        }
        else if (typ == typeof(byte))
        {
            var val = (byte)oldval;
            var strval = $@"{val}";

            if (FlverFormatBank.Bank.IsBooleanProperty(prop.Name))
            {
                bool bVar = false;

                if (val > 0)
                    bVar = true;

                if (ImGui.Checkbox("##value", ref bVar))
                {
                    if (bVar == true)
                        val = 1;
                    else
                        val = 0;

                    newval = val;
                    isChanged = true;
                }
            }
            else
            {
                if (ImGui.InputText("##value", ref strval, 3))
                {
                    var res = byte.TryParse(strval, out val);
                    if (res)
                    {
                        newval = val;
                        isChanged = true;
                    }
                }
            }
        }
        else if (typ == typeof(bool))
        {
            var val = (bool)oldval;
            if (ImGui.Checkbox("##value", ref val))
            {
                newval = val;
                isChanged = true;
            }
        }
        else if (typ == typeof(float))
        {
            var val = (float)oldval;
            if (ImGui.DragFloat("##value", ref val, 0.1f, float.MinValue, float.MaxValue,
                    Utils.ImGui_InputFloatFormat(val)))
            {
                newval = val;
                isChanged = true;
            }
        }
        else if (typ == typeof(string))
        {
            var val = (string)oldval;
            if (val == null)
            {
                val = "";
            }

            if (ImGui.InputText("##value", ref val, 99))
            {
                newval = val;
                isChanged = true;
            }
        }
        else if (typ == typeof(Vector2))
        {
            var val = (Vector2)oldval;
            if (ImGui.DragFloat2("##value", ref val, 0.1f))
            {
                newval = val;
                isChanged = true;
            }
        }
        else if (typ == typeof(Vector3))
        {
            var val = (Vector3)oldval;
            if (ImGui.DragFloat3("##value", ref val, 0.1f))
            {
                newval = val;
                isChanged = true;
            }
        }
        else if (typ.BaseType == typeof(Enum))
        {
            Array enumVals = typ.GetEnumValues();
            var enumNames = typ.GetEnumNames();
            var intVals = new int[enumVals.Length];

            if (typ.GetEnumUnderlyingType() == typeof(byte))
            {
                for (var i = 0; i < enumVals.Length; i++)
                {
                    intVals[i] = (byte)enumVals.GetValue(i);
                }

                if (Utils.EnumEditor(enumVals, enumNames, oldval, out var val, intVals))
                {
                    newval = val;
                    isChanged = true;
                }
            }
            else if (typ.GetEnumUnderlyingType() == typeof(int))
            {
                for (var i = 0; i < enumVals.Length; i++)
                {
                    intVals[i] = (int)enumVals.GetValue(i);
                }

                if (Utils.EnumEditor(enumVals, enumNames, oldval, out var val, intVals))
                {
                    newval = val;
                    isChanged = true;
                }
            }
            else if (typ.GetEnumUnderlyingType() == typeof(uint))
            {
                for (var i = 0; i < enumVals.Length; i++)
                {
                    intVals[i] = (int)(uint)enumVals.GetValue(i);
                }

                if (Utils.EnumEditor(enumVals, enumNames, oldval, out var val, intVals))
                {
                    newval = val;
                    isChanged = true;
                }
            }
            else
            {
                ImGui.Text("ImplementMe");
            }
        }
        else if (typ == typeof(Color))
        {
            var att = prop?.GetCustomAttribute<SupportsAlphaAttribute>();
            if (att != null)
            {
                if (att.Supports == false)
                {
                    var color = (Color)oldval;
                    Vector3 val = new(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f);
                    if (ImGui.ColorEdit3("##value", ref val))
                    {
                        Color newColor = Color.FromArgb((int)(val.X * 255.0f), (int)(val.Y * 255.0f),
                            (int)(val.Z * 255.0f));
                        newval = newColor;
                        isChanged = true;
                    }
                }
                else
                {
                    var color = (Color)oldval;
                    Vector4 val = new(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f);
                    if (ImGui.ColorEdit4("##value", ref val))
                    {
                        Color newColor = Color.FromArgb((int)(val.W * 255.0f), (int)(val.X * 255.0f),
                            (int)(val.Y * 255.0f), (int)(val.Z * 255.0f));
                        newval = newColor;
                        isChanged = true;
                    }
                }
            }
            else
            {
                // SoulsFormats does not define if alpha should be exposed. Expose alpha by default.
                // TaskLogs.AddLog( $"Color property in \"{prop.DeclaringType}\" does not declare if it supports Alpha. Alpha will be exposed by default", LogLevel.Warning, TaskLogs.LogPriority.Low);

                var color = (Color)oldval;
                Vector4 val = new(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f);
                if (ImGui.ColorEdit4("##value", ref val))
                {
                    Color newColor = Color.FromArgb((int)(val.W * 255.0f), (int)(val.X * 255.0f),
                        (int)(val.Y * 255.0f), (int)(val.Z * 255.0f));
                    newval = newColor;
                    isChanged = true;
                }
            }
        }
        else
        {
            ImGui.Text("ImplementMe");
        }

        var isDeactivatedAfterEdit = ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive();

        return (isChanged, isDeactivatedAfterEdit);
    }

    private void UpdateProperty(object prop, Entity selection, object obj, object newval,
        bool changed, bool committed, int arrayindex = -1)
    {
        // TODO2: strip this out in favor of other ChangeProperty
        if (changed)
        {
            ChangeProperty(prop, selection, obj, newval, ref committed, arrayindex);
        }

        if (committed)
        {
            CommitProperty(selection, false);
        }
    }

    private void ChangeProperty(object prop, Entity selection, object obj, object newval,
        ref bool committed, int arrayindex = -1)
    {
        if (prop == _changingPropery && _lastUncommittedAction != null &&
            ContextActionManager.PeekUndoAction() == _lastUncommittedAction)
        {
            ContextActionManager.UndoAction();
        }
        else
        {
            _lastUncommittedAction = null;
        }

        if (_changingObject != null && selection != null && selection.WrappedObject != _changingObject)
        {
            committed = true;
        }
        else
        {
            MapEditor.PropertiesChangedAction action;
            if (arrayindex != -1)
            {
                action = new MapEditor.PropertiesChangedAction((PropertyInfo)prop, arrayindex, obj, newval);
            }
            else
            {
                action = new MapEditor.PropertiesChangedAction((PropertyInfo)prop, obj, newval);
            }

            ContextActionManager.ExecuteAction(action);

            _lastUncommittedAction = action;
            _changingPropery = prop;
            _changingObject = selection != null ? selection.WrappedObject : obj;
        }
    }

    private void CommitProperty(Entity selection, bool destroyRenderModel)
    {
        // Invalidate name cache
        if (selection != null)
        {
            selection.Name = null;
        }

        // Undo and redo the last action with a rendering update
        if (_lastUncommittedAction != null && ContextActionManager.PeekUndoAction() == _lastUncommittedAction)
        {
            if (_lastUncommittedAction is MapEditor.PropertiesChangedAction a)
            {
                // Kinda a hack to prevent a jumping glitch
                a.SetPostExecutionAction(null);
                ContextActionManager.UndoAction();
                if (selection != null)
                {
                    a.SetPostExecutionAction(undo =>
                    {
                        if (destroyRenderModel)
                        {
                            if (selection.RenderSceneMesh != null)
                            {
                                selection.RenderSceneMesh.Dispose();
                                selection.RenderSceneMesh = null;
                            }
                        }

                        selection.UpdateRenderModel();
                    });
                }

                ContextActionManager.ExecuteAction(a);
            }
        }

        _lastUncommittedAction = null;
        _changingPropery = null;
        _changingObject = null;
    }


    /// <summary>
    ///     Handles changing a property's value for any number of entities.
    /// </summary>
    /// <param name="arrayindex">Index of the targeted value in an array of values.</param>
    /// <param name="classIndex">Index of the targeted class in an array of classes.</param>
    private void UpdateProperty(object prop, HashSet<Entity> selection, object newval,
        bool changed, bool committed, int arrayindex, int classIndex)
    {
        if (changed)
        {
            ChangePropertyMultiple(prop, selection, newval, ref committed, arrayindex, classIndex);
        }

        if (committed)
        {
            if (_lastUncommittedAction != null && ContextActionManager.PeekUndoAction() == _lastUncommittedAction)
            {
                if (_lastUncommittedAction is MultipleEntityPropertyChangeAction a)
                {
                    ContextActionManager.UndoAction();
                    a.UpdateRenderModel = true; // Update render model on commit execution, and update on undo/redo.
                    ContextActionManager.ExecuteAction(a);
                }

                _lastUncommittedAction = null;
                _changingPropery = null;
                _changingObject = null;
            }
        }
    }

    /// <summary>
    ///     Change a property's value for any number of entities.
    /// </summary>
    /// <param name="arrayindex">Index of the targeted value in an array of values.</param>
    /// <param name="classIndex">Index of the targeted class in an array of classes.</param>
    private void ChangePropertyMultiple(object prop, HashSet<Entity> ents, object newval, ref bool committed,
        int arrayindex = -1, int classIndex = -1)
    {
        if (prop == _changingPropery && _lastUncommittedAction != null &&
            ContextActionManager.PeekUndoAction() == _lastUncommittedAction)
        {
            ContextActionManager.UndoAction();
        }
        else
        {
            _lastUncommittedAction = null;
        }

        MultipleEntityPropertyChangeAction action;
        foreach (Entity selection in ents)
        {
            if (selection != null && _changingObject != null && !ents.SetEquals((HashSet<Entity>)_changingObject))
            {
                committed = true;
                return;
            }
        }

        action = new MultipleEntityPropertyChangeAction((PropertyInfo)prop, ents, newval, arrayindex, classIndex, false);
        ContextActionManager.ExecuteAction(action);

        _lastUncommittedAction = action;
        _changingPropery = prop;
        _changingObject = ents;
    }

    /// <summary>
    /// Overlays ImGui selectable over prop name text for use as a selectable.
    /// </summary>
    private static void PropContextRowOpener()
    {
        ImGui.Selectable("", false, ImGuiSelectableFlags.AllowItemOverlap);
        ImGui.SameLine();
        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
        {
            ImGui.OpenPopup("ModelPropContextMenu");
        }
    }

    /// <summary>
    /// Displays property context menu.
    /// </summary>
    private void DisplayPropContextMenu(ViewportSelection selection, PropertyInfo prop, object obj)
    {
        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
        {
            ImGui.OpenPopup("ModelPropContextMenu");
        }

        if (ImGui.BeginPopup("ModelPropContextMenu"))
        {
            // Info
            if (CFG.Current.MapEditor_Enable_Property_Info)
            {
                EditorDecorations.ImGui_DisplayPropertyInfo(prop);
                ImGui.Separator();
            }

            ImGui.EndPopup();
        }
    }

    private bool ParamRefRow(PropertyInfo propinfo, object val, ref object newObj)
    {
        /*
        List<ParamReference> attributes = propinfo.GetCustomAttributes<MSBParamReference>().ToList();
        if (attributes.Any())
        {
            List<ParamRef> refs = new();
            foreach (MSBParamReference att in attributes)
            {
                refs.Add(new ParamRef(att.ParamName));
            }

            ImGui.NextColumn();

            EditorDecorations.ParamRefText(refs, null);

            ImGui.NextColumn();
            EditorDecorations.ParamRefsSelectables(ParamBank.PrimaryBank, refs, null, val);
            EditorDecorations.ParamRefEnumQuickLink(ParamBank.PrimaryBank, val, refs, null, null, null);

            if (ImGui.BeginPopupContextItem($"{propinfo.Name}EnumContextMenu"))
            {
                EditorDecorations.ImGui_DisplayPropertyInfo(propinfo);

                var opened = EditorDecorations.ParamRefEnumContextMenuItems(ParamBank.PrimaryBank, val, ref newObj,
                    refs, null, null, null, null);
                ImGui.EndPopup();
                return opened;
            }
        }
        */

        return false;
    }

    private void PropEditorFlverLayout(Entity selection, FLVER2.BufferLayout layout)
    {
        foreach (FLVER.LayoutMember l in layout)
        {
            ImGui.Text(l.Semantic.ToString());
            ImGui.NextColumn();
            ImGui.Text(l.Type.ToString());
            ImGui.NextColumn();
        }
    }

    // Core of the property editor.
    private void PropEditorGeneric(ViewportSelection selection, HashSet<Entity> entSelection, object target = null, bool decorate = true, int classIndex = -1)
    {
        var scale = Smithbox.GetUIScale();
        Entity firstEnt = entSelection.First();
        var obj = target == null ? firstEnt.WrappedObject : target;
        Type type = obj.GetType();

        PropertyInfo[] properties = _propCache.GetCachedProperties(type);

        if (decorate)
        {
            ImGui.Columns(2);
            ImGui.Separator();
            ImGui.Text("Object Type");
            ImGui.NextColumn();
            ImGui.Text(type.Name);
            ImGui.NextColumn();
        }

        // Don't display this object to the user
        if (type == typeof(MapTransformNode))
        {
            return;
        }

        // Custom editors
        if (type == typeof(FLVER2.BufferLayout))
        {
            if (entSelection.Count() == 1)
            {
                PropEditorFlverLayout(firstEnt, (FLVER2.BufferLayout)obj);
            }
            else
            {
                ImGui.Text("Cannot edit multiples of this object at once.");
            }
        }
        else
        {
            var id = 0;
            foreach (PropertyInfo prop in properties)
            {
                if (!prop.CanWrite && !prop.PropertyType.IsArray)
                {
                    continue;
                }

                // Is Hidden
                if (FlverFormatBank.Bank.IsHiddenProperty(prop.Name))
                {
                    continue;
                }

                ImGui.PushID(id);
                ImGui.AlignTextToFramePadding();
                Type typ = prop.PropertyType;

                if (typ.IsArray)
                {
                    var a = (Array)prop.GetValue(obj);
                    for (var i = 0; i < a.Length; i++)
                    {
                        ImGui.PushID(i);

                        Type arrtyp = typ.GetElementType();
                        if (arrtyp.IsClass && arrtyp != typeof(string) && !arrtyp.IsArray)
                        {
                            var open = ImGui.TreeNodeEx($@"{GetFieldName(type, prop, selection)}[{i}]", ImGuiTreeNodeFlags.DefaultOpen);
                            ShowFieldHint(type, prop, selection);
                            ImGui.NextColumn();
                            ImGui.SetNextItemWidth(-1);
                            var o = a.GetValue(i);
                            ImGui.Text(o.GetType().Name);
                            ImGui.NextColumn();
                            if (open)
                            {
                                PropEditorGeneric(selection, entSelection, o, false, i);
                                ImGui.TreePop();
                            }

                            ImGui.PopID();
                        }
                        else
                        {
                            PropContextRowOpener();
                            ImGui.Text($@"{GetFieldName(type, prop, selection)}[{i}]");
                            ShowFieldHint(type, prop, selection);
                            ImGui.NextColumn();
                            ImGui.SetNextItemWidth(-1);
                            var oldval = a.GetValue(i);
                            object newval = null;

                            // Property Editor UI
                            (bool, bool) propEditResults =
                                PropertyRow(typ.GetElementType(), oldval, out newval, prop);
                            var changed = propEditResults.Item1;
                            var committed = propEditResults.Item2;
                            DisplayPropContextMenu(selection, prop, obj);
                            if (ImGui.IsItemActive() && !ImGui.IsWindowFocused())
                            {
                                ImGui.SetItemDefaultFocus();
                            }

                            if (ParamRefRow(prop, oldval, ref newval))
                            {
                                changed = true;
                                committed = true;
                            }

                            UpdateProperty(prop, entSelection, newval, changed, committed, i, classIndex);

                            ImGui.NextColumn();
                            ImGui.PopID();
                        }
                    }

                    ImGui.PopID();
                }
                else if (typ.IsGenericType && typ.GetGenericTypeDefinition() == typeof(List<>))
                {
                    var l = prop.GetValue(obj);
                    PropertyInfo itemprop = l.GetType().GetProperty("Item");
                    var count = (int)l.GetType().GetProperty("Count").GetValue(l);
                    for (var i = 0; i < count; i++)
                    {
                        ImGui.PushID(i);

                        Type arrtyp = typ.GetGenericArguments()[0];
                        if (arrtyp.IsClass && arrtyp != typeof(string) && !arrtyp.IsArray)
                        {
                            var open = ImGui.TreeNodeEx($@"{GetFieldName(type, prop, selection)}[{i}]", ImGuiTreeNodeFlags.DefaultOpen);
                            ShowFieldHint(type, prop, selection);
                            ImGui.NextColumn();
                            ImGui.SetNextItemWidth(-1);
                            var o = itemprop.GetValue(l, new object[] { i });
                            ImGui.Text(o.GetType().Name);
                            ImGui.NextColumn();
                            if (open)
                            {
                                PropEditorGeneric(selection, entSelection, o, false);
                                ImGui.TreePop();
                            }

                            ImGui.PopID();
                        }
                        else
                        {
                            PropContextRowOpener();
                            ImGui.Text($@"{GetFieldName(type, prop, selection)}[{i}]");
                            ShowFieldHint(type, prop, selection);
                            ImGui.NextColumn();
                            ImGui.SetNextItemWidth(-1);
                            var oldval = itemprop.GetValue(l, new object[] { i });
                            object newval = null;

                            // Property Editor UI
                            (bool, bool) propEditResults = PropertyRow(arrtyp, oldval, out newval, prop);
                            var changed = propEditResults.Item1;
                            var committed = propEditResults.Item2;
                            DisplayPropContextMenu(selection, prop, obj);
                            if (ImGui.IsItemActive() && !ImGui.IsWindowFocused())
                            {
                                ImGui.SetItemDefaultFocus();
                            }

                            if (ParamRefRow(prop, oldval, ref newval))
                            {
                                changed = true;
                                committed = true;
                            }

                            UpdateProperty(prop, entSelection, newval, changed, committed, i, classIndex);

                            ImGui.NextColumn();
                            ImGui.PopID();
                        }
                    }

                    ImGui.PopID();
                }
                else if (typ.IsClass && typ == typeof(MSB.Shape))
                {
                    var open = ImGui.TreeNodeEx(GetFieldName(type, prop, selection), ImGuiTreeNodeFlags.DefaultOpen);
                    ShowFieldHint(type, prop, selection);
                    ImGui.NextColumn();
                    ImGui.SetNextItemWidth(-1);
                    var o = prop.GetValue(obj);
                    var shapetype = Enum.Parse<RegionShape>(o.GetType().Name);
                    var shap = (int)shapetype;

                    if (entSelection.Count == 1)
                    {
                        if (ImGui.Combo("##shapecombo", ref shap, _regionShapes, _regionShapes.Length))
                        {
                            MSB.Shape newshape;
                            switch ((RegionShape)shap)
                            {
                                case RegionShape.Box:
                                    newshape = new MSB.Shape.Box();
                                    break;
                                case RegionShape.Point:
                                    newshape = new MSB.Shape.Point();
                                    break;
                                case RegionShape.Cylinder:
                                    newshape = new MSB.Shape.Cylinder();
                                    break;
                                case RegionShape.Sphere:
                                    newshape = new MSB.Shape.Sphere();
                                    break;
                                case RegionShape.Composite:
                                    newshape = new MSB.Shape.Composite();
                                    break;
                                case RegionShape.Rectangle:
                                    newshape = new MSB.Shape.Rectangle();
                                    break;
                                case RegionShape.Circle:
                                    newshape = new MSB.Shape.Circle();
                                    break;
                                default:
                                    throw new Exception("Invalid shape");
                            }

                            MapEditor.PropertiesChangedAction action = new(prop, obj, newshape);
                            action.SetPostExecutionAction(undo =>
                            {
                                var selected = false;
                                if (firstEnt.RenderSceneMesh != null)
                                {
                                    selected = firstEnt.RenderSceneMesh.RenderSelectionOutline;
                                    firstEnt.RenderSceneMesh.Dispose();
                                    firstEnt.RenderSceneMesh = null;
                                }

                                firstEnt.UpdateRenderModel();
                                firstEnt.RenderSceneMesh.RenderSelectionOutline = selected;
                            });
                            ContextActionManager.ExecuteAction(action);
                        }
                    }

                    ImGui.NextColumn();
                    if (open)
                    {
                        PropEditorGeneric(selection, entSelection, o, false);
                        ImGui.TreePop();
                    }

                    ImGui.PopID();
                }
                else if (typ == typeof(BTL.LightType))
                {
                    var open = ImGui.TreeNodeEx(GetFieldName(type, prop, selection), ImGuiTreeNodeFlags.DefaultOpen);
                    ShowFieldHint(type, prop, selection);
                    ImGui.NextColumn();
                    ImGui.SetNextItemWidth(-1);
                    var o = prop.GetValue(obj);
                    var enumTypes = Enum.Parse<LightType>(o.ToString());
                    var thisType = (int)enumTypes;
                    if (ImGui.Combo("##lightTypecombo", ref thisType, _lightTypes, _lightTypes.Length))
                    {
                        BTL.LightType newLight;
                        switch ((LightType)thisType)
                        {
                            case LightType.Directional:
                                newLight = BTL.LightType.Directional;
                                break;
                            case LightType.Point:
                                newLight = BTL.LightType.Point;
                                break;
                            case LightType.Spot:
                                newLight = BTL.LightType.Spot;
                                break;
                            default:
                                throw new Exception("Invalid BTL LightType");
                        }

                        MapEditor.PropertiesChangedAction action = new(prop, obj, newLight);
                        action.SetPostExecutionAction(undo =>
                        {
                            var selected = false;
                            if (firstEnt.RenderSceneMesh != null)
                            {
                                selected = firstEnt.RenderSceneMesh.RenderSelectionOutline;
                                firstEnt.RenderSceneMesh.Dispose();
                                firstEnt.RenderSceneMesh = null;
                            }

                            firstEnt.UpdateRenderModel();
                            firstEnt.RenderSceneMesh.RenderSelectionOutline = selected;
                        });
                        ContextActionManager.ExecuteAction(action);

                        ContextActionManager.ExecuteAction(action);
                    }

                    ImGui.NextColumn();
                    if (open)
                    {
                        PropEditorGeneric(selection, entSelection, o, false);
                        ImGui.TreePop();
                    }

                    ImGui.PopID();
                }
                else if (typ.IsClass && typ != typeof(string) && !typ.IsArray)
                {
                    var open = ImGui.TreeNodeEx(GetFieldName(type, prop, selection), ImGuiTreeNodeFlags.DefaultOpen);
                    ShowFieldHint(type, prop, selection);
                    ImGui.NextColumn();
                    ImGui.SetNextItemWidth(-1);
                    var o = prop.GetValue(obj);
                    ImGui.Text(o.GetType().Name);
                    ImGui.NextColumn();
                    if (open)
                    {
                        PropEditorGeneric(selection, entSelection, o, false);
                        ImGui.TreePop();
                    }

                    ImGui.PopID();
                }
                else
                {
                    PropContextRowOpener();
                    ImGui.Text(GetFieldName(type, prop, selection));
                    ShowFieldHint(type, prop, selection);
                    ImGui.NextColumn();
                    ImGui.SetNextItemWidth(-1);
                    var oldval = prop.GetValue(obj);
                    object newval = null;

                    // Property Editor UI
                    (bool, bool) propEditResults = PropertyRow(typ, oldval, out newval, prop);
                    var changed = propEditResults.Item1;
                    var committed = propEditResults.Item2;
                    DisplayPropContextMenu(selection, prop, obj);
                    if (ImGui.IsItemActive() && !ImGui.IsWindowFocused())
                    {
                        ImGui.SetItemDefaultFocus();
                    }

                    if (ParamRefRow(prop, oldval, ref newval))
                    {
                        changed = true;
                        committed = true;
                    }

                    UpdateProperty(prop, entSelection, newval, changed, committed, -1, classIndex);

                    ImGui.NextColumn();
                    ImGui.PopID();
                }

                id++;
            }
        }

        bool editAlias = false;

        var refID = 0; // ID for ImGui distinction
        if (decorate && entSelection.Count == 1)
        {
            
        }
    }

    public string GetFieldName(Type classType, PropertyInfo prop, ViewportSelection sel)
    {
        string name = prop.Name;

        if (CFG.Current.ModelEditor_Enable_Commmunity_Names)
        {
            Entity _selected = sel.GetFilteredSelection<Entity>().First();

            name = FlverFormatBank.Bank.GetReferenceName(name, name);
        }

        return name;
    }

    public void ShowFieldHint(Type classType, PropertyInfo prop, ViewportSelection sel)
    {
        string name = prop.Name;

        if (CFG.Current.ModelEditor_Enable_Commmunity_Hints)
        {
            Entity _selected = sel.GetFilteredSelection<Entity>().First();

            var desc = FlverFormatBank.Bank.GetReferenceDescription(name);

            ImguiUtils.ShowHoverTooltip(desc);
        }
    }

    public void OnGui(ViewportSelection selection, string id, float w, float h)
    {
        var scale = Smithbox.GetUIScale();
        HashSet<Entity> entSelection = selection.GetFilteredSelection<Entity>();

        ImGui.PushStyleColor(ImGuiCol.ChildBg, CFG.Current.ImGui_ChildBg);
        ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(350, h - 80) * scale, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowPos(new Vector2(w - 370, 20) * scale, ImGuiCond.FirstUseEver);
        ImGui.Begin($@"Properties##{id}");
        ImGui.BeginChild("propedit");

        if (entSelection.Count > 1)
        {
            Entity firstEnt = entSelection.First();

            if (entSelection.All(e => e.WrappedObject.GetType() == firstEnt.WrappedObject.GetType()))
            {
                if (firstEnt.WrappedObject is Param.Row prow || firstEnt.WrappedObject is MergedParamRow)
                {
                    ImGui.Text("Cannot edit multiples of this object at once.");
                    ImGui.EndChild();
                    ImGui.End();
                    ImGui.PopStyleColor();
                    return;
                }

                ImGui.TextColored(new Vector4(0.5f, 1.0f, 0.0f, 1.0f),
                    " Editing Multiple Objects.\n Changes will be applied to all selected objects.");
                ImGui.Separator();
                ImGui.PushStyleColor(ImGuiCol.FrameBg, CFG.Current.ImGui_MultipleInput_Background);
                ImGui.BeginChild("MSB_EditingMultipleObjsChild");

                PropEditorGeneric(selection, entSelection);

                ImGui.PopStyleColor();
                ImGui.EndChild();
            }
            else
            {
                ImGui.Text("Not all selected objects are the same type.");
                ImGui.EndChild();
                ImGui.End();
                ImGui.PopStyleColor();
                return;
            }
        }
        else if (entSelection.Any())
        {
            Entity firstEnt = entSelection.First();

            ImGui.Text($" Model: {firstEnt.Container.Name}");
            if (firstEnt.WrappedObject == null)
            {
                ImGui.Text("Select a model object to edit its properties.");
                ImGui.EndChild();
                ImGui.End();
                ImGui.PopStyleColor();
                return;
            }

            PropEditorGeneric(selection, entSelection);
        }
        else
        {
            ImGui.Text("Select a map object to edit its properties.");
        }

        ImGui.EndChild();
        ImGui.End();
        ImGui.PopStyleColor(2);
    }
}
