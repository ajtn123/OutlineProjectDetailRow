if (args.Length <= 0)
{
    Console.WriteLine("Pass at least one directory as argument.");
    return;
}

var Checker = new DuplicateChecker.Checker(args);

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
{
    var dupInfo = Checker.Duplicates.Select(d => new DuplicateSetInfo(BitConverter.ToString(d.Hash), [.. d.Files.Select(f => f.FullName)]));
    string json = System.Text.Json.JsonSerializer.Serialize(dupInfo, new System.Text.Json.JsonSerializerOptions()
    {
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All),
        WriteIndented = true
    });
    File.WriteAllText($"Result-{DateTime.Now.Millisecond}.json", json);
}

record DuplicateSetInfo(string Hash, string[] Files);
