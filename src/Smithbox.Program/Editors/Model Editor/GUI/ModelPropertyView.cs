using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reflection;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using StudioCore.Utilities;
using StudioCore.Editors.Viewport;
using StudioCore.Editors.MapEditor;

namespace StudioCore.Editors.ModelEditor;

public class ModelPropertyView
{
    public ModelEditorScreen Editor;
    public ProjectEntry Project;

    public ModelPropertyView(ModelEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    private object _changingObject;
    private object _changingPropery;

    private ViewportAction _lastUncommittedAction;
    public PropertyInfo RequestedSearchProperty = null;

    public bool Focus = false;

    private string PropertySearch = "";


    public void OnGui()
    {
        var scale = DPI.UIScale();
        HashSet<Entity> entSelection = Editor.ViewportSelection.GetFilteredSelection<Entity>();

        if (!CFG.Current.Interface_MapEditor_Properties)
            return;

        ImGui.PushStyleColor(ImGuiCol.ChildBg, UI.Current.ImGui_ChildBg);
        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(350, Editor.ModelViewportView.Viewport.Height - 80) * scale, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowPos(new Vector2(Editor.ModelViewportView.Viewport.Width - 370, 20) * scale, ImGuiCond.FirstUseEver);
        ImGui.Begin($@"Properties##modeleditprop");

        Editor.FocusManager.SwitchModelEditorContext(ModelEditorContext.ModelPropertiesHeader);

        // Header
        ImGui.AlignTextToFramePadding();
        ImGui.InputText("##modelPropertySearch", ref PropertySearch, 255);
        UIHelper.Tooltip("Filter the properties by field names that exactly or partially match your input.");

        // Toggle Community Field Names
        ImGui.SameLine();

        if (ImGui.Button($"{Icons.Book}", DPI.IconButtonSize))
        {
            CFG.Current.ModelEditor_Enable_Commmunity_Names = !CFG.Current.ModelEditor_Enable_Commmunity_Names;
        }

        var communityFieldNameMode = "Internal";
        if (CFG.Current.ModelEditor_Enable_Commmunity_Names)
            communityFieldNameMode = "Community";

        UIHelper.Tooltip($"Toggle field name display type between Internal and Community.\nCurrent Mode: {communityFieldNameMode}");

        ImGui.Separator();

        // Properties
        ImGui.BeginChild("propedit");

        Editor.FocusManager.SwitchModelEditorContext(ModelEditorContext.ModelProperties);

        if(Editor.Selection.SelectedModelWrapper != null && Editor.Selection.SelectedModelWrapper.Container != null)
        {
            if(entSelection.Count > 1)
            {
                Entity firstEnt = entSelection.First();

                ImGui.TextColored(new Vector4(0.5f, 1.0f, 0.0f, 1.0f),
                    " Editing Multiple Objects.\n Changes will be applied to all selected objects.");

                ImGui.Separator();
                ImGui.PushStyleColor(ImGuiCol.FrameBg, UI.Current.ImGui_MultipleInput_Background);
                ImGui.BeginChild("Model_EditingMultipleObjsChild");
                Editor.FocusManager.SwitchModelEditorContext(ModelEditorContext.ModelProperties);

                ModelPropertyOrchestrator(Editor.ViewportSelection);

                ImGui.PopStyleColor();
                ImGui.EndChild();
            }
            else if(entSelection.Any())
            {
                Entity firstEnt = entSelection.First();

                if (firstEnt.WrappedObject == null)
                {
                    ImGui.Text("Select a map object to edit its properties.");
                    ImGui.EndChild();
                    ImGui.End();
                    ImGui.PopStyleColor(2);
                    return;
                }

                ModelPropertyOrchestrator(Editor.ViewportSelection);
            }
            else
            {
                ImGui.Text("Nothing has been selected.");
            }
        }

        ImGui.EndChild();
        ImGui.End();
        ImGui.PopStyleColor(2);
    }

    private void ModelPropertyOrchestrator(ViewportSelection selection, int classIndex = -1)
    {
        var entities = selection.GetFilteredSelection<ModelEntity>();

        var types = entities.Select(t => t.WrappedObject.GetType()).Distinct();

        var first = entities.First();

        var type = types.First();

        var objType = first.WrappedObject.GetType();

        // var meta = Editor.Project.ModelData.Meta.GetMeta(type, false);

        ImGui.Columns(2);

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Object Type");

        //if (meta != null)
        //{
        //    UIHelper.Tooltip(meta.Wiki);
        //}

        ImGui.NextColumn();
        ImGui.NextColumn();

        if (types.Count() > 1)
        {
            return;
        }

        ImGui.Separator();

        ModelPropertyHandler(selection, entities, entities.First().WrappedObject, classIndex: classIndex);

        ImGui.Columns(1);
    }

    private void ModelPropertyHandler(
        ViewportSelection selection,
        IEnumerable<Entity> entSelection,
        object obj,
        int classIndex = -1
    )
    {
        var scale = DPI.UIScale();
        Entity firstEnt = entSelection.First();
        Type type = obj.GetType();

        PropertyInfo[] properties = Editor.ModelPropertyCache.GetCachedProperties(type);

        // Properties
        var id = 0;
        foreach (PropertyInfo prop in properties)
        {
            // var meta = Editor.Project.ModelData.Meta.GetFieldMeta(prop.Name, type);

            // Field Name
            var fieldName = prop.Name;

            //if (CFG.Current.ModelEditor_Enable_Commmunity_Names && !meta.IsEmpty)
            //{
            //    fieldName = meta.AltName;
            //}

            // Field Description
            var fieldDescription = "";

            //if (!meta.IsEmpty)
            //{
            //    fieldDescription = meta.Wiki;
            //}

            // Filter by Search
            var filterTerm = PropertySearch.ToLower();

            if (PropertySearch != "")
            {
                if (!prop.Name.ToLower().Contains(filterTerm))
                {
                    continue;
                }
            }

            var ignoreProp = prop.GetCustomAttribute<IgnoreInModelEditor>();
            if(ignoreProp != null)
            {
                continue;
            }

            //if (!meta.IsEmpty && PropertySearch != "")
            //{
            //    if (!meta.AltName.ToLower().Contains(filterTerm))
            //    {
            //        continue;
            //    }
            //}

            if (!prop.CanWrite && !prop.PropertyType.IsArray)
            {
                continue;
            }

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
                            var classOpen = ImGui.TreeNodeEx($@"{fieldName}: {i}", ImGuiTreeNodeFlags.DefaultOpen);

                            ShowFieldHint(obj, prop, fieldDescription);

                            ImGui.NextColumn();
                            ImGui.SetNextItemWidth(-1);

                            var o = a.GetValue(i);

                            ImGui.Text(o.GetType().Name);
                            ImGui.NextColumn();

                            if (classOpen)
                            {
                                ModelPropertyHandler(selection, entSelection, o, i);

                                ImGui.TreePop();
                            }
                        }
                        else
                        {
                            ImGui.AlignTextToFramePadding();
                            var array = obj as object[];

                            DisplayModelPropertyLine(selection, entSelection, prop, typ.GetElementType(), a.GetValue(i), $@"{fieldName}[{i}]", i, classIndex);
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
                if (l != null)
                {
                    PropertyInfo itemprop = l.GetType().GetProperty("Item");
                    var count = (int)l.GetType().GetProperty("Count").GetValue(l);

                    for (var i = 0; i < count; i++)
                    {
                        ImGui.PushID(i);

                        Type arrtyp = typ.GetGenericArguments()[0];

                        if (arrtyp.IsClass && arrtyp != typeof(string) && !arrtyp.IsArray)
                        {
                            var open = ImGui.TreeNodeEx($@"{fieldName}: {i}", ImGuiTreeNodeFlags.DefaultOpen);

                            ShowFieldHint(obj, prop, fieldDescription);

                            ImGui.NextColumn();
                            ImGui.SetNextItemWidth(-1);

                            var o = itemprop.GetValue(l, new object[] { i });

                            ImGui.Text(o.GetType().Name);

                            ImGui.NextColumn();

                            if (open)
                            {
                                ModelPropertyHandler(selection, entSelection, o);

                                ImGui.TreePop();
                            }

                            ImGui.PopID();
                        }
                        else
                        {
                            DisplayModelPropertyLine(selection, entSelection, prop, arrtyp, itemprop.GetValue(l, [i]), $@"{fieldName}[{i}]", i, classIndex);

                            ImGui.PopID();
                        }
                    }
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
                        ModelPropertyHandler(selection, entSelection, o);
                        ImGui.TreePop();
                    }
                }

                ImGui.PopID();
            }
            else
            {
                DisplayModelPropertyLine(selection, entSelection, prop, typ, prop.GetValue(obj), $"{fieldName}", classIndex);

                ImGui.PopID();
            }

            id++;
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
        UIHelper.Tooltip(text);
    }

