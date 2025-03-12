using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Utilities;

public static class SearchFilters
{
    /// <summary>
    /// Returns true is the input string (whole or part) matches the passed string.
    /// </summary>
    public static bool IsBasicMatch(string rawInput, string rawStr)
    {
        bool match = false;

        string input = rawInput.Trim().ToLower();
        string rawString = rawStr.ToLower();

        if (input.Equals(""))
        {
            match = true; // If input is empty, show all
            return match;
        }

        string[] inputParts = input.Split("+");
        bool[] partTruth = new bool[inputParts.Length];

        for (int i = 0; i < partTruth.Length; i++)
        {
            string entry = inputParts[i];

            if (entry == rawString)
                partTruth[i] = true;

            if (rawString.Contains(entry))
                partTruth[i] = true;
        }

        match = true;

        foreach (bool entry in partTruth)
        {
            if (!entry)
                match = false;
        }

        return match;
    }

    /// <summary>
    /// Returns true is the input string (whole or part) matches a filename, reference name or tag.
    /// </summary>
    public static bool IsSearchMatch(string rawInput, string rawRefId, string rawRefName, List<string> rawRefTags, 
        bool matchAssetCategory = false, // Match AEG categories passed in input
        bool stripParticlePrefix = false, // Remove f and preceding zeroes from checked string against input
        bool splitWithDelimiter = false, // Split entry by passed delimiter and check against input
        string delimiter = "_" // Delimiter to split entry by
        )
    {
        bool match = false;

        string input = rawInput.Trim().ToLower();
        string refId = rawRefId.ToLower();
        string refName = rawRefName.ToLower();

        if(rawRefTags == null)
        {
            rawRefTags = new List<string>() { "" };
        }

        if (input.Equals(""))
        {
            match = true; // If input is empty, show all
            return match;
        }

        string[] inputParts = input.Split("+");
        bool[] partTruth = new bool[inputParts.Length];

        for (int i = 0; i < partTruth.Length; i++)
        {
            string entry = inputParts[i];

            // Match: Filename/ID
            if (entry == refId)
                partTruth[i] = true;

            // Contains: Filename/ID
            if (refId.Contains(entry))
                partTruth[i] = true;

            // Match: Reference Name
            if (entry == refName)
                partTruth[i] = true;

            // Match: Reference Name
            if (refName.Contains(entry))
                partTruth[i] = true;

            if (splitWithDelimiter)
            {
                var refParts = refName.Split(delimiter);
                foreach(var refPart in refParts)
                {
                    if(entry == refPart)
                        partTruth[i] = true;

                    if (refPart.Contains(entry))
                        partTruth[i] = true;
                }
            }

            // Match: Reference Segments
            string[] refSegments = refName.Split(" ");
            foreach (string refStr in refSegments)
            {
                string curString = refStr;

                // Remove common brackets so the match ignores them
                if (curString.Contains('('))
                    curString = curString.Replace("(", "");

                if (curString.Contains(')'))
                    curString = curString.Replace(")", "");

                if (curString.Contains('{'))
                    curString = curString.Replace("{", "");

                if (curString.Contains('}'))
                    curString = curString.Replace("}", "");

                if (curString.Contains('('))
                    curString = curString.Replace("(", "");

                if (curString.Contains('['))
                    curString = curString.Replace("[", "");

                if (curString.Contains(']'))
                    curString = curString.Replace("]", "");

                if (entry == curString.Trim())
                    partTruth[i] = true;
            }

            // Match: Tags
            foreach (string tagStr in rawRefTags)
            {
                if (entry == tagStr.ToLower())
                    partTruth[i] = true;

                if (tagStr.ToLower().Contains(entry))
                    partTruth[i] = true;
            }

            // Match: AEG Category
            if (matchAssetCategory)
            {
                if (!entry.Equals("") && entry.All(char.IsDigit))
                {
                    if (refId.Contains("aeg") && refId.Contains("_"))
                    {
                        string[] parts = refId.Split("_");
                        string aegCategory = parts[0].Replace("aeg", "");

                        if (entry == aegCategory)
                        {
                            partTruth[i] = true;
                        }
                    }
                }
            }
        }

        match = true;

        foreach(bool entry in partTruth)
        {
            if (!entry)
                match = false;
        }

        return match;
    }

