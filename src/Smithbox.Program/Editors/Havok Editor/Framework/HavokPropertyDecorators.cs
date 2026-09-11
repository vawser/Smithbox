using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.ParamEditor;
using System.Reflection;

namespace StudioCore.Editors.HavokEditor;

public static class HavokPropertyDecorators
{
    public static bool AddVariableBindingSet(ProjectEntry project, IEditorView view, FieldInfo[] fields, Type type, HavokClass classMeta, object sourceObj)
    {
        if (HavokTypeUtils.IsHKX3(project))
        {
            if (type != typeof(HKLib.hk2018.hkbClipGenerator))
                return false;
        }

        if (HavokTypeUtils.IsHKX2(project))
        {
            if (type != typeof(HKX2.hkbClipGenerator))
                return false;
        }

        if (classMeta == null)
            return false;

        if (!classMeta.SupportVariableBindings)
            return false;

        FieldInfo bindingField = null;

        if (HavokTypeUtils.IsHKX3(project))
        {
            bindingField = fields.FirstOrDefault(e => e.FieldType == typeof(HKLib.hk2018.hkbVariableBindingSet));
        }
        else if (HavokTypeUtils.IsHKX2(project))
        {
            bindingField = fields.FirstOrDefault(e => e.FieldType == typeof(HKX2.hkbVariableBindingSet));
        }

        if (bindingField == null)
            return false;

        if (view is HavokEditorView havokEditorView)
        {
            // Not present
            if (bindingField.GetValue(sourceObj) == null)
            {
                ImGui.Text("Variable Binding Set");

                ImGui.NextColumn();

                if (ImGui.Button("Add##addVariableBindingSet"))
                {
                    if (HavokTypeUtils.IsHKX3(project))
                    {
                        var newBindingSet = Activator.CreateInstance<HKLib.hk2018.hkbVariableBindingSet>();

                        var action = new HavokChangeField(bindingField, sourceObj, newBindingSet, -1, -1);
                        havokEditorView.ActionManager.ExecuteAction(action);
                    }
                    else if (HavokTypeUtils.IsHKX2(project))
                    {
                        var newBindingSet = Activator.CreateInstance<HKX2.hkbVariableBindingSet>();

                        var action = new HavokChangeField(bindingField, sourceObj, newBindingSet, -1, -1);
                        havokEditorView.ActionManager.ExecuteAction(action);
                    }
                }
                GUI.Tooltip("Adds an empty Variable Binding Set to this generator.");

                ImGui.NextColumn();

                if (CFG.Current.HavokEditor_Properties_Display_Type_Column)
                {
                    ImGui.NextColumn();
                }
            }
            // Present
            else
            {
                ImGui.Text("Variable Binding Set");

                ImGui.NextColumn();

                if (ImGui.Button("Remove##removeVariableBindingSet"))
                {
                    if (HavokTypeUtils.IsHKX3(project))
                    {
                        var newBindingSet = Activator.CreateInstance<HKLib.hk2018.hkbVariableBindingSet>();

                        var action = new HavokChangeField(bindingField, sourceObj, newBindingSet, -1, -1);
                        havokEditorView.ActionManager.ExecuteAction(action);
                    }
                    else if(HavokTypeUtils.IsHKX2(project))
                    {
                        var newBindingSet = Activator.CreateInstance<HKX2.hkbVariableBindingSet>();

                        var action = new HavokChangeField(bindingField, sourceObj, newBindingSet, -1, -1);
                        havokEditorView.ActionManager.ExecuteAction(action);
                    }
                }
                GUI.Tooltip("Adds an empty Variable Binding Set to this generator.");

                ImGui.NextColumn();

                if (CFG.Current.HavokEditor_Properties_Display_Type_Column)
                {
                    ImGui.NextColumn();
                }
            }
        }

        return false;
    }

    public static bool ParamRefRow(IEditorView view, HavokClass havokMeta, FieldInfo prop, object val, ref object newObj)
    {
        ParamEditorView activeView = null;

        if(view is MapEditorView mapEditorView)
        {
            if (mapEditorView.Project.Handler.ParamEditor == null)
                return false;

            activeView = mapEditorView.Project.Handler.ParamEditor.ViewHandler.ActiveView;
        }

        if (view is HavokEditorView havokEditorView)
        {
            if (havokEditorView.Project.Handler.ParamEditor == null)
                return false;

            activeView = havokEditorView.Project.Handler.ParamEditor.ViewHandler.ActiveView;
        }

        if (activeView == null)
            return false;

        if (havokMeta == null)
            return false;

        var fieldMeta = havokMeta.Fields.FirstOrDefault(f => f.Field == prop.Name);
        if (fieldMeta == null)
        {
            return false;
        }

        if (fieldMeta.ParamRef == "")
            return false;

        List<ParamRef> refs = new()
        {
            new ParamRef(null, fieldMeta.ParamRef)
        };

        ImGui.NextColumn();

        ParamReferenceHelper.Label(activeView, refs, null);

        ImGui.NextColumn();

        if (activeView.Project.Handler.ParamEditor != null)
        {
            ParamReferenceHelper.Hint(activeView, refs, null, val);
            ParamReferenceHelper.Click(activeView, val, null, refs);

            if (ImGui.BeginPopupContextItem($"{prop.Name}EnumContextMenu"))
            {
                var opened = ParamReferenceHelper.ContextMenu(activeView, refs, null, val, ref newObj, null);
                ImGui.EndPopup();
                return opened;
            }
        }

        if (view is MapEditorView)
        {
            if (CFG.Current.MapEditor_HavokEdit_Display_Type_Column)
            {
                ImGui.NextColumn();
            }
        }
        else if (view is HavokEditorView)
        {
            if (CFG.Current.HavokEditor_Properties_Display_Type_Column)
            {
                ImGui.NextColumn();
            }
        }

        return false;
    }

