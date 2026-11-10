using SoulsFormats;
using StudioCore.Application;

namespace StudioCore.Editors.MaterialEditor;

public class MaterialSelection
{
    public MaterialEditorView Parent;
    public ProjectEntry Project;

    public MaterialSelection(MaterialEditorView view, ProjectEntry project)
    {
        Parent = view; 
        Project = project;
    }

    public MaterialSourceType SourceType { get; set; }

    /// <summary>
    /// What was selected under each source tab, so switching tabs swaps the selection rather than leaving
    /// the other tab's selection behind
    /// </summary>
    private readonly Dictionary<MaterialSourceType, SelectionState> SavedSelections = new();

    private record SelectionState(
        FileDictionaryEntry BinderEntry, MTDWrapper MTDWrapper, MATBINWrapper MATBINWrapper, string FileKey,
        MTD MTD, MATBIN MATBIN, MaterialFileWrapper FileWrapper, bool ScrollToContainer, bool ScrollToFile);

    /// <summary>
    /// Called by the source tab being drawn. Changing type keeps the old type's selection for its tab and
    /// brings back the new type's. A jump to a material briefly draws the old tab for a frame; the swap
    /// there and back leaves the jump's selection in place.
    /// </summary>
    public void SetSourceType(MaterialSourceType type)
    {
        if (SourceType == type)
            return;

        SavedSelections[SourceType] = new SelectionState(SelectedBinderEntry, MTDWrapper, MATBINWrapper,
            SelectedFileKey, SelectedMTD, SelectedMATBIN, SelectedFileWrapper, ScrollToContainer, ScrollToFile);

        SourceType = type;

        SavedSelections.TryGetValue(type, out var state);

        SelectedBinderEntry = state?.BinderEntry;
        MTDWrapper = state?.MTDWrapper;
        MATBINWrapper = state?.MATBINWrapper;
        SelectedFileKey = state?.FileKey ?? "";
        SelectedMTD = state?.MTD;
        SelectedMATBIN = state?.MATBIN;
        SelectedFileWrapper = state?.FileWrapper;
        ScrollToContainer = state?.ScrollToContainer ?? false;
        ScrollToFile = state?.ScrollToFile ?? false;

        // The restored file may have been deleted or renamed while the other tab was shown
        Revalidate();
    }

    /// <summary>
    /// The binder of the current source type
    /// </summary>
    public IMaterialBinder SelectedBinder => SourceType is MaterialSourceType.MTD ? MTDWrapper : MATBINWrapper;

    /// <summary>
    /// After files are added, removed or renamed: follows a renamed selected file, and drops a removed one
    /// </summary>
    public void Revalidate()
    {
        if (SelectedFileWrapper == null)
            return;

        if (SelectedBinder?.GetFile(SelectedFileWrapper.Key) == SelectedFileWrapper)
        {
            SelectedFileKey = SelectedFileWrapper.Key;
            return;
        }

        SelectedFileKey = "";
        SelectedFileWrapper = null;
        SelectedMTD = null;
        SelectedMATBIN = null;
    }

    /// <summary>
    /// Selects a file of the current binder
    /// </summary>
    public void SelectFile(MaterialFileWrapper file)
    {
        SelectedFileKey = file.Key;
        SelectedFileWrapper = file;
        SelectedMTD = (file as MTDFileWrapper)?.MTD;
        SelectedMATBIN = (file as MATBINFileWrapper)?.MATBIN;
        ScrollToFile = true;
    }

    public FileDictionaryEntry SelectedBinderEntry { get; set; }

    public MTDWrapper MTDWrapper { get; set; }

    public MATBINWrapper MATBINWrapper { get; set; }

    public string SelectedFileKey { get; set; }

    public MTD SelectedMTD { get; set; }

    public MATBIN SelectedMATBIN { get; set; }

    /// <summary>
    /// The wrapper for the selected MTD or MATBIN, used to track its modified state
    /// </summary>
    public MaterialFileWrapper SelectedFileWrapper { get; set; }

    // Focus
    public bool SelectFileListEntry = false;

    // Jump To
    public MaterialSourceType? RequestedSourceType = null;
    public bool ScrollToContainer = false;
    public bool ScrollToFile = false;

    /// <summary>
    /// Select a material file, switching the source tab and scrolling the lists to it
    /// </summary>
    public void JumpTo(MaterialBank bank, MaterialReference reference)
    {
        // Keeps the current tab's selection for when the user goes back to it
        SetSourceType(reference.SourceType);
        RequestedSourceType = reference.SourceType;

        SelectedBinderEntry = reference.BinderEntry;
        SelectedFileKey = reference.FileKey;
        SelectedFileWrapper = reference.File;

        SelectedMTD = null;
        SelectedMATBIN = null;

        if (reference.File is MTDFileWrapper mtdFile)
        {
            MTDWrapper = bank.MTDs[reference.BinderEntry];
            SelectedMTD = mtdFile.MTD;
        }

        if (reference.File is MATBINFileWrapper matbinFile)
        {
            MATBINWrapper = bank.MATBINs[reference.BinderEntry];
            SelectedMATBIN = matbinFile.MATBIN;
        }

        ScrollToContainer = true;
        ScrollToFile = true;
    }
}
