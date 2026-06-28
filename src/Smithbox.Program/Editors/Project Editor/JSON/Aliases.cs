using StudioCore.Editors.MetadataEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Application;

public class AliasStore : Dictionary<ProjectAliasType, List<AliasEntry>>;

public class AliasEntry : IComparable<AliasEntry>
{
    public string ID { get; set; }
    public string Name { get; set; }
    public List<string> Tags { get; set; }

    public int CompareTo(AliasEntry other)
    {
        if (other == null) 
            return 1;

        return NaturalCompare(ID, other.ID);
    }
    private int NaturalCompare(string a, string b)
    {
        int i = 0, j = 0;
        while (i < a.Length && j < b.Length)
        {
            if (char.IsDigit(a[i]) && char.IsDigit(b[j]))
            {
                int startI = i, startJ = j;
                while (i < a.Length && char.IsDigit(a[i])) i++;
                while (j < b.Length && char.IsDigit(b[j])) j++;

                string numA = a.Substring(startI, i - startI);
                string numB = b.Substring(startJ, j - startJ);

                int numCompare = int.Parse(numA).CompareTo(int.Parse(numB));
                if (numCompare != 0) return numCompare;
            }
            else
            {
                int charCompare = char.ToLowerInvariant(a[i]).CompareTo(char.ToLowerInvariant(b[j]));
                if (charCompare != 0) return charCompare;
                i++; j++;
            }
        }
        return (a.Length - i).CompareTo(b.Length - j);
    }
}