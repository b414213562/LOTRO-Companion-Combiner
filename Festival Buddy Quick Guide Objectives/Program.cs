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
        .Replace("(${CURRENT}/${MAX})", "%((%d+)/(%d+)%)");
    return processed;
}

string ProcessTarget(string target)
{
    return target.Replace("\\q", "\\\"");
}

string ProcessDisplay(string display)
{
    return display
        .Replace("\\q", "\\\"")
        .Replace(" (${CURRENT}/${MAX})", "")
        .Replace(" (${NUMBER}/${TOTAL})", "");
}

string GetSourceKey(string entry)
{
    var commentStart = entry.IndexOf(" -- ", StringComparison.Ordinal);
    return (commentStart >= 0 ? entry[..commentStart] : entry).Trim();
}

string GetLocalizedLabel(LabelLoaderLib.Labels labels, string language, string key)
{
    if (labels.HasKey(language, key))
    {
        return labels.GetLabel(language, key);
    }
    return labels.GetLabel("en", key);
}

var languages = new List<string> { "en", "fr", "de" };

Dictionary<string, List<string>> CreateSection(string declaration)
{
    return languages.ToDictionary(
        language => language,
        _ => new List<string> { "", declaration });
}

void CloseSection(Dictionary<string, List<string>> section)
{
    foreach (var lines in section.Values)
    {
        lines.Add("};");
    }
}

var path = @"C:\dev\_\lotro-data\lore";
if (args.Length > 0)
{
    path = args[1];
}

var labels = new LabelLoaderLib.Labels(path, languages, LabelLoaderLib.LabelTypes.Everything);

var objectives = CreateSection("_OBJECTIVES = {");
var progress = CreateSection("_PROGRESS = {");
var targets = CreateSection("_TARGETS = {");
var targetsUnique = languages.ToDictionary(
    language => language,
    _ => new Dictionary<string, string>());
var npcs = CreateSection("_NPCS ={");
var displays = CreateSection("_DISPLAY ={");

