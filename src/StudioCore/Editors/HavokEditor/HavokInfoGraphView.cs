using HKLib.hk2018;
using ImGuiNET;
using StudioCore.Editors.MapEditor;
using StudioCore.HavokEditor;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.HavokEditor;

public class HavokInfoGraphView
{
    private HavokEditorScreen Screen;
    private HavokPropertyEditor PropertyEditor;

    public HavokInfoGraphView(HavokEditorScreen screen)
    {
        Screen = screen;
        PropertyEditor = new HavokPropertyEditor(screen);
    }

    private int depth = 0;
    private IHavokObject _selectedHavokObject;

    public void DisplayGraph()
    {
        var loadedFile = Screen.SelectedContainerInfo.LoadedFile;

        if (loadedFile != null)
        {
            foreach (var entry in loadedFile.m_namedVariants)
            {
                hkbProjectData hkbProjectData = (hkbProjectData)entry.m_variant;

                hkbProjectDataView(hkbProjectData, depth);
            }
        }
    }

    public void hkbProjectDataView(hkbProjectData entry, int depth)
    {
        if (entry == null)
            return;

        depth++;

        if (ImGui.Selectable($"hkbProjectData##hkbProjectData{depth}"))
        {
            _selectedHavokObject = entry;
        }

        hkbProjectStringDataView(entry.m_stringData, depth);
    }

    public void hkbProjectStringDataView(hkbProjectStringData entry, int depth)
    {
        if (entry == null)
            return;

        depth++;

        if (ImGui.Selectable($"hkbProjectStringData##hkbProjectStringData{depth}"))
        {
            _selectedHavokObject = entry;
        }
    }

    public void DisplayProperties()
    {
        if (_selectedHavokObject == null)
            return;

        if(_selectedHavokObject.GetType() == typeof(hkbProjectData))
        {
            hkbProjectDataEdit((hkbProjectData)_selectedHavokObject);
        }
        if (_selectedHavokObject.GetType() == typeof(hkbProjectStringData))
        {
            hkbProjectStringDataEdit((hkbProjectStringData)_selectedHavokObject);
        }
    }

    public void hkbProjectDataEdit(hkbProjectData entry)
    {
        if (entry == null)
            return;

        ImGui.Columns(2);
        
        ImGui.AlignTextToFramePadding();
        UIHelper.WrappedText($"worldUpWS");
        ImGui.AlignTextToFramePadding();
        UIHelper.WrappedText($"defaultEventMode");

        ImGui.NextColumn();

        entry.m_worldUpWS = (Vector4)PropertyEditor.EditProperty("m_worldUpWS", entry.m_worldUpWS);
        entry.m_defaultEventMode = (hkbTransitionEffect.EventMode)PropertyEditor.EditProperty("m_defaultEventMode", entry.m_defaultEventMode);

        ImGui.Columns(1);
    }
    public void hkbProjectStringDataEdit(hkbProjectStringData entry)
    {
        if (entry == null)
            return;

        ImGui.Columns(2);

        ImGui.AlignTextToFramePadding();
        UIHelper.WrappedText($"animationPath");

        ImGui.AlignTextToFramePadding();
        UIHelper.WrappedText($"behaviorPath");

        ImGui.AlignTextToFramePadding();
        UIHelper.WrappedText($"characterPath");

        ImGui.AlignTextToFramePadding();
        UIHelper.WrappedText($"scriptsPath");

        ImGui.AlignTextToFramePadding();
        UIHelper.WrappedText($"fullPathToSource");
        
        ImGui.AlignTextToFramePadding();
        UIHelper.WrappedText($"behaviorFilenames");
        foreach (var count in entry.m_behaviorFilenames)
        {
            ImGui.AlignTextToFramePadding();
            UIHelper.WrappedText("");
        }

        ImGui.AlignTextToFramePadding();
        UIHelper.WrappedText($"characterFilenames");
        foreach (var count in entry.m_characterFilenames)
        {
            ImGui.AlignTextToFramePadding();
            UIHelper.WrappedText("");
        }

        ImGui.AlignTextToFramePadding();
        UIHelper.WrappedText($"eventNames");
        foreach (var count in entry.m_eventNames)
        {
            ImGui.AlignTextToFramePadding();
            UIHelper.WrappedText("");
        }

        ImGui.NextColumn();

        entry.m_animationPath = (string)PropertyEditor.EditProperty("m_animationPath", entry.m_animationPath);
        entry.m_behaviorPath = (string)PropertyEditor.EditProperty("m_behaviorPath", entry.m_behaviorPath);
        entry.m_characterPath = (string)PropertyEditor.EditProperty("m_characterPath", entry.m_characterPath);
        entry.m_scriptsPath = (string)PropertyEditor.EditProperty("m_scriptsPath", entry.m_scriptsPath);
        entry.m_fullPathToSource = (string)PropertyEditor.EditProperty("m_fullPathToSource", entry.m_fullPathToSource);

        entry.m_behaviorFilenames = (List<string>)PropertyEditor.EditProperty("m_behaviorFilenames", entry.m_behaviorFilenames);
        entry.m_characterFilenames = (List<string>)PropertyEditor.EditProperty("m_characterFilenames", entry.m_characterFilenames);
        entry.m_eventNames = (List<string>)PropertyEditor.EditProperty("m_eventNames", entry.m_eventNames);

        ImGui.Columns(1);
    }
}
