﻿using Andre.Formats;
using ImGuiNET;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using SoulsFormats.KF4;
using StudioCore.Editor;
using StudioCore.Editors.MapEditor.Enums;
using StudioCore.Editors.ModelEditor.Utils;
using StudioCore.Editors.ParamEditor;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.Tasks;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reflection;
using static HKLib.hk2018.hkaiUserEdgeUtils;

namespace StudioCore.Editors.MapEditor.PropertyEditor;

public class MapPropertyEditor
{
    private readonly string[] _lightTypes = { "Spot", "Directional", "Point" };

    private readonly MapPropertyCache _propCache;

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

    private MapEditorScreen Screen;

    public bool Focus = false;

    private string msbFieldSearch = "";

    public MapPropertyEditor(MapEditorScreen screen, ViewportActionManager manager, MapPropertyCache propCache, IViewport viewport)
    {
        Screen = screen;
        ContextActionManager = manager;
        _propCache = propCache;
        _viewport = viewport;
    }

    public void OnGui(ViewportSelection selection, string id, float w, float h)
    {
        var scale = DPI.GetUIScale();
        HashSet<Entity> entSelection = selection.GetFilteredSelection<Entity>();

        if (!UI.Current.Interface_MapEditor_Properties)
            return;

        ImGui.PushStyleColor(ImGuiCol.ChildBg, UI.Current.ImGui_ChildBg);
        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(350, h - 80) * scale, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowPos(new Vector2(w - 370, 20) * scale, ImGuiCond.FirstUseEver);
        ImGui.Begin($@"Properties##{id}");
        Smithbox.EditorHandler.MapEditor.Selection.SwitchWindowContext(MapEditorContext.MapObjectProperties);

        // Header
        ImGui.SetNextItemWidth(ImGui.GetColumnWidth() * 0.75f);
        ImGui.AlignTextToFramePadding();
        ImGui.InputText("Search##msbFieldSearch", ref msbFieldSearch, 255);
        ImGui.Separator();

        // Properties
        ImGui.BeginChild("propedit");
        Smithbox.EditorHandler.MapEditor.Selection.SwitchWindowContext(MapEditorContext.MapObjectProperties);

        if (Screen.Universe.postLoad && entSelection.Count > 1)
        {
            Entity firstEnt = entSelection.First();
            if (firstEnt.WrappedObject is Param.Row prow || firstEnt.WrappedObject is MergedParamRow)
            {
                ImGui.Text("Cannot edit multiples of this object at once.");
                ImGui.EndChild();
                ImGui.End();
                ImGui.PopStyleColor(2);
                return;
            }

            ImGui.TextColored(new Vector4(0.5f, 1.0f, 0.0f, 1.0f),
                " Editing Multiple Objects.\n Changes will be applied to all selected objects.");
            ImGui.Separator();
            ImGui.PushStyleColor(ImGuiCol.FrameBg, UI.Current.ImGui_MultipleInput_Background);
            ImGui.BeginChild("MSB_EditingMultipleObjsChild");
            Smithbox.EditorHandler.MapEditor.Selection.SwitchWindowContext(MapEditorContext.MapObjectProperties);
            PropEditorSelectedEntities(selection);
            ImGui.PopStyleColor();
            ImGui.EndChild();
        }
        else if (Screen.Universe.postLoad && entSelection.Any())
        {
            Entity firstEnt = entSelection.First();
            //ImGui.Text($" Map: {firstEnt.Container.Name}");
            if (firstEnt.WrappedObject == null)
            {
                ImGui.Text("Select a map object to edit its properties.");
                ImGui.EndChild();
                ImGui.End();
                ImGui.PopStyleColor(2);
                return;
            }

            if (firstEnt.WrappedObject is Param.Row prow || firstEnt.WrappedObject is MergedParamRow)
            {
                PropEditorParamRow(firstEnt);
            }
            else
            {
                PropEditorSelectedEntities(selection);
            }
        }
        else if (!Screen.Universe.postLoad)
        {
            ImGui.Text("");
        }
        else
        {
            ImGui.Text("Select a map object to edit its properties.");
        }

        ImGui.EndChild();
        ImGui.End();
        ImGui.PopStyleColor(2);
    }

