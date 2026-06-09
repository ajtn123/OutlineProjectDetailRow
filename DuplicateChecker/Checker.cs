using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;

namespace DuplicateChecker;

public class Checker(string[] paths)
{
    public string[] Paths { get; } = [.. paths.Select(s => s.Trim('"', ' ', '\'')).Where(Directory.Exists)];
    public bool Recursive { get; set; } = true;
    public string SearchPattern { get; set; } = "*";

    public List<DuplicateSet> Duplicates { get; private set; } = [];

    public void Check()
    {
        var files = Paths.SelectMany(path => Directory.EnumerateFiles(path, SearchPattern, new EnumerationOptions
        {
            RecurseSubdirectories = Recursive,
            IgnoreInaccessible = true,
        })).Distinct().Select(fp => new FileInfo(fp));

        var lengthGroups = files.GroupBy(file => file.Length).Where(group => group.Skip(1).Any());

        Parallel.ForEach(lengthGroups, lengthGroup =>
        {
            var hashGroups = lengthGroup.GroupBy(HashFile, new HashEqualityComparer()).Where(group => group.Skip(1).Any());

            Duplicates.AddRange(hashGroups.Select(hashGroup => new DuplicateSet(hashGroup.Key, [.. hashGroup])));
        });
    }

    private static byte[] HashFile(FileInfo file)
    {
        using var fs = file.OpenRead();
        return SHA256.HashData(fs);
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
