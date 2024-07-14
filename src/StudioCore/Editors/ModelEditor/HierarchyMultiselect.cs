using StudioCore.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor
{
    public class HierarchyMultiselect
    {
        public List<int> StoredIndices = new List<int>();

        public HierarchyMultiselect()
        {

        }

        public bool HasValidMultiselection()
        {
            if(StoredIndices.Count < 1)
            {
                return false;
            }

            return true;
        }

        public bool IsMultiselected(int index)
        {
            return StoredIndices.Contains(index);
        }

        public void HandleMultiselect(int storedIndex, int currentIndex)
        {
            // Multi-Select: Range Select
            if (InputTracker.GetKey(KeyBindings.Current.ModelEditor_Multiselect_Range))
            {
                var start = storedIndex;
                var end = currentIndex;

                if (end < start)
                {
                    start = currentIndex;
                    end = storedIndex;
                }

                for (int k = start; k < end; k++)
                {
                    if (StoredIndices.Contains(k))
                    {
                        StoredIndices.Remove(k);
                    }
                    else
                    {
                        StoredIndices.Add(k);
                    }
                }
            }
            // Multi-Select Mode
            else if (InputTracker.GetKey(KeyBindings.Current.ModelEditor_Multiselect))
            {
                if (StoredIndices.Contains(currentIndex))
                {
                    StoredIndices.Remove(currentIndex);
                }
                else
                {
                    StoredIndices.Add(currentIndex);
                }
            }
            // Reset Multi-Selection if normal selection occurs
            else
            {
                StoredIndices = new List<int>();
            }
        }
    }
}
