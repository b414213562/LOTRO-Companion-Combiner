using LOTRO_Companion_Combiner_Lib;
using NLua;
using System.Text;
using System.Xml.Serialization;

namespace LOTRO_Companion_to_Lua_Lib
{
    public class ExtractTravelSkills
    {
        public ExtractTravelSkills(string lotroDataPath, string? langVariable)
        {
            // Get list of languages
            var languages = LotroCompanionHelpers.GetLotroCompanionLanguages(lotroDataPath);
            var skillsPath = Path.Combine(lotroDataPath, "skills");

            // Open the translated files (ending in -{lang}.xml) and convert them to Lua:
            foreach (var lang in languages)
            {
                var fileName = Path.Combine(skillsPath, $"skills-{lang}.xml");
                if (File.Exists(fileName))
                {
                    var skills = ParseSkillsFile(fileName);
                    if (skills != null)
                    {
                        var outputFileName = Path.Combine(skillsPath, $"skills-{lang}.lua");
                        var skillsLua = MakeLuaFromXml(skills, lang, langVariable);
                        File.WriteAllText(outputFileName, skillsLua);
                    }
                    else
                    {
                        Console.Write($"Couldn't parse {skillsPath}");
                    }
                }
                else
                {
                    Console.Write($"Couldn't find {skillsPath}");
                }


            }
        }

        private string MakeLuaFromXml(skills skills, string lang, string? langVariable)
        {
            var eol = Environment.NewLine;

            var travelSkills = new List<skillsSkill>();
            foreach (var skill in skills.Items)
            {
                if (skill is skillsSkill s)
                {                 
                    if (s.category == "102" && s.identifier != "1879276393") // travel skill but not Smell the Roses
                    {
                        travelSkills.Add(s);
                    }

                }
            }

            var result = "";
            if (langVariable != null)
            {
                result += $"if ({langVariable} ~= \"{lang}\") then return; end" + eol;
            }
            result += $"SKILLS = {{ -- {travelSkills.Count} skills" + eol;

            foreach (var s in travelSkills)
            {
                var description = s.description.Replace("\n", " ");
                description = description.Replace("\\q", "'");

                result += $"  [{s.identifier}] = {{" + eol +
                    $"    [\"NAME\"] = \"{s.name}\"; " + eol +
                    $"    [\"DESCRIPTION\"] = \"{description}\"; " + eol +
                    $"    [\"EFFECT_ID\"] = \"{s.effect[0].id}\"; " + eol +
                    $"    [\"EFFECT_NAME\"] = \"{s.effect[0].name}\"; " + eol +
                    $"  }}; " + eol;
            }

            result += "};" + eol;

            return result;
        }

        private static skills? ParseSkillsFile(string fileName)
        {
            var serializer = new XmlSerializer(typeof(skills));
            using var stream = new FileStream(fileName, FileMode.Open);
            var skills = serializer.Deserialize(stream) as skills;
            return skills;
        }

    }
}