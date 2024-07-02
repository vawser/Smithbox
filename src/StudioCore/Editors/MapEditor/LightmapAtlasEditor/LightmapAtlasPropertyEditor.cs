using Google.Protobuf.WellKnownTypes;
using ImGuiNET;
using SoulsFormats;
using StudioCore.Editor;
using StudioCore.GraphicsEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static SoulsFormats.GPARAM;

namespace StudioCore.Editors.MapEditor.LightmapAtlasEditor;

public class LightmapAtlasPropertyEditor
{
    private ActionManager EditorActionManager = new();

    private static object _editedValueCache;

    // Value has been changed via input
    private static bool _changedCache;

    // Value can be changed in the BTAB
    private static bool _committedCache;

    // Value to change without commit
    private static bool _uncommittedCache;

    public LightmapAtlasPropertyEditor()
    {

    }

    public void AtlasID(BTAB.Entry entry, int idx)
    {
        _changedCache = false;
        _committedCache = false;
        _uncommittedCache = false;

        ImGui.SetNextItemWidth(-1);

        int newValue = -1;

        // int
        int fieldValue = entry.AtlasID;
        int intInput = fieldValue;
        int oldValue = fieldValue;

        if (ImGui.InputInt($"##value{idx}", ref intInput))
        {
            newValue = intInput;

            _editedValueCache = newValue;
            _changedCache = true;
        }
        _committedCache = ImGui.IsItemDeactivatedAfterEdit();

        if (_changedCache)
        {
            if (_committedCache)
            {
                if (newValue != -1)
                {
                    LightmapAtlasChangeAtlasID action = null;
                    action = new LightmapAtlasChangeAtlasID(LightmapAtlasScreen._selectedParentEntry, LightmapAtlasScreen._selectedEntry, newValue, oldValue);

                    EditorActionManager.ExecuteAction(action);
                }
            }
        }
    }
    public void PartName(BTAB.Entry entry, int idx)
    {
        _changedCache = false;
        _committedCache = false;
        _uncommittedCache = false;

        ImGui.SetNextItemWidth(-1);

        string newValue = null;

        // string
        string fieldValue = entry.PartName;
        string strInput = fieldValue;
        string oldValue = fieldValue;

        if (ImGui.InputText($"##value{idx}", ref strInput, 255))
        {
            newValue = strInput;

            _editedValueCache = newValue;
            _changedCache = true;
        }
        _committedCache = ImGui.IsItemDeactivatedAfterEdit();

        if (_changedCache)
        {
            if (_committedCache)
            {
                if (newValue != null)
                {
                    LightmapAtlasChangePartName action = null;
                    action = new LightmapAtlasChangePartName(LightmapAtlasScreen._selectedParentEntry, LightmapAtlasScreen._selectedEntry, newValue, oldValue);

                    EditorActionManager.ExecuteAction(action);
                }
            }
        }
    }

    public void MaterialName(BTAB.Entry entry, int idx)
    {
        _changedCache = false;
        _committedCache = false;
        _uncommittedCache = false;

        ImGui.SetNextItemWidth(-1);

        string newValue = null;

        // string
        string fieldValue = entry.MaterialName;
        string strInput = fieldValue;
        string oldValue = fieldValue;

        if (ImGui.InputText($"##value{idx}", ref strInput, 255))
        {
            newValue = strInput;

            _editedValueCache = newValue;
            _changedCache = true;
        }
        _committedCache = ImGui.IsItemDeactivatedAfterEdit();

        if (_changedCache)
        {
            if (_committedCache)
            {
                if (newValue != null)
                {
                    LightmapAtlasChangeMaterialName action = null;
                    action = new LightmapAtlasChangeMaterialName(LightmapAtlasScreen._selectedParentEntry, LightmapAtlasScreen._selectedEntry, newValue, oldValue);

                    EditorActionManager.ExecuteAction(action);
                }
            }
        }
    }
    public void UVOffset(BTAB.Entry entry, int idx)
    {
        _changedCache = false;
        _committedCache = false;
        _uncommittedCache = false;

        ImGui.SetNextItemWidth(-1);

        Vector2 newValue = new Vector2(-1, -1);

        // vector2
        Vector2 fieldValue = entry.UVOffset;
        Vector2 vector2Input = fieldValue;
        Vector2 oldValue = fieldValue;

        if (ImGui.InputFloat2($"##value{idx}", ref vector2Input))
        {
            newValue = vector2Input;

            _editedValueCache = newValue;
            _changedCache = true;
        }
        _committedCache = ImGui.IsItemDeactivatedAfterEdit();

        if (_changedCache)
        {
            if (_committedCache)
            {
                if (newValue.X != -1 && newValue.Y != -1)
                {
                    LightmapAtlasChangeUVOffset action = null;
                    action = new LightmapAtlasChangeUVOffset(LightmapAtlasScreen._selectedParentEntry, LightmapAtlasScreen._selectedEntry, newValue, oldValue);

                    EditorActionManager.ExecuteAction(action);
                }
            }
        }
    }
    public void UVScale(BTAB.Entry entry, int idx)
    {
        _changedCache = false;
        _committedCache = false;
        _uncommittedCache = false;

        ImGui.SetNextItemWidth(-1);

        Vector2 newValue = new Vector2(-1, -1);

        // vector2
        Vector2 fieldValue = entry.UVScale;
        Vector2 vector2Input = fieldValue;
        Vector2 oldValue = fieldValue;

        if (ImGui.InputFloat2($"##value{idx}", ref vector2Input))
        {
            newValue = vector2Input;

            _editedValueCache = newValue;
            _changedCache = true;
        }
        _committedCache = ImGui.IsItemDeactivatedAfterEdit();

        if (_changedCache)
        {
            if (_committedCache)
            {
                if (newValue.X != -1 && newValue.Y != -1)
                {
                    LightmapAtlasChangeUVScale action = null;
                    action = new LightmapAtlasChangeUVScale(LightmapAtlasScreen._selectedParentEntry, LightmapAtlasScreen._selectedEntry, newValue, oldValue);

                    EditorActionManager.ExecuteAction(action);
                }
            }
        }
    }
}
