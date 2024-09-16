using ImGuiNET;
using SoulsFormats;
using StudioCore.Editor;
using System.Numerics;

namespace StudioCore.Editors.MapEditor.LightmapAtlasEditor;

public class LightmapAtlasPropertyEditor
{
    private ViewportActionManager EditorActionManager = new();

    private static object _editedValueCache;

    // Value has been changed via input
    private static bool _changedCache;

    // Value can be changed in the BTAB
    private static bool _committedCache;

    // Value to change without commit
    private static bool _uncommittedCache;

    private LightmapAtlasScreen AtlasScreen;

    public LightmapAtlasPropertyEditor(LightmapAtlasScreen atlasScreen)
    {
        AtlasScreen = atlasScreen;
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
                    EditorActionManager.ExecuteAction(new LightmapAtlasChangeAtlasID(AtlasScreen._selectedParentEntry, AtlasScreen._selectedEntry, newValue, oldValue));
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
                    EditorActionManager.ExecuteAction(new LightmapAtlasChangePartName(AtlasScreen._selectedParentEntry, AtlasScreen._selectedEntry, newValue, oldValue));
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
                    EditorActionManager.ExecuteAction(new LightmapAtlasChangeMaterialName(AtlasScreen._selectedParentEntry, AtlasScreen._selectedEntry, newValue, oldValue));
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
                    EditorActionManager.ExecuteAction(new LightmapAtlasChangeUVOffset(AtlasScreen._selectedParentEntry, AtlasScreen._selectedEntry, newValue, oldValue));
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
                    EditorActionManager.ExecuteAction(new LightmapAtlasChangeUVScale(AtlasScreen._selectedParentEntry, AtlasScreen._selectedEntry, newValue, oldValue));
                }
            }
        }
    }
}
