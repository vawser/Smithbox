using StudioCore.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor
{
    public class FmgEntrySelection
    {
        public int _previousEntryId = -1;
        public int _currentEntryId = -1;
        public List<int> EntryIds = new List<int>();

        public FmgEntrySelection() { }

        public bool HasValidSelection()
        {
            if (EntryIds.Count < 1)
            {
                return false;
            }

            return true;
        }

        public bool IsSelected(int id)
        {
            return EntryIds.Contains(id);
        }

        public void HandleSelection(int currentId)
        {
            if(_previousEntryId == -1)
            {
                _previousEntryId = currentId;
            }
            else if (_currentEntryId == -1)
            {
                _currentEntryId = currentId;
            }
            else
            {
                _previousEntryId = _currentEntryId;
                _currentEntryId = currentId;
            }

            // Multi-Select: Range Select
            if (InputTracker.GetKey(Veldrid.Key.LShift))
            {
                var start = _previousEntryId;
                var end = _currentEntryId;

                if (end < start)
                {
                    start = _currentEntryId;
                    end = _previousEntryId;
                }

                for (int k = start; k <= end; k++)
                {
                    if (EntryIds.Contains(k) && _previousEntryId != k)
                    {
                        EntryIds.Remove(k);
                    }
                    else
                    {
                        EntryIds.Add(k);
                    }
                }
            }
            // Multi-Select: Pick Select
            else if (InputTracker.GetKey(Veldrid.Key.ControlLeft))
            {
                if (EntryIds.Contains(currentId) && _previousEntryId != currentId)
                {
                    EntryIds.Remove(currentId);
                }
                else
                {
                    EntryIds.Add(currentId);
                }
            }
            // Reset
            else
            {
                EntryIds = new List<int>();
                EntryIds.Add(currentId);
            }
        }
    }
}