    private void PropEditorParamRow(Entity selection)
    {
        var rowID = -1;

        IReadOnlyList<Param.Cell> cells = new List<Param.Cell>();
        if (selection.WrappedObject is Param.Row row)
        {
            cells = row.Cells;
            rowID = row.ID;
        }
        else if (selection.WrappedObject is MergedParamRow mrow)
        {
            cells = mrow.CellHandles;
            rowID = mrow.ID;
        }

        ImGui.Columns(2);
        ImGui.Separator();
        var id = 0;

        // This should be rewritten somehow it's super ugly
        PropertyInfo nameProp = selection.WrappedObject.GetType().GetProperty("Name");
        PropertyInfo idProp = selection.WrappedObject.GetType().GetProperty("ID");
        PropEditorPropInfoRow(selection.WrappedObject, nameProp, "Name", ref id, selection);
        PropEditorPropInfoRow(selection.WrappedObject, idProp, "ID", ref id, selection);

        MsbFieldMetaData meta = null;


        foreach (Param.Cell cell in cells)
        {
            if (selection.WrappedObject is MergedParamRow)
            {
                var mergedRow = (MergedParamRow)selection.WrappedObject;    

                meta = MsbMeta.GetParamFieldMeta(cell.Def.InternalName, $"Param_{mergedRow.MetaName}");
            }
            if (selection.WrappedObject is Param.Row)
            {
                var paramRow = (Param.Row)selection.WrappedObject;

                meta = MsbMeta.GetParamFieldMeta(cell.Def.InternalName, $"Param_{paramRow.Def.ParamType}");
            }



            PropEditorPropCellRow(meta, cell, ref id, selection, rowID);
        }

        ImGui.Columns(1);
    }

    public void PropEditorParamRow(Param.Row row)
    {
        ImGui.Columns(2);
        ImGui.Separator();
        var id = 0;

        // This should be rewritten somehow it's super ugly
        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_ParamRow_Text);
        PropertyInfo nameProp = row.GetType().GetProperty("Name");
        PropertyInfo idProp = row.GetType().GetProperty("ID");
        PropEditorPropInfoRow(row, nameProp, "Name", ref id, null);
        PropEditorPropInfoRow(row, idProp, "ID", ref id, null);
        ImGui.PopStyleColor();

        ImGui.Separator();

        foreach (Param.Column cell in row.Columns)
        {
            var meta = MsbMeta.GetParamFieldMeta(cell.Def.InternalName, cell.Def.Parent.ParamType);

            PropEditorPropCellRow(meta, row[cell], ref id, null, row.ID);
        }

