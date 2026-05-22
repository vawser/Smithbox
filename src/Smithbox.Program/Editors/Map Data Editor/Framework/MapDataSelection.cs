using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using static HKLib.hk2018.hkSerialize.CompatTypeParentInfo;

namespace StudioCore.Editors.MapDataEditor;

public class MapDataSelection
{
    public MapDataEditorView View;
    public ProjectEntry Project;

    public SubEditorType SubEditorMode { get; set; } = SubEditorType.MSB;

    // Map Data
    public FileDictionaryEntry SelectedMapDescriptor { get; set; }
    public IMsb SelectedMap { get; set; }

    public bool SelectMapEntry = false;

    public string SelectedBaseCategory;
    public Type SelectedBaseCategoryType;

    public string SelectedSubCategory;
    public Type SelectedSubCategoryType;
    public SortedDictionary<int, object> SelectedEntries { get; set; } = new();

    // Entry File Lists
    public FileDictionaryEntry SelectedListDescriptor { get; set; }
    public ENFL SelectedList { get; set; }

    public bool SelectListEntry = false;

    public MapDataSelection(MapDataEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void ResetMsbSelection()
    {
        SelectedBaseCategory = null;
        SelectedBaseCategoryType = null;
        SelectedSubCategory = null;
        SelectedSubCategoryType = null;
        SelectedEntries.Clear();
    }

    public void ResetMsbEntrySelection()
    {
        SelectedEntries.Clear();
    }

    public void HandleMsbEntrySelection(int selectedIndex, int curListIndex, object entry)
    {
        // Multi-Select: Range Select
        if (InputManager.HasShiftDown())
        {
            var start = selectedIndex;
            var end = curListIndex;

            if (end < start)
            {
                start = curListIndex;
                end = selectedIndex;
            }

            for (int k = start; k <= end; k++)
            {
                if (!SelectedEntries.ContainsKey(k))
                {
                    SelectedEntries.Add(k, entry);
                }
            }
        }
        // Multi-Select Mode
        else if (InputManager.HasCtrlDown())
        {
            if (SelectedEntries.ContainsKey(curListIndex) && SelectedEntries.Count > 1)
            {
                SelectedEntries.Remove(curListIndex);
            }
            else
            {
                if (!SelectedEntries.ContainsKey(curListIndex))
                {
                    SelectedEntries.Add(curListIndex, entry);
                }
            }
        }
        // Reset Multi-Selection if normal selection occurs
        else
        {
            SelectedEntries.Clear();
            SelectedEntries.Add(curListIndex, entry);
        }
    }
}

public enum SubEditorType
{
    [Display(Name = "Map Data")]
    MSB,
    [Display(Name = "Entry File List")]
    ENFL

    // TODO:
    // Lights: BTL
    // Invasion Points: AIP
    // Light Atlases: BTAB
    // Light Probes: BTPB
    // Navmesh: NVA
}