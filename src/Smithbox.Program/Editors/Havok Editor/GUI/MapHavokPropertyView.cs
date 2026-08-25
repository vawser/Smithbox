using Hexa.NET.ImGui;
using HKLib.hk2018;
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;
using StudioCore.Utilities;
using System.Collections;
using System.Drawing;
using System.Numerics;
using System.Reflection;

namespace StudioCore.Editors.HavokEditor;

/// <summary>
/// Handles the property editing for collision / navmesh HKX files
/// Used in the Map Editor
/// </summary>
public class MapHavokPropertyView
{
    private MapEditorView View;
    private ProjectEntry Project;

    private object _changingProperty;
    private ViewportAction _lastUncommittedAction;

    public HavokPropertyMode PropertyMode = HavokPropertyMode.Collision;

    public MapCollisionEditType CollisionEditType = MapCollisionEditType.High;
    public MapNavmeshEditType NavmeshEditType = MapNavmeshEditType.N;

    public MapHavokPropertyView(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void SetPropertyMode(HavokPropertyMode newMode)
    {
        PropertyMode = newMode;
    }

    public void Display()
    {
        HashSet<Entity> entSelection = View.ViewportSelection.GetFilteredSelection<Entity>();

        // Properties
        ImGui.BeginChild("mapHavokEdit", ImGuiChildFlags.Borders);

        if (View.Universe.HasProcessedMapLoad && entSelection.Any())
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

            if (PropertyMode is HavokPropertyMode.Collision)
            {
                CollisionPropEditor(firstEnt);
            }
            else if (PropertyMode is HavokPropertyMode.Navmesh)
            {
                NavmeshPropEditor(firstEnt);
            }
        }
        else if (!View.Universe.HasProcessedMapLoad)
        {
            ImGui.Text("");
        }
        else
        {
            ImGui.Text("Select a map object to edit its properties.");
        }

        ImGui.EndChild();
    }


    public void CollisionPropEditor(Entity ent)
    {
        var mapID = View.Selection.SelectedMapID;

        PropertyInfo prop = ent.WrappedObject.GetType().GetProperty("ModelName");
        var value = prop.GetValue(ent.WrappedObject);

        if (value == null)
            return;

        var modelName = (string)value;

        var fullName = $"h{mapID.Replace("m", "")}_{modelName.Replace("h", "")}.hkx.dcx";

        if (CollisionEditType is MapCollisionEditType.Low)
        {
            fullName = $"l{mapID.Replace("m", "")}_{modelName.Replace("h", "")}.hkx.dcx";
        }

        if (CollisionEditType is MapCollisionEditType.FallProtection)
        {
            fullName = $"f{mapID.Replace("m", "")}_{modelName.Replace("h", "")}.hkx.dcx";
        }

        if (View.HavokCollisionBank.HavokContainers.ContainsKey(fullName))
        {
            GUI.SimpleHeader($"{fullName}", "");

            var curCollision = View.HavokCollisionBank.HavokContainers[fullName];

            HavokPropEdit(curCollision);
        }
        else
        {
            GUI.WrappedText("No collision file for this model name of this type.");
        }
    }

    public void NavmeshPropEditor(Entity ent)
    {
        var mapID = View.Selection.SelectedMapID;

        PropertyInfo prop = ent.WrappedObject.GetType().GetProperty("ModelID");
        var value = prop.GetValue(ent.WrappedObject);

        if (value == null)
            return;

        var modelID = value.ToString();

        if (modelID.Length == 1)
        {
            modelID = $"00000{modelID}";
        }
        else if (modelID.Length == 2)
        {
            modelID = $"0000{modelID}";
        }
        else if (modelID.Length == 3)
        {
            modelID = $"000{modelID}";
        }
        else if (modelID.Length == 4)
        {
            modelID = $"00{modelID}";
        }
        else if (modelID.Length == 5)
        {
            modelID = $"0{modelID}";
        }

        var fullName = $"n{mapID.Replace("m", "")}_{modelID}";

        if (NavmeshEditType is MapNavmeshEditType.O)
        {
            fullName = $"o{mapID.Replace("m", "")}_{modelID}";
        }

        if (View.HavokNavmeshBank.HKX3_Containers.ContainsKey(fullName))
        {
            GUI.SimpleHeader($"{fullName}", "");

            var curNavmesh = View.HavokNavmeshBank.HKX3_Containers[fullName];

            HavokPropEdit(curNavmesh);
        }
        else
        {
            GUI.WrappedText("No navmesh file for this model name of this type.");
        }
    }