        ImGui.Columns(1);
    }

    // Many parameter options, which may be simplified.
    private void PropEditorPropInfoRow(object rowOrWrappedObject, PropertyInfo prop, string visualName, ref int id,
        Entity nullableSelection)
    {
        PropEditorPropRow(null, prop.GetValue(rowOrWrappedObject), ref id, visualName, prop.PropertyType, null, null,
            prop, rowOrWrappedObject, nullableSelection, -1);
    }

    private void PropEditorPropCellRow(MsbFieldMetaData meta, Param.Cell cell, ref int id, Entity nullableSelection, int rowID)
    {
        var filterTerm = msbFieldSearch.ToLower();
        if (msbFieldSearch != "")
        {
            if (!cell.Def.InternalName.ToLower().Contains(filterTerm) && (meta != null && !meta.AltName.ToLower().Contains(filterTerm)))
            {
                return;
            }
        }

        PropEditorPropRow(meta, cell.Value, ref id, cell.Def.InternalName, cell.Value.GetType(), null,
            cell.Def.InternalName, cell.GetType().GetProperty("Value"), cell, nullableSelection, rowID);
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
    /// Displays property context menu (for DS2 map params)
    /// </summary>
    private void DisplayParamContextMenu(MsbFieldMetaData meta, PropertyInfo propinfo, object val, ref object newObj, string fieldName)
    {
        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
        {
            ImGui.OpenPopup("MsbPropContextMenu");
        }

        if (ImGui.BeginPopup("MsbPropContextMenu"))
        {
            if (ImGui.Selectable(@"Copy Property Name##CopyPropName"))
            {
                PlatformUtils.Instance.SetClipboardText(fieldName);
            }

            ImGui.EndPopup();
        }
    }

    /// <summary>
    /// Displays property context menu.
    /// </summary>
    private void DisplayPropContextMenu(MsbFieldMetaData meta, ViewportSelection selection, PropertyInfo prop, object obj, int arrayIndex)
    {
        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
        {
            ImGui.OpenPopup("MsbPropContextMenu");
        }

        if (ImGui.BeginPopup("MsbPropContextMenu"))
        {
            // Info
            if (CFG.Current.MapEditor_Enable_Property_Info)
            {
                EditorDecorations.ImGui_DisplayPropertyInfo(prop);
                ImGui.Separator();
            }

            // Actions
            if (ImGui.Selectable(@"Search##PropSearch"))
            {
                Screen.ToolWindow.FocusLocalPropertySearch = true;
                RequestedSearchProperty = prop;
                EditorCommandQueue.AddCommand($@"map/propsearch/{prop.Name}");
            }
            if (ImGui.Selectable(@"Copy Property Name##CopyPropName"))
            {
                PlatformUtils.Instance.SetClipboardText(prop.Name);
            }

            if (Screen.MapQueryHandler.IsOpen)
            {
                if (ImGui.Selectable("Add to Property Filter"))
                {
                    Screen.MapQueryHandler.AddPropertyFilterInput(prop, arrayIndex);
                }
            }

            // Position - Copy/Paste
            if (meta != null && meta.PositionProperty)
            {
                if (ImGui.Selectable(@"Copy##CopyPosition"))
                {
                    PropAction_Position.CopyCurrentPosition(prop, obj);
                }
                if (ImGui.Selectable(@"Paste##PastePosition"))
                {
                    PropAction_Position.PasteSavedPosition(selection);
                }
            }

            // Rotation - Copy/Paste
            if (meta != null && meta.RotationProperty)
            {
                if (ImGui.Selectable(@"Copy##CopyRotation"))
                {
                    PropAction_Rotation.CopyCurrentRotation(prop, obj);
                }
                if (ImGui.Selectable(@"Paste##PasteRotation"))
                {
                    PropAction_Rotation.PasteSavedRotation(selection);
                }
            }

            // Scale - Copy/Paste
            if (meta != null && meta.ScaleProperty)
            {
                if (ImGui.Selectable(@"Copy##CopyScale"))
                {
                    PropAction_Scale.CopyCurrentScale(prop, obj);
                }
                if (ImGui.Selectable(@"Paste##PasteScale"))
                {
                    PropAction_Scale.PasteSavedScale(selection);
                }
            }

            ImGui.EndPopup();
        }
    }

    /// <summary>
    /// Param MSB Object Field
    /// </summary>
    private void PropEditorPropRow(MsbFieldMetaData meta, object oldval, ref int id, string visualName, Type propType,
        Entity nullableEntity, string nullableName, PropertyInfo proprow, object paramRowOrCell,
        Entity nullableSelection, int rowID)
    {
        ImGui.PushID(id);
        ImGui.AlignTextToFramePadding();

        PropContextRowOpener();

        var displayedName = visualName;

        // Name
        if (meta != null && meta.AltName != "")
        {
            displayedName = meta.AltName;
        }

        ImGui.Text(displayedName);

        // Description
        if (meta != null && meta.Wiki != "")
        {
            UIHelper.ShowHoverTooltip(meta.Wiki);
        }

        ImGui.NextColumn();
        ImGui.SetNextItemWidth(-1);

        object newval;

        (bool, bool) propEditResults = PropertyRow(meta, propType, oldval, out newval, proprow, null);

        var changed = propEditResults.Item1;
        var committed = propEditResults.Item2;

        DisplayParamContextMenu(meta, proprow, oldval, ref newval, visualName);

        if (ImGui.IsItemActive() && !ImGui.IsWindowFocused())
        {
            ImGui.SetItemDefaultFocus();
        }

        // Param References
        if (MapEditorDecorations.ParamRefRow(meta, proprow, oldval, ref newval))
        {
            changed = true;
            committed = true;
        }

        // Text References
        if (MapEditorDecorations.FmgRefRow(meta, proprow, oldval, ref newval))
        {
            changed = true;
            committed = true;
        }

        // Enum List
        if (MapEditorDecorations.GenericEnumRow(meta, proprow, oldval, ref newval))
        {
            changed = true;
            committed = true;
        }

        // Alias List
        if (MapEditorDecorations.AliasEnumRow(meta, proprow, oldval, ref newval))
        {
            changed = true;
            committed = true;
        }

        // DS2: Spawn State List
        if (MapEditorDecorations.SpawnStateListRow(meta, proprow, oldval, ref newval, rowID))
        {
            changed = true;
            committed = true;
        }

        UpdateProperty(proprow, nullableSelection, paramRowOrCell, newval, changed, committed);
        ImGui.NextColumn();
        ImGui.PopID();
        id++;
    }

    /// <summary>
    /// Standard MSB Object Field
    /// </summary>
    private void PropGenericFieldRow(
        ViewportSelection selection,
        IEnumerable<Entity> entSelection,
        PropertyInfo prop,
        Type type,
        object obj,
        string name,
        int arrayIndex = -1,
        int classIndex = -1
    )
    {
        PropContextRowOpener();

        var meta = MsbMeta.GetFieldMeta(prop.Name, prop.ReflectedType);

        // Field Name
        var fieldName = prop.Name;

        if (CFG.Current.MapEditor_Enable_Commmunity_Names && !meta.IsEmpty)
        {
            fieldName = meta.AltName;

            if(meta.ArrayProperty)
            {
                fieldName = $"{meta.AltName}: {arrayIndex}";
            }
        }

        // Field Description
        var fieldDescription = "";
        if (!meta.IsEmpty)
        {
            fieldDescription = meta.Wiki;
        }

        ImGui.Text(fieldName);
        ShowFieldHint(obj, prop, fieldDescription);

        ImGui.NextColumn();
        ImGui.SetNextItemWidth(-1);
        var oldval = obj;
        object newval;

        // Property Editor UI
        (bool, bool) propEditResults = PropertyRow(meta, type, oldval, out newval, prop, entSelection);
        var changed = propEditResults.Item1;
        var committed = propEditResults.Item2;

        DisplayPropContextMenu(meta, selection, prop, obj, arrayIndex);

        if (ImGui.IsItemActive() && !ImGui.IsWindowFocused())
        {
            ImGui.SetItemDefaultFocus();
        }

        // Model Link
        MapEditorDecorations.ModelNameRow(meta, entSelection, prop, oldval);

        // Param References
        if (MapEditorDecorations.ParamRefRow(meta, prop, oldval, ref newval))
        {
            changed = true;
            committed = true;
        }

        // Text References
        if (MapEditorDecorations.FmgRefRow(meta, prop, oldval, ref newval))
        {
            changed = true;
            committed = true;
        }

        // Map References - TODO: this still uses the C# attribute method, change to MSBMETA
        if (MapEditorDecorations.MsbReferenceRow(meta, prop, oldval, ref newval, entSelection))
        {
            changed = true;
            committed = true;
        }

        // Enum List
        if (MapEditorDecorations.GenericEnumRow(meta, prop, oldval, ref newval))
        {
            changed = true;
            committed = true;
        }

        // Alias List
        if (MapEditorDecorations.AliasEnumRow(meta, prop, oldval, ref newval))
        {
            changed = true;
            committed = true;
        }

        // ER: Mask List - TODO: this still uses the C# attribute method, change to MSBMETA
        if (prop.GetCustomAttribute<EldenRingAssetMask>() != null)
        {
            if (MapEditorDecorations.EldenRingAssetMaskAndAnimRow(meta, prop, oldval, ref newval, selection))
            {
                changed = true;
                committed = true;
            }
        }

        UpdateProperty(prop, entSelection, newval, changed, committed, arrayIndex, classIndex);
        ImGui.NextColumn();
    }

    (string name, Entity entity) editName = ("", null);
    private void PropEditorNameDirect(IEnumerable<MsbEntity> entities)
    {
        ImGui.Text("Name");
        ImGui.NextColumn();
        var first = entities.First();
        if (first != editName.entity) editName = (first.Name, first);

        ImGui.PushItemWidth(-1);
        if (ImGui.InputText("##EntityName", ref editName.name, 64))
        {
            if (entities.Count() == 1)
            {
                ContextActionManager.ExecuteAction(new RenameObjectsAction(
                    entities.ToList(),
                    new List<string> { editName.name },
                    false
                ));
            }
            else
            {
                ContextActionManager.ExecuteAction(new RenameObjectsAction(
                    entities.ToList(),
                    entities.Select((ent, i) => $"{editName.name}_{i}").ToList(),
                    false
                ));
            }
        }
        ImGui.PopItemWidth();
        ImGui.NextColumn();
    }

    private void PropEditorNameWithRef(IEnumerable<MsbEntity> entities)
    {
        var first = entities.First();
        if (first.WrappedObject is not IMsbEntry) return;

        ImGui.Text("Name");
        ImGui.NextColumn();

        bool single = entities.Count() == 1;
        if (single)
        {
            if (first != editName.entity)
                editName = (first.Name, first);

            ImGui.Text($"{first.Name}");
        }
        else
        {
            if (first != editName.entity)
                editName = ("", first);

            ImGui.Text("...");
        }

        ImGui.NextColumn();
        if (ImGui.Button("Rename"))
        {
            if (single)
            {
                ContextActionManager.ExecuteAction(new RenameObjectsAction(
                    new List<MsbEntity> { first },
                    new List<string> { editName.name },
                    true
                ));
            }
            else
            {
                var nameList = entities
                    .GroupBy(e => e.WrappedObject.GetType())
                    .SelectMany(group =>
                        group.Select((ent, index) => $"{editName.name}-{group.Key.Name}"
                    ));

                ContextActionManager.ExecuteAction(new RenameObjectsAction(
                    entities.ToList(),
                    nameList.ToList(),
                    true
                ));
            }
        }
        ImGui.NextColumn();

        ImGui.PushItemWidth(-1);
        ImGui.InputText("##Name", ref editName.name, 64);
        ImGui.PopItemWidth();
        ImGui.NextColumn();
    }

    private void PropEditorSelectedEntities(ViewportSelection selection, int classIndex = -1)
    {
        var entities = selection.GetFilteredSelection<MsbEntity>();
        var types = entities.Select(t => t.WrappedObject.GetType()).Distinct();
        var maps = entities.Select(t => t.Container).Distinct();
        var first = entities.First();

        var type = types.Count() == 1 ? types.First() : typeof(IMsbEntry);
        var meta = MsbMeta.GetMeta(type, false);

        if (CFG.Current.MapEditor_Enable_Property_Property_TopDecoration)
        {
            DisplayPropertyViewDecorations(selection, 0);
        }

        ImGui.Columns(2);

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Object Type");

        ImGui.NextColumn();

        ImGui.AlignTextToFramePadding();
        ImGui.Text(type.Name);

        if (meta != null)
        {
            UIHelper.ShowHoverTooltip(meta.Wiki);
        }

        ImGui.NextColumn();

        // Name
        if (CFG.Current.MapEditor_Enable_Referenced_Rename)
        {
            PropEditorNameWithRef(entities);
        }
        else
        {
            PropEditorNameDirect(entities);
        }

        if (types.Count() > 1)
        {
            return;
        }

        ImGui.Separator();

        PropEditorGeneric(selection, entities, entities.First().WrappedObject, classIndex: classIndex);

        // Bottom Decoration
        ImGui.Columns(1);

        if (!CFG.Current.MapEditor_Enable_Property_Property_TopDecoration)
        {
            DisplayPropertyViewDecorations(selection, 0);
        }
    }

    /// <summary>
    /// Map Object Property column
    /// </summary>
    private void PropEditorGeneric(
        ViewportSelection selection,
        IEnumerable<Entity> entSelection,
        object obj,
        int classIndex = -1
    )
    {
        var scale = DPI.GetUIScale();
        Entity firstEnt = entSelection.First();
        Type type = obj.GetType();

        PropertyInfo[] properties = _propCache.GetCachedProperties(type);

        // Properties
        var id = 0;
        foreach (PropertyInfo prop in properties)
        {
            var meta = MsbMeta.GetFieldMeta(prop.Name, type);

            // Field Name
            var fieldName = prop.Name;

            if(CFG.Current.MapEditor_Enable_Commmunity_Names && !meta.IsEmpty)
            {
                fieldName = meta.AltName;
            }

            // Field Description
            var fieldDescription = "";
            if (!meta.IsEmpty)
            {
                fieldDescription = meta.Wiki;
            }

            // Filter by Search
            var filterTerm = msbFieldSearch.ToLower();
            if (msbFieldSearch != "")
            {
                if (!prop.Name.ToLower().Contains(filterTerm))
                {
                    continue;
                }
            }

            if (!meta.IsEmpty && msbFieldSearch != "")
            {
                if(!meta.AltName.ToLower().Contains(filterTerm))
                {
                    continue;
                }
            }

            if (!prop.CanWrite && !prop.PropertyType.IsArray)
            {
                continue;
            }

            // IMsbEntry.Name needs special handling to keep it unique
            if (typeof(IMsbEntry).IsAssignableFrom(type) && prop.Name == "Name")
                continue;

            // Index Properties are hidden by default
            if (meta != null && meta.IndexProperty)
                continue;

            ImGui.PushID(id);
            ImGui.AlignTextToFramePadding();
            Type typ = prop.PropertyType;

            if (typ.IsArray)
            {
                var a = (Array)prop.GetValue(obj);
                var open = ImGui.TreeNodeEx($@"{fieldName}s", ImGuiTreeNodeFlags.DefaultOpen);
                ShowFieldHint(obj, prop, fieldDescription);
                ImGui.NextColumn();
                ImGui.NextColumn();
                if (open)
                {
                    for (var i = 0; i < a.Length; i++)
                    {
                        ImGui.PushID(i);
                        Type arrtyp = typ.GetElementType();
                        if (arrtyp.IsClass && arrtyp != typeof(string) && !arrtyp.IsArray)
                        {
                            var classOpen = ImGui.TreeNodeEx($@"{fieldName}[{i}]", ImGuiTreeNodeFlags.DefaultOpen);
                            ShowFieldHint(obj, prop, fieldDescription);
                            ImGui.NextColumn();
                            ImGui.SetNextItemWidth(-1);
                            var o = a.GetValue(i);
                            ImGui.Text(o.GetType().Name);
                            ImGui.NextColumn();
                            if (classOpen)
                            {
                                PropEditorGeneric(selection, entSelection, o, i);
                                ImGui.TreePop();
                            }
                        }
                        else
                        {
                            ImGui.AlignTextToFramePadding();
                            var array = obj as object[];
                            PropGenericFieldRow(selection, entSelection, prop, typ.GetElementType(), a.GetValue(i), $@"{fieldName}[{i}]", i, classIndex);
                        }
                        ImGui.PopID();
                    }
                    ImGui.TreePop();
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
                        var open = ImGui.TreeNodeEx($@"{fieldName}[{i}]", ImGuiTreeNodeFlags.DefaultOpen);
                        ShowFieldHint(obj, prop, fieldDescription);
                        ImGui.NextColumn();
                        ImGui.SetNextItemWidth(-1);
                        var o = itemprop.GetValue(l, new object[] { i });
                        ImGui.Text(o.GetType().Name);
                        ImGui.NextColumn();
                        if (open)
                        {
                            PropEditorGeneric(selection, entSelection, o);
                            ImGui.TreePop();
                        }

                        ImGui.PopID();
                    }
                    else
                    {
                        PropGenericFieldRow(selection, entSelection, prop, arrtyp, itemprop.GetValue(l, [i]), $@"{fieldName}[{i}]", i, classIndex);
                        ImGui.PopID();
                    }
                }

                ImGui.PopID();
            }
            else if (typ.IsClass && typ == typeof(MSB.Shape))
            {
                var open = ImGui.TreeNodeEx($"{fieldName}", ImGuiTreeNodeFlags.DefaultOpen);
                ShowFieldHint(obj, prop, fieldDescription);
                ImGui.NextColumn();
                ImGui.SetNextItemWidth(-1);
                var o = prop.GetValue(obj);
                var shapetype = Enum.Parse<RegionShape>(o.GetType().Name);
                var shap = (int)shapetype;

                if (entSelection.Count() == 1)
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
                    PropEditorGeneric(selection, entSelection, o);
                    ImGui.TreePop();
                }

                ImGui.PopID();
            }
            else if (typ == typeof(BTL.LightType))
            {
                var open = ImGui.TreeNodeEx($"{fieldName}", ImGuiTreeNodeFlags.DefaultOpen);
                ShowFieldHint(obj, prop, fieldDescription);
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
                    PropEditorGeneric(selection, entSelection, o);
                    ImGui.TreePop();
                }

                ImGui.PopID();
            }
            else if (typ.IsClass && typ != typeof(string) && !typ.IsArray)
            {
                var o = prop.GetValue(obj);
                if (o != null)
                {
                    var open = ImGui.TreeNodeEx($"{fieldName}", ImGuiTreeNodeFlags.DefaultOpen);
                    ShowFieldHint(obj, prop, fieldDescription);
                    ImGui.NextColumn();
                    ImGui.SetNextItemWidth(-1);
                    ImGui.Text(o.GetType().Name);
                    ImGui.NextColumn();
                    if (open)
                    {
                        PropEditorGeneric(selection, entSelection, o);
                        ImGui.TreePop();
                    }
                }

                ImGui.PopID();
            }
            else
            {
                PropGenericFieldRow(selection, entSelection, prop, typ, prop.GetValue(obj), $"{fieldName}", classIndex);
                ImGui.PopID();
            }

            id++;
        }


    }

    private void DisplayPropertyViewDecorations(ViewportSelection selection, int refID)
    {
        var entSelection = selection.GetFilteredSelection<Entity>();
        if (entSelection.Count != 1) return;

        var firstEnt = entSelection.FirstOrDefault();
        if (firstEnt.References is null) return;

        if (CFG.Current.MapEditor_Enable_Property_Property_Class_Info)
        {
            PropInfo_MapObjectType.Display(firstEnt);
        }
        if (CFG.Current.MapEditor_Enable_Property_Property_SpecialProperty_Info)
        {
            PropInfo_Region_Connection.Display(firstEnt);
            PropInfo_Part_ConnectCollision.Display(firstEnt);
        }
        if (CFG.Current.MapEditor_Enable_Property_Property_ReferencesTo)
        {
            PropInfo_ReferencesTo.Display(firstEnt, _viewport, ref selection, ref refID);
        }
        if (CFG.Current.MapEditor_Enable_Property_Property_ReferencesBy)
        {
            PropInfo_ReferencedBy.Display(firstEnt, _viewport, ref selection, ref refID);
        }
        if (CFG.Current.MapEditor_Enable_Param_Quick_Links)
        {
            PropInfo_ParamJumps.Display(firstEnt, _viewport, ref selection, ref refID);
        }
    }

    public void ShowFieldHint(object obj, PropertyInfo prop, string description)
    {
        var text = description;

        // Property Details
        var propType = prop.ReflectedType;

        if (propType.IsArray)
        {
            var a = (Array)prop.GetValue(obj);

            var str = $"Array Type: {prop.ReflectedType.Name}";
            if (a.Length > 0)
            {
                str += $" (Length: {a.Length})";
            }

            text = $"{text}\n{str}";
        }

        if (propType.IsValueType)
        {
            var str = $"Value Type: {propType.Name}";
            var min = propType.GetField("MinValue")?.GetValue(propType);
            var max = propType.GetField("MaxValue")?.GetValue(propType);
            if (min != null && max != null)
            {
                str += $" (Min {min}, Max {max})";
            }

            text = $"{text}\n{str}";
        }
        else if (propType == typeof(string))
        {
            var a = (Array)prop.GetValue(obj);

            var str = $"String Type: {propType.Name}";
            if (a.Length > 0)
            {
                str += $" (Length: {a.Length})";
            }

            text = $"{text}\n{str}";
        }

        // Final description
        UIHelper.ShowHoverTooltip(text);
    }


    private (bool, bool) PropertyRow(MsbFieldMetaData meta, Type typ, object oldval, out object newval, PropertyInfo prop, IEnumerable<Entity> entSelection)
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

            if (meta != null && meta.IsBool)
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

            if (meta != null && meta.IsBool)
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

            if (meta != null && meta.IsBool)
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

                    newval = (short)val;
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

            if (meta != null && meta.IsBool)
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

            if (meta != null && meta.IsBool)
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

                    newval = (sbyte)val;
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

            if (meta != null && meta.IsBool)
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

            bool showNormalInput = true;

            if (entSelection != null && meta != null && meta.ScaleProperty)
            {
                var ent = entSelection.FirstOrDefault();
                if (ent != null)
                {
                    if (ent.IsPartEnemy() || ent.IsPartDummyEnemy())
                    {
                        showNormalInput = false;
                    }
                }
            }

            if (showNormalInput)
            {
                if (ImGui.DragFloat3("##value", ref val, 0.1f))
                {
                    newval = val;
                    isChanged = true;
                }
            }
            else
            {
                ImGui.BeginDisabled();
                if (ImGui.DragFloat3("##value", ref val, 0.1f))
                {
                    newval = val;
                    isChanged = true;
                }
                ImGui.EndDisabled();
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
                    LogLevel.Warning, LogPriority.Low);

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

        selection.BuildReferenceMap();
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
    private void UpdateProperty(object prop,
     IEnumerable<Entity> selection,
     object newval,

        bool changed,
         bool committed,
         int arrayindex,
         int classIndex)
    {
        foreach (var ent in selection)
        {
            ent.CachedAliasName = null;
            if (changed) { ent.BuildReferenceMap(); }
        }

        if (changed)
        {
            ChangePropertyMultiple(prop, selection, newval, ref committed, arrayindex, classIndex);
            foreach (var ent in selection)
            {
                ent.BuildReferenceMap();
            }
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
    private void ChangePropertyMultiple(object prop, IEnumerable<Entity> ents, object newval, ref bool committed,
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

        var set = ents.ToHashSet();
        MultipleEntityPropertyChangeAction action;
        foreach (Entity selection in ents)
        {
            if (selection != null && _changingObject != null && !set.SetEquals((HashSet<Entity>)_changingObject))
            {
                committed = true;
                return;
            }
        }

        action = new MultipleEntityPropertyChangeAction((PropertyInfo)prop, set, newval, arrayindex, classIndex);
        ContextActionManager.ExecuteAction(action);

        _lastUncommittedAction = action;
        _changingPropery = prop;
        _changingObject = set;
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