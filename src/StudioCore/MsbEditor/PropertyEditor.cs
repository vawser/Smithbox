using Andre.Formats;
using ImGuiNET;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Editor;
using StudioCore.ParamEditor;
using StudioCore.Scene;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reflection;

namespace StudioCore.MsbEditor;

public class PropertyEditor
{
    private readonly string[] _lightTypes = { "Spot", "Directional", "Point" };

    private readonly PropertyCache _propCache;

    private readonly string[] _regionShapes =
    {
        "Point", "Sphere", "Cylinder", "Box", "Composite", "Rectangle", "Circle"
    };

    private object _changingObject;
    private object _changingPropery;
    private Action _lastUncommittedAction;

    public ActionManager ContextActionManager;
    public PropertyInfo RequestedSearchProperty = null;

    public PropertyEditor(ActionManager manager, PropertyCache propCache)
    {
        ContextActionManager = manager;
        _propCache = propCache;
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
            if (ImGui.InputInt("##value", ref val))
            {
                newval = val;
                isChanged = true;
            }
        }
        else if (typ == typeof(uint))
        {
            var val = (uint)oldval;
            var strval = $@"{val}";
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
        else if (typ == typeof(short))
        {
            int val = (short)oldval;
            if (ImGui.InputInt("##value", ref val))
            {
                newval = (short)val;
                isChanged = true;
            }
        }
        else if (typ == typeof(ushort))
        {
            var val = (ushort)oldval;
            var strval = $@"{val}";
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
        else if (typ == typeof(sbyte))
        {
            int val = (sbyte)oldval;
            if (ImGui.InputInt("##value", ref val))
            {
                newval = (sbyte)val;
                isChanged = true;
            }
        }
        else if (typ == typeof(byte))
        {
            var val = (byte)oldval;
            var strval = $@"{val}";
            if (ImGui.InputText("##value", ref strval, 3))
            {
                var res = byte.TryParse(strval, out val);
                if (res)
                {
                    newval = val;
                    isChanged = true;
                }
            }
            /*
            // TODO: Set Next Unique Value
            // (needs prop search to scan through structs)
            if (obj != null && ImGui.BeginPopupContextItem(propname))
            {
                if (ImGui.Selectable("Set Next Unique Value"))
                {
                    newval = obj.Container.GetNextUnique(propname, val);
                    _forceCommit = true;
                    ImGui.EndPopup();
                    edited = true;
                }
                ImGui.EndPopup();
            }
            */
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
                TaskLogs.AddLog(
                    $"Color property in \"{prop.DeclaringType}\" does not declare if it supports Alpha. Alpha will be exposed by default",
                    LogLevel.Warning, TaskLogs.LogPriority.Low);

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
            PropertiesChangedAction action;
            if (arrayindex != -1)
            {
                action = new PropertiesChangedAction((PropertyInfo)prop, arrayindex, obj, newval);
            }
            else
            {
                action = new PropertiesChangedAction((PropertyInfo)prop, obj, newval);
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
            if (_lastUncommittedAction is PropertiesChangedAction a)
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

        action = new MultipleEntityPropertyChangeAction((PropertyInfo)prop, ents, newval, arrayindex, classIndex);
        ContextActionManager.ExecuteAction(action);

        _lastUncommittedAction = action;
        _changingPropery = prop;
        _changingObject = ents;
    }

    private void PropEditorParamRow(Entity selection)
    {
        IReadOnlyList<Param.Cell> cells = new List<Param.Cell>();
        if (selection.WrappedObject is Param.Row row)
        {
            cells = row.Cells;
        }
        else if (selection.WrappedObject is MergedParamRow mrow)
        {
            cells = mrow.CellHandles;
        }

        ImGui.Columns(2);
        ImGui.Separator();
        var id = 0;

        // This should be rewritten somehow it's super ugly
        PropertyInfo nameProp = selection.WrappedObject.GetType().GetProperty("Name");
        PropertyInfo idProp = selection.WrappedObject.GetType().GetProperty("ID");
        PropEditorPropInfoRow(selection.WrappedObject, nameProp, "Name", ref id, selection);
        PropEditorPropInfoRow(selection.WrappedObject, idProp, "ID", ref id, selection);

        foreach (Param.Cell cell in cells)
        {
            PropEditorPropCellRow(cell, ref id, selection);
        }

        ImGui.Columns(1);
    }

    public void PropEditorParamRow(Param.Row row)
    {
        ImGui.Columns(2);
        ImGui.Separator();
        var id = 0;

        // This should be rewritten somehow it's super ugly
        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.8f, 0.8f, 0.8f, 1.0f));
        PropertyInfo nameProp = row.GetType().GetProperty("Name");
        PropertyInfo idProp = row.GetType().GetProperty("ID");
        PropEditorPropInfoRow(row, nameProp, "Name", ref id, null);
        PropEditorPropInfoRow(row, idProp, "ID", ref id, null);
        ImGui.PopStyleColor();

        foreach (Param.Column cell in row.Columns)
        {
            PropEditorPropCellRow(row[cell], ref id, null);
        }

        ImGui.Columns(1);
    }

    // Many parameter options, which may be simplified.
    private void PropEditorPropInfoRow(object rowOrWrappedObject, PropertyInfo prop, string visualName, ref int id,
        Entity nullableSelection)
    {
        PropEditorPropRow(prop.GetValue(rowOrWrappedObject), ref id, visualName, prop.PropertyType, null, null,
            prop, rowOrWrappedObject, nullableSelection);
    }

    private void PropEditorPropCellRow(Param.Cell cell, ref int id, Entity nullableSelection)
    {
        PropEditorPropRow(cell.Value, ref id, cell.Def.InternalName, cell.Value.GetType(), null,
            cell.Def.InternalName, cell.GetType().GetProperty("Value"), cell, nullableSelection);
    }

    private void PropEditorPropRow(object oldval, ref int id, string visualName, Type propType,
        Entity nullableEntity, string nullableName, PropertyInfo proprow, object paramRowOrCell,
        Entity nullableSelection)
    {
        ImGui.PushID(id);
        ImGui.AlignTextToFramePadding();
        ImGui.Text(visualName);
        ImGui.NextColumn();
        ImGui.SetNextItemWidth(-1);

        object newval;
        // Property Editor UI
        (bool, bool) propEditResults = PropertyRow(propType, oldval, out newval, proprow);
        var changed = propEditResults.Item1;
        var committed = propEditResults.Item2;
        UpdateProperty(proprow, nullableSelection, paramRowOrCell, newval, changed, committed);
        ImGui.NextColumn();
        ImGui.PopID();
        id++;
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
            ImGui.OpenPopup("MsbPropContextMenu");
        }
    }

