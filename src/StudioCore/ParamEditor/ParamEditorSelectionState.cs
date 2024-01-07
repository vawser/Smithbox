using Andre.Formats;
using StudioCore.Editor;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.ParamEditor;

public class ParamEditorSelectionState
{
    private static string _globalRowSearchString = "";
    private static string _globalPropSearchString = "";
    private readonly Dictionary<string, ParamEditorParamSelectionState> _paramStates = new();
    private readonly ParamEditorScreen _scr;

    private readonly List<(string, Param.Row)> pastStack = new();
    private string _activeParam;
    internal string currentParamSearchString = "";

    public ParamEditorSelectionState(ParamEditorScreen paramEditor)
    {
        _scr = paramEditor;
    }

    private void PushHistory(string newParam, Param.Row newRow)
    {
        if (pastStack.Count > 0)
        {
            (string, Param.Row) prev = pastStack[pastStack.Count - 1];
            if (prev.Item1 == newParam && prev.Item2 == null)
            {
                pastStack[pastStack.Count - 1] = (prev.Item1, newRow);
            }

            prev = pastStack[pastStack.Count - 1];
            if (prev.Item1 == newParam && prev.Item2 == newRow)
            {
                return;
            }
        }

        if (_activeParam != null)
        {
            pastStack.Add((_activeParam, _paramStates[_activeParam].activeRow));
        }

        if (pastStack.Count >= 6)
        {
            pastStack.RemoveAt(0);
        }
    }

    public void PopHistory()
    {
        if (pastStack.Count > 0)
        {
            (string, Param.Row) past = pastStack[pastStack.Count - 1];
            pastStack.RemoveAt(pastStack.Count - 1);
            if (past.Item2 == null && pastStack.Count > 0)
            {
                past = pastStack[pastStack.Count - 1];
                pastStack.RemoveAt(pastStack.Count - 1);
            }

            SetActiveParam(past.Item1, true);
            SetActiveRow(past.Item2, true, true);
        }
    }

    public bool HasHistory()
    {
        return pastStack.Count > 0;
    }

    public bool ActiveParamExists()
    {
        return _activeParam != null;
    }

    public string GetActiveParam()
    {
        return _activeParam;
    }

    public void SetActiveParam(string param, bool isHistory = false)
    {
        if (!isHistory)
        {
            PushHistory(param, null);
        }

        _activeParam = param;
        if (!_paramStates.ContainsKey(_activeParam))
        {
            _paramStates.Add(_activeParam, new ParamEditorParamSelectionState());
        }
    }

    public ref string GetCurrentRowSearchString()
    {
        if (_activeParam == null)
        {
            return ref _globalRowSearchString;
        }

        return ref _paramStates[_activeParam].currentRowSearchString;
    }

    public ref string GetCurrentPropSearchString()
    {
        if (_activeParam == null)
        {
            return ref _globalPropSearchString;
        }

        return ref _paramStates[_activeParam].currentPropSearchString;
    }

    public void SetCurrentRowSearchString(string s)
    {
        if (_activeParam == null)
        {
            return;
        }

        _paramStates[_activeParam].currentRowSearchString = s;
        _paramStates[_activeParam].selectionCacheDirty = true;
    }

    public void SetCurrentPropSearchString(string s)
    {
        if (_activeParam == null)
        {
            return;
        }

        _paramStates[_activeParam].currentPropSearchString = s;
    }

    public bool RowSelectionExists()
    {
        return _activeParam != null && _paramStates[_activeParam].selectionRows.Count > 0;
    }

    public Param.Row GetActiveRow()
    {
        if (_activeParam == null)
        {
            return null;
        }

        return _paramStates[_activeParam].activeRow;
    }

    public Param.Row GetCompareRow()
    {
        if (_activeParam == null)
        {
            return null;
        }

        return _paramStates[_activeParam].compareRow;
    }

    public Param.Column GetCompareCol()
    {
        if (_activeParam == null)
        {
            return null;
        }

        return _paramStates[_activeParam].compareCol;
    }

