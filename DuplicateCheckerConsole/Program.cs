var checker = new DuplicateChecker.Checker();

int groups = 0, files = 0;
foreach (var group in checker.Enumerate(args))
{
    groups++;
    Write($"[{groups}]", ConsoleColor.Green);
    Write($"[{Convert.ToHexString(group.Key)}]", ConsoleColor.DarkGray);
    Console.WriteLine();

    foreach (var file in group)
    {
        files++;
        Console.WriteLine(Path.GetRelativePath(".", file.FullName));
    }

    Console.WriteLine();
}

Console.WriteLine($"Found {files} files in {groups} groups.");

static void Write(string message, ConsoleColor color)
{
    Console.ForegroundColor = color;
    Console.Write(message);
    Console.ResetColor();
}
