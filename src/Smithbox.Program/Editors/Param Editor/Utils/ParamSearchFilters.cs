using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public static class ParamSearchFilters
{
    public static bool IsEditorSearchMatch(string rawInput, string checkInput, string delimiter)
    {
        bool match = false;
        string cleanRawInput = rawInput.Trim().ToLower();
        string cleanCheckInput = checkInput.Trim().ToLower();

        if (cleanRawInput.Equals(""))
        {
            match = true; // If input is empty, show all
            return match;
        }

        string[] inputParts = cleanRawInput.Split("+");

        bool[] partTruth = new bool[inputParts.Length];

        for (int i = 0; i < partTruth.Length; i++)
        {
            string entry = inputParts[i];

            if (entry == cleanCheckInput)
                partTruth[i] = true;

            var refParts = cleanCheckInput.Split($"{delimiter}");

            foreach (var refPart in refParts)
            {
                if (entry == refPart)
                {
                    partTruth[i] = true;
                }
            }
        }

        match = true;

        foreach (bool entry in partTruth)
        {
            if (!entry)
                match = false;
        }

        return match;
    }
}
