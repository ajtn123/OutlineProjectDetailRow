var enumerator = new DuplicateChecker.Enumerator();

int gi = 0, fi = 0;
foreach (var group in enumerator.Enumerate(args is [] ? ["."] : args))
{
    gi++;
    Write($"[{gi}]", ConsoleColor.Green);
    Write($"[{Convert.ToHexString(group.Key)}]", ConsoleColor.White);
    Console.WriteLine();

    foreach (var file in group)
    {
        fi++;
        Console.WriteLine(Path.GetRelativePath(".", file.FullName));
    }

    Console.WriteLine();
}

Console.WriteLine($"Found {fi} files in {gi} groups.");

static void Write(string message, ConsoleColor color)
{
    Console.ForegroundColor = color;
    Console.Write(message);
    Console.ResetColor();
}
