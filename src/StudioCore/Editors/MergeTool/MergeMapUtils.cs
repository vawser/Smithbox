using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MergeTool;

public static class MergeMapUtils
{
    public static MSBE BuildDiffMap(MSBE currentMap, MSBE targetMap)
    {
        // Build a MSBE instance that only contains the differences between current and target MSBE
        MSBE diffMap = new MSBE();

        DiffParts(ref diffMap, currentMap, targetMap);

        // Heuristics:
        // Start from target-side first (since we are bringing in data from that)

        // Check for indices first (e.g. root Parts[0] == target Parts[0])
        // -> If that fails, check by Name (e.g. root Part.Name == target Part.Name)
        // -> If no match is found, then entry is likely unique to target side: add option to bring entry into root side

        // If match is found: check inner properties for differences
        // -> If inner properties are different, add option to bring 'target-side' differences only into the root side version

        return diffMap;
    }

    private static void DiffParts(ref MSBE diffMap, MSBE currentMap, MSBE targetMap)
    {
        // MapPieces
        for (int i = 0; i < targetMap.Parts.MapPieces.Count; i++)
        {
            MSBE.Part.MapPiece tPart = targetMap.Parts.MapPieces[i];
            MSBE.Part.MapPiece cPart = null;

            // Check by index
            if (currentMap.Parts.MapPieces.Count >= i)
            {
                cPart = currentMap.Parts.MapPieces[i];
            }

            if(tPart == cPart)
            {
                // No difference
            }
            else
            {
                // Check via name now
                cPart = currentMap.Parts.MapPieces.Where(e => e.Name == tPart.Name).FirstOrDefault();

                if (tPart == cPart)
                {
                    // No difference
                }
            }
        }
    }
}
