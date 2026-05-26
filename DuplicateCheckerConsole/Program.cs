if (args.Length <= 0)
{
    Console.WriteLine("Pass at least one directory as argument.");
    return;
}

var Checker = new DuplicateChecker.Checker(args);

Console.Write("Started. ");

Checker.Check();

Console.WriteLine($"Finished. Found {Checker.Duplicates.Count} Matches.");

foreach (var item in Checker.Duplicates)
{
    Console.WriteLine();
    Console.WriteLine(Convert.ToHexString(item.Hash));

    foreach (var i in item.Files)
        Console.WriteLine($" - {i.FullName}");
}