    public void HavokPropEdit(hkRootLevelContainer root)
    {
        var mapID = View.Selection.SelectedMapID;
        var type = root.GetType();

        var columnCount = 2;
        if(CFG.Current.MapEditor_HavokEdit_Display_Type_Column)
        {
            columnCount = 3;
        }

        ImGui.Columns(columnCount);

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Object Type");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Map ID");
        GUI.Tooltip("The map ID of the map that the first entry of the current selection is found in.");

        ImGui.NextColumn();

        ImGui.AlignTextToFramePadding();
        ImGui.Text(type.Name);

        ImGui.AlignTextToFramePadding();

        ImGui.Text(mapID);

        if (mapID != "")
        {
            var mapAlias = AliasHelper.GetMapNameAlias(View.Project, mapID);
            GUI.DisplayAlias(mapAlias);
        }

        ImGui.NextColumn();

        if (CFG.Current.MapEditor_HavokEdit_Display_Type_Column)
        {
            ImGui.NextColumn();
        }

        var havokMeta = HavokMetaHelper.GetMeta(Project, type);

        HavokPropEditGeneric(root, havokMeta);

        ImGui.Columns(1);
    }

    private void HavokPropEditGeneric(object obj, HavokClass havokMeta, int classIndex = -1)
    {
        var scale = DPI.UIScale();
        Type type = obj.GetType();

        FieldInfo[] properties = View.MapPropertyCache.GetCachedHavokFields(type);

        // Properties
        var id = 0;
        foreach (FieldInfo prop in properties)
        {
            havokMeta = HavokMetaHelper.GetMeta(Project, type);

            var treeFlags = ImGuiTreeNodeFlags.DefaultOpen;

            // Field Name
            var fieldName = prop.Name;
            var fieldDescription = "";

            if (havokMeta != null)
            {
                if (CFG.Current.MapEditor_Properties_Enable_Commmunity_Names)
                {
                    fieldName = HavokMetaHelper.GetFieldName(havokMeta, prop.Name);
                }

                fieldDescription = $"{HavokMetaHelper.GetFieldDescription(havokMeta, prop.Name)}";
            }

            ImGui.PushID(id);
            ImGui.AlignTextToFramePadding();
            Type typ = prop.FieldType;

            if (typ.IsArray)
            {
                var a = (Array)prop.GetValue(obj);
                var open = ImGui.TreeNodeEx($@"{fieldName}", treeFlags);
                GUI.Tooltip(fieldDescription);
                ImGui.NextColumn();
                ImGui.NextColumn();
                if (CFG.Current.MapEditor_HavokEdit_Display_Type_Column)
                {
                    PropContextRowOpener("arrayTypeCol");

                    ImGui.Text(type.FullName);

                    DisplayContextMenu(fieldName, fieldDescription, prop);

                    ImGui.NextColumn();
                }

                if (open)
                {
                    for (var i = 0; i < a.Length; i++)
                    {
                        ImGui.PushID(i);
                        Type arrtyp = typ.GetElementType();
                        if (arrtyp.IsClass && arrtyp != typeof(string) && !arrtyp.IsArray)
                        {
                            var classOpen = ImGui.TreeNodeEx($@"{fieldName}: {i}", treeFlags);
                            GUI.Tooltip(fieldDescription);
                            ImGui.NextColumn();
                            ImGui.SetNextItemWidth(-1);
                            var o = a.GetValue(i);
                            ImGui.Text("");
                            ImGui.NextColumn();
                            if (CFG.Current.MapEditor_HavokEdit_Display_Type_Column)
                            {
                                PropContextRowOpener("arrayTypeEntryCol");

                                ImGui.Text(type.FullName);

                                DisplayContextMenu(fieldName, fieldDescription, prop);

                                ImGui.NextColumn();
                            }

                            if (classOpen)
                            {
                                HavokPropEditGeneric(o, havokMeta, i);
                                ImGui.TreePop();
                            }
                        }
                        else
                        {
                            ImGui.AlignTextToFramePadding();
                            var array = obj as object[];

                            // Handle property display (and search filtering)
                            if (DisplayProperty(havokMeta, obj, prop, type))
                            {
                                PropGenericFieldRow(prop, typ.GetElementType(), havokMeta, a.GetValue(i), obj, $@"{fieldName}[{i}]", fieldDescription, i, classIndex);
                            }
                        }
                        ImGui.PopID();
                    }
                    ImGui.TreePop();
                }
                ImGui.PopID();
            }
            else if (typ.IsGenericType && typ.GetGenericTypeDefinition() == typeof(List<>))
            {
                Type arrtyp = typ.GetGenericArguments()[0];
                PropEditorGenericList(havokMeta, obj, prop, arrtyp, fieldName, fieldDescription, treeFlags, classIndex);
            }
            else if (typ.BaseType != null && typ.BaseType.IsGenericType
                && typ.BaseType.GetGenericTypeDefinition() == typeof(List<>))
            {
                Type arrtyp = typ.BaseType.GetGenericArguments()[0];
                PropEditorGenericList(havokMeta, obj, prop, arrtyp, fieldName, fieldDescription, treeFlags, classIndex);
            }
            else if (typ.IsClass && typ != typeof(string) && !typ.IsArray)
            {
                var o = prop.GetValue(obj);
                if (o != null)
                {
                    var open = ImGui.TreeNodeEx($"{fieldName}", treeFlags);
                    GUI.Tooltip(fieldDescription);
                    ImGui.NextColumn();
                    ImGui.SetNextItemWidth(-1);
                    ImGui.Text("");
                    ImGui.NextColumn();
                    if (CFG.Current.MapEditor_HavokEdit_Display_Type_Column)
                    {
                        PropContextRowOpener("classTypeCol");

                        ImGui.Text(type.FullName);

                        DisplayContextMenu(fieldName, fieldDescription, prop);

                        ImGui.NextColumn();
                    }

                    if (open)
                    {
                        HavokPropEditGeneric(o, havokMeta);
                        ImGui.TreePop();
                    }
                }

                ImGui.PopID();
            }
            else
            {
                // Handle property display (and search filtering)
                if (DisplayProperty(havokMeta, obj, prop, type))
                {
                    PropGenericFieldRow(prop, typ, havokMeta, prop.GetValue(obj), obj, fieldName, fieldDescription, classIndex);
                }

                ImGui.PopID();
            }

            id++;
        }
    }

    public bool DisplayProperty(HavokClass havokMeta, object propObj, FieldInfo prop, Type type)
    {
        var propName = prop.Name;

        if (havokMeta != null)
        {
            var isRawData = HavokMetaHelper.IsRawData(havokMeta, prop.Name);

            if (!CFG.Current.MapEditor_CollisionEdit_Display_Raw_Data_Fields)
            {
                if (isRawData)
                {
                    return false;
                }
            }
        }

        // Normal filter
        var isMatch = EditorFilters.IsMatch(View.MapPropertyView.MapPropFilter, propName, View.MapPropertyView.ExactMapPropFilter);
        var isValueMatch = false;

        if (View.MapPropertyView.MapPropFilter.StartsWith("val:"))
            isValueMatch = true;

        if (!isMatch && !isValueMatch)
        {
            return false;
        }
        else if (isValueMatch)
        {
            // TODO: currently doesn't match correctly with array list values
            var valStr = View.MapPropertyView.MapPropFilter.Replace("val:", "");

            var propVal = prop.GetValue(propObj);

            if (propVal != null)
            {
                var value = $"{propVal}";

                if (View.MapPropertyView.ExactMapPropFilter)
                {
                    if (valStr != value)
                        return false;
                }
                else
                {
                    if (!value.Contains(valStr))
                        return false;
                }
            }
        }

        return true;
    }

    private void PropGenericFieldRow(
        FieldInfo prop,
        Type type,
        HavokClass havokMeta,
        object value,
        object containerObj,
        string name,
        string description,
        int arrayIndex = -1,
        int classIndex = -1,
        Action onRemove = null
    )
    {
        PropContextRowOpener("nameCol");

        ImGui.Text(name);

        // Remove-from-list button (only passed in by list entries)
        if (onRemove != null)
        {
            ImGui.SameLine();
            if (ImGui.Button("-##removeListEntry"))
            {
                onRemove();
            }
            GUI.Tooltip("Remove this entry from the list.");
        }

        GUI.Tooltip(description);

        ImGui.NextColumn();
        ImGui.SetNextItemWidth(-1);

        var oldval = value;
        object newval;

        // Property Editor UI
        (bool, bool) propEditResults = PropertyRow(type, oldval, out newval, prop);
        var changed = propEditResults.Item1;
        var committed = propEditResults.Item2;

        DisplayContextMenu(name, description, prop);

        if (ImGui.IsItemActive() && !ImGui.IsWindowFocused())
        {
            ImGui.SetItemDefaultFocus();
        }

        if (HavokPropertyDecorators.ParamRefRow(View, havokMeta, prop, oldval, ref newval))
        {
            changed = true;
            committed = true;
        }

        UpdateProperty(prop, containerObj, oldval, newval, changed, committed, arrayIndex, classIndex);

        if (CFG.Current.MapEditor_HavokEdit_Display_Type_Column)
        {
            ImGui.NextColumn();

            var reflectedType = prop.ReflectedType;
            if (reflectedType != null)
            {
                PropContextRowOpener("typecol");

                ImGui.Text(reflectedType.FullName);
            }
        }

        ImGui.NextColumn();
    }

    private static void PropContextRowOpener(string id)
    {
        ImGui.Selectable($"###{id}", false, ImGuiSelectableFlags.AllowOverlap);
        ImGui.SameLine();
        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
        {
            ImGui.OpenPopup("CollisionPropContextMenu");
        }
    }

    private void DisplayContextMenu(string name, string description, FieldInfo prop)
    {
        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
        {
            ImGui.OpenPopup("CollisionPropContextMenu");
        }

        if (ImGui.BeginPopup("CollisionPropContextMenu"))
        {
            if (ImGui.Selectable(@"Copy Property Name##CopyPropName"))
            {
                PlatformUtils.Instance.SetClipboardText(name);
            }

            if (ImGui.Selectable(@"Copy Property Description##CopyPropDesc"))
            {
                PlatformUtils.Instance.SetClipboardText(description);
            }

            if (ImGui.Selectable(@"Copy Property Type##CopyPropType"))
            {
                var reflectedType = prop.ReflectedType;
                if (reflectedType != null)
                {
                    PlatformUtils.Instance.SetClipboardText(reflectedType.FullName);
                }
            }

            ImGui.EndPopup();
        }
    }

    private void UpdateProperty(object prop, object obj, object oldval, object newval,
        bool changed, bool committed, int arrayindex = -1, int classIndex = -1)
    {
        if (changed)
        {
            ChangeProperty(prop, obj, oldval, newval, ref committed, arrayindex, classIndex);
        }

        if (committed)
        {
            if (_lastUncommittedAction != null && View.ViewportActionManager.PeekUndoAction() == _lastUncommittedAction)
            {
                if (_lastUncommittedAction is PropMultChangeAction a)
                {
                    View.ViewportActionManager.UndoAction();
                    View.ViewportActionManager.ExecuteAction(a);
                }

                _lastUncommittedAction = null;
                _changingProperty = null;
            }
        }
    }

    private void ChangeProperty(object prop, object obj, object oldval, object newval, 
        ref bool committed,
        int arrayindex = -1, int classIndex = -1)
    {
        if (prop == _changingProperty && _lastUncommittedAction != null &&
            View.ViewportActionManager.PeekUndoAction() == _lastUncommittedAction)
        {
            View.ViewportActionManager.UndoAction();
        }
        else
        {
            _lastUncommittedAction = null;
        }

        var action = new MapHavokPropChange(View, (FieldInfo)prop, obj, newval, arrayindex, classIndex);
        View.ViewportActionManager.ExecuteAction(action);

        _lastUncommittedAction = action;
        _changingProperty = prop;
    }

    private (bool, bool) PropertyRow(Type typ, object oldval, out object newval, FieldInfo prop)
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
        else if (typ == typeof(ulong))
        {
            var val = (ulong)oldval;
            var strval = $@"{val}";

            var input = new InputTextHandler(strval);

            if (input.Draw("##value", out string newValue))
            {
                var res = ulong.TryParse(newValue, out val);
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

            if (ImGui.DragFloat3("##value", ref val, 0.1f))
            {
                newval = val;
                isChanged = true;
            }
        }
        else if (typ == typeof(Vector4))
        {
            var val = (Vector4)oldval;

            if (ImGui.DragFloat4("##value", ref val, 0.1f))
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
            else if (typ.GetEnumUnderlyingType() == typeof(sbyte))
            {
                for (var i = 0; i < enumVals.Length; i++)
                {
                    intVals[i] = (sbyte)enumVals.GetValue(i);
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
                //Smithbox.Log(this,
                //    $"Color property in \"{prop.DeclaringType}\" does not declare if it supports Alpha. Alpha will be exposed by default",
                //    LogLevel.Warning, LogPriority.Low);

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
            ImGui.Text($"ImplementMe: {prop.FieldType}");
        }

        var isDeactivatedAfterEdit = ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive();

        return (isChanged, isDeactivatedAfterEdit);
    }

    private void PropEditorGenericList(
        HavokClass havokMeta,
        object obj,
        FieldInfo prop,
        Type elementType,
        string fieldName,
        string fieldDescription,
        ImGuiTreeNodeFlags treeFlags,
        int classIndex
    )
    {
        var list = (IList)prop.GetValue(obj);

        var open = ImGui.TreeNodeEx($@"{fieldName}", treeFlags);
        GUI.Tooltip(fieldDescription);
        ImGui.NextColumn();

        if (list != null)
        {
            if (ImGui.Button("+##addListEntry"))
            {
                var newEntry = PropFinderUtil.CreateDefaultListElement(elementType);
                var action = new MapHavokAddListEntryAction(prop, obj, newEntry, list.Count);
                View.ViewportActionManager.ExecuteAction(action);
            }
            GUI.Tooltip("Add a new entry to the end of this list.");
        }
        ImGui.NextColumn();

        if (open)
        {
            if (list != null)
            {
                var removeIndex = -1;

                for (var i = 0; i < list.Count; i++)
                {
                    ImGui.PushID(i);
                    var idx = i;
                    var elem = list[i];
                    void OnRemove() => removeIndex = idx;

                    if (elementType.IsClass && elementType != typeof(string) && !elementType.IsArray)
                    {
                        var classOpen = ImGui.TreeNodeEx($@"{fieldName}: {i}", treeFlags);
                        GUI.Tooltip(fieldDescription);
                        ImGui.NextColumn();
                        ImGui.SetNextItemWidth(-1);
                        ImGui.Text(elem?.GetType().Name ?? "null");

                        ImGui.SameLine();
                        if (ImGui.Button("-##removeListEntry"))
                        {
                            OnRemove();
                        }
                        GUI.Tooltip("Remove this entry from the list.");

                        ImGui.NextColumn();

                        if (CFG.Current.HavokEditor_Properties_Display_Type_Column)
                        {
                            PropContextRowOpener("listTypeCol");

                            ImGui.Text(elem?.GetType().FullName);

                            DisplayContextMenu(fieldName, fieldDescription, prop);

                            ImGui.NextColumn();
                        }

                        if (classOpen)
                        {
                            if (elem != null)
                                HavokPropEditGeneric(elem, havokMeta, idx);

                            ImGui.TreePop();
                        }
                    }
                    else
                    {
                        // Handle property display (and search filtering)
                        if (DisplayProperty(havokMeta, obj, prop, elementType))
                        {
                            PropGenericFieldRow(prop, elementType, havokMeta, elem, obj, $@"{fieldName}[{i}]", fieldDescription, i, classIndex, OnRemove);
                        }
                    }

                    ImGui.PopID();
                }

                if (removeIndex != -1)
                {
                    var action = new MapHavokRemoveListEntryAction(prop, obj, removeIndex);
                    View.ViewportActionManager.ExecuteAction(action);
                }
            }

            ImGui.TreePop();
        }

        ImGui.PopID();
    }
}