    /// <summary>
    /// Displays property context menu.
    /// </summary>
    private void DisplayPropContextMenu(PropertyInfo prop, object obj)
    {
        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
        {
            ImGui.OpenPopup("MsbPropContextMenu");
        }

        if (ImGui.BeginPopup("MsbPropContextMenu"))
        {
            EditorDecorations.ImGui_DisplayPropertyInfo(prop);

            if (ImGui.Selectable(@"Search##PropSearch"))
            {
                RequestedSearchProperty = prop;
                EditorCommandQueue.AddCommand($@"map/propsearch/{prop.Name}");
            }

            ImGui.EndPopup();
        }
    }

    private bool ParamRefRow(PropertyInfo propinfo, object val, ref object newObj)
    {
        List<MSBParamReference> attributes = propinfo.GetCustomAttributes<MSBParamReference>().ToList();
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

    private void PropEditorGeneric(Selection selection, HashSet<Entity> entSelection, object target = null,
        bool decorate = true, int classIndex = -1)
    {
        var scale = MapStudioNew.GetUIScale();
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

                if (prop.GetCustomAttribute<HideProperty>() != null)
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
                            var open = ImGui.TreeNodeEx($@"{prop.Name}[{i}]", ImGuiTreeNodeFlags.DefaultOpen);
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
                            ImGui.Text($@"{prop.Name}[{i}]");
                            ImGui.NextColumn();
                            ImGui.SetNextItemWidth(-1);
                            var oldval = a.GetValue(i);
                            object newval = null;

                            // Property Editor UI
                            (bool, bool) propEditResults =
                                PropertyRow(typ.GetElementType(), oldval, out newval, prop);
                            var changed = propEditResults.Item1;
                            var committed = propEditResults.Item2;
                            DisplayPropContextMenu(prop, obj);
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
                            var open = ImGui.TreeNodeEx($@"{prop.Name}[{i}]", ImGuiTreeNodeFlags.DefaultOpen);
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
                            ImGui.Text($@"{prop.Name}[{i}]");
                            ImGui.NextColumn();
                            ImGui.SetNextItemWidth(-1);
                            var oldval = itemprop.GetValue(l, new object[] { i });
                            object newval = null;

                            // Property Editor UI
                            (bool, bool) propEditResults = PropertyRow(arrtyp, oldval, out newval, prop);
                            var changed = propEditResults.Item1;
                            var committed = propEditResults.Item2;
                            DisplayPropContextMenu(prop, obj);
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
                    var open = ImGui.TreeNodeEx(prop.Name, ImGuiTreeNodeFlags.DefaultOpen);
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

                            PropertiesChangedAction action = new(prop, obj, newshape);
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
                    var open = ImGui.TreeNodeEx(prop.Name, ImGuiTreeNodeFlags.DefaultOpen);
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

                        PropertiesChangedAction action = new(prop, obj, newLight);
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
                    var open = ImGui.TreeNodeEx(prop.Name, ImGuiTreeNodeFlags.DefaultOpen);
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
                    ImGui.Text(prop.Name);
                    ImGui.NextColumn();
                    ImGui.SetNextItemWidth(-1);
                    var oldval = prop.GetValue(obj);
                    object newval = null;

                    // Property Editor UI
                    (bool, bool) propEditResults = PropertyRow(typ, oldval, out newval, prop);
                    var changed = propEditResults.Item1;
                    var committed = propEditResults.Item2;
                    DisplayPropContextMenu(prop, obj);
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

        var refID = 0; // ID for ImGui distinction
        if (decorate && entSelection.Count == 1)
        {
            ImGui.Columns(1);
            if (firstEnt.References != null)
            {
                ImGui.NewLine();
                ImGui.Text("References: ");
                ImGui.Indent(10 * scale);
                foreach (KeyValuePair<string, object[]> m in firstEnt.References)
                {
                    foreach (var n in m.Value)
                    {
                        if (n is Entity e)
                        {
                            var nameWithType = e.PrettyName.Insert(2, e.WrappedObject.GetType().Name + " - ");
                            if (ImGui.Button(nameWithType + "##MSBRefTo" + refID))
                            {
                                selection.ClearSelection();
                                selection.AddSelection(e);
                            }
                        }
                        else if (n is ObjectContainerReference r)
                        {
                            // Try to select the map's RootObject if it is loaded, and the reference otherwise.
                            // It's not the end of the world if we choose the wrong one, as SceneTree can use either,
                            // but only the RootObject has the TransformNode and Viewport integration.
                            var mapid = r.Name;
                            var prettyName = $"{ForkAwesome.Cube} {mapid}";
                            if (AliasBank.MapNames != null &&
                                AliasBank.MapNames.TryGetValue(mapid, out var metaName))
                            {
                                prettyName += $" <{metaName.Replace("--", "")}>";
                            }

                            if (ImGui.Button(prettyName + "##MSBRefTo" + refID))
                            {
                                ISelectable rootTarget = r.GetSelectionTarget();
                                selection.ClearSelection();
                                selection.AddSelection(rootTarget);
                                // For this type of connection, jump to the object in the list to actually load the map
                                // (is this desirable in other cases?). It could be possible to have a Load context menu
                                // here, but that should be shared with SceneTree.
                                selection.GotoTreeTarget = rootTarget;
                            }

                            if (ImGui.BeginPopupContextItem())
                            {
                                Map map = firstEnt.Universe.GetLoadedMap(mapid);
                                if (map == null)
                                {
                                    if (ImGui.Selectable("Load Map"))
                                    {
                                        firstEnt.Universe.LoadMap(mapid);
                                    }
                                }
                                else
                                {
                                    if (ImGui.Selectable("Unload Map"))
                                    {
                                        firstEnt.Universe.UnloadContainer(map);
                                    }
                                }

                                ImGui.EndPopup();
                            }
                        }

                        refID++;
                    }
                }
            }

            ImGui.Unindent(10 * scale);
            ImGui.NewLine();
            ImGui.Text("Objects referencing this object:");
            ImGui.Indent(10 * scale);
            foreach (Entity m in firstEnt.GetReferencingObjects())
            {
                var nameWithType = m.PrettyName.Insert(2, m.WrappedObject.GetType().Name + " - ");
                if (ImGui.Button(nameWithType + "##MSBRefBy" + refID))
                {
                    selection.ClearSelection();
                    selection.AddSelection(m);
                }

                refID++;
            }

            ImGui.Unindent(10 * scale);
        }
    }

    public void OnGui(Selection selection, string id, float w, float h)
    {
        var scale = MapStudioNew.GetUIScale();
        HashSet<Entity> entSelection = selection.GetFilteredSelection<Entity>();

        ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(0.145f, 0.145f, 0.149f, 1.0f));
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
                ImGui.PushStyleColor(ImGuiCol.FrameBg, new Vector4(0.0f, 0.5f, 0.0f, 0.1f));
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
            ImGui.Text($" Map: {firstEnt.Container.Name}");
            if (firstEnt.WrappedObject == null)
            {
                ImGui.Text("Select a map object to edit its properties.");
                ImGui.EndChild();
                ImGui.End();
                ImGui.PopStyleColor();
                return;
            }

            if (firstEnt.WrappedObject is Param.Row prow || firstEnt.WrappedObject is MergedParamRow)
            {
                PropEditorParamRow(firstEnt);
            }
            else
            {
                PropEditorGeneric(selection, entSelection);
            }
        }
        else
        {
            ImGui.Text("Select a map object to edit its properties.");
        }

        ImGui.EndChild();
        ImGui.End();
        ImGui.PopStyleColor();
    }

    internal enum RegionShape
    {
        Point,
        Sphere,
        Cylinder,
        Box,
        Composite,
        Rectangle,
        Circle
    }

    internal enum LightType
    {
        Spot,
        Directional,
        Point
    }
}
