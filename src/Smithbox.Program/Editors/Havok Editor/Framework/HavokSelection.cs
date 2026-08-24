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
    public HavokPropertyViewType PropertyViewType = HavokPropertyViewType.Structured;

    // File-specific State
    public bool AppliedHavokTreeSearch = false;

    public HavokSelection(HavokEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void ClearSelection()
    {
        View.Selection.BinderFileEntry = null;
        View.Selection.FilePath = null;

        AppliedHavokTreeSearch = false;

        View.PropertyView.BehaviorView.ResetSelection();
    }

    public void ClearFileSelection()
    {
        View.Selection.FilePath = null;

        AppliedHavokTreeSearch = false;

        View.PropertyView.BehaviorView.ResetSelection();
    }

    public void ApplyFileSpecificTreeSearches(object sourceObject)
    {
        if (AppliedHavokTreeSearch)
            return;

        View.PropertyView.BehaviorView.SetupBehaviorView(sourceObject);

        AppliedHavokTreeSearch = true;
    }
}
