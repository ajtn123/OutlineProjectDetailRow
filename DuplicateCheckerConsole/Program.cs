var checker = new DuplicateChecker.Checker();

int sets = 0, files = 0;
foreach (var set in checker.Enumerate(args))
{
    sets++; files += set.Files.Length;

    Console.WriteLine();
    Console.WriteLine(Convert.ToHexString(set.Hash));
    foreach (var file in set.Files)
        Console.WriteLine($"  {Path.GetRelativePath(".", file.FullName)}");
}

Console.WriteLine();
Console.WriteLine($"Found {files} files in {sets} groups.");
