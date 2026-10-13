
string ProcessObjective(string objective)
{
    var processed = objective
        .Replace("\\q", "\\\"")
        .Replace("${CURRENT}", "%d")
        .Replace("${MAX}", "%d")
        .Replace("${NUMBER}", "%d")
        .Replace("${TOTAL}", "%d");
    return processed;
}

string ProcessProgress(string progress)
{
    var processed = progress
        .Replace("\\q", "\\\"")
        .Replace("-", "%-")
        .Replace("(${NUMBER}/${TOTAL})", "%((%d+)/(%d+)%)")
        .Replace("(${CURRENT}/${MAX})", "%((%d+)/(%d+)%)")
    ;
    return processed;
}

string ProcessTarget(string target)
{
    var processed = target
        .Replace("\\q", "\\\"");
    return processed;
}

string ProcessDisplay(string display)
{
    var processed = display
        .Replace("\\q", "\\\"")
        .Replace(" (${CURRENT}/${MAX})", "")
        .Replace(" (${NUMBER}/${TOTAL})", "")
        ;
    return processed;
}

var path = @"C:\dev\_\lotro-data\lore";
if (args.Length > 0)
{
    path = args[1];
}

var languages = new List<string> { "en", "fr", "de" };
var labels = new LabelLoaderLib.Labels(path, languages, LabelLoaderLib.LabelTypes.Everything);

// Set up output:
var objectives = new List<string>();
objectives.Add("");
objectives.Add("_OBJECTIVES = {");

var progress = new List<string>();
progress.Add("");
progress.Add("_PROGRESS = {");

var targets = new List<string>();
targets.Add("");
targets.Add("_TARGETS = {");
var targetsUnique = new Dictionary<string, string>();

var npcs = new List<string>();
npcs.Add("");
npcs.Add("_NPCS ={");

var displays = new List<string>();
displays.Add("");
displays.Add("_DISPLAY ={");

