using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace LOTRO_Companion_Combiner_Lib
{
    public class LotroCompanionCombiner
    {
        private readonly LabelLoaderLib.Labels labels;
        private readonly List<string> langs;

        public LotroCompanionCombiner(string rootLotroDataPath)
        {
            var diRoot = new DirectoryInfo(rootLotroDataPath);

            labels = new LabelLoaderLib.Labels(rootLotroDataPath);
            langs = labels.GetLoadedLanguages();

            // now process each language:
            foreach (var lang in langs)
            {
                Console.WriteLine($"Processing {lang} files...");
                ProcessDirectory(diRoot, lang);
            }


        }

        public void ProcessDirectory(
            DirectoryInfo directoryToProcess,
            string lang)
        {
            ProcessFiles(directoryToProcess, lang);

            var directoriesToSkip = new[] { ".git", "labels" };
            foreach (var subDirectory in directoryToProcess.EnumerateDirectories())
            {
                if (directoriesToSkip.Contains(subDirectory.Name)) { continue; }
                ProcessDirectory(subDirectory, lang);
            }
        }

        public void ProcessFiles(
            DirectoryInfo directory,
            string lang)
        {
            foreach (var file in directory.EnumerateFiles())
            {
                // Skip anything that doesn't look like an xml file:
                if (!file.Name.ToLowerInvariant().EndsWith("xml")) { continue; }

                // Skip anything that alread has a language code:
                var shouldSkip = false;
                foreach (var supportedLang in langs)
                {
                    if (file.Name.ToLowerInvariant().EndsWith($"-{supportedLang}.xml")) { shouldSkip = true; }
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

            var skillsRegexPattern = @"<skill identifier=""(\d+)"" name=""(.*)"" category";
            var skillRegex = new Regex(skillsRegexPattern);

            var effectRegexPattern = @"<effect id=""(\d+)"" name=""(.*)""/>";
            var effectRegex = new Regex(effectRegexPattern);

            var outputLines = new List<string>();
            foreach (var line in File.ReadAllLines(file.FullName))
            {
                var outputLine = line;
                var matches = regex.Matches(line);

                foreach (Match match in matches)
                {
                    if (match.Groups.Count == 0) { continue; }

                    var value = match.Groups[1].Value;
                    if (labels.HasKey(lang, value))
                    {
                        outputLine = outputLine.Replace(value, labels.GetLabel(lang, value));
                    }
                    else if (lang != "en" && labels.HasKey("en", value))
                    {
                        outputLine = outputLine.Replace(value, labels.GetLabel("en", value));
                    }
                }

                if (file.Name == "skills.xml")
                {
                    // skill names and effects are done differently. 
                    // skill.name is in labels/[lang]/skills.xml, and
                    // effect.name is in labels/[lang]/effects.xml

                    // Example:
                    // <skill identifier="1879193216" name="Strength of the Dead" iconId="1090522259">
                    //   <effect id="1879193319" name="Strength of the Dead - Aura"/>
                    // </skill>
                    var skillMatch = skillRegex.Match(line);
                    if (skillMatch.Groups.Count == 0) { continue; }

                    var skillId = skillMatch.Groups[1].Value;
                    var skillName = skillMatch.Groups[2].Value;
                    if (labels.HasKey(lang, skillId))
                    {
                        outputLine = outputLine.Replace(skillName, labels.GetLabel(lang, skillId));
                    }


                    var effectMatch = effectRegex.Match(line);
                    if (effectMatch.Groups.Count == 0) { continue; }

                    var effectId = effectMatch.Groups[1].Value;
                    var effectName = effectMatch.Groups[2].Value;
                    if (labels.HasKey(lang, effectId))
                    {
                        outputLine = outputLine.Replace(effectName, labels.GetLabel(lang, effectId));
                    }
                }
                outputLines.Add(outputLine);

            }
            File.WriteAllLines(file.FullName.Replace(".xml", $"-{lang}.xml"), outputLines);
        }

    }
}