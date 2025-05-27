using StudioCore.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BehaviorEditorNS;

public class BehaviorFieldChange : EditorAction
{
    private readonly FieldInfo FieldInfo;
    private readonly object Instance;
    private readonly object NewValue;
    private readonly object OldValue;
    private Action<bool> PostExecutionAction;

    public BehaviorFieldChange(FieldInfo fieldInfo, object instance, object curValue, object newValue)
    {
        FieldInfo = fieldInfo;
        Instance = instance;
        NewValue = newValue;
        OldValue = curValue;
    }

    public override ActionEvent Execute()
    {
        FieldInfo.SetValue(Instance, NewValue);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        FieldInfo.SetValue(Instance, OldValue);

        return ActionEvent.NoEvent;
    }
}