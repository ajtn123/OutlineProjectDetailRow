using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;

namespace DuplicateChecker;

public class Checker
{
    public string SearchPattern { get; set; } = "*";
    public EnumerationOptions EnumerationOptions { get; set; } = new()
    {
        RecurseSubdirectories = true,
        IgnoreInaccessible = true,
    };

    public IEnumerable<DuplicateSet> Enumerate(string[] directories) => directories
        .Select(s => s.Trim('"', '\'', ' '))
        .Where(Directory.Exists)
        .SelectMany(path => Directory.EnumerateFiles(path, SearchPattern, EnumerationOptions))
        .Distinct()
        .Select(fp => new FileInfo(fp))
        .GroupBy(file => file.Length)
        .Where(group => group.Skip(1).Any())
        .SelectMany(group => group
            .GroupBy(Hash, new HashEqualityComparer())
            .Where(group => group.Skip(1).Any()))
        .Select(hashGroup => new DuplicateSet(hashGroup.Key, [.. hashGroup]));

    private static byte[] Hash(FileInfo file)
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
