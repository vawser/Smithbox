using ImGuiNET;
using SoulsFormats;
using StudioCore.Editor;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;

namespace StudioCore.TextEditor;

public class PropertyEditor
{
    private FMGBank.EntryGroup _eGroupCache;
    private FMG.Entry _entryCache;

    private int _fmgID;

    private int _idCache = -1;

    private Dictionary<string, PropertyInfo[]> _propCache = new();

    private string _textCache = "";
    public ActionManager ContextActionManager;

    public PropertyEditor(ActionManager manager)
    {
        ContextActionManager = manager;
    }

    private bool PropertyRow(Type typ, object oldval, out object newval, bool isBool)
    {
        try
        {
            if (isBool)
            {
                dynamic val = oldval;
                bool checkVal = val > 0;
                if (ImGui.Checkbox("##valueBool", ref checkVal))
                {
                    newval = Convert.ChangeType(checkVal ? 1 : 0, oldval.GetType());
                    return true;
                }

                ImGui.SameLine();
            }
        }
        catch
        {
        }

        if (typ == typeof(long))
        {
            var val = (long)oldval;
            var strval = $@"{val}";
            if (ImGui.InputText("##value", ref strval, 128))
            {
                var res = long.TryParse(strval, out val);
                if (res)
                {
                    newval = val;
                    return true;
                }
            }
        }
        else if (typ == typeof(int))
        {
            var val = (int)oldval;
            if (ImGui.InputInt("##value", ref val))
            {
                newval = val;
                return true;
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
                    return true;
                }
            }
        }
        else if (typ == typeof(short))
        {
            int val = (short)oldval;
            if (ImGui.InputInt("##value", ref val))
            {
                newval = (short)val;
                return true;
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
                    return true;
                }
            }
        }
        else if (typ == typeof(sbyte))
        {
            int val = (sbyte)oldval;
            if (ImGui.InputInt("##value", ref val))
            {
                newval = (sbyte)val;
                return true;
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
                    return true;
                }
            }
        }
        else if (typ == typeof(bool))
        {
            var val = (bool)oldval;
            if (ImGui.Checkbox("##value", ref val))
            {
                newval = val;
                return true;
            }
        }
        else if (typ == typeof(float))
        {
            var val = (float)oldval;
            if (ImGui.DragFloat("##value", ref val, 0.1f))
            {
                newval = val;
                return true;
                // shouldUpdateVisual = true;
            }
        }
        else if (typ == typeof(string))
        {
            var val = (string)oldval;
            if (val == null)
            {
                val = "";
            }

            if (ImGui.InputText("##value", ref val, 128))
            {
                newval = val;
                return true;
            }
        }
        else if (typ == typeof(Vector2))
        {
            var val = (Vector2)oldval;
            if (ImGui.DragFloat2("##value", ref val, 0.1f))
            {
                newval = val;
                return true;
                // shouldUpdateVisual = true;
            }
        }
        else if (typ == typeof(Vector3))
        {
            var val = (Vector3)oldval;
            if (ImGui.DragFloat3("##value", ref val, 0.1f))
            {
                newval = val;
                return true;
                // shouldUpdateVisual = true;
            }
        }
        else
        {
            ImGui.Text("ImplementMe");
        }

        newval = null;
        return false;
    }

    private void UpdateProperty(object prop, object obj, object newval,
        bool changed = true, bool committed = true, int arrayindex = -1)
    {
        if (changed)
        {
            ChangeProperty(prop, obj, newval, ref committed, arrayindex);
        }
    }

    private void ChangeProperty(object prop, object obj, object newval,
        ref bool committed, int arrayindex = -1)
    {
        if (committed)
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
        }
    }

    public void PropEditorFMGBegin()
    {
        _fmgID = 0;
        ImGui.Columns(2);
        ImGui.Separator();
    }

    public void PropEditorFMG(FMG.Entry entry, string name)
    {
        ImGui.PushID(_fmgID);
        ImGui.AlignTextToFramePadding();
        ImGui.Text(name);
        ImGui.NextColumn();
        var oldval = entry.Text;

        var val = oldval;
        if (val == null)
        {
            val = "";
        }

        var height = (20.0f + ImGui.CalcTextSize(val).Y) * MapStudioNew.GetUIScale();

        if (ImGui.InputTextMultiline("##value", ref val, 2000, new Vector2(-1, height)))
        {
            _textCache = val;
            _entryCache = entry;
        }

        var committed = ImGui.IsItemDeactivatedAfterEdit();
        if (committed && _entryCache != null)
        {
            if (_textCache != _entryCache.Text)
            {
                UpdateProperty(_entryCache.GetType().GetProperty("Text"), _entryCache, _textCache);
                _entryCache = null;
            }
        }

        ImGui.NextColumn();
        ImGui.PopID();
        _fmgID++;
    }

    public void PropIDFMG(FMGBank.EntryGroup eGroup, List<FMG.Entry> entryCache)
    {
        var oldID = eGroup.ID;
        var id = oldID;
        if (ImGui.InputInt("##id", ref id))
        {
            _idCache = id;
            _eGroupCache = eGroup;
        }

        var committed = ImGui.IsItemDeactivatedAfterEdit();
        if (committed && _eGroupCache != null)
        {
            if (_idCache != oldID)
            {
                // Forbid duplicate IDs
                if (entryCache.Find(e => e.ID == _idCache) == null)
                {
                    UpdateProperty(_eGroupCache.GetType().GetProperty("ID"), _eGroupCache, _idCache);
                    _eGroupCache = null;
                }
            }
        }
    }

    public void PropEditorFMGEnd()
    {
        ImGui.Columns(1);
    }
}
