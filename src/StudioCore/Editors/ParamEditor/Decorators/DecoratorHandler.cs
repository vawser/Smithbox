using StudioCore.Core;
using StudioCore.Editors.TextEditor.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor.Decorators;

public class DecoratorHandler
{
    public ParamEditorScreen Editor;
    public ProjectEntry Project;

    public Dictionary<string, FmgRowDecorator> _rowFmgDecorators = new();

    private bool InitializedFmgDecorator = false;


    public DecoratorHandler(ParamEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    /// <summary>
    /// Initialize decorators that should fill up once and then wait for invalidation
    /// </summary>
    public void Initialize()
    {
        if (Project.TextEditor != null)
        {
            if (!InitializedFmgDecorator)
            {
                InitializedFmgDecorator = true;

                SetupFmgDecorators();
            }
        }
    }

    /// <summary>
    /// Setup the Text Param Row decorators
    /// </summary>
    public void SetupFmgDecorators()
    {
        _rowFmgDecorators.Clear();
        foreach (var entry in Project.ParamData.PrimaryBank.Params)
        {
            var paramName = entry.Key;
            var entries = TextParamUtils.GetFmgEntriesByAssociatedParam(Editor, paramName);

            if (entries.Count != 0)
            {
                _rowFmgDecorators.Add(paramName, new FmgRowDecorator(Editor, paramName));
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

        InitializedFmgDecorator = false;
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
