using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;

namespace DuplicateChecker;

public class Checker
{
    public Checker(string[] paths, bool recursive = true, string pattern = "")
    {
        SearchPattern = pattern;
        Recursive = recursive;
        Paths = [.. paths.Select(s => s.Trim('"', ' ', '\'')).Where(Directory.Exists)];
    }

    public string[] Paths { get; }
    public bool Recursive { get; }
    public string SearchPattern { get; }

    public List<DuplicateSet> Duplicates { get; private set; } = [];

    public void Check()
    {
        var files = Paths.SelectMany(path => Directory.EnumerateFiles(path, SearchPattern, new EnumerationOptions
        {
            RecurseSubdirectories = Recursive,
            IgnoreInaccessible = true,
        })).Distinct().Select(fp => new FileInfo(fp));

        var lengthGroups = files.GroupBy(file => file.Length);

        foreach (var lengthGroup in lengthGroups)
        {
            if (lengthGroup.Count() == 1) continue;

            var hashGroups = lengthGroup.GroupBy(ComputeHash, new HashEqualityComparer());

            foreach (var hashGroup in hashGroups)
            {
                if (hashGroup.Count() == 1) continue;

                Duplicates.Add(new(hashGroup.Key, [..hashGroup]));
            }
        }
    }

    private static readonly SHA256 sha256 = SHA256.Create();
    public byte[] ComputeHash(FileInfo file)
    {
        using var fs = file.OpenRead();
        return sha256.ComputeHash(fs);
    }
}

public record DuplicateSet(byte[] Hash, FileInfo[] Files);

public class HashEqualityComparer : IEqualityComparer<byte[]>
{
    public bool Equals(byte[]? x, byte[]? y)
    {
        if (x == null || y == null) return false;
        return x.SequenceEqual(y);
    }

    public int GetHashCode([DisallowNull] byte[] obj)
    {
        var hash = new HashCode();
        hash.AddBytes(obj);
        return hash.ToHashCode();
    }
}
