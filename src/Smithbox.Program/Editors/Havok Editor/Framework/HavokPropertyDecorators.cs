using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.ParamEditor;
using System.Reflection;

namespace StudioCore.Editors.HavokEditor;

public static class HavokPropertyDecorators
{
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

        // The context menu result is returned after the type column, so the columns stay aligned while it's open
        var changed = false;

        if (activeView.Project.Handler.ParamEditor != null)
        {
            ParamReferenceHelper.Hint(activeView, refs, null, val);
            ParamReferenceHelper.Click(activeView, val, null, refs);

            if (ImGui.BeginPopupContextItem($"{prop.Name}EnumContextMenu"))
            {
                changed = ParamReferenceHelper.ContextMenu(activeView, refs, null, val, ref newObj, null);
                ImGui.EndPopup();
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

        return changed;
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

        // The type column is still advanced on a change, so the rows below stay aligned
        if (changed)
        {
            newval = (sbyte)flags;
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

        return changed;
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

        var changed = false;

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
                }
                else
                {
                    newval = (short)(freeNum);
                    changed = true;
                }
            }
            GUI.Tooltip("Set the value to the next available unused ID.");

            // Advanced even after a click, so the rows below stay aligned
            if (CFG.Current.HavokEditor_Properties_Display_Type_Column)
            {
                ImGui.NextColumn();
            }
        }

        return changed;
    }
}
