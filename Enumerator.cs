namespace OutlineProjectDetailRow;

public class Enumerator
{
    public string SearchPattern { get; set; } = "*";
    public EnumerationOptions EnumerationOptions { get; set; } = new()
    {
        RecurseSubdirectories = true,
        IgnoreInaccessible = true,
    };

    public IEnumerable<IGrouping<byte[], FileInfo>> Enumerate(string[] directories) => directories
        .SelectMany(directory => Directory.EnumerateFiles(directory, SearchPattern, EnumerationOptions))
        .Distinct()
        .Select(file => new FileInfo(file))
        .GroupBy(file => file.Length)
        .Where(group => group.Skip(1).Any())
        .SelectMany(group => group
            .GroupBy(Hash, new HashEqualityComparer())
            .Where(group => group.Skip(1).Any()));

    private static byte[] Hash(FileInfo file)
    {
        using var fs = file.OpenRead();
        return System.Security.Cryptography.SHA256.HashData(fs);
    }
}

public class HashEqualityComparer : IEqualityComparer<byte[]>
{
    public bool Equals(byte[]? x, byte[]? y)
    {
        if (x is null || y is null) return false;
        return x.SequenceEqual(y);
    }

    public int GetHashCode(byte[] obj) => BitConverter.ToInt32(obj);
}
