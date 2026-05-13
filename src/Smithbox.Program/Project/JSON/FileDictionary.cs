using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Application;

public class FileDictionary
{
    public HashSet<FileDictionaryEntry> Entries { get; set; } = new();
}

public class FileDictionaryEntry : IComparable
{
    public string Archive { get; set; }
    public string Path { get; set; }
    public string Folder { get; set; }
    public string Filename { get; set; }
    public string Extension { get; set; }

    public int CompareTo(object obj)
    {
        var compare = (FileDictionaryEntry)obj;
        return Filename.CompareTo(compare.Filename);
    }

    public FileDictionaryEntry Clone()
    {
        return (FileDictionaryEntry)this.MemberwiseClone();
    }

    public override bool Equals(object obj)
    {
        if (obj is not FileDictionaryEntry other) return false;
        return string.Equals(Archive, other.Archive, StringComparison.OrdinalIgnoreCase)
            && string.Equals(Path, other.Path, StringComparison.OrdinalIgnoreCase)
            && string.Equals(Folder, other.Folder, StringComparison.OrdinalIgnoreCase)
            && string.Equals(Filename, other.Filename, StringComparison.OrdinalIgnoreCase)
            && string.Equals(Extension, other.Extension, StringComparison.OrdinalIgnoreCase);
    }

    public override int GetHashCode()
    {
        var comparer = StringComparer.OrdinalIgnoreCase;
        return HashCode.Combine(
            comparer.GetHashCode(Archive ?? string.Empty),
            comparer.GetHashCode(Path ?? string.Empty),
            comparer.GetHashCode(Folder ?? string.Empty),
            comparer.GetHashCode(Filename ?? string.Empty),
            comparer.GetHashCode(Extension ?? string.Empty)
        );
    }
}