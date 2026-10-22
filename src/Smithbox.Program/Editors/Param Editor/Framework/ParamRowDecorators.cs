using Andre.Formats;
using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.ParamEditor;


public class ParamRowDecorators
{
    public ParamEditorScreen Editor;
    public ProjectEntry Project;
    public ParamEditorView ParentView;

    public Dictionary<string, FmgRowDecorator> _rowFmgDecorators = new();

    /// <summary>
    /// The param the decorators were last built for.
    /// Empty means decorators were built for all params (or none are built).
    /// </summary>
    private string CachedParam = "";

    public ParamRowDecorators(ParamEditorScreen editor, ProjectEntry project, ParamEditorView curView)
    {
        Editor = editor;
        Project = project;
        ParentView = curView;
    }

    /// <summary>
    /// Called when the Text Editor wants the decorator text updated.
    /// Forces a rebuild for whatever is currently active (the selected param, or all params).
    /// </summary>
    public void Refresh()
    {
        Rebuild(CachedParam);
    }

    /// <summary>
    /// Called when a param is selected.
    /// Does nothing if the decorator for this param is already built, otherwise rebuilds.
    /// </summary>
    public void RefreshFmgDecorator(string targetParam = "")
    {
        if (!CFG.Current.ParamEditor_Row_List_Display_Decorators)
        {
            ClearAll();
            return;
        }

        // Same param selected again and its decorator already exists: keep it.
        if (targetParam != "" &&
            CachedParam == targetParam &&
            _rowFmgDecorators.ContainsKey(targetParam))
        {
            return;
        }

        Rebuild(targetParam);
    }

    private void Rebuild(string targetParam)
    {
        ClearAll();

        if (!CFG.Current.ParamEditor_Row_List_Display_Decorators)
            return;

        CachedParam = targetParam;

        if (targetParam != "")
        {
            TryAddDecorator(targetParam);
        }
        else
        {
            foreach (var entry in Project.Handler.ParamData.PrimaryBank.Params)
            {
                TryAddDecorator(entry.Key);
            }
        }
    }

    private void TryAddDecorator(string paramName)
    {
        var entries = ParamFmgUtils.GetRowDecoratorFmgEntries(Editor, paramName);

        if (entries.Count != 0)
        {
            _rowFmgDecorators[paramName] = new FmgRowDecorator(Editor, paramName);
        }
    }

    private void ClearAll()
    {
        ClearFmgDecorators();
        _rowFmgDecorators.Clear();
        CachedParam = "";
    }

    /// <summary>
    /// Clear the Text Param Row decorators when the text editor primary language changes
    /// </summary>
    public void ClearFmgDecorators()
    {
        foreach (KeyValuePair<string, FmgRowDecorator> dec in _rowFmgDecorators)
        {
            dec.Value.ClearDecoratorCache();
        }
    }

    public FmgRowDecorator GetFmgRowDecorator(string activeParam)
    {
        _rowFmgDecorators.TryGetValue(activeParam, out var decorator);
        return decorator;
    }
}

#region Row Decorator: FMG
public class FmgRowDecorator
{
    private ParamEditorScreen Editor;

    private readonly Dictionary<int, FMG.Entry> _entryCache = new();

    // Go to Text command lines, cached per row ID
    private readonly Dictionary<int, string> _commandLineCache = new();

    private bool _populated = false;

    private string FmgName;

    public FmgRowDecorator(ParamEditorScreen editor, string paramName)
    {
        Editor = editor;
        FmgName = paramName;
    }

    public void ClearDecoratorCache()
    {
        _entryCache.Clear();
        _commandLineCache.Clear();
        _populated = false;
    }


    public void DecorateParam(Param.Row row)
    {
        if (!CFG.Current.ParamEditor_Row_List_Display_Decorators)
            return;

        PopulateDecorator();

        if (_entryCache.TryGetValue(row.ID, out var entry))
        {
            ImGui.SameLine();
            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_FmgRef_Text);
            ImGui.TextUnformatted($@" <{entry.Text}>");
            ImGui.PopStyleColor();
        }
    }

    public FMG.Entry GetRowFmgEntry(Param.Row row)
    {
        PopulateDecorator();

        _entryCache.TryGetValue(row.ID, out var entry);
        return entry;
    }

    public void DecorateContextMenuItems(Param.Row row)
    {
        if (!CFG.Current.ParamEditor_Row_List_Display_Decorators)
            return;

        PopulateDecorator();

        if (!_entryCache.ContainsKey(row.ID))
            return;

        if (Editor.Project.Handler.TextEditor == null)
            return;

        // Build the command line for this specific row, once
        if (!_commandLineCache.TryGetValue(row.ID, out var commandLine))
        {
            commandLine = ParamFmgUtils.GetFmgRefCommandLine(Editor, _entryCache, row) ?? "";
            _commandLineCache[row.ID] = commandLine;
        }

        if (commandLine == "")
            return;

        // Go to Text
        if (ImGui.Selectable($"{LOC.Get("PARAM_RowDecorator_Action_Go_To_Text")}##goToTextAction"))
        {
            EditorCommandQueue.AddCommand(commandLine);
        }
        GUI.Tooltip(LOC.Get("PARAM_RowDecorator_Action_Go_To_Text_TT"));
    }

    private void PopulateDecorator()
    {
        if (_populated)
            return;

        _populated = true;

        List<FMG.Entry> fmgEntries = ParamFmgUtils.GetRowDecoratorFmgEntries(Editor, FmgName);

        foreach (FMG.Entry fmgEntry in fmgEntries)
        {
            _entryCache[fmgEntry.ID] = fmgEntry;
        }
    }
}
#endregion