using HKLib.hk2018.hk;
using HKLib.hk2018.hkAsyncThreadPool;
using SoulsFormats;
using StudioCore.Editors.TimeActEditor.Bank;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Formats.Tar;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StudioCore.Editors.TimeActEditor.Bank.TimeActBank;

namespace StudioCore.Editors.TimeActEditor;

public static class TimeActFilters
{
    public static string _fileContainerFilterString = "";
    public static string _timeActFilterString = "";
    public static string _timeActAnimationFilterString = "";
    public static string _timeActEventFilterString = "";
    public static string _timeActEventPropertyFilterString = "";

    public static bool FileContainerFilter(TimeActContainerWrapper info)
    {
        bool isValid = true;
        var input = _fileContainerFilterString.ToLower();

        if (input != "")
        {
            string[] inputParts = input.Split("+");
            bool[] partTruth = new bool[inputParts.Length];

            var id = info.Name.ToLower();

            var referenceDict = Smithbox.AliasCacheHandler.AliasCache.Characters;
            var alias = "";
            var tags = new List<string>();

            if (referenceDict.ContainsKey(info.Name))
            {
                alias = referenceDict[info.Name].name.ToLower();
                tags = referenceDict[info.Name].tags;
            }

            for (int i = 0; i < partTruth.Length; i++)
            {
                string entry = inputParts[i];

                if (entry == id)
                    partTruth[i] = true;

                if (id.Contains(entry))
                    partTruth[i] = true;

                if (entry == alias)
                    partTruth[i] = true;

                if (alias.Contains(entry))
                    partTruth[i] = true;

                foreach (string tagStr in tags)
                {
                    if (entry == tagStr.ToLower())
                        partTruth[i] = true;

                    if (tagStr.ToLower().Contains(entry))
                        partTruth[i] = true;
                }
            }

            foreach (bool entry in partTruth)
            {
                if (!entry)
                    isValid = false;
            }
        }

        return isValid;
    }

    public static bool TimeActFilter(TimeActContainerWrapper info, TAE taeEntry)
    {
        bool isValid = true;
        var input = _timeActFilterString.ToLower();

        if (input != "")
        {
            string[] inputParts = input.Split("+");
            bool[] partTruth = new bool[inputParts.Length];

            var id = taeEntry.ID.ToString();
            var alias = "";
            var tags = new List<string>();

            if (Smithbox.BankHandler.TimeActAliases.Aliases != null)
            {
                var idStr = id.ToString();
                var idSection = idStr.Substring(idStr.Length - 3);

                var searchStr = $"{info.Name}_{idSection}";
                var refAlias = Smithbox.BankHandler.TimeActAliases.Aliases.list.Where(e => e.id == searchStr)
                    .FirstOrDefault();

                if (refAlias != null)
                {
                    alias = refAlias.name.ToLower();
                    tags = refAlias.tags;
                }
            }

            for (int i = 0; i < partTruth.Length; i++)
            {
                string entry = inputParts[i];

                if (entry == id)
                    partTruth[i] = true;

                if (id.Contains(entry))
                    partTruth[i] = true;

                if (entry == alias)
                    partTruth[i] = true;

                if (alias.Contains(entry))
                    partTruth[i] = true;

                foreach (string tagStr in tags)
                {
                    if (entry == tagStr.ToLower())
                        partTruth[i] = true;

                    if (tagStr.ToLower().Contains(entry))
                        partTruth[i] = true;
                }
            }

            foreach (bool entry in partTruth)
            {
                if (!entry)
                    isValid = false;
            }
        }

        return isValid;
    }

    public static bool TimeActAnimationFilter(TimeActContainerWrapper info, TAE.Animation animEntry)
    {
        bool isValid = true;
        var input = _timeActAnimationFilterString.ToLower();

        if (input != "")
        {
            string[] inputParts = input.Split("+");
            bool[] partTruth = new bool[inputParts.Length];

            var id = animEntry.ID.ToString();
            var tags = new List<string>();

            var refAlias = Smithbox.BankHandler.HavokGeneratorAliases.HavokAliases.List.Where(e => e.ID == animEntry.ID.ToString()).FirstOrDefault();

            for (int i = 0; i < partTruth.Length; i++)
            {
                string entry = inputParts[i];

                if (entry == id)
                    partTruth[i] = true;

                if (id.Contains(entry))
                    partTruth[i] = true;

                if (refAlias != null)
                {
                    if (entry == refAlias.ID)
                        partTruth[i] = true;

                    if (refAlias.ID.Contains(entry))
                        partTruth[i] = true;

                    foreach (string generator in refAlias.Generators)
                    {
                        if (entry == generator.ToLower())
                            partTruth[i] = true;

                        if (generator.ToLower().Contains(entry))
                            partTruth[i] = true;
                    }
                }
            }

            foreach (bool entry in partTruth)
            {
                if (!entry)
                    isValid = false;
            }
        }

        return isValid;
    }

    public static bool TimeActEventFilter(TimeActContainerWrapper info, TAE.Event evtEntry)
    {
        bool isValid = true;
        var input = _timeActEventFilterString.ToLower();

        if (input != "")
        {
            string[] inputParts = input.Split("+");
            bool[] partTruth = new bool[inputParts.Length];

            var id = evtEntry.TypeName.ToString().ToLower();
            var alias = "";
            var tags = new List<string>();

            for (int i = 0; i < partTruth.Length; i++)
            {
                string entry = inputParts[i];

                if (entry == id)
                    partTruth[i] = true;

                if (id.Contains(entry))
                    partTruth[i] = true;

                if (entry == alias)
                    partTruth[i] = true;

                if (alias.Contains(entry))
                    partTruth[i] = true;

                foreach (string tagStr in tags)
                {
                    if (entry == tagStr.ToLower())
                        partTruth[i] = true;

                    if (tagStr.ToLower().Contains(entry))
                        partTruth[i] = true;
                }
            }

            foreach (bool entry in partTruth)
            {
                if (!entry)
                    isValid = false;
            }
        }

        return isValid;
    }

    public static bool TimeActEventPropertyFilter(TimeActContainerWrapper info, string propertyName)
    {
        bool isValid = true;
        var input = _timeActEventPropertyFilterString.ToLower();

        if (input != "")
        {
            string[] inputParts = input.Split("+");
            bool[] partTruth = new bool[inputParts.Length];

            var id = propertyName;
            var alias = "";
            var tags = new List<string>();

            for (int i = 0; i < partTruth.Length; i++)
            {
                string entry = inputParts[i];

                if (entry == id)
                    partTruth[i] = true;

                if (id.Contains(entry))
                    partTruth[i] = true;

                if (entry == alias)
                    partTruth[i] = true;

                if (alias.Contains(entry))
                    partTruth[i] = true;

                foreach (string tagStr in tags)
                {
                    if (entry == tagStr.ToLower())
                        partTruth[i] = true;

                    if (tagStr.ToLower().Contains(entry))
                        partTruth[i] = true;
                }
            }

            foreach (bool entry in partTruth)
            {
                if (!entry)
                    isValid = false;
            }
        }

        return isValid;
    }
}