    public static bool ClipGenFlags(ProjectEntry project, IEditorView view, HavokClass havokMeta, FieldInfo prop, object val, ref object newObj, object sourceObj, ref object newval)
    {
        if (havokMeta == null)
            return false;

        var fieldMeta = havokMeta.Fields.FirstOrDefault(f => f.Field == prop.Name);
        if (fieldMeta == null)
        {
            return false;
        }

        if (!fieldMeta.ClipGeneratorFlags)
            return false;

        ImGui.NextColumn();
        ImGui.NextColumn();

        int flags = Convert.ToInt32(val);
        bool changed = false;

        if (HavokTypeUtils.IsHKX3(project))
        {
            foreach (HKLib.hk2018.hkbClipGenerator.ClipFlags flag in Enum.GetValues(typeof(HKLib.hk2018.hkbClipGenerator.ClipFlags)))
            {
                int flagVal = (int)flag;
                bool isSet = (flags & flagVal) != 0;

                if (ImGui.Checkbox(flag.ToString(), ref isSet))
                {
                    if (isSet)
                        flags |= flagVal;
                    else
                        flags &= ~flagVal;

                    changed = true;
                }
            }
        }
        else if (HavokTypeUtils.IsHKX2(project))
        {
            foreach (HKX2.hkbClipGenerator.ClipFlags flag in Enum.GetValues(typeof(HKX2.hkbClipGenerator.ClipFlags)))
            {
                int flagVal = (int)flag;
                bool isSet = (flags & flagVal) != 0;

                if (ImGui.Checkbox(flag.ToString(), ref isSet))
                {
                    if (isSet)
                        flags |= flagVal;
                    else
                        flags &= ~flagVal;

                    changed = true;
                }
            }
        }

        if (changed)
        {
            newval = (sbyte)flags;
            return true;
        }

        if (view is MapEditorView)
        {
            if (CFG.Current.MapEditor_HavokEdit_Display_Type_Column)
            {
                ImGui.NextColumn();
            }
        }
        else if (view is HavokEditorView)
        {
            if (CFG.Current.HavokEditor_Properties_Display_Type_Column)
            {
                ImGui.NextColumn();
            }
        }

        return false;
    }

    public static bool ClipGenInternalID(ProjectEntry project, IEditorView view, HavokClass havokMeta, FieldInfo prop, object val, ref object newObj, object sourceObj, ref object newval)
    {
        if (havokMeta == null)
            return false;

        var fieldMeta = havokMeta.Fields.FirstOrDefault(f => f.Field == prop.Name);
        if (fieldMeta == null)
        {
            return false;
        }

        if (!fieldMeta.AnimationInternalID)
            return false;

        if (view is HavokEditorView havokEditorView)
        {
            ImGui.NextColumn();
            ImGui.NextColumn();

            if (ImGui.Button("Set to Free ID"))
            {
                short freeNum = -1;

                if (HavokTypeUtils.IsHKX3(project))
                {
                    var objects = HavokTreeSearch.FindValueList<HKLib.hk2018.hkbClipGenerator>(sourceObj, havokEditorView.PropertyCache.GetCachedHavokFields, "m_animationInternalId", typeof(short));

                    var shorts = objects.Cast<short>().ToList();


                    for (short i = 0; i < short.MaxValue; i++)
                    {
                        if (!shorts.Contains(i))
                        {
                            freeNum = i;
                            break;
                        }
                    }
                }
                else if (HavokTypeUtils.IsHKX2(project))
                {
                    var objects = HavokTreeSearch.FindValueList<HKX2.hkbClipGenerator>(sourceObj, havokEditorView.PropertyCache.GetCachedHavokFields, "m_animationInternalId", typeof(short));

                    var shorts = objects.Cast<short>().ToList();


                    for (short i = 0; i < short.MaxValue; i++)
                    {
                        if (!shorts.Contains(i))
                        {
                            freeNum = i;
                            break;
                        }
                    }
                }

                if (freeNum == -1)
                {
                    Smithbox.LogError(typeof(HavokPropertyDecorators), "No free ID to assign.");
                    return false;
                }
                else
                {
                    newval = (short)(freeNum);
                    return true;
                }
            }
            GUI.Tooltip("Set the value to the next available unused ID.");

            if (CFG.Current.HavokEditor_Properties_Display_Type_Column)
            {
                ImGui.NextColumn();
            }
        }

        return false;
    }
}
