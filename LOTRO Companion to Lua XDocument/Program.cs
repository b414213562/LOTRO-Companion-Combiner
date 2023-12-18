// Written by dt192 2023-12-18
// modified by Cube to produce identical output to the XSerializer version.

using System.Xml.Linq;

string language = "de";
string targetFile = @"C:\dev\_\lotro-data\skills\skills-de.xdocument.lua";

var skillData = XDocument.Load(@"C:\dev\_\lotro-data\skills\skills.xml");
var skillLabels = XDocument.Load($@"C:\dev\_\lotro-data\labels\{language}\skills.xml");
var effectLabels = XDocument.Load($@"C:\dev\_\lotro-data\labels\{language}\effects.xml");

var query = from skill in skillData.Root.Elements("skill")

            let cat = (string)skill.Attribute("category") ?? ""
            let id = (string)skill.Attribute("identifier") ?? ""

            where cat == "102" && id != "1879276393"

            join skillDesc in skillLabels.Root.Elements("label")
            on (string)skill.Attribute("description") equals (string)skillDesc.Attribute("key")

            join skillName in skillLabels.Root.Elements("label")
            on (string)skill.Attribute("identifier") equals (string)skillName.Attribute("key")

            join effectName in effectLabels.Root.Elements("label")
            on (string)skill.Element("effect").Attribute("id") equals (string)effectName.Attribute("key")

            select new
            {
                ID = id,
                Name = (string)skillName.Attribute("value"),
                Description = ((string)skillDesc.Attribute("value")).Replace("\n", "\\n").Replace("\\q", "\\\""),
                EffectID = (string)skill.Element("effect").Attribute("id"),
                EffectName = (string)effectName.Attribute("value")
            };

using (StreamWriter writetext = new StreamWriter(targetFile))
{
    writetext.WriteLine($"if (LANGUAGE ~= \"{language}\") then return; end");
    writetext.WriteLine($"SKILLS = {{ -- {query.Count()} skills");
    foreach (var el in query)
    {
        writetext.WriteLine(
         $@"  [{el.ID}] = {{
    [""NAME""] = ""{el.Name}""; 
    [""DESCRIPTION""] = ""{el.Description}""; 
    [""EFFECT_ID""] = ""{el.EffectID}""; 
    [""EFFECT_NAME""] = ""{el.EffectName}""; 
  }}; "
        );
    }
    writetext.WriteLine("};");
}