using System.Text.RegularExpressions;

namespace LOTRO_Companion_Combiner_Lib
{
    public class LotroCompanionCombiner
    {
        // langLookup["en"]["key:620912555:228870261"] = "The More the Merrier"
        private Dictionary<string, Dictionary<string, string>> langLookup = new Dictionary<string, Dictionary<string, string>>();

        public LotroCompanionCombiner(string rootLotroDataPath)
        {
            var diRoot = new DirectoryInfo(rootLotroDataPath);

            // load the labels:
            var diLabels = new DirectoryInfo(Path.Combine(rootLotroDataPath, "labels"));
            foreach (var langDir in diLabels.EnumerateDirectories())
            {
                var lang = langDir.Name;

                LoadLanguageKeys(langDir, lang);
            }

            // now process each language:
            foreach (var kvp in langLookup)
            {
                var lang = kvp.Key;
                Console.WriteLine($"Processing {lang} files...");

                ProcessDirectory(diRoot, lang);
            }


        }

        public void ProcessDirectory(DirectoryInfo directoryToProcess, string lang)
        {
            ProcessFiles(directoryToProcess, lang);

            var directoriesToSkip = new[] { ".git", "labels" };
            foreach (var subDirectory in directoryToProcess.EnumerateDirectories())
            {
                if (directoriesToSkip.Contains(subDirectory.Name)) { continue; }
                ProcessDirectory(subDirectory, lang);
            }
        }

        public void ProcessFiles(DirectoryInfo directory, string lang)
        {
            foreach (var file in directory.EnumerateFiles())
            {
                // Skip anything that doesn't look like an xml file:
                if (!file.Name.ToLowerInvariant().EndsWith("xml")) { continue; }

                // Skip anything that alread has a language code:
                var shouldSkip = false;
                foreach (var kvp in langLookup)
                {
                    if (file.Name.ToLowerInvariant().EndsWith($"-{kvp.Key}.xml")) { shouldSkip = true; }
                }
                if (shouldSkip) { continue; }

                ProcessFile(file, lang);
            }

        }

        public void ProcessFile(FileInfo file, string lang)
        {
            // Match things that look like:
            // <trait identifier="1879049557" name="Novice" iconId="1090522632" category="29" nature="1" cosmetic="true" tooltip="key:620757613:191029568" description="key:620757613:54354734"/>
            var keyRegex = "\"(key:\\d+:\\d+)\"";
            var regex = new Regex(keyRegex);

            var outputLines = new List<string>();
            foreach (var line in File.ReadAllLines(file.FullName))
            {
                var outputLine = line;
                var matches = regex.Matches(line);

                foreach (Match match in matches)
                {
                    if (match.Groups.Count == 0) { continue; }

                    var value = match.Groups[1].Value;
                    if (langLookup[lang].ContainsKey(value))
                    {
                        outputLine = outputLine.Replace(value, langLookup[lang][value]);
                    }
                    else if (lang != "en" && langLookup["en"].ContainsKey(value))
                    {
                        outputLine = outputLine.Replace(value, langLookup["en"][value]);
                    }
                }
                outputLines.Add(outputLine);
            }
            File.WriteAllLines(file.FullName.Replace(".xml", $"-{lang}.xml"), outputLines);
        }

        public void LoadLanguageKeys(DirectoryInfo labelsLangDir, string lang)
        {
            // Match things that look like:
            //   <label key="1879048666" value="Lore of the Blade"/>
            var keyValueRegex = "<label key=\"(.*)\" value=\"(.*)\"/>";

            if (!langLookup.ContainsKey(lang))
            {
                Console.WriteLine($"Adding keys for {lang}...");
                langLookup[lang] = new Dictionary<string, string>();
            }

            var regex = new Regex(keyValueRegex);

            foreach (var keyFile in labelsLangDir.EnumerateFiles())
            {
                foreach (var line in File.ReadLines(keyFile.FullName))
                {
                    var match = regex.Match(line);
                    if (match.Groups.Count == 3)
                    {
                        var key = match.Groups[1].Value;
                        var value = match.Groups[2].Value;

                        langLookup[lang][key] = value;
                    }

                }
            }
        }


    }
}