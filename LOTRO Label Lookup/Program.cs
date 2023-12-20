
var path = @"C:\dev\_\lotro-data";
if (args.Length > 0)
{
    path = args[1];
}

var labels = new LabelLoaderLib.Labels(path);

while (true)
{
    Console.WriteLine();
    Console.Write("Enter label: ");
    var label = Console.ReadLine();
    if (label == null) { return 0; }

    label = label.Trim('\"');

    var results = labels.GetLabels(label);
    if (results.Count == 0)
    {
        Console.WriteLine("No results found");
    }
    else
    {
        foreach (var result in results)
        {
            Console.WriteLine($"Key: {result.Item1}");
            foreach (var kvp in result.Item2)
            {
                var quotedValue = $"\"{kvp.Value}\"";
                Console.WriteLine($"  {kvp.Key}: {quotedValue}");

                //if (kvp.Key == "fr")
                //{
                //    System.Windows.Clipboard.SetText(quotedValue);
                //}
            }
        }
    }

}



return 0;