    private void DisplayModelPropertyLine(
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
        OpenModelPropertyContextMenu();

        // var meta = Editor.Project.MapData.Meta.GetFieldMeta(prop.Name, prop.ReflectedType);

        // Field Name
        var fieldName = prop.Name;

        //if (CFG.Current.MapEditor_Enable_Commmunity_Names && !meta.IsEmpty)
        //{
        //    fieldName = meta.AltName;

        //    if (meta.ArrayProperty)
        //    {
        //        fieldName = $"{meta.AltName}: {arrayIndex}";
        //    }
        //}

        // Field Description
        var fieldDescription = "";

        //if (!meta.IsEmpty)
        //{
        //    fieldDescription = meta.Wiki;
        //}

        ImGui.Text(fieldName);

        ShowFieldHint(obj, prop, fieldDescription);

        ImGui.NextColumn();
        ImGui.SetNextItemWidth(-1);

        var oldval = obj;
        object newval;

        // Property Editor UI
        (bool, bool) propEditResults = HandlePropertyInput(type, oldval, out newval, prop, entSelection);

        var changed = propEditResults.Item1;
        var committed = propEditResults.Item2;

        DisplayModelPropertyContextMenu(selection, prop, obj, arrayIndex, fieldName);

        if (ImGui.IsItemActive() && !ImGui.IsWindowFocused())
        {
            ImGui.SetItemDefaultFocus();
        }

        // Dummy References
        var dummyRef = prop.GetCustomAttribute<DummyReference>();
        if(dummyRef != null)
        {
            var container = Editor.Selection.SelectedModelWrapper.Container;
            var value = int.Parse(oldval.ToString());

            ImGui.NextColumn();

            ImGui.Text("");

            ImGui.NextColumn();

            for(int i = 0; i < container.Dummies.Count; i++)
            {
                var curDummy = container.Dummies[i];

                if(i == value)
                {
                    var dummy = (FLVER.Dummy)curDummy.WrappedObject;

                    if (ImGui.Button($"{Icons.Binoculars}##dummySelect{i}"))
                    {
                        EditorCommandQueue.AddCommand($"model/select/dummy/{i}");
                    }

                    ImGui.SameLine();

                    ImGui.Text($"Dummy {i}: {dummy.ReferenceID}");
                }
            }
        }

        // Node References
        var nodeRef = prop.GetCustomAttribute<NodeReference>();
        if (nodeRef != null)
        {
            var container = Editor.Selection.SelectedModelWrapper.Container;
            var value = int.Parse(oldval.ToString());

            ImGui.NextColumn();

            ImGui.Text("");

            ImGui.NextColumn();

            for (int i = 0; i < container.Nodes.Count; i++)
            {
                var curNode = container.Nodes[i];

                if (i == value)
                {
                    var node = (FLVER.Node)curNode.WrappedObject;

                    if (ImGui.Button($"{Icons.Binoculars}##nodeSelect{i}"))
                    {
                        EditorCommandQueue.AddCommand($"model/select/node/{i}");
                    }

                    ImGui.SameLine();

                    ImGui.Text($"{node.Name}");
                }
            }
        }

        // Material References
        var matRef = prop.GetCustomAttribute<MaterialReference>();
        if (matRef != null)
        {
            var container = Editor.Selection.SelectedModelWrapper.Container;
            var value = int.Parse(oldval.ToString());

            ImGui.NextColumn();

            ImGui.Text("");

            ImGui.NextColumn();

            for (int i = 0; i < container.Materials.Count; i++)
            {
                var curMaterial = container.Materials[i];

                if (i == value)
                {
                    var material = (FLVER2.Material)curMaterial.WrappedObject;

                    if (ImGui.Button($"{Icons.Binoculars}##matSelect{i}"))
                    {
                        EditorCommandQueue.AddCommand($"model/select/material/{i}");
                    }

                    ImGui.SameLine();

                    ImGui.Text($"{material.Name}");
                }
            }
        }

        UpdateProperty(prop, entSelection, oldval, newval, changed, committed, arrayIndex, classIndex);

        ImGui.NextColumn();
    }