    public static bool IsAssetBrowserSearchMatch(string rawInput, string rawRefId, string rawRefName, List<string> rawRefTags)
    {
        bool match = false;

        string input = rawInput.Trim().ToLower();
        string refId = rawRefId.ToLower();
        string refName = rawRefName.ToLower();

        if (rawRefTags == null)
        {
            rawRefTags = new List<string>() { "" };
        }

        if (input.Equals(""))
        {
            match = true; // If input is empty, show all
            return match;
        }

        string[] inputParts = input.Split("+");
        bool[] partTruth = new bool[inputParts.Length];

        for (int i = 0; i < partTruth.Length; i++)
        {
            string entry = inputParts[i];

            // Match: ID
            if (entry == refId)
                partTruth[i] = true;

            // Contains: ID
            if (refId.Contains(entry))
                partTruth[i] = true;

            var refIdParts = refId.Split("_");
            foreach (var refIdPart in refIdParts)
            {
                if (entry == refIdPart)
                    partTruth[i] = true;

                if (refIdPart.Contains(entry))
                    partTruth[i] = true;
            }

            // Match: Name
            if (entry == refName)
                partTruth[i] = true;

            if (refName.Contains(entry))
                partTruth[i] = true;

            var refParts = refName.Split("_");
            foreach (var refPart in refParts)
            {
                if (entry == refPart)
                    partTruth[i] = true;

                if (refPart.Contains(entry))
                    partTruth[i] = true;
            }

            // Match: Reference Segments
            string[] refSegments = refName.Split(" ");
            foreach (string refStr in refSegments)
            {
                string curString = refStr;

                // Remove common brackets so the match ignores them
                if (curString.Contains('('))
                    curString = curString.Replace("(", "");

                if (curString.Contains(')'))
                    curString = curString.Replace(")", "");

                if (curString.Contains('{'))
                    curString = curString.Replace("{", "");

                if (curString.Contains('}'))
                    curString = curString.Replace("}", "");

                if (curString.Contains('('))
                    curString = curString.Replace("(", "");

                if (curString.Contains('['))
                    curString = curString.Replace("[", "");

                if (curString.Contains(']'))
                    curString = curString.Replace("]", "");

                if (entry == curString.Trim())
                    partTruth[i] = true;
            }

            // Match: Tags
            foreach (string tagStr in rawRefTags)
            {
                if (tagStr.ToLower().Contains(entry))
                    partTruth[i] = true;

                if (entry == tagStr.ToLower())
                    partTruth[i] = true;
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

    public static bool IsSelectionSearchMatch(string rawInput, string checkInput, List<string> tags)
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

            var refParts = cleanCheckInput.Split($"_");
            foreach (var refPart in refParts)
            {
                if (entry == refPart)
                {
                    partTruth[i] = true;
                }
            }

            // Match: Tags
            foreach (string tagStr in tags)
            {
                if (entry == tagStr.ToLower())
                    partTruth[i] = true;
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

    public static bool IsIdSearchMatch(string rawInput, string checkInput)
    {
        bool match = false;

        int rawInputNum = -1;
        int.TryParse(rawInput, out rawInputNum);

        int checkInputNum = -1;
        int.TryParse(checkInput, out checkInputNum);

        string[] inputParts = rawInput.Split("+");
        bool[] partTruth = new bool[inputParts.Length];

        if (rawInput.Equals(""))
        {
            match = true; // If input is empty, show all
            return match;
        }

        for (int i = 0; i < partTruth.Length; i++)
        {
            int entry;
            int.TryParse(inputParts[i], out entry);

            if (entry == checkInputNum)
                partTruth[i] = true;
        }

        // Act as OR
        if(partTruth.Contains(true))
        {
            match = true;
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

            // Match: Map ID
            if (entry == MapID)
                match = true;

            // Contains: Map ID
            if (MapID.Contains(entry))
                match = true;

            // Match: Map ID parts
            var MapIDParts = MapID.Split("_");
            foreach (var refPart in MapIDParts)
            {
                var part = refPart.Replace("m", "");

                if (entry == refPart)
                    match = true;

                if (refPart.Contains(entry))
                    match = true;
            }

            // Match: Alias Name
            if (entry == MapName)
                match = true;

            // Contains: Map ID
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
}
