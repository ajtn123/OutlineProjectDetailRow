using DuplicateChecker;

if (args.Length <= 0)
{
    Console.WriteLine("Pass at least one directory as argument.");
    return;
}

var Checker = new Checker(args);

Console.Write("Started. ");

Checker.Check();

Console.WriteLine($"Finished. Found {Checker.Duplicates.Count} Matches.");
Console.WriteLine();

foreach (var item in Checker.Duplicates)
{
    Console.WriteLine(BitConverter.ToString(item.Hash));

    foreach (var i in item.Files)
        Console.WriteLine($" > {i.FullName}");

    Console.WriteLine();
}

Console.Write("Save result? (Y/n) > ");
if (Console.ReadLine() is not "n" and not "N")
    Checker.SaveResult();
