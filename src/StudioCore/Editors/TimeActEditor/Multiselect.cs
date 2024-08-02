using StudioCore.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TimeActEditor;

public class Multiselect
{
    public List<int> _storedIndices = new List<int>();

    public Multiselect() { }

    public bool HasValidMultiselection()
    {
        if (_storedIndices.Count < 1)
        {
            return false;
        }

        return true;
    }

    public bool IsMultiselected(int index)
    {
        return _storedIndices.Contains(index);
    }

    public void HandleMultiselect(int currentSelectionIndex, int currentIndex)
    {
        // Multi-Select: Range Select
        if (InputTracker.GetKey(Veldrid.Key.LShift))
        {
            var start = currentSelectionIndex;
            var end = currentIndex;

            if (end < start)
            {
                start = currentIndex;
                end = currentSelectionIndex;
            }

            for (int k = start; k <= end; k++)
            {
                if (_storedIndices.Contains(k))
                {
                    _storedIndices.Remove(k);
                }
                else
                {
                    _storedIndices.Add(k);
                }
            }
        }
        // Multi-Select Mode
        else if (InputTracker.GetKey(KeyBindings.Current.TimeActEditor_Multiselect))
        {
            if (_storedIndices.Contains(currentIndex))
            {
                _storedIndices.Remove(currentIndex);
            }
            else
            {
                _storedIndices.Add(currentIndex);
            }
        }
        // Reset Multi-Selection if normal selection occurs
        else
        {
            _storedIndices = new List<int>();
        }
    }
}
