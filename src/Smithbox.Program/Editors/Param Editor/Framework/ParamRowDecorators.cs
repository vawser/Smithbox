using Andre.Formats;
using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.TextEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;


public class ParamRowDecorators
{
    public ParamEditorScreen Editor;
    public ProjectEntry Project;
    public ParamView ParentView;

    public Dictionary<string, FmgRowDecorator> _rowFmgDecorators = new();

    private string CurrentParam = "";

    public ParamRowDecorators(ParamEditorScreen editor, ProjectEntry project, ParamView curView)
    {
        Editor = editor;
        Project = project;
        ParentView = curView;
    }

    /// <summary>
    /// Setup the Text Param Row decorators
    /// </summary>
    public void SetupFmgDecorators(string targetParam = "")
    {
        if (targetParam != "")
        {
            if (CurrentParam != targetParam)
            {
                ClearFmgDecorators();

                CurrentParam = targetParam;

                _rowFmgDecorators.Clear();

                foreach (var entry in Project.Handler.ParamData.PrimaryBank.Params)
                {
                    var paramName = entry.Key;
                    var entries = ParamFmgUtils.GetRowDecoratorFmgEntries(Editor, paramName);

                    if (entries.Count != 0)
                    {
                        _rowFmgDecorators.Add(paramName, new FmgRowDecorator(Editor, paramName));
                    }
                }
            }
        }
        else
        {
            ClearFmgDecorators();
            _rowFmgDecorators.Clear();

            foreach (var entry in Project.Handler.ParamData.PrimaryBank.Params)
            {
                var paramName = entry.Key;
                var entries = ParamFmgUtils.GetRowDecoratorFmgEntries(Editor, paramName);

                if (entries.Count != 0)
                {
                    _rowFmgDecorators.Add(paramName, new FmgRowDecorator(Editor, paramName));
                }
            }
        }
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
        FmgRowDecorator decorator = null;

        if (_rowFmgDecorators.ContainsKey(activeParam))
        {
            decorator = _rowFmgDecorators[activeParam];
        }

        return decorator;
    }
}

#region Row Decorator: FMG
public class FmgRowDecorator
{
    private ParamEditorScreen Editor;

    private readonly Dictionary<int, FMG.Entry> _entryCache = new();

    private string FmgName;
    private string CommandLine;

    public FmgRowDecorator(ParamEditorScreen editor, string paramName)
    {
        Editor = editor;
        FmgName = paramName;
        CommandLine = "";
    }

    public void ClearDecoratorCache()
    {
        _entryCache.Clear();
    }


    public void DecorateParam(Param.Row row)
    {
        PopulateDecorator();
        FMG.Entry entry = null;
        _entryCache.TryGetValue(row.ID, out entry);

        if (entry != null)
        {
            ImGui.SameLine();
            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_FmgRef_Text);
            ImGui.TextUnformatted($@" <{entry.Text}>");
            ImGui.PopStyleColor();
        }
    }
    public void DecorateContextMenuItems(Param.Row row)
    {
        PopulateDecorator();
        if (!_entryCache.ContainsKey(row.ID))
        {
            return;
        }

        if (Editor.Project.Handler.TextEditor == null)
            return;

        if (CommandLine == "")
        {
            var category = CFG.Current.TextEditor_Primary_Category.ToString();
            if (_entryCache.Values.Count > 0)
            {
                var cachedEntry = _entryCache.Values.Where(e => e.ID == row.ID).FirstOrDefault();

                var containerName = "";
                var fmg = cachedEntry.Parent;
                var fmgName = fmg.Name;

                foreach (var (path, entry) in Editor.Project.Handler.TextData.PrimaryBank.Containers)
                {
                    if (entry.ContainerDisplayCategory == CFG.Current.TextEditor_Primary_Category)
                    {

                        foreach (var fmgInfo in entry.FmgWrappers)
                        {
                            var grouping = TextUtils.GetFmgGrouping(Editor.Project, entry, fmgInfo.ID, fmgInfo.Name);

                            if (grouping == "Title" || grouping == "Common")
                            {
                                if (fmgInfo.Name == fmgName)
                                {
                                    containerName = entry.FileEntry.Filename;
                                }
                            }
                        }
                    }
                }

                CommandLine = $@"text/select/{category}/{containerName}/{fmgName}/{row.ID}";
            }
        }
        else
        {
            if (CommandLine != "")
            {
                if (ImGui.Selectable($@"Go to Text"))
                {
                    EditorCommandQueue.AddCommand(CommandLine);
                }
            }
        }
    }


    // NOTE: FMG Descriptor version
    //public void DecorateContextMenuItems(Param.Row row)
    //{
    //    PopulateDecorator();

    //    if (!_entryCache.ContainsKey(row.ID))
    //    {
    //        return;
    //    }

    //    if (Editor.Project.Handler.TextEditor == null)
    //        return;

    //    if (CommandLine == "")
    //    {
    //        var language = CFG.Current.Text_Editor_Primary_Language;

    //        if (_entryCache.Values.Count > 0)
    //        {
    //            var cachedEntry = _entryCache.Values.Where(e => e.ID == row.ID).FirstOrDefault();

    //            var fmgDescriptors = Editor.Project.Handler.TextData.FmgDescriptors;

    //            var containerName = "";
    //            var fmg = cachedEntry.Parent;
    //            var fmgName = fmg.Name;

    //            foreach (var entry in Editor.Project.Handler.TextData.PrimaryBank.Containers)
    //            {
    //                var container = entry.Value;

    //                if (container.Language.Language != language)
    //                    continue;

    //                foreach (var fmgWrapper in container.FmgWrappers)
    //                {
    //                    if (fmgWrapper.Name == fmgName)
    //                    {
    //                        containerName = container.Descriptor.FileName;
    //                    }
    //                }
    //            }

    //            CommandLine = $@"text/select/{language}/{containerName}/{fmgName}/{row.ID}";
    //        }
    //    }
    //    else
    //    {
    //        if (CommandLine != "")
    //        {
    //            if (ImGui.Selectable($@"Go to Text"))
    //            {
    //                EditorCommandQueue.AddCommand(CommandLine);
    //            }
    //        }
    //    }
    //}

    private void PopulateDecorator()
    {
        // FMG Name decoration on row 
        if (_entryCache.Count == 0)
        {
            List<FMG.Entry> fmgEntries = ParamFmgUtils.GetRowDecoratorFmgEntries(Editor, FmgName);

            foreach (FMG.Entry fmgEntry in fmgEntries)
            {
                _entryCache[fmgEntry.ID] = fmgEntry;
            }
        }
    }
}
#endregion