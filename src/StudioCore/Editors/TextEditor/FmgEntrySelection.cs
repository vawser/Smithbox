using SoulsFormats;
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

        public void HandleSelection(int currentId, List<FMG.Entry> entries)
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

                foreach(var entry in entries)
                {
                    var curId = entry.ID;

                    if (curId >= start && curId <= end)
                    {
                        if (EntryIds.Contains(curId) && _previousEntryId != curId)
                        {
                            EntryIds.Remove(curId);
                        }
                        else if (!EntryIds.Contains(curId))
                        {
                            EntryIds.Add(curId);
                        }
                    }
                }
            }
            // Multi-Select: Pick Select
            else if (InputTracker.GetKey(KeyBindings.Current.TEXT_Multiselect))
            {
                if (EntryIds.Contains(currentId) && _previousEntryId != currentId)
                {
                    EntryIds.Remove(currentId);
                }
                else if (!EntryIds.Contains(currentId))
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

        public void Shortcuts(List<FMG.Entry> entries)
        {
            // Select All
            if (InputTracker.GetKey(KeyBindings.Current.TEXT_SelectAll))
            {
                EntryIds = new List<int>();

                foreach (var entry in entries)
                {
                    var curId = entry.ID;

                    EntryIds.Add(curId);
                }
            }
        }
    }
}
