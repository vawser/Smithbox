using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StudioCore.Editors.Common;

public static class SearchFilters
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

    public static bool IsMapSearchMatch(string rawInput, string mapId, string mapAlias, List<string> mapTags)
    {
        bool match = false;
        List<string> MapTags = mapTags;
        string input = rawInput.Trim().ToLower();
        string MapID = mapId.ToLower();
        string MapName = mapAlias.ToLower();
        if (input.Equals(""))
        {
            match = true; // If input is empty, show all
            return match;
        }
        string[] inputParts = input.Split("|");
        for (int i = 0; i < inputParts.Length; i++)
        {
            string entry = inputParts[i];

            string fuzzyEntry = ApplyFuzzyTransform(entry);

            // Match: Map ID (original entry)
            if (entry == MapID)
                match = true;

            // Match: Map ID (fuzzy transformed entry)
            if (fuzzyEntry != entry && fuzzyEntry == MapID)
                match = true;

            // Contains: Map ID (original entry)
            if (MapID.Contains(entry))
                match = true;

            // Contains: Map ID (fuzzy transformed entry)
            if (fuzzyEntry != entry && MapID.Contains(fuzzyEntry))
                match = true;

            // Match: Map ID parts
            var MapIDParts = MapID.Split("_");
            foreach (var refPart in MapIDParts)
            {
                var part = refPart.Replace("m", "");

                // Original entry matching
                if (entry == refPart)
                    match = true;
                if (refPart.Contains(entry))
                    match = true;

                // Fuzzy entry matching
                if (fuzzyEntry != entry)
                {
                    if (fuzzyEntry == refPart)
                        match = true;
                    if (refPart.Contains(fuzzyEntry))
                        match = true;
                }
            }

            // Match: Alias Name (original entry)
            if (entry == MapName)
                match = true;

            // Contains: Map Name (original entry)
            if (MapName.Contains(entry))
                match = true;

            // Match: Alias Name parts
            var MapNameParts = MapName.Split(" ");
            foreach (var refPart in MapNameParts)
            {
                if (entry == refPart)
                    match = true;
                if (refPart.Contains(entry))
                    match = true;
            }

            // Match: Tags
            if (MapTags != null)
            {
                foreach (string tagStr in MapTags)
                {
                    if (tagStr.ToLower().Contains(entry))
                        match = true;
                    if (entry == tagStr.ToLower())
                        match = true;
                }
            }
        }
        return match;
    }

    private static string ApplyFuzzyTransform(string input)
    {
        if (input.Length <= 2 || !input.All(char.IsDigit))
            return input;

        StringBuilder result = new StringBuilder();
        for (int i = 0; i < input.Length; i++)
        {
            if (i > 0 && i % 2 == 0)
                result.Append('_');
            result.Append(input[i]);
        }
        return result.ToString();
    }
}