    private static void OpenModelPropertyContextMenu()
    {
        ImGui.Selectable("", false, ImGuiSelectableFlags.AllowOverlap);

        ImGui.SameLine();

        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
        {
            ImGui.OpenPopup("ModelPropContextMenu");
        }
    }

    private void DisplayModelPropertyContextMenu(ViewportSelection selection, PropertyInfo prop, 
        object obj, int arrayIndex, string fieldName)
    {
        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
        {
            ImGui.OpenPopup("ModelPropContextMenu");
        }

        if (ImGui.BeginPopup("ModelPropContextMenu"))
        {
            if (ImGui.Selectable(@"Copy Property Name##CopyPropName"))
            {
                PlatformUtils.Instance.SetClipboardText(fieldName);
            }

            if (ImGui.Selectable(@"Copy Property Type##CopyPropType"))
            {
                var propType = prop.PropertyType;

                if (propType != null)
                {
                    var primitiveType = propType.ToString().Replace("System.", "");
                    PlatformUtils.Instance.SetClipboardText(primitiveType);
                }
            }

            ImGui.EndPopup();
        }
    }

    #region Property Input
    private (bool, bool) HandlePropertyInput(Type typ, object oldval, out object newval, PropertyInfo prop, 
        IEnumerable<Entity> entSelection)
    {
        ImGui.SetNextItemWidth(-1);

        newval = null;
        var isChanged = false;
        if (typ == typeof(long))
        {
            var val = (long)oldval;
            var strval = $@"{val}";

            var input = new InputTextHandler(strval);

            if (input.Draw("##value", out string newValue))
            {
                var res = long.TryParse(newValue, out val);
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

            var input = new InputTextHandler(strval);

            if (input.Draw("##value", out string newValue))
            {
                var res = uint.TryParse(newValue, out val);
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

            var input = new InputTextHandler(strval);

            if (input.Draw("##value", out string newValue))
            {
                var res = ushort.TryParse(newValue, out val);
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

            var input = new InputTextHandler(strval);

            if (input.Draw("##value", out string newValue))
            {
                var res = byte.TryParse(newValue, out val);
                if (res)
                {
                    newval = val;
                    isChanged = true;
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

            var input = new InputTextHandler(val);

            if (input.Draw("##value", out string newValue))
            {
                newval = newValue;
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

                    var flags = ImGuiColorEditFlags.AlphaOpaque;

                    if (ImGui.ColorEdit4("##value", ref val, flags))
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

                var flags = ImGuiColorEditFlags.AlphaOpaque;

                if (ImGui.ColorEdit4("##value", ref val, flags))
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

    #endregion

    #region Property Change - Update - Commit

    private void UpdateProperty(object prop, Entity selection, object obj, object oldval, object newval,
        bool changed, bool committed, int arrayindex = -1)
    {
        if (changed)
        {
            ChangeProperty(prop, selection, obj, oldval, newval, ref committed, arrayindex);
        }

        if (committed)
        {
            CommitProperty(selection, oldval, newval, false);
        }
    }

    private void UpdateProperty(object prop, IEnumerable<Entity> selection,  object oldval, object newval, 
        bool changed, bool committed, int arrayindex, int classIndex)
    {
        foreach (var ent in selection)
        {
            if (changed)
            {
                ent.CachedAliasName = null;
                ent.BuildReferenceMap();
            }
        }

        if (changed)
        {
            ChangePropertyMultiple(prop, selection, oldval, newval, ref committed, arrayindex, classIndex);

            foreach (var ent in selection)
            {
                ent.BuildReferenceMap();
            }
        }

        if (committed)
        {
            if (_lastUncommittedAction != null && Editor.EditorActionManager.PeekUndoAction() == _lastUncommittedAction)
            {
                if (_lastUncommittedAction is MultipleEntityPropertyChangeAction a)
                {
                    Editor.EditorActionManager.UndoAction();

                    a.UpdateRenderModel = true; // Update render model on commit execution, and update on undo/redo.

                    Editor.EditorActionManager.ExecuteAction(a);
                }

                _lastUncommittedAction = null;
                _changingPropery = null;
                _changingObject = null;
            }
        }
    }
    private void ChangePropertyMultiple(object prop, IEnumerable<Entity> ents, object oldval, object newval, ref bool committed,
        int arrayindex = -1, int classIndex = -1)
    {
        if (prop == _changingPropery && _lastUncommittedAction != null &&
            Editor.EditorActionManager.PeekUndoAction() == _lastUncommittedAction)
        {
            Editor.EditorActionManager.UndoAction();
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

        action = new MultipleEntityPropertyChangeAction(Editor, (PropertyInfo)prop, set, newval, arrayindex, classIndex);

        Editor.EditorActionManager.ExecuteAction(action);

        _lastUncommittedAction = action;
        _changingPropery = prop;
        _changingObject = set;
    }

    private void ChangeProperty(object prop, Entity selection, object obj, object oldval, object newval,
        ref bool committed, int arrayindex = -1)
    {
        if (prop == _changingPropery && _lastUncommittedAction != null &&
            Editor.EditorActionManager.PeekUndoAction() == _lastUncommittedAction)
        {
            Editor.EditorActionManager.UndoAction();
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

            Editor.EditorActionManager.ExecuteAction(action);

            _lastUncommittedAction = action;
            _changingPropery = prop;
            _changingObject = selection != null ? selection.WrappedObject : obj;
        }
    }

    private void CommitProperty(Entity selection, object oldval, object newval, bool destroyRenderModel)
    {
        // Invalidate name cache
        if (selection != null)
        {
            selection.Name = null;
        }

        selection.BuildReferenceMap();

        // Undo and redo the last action with a rendering update
        if (_lastUncommittedAction != null && Editor.EditorActionManager.PeekUndoAction() == _lastUncommittedAction)
        {
            if (_lastUncommittedAction is PropertiesChangedAction a)
            {
                // Kinda a hack to prevent a jumping glitch
                a.SetPostExecutionAction(null);

                Editor.EditorActionManager.UndoAction();

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

                        selection.UpdateRenderModel(Editor);
                    });
                }

                Editor.EditorActionManager.ExecuteAction(a);
            }
        }

        _lastUncommittedAction = null;
        _changingPropery = null;
        _changingObject = null;
    }
    #endregion
}
