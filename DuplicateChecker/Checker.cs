using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

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

    public List<DuplicateSet> Check()
    {
        var files = Dirs.SelectMany(d => d.GetFiles(SearchPattern, Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
            .Where(f => (f.Attributes & FileAttributes.System) == 0)
            .Distinct(new FileInfoEqualityComparer())
            .Select(f => new HaFile(f));

        var groupByLength = files.GroupBy(f => f.Length);

        List<DuplicateSet> result = [];

        foreach (var group in groupByLength)
        {
            if (group.Count() <= 1) continue;
            foreach (var f in group)
                f.ComputeHash();
            var groupByHash = group.GroupBy(f => f.Hash, new HashEqualityComparer());

            foreach (var f in groupByHash)
            {
                if (f.Count() <= 1) continue;

                result.Add(new DuplicateSet(f.Key ?? [], [.. f.Select(f => f.FileInfo)]));
            }
        }

        return Duplicates = result;
    }

    private static readonly JsonSerializerOptions serializerOptions = new()
    {
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
        WriteIndented = true
    };
    public void SaveResult()
    {
        var dupInfo = Duplicates.Select(f => new DuplicateSetInfo(f));
        string json = JsonSerializer.Serialize(dupInfo, serializerOptions);
        File.WriteAllText($"Result-{DateTime.Now.Millisecond}.json", json);
    }
}

public class DuplicateSet(byte[] hash, FileInfo[] files)
{
    public byte[] Hash { get; } = hash;
    public FileInfo[] Files { get; } = files;
}
public class DuplicateSetInfo(DuplicateSet set)
{
    public string Hash { get; } = BitConverter.ToString(set.Hash);
    public string[] Files { get; } = [.. set.Files.Select(f => f.FullName)];
}

public class HaFile(FileInfo file)
{
    private static readonly SHA256 S256 = SHA256.Create();
    public FileInfo FileInfo { get; } = file;
    public long Length { get; } = file.Length;
    public byte[]? Hash { get; private set; }


    public byte[] ComputeHash()
    {
        using var fs = FileInfo.OpenRead();
        return Hash = S256.ComputeHash(fs);
    }
}

public class HashEqualityComparer : IEqualityComparer<byte[]?>
{
    public bool Equals(byte[]? x, byte[]? y)
    {
        if (x == null || y == null) return false;
        return x.SequenceEqual(y);
    }

    public int GetHashCode([DisallowNull] byte[]? obj)
    {
        if (obj == null) return 0;
        var hash = new HashCode();
        foreach (var b in obj)
            hash.Add(b);
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
