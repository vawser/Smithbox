using Andre.Formats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.ParamEditor;


public class AddParamsAction : EditorAction
{
    private ParamEditorScreen Editor;
    private readonly bool appOnly;
    private readonly List<Param.Row> Clonables = new();
    private readonly List<Param.Row> Clones = new();
    private readonly int InsertIndex;
    private readonly Param Param;
    private readonly List<Param.Row> Removed = new();
    private readonly List<int> RemovedIndex = new();
    private readonly bool replParams;
    private string ParamString;
    private int IdOffset;
    private bool IsDuplicate;
    private Action<bool> PostExecutionAction;

    public AddParamsAction(ParamEditorScreen editor, Param param, string pstring, List<Param.Row> rows, bool appendOnly, bool replaceParams, int index = -1, int idOffset = 1, bool isDuplicate = false)
    {
        Editor = editor;
        Param = param;
        Clonables.AddRange(rows);
        ParamString = pstring;
        appOnly = appendOnly;
        replParams = replaceParams;
        InsertIndex = index;
        IdOffset = idOffset;
        IsDuplicate = isDuplicate;
    }

    public override ActionEvent Execute()
    {
        foreach (Param.Row row in Clonables)
        {
            var newrow = new Param.Row(row);

            var paramMeta = Editor.Project.ParamData.GetParamMeta(row.Def);

            // Only apply for Duplicate action
            if (IsDuplicate)
            {
                foreach (var cell in newrow.Cells)
                {
                    var meta = Editor.Project.ParamData.GetParamFieldMeta(paramMeta, cell.Def);
                    var adjust = false;

                    if (meta.DeepCopyTargetType != null)
                    {
                        // Attack
                        if (CFG.Current.Param_Toolbar_Duplicate_AffectAttackField)
                        {
                            adjust = true;
                        }

                        // Bullet
                        if (CFG.Current.Param_Toolbar_Duplicate_AffectBulletField)
                        {
                            adjust = true;
                        }

                        // Behavior
                        if (CFG.Current.Param_Toolbar_Duplicate_AffectBehaviorField)
                        {
                            adjust = true;
                        }

                        // SpEffect
                        if (CFG.Current.Param_Toolbar_Duplicate_AffectSpEffectField)
                        {
                            adjust = true;
                        }

                        // Origin
                        if (CFG.Current.Param_Toolbar_Duplicate_AffectSourceField)
                        {
                            adjust = true;
                        }

                        if (adjust)
                        {
                            // Assuming ints here
                            int id = (int)cell.Value;

                            // Ignore if value is -1
                            if (id != -1)
                            {
                                id = id + IdOffset;

                                cell.Value = id;
                            }
                        }
                    }
                }
            }

            if (InsertIndex > -1)
            {
                newrow.Name = row.Name != null ? row.Name + "_1" : "";
                Param.InsertRow(InsertIndex, newrow);
            }
            else
            {
                if (Param[row.ID] != null)
                {
                    if (replParams)
                    {
                        Param.Row existing = Param[row.ID];
                        RemovedIndex.Add(Param.IndexOfRow(existing));
                        Removed.Add(existing);
                        Param.RemoveRow(existing);
                    }
                    else
                    {
                        var rowIdOffset = IdOffset;
                        if (rowIdOffset <= 0)
                            rowIdOffset = 1;

                        newrow.Name = row.Name != null ? row.Name + "_1" : "";
                        var newID = row.ID + rowIdOffset;
                        while (Param[newID] != null)
                        {
                            newID += rowIdOffset;
                        }

                        newrow.ID = newID;
                        Param.InsertRow(Param.IndexOfRow(Param[newID - rowIdOffset]) + 1, newrow);
                    }
                }

                if (Param[row.ID] == null)
                {
                    newrow.Name = row.Name != null ? row.Name : "";
                    if (appOnly)
                    {
                        Param.AddRow(newrow);
                    }
                    else
                    {
                        var index = 0;
                        foreach (Param.Row r in Param.Rows)
                        {
                            if (r.ID > newrow.ID)
                            {
                                break;
                            }

                            index++;
                        }

                        Param.InsertRow(index, newrow);
                    }
                }
            }

            Clones.Add(newrow);
        }

        Editor.Project.ParamData.RefreshParamDifferenceCacheTask();

        var activeParam = Editor._activeView.Selection.GetActiveParam();
        Editor._activeView.TableGroupView.UpdateTableSelection(activeParam);

        return ActionEvent.NoEvent;
    }

    public List<Param.Row> GetResultantRows()
    {
        return Clones;
    }

    public override ActionEvent Undo()
    {
        for (var i = 0; i < Clones.Count(); i++)
        {
            Param.RemoveRow(Clones[i]);
        }

        for (var i = Removed.Count() - 1; i >= 0; i--)
        {
            Param.InsertRow(RemovedIndex[i], Removed[i]);
        }

        Clones.Clear();
        RemovedIndex.Clear();
        Removed.Clear();

        var activeParam = Editor._activeView.Selection.GetActiveParam();
        Editor._activeView.TableGroupView.UpdateTableSelection(activeParam);

        return ActionEvent.NoEvent;
    }
    public void SetPostExecutionAction(Action<bool> action)
    {
        PostExecutionAction = action;
    }
}