    public void SetActiveRow(Param.Row row, bool clearSelection, bool isHistory = false)
    {
        if (_activeParam != null)
        {
            ParamEditorParamSelectionState s = _paramStates[_activeParam];
            if (s.activeRow != null && !ParamBank.VanillaBank.IsLoadingParams)
            {
                ParamBank.PrimaryBank.RefreshParamRowDiffs(s.activeRow, _activeParam);
            }

            if (!isHistory)
            {
                PushHistory(_activeParam, s.activeRow);
            }

            s.activeRow = row;
            s.selectionRows.Clear();
            s.selectionRows.Add(row);
            if (s.activeRow != null && !ParamBank.VanillaBank.IsLoadingParams)
            {
                ParamBank.PrimaryBank.RefreshParamRowDiffs(s.activeRow, _activeParam);
            }

            s.selectionCacheDirty = true;
        }
    }

    public void SetCompareRow(Param.Row row)
    {
        if (_activeParam != null)
        {
            ParamEditorParamSelectionState s = _paramStates[_activeParam];
            s.compareRow = row;
        }
    }

    public void SetCompareCol(Param.Column col)
    {
        if (_activeParam != null)
        {
            ParamEditorParamSelectionState s = _paramStates[_activeParam];
            s.compareCol = col;
        }
    }

    public void ToggleRowInSelection(Param.Row row)
    {
        if (_activeParam != null)
        {
            ParamEditorParamSelectionState s = _paramStates[_activeParam];
            if (s.selectionRows.Contains(row))
            {
                s.selectionRows.Remove(row);
            }
            else
            {
                s.selectionRows.Add(row);
            }

            s.selectionCacheDirty = true;
        }
        //Do not perform vanilla diff here, will be very slow when making large selections
    }

    public void AddRowToSelection(Param.Row row)
    {
        if (_activeParam != null)
        {
            ParamEditorParamSelectionState s = _paramStates[_activeParam];
            if (!s.selectionRows.Contains(row))
            {
                s.selectionRows.Add(row);
                s.selectionCacheDirty = true;
            }
        }
        //Do not perform vanilla diff here, will be very slow when making large selections
    }

    public void RemoveRowFromSelection(Param.Row row)
    {
        if (_activeParam != null)
        {
            _paramStates[_activeParam].selectionRows.Remove(row);
            _paramStates[_activeParam].selectionCacheDirty = true;
        }
    }

    public void RemoveRowFromAllSelections(Param.Row row)
    {
        foreach (ParamEditorParamSelectionState state in _paramStates.Values)
        {
            state.selectionRows.Remove(row);
            if (state.activeRow == row)
            {
                state.activeRow = null;
            }

            state.selectionCacheDirty = true;
        }
    }

    public List<Param.Row> GetSelectedRows()
    {
        if (_activeParam == null)
        {
            return null;
        }

        return _paramStates[_activeParam].selectionRows;
    }

    public bool[] GetSelectionCache(List<Param.Row> rows, string cacheVer)
    {
        if (_activeParam == null)
        {
            return null;
        }

        ParamEditorParamSelectionState s = _paramStates[_activeParam];
        // We maintain this flag as clearing the cache properly is slow for the number of times we modify selection
        if (s.selectionCacheDirty)
        {
            UICache.RemoveCache(_scr, s);
        }

        return UICache.GetCached(_scr, s, "selectionCache" + cacheVer, () =>
        {
            s.selectionCacheDirty = false;
            return rows.Select(x => GetSelectedRows().Contains(x)).ToArray();
        });
    }

    public void CleanSelectedRows()
    {
        if (_activeParam != null)
        {
            ParamEditorParamSelectionState s = _paramStates[_activeParam];
            s.selectionRows.Clear();
            if (s.activeRow != null)
            {
                s.selectionRows.Add(s.activeRow);
            }

            s.selectionCacheDirty = true;
        }
    }

    public void CleanAllSelectionState()
    {
        foreach (ParamEditorParamSelectionState s in _paramStates.Values)
        {
            s.selectionCacheDirty = true;
        }

        _activeParam = null;
        _paramStates.Clear();
    }

    public void SortSelection()
    {
        if (_activeParam != null)
        {
            ParamEditorParamSelectionState s = _paramStates[_activeParam];
            Param p = ParamBank.PrimaryBank.Params[_activeParam];
            s.selectionRows.Sort((a, b) => { return p.IndexOfRow(a) - p.IndexOfRow(b); });
        }
    }
}

internal class ParamEditorParamSelectionState
{
    internal Param.Row activeRow;
    internal Param.Column compareCol;
    internal Param.Row compareRow;
    internal string currentPropSearchString = "";
    internal string currentRowSearchString = "";
    internal bool selectionCacheDirty = true;

    internal List<Param.Row> selectionRows = new();
}
