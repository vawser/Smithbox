using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace StudioCore.Application;

public class ChangeAliasTag : EditorAction
{
    private readonly List<string> TagSource;
    private readonly string NewContents;
    private readonly string OldContents;
    private readonly int Index;

    public ChangeAliasTag(List<string> tagSource, int index, string newContents)
    {
        TagSource = tagSource;
        Index = index;

        var curContents = tagSource[index];

        NewContents = newContents;
        OldContents = curContents;
    }

    public override ActionEvent Execute()
    {
        TagSource[Index] = NewContents;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        TagSource[Index] = OldContents;

        return ActionEvent.NoEvent;
    }
}