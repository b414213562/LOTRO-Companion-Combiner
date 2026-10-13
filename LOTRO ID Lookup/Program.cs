using System.Linq;

var path = @"C:\dev\_\lotro-data\lore";
if (args.Length > 0)
{
    path = args[1];
}

var labels = new LabelLoaderLib.Labels(path, new List<string> { "en" }, LabelLoaderLib.LabelTypes.OnlyItems);

while (true)
{
    Console.WriteLine();
    Console.Write("Enter labels (blank line to process): ");

    const string firstSeparator = "#";
    const string separator = "~";

    bool isNotNullOrEmpty = false;
    var inputs = new List<string>();
    do
    {
        var label = Console.ReadLine();
        isNotNullOrEmpty = !string.IsNullOrEmpty(label);
        if (isNotNullOrEmpty)
        {
            inputs.Add(label);
        }
    }
    while (isNotNullOrEmpty);

    foreach (var input in inputs)
    {
        var label = input.Trim();
        label = label.Trim('\"');
        label = label.Replace("&", "&amp;");

        var fullResults = labels.GetLabels(label);

        // for this purpose, discount any that start with "key:":
        var results = (from result in fullResults
                       where !result.Item1.StartsWith("key:")
                       select result.Item1).ToList();

        if (results.Count == 0)
        {
            Console.WriteLine($"{label}{firstSeparator}No results found");
        }
        else
        {
            var moreThanOne = "";
            if (results.Count > 1)
            {
                moreThanOne = $" ({results.Count} found)";
            }

            // Console.WriteLine($"{label}{firstSeparator}{results.Last().Item1}{separator}{moreThanOne}");
            // Just output the id so it can be copied into Excel:
            Console.WriteLine($"{results[0]}{moreThanOne}");
        }
    }
}
return 0;