var quickGuideSourceDir = @"C:\Users\bkckx\Documents\The Lord of the Rings Online\Plugins\CubePlugins\FestivalBuddyII\_sourceFiles\Quick Guides";
foreach (var filePath in Directory.EnumerateFiles(quickGuideSourceDir))
{
    var fileInfo = new FileInfo(filePath);

    if (fileInfo.Name.StartsWith("Objectives_"))
    {
        var festival = fileInfo.Name.Replace("Objectives_", "").Replace(".txt", "");
        Console.WriteLine($"Processing objectives for {festival}...");
        foreach (var lines in objectives.Values)
        {
            lines.Add($"    [{festival}] = {{");
        }

        foreach (var entry in File.ReadAllLines(filePath))
        {
            var key = GetSourceKey(entry);
            if (key == "")
            {
                continue;
            }

            foreach (var language in languages)
            {
                var value = ProcessObjective(GetLocalizedLabel(labels, language, key));
                objectives[language].Add($"        [\"{key}\"] = \"{value}\";");
            }
        }

        foreach (var lines in objectives.Values)
        {
            lines.Add("    };");
        }
    }
    else if (fileInfo.Name.StartsWith("Progress_"))
    {
        var festival = fileInfo.Name.Replace("Progress_", "").Replace(".txt", "");
        Console.WriteLine($"Processing progress for {festival}...");
        foreach (var lines in progress.Values)
        {
            lines.Add($"    [{festival}] = {{");
        }

        foreach (var entry in File.ReadAllLines(filePath))
        {
            var key = GetSourceKey(entry);
            if (key == "")
            {
                continue;
            }

            foreach (var language in languages)
            {
                var value = ProcessProgress(GetLocalizedLabel(labels, language, key));
                progress[language].Add($"        [\"{key}\"] = \"{value}\";");
            }
        }

        foreach (var lines in progress.Values)
        {
            lines.Add("    };");
        }
    }
    else if (fileInfo.Name.StartsWith("Targets_"))
    {
        var festival = fileInfo.Name.Replace("Targets_", "").Replace(".txt", "");
        Console.WriteLine($"Processing targets for {festival}...");

        foreach (var entry in File.ReadAllLines(filePath))
        {
            var key = GetSourceKey(entry);
            if (key == "")
            {
                continue;
            }

            foreach (var language in languages)
            {
                targetsUnique[language][key] =
                    ProcessTarget(GetLocalizedLabel(labels, language, key));
            }
        }

        foreach (var language in languages)
        {
            targets[language].Add($"    [{festival}] = {{");
            foreach (var target in targetsUnique[language].OrderBy(target => target.Key, StringComparer.Ordinal))
            {
                targets[language].Add($"        [\"{target.Key}\"] = \"{target.Value}\";");
            }
            targets[language].Add("    };");
        }
    }
    else if (fileInfo.Name.StartsWith("NPCs_"))
    {
        var festival = fileInfo.Name.Replace("NPCs_", "").Replace(".txt", "");
        Console.WriteLine($"Processing NPCs for {festival}...");
        foreach (var lines in npcs.Values)
        {
            lines.Add($"    [{festival}] = {{");
        }

        foreach (var entry in File.ReadAllLines(filePath))
        {
            var key = GetSourceKey(entry);
            var parts = key.Split('=', 2);
            if (parts.Length == 2)
            {
                var npcKey = parts[0].Trim();
                var id = parts[1].Trim();

                foreach (var language in languages)
                {
                    var value = ProcessTarget(GetLocalizedLabel(labels, language, id));
                    npcs[language].Add($"        [\"{npcKey}\"] = \"{value}\";");
                }
            }
        }

        foreach (var lines in npcs.Values)
        {
            lines.Add("    };");
        }
    }
    else if (fileInfo.Name.StartsWith("Display_"))
    {
        var festival = fileInfo.Name.Replace("Display_", "").Replace(".txt", "");
        Console.WriteLine($"Processing display for {festival}...");
        foreach (var lines in displays.Values)
        {
            lines.Add($"    [{festival}] = {{");
        }

        foreach (var entry in File.ReadAllLines(filePath))
        {
            var key = GetSourceKey(entry);
            if (key == "")
            {
                continue;
            }

            foreach (var language in languages)
            {
                var value = ProcessDisplay(GetLocalizedLabel(labels, language, key));
                displays[language].Add($"        [\"{key}\"] = \"{value}\";");
            }
        }

        foreach (var lines in displays.Values)
        {
            lines.Add("    };");
        }
    }
}

CloseSection(objectives);
CloseSection(progress);
CloseSection(targets);
CloseSection(npcs);
CloseSection(displays);

var time = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
var outputDirectory = @"C:\Users\bkckx\Documents\The Lord of the Rings Online\Plugins\CubePlugins\FestivalBuddyII\QuickGuides";
var languageCodes = new Dictionary<string, string>
{
    ["en"] = "EN",
    ["fr"] = "FR",
    ["de"] = "DE"
};

foreach (var language in languages)
{
    var allLines = new List<string>
    {
        $"-- auto-generated by Cube's 'Festival Buddy Quick Guide Objectives' tool at {time} UTC"
    };
    allLines.AddRange(objectives[language]);
    allLines.AddRange(progress[language]);
    allLines.AddRange(targets[language]);
    allLines.AddRange(npcs[language]);
    allLines.AddRange(displays[language]);

    var languageCode = languageCodes[language];
    File.WriteAllLines(
        Path.Combine(outputDirectory, $"QuickGuideStrings{languageCode}.lua"),
        allLines);
}

File.WriteAllLines(
    Path.Combine(outputDirectory, "QuickGuideStrings.lua"),
    new[]
    {
        "-- Import only the current client's quick guide strings.",
        "if (CLIENTLANG == \"FRENCH\") then",
        "    import \"CubePlugins.FestivalBuddyII.QuickGuides.QuickGuideStringsFR\";",
        "elseif (CLIENTLANG == \"GERMAN\") then",
        "    import \"CubePlugins.FestivalBuddyII.QuickGuides.QuickGuideStringsDE\";",
        "else",
        "    import \"CubePlugins.FestivalBuddyII.QuickGuides.QuickGuideStringsEN\";",
        "end"
    });
