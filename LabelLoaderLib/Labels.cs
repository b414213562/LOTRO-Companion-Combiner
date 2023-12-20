using System.Text.RegularExpressions;

namespace LabelLoaderLib
{
    public class Labels
    {
        // langLookup["en"]["key:620912555:228870261"] = "The More the Merrier"
        private Dictionary<string, Dictionary<string, string>> langLookup = new Dictionary<string, Dictionary<string, string>>();

        public Labels(string lotroDataPath)
        {
            // load the labels:
            var diLabels = new DirectoryInfo(Path.Combine(lotroDataPath, "labels"));
            foreach (var langDir in diLabels.EnumerateDirectories())
            {
                var lang = langDir.Name;
                if (lang == "ru") continue;

                LoadLanguageKeys(langDir, lang);
            }
        }

        public List<string> GetLoadedLanguages()
        {
            var result = new List<string>();

            foreach (var kvp in langLookup)
            {
                result.Add(kvp.Key);
            }

            return result;
        }

        private void LoadLanguageKeys(DirectoryInfo labelsLangDir, string lang)
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

        public List<string> HasLabel(string label)
        {
            var result = new List<string>();
            foreach (var lang in GetLoadedLanguages())
            {
                if (HasLabel(lang, label))
                {
                    result.Add(lang);
                }
            }
            return result;
        }

        public List<string> HasKey(string key)
        {
            var result = new List<string>();
            foreach (var lang in GetLoadedLanguages())
            {
                if (HasKey(lang, key))
                {
                    result.Add(lang);
                }
            }
            return result;
        }

        public bool HasLabel(string lang, string label)
        {
            if (!langLookup.ContainsKey(lang)) { return false; }
            var result = langLookup[lang].Any(l => l.Value == label);
            return result;
        }

        public bool HasKey(string lang, string key)
        {
            var result = langLookup.ContainsKey(lang) && langLookup[lang].ContainsKey(key);
            return result;
        }

        public string GetLabel(string lang, string key)
        {
            var result = key;
            if (langLookup.ContainsKey(lang) && langLookup[lang].ContainsKey(key)) 
            { 
                result = langLookup[lang][key];
            }
            return result;
        }

        public List<Tuple<string, Dictionary<string, string>>> GetLabels(string label)
        {
            var results = new List<Tuple<string, Dictionary<string, string>>>();

            var keys = new List<string>();
            foreach (var lang in GetLoadedLanguages())
            {
                var lookups = langLookup[lang].Where(l => l.Value == label);
                foreach (var lookup in lookups)
                {
                    if (lookup.Value == label)
                    {
                        keys.Add(lookup.Key);
                    }
                }
            }

            foreach (var key in keys)
            {
                var langLabelLookup = new Dictionary<string, string>();
                foreach (var lang in GetLoadedLanguages())
                {
                    if (HasKey(lang, key))
                    {
                        langLabelLookup[lang] = GetLabel(lang, key);
                    }
                }
                var entry = new Tuple<string, Dictionary<string, string>>(key, langLabelLookup);
                results.Add(entry);
            }

            return results;
        }

    }
}