// Find each source file:
var quickGuideSourceDir = @"C:\Users\bkckx\Documents\The Lord of the Rings Online\Plugins\CubePlugins\FestivalBuddyII\_sourceFiles\Quick Guides";
foreach (var filePath in Directory.EnumerateFiles(quickGuideSourceDir))
{
    var fileInfo = new FileInfo(filePath);

    if (fileInfo.Name.StartsWith("Objectives_"))
    {
        // Get festival:
        var festival = fileInfo.Name.Replace("Objectives_", "").Replace(".txt", "");
        Console.WriteLine($"Processing objectives for {festival}...");
        objectives.Add($"    [{festival}] = {{");

        var entries = File.ReadAllLines(filePath);
        foreach (var entry in entries)
        {
            var english = ProcessObjective(labels.GetLabel("en", entry));
            var french = ProcessObjective(labels.GetLabel("fr", entry));
            var german = ProcessObjective(labels.GetLabel("de", entry));
            var translations = $"[\"ENGLISH\"] = \"{english}\"; [\"FRENCH\"] = \"{french}\"; [\"GERMAN\"] = \"{german}\"; ";
            objectives.Add($"        [\"{entry}\"] = {{ {translations} }};");
        }
        objectives.Add("    };");
    }
    else if (fileInfo.Name.StartsWith("Progress_"))
    {
        // Get festival:
        var festival = fileInfo.Name.Replace("Progress_", "").Replace(".txt", "");
        Console.WriteLine($"Processing progress for {festival}...");
        progress.Add($"    [{festival}] = {{");

        var entries = File.ReadAllLines(filePath);
        foreach (var entry in entries)
        {
            var english = ProcessProgress(labels.GetLabel("en", entry));
            var french = ProcessProgress(labels.GetLabel("fr", entry));
            var german = ProcessProgress(labels.GetLabel("de", entry));
            var translations = $"[\"ENGLISH\"] = \"{english}\"; [\"FRENCH\"] = \"{french}\"; [\"GERMAN\"] = \"{german}\"; ";
            progress.Add($"        [\"{entry}\"] = {{ {translations} }};");
        }
        progress.Add("    };");
    }
    else if (fileInfo.Name.StartsWith("Targets_"))
    {
        // Get festival:
        var festival = fileInfo.Name.Replace("Targets_", "").Replace(".txt", "");
        Console.WriteLine($"Processing targets for {festival}...");

        var entries = File.ReadAllLines(filePath);
        foreach (var entry in entries)
        {
            var english = ProcessTarget(labels.GetLabel("en", entry));
            var french = ProcessTarget(labels.GetLabel("fr", entry));
            var german = ProcessTarget(labels.GetLabel("de", entry));
            var translations = $"[\"ENGLISH\"] = \"{english}\"; [\"FRENCH\"] = \"{french}\"; [\"GERMAN\"] = \"{german}\"; ";
            targetsUnique[entry] = translations;
            // targets.Add($"        [\"{entry}\"] = {{ {translations} }};");
        }

        var targetLines = new List<string>();
        foreach (var kvp in targetsUnique)
        {
            targetLines.Add($"        [\"{kvp.Key}\"] = {{ {kvp.Value} }};");
        }
        targetLines.Sort();

        targets.Add($"    [{festival}] = {{");
        targets.AddRange(targetLines);
        targets.Add("    };");
    }
    else if (fileInfo.Name.StartsWith("NPCs_"))
    {
        // Get festival:
        var festival = fileInfo.Name.Replace("NPCs_", "").Replace(".txt", "");
        Console.WriteLine($"Processing NPCs for {festival}...");
        npcs.Add($"    [{festival}] = {{");

        var entries = File.ReadAllLines(filePath);
        foreach (var entry in entries)
        {
            var parts = entry.Split("=");
            if (parts.Length == 2)
            {
                var key = parts[0];
                var id = parts[1];

                var english = ProcessTarget(labels.GetLabel("en", id));
                var french = ProcessTarget(labels.GetLabel("fr", id));
                var german = ProcessTarget(labels.GetLabel("de", id));
                var translations = $"[\"ENGLISH\"] = \"{english}\"; [\"FRENCH\"] = \"{french}\"; [\"GERMAN\"] = \"{german}\"; ";
                npcs.Add($"        [\"{key}\"] = {{ {translations} }};");
            }
        }

        npcs.Add("    };");
    }
    else if (fileInfo.Name.StartsWith("Display_"))
    {
        // Get festival:
        var festival = fileInfo.Name.Replace("Display_", "").Replace(".txt", "");
        Console.WriteLine($"Processing display for {festival}...");
        displays.Add($"    [{festival}] = {{");

        var entries = File.ReadAllLines(filePath);
        foreach (var entry in entries)
        {
            var english = ProcessDisplay(labels.GetLabel("en", entry));
            var french = ProcessDisplay(labels.GetLabel("fr", entry));
            var german = ProcessDisplay(labels.GetLabel("de", entry));
            var translations = $"[\"ENGLISH\"] = \"{english}\"; [\"FRENCH\"] = \"{french}\"; [\"GERMAN\"] = \"{german}\"; ";
            displays.Add($"        [\"{entry}\"] = {{ {translations} }};");
        }
        displays.Add("    };");
    }
}

objectives.Add("};");
progress.Add("};");
targets.Add("};");
npcs.Add("};");
displays.Add("};");

var time = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

var allLines = new List<string>
{
    $"-- auto-generated by Cube's 'Festival Buddy Quick Guide Objectives' tool at {time} UTC"
};
allLines.AddRange(objectives);
allLines.AddRange(progress);
allLines.AddRange(targets);
allLines.AddRange(npcs);
allLines.AddRange(displays);

File.WriteAllLines(@"C:\Users\bkckx\Documents\The Lord of the Rings Online\Plugins\CubePlugins\FestivalBuddyII\QuickGuides\QuickGuideStrings.lua", allLines);
