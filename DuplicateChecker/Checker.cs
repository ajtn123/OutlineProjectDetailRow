using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;

namespace DuplicateChecker;

public class Checker
{
    public Checker(string[] paths, bool recursive = true, string searchPattern = "")
    {
        SearchPattern = searchPattern;
        Recursive = recursive;
        Paths = [.. paths.Select(s => s.Trim('"', ' ', '\'')).Where(s => !string.IsNullOrWhiteSpace(s))];
        Dirs = [.. Paths.Select(p => new DirectoryInfo(p))];
    }

    public string[] Paths { get; }
    public DirectoryInfo[] Dirs { get; }
    public bool Recursive { get; }
    public string SearchPattern { get; }

    public List<DuplicateSet> Duplicates { get; private set; } = [];

    public void Check()
    {
        var files = Dirs.SelectMany(d => d.EnumerateFiles(SearchPattern, Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
            .Where(f => (f.Attributes & FileAttributes.System) == 0)
            .Distinct(new FileInfoEqualityComparer());

        var lengthGroups = files.GroupBy(f => f.Length);

        List<DuplicateSet> result = [];

        foreach (var lengthGroup in lengthGroups)
        {
            if (lengthGroup.Count() == 1) continue;

            var hashGroups = lengthGroup.GroupBy(ComputeHash, new HashEqualityComparer());

            foreach (var hashGroup in hashGroups)
            {
                if (hashGroup.Count() == 1) continue;

                result.Add(new(hashGroup.Key, [..hashGroup]));
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

public class FileInfoEqualityComparer : IEqualityComparer<FileInfo>
{
    public bool Equals(FileInfo? x, FileInfo? y)
    {
        if (x == null || y == null) return false;
        return string.Equals(x.FullName, y.FullName, StringComparison.OrdinalIgnoreCase);
    }

    public int GetHashCode([DisallowNull] FileInfo obj)
        => StringComparer.OrdinalIgnoreCase.GetHashCode(obj.FullName);
}
