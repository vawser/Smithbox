using HKLib.hk2018;
using System;
using System.Collections.Generic;
using System.Text;

namespace StudioCore.Editors.HavokEditor;

public class HavokSelection
{
    private HavokEditorView View;
    private ProjectEntry Project;

    public bool DoFocus = false;

    // Category List
    public HavokCategoryMode CategoryMode = HavokCategoryMode.None;

    // Binder List
    public FileDictionaryEntry BinderFileEntry;

    // File List
    public string FilePath;

    // Properties
    public HavokPropertyViewType PropertyViewType = HavokPropertyViewType.Flat;

    // File-specific State
    public bool AppliedHavokTreeSearchesForFile = false;
    public bool IsBehaviorGraph = false;

    public HavokSelection(HavokEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void ClearSelection()
    {
        View.Selection.BinderFileEntry = null;
        View.Selection.FilePath = null;

        AppliedHavokTreeSearchesForFile = false;
        IsBehaviorGraph = false;
    }

    public void ClearFileSelection()
    {
        View.Selection.FilePath = null;

        AppliedHavokTreeSearchesForFile = false;
        IsBehaviorGraph = false;
    }

    public void ApplyFileSpecificTreeSearches(object sourceObject)
    {
        if (AppliedHavokTreeSearchesForFile)
            return;

        // Tag as Behavior Graph is present in HKX
        var behaviorGraphs = HavokTreeSearch.FindAll<hkbBehaviorGraph>(sourceObject, View.PropertyCache.GetCachedHavokFields);

        if (behaviorGraphs.Count > 0)
            IsBehaviorGraph = true;

        AppliedHavokTreeSearchesForFile = true;
    }
}
